using HAWKAIToolkit.Core;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class ImageChatExample : MonoBehaviour
{
    public AIProviderBase provider; // 拖入你的 OpenAIProvider 配置文件
    public RawImage sourceImage;        // 拖入显示图片的 RawImage
    public InputField promptInput;       // 输入问题
    public Text resultText;             // 显示结果
    public Button sendButton;           // 发送按钮
    public ToggleGroup modeToggleGroup; // 模式选择按钮组

    private Texture2D tex; // 需要看的图片

    void Start()
    {
        tex = sourceImage.texture as Texture2D;

        sendButton.onClick.AddListener(OnSend);
    }

    void OnSend()
    {
        var selectedToggle = modeToggleGroup.ActiveToggles();
        foreach (var item in selectedToggle)
        {
            switch (item.name)
            {
                case "TrueTog":
                    // 流式调用（打字机效果）
                    provider.AIVisionToUIText(tex, promptInput.text, resultText, true);
                    break;
                case "FalseTog":
                    // 非流式调用（等全部生成完显示）
                    provider.AIVisionToUIText(tex, promptInput.text, resultText, false);
                    break;
            }
        }
    }

    private void Update()
    {
        // 非流式
        if (Input.GetKeyDown(KeyCode.Q))
        {
            provider.AIVisionToAction(
                tex,
                "这是什么？",
                (chunk, isDone) => // 流式回调，因为是非流式所以忽略
                {
                    // 非流式模式下，这个回调不会被调用
                },
                (response) => // 完成回调
                {
                    if (response.isSuccess)
                    {
                        Debug.Log("非流式分析结果: " + response.content);
                    }
                    else
                    {
                        Debug.LogError("非流式分析失败: " + response.content);
                    }
                },
                false // 非流式
            );
        }

        // 流式
        if (Input.GetKeyDown(KeyCode.W))
        {
            provider.AIVisionToAction(
                tex,
                "这是什么？",
                (chunk, isDone) => // 流式回调
                {
                    if (!isDone) // 如果不是结束
                    {
                        Debug.Log("流式 chunk: " + chunk);
                    }
                },
                (response) => // 完成回调
                {
                    if (!response.isSuccess)
                    {
                        Debug.LogError("流式分析失败: " + response.content);
                    }
                },
                true // 流式
            );
        }
    }

}
