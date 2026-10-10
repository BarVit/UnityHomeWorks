using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace UiContent
{
    public static class ButtonStyle
    {
        public const string ButtonSprite = "Assets/Sci-Fi UI/_SciFi_GUISkin_/Skin_Assets/buttons/button1.png";

        private const float FadeDuration = 0.08f;
        private static readonly Color Idle = new Color(0.8f, 0.8f, 0.8f);
        private static readonly Color Pressed = new Color(0.65f, 0.65f, 0.65f);

        [MenuItem("Tools/UI/Apply Button Hover")]
        public static void ApplyToScene()
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSprite);
            int count = 0;

            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Image image = button.targetGraphic as Image;

                if (image == null || image.sprite != sprite)
                    continue;

                Apply(button);
                count++;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Кнопки: подсветка при наведении на " + count + " кнопках");
        }

        public static void Apply(Button button)
        {
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;

            colors.normalColor = Idle;
            colors.highlightedColor = Color.white;
            colors.selectedColor = Idle;
            colors.pressedColor = Pressed;
            colors.fadeDuration = FadeDuration;
            button.colors = colors;

            if (button.GetComponent<HoverScale>() == null)
                button.gameObject.AddComponent<HoverScale>();

            EditorUtility.SetDirty(button);
        }
    }
}
