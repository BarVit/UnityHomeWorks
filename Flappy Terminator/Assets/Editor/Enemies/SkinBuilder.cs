using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EnemyContent
{
    public static class SkinBuilder
    {
        private const string Folder = "Assets/Data/Skins";
        private const int NeutralSpread = 14;
        private const int OutlineLevel = 36;
        private const int LightLevel = 185;

        private static readonly Color Blue = new Color(0.55f, 0.76f, 1f);
        private static readonly Color DarkBlue = new Color(0.36f, 0.5f, 0.95f);
        private static readonly Color Violet = new Color(0.8f, 0.5f, 1f);
        private static readonly Color DarkViolet = new Color(0.55f, 0.25f, 0.8f);

        [MenuItem("Tools/Enemies/Build Skins")]
        public static void Build()
        {
            if (AssetDatabase.IsValidFolder(Folder) == false)
                AssetDatabase.CreateFolder("Assets/Data", "Skins");

            Make("Assets/Sprites/Ships/Enemy 1.png", "Assets/Sprites/Ships/Enemy 1 Red.png", "Warden II",
                color => Recolor(color, true, false));
            Make("Assets/Sprites/Ships/Enemy 1.png", "Assets/Sprites/Ships/Enemy 1 Red Dark.png", "Warden III",
                color => Recolor(color, true, true));
            Make("Assets/Sprites/Ships/Enemy 2.png", "Assets/Sprites/Ships/Enemy 2 Blue.png", "Hunter II",
                color => Tint(color, Blue, DarkBlue));
            Make("Assets/Sprites/Ships/Enemy 2.png", "Assets/Sprites/Ships/Enemy 2 Violet.png", "Hunter III",
                color => Tint(color, Violet, DarkViolet));

            AssetDatabase.SaveAssets();
            Debug.Log("Скины врагов: собраны в " + Folder);
        }

        private static void Make(string source, string target, string skinName, Func<Color32, Color32> recolor)
        {
            if (File.Exists(target) == false)
                AssetDatabase.CopyAsset(source, target);

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);

            texture.LoadImage(File.ReadAllBytes(source));

            Color32[] pixels = texture.GetPixels32();

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = recolor(pixels[i]);

            texture.SetPixels32(pixels);
            File.WriteAllBytes(target, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceUpdate);

            string skinPath = Folder + "/" + skinName + ".asset";
            SpriteSkin skin = AssetDatabase.LoadAssetAtPath<SpriteSkin>(skinPath);

            if (skin == null)
            {
                skin = ScriptableObject.CreateInstance<SpriteSkin>();
                AssetDatabase.CreateAsset(skin, skinPath);
            }

            Sprite[] from = LoadSprites(source);
            Sprite[] to = LoadSprites(target);

            if (from.Length != to.Length)
                Debug.LogWarning("Скин " + skinName + ": кадров " + from.Length + " и " + to.Length);

            skin.Fill(from, to);
            EditorUtility.SetDirty(skin);
        }

        private static Sprite[] LoadSprites(string path)
        {
            List<Sprite> sprites = new List<Sprite>();

            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite sprite)
                    sprites.Add(sprite);
            }

            sprites.Sort((left, right) => string.CompareOrdinal(left.name, right.name));

            return sprites.ToArray();
        }

        private static Color32 Recolor(Color32 color, bool greyToRed, bool lightToDark)
        {
            if (color.a == 0)
                return color;

            int max = Math.Max(color.r, Math.Max(color.g, color.b));
            int min = Math.Min(color.r, Math.Min(color.g, color.b));

            if (max - min > NeutralSpread || max < OutlineLevel)
                return color;

            if (max >= LightLevel)
                return lightToDark ? ToDarkGrey(max, color.a) : color;

            return greyToRed ? ToRed(max, color.a) : color;
        }

        private static Color32 Tint(Color32 color, Color light, Color dark)
        {
            if (color.a == 0)
                return color;

            int max = Math.Max(color.r, Math.Max(color.g, color.b));
            int min = Math.Min(color.r, Math.Min(color.g, color.b));

            if (max - min > NeutralSpread || max < OutlineLevel)
                return color;

            Color tint = max >= LightLevel ? light : dark;

            return new Color32(Scale(max, tint.r), Scale(max, tint.g), Scale(max, tint.b), color.a);
        }

        private static byte Scale(int value, float share)
        {
            return (byte)Mathf.Clamp(Mathf.RoundToInt(value * share), 0, 255);
        }

        private static Color32 ToRed(int value, byte alpha)
        {
            return new Color32((byte)Math.Min(255, value * 13 / 10 + 10), (byte)(value * 22 / 100),
                (byte)(value * 22 / 100), alpha);
        }

        private static Color32 ToDarkGrey(int value, byte alpha)
        {
            int grey = 40 + (value - LightLevel) * 3 / 4;

            return new Color32((byte)grey, (byte)grey, (byte)(grey + 4), alpha);
        }
    }
}
