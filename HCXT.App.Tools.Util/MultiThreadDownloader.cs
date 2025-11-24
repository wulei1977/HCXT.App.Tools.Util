using System;
using System.IO;
using System.Net;
using System.Threading;

namespace HCXT.App.Tools.Util
{
    /// <summary>
    /// 多线程下载器 - 使用 HTTP Range 实现分片下载
    /// 优化 HTTP 服务使用，实现真正的多线程下载
    /// </summary>
    public class MultiThreadDownloader
    {
        private string _url;
        private string _outputPath;
        private int _threadCount;
        private long _fileSize;
        private Action<string> _logger;
        private readonly object _fileLock = new object();

        public MultiThreadDownloader(string url, string outputPath, int threadCount = 4, Action<string> logger = null)
        {
            _url = url;
            _outputPath = outputPath;
            _threadCount = Math.Max(1, Math.Min(threadCount, 32));
            _logger = logger ?? Console.WriteLine;
        }

        /// <summary>
        /// 获取远程文件大小
        /// </summary>
        public bool GetFileSize(out long fileSize)
        {
            fileSize = 0;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_url);
                req.Method = "HEAD";
                req.Timeout = 5000;
                
                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    if (res.Headers["Accept-Ranges"] == "bytes" || 
                        res.StatusCode == HttpStatusCode.OK || 
                        res.StatusCode == HttpStatusCode.PartialContent)
                    {
                        fileSize = res.ContentLength;
                        _fileSize = fileSize;
                        _logger?.Invoke(string.Format("文件大小：{0} 字节", fileSize));
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Invoke(string.Format("获取文件大小失败：{0}", ex.Message));
            }
            return false;
        }

        /// <summary>
        /// 开始多线程下载
        /// </summary>
        public bool StartDownload()
        {
            if (_fileSize <= 0)
            {
                _logger?.Invoke("未知文件大小");
                return false;
            }

            try
            {
                if (File.Exists(_outputPath))
                    File.Delete(_outputPath);

                using (FileStream fs = new FileStream(_outputPath, FileMode.Create, FileAccess.Write))
                {
                    fs.SetLength(_fileSize);
                }

                _logger?.Invoke(string.Format("开始多线程下载，线程数：{0}", _threadCount));

                Thread[] threads = new Thread[_threadCount];
                long chunkSize = _fileSize / _threadCount;

                for (int i = 0; i < _threadCount; i++)
                {
                    int threadIndex = i;
                    long start = threadIndex * chunkSize;
                    long end = (threadIndex == _threadCount - 1) ? 
                        _fileSize - 1 : 
                        start + chunkSize - 1;

                    threads[i] = new Thread(() => DownloadChunk(threadIndex, start, end))
                    {
                        IsBackground = true,
                        Name = string.Format("DownloadThread-{0}", threadIndex)
                    };
                    
                    threads[i].Start();
                    _logger?.Invoke(string.Format("线程 {0} 下载字节范围 {1}-{2}", threadIndex, start, end));
                }

                foreach (Thread thread in threads)
                {
                    thread.Join();
                }

                _logger?.Invoke("下载完成！");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Invoke(string.Format("下载失败：{0}", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 下载文件指定范围
        /// </summary>
        private void DownloadChunk(int threadIndex, long start, long end)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_url);
                req.Method = "GET";
                req.Timeout = 30000;
                
                if (start <= int.MaxValue && end <= int.MaxValue)
                {
                    req.AddRange((int)start, (int)end);
                }
                else
                {
                    req.Headers["Range"] = string.Format("bytes={0}-{1}", start, end);
                }

                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    if (res.StatusCode != HttpStatusCode.PartialContent && 
                        res.StatusCode != HttpStatusCode.OK)
                    {
                        _logger?.Invoke(string.Format("线程 {0} 失败：{1}", threadIndex, res.StatusCode));
                        return;
                    }

                    using (Stream stream = res.GetResponseStream())
                    {
                        byte[] buffer = new byte[8192];
                        int readBytes;
                        long totalRead = 0;
                        long currentPosition = start;

                        while ((readBytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            lock (_fileLock)
                            {
                                using (FileStream fs = new FileStream(_outputPath, FileMode.Open, FileAccess.Write, FileShare.Write))
                                {
                                    fs.Seek(currentPosition, SeekOrigin.Begin);
                                    fs.Write(buffer, 0, readBytes);
                                    fs.Flush();
                                }
                            }
                            
                            currentPosition += readBytes;
                            totalRead += readBytes;

                            if (totalRead % (1024 * 1024) == 0)
                            {
                                _logger?.Invoke(string.Format("线程 {0} 进度：{1}/{2} KB", 
                                    threadIndex, 
                                    totalRead / 1024, 
                                    (end - start + 1) / 1024));
                            }
                        }

                        _logger?.Invoke(string.Format("线程 {0} 完成：{1} 字节", threadIndex, totalRead));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Invoke(string.Format("线程 {0} 异常：{1}", threadIndex, ex.Message));
            }
        }

        /// <summary>
        /// 验证文件完整性，使用 MD5
        /// </summary>
        public string CalculateMD5()
        {
            try
            {
                using (FileStream fs = File.OpenRead(_outputPath))
                {
                    System.Security.Cryptography.MD5 md5 = 
                        System.Security.Cryptography.MD5.Create();
                    byte[] hash = md5.ComputeHash(fs);
                    
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (byte b in hash)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger?.Invoke(string.Format("计算 MD5 失败：{0}", ex.Message));
                return null;
            }
        }
    }

    /// <summary>
    /// 断点续传下载器 - 单线程但支持断点续传
    /// </summary>
    public class ResumeableDownloader
    {
        private string _url;
        private string _outputPath;
        private string _metadataPath;
        private Action<string> _logger;

        public ResumeableDownloader(string url, string outputPath, Action<string> logger = null)
        {
            _url = url;
            _outputPath = outputPath;
            _metadataPath = outputPath + ".meta";
            _logger = logger ?? Console.WriteLine;
        }

        /// <summary>
        /// 开始或继续下载
        /// </summary>
        public bool StartOrResumeDownload()
        {
            try
            {
                long downloadedBytes = 0;
                long totalBytes = 0;

                if (File.Exists(_metadataPath))
                {
                    string[] lines = File.ReadAllLines(_metadataPath);
                    if (lines.Length >= 2)
                    {
                        long.TryParse(lines[0], out downloadedBytes);
                        long.TryParse(lines[1], out totalBytes);
                        _logger?.Invoke(string.Format("继续下载：已下载 {0} 字节，总计 {1} 字节", 
                            downloadedBytes, totalBytes));
                    }
                }

                if (totalBytes == 0)
                {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(_url);
                    req.Method = "HEAD";
                    using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                    {
                        totalBytes = res.ContentLength;
                    }
                }

                HttpWebRequest downloadReq = (HttpWebRequest)WebRequest.Create(_url);
                downloadReq.Method = "GET";
                
                if (downloadedBytes <= int.MaxValue && totalBytes - 1 <= int.MaxValue)
                {
                    downloadReq.AddRange((int)downloadedBytes, (int)(totalBytes - 1));
                }
                else
                {
                    downloadReq.Headers["Range"] = string.Format("bytes={0}-{1}", downloadedBytes, totalBytes - 1);
                }

                using (HttpWebResponse res = (HttpWebResponse)downloadReq.GetResponse())
                using (Stream stream = res.GetResponseStream())
                using (FileStream fs = new FileStream(_outputPath, 
                    downloadedBytes > 0 ? FileMode.Append : FileMode.Create, 
                    FileAccess.Write))
                {
                    byte[] buffer = new byte[8192];
                    int readBytes;

                    while ((readBytes = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, readBytes);
                        downloadedBytes += readBytes;

                        File.WriteAllLines(_metadataPath, new[] 
                        { 
                            downloadedBytes.ToString(), 
                            totalBytes.ToString() 
                        });

                        if (downloadedBytes % (1024 * 1024) == 0)
                        {
                            double percent = (double)downloadedBytes / totalBytes * 100;
                            _logger?.Invoke(string.Format("下载进度：{0:F1}% ({1}/{2} MB)", 
                                percent, 
                                downloadedBytes / (1024 * 1024), 
                                totalBytes / (1024 * 1024)));
                        }
                    }
                }

                if (File.Exists(_metadataPath))
                    File.Delete(_metadataPath);

                _logger?.Invoke("下载完成！");
                return true;
            }
            catch (Exception ex)
            {
                _logger?.Invoke(string.Format("下载失败：{0}", ex.Message));
                return false;
            }
        }
    }
}
