using System;
using System.IO;
using System.Net;
using System.Text;

namespace HCXT.App.Tools.Util
{
    /// <summary>
    /// HTTP 服务优化模块 - 支持断点续传和多线程下载
    /// </summary>
    public class HttpServerOptimization
    {
        /// <summary>
        /// 支持 HTTP Range 请求的文件传输，用于断点续传和多线程下载
        /// </summary>
        public static void SendFileWithRangeSupport(HttpListenerResponse response, HttpListenerRequest request, string filePath, Action<string> logger = null)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;
            const int bufferSize = 65536; // 64KB 缓冲区
            
            response.ContentType = GetContentType(filePath);
            response.AddHeader("Accept-Ranges", "bytes");
            response.AddHeader("Last-Modified", fileInfo.LastWriteTime.ToString("R"));

            string rangeHeader = request.Headers["Range"];
            
            if (string.IsNullOrEmpty(rangeHeader))
            {
                // 不支持 Range，返回整个文件
                response.StatusCode = 200;
                response.ContentLength64 = fileSize;
                SendFileStream(response, filePath, 0, fileSize, bufferSize);
                logger?.Invoke(string.Format("{0} {1} - 200 ({2} bytes)", request.HttpMethod, request.Url.AbsolutePath, fileSize));
            }
            else
            {
                // 解析 Range 头并处理分片请求
                if (ParseRangeHeader(rangeHeader, fileSize, out long rangeStart, out long rangeEnd))
                {
                    response.StatusCode = 206;
                    response.ContentLength64 = rangeEnd - rangeStart + 1;
                    response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", rangeStart, rangeEnd, fileSize));
                    SendFileStream(response, filePath, rangeStart, rangeEnd - rangeStart + 1, bufferSize);
                    logger?.Invoke(string.Format("{0} {1} - 206 (Partial Content: {2}-{3})", request.HttpMethod, request.Url.AbsolutePath, rangeStart, rangeEnd));
                }
                else
                {
                    // Range 无效，返回 416 错误
                    response.StatusCode = 416;
                    response.AddHeader("Content-Range", string.Format("bytes */{0}", fileSize));
                    logger?.Invoke(string.Format("{0} {1} - 416 (Range Not Satisfiable)", request.HttpMethod, request.Url.AbsolutePath));
                }
            }
        }

        /// <summary>
        /// 使用流式传输发送文件，避免全量加载到内存
        /// 真正执行支持大文件和高并发的传输
        /// </summary>
        public static void SendFileStream(HttpListenerResponse response, string filePath, long offset, long length, int bufferSize)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
                if (offset > 0)
                {
                    fs.Seek(offset, SeekOrigin.Begin);
                }

                byte[] buffer = new byte[bufferSize];
                long remainingBytes = length;
                int readBytes;

                while (remainingBytes > 0)
                {
                    int bytesToRead = (int)Math.Min(bufferSize, remainingBytes);
                    readBytes = fs.Read(buffer, 0, bytesToRead);
                    
                    if (readBytes == 0)
                        break;

                    try
                    {
                        response.OutputStream.Write(buffer, 0, readBytes);
                        response.OutputStream.Flush();
                    }
                    catch (HttpListenerException)
                    {
                        break;
                    }
                    remainingBytes -= readBytes;
                }
            }
            catch (Exception)
            {
                // 客户端断开连接或其他IO错误，静默处理
            }
            finally
            {
                if (fs != null)
                    fs.Dispose();
            }
        }

        /// <summary>
        /// 解析 HTTP Range 头，支持多种格式：
        /// - bytes=0-100 (从0到100)
        /// - bytes=100- (从100到末尾)
        /// - bytes=-100 (最后100字节)
        /// </summary>
        public static bool ParseRangeHeader(string rangeHeader, long fileSize, out long rangeStart, out long rangeEnd)
        {
            rangeStart = 0;
            rangeEnd = fileSize - 1;

            try
            {
                if (!rangeHeader.StartsWith("bytes="))
                    return false;

                string range = rangeHeader.Substring(6);
                
                if (range.Contains("-"))
                {
                    string[] parts = range.Split('-');
                    
                    if (parts.Length == 2)
                    {
                        if (string.IsNullOrEmpty(parts[0]))
                        {
                            // bytes=-500 (最后500字节)
                            if (long.TryParse(parts[1], out long suffixLength) && suffixLength > 0)
                            {
                                rangeStart = Math.Max(0, fileSize - suffixLength);
                            }
                            else
                                return false;
                        }
                        else if (string.IsNullOrEmpty(parts[1]))
                        {
                            // bytes=100- (从100到末尾)
                            if (long.TryParse(parts[0], out rangeStart) && rangeStart < fileSize)
                            {
                                rangeEnd = fileSize - 1;
                            }
                            else
                                return false;
                        }
                        else
                        {
                            // bytes=100-200
                            if (long.TryParse(parts[0], out rangeStart) && long.TryParse(parts[1], out rangeEnd))
                            {
                                if (rangeStart > rangeEnd || rangeStart >= fileSize)
                                    return false;
                                rangeEnd = Math.Min(rangeEnd, fileSize - 1);
                            }
                            else
                                return false;
                        }
                        
                        return rangeStart >= 0 && rangeEnd >= rangeStart && rangeEnd < fileSize;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// 获取文件的 MIME 类型
        /// </summary>
        public static string GetContentType(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLower();
            switch (ext)
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".json":
                    return "application/json";
                case ".xml":
                    return "text/xml";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".ico":
                    return "image/x-icon";
                case ".txt":
                    return "text/plain";
                case ".pdf":
                    return "application/pdf";
                case ".zip":
                    return "application/zip";
                case ".mp4":
                case ".mpeg":
                    return "video/mp4";
                case ".mp3":
                    return "audio/mpeg";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
