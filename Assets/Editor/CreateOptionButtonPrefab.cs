using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Sebbe.EditorTools
{
    public static class OptionButtonPrefabCreator
    {
        [MenuItem("Tools/Create Option Button Prefab")]
        public static void CreatePrefab()
        {
            string prefabDir = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            // Root button object
            var go = new GameObject("OptionButton");
            var rt = go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            var btn = go.AddComponent<Button>();
            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 34f;
            layout.preferredWidth = 200f;

            // Label
            var label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(go.transform, false);
            var txt = label.AddComponent<TextMeshProUGUI>();
            txt.text = "Option";
            txt.fontSize = 24;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color = Color.white;
            var lrt = label.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(6, 4);
            lrt.offsetMax = new Vector2(-6, -4);

            // Save as prefab
            string prefabPath = "Assets/Prefabs/OptionButton.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.UserAction);
            Object.DestroyImmediate(go);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Prefab Created", $"Created prefab at {prefabPath}", "OK");

            // Try to assign to DialogueManager in the open scene
            var dm = Object.FindObjectOfType<Sebbe.DialogueManager>();
            if (dm != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    var btnComp = prefab.GetComponent<Button>();
                    if (btnComp != null)
                    {
                        var so = new SerializedObject(dm);
                        var prop = so.FindProperty("optionButtonPrefab");
                        if (prop != null)
                        {
                            prop.objectReferenceValue = btnComp;
                            so.ApplyModifiedProperties();
                            EditorUtility.DisplayDialog("Assigned", "Assigned OptionButton prefab to DialogueManager in scene.", "OK");
                            return;
                        }
                    }
                }
                EditorUtility.DisplayDialog("Notice", "Could not assign prefab automatically — assign `Assets/Prefabs/OptionButton.prefab` to your DialogueManager manually.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Note", "No DialogueManager instance found in the open scenes. Assign the prefab manually in the inspector.", "OK");
            }
        }
    }
}
