using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "ExperienceSettings", menuName = "Game/Experience Settings")]
public class ExperienceSO : ScriptableObject
{
    [Header("Generation Parameters")]
    [Tooltip("The XP required to go from level 1 → 2")]
    public int baseExperience = 50;

    [Tooltip("How fast XP requirements grow each level (e.g., 1.15 = 15% more each level).")]
    public float growthRate = 1.15f;

    [Tooltip("Max level to generate XP values for.")]
    public int maxLevel = 100;

    [Header("Generated Data (Read Only)")]
    public List<int> experiencePerLevel = new List<int>();

    // Returns the required XP for reaching a specific level.
    public int GetExperienceForLevel(int level)
    {
        if (level < 1 || level > experiencePerLevel.Count)
            return 0;
        return experiencePerLevel[level - 1];
    }

    //Generates XP requirements from level 1 → maxLevel.
    public void Generate()
    {
        experiencePerLevel.Clear();

        float xp = baseExperience;

        for (int i = 1; i <= maxLevel; i++)
        {
            experiencePerLevel.Add(Mathf.RoundToInt(xp));
            xp *= growthRate;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ExperienceSO))]
public class ExperienceSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ExperienceSO script = (ExperienceSO)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate XP Table"))
        {
            script.Generate();
            EditorUtility.SetDirty(script);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
