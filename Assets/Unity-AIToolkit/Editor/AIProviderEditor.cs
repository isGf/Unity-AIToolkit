using HAWKAIToolkit.Core;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AIProviderBase), true)]
public class AIProviderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 获取当前编辑的对象
        AIProviderBase provider = (AIProviderBase)target;

        // 【优化点】把下拉框和按钮放在同一行
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("选择厂商预设", EditorStyles.boldLabel);
        ProviderPreset newPreset;
        EditorGUI.BeginChangeCheck();
        // 使用 HorizontalScope 让下拉框和按钮在同一行
        using (new EditorGUILayout.HorizontalScope())
        {
            newPreset = (ProviderPreset)EditorGUILayout.EnumPopup(provider.preset);

            // 按钮的宽度固定，看起来更整齐
            if (GUILayout.Button("申请 API Key", GUILayout.Width(120)))
            {
                switch (provider.preset)
                {
                    case ProviderPreset.OpenAI:
                        Application.OpenURL("https://platform.openai.com/api-keys");
                        break;
                    case ProviderPreset.DeepSeek:
                        Application.OpenURL("https://platform.deepseek.com/api_keys");
                        break;
                    case ProviderPreset.ZhipuAI:
                        Application.OpenURL("https://open.bigmodel.cn/usercenter/proj-mgmt/apikeys");
                        break;
                    case ProviderPreset.Moonshot:
                        Application.OpenURL("https://platform.moonshot.cn/console/api-keys");
                        break;
                    case ProviderPreset.Qwen:
                        Application.OpenURL("https://bailian.console.aliyun.com/cn-beijing/?tab=model#/api-key");
                        break;
                    case ProviderPreset.Ollama:
                        Application.OpenURL("https://ollama.com/download");
                        break;
                    default:
                        Debug.LogWarning("请先选择一个厂商预设，再点击申请 API Key。");
                        break;
                }
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(provider, "Change Provider Preset");
            provider.preset = newPreset;
            provider.ApplyPresetSettings();
            EditorUtility.SetDirty(provider);
        }



        // 【修改点2】把默认的 Inspector 放在下面
        DrawDefaultInspector();

    }
}
