using TMPro;
using UnityEngine;

namespace StudioPeipeiko.VRClarity.Runtime
{
    /// <summary>
    /// VRClarity Notice Panel の管理コンポーネント。
    /// フォントの一括差し替えに使用します。
    /// VRChat アップロード時にはストリップされますが、フォントは各テキストに適用済みのため影響ありません。
    /// </summary>
    [DisallowMultipleComponent]
    public class VRClarityNoticePanel : MonoBehaviour
    {
        [SerializeField] internal TMP_FontAsset _font;
    }
}
