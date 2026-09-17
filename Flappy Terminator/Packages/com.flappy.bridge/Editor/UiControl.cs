using UnityEngine;
using UnityEngine.UI;

namespace FlappyBridge
{
    public static class UiControl
    {
        public static string Click(string path)
        {
            if (Application.isPlaying == false)
                return Json.Error("the editor is not playing, so there is no interface to click");

            Transform found = SceneQuery.Find(path);

            if (found == null)
                return Json.Error("no object at path " + path);

            Button button = found.GetComponent<Button>();

            if (button == null)
                return Json.Error("no Button on " + path);

            if (button.IsActive() == false || button.IsInteractable() == false)
                return Json.Error("the button on " + path + " is not accepting clicks right now");

            button.onClick.Invoke();

            return Json.Object(
                "\"ok\":true",
                Json.Field("path", path),
                Json.Field("note", "the click handler was invoked directly, not delivered as a pointer event"));
        }
    }
}
