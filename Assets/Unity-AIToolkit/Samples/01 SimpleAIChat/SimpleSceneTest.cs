using HAWKAIToolkit.Core;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSceneTest : MonoBehaviour
{
    public AIProviderBase provider;
    public Text AIResponse;
    List<ChatMessage> messages;

    void Start()
    {
        messages = new List<ChatMessage>
        {
            new ChatMessage(MessageRole.User, "关于Unity开发的100字介绍。")
        };
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) // 输出到 UI.Text，非流式
        {
            provider.AISimpleToUIText(messages, AIResponse, false);
        }
        if (Input.GetKeyDown(KeyCode.W)) // 输出到 UI.Text，流式
        {
            provider.AISimpleToUIText(messages, AIResponse, true);
        }

        if (Input.GetKeyDown(KeyCode.E)) // 自定义输出，非流式
        {
            provider.AISimpleToAction(messages,
                (chunk) =>
                {
                    // 非流式模式下，这个回调不会被调用
                },
                (res) =>
                {
                    Debug.Log("非流式分析结果: " + res.content);
                },
                false);
        }
        if (Input.GetKeyDown(KeyCode.R)) // 自定义输出，流式
        {
            provider.AISimpleToAction(messages,
                (chunk) =>
                {
                    Debug.Log("流式 chunk" + chunk);
                },
                (res) =>
                {
                    Debug.Log("流式分析完成: " + res.content);
                },
                true);
        }
    }
}
