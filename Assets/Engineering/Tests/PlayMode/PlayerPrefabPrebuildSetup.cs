#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.TestTools;

namespace Engineering.Tests
{
    public class PlayerPrefabPrebuildSetup : IPrebuildSetup
    {
        private const string SourcePath = "Assets/Engineering/Prefabs/Player.prefab";
        private const string TargetDir = "Assets/Engineering/Tests/PlayMode/Resources";
        private const string TargetFileName = "Player.prefab";

        public void Setup()
        {
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder(TargetDir))
            {
                var parent = Path.GetDirectoryName(TargetDir)?.Replace("\\", "/");
                var folderName = Path.GetFileName(TargetDir);
                if (!string.IsNullOrEmpty(parent))
                    AssetDatabase.CreateFolder(parent, folderName);
            }

            var targetPath = Path.Combine(TargetDir, TargetFileName).Replace("\\", "/");

            var targetAssetPath = targetPath;
            if (System.IO.File.Exists(targetAssetPath))
            {
                var existing = AssetDatabase.LoadAssetAtPath<GameObject>(targetAssetPath);
                if (existing != null)
                    return;
            }

            if (!AssetDatabase.CopyAsset(SourcePath, targetPath))
                Debug.LogError(
                    $"[PlayerPrefabPrebuildSetup] Failed to copy '{SourcePath}' to '{targetPath}'.");
#endif
        }
    }
}
