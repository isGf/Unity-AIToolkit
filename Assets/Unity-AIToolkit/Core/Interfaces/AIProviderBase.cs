using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace HAWKAIToolkit.Core
{
    public enum ProviderPreset
    {
        OpenAI,
        Qwen,
        DeepSeek,
        ZhipuAI, // 智谱
        Moonshot, // Kimi
        Ollama
    }
    public abstract class AIProviderBase : ScriptableObject
    {


        [HideInInspector]
        public ProviderPreset preset = ProviderPreset.OpenAI;
        [Header("如选预设，baseURL 会自动更新", order = 1)]
        public string baseURL;
        [Header("模型APIKey，需在厂商官网申请", order = 1)]
        public string apiKey;
        [Header("当前厂商提供的模型，填入模型ID", order = 1)]
        public string model;

        [Header("Settings")]
        public float timeout = 30f;


        public void ApplyPresetSettings()
        {
            apiKey = ""; // 重置 API Key，用户需手动填写
            model = ""; // 重置模型名，用户需手动填写
            switch (preset)
            {
                case ProviderPreset.OpenAI:
                    baseURL = "https://api.openai.com/v1/chat/completions";
                    break;
                case ProviderPreset.DeepSeek:
                    baseURL = "https://api.deepseek.com/v1/chat/completions";
                    break;
                case ProviderPreset.ZhipuAI:
                    baseURL = "https://open.bigmodel.cn/api/paas/v4/chat/completions";
                    break;
                case ProviderPreset.Moonshot:
                    baseURL = "https://api.moonshot.cn/v1/chat/completions";
                    break;
                case ProviderPreset.Ollama:
                    baseURL = "http://localhost:11434/v1/chat/completions";
                    break;
                case ProviderPreset.Qwen:
                    baseURL = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";
                    break;
            }
        }


        // ================= 配置区域 =================

        // 原有的回调定义
        public delegate void OnAIResponse(AIResponse response);

        // 流式回调参数 (isDone 表示是否结束)
        public delegate void OnStreamChunk(string contentChunk, bool isDone);

        // ================= 公共接口 =================

        /// <summary>
        /// 发送普通（非流式）请求
        /// </summary>
        public void SendRequest(List<ChatMessage> messages, OnAIResponse callback)
        {
            CoroutineRunner.Instance.StartCoroutine(RequestCoroutine(messages, callback, null));
        }

        /// <summary>
        /// 发送流式请求
        /// </summary>
        public void SendStreamRequest(List<ChatMessage> messages, OnStreamChunk onStreamChunk, OnAIResponse onComplete = null)
        {
            CoroutineRunner.Instance.StartCoroutine(RequestCoroutine(messages, onComplete, onStreamChunk));
        }

        public void SendRawJsonRequest(string jsonPayload, OnAIResponse onCompleteCallback)
        {
            CoroutineRunner.Instance.StartCoroutine(RequestCoroutine(jsonPayload, onCompleteCallback));
        }

        public void SendRawJsonRequest(string jsonPayload, bool isStream, OnAIResponse onCompleteCallback, OnStreamChunk streamCallback = null)
        {
            // 调用内部的 RequestCoroutine 重载
            CoroutineRunner.Instance.StartCoroutine(RequestCoroutine(jsonPayload, onCompleteCallback, streamCallback));
        }

        // ================= 核心协程 =================

        // 直接接收 JSON 字符串，用于 Vision 或其他特殊请求
        protected IEnumerator RequestCoroutine(string jsonPayload, OnAIResponse onCompleteCallback, OnStreamChunk streamCallback = null)
        {
            bool isStreaming = streamCallback != null;

            using (UnityWebRequest request = new UnityWebRequest(baseURL, "POST"))
            {
                byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonPayload);

                // 复用原有的 Handler 逻辑
                if (isStreaming)
                {
                    request.downloadHandler = new StreamDownloadHandler((jsonData) =>
                    {
                        if (jsonData == null)
                        {
                            streamCallback(null, true);
                            return;
                        }
                        string content = ParseStreamContent(jsonData);
                        if (!string.IsNullOrEmpty(content))
                        {
                            streamCallback(content, false);
                        }
                    });
                }
                else
                {
                    request.downloadHandler = new DownloadHandlerBuffer();
                }

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.timeout = (int)timeout;

                // 复用 Header 设置逻辑
                SetHeaders(request, isStreaming);

                yield return request.SendWebRequest();

                // 复用原有的结果处理逻辑
                if (onCompleteCallback != null)
                {
                    AIResponse response = new AIResponse();

                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        response.isSuccess = false;
                        response.content = $"Error: {request.error}\n{request.downloadHandler.text}";
                    }
                    else
                    {
                        if (!isStreaming)
                        {
                            response.isSuccess = true;
                            // 调用子类实现的 ParseContent
                            // OpenAIProvider 解析 choices[0].message.content
                            response.content = ParseContent(request.downloadHandler.text);
                        }
                        else
                        {
                            response.isSuccess = true;
                            response.content = "Stream Finished.";
                        }
                    }
                    onCompleteCallback(response);
                }
            }
        }

        private IEnumerator RequestCoroutine(List<ChatMessage> messages, OnAIResponse onCompleteCallback, OnStreamChunk streamCallback)
        {
            bool isStreaming = streamCallback != null;
            string jsonPayload = SerializePayload(messages, isStreaming);

            using (UnityWebRequest request = new UnityWebRequest(baseURL, "POST"))
            {
                byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonPayload);

                // 如果是流式请求，使用自定义 Handler；否则使用默认 Buffer
                if (isStreaming)
                {
                    request.downloadHandler = new StreamDownloadHandler((jsonData) =>
                    {
                        if (jsonData == null)
                        {
                            streamCallback(null, true); // 结束信号
                            return;
                        }
                        string content = ParseStreamContent(jsonData);
                        if (!string.IsNullOrEmpty(content))
                        {
                            streamCallback(content, false);
                        }
                    });
                }
                else
                {
                    request.downloadHandler = new DownloadHandlerBuffer();
                }

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.timeout = (int)timeout;

                SetHeaders(request, isStreaming);

                yield return request.SendWebRequest();

                // 处理最终完成回调（主要用于非流式，或流式请求的错误处理）
                if (onCompleteCallback != null)
                {
                    AIResponse response = new AIResponse();

                    if (request.result == UnityWebRequest.Result.ConnectionError ||
                        request.result == UnityWebRequest.Result.ProtocolError)
                    {
                        response.isSuccess = false;
                        response.content = $"Error: {request.error}\n{request.downloadHandler.text}";
                    }
                    else
                    {
                        // 非流式请求在这里解析内容
                        if (!isStreaming)
                        {
                            response.isSuccess = true;
                            response.content = ParseContent(request.downloadHandler.text);//解析非流式响应的完整内容
                        }
                        else
                        {
                            // 流式请求成功
                            response.isSuccess = true;
                            response.content = "Stream Finished.";
                        }
                    }
                    onCompleteCallback(response);
                }
            }
        }

        // ================= 抽象方法 (需子类实现) =================

        /// <summary>
        /// 序列化请求体
        /// </summary>
        protected abstract string SerializePayload(List<ChatMessage> messages, bool isStreaming = false);

        /// <summary>
        /// 设置请求头
        /// </summary>
        protected abstract void SetHeaders(UnityWebRequest request, bool isStreaming = false);

        /// <summary>
        /// 解析非流式响应的完整内容
        /// </summary>
        protected abstract string ParseContent(string jsonResponse);

        /// <summary>
        /// 解析流式响应的单个数据块内容 (提取 choices[0].delta.content)
        /// </summary>
        protected abstract string ParseStreamContent(string jsonChunk);


    }
}
