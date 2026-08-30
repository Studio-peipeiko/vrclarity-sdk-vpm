using StudioPeipeiko.VRClarity.Runtime;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace StudioPeipeiko.VRClarity.Editor
{
    [CustomEditor(typeof(VRClarityTracker))]
    public class VRClarityTrackerEditor : UnityEditor.Editor
    {
        private const string PrefabPath = "Packages/net.vrclarity.sdk/Runtime/VRClarity Tracker.prefab";

        [MenuItem("GameObject/VRClarity/Create Tracker", false, 10)]
        private static void CreateVRClarityTracker(MenuCommand menuCommand)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[VRClarity] Tracker prefab not found at: " + PrefabPath);
                return;
            }

            var parent = menuCommand.context as GameObject;

            // Tracker
            GameObject tracker = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            GameObjectUtility.SetParentAndAlign(tracker, parent);
            Undo.RegisterCreatedObjectUndo(tracker, "Create VRClarity Tracker");

            // Notice Panel
            var panel = VRClarityNoticePanelCreator.CreatePanel(parent);
            Undo.RegisterCreatedObjectUndo(panel, "Create VRClarity Notice Panel");

            Selection.activeGameObject = tracker;
        }
        private SerializedProperty _keyId;
        private SerializedProperty _encryptionKey;
        private SerializedProperty _enableTracking;
        private SerializedProperty _stayUrls;
        private SerializedProperty _moveUrls;
        private SerializedProperty _joinUrls;
        private SerializedProperty _platformUrls;
        private SerializedProperty _pcUrls;
        private SerializedProperty _verifyUrl;

        // Must match the totalUrls computation in VRClarityUrlBaker (9 + 6 + 100 + 5 + 81 + 1).
        private const int TOTAL_URL_COUNT = 202;

        private bool _showUrlStatus = false;
        private GUIStyle _errorStyle;

        private void OnEnable()
        {
            _keyId = serializedObject.FindProperty("keyId");
            _encryptionKey = serializedObject.FindProperty("encryptionKey");
            _enableTracking = serializedObject.FindProperty("enableTracking");
            _stayUrls = serializedObject.FindProperty("_stayUrls");
            _moveUrls = serializedObject.FindProperty("_moveUrls");
            _joinUrls = serializedObject.FindProperty("_joinUrls");
            _platformUrls = serializedObject.FindProperty("_platformUrls");
            _pcUrls = serializedObject.FindProperty("_pcUrls");
            _verifyUrl = serializedObject.FindProperty("_verifyUrl");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var tracker = (VRClarityTracker)target;

            // Header
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("VRClarity Tracker", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "VRClarityダッシュボードで発行したAPI KeyとEncryption Keyを入力してください。\n" +
                "URLはビルド時に自動生成されます。World IDはVRC_SceneDescriptorのBlueprint IDから自動取得されます。",
                MessageType.Info);
            EditorGUILayout.Space(4);

            // Settings
            EditorGUILayout.LabelField("API Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_keyId, new GUIContent("Key ID", "sk_ + 24 hex characters"));
            EditorGUILayout.PropertyField(_encryptionKey, new GUIContent("Encryption Key", "64 hex characters (256-bit AES key)"));

            EditorGUILayout.Space(8);

            // Production tracking gate
            EditorGUILayout.LabelField("Tracking", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_enableTracking, new GUIContent("Enable Tracking", "アップロードする公開ワールドでのみチェックを入れてください。"));
            if (_enableTracking.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "本番送信が有効です。アップロード後、実プレイのハートビートが送信されます。",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "ローカルデバッグ保護中：ハートビートは送信されません（設定検証リクエストのみ）。\n" +
                    "本番アップロードの直前にチェックを入れてください。チェックを忘れるとデータは送信されません。",
                    MessageType.Warning);
            }

            EditorGUILayout.Space(8);

            // Validation
            DrawValidationStatus(tracker);

            EditorGUILayout.Space(4);

            // URL Pool Status
            _showUrlStatus = EditorGUILayout.Foldout(_showUrlStatus, "URL Pool Status");
            if (_showUrlStatus)
            {
                EditorGUI.indentLevel++;
                DrawUrlPoolStatus("Stay URLs", _stayUrls, 9);
                DrawUrlPoolStatus("Move URLs", _moveUrls, 6);
                DrawUrlPoolStatus("Join URLs (platform x visit)", _joinUrls, 100);
                DrawUrlPoolStatus("Platform URLs", _platformUrls, 5);
                DrawUrlPoolStatus("PC URLs", _pcUrls, 81);
                int verifyCount = VerifyUrlCount(_verifyUrl);
                EditorGUILayout.LabelField("Verify URL (config check)",
                    verifyCount == 1 ? "1 / 1" : "0 / 1 (needs bake)");

                int total = SafeArraySize(_stayUrls) + SafeArraySize(_moveUrls) +
                            SafeArraySize(_joinUrls) + SafeArraySize(_platformUrls) + SafeArraySize(_pcUrls) +
                            verifyCount;
                EditorGUILayout.LabelField($"Total: {total} / {TOTAL_URL_COUNT} URLs baked");
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawValidationStatus(VRClarityTracker tracker)
        {
            bool keyIdValid = VRClarityEncryption.IsValidKeyId(tracker.keyId);
            bool encKeyValid = VRClarityEncryption.IsValidEncryptionKey(tracker.encryptionKey);

            DrawStatusLine("Key ID", keyIdValid, string.IsNullOrEmpty(tracker.keyId) ? "Not set" : "Invalid format (expected sk_ + 24 hex)");
            DrawStatusLine("Encryption Key", encKeyValid, string.IsNullOrEmpty(tracker.encryptionKey) ? "Not set" : "Invalid format (expected 64 hex characters)");
        }

        private void DrawStatusLine(string label, bool isValid, string errorMessage)
        {
            EditorGUILayout.BeginHorizontal();
            if (isValid)
            {
                EditorGUILayout.LabelField($"  {label}: OK", EditorStyles.miniLabel);
            }
            else
            {
                if (_errorStyle == null)
                {
                    _errorStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.red } };
                }
                EditorGUILayout.LabelField($"  {label}: {errorMessage}", _errorStyle);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawUrlPoolStatus(string label, SerializedProperty prop, int expected)
        {
            int count = SafeArraySize(prop);
            string status = count == expected ? $"{count} / {expected}" : $"{count} / {expected} (needs bake)";
            EditorGUILayout.LabelField(label, status);
        }

        private int SafeArraySize(SerializedProperty prop)
        {
            return prop != null && prop.isArray ? prop.arraySize : 0;
        }

        // VRCUrl is a single serialized struct, not an array: count it as baked when its "url" is non-empty.
        private int VerifyUrlCount(SerializedProperty prop)
        {
            var urlField = prop?.FindPropertyRelative("url");
            return urlField != null && !string.IsNullOrEmpty(urlField.stringValue) ? 1 : 0;
        }

    }
}
