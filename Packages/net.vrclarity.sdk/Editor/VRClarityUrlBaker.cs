using StudioPeipeiko.VRClarity.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using VRC.SDKBase;

namespace StudioPeipeiko.VRClarity.Editor
{
    /// <summary>
    /// Pre-build processor that bakes encrypted VRCUrl pools into VRClarityTracker components.
    /// Runs automatically before every build via IPreprocessBuildWithReport.
    /// </summary>
    public class VRClarityUrlBaker : IPreprocessBuildWithReport
    {
        private const string BASE_URL = "https://api.vrclarity.net/api/sdk/hb";

        private static readonly int[] StayMilestones = { 1, 5, 15, 30, 60, 120, 240, 360, 480 };
        private static readonly int[] MoveMilestones = { 10, 50, 150, 400, 1000, 2500 };
        private static readonly int[] VisitBuckets = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 40, 50, 75, 100, 150, 200 };
        private static readonly int[] PlatformTypes = { 1, 2, 3, 4, 5 }; // 1=PCVR, 2=Desktop, 3=Quest, 4=Android, 5=iOS
        private const int MAX_PC = 80;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var trackers = Object.FindObjectsOfType<VRClarityTracker>();
            if (trackers == null || trackers.Length == 0) return;

            foreach (var tracker in trackers)
            {
                BakeUrls(tracker);
            }
        }

        /// <summary>
        /// Bake all URL pools for a single VRClarityTracker instance.
        /// Can be called from the custom Inspector for manual baking.
        /// </summary>
        public static bool BakeUrls(VRClarityTracker tracker)
        {
            if (tracker == null) return false;

            if (!VRClarityEncryption.IsValidKeyId(tracker.keyId))
            {
                Debug.LogError($"[VRClarity] Invalid Key ID: '{tracker.keyId}'. Expected format: sk_ + 24 hex characters.");
                return false;
            }

            if (!VRClarityEncryption.IsValidEncryptionKey(tracker.encryptionKey))
            {
                Debug.LogError($"[VRClarity] Invalid Encryption Key. Expected 64 hex characters (256-bit).");
                return false;
            }

            if (string.IsNullOrEmpty(tracker.worldId) || !tracker.worldId.StartsWith("wrld_"))
            {
                Debug.LogError($"[VRClarity] Invalid World ID: '{tracker.worldId}'. Expected format: wrld_xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx");
                return false;
            }

            byte[] keyBytes;
            try
            {
                keyBytes = VRClarityEncryption.HexToBytes(tracker.encryptionKey);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[VRClarity] Failed to parse encryption key: {e.Message}");
                return false;
            }

            string keyId = tracker.keyId;
            string worldId = tracker.worldId;

            var so = new SerializedObject(tracker);

            // Stay URLs (9)
            BakeUrlArray(so, "_stayUrls", "stay", StayMilestones, keyId, worldId, keyBytes);

            // Move URLs (6)
            BakeUrlArray(so, "_moveUrls", "move", MoveMilestones, keyId, worldId, keyBytes);

            // Visit URLs (20 buckets)
            BakeUrlArray(so, "_visitUrls", "visit", VisitBuckets, keyId, worldId, keyBytes);

            // Platform URLs (5)
            BakeUrlArray(so, "_platformUrls", "platform", PlatformTypes, keyId, worldId, keyBytes);

            // PC URLs (0..80)
            BakePcUrls(so, keyId, worldId, keyBytes);

            so.ApplyModifiedProperties();

            int totalUrls = StayMilestones.Length + MoveMilestones.Length + VisitBuckets.Length + PlatformTypes.Length + (MAX_PC + 1);
            Debug.Log($"[VRClarity] Baked {totalUrls} URLs for world {worldId}.");

            return true;
        }

        private static void BakeUrlArray(
            SerializedObject so, string propertyName, string eventType,
            int[] milestones, string keyId, string worldId, byte[] keyBytes)
        {
            var prop = so.FindProperty(propertyName);
            prop.arraySize = milestones.Length;

            for (int i = 0; i < milestones.Length; i++)
            {
                string plaintext = $"w={worldId}&e={eventType}&v={milestones[i]}";
                string encrypted = VRClarityEncryption.EncryptPayload(plaintext, keyBytes);
                string url = $"{BASE_URL}?k={keyId}&d={encrypted}";

                var element = prop.GetArrayElementAtIndex(i);
                var urlField = element.FindPropertyRelative("url");
                if (urlField != null)
                {
                    urlField.stringValue = url;
                }
                else
                {
                    Debug.LogError($"[VRClarity] Failed to find 'url' property on VRCUrl. URL baking may have failed for {propertyName}[{i}].");
                }
            }
        }

        private static void BakePcUrls(
            SerializedObject so, string keyId, string worldId, byte[] keyBytes)
        {
            var prop = so.FindProperty("_pcUrls");
            prop.arraySize = MAX_PC + 1; // 0..80 = 81 entries

            for (int v = 0; v <= MAX_PC; v++)
            {
                string plaintext = $"w={worldId}&e=pc&v={v}";
                string encrypted = VRClarityEncryption.EncryptPayload(plaintext, keyBytes);
                string url = $"{BASE_URL}?k={keyId}&d={encrypted}";

                var element = prop.GetArrayElementAtIndex(v);
                var urlField = element.FindPropertyRelative("url");
                if (urlField != null)
                {
                    urlField.stringValue = url;
                }
                else
                {
                    Debug.LogError($"[VRClarity] Failed to find 'url' property on VRCUrl. URL baking may have failed for _pcUrls[{v}].");
                }
            }
        }
    }
}
