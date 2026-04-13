using StudioPeipeiko.VRClarity.Runtime;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace StudioPeipeiko.VRClarity.Editor
{
    [CustomEditor(typeof(VRClarityNoticePanel))]
    public class VRClarityNoticePanelEditor : UnityEditor.Editor
    {
        private SerializedProperty _font;

        private void OnEnable()
        {
            _font = serializedObject.FindProperty("_font");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("VRClarity Notice Panel", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.PropertyField(_font, new GUIContent("Font", "パネル全体に適用するフォント"));

            EditorGUILayout.Space(8);

            using (new EditorGUI.DisabledScope(_font.objectReferenceValue == null))
            {
                if (GUILayout.Button("パネルのフォントを一括適用"))
                    ApplyFont();
            }

            if (_font.objectReferenceValue == null)
                EditorGUILayout.HelpBox("フォントを設定してください。", MessageType.Info);

            serializedObject.ApplyModifiedProperties();
        }

        private void ApplyFont()
        {
            var panel    = (VRClarityNoticePanel)target;
            var font     = (TMP_FontAsset)_font.objectReferenceValue;
            var tmps     = panel.GetComponentsInChildren<TextMeshProUGUI>(true);

            Undo.RecordObjects(System.Array.ConvertAll(tmps, t => (Object)t),
                "Apply Notice Panel Font");

            foreach (var tmp in tmps)
                tmp.font = font;

            Debug.Log($"[VRClarity] フォントを '{font.name}' に変更しました（{tmps.Length} 個）");
        }
    }
}
