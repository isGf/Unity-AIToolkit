using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using HAWKAIToolkit.Core;

namespace HAWKAIToolkit.Core
{
    /// <summary>
    /// 图像识别的辅助方法，此类包含了流式以及非流式的封装
    /// </summary>
    public static class AIVisionHelper
    {
        /// <summary>
        /// 发送请求并将结果直接赋值给 UI.Text，失败则显示错误信息
        /// </summary>
        /// <param name="isStream">是否为流式请求</param>

        public static void AIVisionToUIText(this AIProviderBase provider,
                                         Texture2D image,
                                         string prompt,
                                         Text targetText,
                                         bool isStream = false)
        {
            if (targetText == null) return;

            if (!isStream) targetText.text = "AI 正在看图...";
            else targetText.text = "";

            DoVisionRequest(provider, image, prompt, isStream,
                (chunk, isDone) => //注意这里现在有两个参数了，isDone 我们用不到，用下划线 _ 忽略也行
                {
                    if (chunk != null)
                    {
                        targetText.text += chunk;
                    }
                },
                (response) =>
                {
                    if (isStream)
                    {
                        if (!response.isSuccess) targetText.text = $"Error: {response.content}";
                    }
                    else
                    {
                        if (response.isSuccess)
                        {
                            targetText.text = response.content;
                        }
                        else
                        {
                            targetText.text = $"Error: {response.content}";
                        }
                    }
                });
        }
        /// <summary>
        /// 自定义回调
        /// </summary>
        /// <param name="isStream">是否为流式请求</param>
        public static void AIVisionToAction(this AIProviderBase provider,
                                         Texture2D image,
                                         string prompt,
                                         Action<string, bool> onChunk,     // 流式回调
                                         Action<AIResponse> onComplete, // 结束回调
                                         bool isStream = false)
        {
            DoVisionRequest(provider, image, prompt, isStream, onChunk, onComplete);
        }


        private static void DoVisionRequest(AIProviderBase provider, Texture2D image, string prompt, bool isStream, Action<string, bool> onChunk, Action<AIResponse> onComplete)
        {
            CoroutineRunner.Instance.StartCoroutine(DoVisionRequestCoroutine(provider, image, prompt, isStream, onChunk, onComplete));
        }

        private static IEnumerator DoVisionRequestCoroutine(AIProviderBase provider, Texture2D image, string prompt, bool isStream, Action<string, bool> onChunk, Action<AIResponse> onComplete)
        {
            // 转 Base64
            string base64Image = Convert.ToBase64String(image.EncodeToPNG());

            // 获取 Model
            string modelName = provider.GetType().GetField("model")?.GetValue(provider) as string ?? "";

            // 构建 JSON（根据 isStream 决定是否包含 "stream": true）
            string streamParam = isStream ? "\"stream\": true," : "\"stream\": false,";

            string jsonBody = $"{{" +
                $"\"model\": \"{modelName}\"," +
                $"{streamParam}" + // 动态插入流式参数
                $"\"messages\": [{{" +
                    $"\"role\": \"user\"," +
                    $"\"content\": [" +
                        $"{{\"type\": \"text\", \"text\": \"{prompt}\"}}," +
                        $"{{\"type\": \"image_url\", \"image_url\": {{\"url\": \"data:image/png;base64,{base64Image}\"}}}}" +
                    $"]" +
                $"}}]" +
            $"}}";

            Debug.Log(jsonBody);

            // 发送请求
            bool isDone = false;
            AIResponse response = null;

            // 直接在参数位置判断
            // 如果是流式 (isStream=true)，传入 onChunk
            // 如果是非流式 (isStream=false)，传入 null
            provider.SendRawJsonRequest(jsonBody, isStream,
                (res) =>
                {
                    response = res;
                    isDone = true;
                    onComplete?.Invoke(response);
                },
                isStream ? new AIProviderBase.OnStreamChunk(onChunk) : (AIProviderBase.OnStreamChunk)null
            );

            while (!isDone)
            {
                yield return null;
            }
        }
    }
}
