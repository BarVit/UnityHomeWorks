using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FlappyBridge
{
    public static class SceneQuery
    {
        private const int MaxNodes = 600;

        public static string GetHierarchy(string path, int depth)
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            List<string> nodes = new List<string>();
            int budget = MaxNodes;

            if (path.Length == 0)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                    nodes.Add(Describe(root.transform, depth, ref budget));
            }
            else
            {
                Transform found = Find(path);

                if (found == null)
                    return Json.Error("no object at path " + path);

                nodes.Add(Describe(found, depth, ref budget));
            }

            return Json.Object(
                "\"ok\":true",
                Json.Field("scene", scene.name),
                Json.Field("truncated", budget <= 0),
                Json.Raw("roots", Json.Array(nodes)));
        }

        public static string GetObject(string path)
        {
            Transform found = Find(path);

            if (found == null)
                return Json.Error("no object at path " + path);

            GameObject target = found.gameObject;
            List<string> components = new List<string>();

            foreach (Component component in target.GetComponents<Component>())
            {
                if (component == null)
                {
                    components.Add(Json.Object(Json.Field("type", "<missing script>")));

                    continue;
                }

                components.Add(Json.Object(
                    Json.Field("type", component.GetType().Name),
                    Json.Raw("properties", Json.Array(ReadProperties(component)))));
            }

            return Json.Object(
                "\"ok\":true",
                Json.Field("path", path),
                Json.Field("name", target.name),
                Json.Field("active", target.activeSelf),
                Json.Field("tag", target.tag),
                Json.Field("layer", LayerMask.LayerToName(target.layer)),
                Json.Raw("components", Json.Array(components)));
        }

        public static string ReadProperty(string path, string componentName, string propertyName)
        {
            Transform found = Find(path);

            if (found == null)
                return Json.Error("no object at path " + path);

            foreach (Component component in found.GetComponents<Component>())
            {
                if (component == null || component.GetType().Name != componentName)
                    continue;

                System.Reflection.PropertyInfo property = component.GetType().GetProperty(propertyName);

                if (property == null || property.CanRead == false)
                    return Json.Error("no readable property " + propertyName + " on " + componentName);

                object value = property.GetValue(component);

                return Json.Object(
                    "\"ok\":true",
                    Json.Field("component", componentName),
                    Json.Field("property", propertyName),
                    Json.Field("value", value == null ? "null" : value.ToString()));
            }

            return Json.Error("no component " + componentName + " on " + path);
        }

        public static string CallMethod(string path, string componentName, string methodName)
        {
            Transform found = Find(path);

            if (found == null)
                return Json.Error("no object at path " + path);

            foreach (Component component in found.GetComponents<Component>())
            {
                if (component == null || component.GetType().Name != componentName)
                    continue;

                System.Reflection.MethodInfo method = component.GetType()
                    .GetMethod(methodName, System.Type.EmptyTypes);

                if (method == null)
                    return Json.Error("no public method " + methodName + "() without arguments on " + componentName);

                method.Invoke(component, null);

                return Json.Object(
                    "\"ok\":true",
                    Json.Field("component", componentName),
                    Json.Field("method", methodName));
            }

            return Json.Error("no component " + componentName + " on " + path);
        }

        public static Transform Find(string path)
        {
            string[] parts = path.Split('/');
            Transform current = null;

            foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == parts[0])
                {
                    current = root.transform;

                    break;
                }
            }

            if (current == null)
                return null;

            for (int i = 1; i < parts.Length; i++)
            {
                Transform next = null;

                for (int child = 0; child < current.childCount; child++)
                {
                    if (current.GetChild(child).name == parts[i])
                    {
                        next = current.GetChild(child);

                        break;
                    }
                }

                if (next == null)
                    return null;

                current = next;
            }

            return current;
        }

        public static string Format(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString(CultureInfo.InvariantCulture);

                case SerializedPropertyType.Boolean:
                    return property.boolValue ? "true" : "false";

                case SerializedPropertyType.Float:
                    return property.floatValue.ToString("0.#####", CultureInfo.InvariantCulture);

                case SerializedPropertyType.String:
                    return property.stringValue;

                case SerializedPropertyType.Enum:
                    return FormatEnum(property);

                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null
                        ? "null"
                        : property.objectReferenceValue.name + " (" + property.objectReferenceValue.GetType().Name + ")";

                case SerializedPropertyType.Vector2:
                    return Pack(property.vector2Value.x, property.vector2Value.y);

                case SerializedPropertyType.Vector3:
                    return Pack(property.vector3Value.x, property.vector3Value.y, property.vector3Value.z);

                case SerializedPropertyType.Vector4:
                    return Pack(property.vector4Value.x, property.vector4Value.y, property.vector4Value.z,
                        property.vector4Value.w);

                case SerializedPropertyType.Quaternion:
                    Vector3 euler = property.quaternionValue.eulerAngles;

                    return Pack(euler.x, euler.y, euler.z);

                case SerializedPropertyType.Color:
                    return Pack(property.colorValue.r, property.colorValue.g, property.colorValue.b,
                        property.colorValue.a);

                case SerializedPropertyType.LayerMask:
                    return property.intValue.ToString(CultureInfo.InvariantCulture);

                case SerializedPropertyType.ArraySize:
                    return property.intValue.ToString(CultureInfo.InvariantCulture);

                default:
                    if (property.isArray)
                        return "array[" + property.arraySize + "]";

                    return property.propertyType.ToString();
            }
        }

        private static string FormatEnum(SerializedProperty property)
        {
            string[] names = property.enumNames;

            if (property.enumValueIndex >= 0 && property.enumValueIndex < names.Length)
                return names[property.enumValueIndex];

            return property.intValue.ToString(CultureInfo.InvariantCulture);
        }

        private static string Pack(params float[] values)
        {
            string[] parts = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
                parts[i] = values[i].ToString("0.#####", CultureInfo.InvariantCulture);

            return string.Join(",", parts);
        }

        private static List<string> ReadProperties(Component component)
        {
            List<string> result = new List<string>();
            SerializedObject serialized = new SerializedObject(component);
            SerializedProperty property = serialized.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                result.Add(Json.Object(
                    Json.Field("name", property.name),
                    Json.Field("type", property.propertyType.ToString()),
                    Json.Field("value", Format(property))));
            }

            serialized.Dispose();

            return result;
        }

        private static string Describe(Transform transform, int depth, ref int budget)
        {
            budget--;

            List<string> fields = new List<string>();
            fields.Add(Json.Field("name", transform.name));
            fields.Add(Json.Field("active", transform.gameObject.activeSelf));
            fields.Add(Json.Raw("components", Json.Array(GetComponentNames(transform.gameObject))));

            if (depth > 0 && transform.childCount > 0 && budget > 0)
            {
                List<string> children = new List<string>();

                for (int child = 0; child < transform.childCount && budget > 0; child++)
                    children.Add(Describe(transform.GetChild(child), depth - 1, ref budget));

                fields.Add(Json.Raw("children", Json.Array(children)));
            }

            return Json.Object(fields.ToArray());
        }

        private static List<string> GetComponentNames(GameObject target)
        {
            List<string> names = new List<string>();

            foreach (Component component in target.GetComponents<Component>())
                names.Add(Json.Quote(component == null ? "<missing script>" : component.GetType().Name));

            return names;
        }
    }
}
