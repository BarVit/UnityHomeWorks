using UnityEditor;
using UnityEngine;

namespace TableImport
{
    public static class GeneratedAsset
    {
        public static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);

            return asset;
        }
    }
}
