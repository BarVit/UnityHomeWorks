using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FlappyBridge
{
    public static class SceneEdit
    {
        public static string SetProperty(string path, string componentName, string propertyName, string value)
        {
            Transform found = SceneQuery.Find(path);

            if (found == null)
                return Json.Error("no object at path " + path);

            if (componentName.Length == 0)
                return SetObjectField(found.gameObject, propertyName, value);

            Component component = FindComponent(found.gameObject, componentName);

            if (component == null)
                return Json.Error("no component " + componentName + " on " + path);

            SerializedObject serialized = new SerializedObject(component);
            SerializedProperty property = serialized.FindProperty(propertyName);

            if (property == null)
            {
                serialized.Dispose();

                return Json.Error("no serialized property " + propertyName + " on " + componentName
                    + "; read /object?path=" + path + " for the available names");
            }

            string before = SceneQuery.Format(property);
            string failure = Apply(property, value);

            if (failure != null)
            {
                serialized.Dispose();

                return Json.Error(failure);
            }

            serialized.ApplyModifiedProperties();
            string after = SceneQuery.Format(property);
            serialized.Dispose();

            MarkDirty(found.gameObject);

            return Json.Object(
                "\"ok\":true",
                Json.Field("path", path),
                Json.Field("component", componentName),
                Json.Field("property", propertyName),
                Json.Field("before", before),
                Json.Field("after", after));
        }

        public static string CreateObject(string parentPath, string name, bool asInterface)
        {
            if (name.Length == 0)
                return Json.Error("pass a name for the new object");

            GameObject created = asInterface
                ? new GameObject(name, typeof(RectTransform))
                : new GameObject(name);

            if (parentPath.Length > 0)
            {
                Transform parent = SceneQuery.Find(parentPath);

                if (parent == null)
                {
                    Object.DestroyImmediate(created);

                    return Json.Error("no object at " + parentPath);
                }

                created.transform.SetParent(parent, false);
            }

            Undo.RegisterCreatedObjectUndo(created, "Bridge create " + name);
            MarkDirty(created);

            return Json.Object(
                "\"ok\":true",
                Json.Field("created", name),
                Json.Field("parent", parentPath));
        }

        public static string AddComponent(string path, string typeName)
        {
            Transform found = SceneQuery.Find(path);

            if (found == null)
                return Json.Error("no object at path " + path);

            System.Type type = ResolveComponentType(typeName);

            if (type == null)
                return Json.Error("no component type named " + typeName + " in the loaded assemblies");

            Component added = Undo.AddComponent(found.gameObject, type);

            if (added == null)
                return Json.Error(typeName + " could not be added to " + path);

            MarkDirty(found.gameObject);

            return Json.Object(
                "\"ok\":true",
                Json.Field("path", path),
                Json.Field("added", added.GetType().Name));
        }

        public static string RemoveComponent(string path, string typeName)
        {
            Transform found = SceneQuery.Find(path);

            if (found == null)
                return Json.Error("no object at path " + path);

            if (typeName == "missing")
            {
                int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(found.gameObject);
                MarkDirty(found.gameObject);

                return Json.Object(
                    "\"ok\":true",
                    Json.Field("path", path),
                    Json.Field("removed", removed + " missing scripts"));
            }

            Component component = FindComponent(found.gameObject, typeName);

            if (component == null)
                return Json.Error("no component " + typeName + " on " + path);

            GameObject owner = found.gameObject;
            Undo.DestroyObjectImmediate(component);
            MarkDirty(owner);

            return Json.Object(
                "\"ok\":true",
                Json.Field("path", path),
                Json.Field("removed", typeName));
        }

        private static System.Type ResolveComponentType(string typeName)
        {
            foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (System.Reflection.ReflectionTypeLoadException exception)
                {
                    types = exception.Types;
                }

                foreach (System.Type type in types)
                {
                    if (type == null)
                        continue;

                    if (type.Name == typeName && typeof(Component).IsAssignableFrom(type) && type.IsAbstract == false)
                        return type;
                }
            }

            return null;
        }

        private static string SetObjectField(GameObject target, string propertyName, string value)
        {
            Undo.RecordObject(target, "Bridge set " + propertyName);

            string before;

            if (propertyName == "active")
            {
                before = target.activeSelf ? "true" : "false";
                target.SetActive(value == "true" || value == "1");
            }
            else if (propertyName == "name")
            {
                before = target.name;
                target.name = value;
            }
            else if (propertyName == "tag")
            {
                before = target.tag;
                target.tag = value;
            }
            else if (propertyName == "layer")
            {
                before = LayerMask.LayerToName(target.layer);
                target.layer = LayerMask.NameToLayer(value);
            }
            else
            {
                return Json.Error("without a component only active, name, tag and layer can be set");
            }

            MarkDirty(target);

            return Json.Object(
                "\"ok\":true",
                Json.Field("path", target.name),
                Json.Field("property", propertyName),
                Json.Field("before", before),
                Json.Field("after", value));
        }

        private static void MarkDirty(GameObject target)
        {
            if (Application.isPlaying)
                return;

            EditorUtility.SetDirty(target);
            EditorSceneManager.MarkSceneDirty(target.scene);
        }

        private static Component FindComponent(GameObject target, string componentName)
        {
            foreach (Component component in target.GetComponents<Component>())
            {
                if (component != null && component.GetType().Name == componentName)
                    return component;
            }

            return null;
        }

        private static string Apply(SerializedProperty property, string value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return ApplyInteger(property, value);

                case SerializedPropertyType.Boolean:
                    property.boolValue = value == "true" || value == "1";

                    return null;

                case SerializedPropertyType.Float:
                    return ApplyFloat(property, value);

                case SerializedPropertyType.String:
                    property.stringValue = value;

                    return null;

                case SerializedPropertyType.Enum:
                    return ApplyEnum(property, value);

                case SerializedPropertyType.Vector2:
                    return ApplyVector2(property, value);

                case SerializedPropertyType.Vector3:
                    return ApplyVector3(property, value);

                case SerializedPropertyType.Color:
                    return ApplyColor(property, value);

                case SerializedPropertyType.ObjectReference:
                    return ApplyObjectReference(property, value);

                default:
                    return property.propertyType + " cannot be set through the bridge yet";
            }
        }

        private static string ApplyObjectReference(SerializedProperty property, string value)
        {
            if (value.Length == 0 || value == "null")
            {
                property.objectReferenceValue = null;

                return null;
            }

            if (value.StartsWith("builtin:"))
            {
                string builtinPath = value.Substring("builtin:".Length);
                Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(builtinPath);

                if (sprite == null)
                    return "no builtin sprite at " + builtinPath
                        + "; try UI/Skin/Knob.psd or UI/Skin/UISprite.psd";

                property.objectReferenceValue = sprite;

                return null;
            }

            if (value.StartsWith("Assets/"))
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(value);

                if (asset == null)
                    return "no asset at " + value;

                property.objectReferenceValue = asset;

                return null;
            }

            string objectPath = value;
            string componentName = string.Empty;
            int separator = value.IndexOf('#');

            if (separator >= 0)
            {
                objectPath = value.Substring(0, separator);
                componentName = value.Substring(separator + 1);
            }

            Transform target = SceneQuery.Find(objectPath);

            if (target == null)
                return "no object at " + objectPath
                    + "; pass a hierarchy path, a hierarchy path with #ComponentType, or a path under Assets/";

            if (componentName.Length == 0)
            {
                property.objectReferenceValue = target.gameObject;

                return null;
            }

            Component component = FindComponent(target.gameObject, componentName);

            if (component == null)
                return "no component " + componentName + " on " + objectPath;

            property.objectReferenceValue = component;

            return null;
        }

        private static string ApplyInteger(SerializedProperty property, string value)
        {
            int parsed;

            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) == false)
                return value + " is not a whole number";

            property.intValue = parsed;

            return null;
        }

        private static string ApplyFloat(SerializedProperty property, string value)
        {
            float parsed;

            if (TryParseFloat(value, out parsed) == false)
                return value + " is not a number";

            property.floatValue = parsed;

            return null;
        }

        private static string ApplyEnum(SerializedProperty property, string value)
        {
            string[] names = property.enumNames;

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == value)
                {
                    property.enumValueIndex = i;

                    return null;
                }
            }

            int index;

            if (int.TryParse(value, out index) && index >= 0 && index < names.Length)
            {
                property.enumValueIndex = index;

                return null;
            }

            return value + " is not one of " + string.Join(", ", names);
        }

        private static string ApplyVector2(SerializedProperty property, string value)
        {
            float[] parts;

            if (TryParseNumbers(value, 2, out parts) == false)
                return "expected two numbers separated by commas, got " + value;

            property.vector2Value = new Vector2(parts[0], parts[1]);

            return null;
        }

        private static string ApplyVector3(SerializedProperty property, string value)
        {
            float[] parts;

            if (TryParseNumbers(value, 3, out parts) == false)
                return "expected three numbers separated by commas, got " + value;

            property.vector3Value = new Vector3(parts[0], parts[1], parts[2]);

            return null;
        }

        private static string ApplyColor(SerializedProperty property, string value)
        {
            float[] parts;

            if (TryParseNumbers(value, 4, out parts) == false)
                return "expected four numbers separated by commas, got " + value;

            property.colorValue = new Color(parts[0], parts[1], parts[2], parts[3]);

            return null;
        }

        private static bool TryParseNumbers(string value, int count, out float[] parsed)
        {
            parsed = new float[count];
            string[] parts = value.Split(',');

            if (parts.Length != count)
                return false;

            for (int i = 0; i < count; i++)
            {
                if (TryParseFloat(parts[i], out parsed[i]) == false)
                    return false;
            }

            return true;
        }

        private static bool TryParseFloat(string value, out float parsed)
        {
            return float.TryParse(value.Trim().Replace(',', '.'), NumberStyles.Float,
                CultureInfo.InvariantCulture, out parsed);
        }
    }
}
