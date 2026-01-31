using UnityEngine;
using UnityEditor;
using HAWKAIToolkit.Core;

namespace HAWKAIToolkit.Editor
{
    public class AIToolkitWindow : EditorWindow
    {
        [MenuItem("Tools/AI Toolkit Configuration")]
        public static void ShowWindow()
        {
            GetWindow<AIToolkitWindow>("AI Toolkit");
        }

        void OnGUI()
        {
            GUILayout.Label("AI Toolkit Settings", EditorStyles.boldLabel);
            GUILayout.Space(10);

            GUILayout.Label("1. Create Provider Configs in Project View");
            GUILayout.Label("   Right Click -> Create -> AIToolkit -> Providers");

            GUILayout.Space(20);

            if (GUILayout.Button("Open Documentation"))
            {
                Application.OpenURL("https://your-docs-url.com");
            }
        }
    }
}
