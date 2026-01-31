using HAWKAIToolkit.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HAWKAIToolkit.Core
{
    /// <summary>
    /// AI 请求辅助类：统一处理流式和非流式请求
    /// </summary>
    public static class AISimpleHelper
    {
        #region 统一请求方法

        /// <summary>
        /// 用法：provider.ToUIText(messages, myUIText, isStream);
        /// 发送请求并将结果直接赋值给 UI.Text，失败则显示错误信息
        /// </summary>
        /// <param name="isStream">是否为流式请求</param>
        public static void AISimpleToUIText(this AIProviderBase provider,
                                   List<ChatMessage> messages,
                                   Text targetText,
                                   bool isStream = true) // 默认为流式
        {
            if (targetText == null)
            {
                Debug.LogError("AIHelper: Target UI Text is null!");
                return;
            }

            if (isStream)
            {
                // 流式请求：清空文本，准备打字机效果
                targetText.text = "";
                CoroutineRunner.Instance.StartCoroutine(DoStreamRequest(provider, messages, (chunk) =>
                {
                    targetText.text += chunk;
                }, null)); // onComplete 可选，这里传 null
            }
            else
            {
                // 非流式请求：显示 "AI 正在思考..."
                targetText.text = "AI 正在思考...";
                CoroutineRunner.Instance.StartCoroutine(DoRequest(provider, messages, (response) =>
                {
                    if (response.isSuccess)
                    {
                        targetText.text = response.content;
                    }
                    else
                    {
                        targetText.text = $"Error: {response.content}";
                    }
                }));
            }
        }


        /// <summary>
        /// 用法：provider.ToAction(messages, (res) => { ... }, isStream);
        /// 自定义回调
        /// </summary>
        /// <param name="isStream">是否为流式请求</param>
        public static void AISimpleToAction(this AIProviderBase provider,
                                   List<ChatMessage> messages,
                                   Action<string> onChunk, // 处理每个 chunk
                                   Action<AIResponse> onComplete, // 处理最终结果
                                   bool isStream = true) // 默认为流式
        {
            if (isStream)
            {
                // 流式请求：调用 onComplete 回调
                CoroutineRunner.Instance.StartCoroutine(DoStreamRequest(provider, messages, onChunk, onComplete));
            }
            else
            {
                // 非流式请求：调用 onComplete 回调
                CoroutineRunner.Instance.StartCoroutine(DoRequest(provider, messages, onComplete));
            }
        }

        #endregion

        #region 内部协程处理（保持不变）

        // 非流式请求的协程
        private static IEnumerator DoRequest(AIProviderBase provider, List<ChatMessage> messages, Action<AIResponse> callback)
        {
            bool isDone = false;
            AIResponse response = null;

            provider.SendRequest(messages, (res) =>
            {
                response = res;
                isDone = true;
            });

            while (!isDone)
            {
                yield return null;
            }

            callback?.Invoke(response);
        }

        // 流式请求的协程
        private static IEnumerator DoStreamRequest(AIProviderBase provider,
                                                  List<ChatMessage> messages,
                                                  Action<string> onChunk,
                                                  Action<AIResponse> onComplete)
        {
            bool isRequestActive = true;

            provider.SendStreamRequest(messages,
                // Stream Callback (每收到一个字)
                (chunk, isDone) =>
                {
                    if (isDone)
                    {
                        isRequestActive = false;
                    }
                    else
                    {
                        onChunk?.Invoke(chunk);
                    }
                },
                // Complete Callback (整个请求结束)
                (response) =>
                {
                    isRequestActive = false;
                    onComplete?.Invoke(response);
                });

            // 等待请求完成
            while (isRequestActive)
            {
                yield return null;
            }
        }

        #endregion
    }
}
