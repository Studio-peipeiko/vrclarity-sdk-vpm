using StudioPeipeiko.VRClarity.Runtime;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace StudioPeipeiko.VRClarity.Editor
{
    public static class VRClarityNoticePanelCreator
    {
        private const string LogoPath      = "Packages/net.vrclarity.sdk/Runtime/Assets/vrclarity_logo.png";
        private const string FontAssetPath = "Packages/net.vrclarity.sdk/Runtime/Assets/Fonts/NotoSansJP-Regular SDF.asset";

        // Canvas size: 800x500 canvas units = 0.8m x 0.5m in world space
        private const float PanelWidth  = 570f;
        private const float PanelHeight = 350f;
        private const float WorldScale  = 0.001f;

        // Colors matched to VRClarity dashboard
        private static readonly Color BackgroundColor = new Color(0.044f, 0.102f, 0.182f, 0.97f); // rgb(20, 33, 51)
        private static readonly Color AccentColor     = new Color(0.000f, 0.788f, 0.655f, 1.00f); // #00C9A7
        private static readonly Color BodyColor       = new Color(0.929f, 0.945f, 0.961f, 1.00f); // #EDF1F5
        private static readonly Color SubtleColor     = new Color(0.424f, 0.471f, 0.541f, 1.00f); // #6C788A

        // ── Menu items ───────────────────────────────────────────────────────

        [MenuItem("GameObject/VRClarity/Create Notice Panel", false, 11)]
        private static void CreateNoticePanelMenu(MenuCommand menuCommand)
        {
            var panel = CreatePanel(menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(panel, "Create VRClarity Notice Panel");
            Selection.activeGameObject = panel;
        }


        // ── Panel creation ───────────────────────────────────────────────────

        /// <summary>パネルを生成して返します。Undo登録・Selection変更は呼び出し元の責務です。</summary>
        public static GameObject CreatePanel(GameObject parent = null)
        {
            var font = GetOrCreateFontAsset();

            var root   = new GameObject("VRClarity Notice Panel");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.sizeDelta   = new Vector2(PanelWidth, PanelHeight);
            rootRt.localScale  = Vector3.one * WorldScale;

            // Inspector からフォントを差し替えられるコンポーネントを追加
            var panelComp = root.AddComponent<VRClarityNoticePanel>();
            var so = new SerializedObject(panelComp);
            so.FindProperty("_font").objectReferenceValue = font;
            so.ApplyModifiedProperties();

            CreateBackground(root);
            CreateTopBorder(root);
            CreateHeader(root, font);
            CreateDivider(root);
            CreateBody(root, font);
            CreateFooter(root, font);

            GameObjectUtility.SetParentAndAlign(root, parent);
            return root;
        }

        // ── Font asset ───────────────────────────────────────────────────────

        private static TMP_FontAsset GetOrCreateFontAsset()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (asset == null)
                Debug.LogError("[VRClarity] フォントアセットが見つかりません。\n" +
                    $"パッケージが正しくインポートされているか確認してください。\n{FontAssetPath}");
            return asset;
        }

        // ── Panel parts ──────────────────────────────────────────────────────

        private static void CreateBackground(GameObject parent)
        {
            var go = CreateChild(parent, "Background");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = BackgroundColor;
        }

        private static void CreateTopBorder(GameObject parent)
        {
            var go = CreateChild(parent, "TopBorder");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(0f, 4f);
            go.AddComponent<Image>().color = AccentColor;
        }

        private static void CreateHeader(GameObject parent, TMP_FontAsset font)
        {
            var logoRt = CreateChild(parent, "Logo").GetComponent<RectTransform>();
            logoRt.anchorMin = new Vector2(0f, 1f);
            logoRt.anchorMax = new Vector2(0f, 1f);
            logoRt.pivot     = new Vector2(0f, 1f);
            logoRt.anchoredPosition = new Vector2(30f, -20f);
            logoRt.sizeDelta = new Vector2(80f, 80f);
            var logoImg = logoRt.gameObject.AddComponent<RawImage>();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(LogoPath);
            if (tex != null) logoImg.texture = tex;

            var titleTmp = CreateTMP(parent, "Title", font);
            var titleRt  = titleTmp.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0f, 1f);
            titleRt.anchorMax = new Vector2(1f, 1f);
            titleRt.pivot     = new Vector2(0f, 1f);
            titleRt.anchoredPosition = new Vector2(128f, -22f);
            titleRt.sizeDelta = new Vector2(-158f, 50f);
            titleTmp.text      = "VRClarity(β)";
            titleTmp.fontSize  = 40f;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.color     = BodyColor;
            titleTmp.alignment = TextAlignmentOptions.BottomLeft;

            var catchTmp = CreateTMP(parent, "Catchphrase", font);
            var catchRt  = catchTmp.GetComponent<RectTransform>();
            catchRt.anchorMin = new Vector2(0f, 1f);
            catchRt.anchorMax = new Vector2(1f, 1f);
            catchRt.pivot     = new Vector2(0f, 1f);
            catchRt.anchoredPosition = new Vector2(128f, -76f);
            catchRt.sizeDelta = new Vector2(-158f, 34f);
            catchTmp.text      = "ワールドクリエイターのための、分析ツール。";
            catchTmp.fontSize  = 18f;
            catchTmp.color     = SubtleColor;
            catchTmp.alignment = TextAlignmentOptions.TopLeft;
        }

        private static void CreateDivider(GameObject parent)
        {
            var go = CreateChild(parent, "Divider");
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -122f);
            rt.sizeDelta = new Vector2(-60f, 2f);
            go.AddComponent<Image>().color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.35f);
        }

        private static void CreateBody(GameObject parent, TMP_FontAsset font)
        {
            var tmp = CreateTMP(parent, "Body", font);
            var rt  = tmp.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(44f,   54f);
            rt.offsetMax = new Vector2(-44f, -144f);
            tmp.text =
                "このワールドは <color=#00C9A7>VRClarity SDK</color> を導入しています\n\n" +
                "匿名の統計データを取得しています\n" +
                "（UserID・displayName は送信されません）";
            tmp.fontSize    = 22f;
            tmp.color       = BodyColor;
            tmp.alignment   = TextAlignmentOptions.TopLeft;
            tmp.lineSpacing = 6f;
        }

        private static void CreateFooter(GameObject parent, TMP_FontAsset font)
        {
            var tmp = CreateTMP(parent, "Footer", font);
            var rt  = tmp.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot     = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 18f);
            rt.sizeDelta = new Vector2(-60f, 28f);
            tmp.text      = "https://vrclarity.net";
            tmp.fontSize  = 16f;
            tmp.color     = SubtleColor;
            tmp.alignment = TextAlignmentOptions.BottomRight;
        }

        // ── Utility ──────────────────────────────────────────────────────────

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static TextMeshProUGUI CreateTMP(GameObject parent, string name, TMP_FontAsset font)
        {
            var tmp = CreateChild(parent, name).AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            return tmp;
        }
    }
}
