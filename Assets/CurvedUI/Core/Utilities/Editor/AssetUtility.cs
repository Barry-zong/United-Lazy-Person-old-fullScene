using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CurvedUI.Core.Utilities.Editor
{
    public static class AssetUtility
    {
        public static T TryFindAssetOfType<T>(params string[] nameParts) where T : UnityEngine.Object 
            => FindAssetsOfType<T>(nameParts).FirstOrDefault();

        public static IEnumerable<T> FindAssetsOfType<T>(params string[] nameParts) where T : UnityEngine.Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(x => x != null)
                .Where(x => nameParts == null || nameParts.Length == 0 || nameParts.All(part => x.name.Contains(part)));

        public static IEnumerable<T> GetSubAssetsOfType<T>(UnityEngine.Object mainAsset) where T : UnityEngine.Object =>
            AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(mainAsset))
                .OfType<T>()
                .Where(x => x != null && (x.hideFlags & HideFlags.HideInHierarchy) == 0);
    }
}