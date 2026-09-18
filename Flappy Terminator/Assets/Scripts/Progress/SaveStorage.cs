using System;
using System.IO;
using UnityEngine;

public static class SaveStorage
{
    private const string FileName = "profile.json";

    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static SaveData Load()
    {
        if (File.Exists(FilePath) == false)
            return new SaveData();

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));

            if (data == null || data.Version != SaveData.CurrentVersion)
                return new SaveData();

            return data;
        }
        catch (Exception)
        {
            return new SaveData();
        }
    }

    public static void Save(SaveData data)
    {
        File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
    }

    public static void Delete()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }
}
