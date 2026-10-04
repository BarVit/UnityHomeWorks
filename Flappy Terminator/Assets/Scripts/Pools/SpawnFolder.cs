using UnityEngine;

public static class SpawnFolder
{
    private const string RootName = "Spawned";

    public static Transform Get(string section)
    {
        GameObject root = GameObject.Find(RootName);

        if (root == null)
            root = new GameObject(RootName);

        return Get(root.transform, section);
    }

    public static Transform Get(Transform parent, string name)
    {
        Transform found = parent.Find(name);

        if (found != null)
            return found;

        Transform created = new GameObject(name).transform;

        created.SetParent(parent, false);

        return created;
    }
}
