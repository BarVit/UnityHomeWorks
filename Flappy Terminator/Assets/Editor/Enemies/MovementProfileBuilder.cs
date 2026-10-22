using System;
using UnityEditor;
using UnityEngine;

namespace EnemyContent
{
    public static class MovementProfileBuilder
    {
        private const string Folder = "Assets/Data/Movements";

        [MenuItem("Tools/Enemies/Create Movement Profiles")]
        public static void Build()
        {
            if (AssetDatabase.IsValidFolder(Folder) == false)
                AssetDatabase.CreateFolder("Assets/Data", "Movements");

            Create<ForwardProfile>("Forward", profile => SetSpeed(profile, 0.5f, 3f));
            Create<DriftProfile>("Drift", profile =>
            {
                SetSpeed(profile, 0.5f, 3f);
                SetDrift(profile);
            });
            Create<HoverProfile>("Hover", profile =>
            {
                SetSpeed(profile, 0.5f, 3f);
                SetHover(profile, 0.1f, 0.1f, 3, 0.5f, 3f, 5f);
            });
            Create<HoverProfile>("Hover Guard", profile =>
            {
                SetSpeed(profile, 0.5f, 3f);
                SetHover(profile, 0.1f, 0.333f, 1, 1f, 30f, 30f);
            });
            Create<ForwardLeapProfile>("Forward Leap", profile =>
            {
                SetSpeed(profile, 1f, 1f);
                profile.FindProperty("_leapSpeed").floatValue = 5f;
                profile.FindProperty("_minLeapStartShare").floatValue = 0.2f;
                profile.FindProperty("_maxLeapStartShare").floatValue = 0.2f;
                profile.FindProperty("_leapShare").floatValue = 0.4f;
                profile.FindProperty("_afterLeapSpeed").floatValue = 0.75f;
            });
            Create<DriftRetreatProfile>("Drift Retreat", profile =>
            {
                SetSpeed(profile, 0.5f, 3f);
                SetDrift(profile);
                profile.FindProperty("_advanceShare").floatValue = 0.667f;
                profile.FindProperty("_retreatShare").floatValue = 0.5f;
            });
            Create<UpDownProfile>("Up And Down", profile =>
            {
                SetSpeed(profile, 1f, 1f);
                profile.FindProperty("_entryShare").floatValue = 0.25f;
                profile.FindProperty("_duration").floatValue = 20f;
                profile.FindProperty("_edgeMargin").floatValue = 1.5f;
            });
            Create<UpDownProfile>("Boss Up And Down", profile =>
            {
                SetSpeed(profile, 0.5f, 2f);
                profile.FindProperty("_entryShare").floatValue = 0f;
                profile.FindProperty("_duration").floatValue = 0f;
                profile.FindProperty("_edgeMargin").floatValue = 0f;
                profile.FindProperty("_bodyGapShare").floatValue = 0.333f;
            });
            Create<EllipseProfile>("Ellipse", profile =>
            {
                SetSpeed(profile, 1f, 3f);
                profile.FindProperty("_minRadiusShare").floatValue = 0.3f;
                profile.FindProperty("_maxRadiusShare").floatValue = 0.6f;
                profile.FindProperty("_backwardSpeedShare").floatValue = 0.5f;
            });

            AssetDatabase.SaveAssets();
            Debug.Log("Режимы полёта: профили на месте в " + Folder);
        }

        private static void Create<TProfile>(string name, Action<SerializedObject> fill)
            where TProfile : MovementProfile
        {
            string path = Folder + "/" + name + ".asset";

            if (AssetDatabase.LoadAssetAtPath<TProfile>(path) != null)
                return;

            TProfile profile = ScriptableObject.CreateInstance<TProfile>();

            AssetDatabase.CreateAsset(profile, path);

            SerializedObject serialized = new SerializedObject(profile);

            fill(serialized);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetSpeed(SerializedObject profile, float min, float max)
        {
            profile.FindProperty("_minSpeed").floatValue = min;
            profile.FindProperty("_maxSpeed").floatValue = max;
        }

        private static void SetDrift(SerializedObject profile)
        {
            profile.FindProperty("_amplitude").floatValue = 1.5f;
            profile.FindProperty("_period").floatValue = 2f;
        }

        private static void SetHover(SerializedObject profile, float minFirst, float maxFirst, int count,
            float chance, float minHold, float maxHold)
        {
            profile.FindProperty("_minFirstStopShare").floatValue = minFirst;
            profile.FindProperty("_maxFirstStopShare").floatValue = maxFirst;
            profile.FindProperty("_stopStep").floatValue = 0.333f;
            profile.FindProperty("_stopCount").intValue = count;
            profile.FindProperty("_stopChance").floatValue = chance;
            profile.FindProperty("_minHold").floatValue = minHold;
            profile.FindProperty("_maxHold").floatValue = maxHold;
        }
    }
}
