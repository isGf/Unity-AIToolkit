using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace HAWKAIToolkit.Core
{
    // 枚举定义
    public enum MessageRole
    {
        System,
        User,
        Assistant
    }

    [Serializable]
    public class ChatMessage
    {
        public MessageRole role;
        public string content;

        public ChatMessage(MessageRole role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    // === 专门用于网络传输的 DTO ===
    // 控制 JSON 输出的格式，而不影响业务代码使用的枚举
    [Serializable]
    public class ChatMessageDTO
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        public ChatMessageDTO(string role, string content)
        {
            Role = role; // 这里传入的是小写字符串
            Content = content;
        }
    }

    public class AIResponse
    {
        public bool isSuccess;
        public string content;
        public string rawResponse; // 用于调试或高级解析
    }

    public delegate void OnAIResponse(AIResponse response);
}