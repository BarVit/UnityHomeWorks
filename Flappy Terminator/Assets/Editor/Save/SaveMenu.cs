using UnityEditor;
using UnityEngine;

namespace SaveTools
{
    public static class SaveMenu
    {
        [MenuItem("Tools/Save/Delete Profile")]
        private static void DeleteProfile()
        {
            SaveStorage.Delete();
            Debug.Log("Сохранение удалено: " + SaveStorage.FilePath);
        }

        [MenuItem("Tools/Save/Show Profile Path")]
        private static void ShowPath()
        {
            Debug.Log("Сохранение лежит здесь: " + SaveStorage.FilePath);
        }
    }
}
