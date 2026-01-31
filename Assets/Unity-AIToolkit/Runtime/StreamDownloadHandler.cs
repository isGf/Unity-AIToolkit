using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HAWKAIToolkit.Core
{
    /// <summary>
    /// 专用于 SSE (Server-Sent Events) 的下载处理器
    /// </summary>
    public class StreamDownloadHandler : DownloadHandlerScript
    {
        private readonly Action<string> _onDataReceived;
        private string _buffer = string.Empty;

        public StreamDownloadHandler(Action<string> onDataReceived) : base()
        {
            _onDataReceived = onDataReceived;
        }

        // 每当接收到数据包时调用
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || data.Length == 0) return true;

            string textChunk = Encoding.UTF8.GetString(data, 0, dataLength);
            _buffer += textChunk;

            ProcessBuffer();
            return true;
        }

        private void ProcessBuffer()
        {
            // SSE 格式通常以双换行符 \n\n 分隔不同的数据块
            int splitIndex;
            while ((splitIndex = _buffer.IndexOf("\n\n")) != -1)
            {
                string packet = _buffer.Substring(0, splitIndex).Trim();
                _buffer = _buffer.Substring(splitIndex + 2);

                if (string.IsNullOrEmpty(packet)) continue;

                // 移除 "data: " 前缀
                if (packet.StartsWith("data: "))
                {
                    string jsonPayload = packet.Substring(6);

                    // 检查是否为结束标志 [DONE]
                    if (jsonPayload.Trim() == "[DONE]")
                    {
                        _onDataReceived?.Invoke(null); // 发送 null 表示流结束
                        return;
                    }

                    _onDataReceived?.Invoke(jsonPayload);
                }
            }
        }
    }
}
