using HAWKAIToolkit.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HAWKAIToolkit.Providers.OpenAI
{
    [CreateAssetMenu(fileName = "AIProvider", menuName = "AIToolkit/AIProvider")]
    public class OpenAIProvider : AIProviderBase
    {
        protected override string SerializePayload(List<ChatMessage> messages, bool isStreaming = false)
        {
            // 将业务层的 ChatMessage 转换为传输层的 ChatMessageDTO
            var dtoMessages = new List<ChatMessageDTO>();
            foreach (var msg in messages)
            {
                dtoMessages.Add(new ChatMessageDTO(msg.role.ToString().ToLower(), msg.content));
            }

            // 构建匿名对象
            var payload = new
            {
                model = this.model,
                stream = isStreaming,
                messages = dtoMessages
            };

            // 序列化
            string json = JsonConvert.SerializeObject(payload);

            // === 调试打印 ===
            Debug.Log($"[OpenAIProvider] Sending JSON:\n{json}");

            return json;
        }

        protected override void SetHeaders(UnityWebRequest request, bool isStreaming = false)
        {
            request.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }
        }

        protected override string ParseContent(string jsonResponse)
        {
            try
            {
                // 打印原始 JSON
                Debug.Log($"[OpenAIProvider] Received JSON:\n{jsonResponse}");

                // 使用 JObject 动态解析
                JObject json = JObject.Parse(jsonResponse);

                // 检查错误
                if (json["error"] != null)
                {
                    return $"Error: {json["error"]}";
                }

                // 尝试 Ollama 的直接结构: message.content
                var content = json["message"]?["content"]?.ToString();

                // 如果 Ollama 结构为空，回退到标准 OpenAI 结构
                if (string.IsNullOrEmpty(content))
                {
                    content = json["choices"]?[0]?["message"]?["content"]?.ToString();
                }

                return content ?? "Empty Response";
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Parse Error: {e.Message}\n{jsonResponse}");
                return "Parse Error";
            }
        }

        protected override string ParseStreamContent(string jsonChunk)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<OpenAIResponse>(jsonChunk);
                return result?.choices?[0]?.delta?.content ?? "";
            }
            catch
            {
                return "";
            }
        }




        // === 定义标准的 OpenAI 响应结构 ===
        // 包含了 Ollama 可能返回的错误字段
        [System.Serializable]
        private class OpenAIResponse
        {
            public List<Choice> choices;
            public ErrorInfo error;
        }

        [System.Serializable]
        private class Choice
        {
            public Message message;
            public Delta delta;
        }

        [System.Serializable]
        private class Message
        {
            public string content;
        }

        [System.Serializable]
        private class Delta
        {
            public string content;
        }

        [System.Serializable]
        private class ErrorInfo
        {
            public string message;
            public string type;
        }
    }
}
