using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaceObstacle))]
public class ObstacleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RaceObstacle obs = (RaceObstacle)target;

        obs.type = (RaceObstacle.ObstacleType)EditorGUILayout.EnumPopup("Obstacle Type", obs.type);

        switch (obs.type)
        {
            case RaceObstacle.ObstacleType.Mud:
                EditorGUILayout.LabelField("Mud Settings", EditorStyles.boldLabel);
                obs.slowMultiplier = EditorGUILayout.FloatField(new GUIContent("Slow Multiplier", "Multiplier applied to player speed when on mud."), obs.slowMultiplier);
                obs.slowDuration = EditorGUILayout.FloatField(new GUIContent("Slow Duration", "Duration in seconds the speed reduction lasts."), obs.slowDuration);
                break;

            case RaceObstacle.ObstacleType.Swinging:
                EditorGUILayout.LabelField("Swinging Obstacle Settings", EditorStyles.boldLabel);
                obs.swingStartPos = EditorGUILayout.Vector3Field(new GUIContent("Start Position", "Starting position of the swinging obstacle."), obs.swingStartPos);
                obs.swingEndPos = EditorGUILayout.Vector3Field(new GUIContent("End Position", "Ending position of the swinging obstacle."), obs.swingEndPos);
                obs.swingSpeed = EditorGUILayout.FloatField(new GUIContent("Speed", "Movement speed of the swinging obstacle."), obs.swingSpeed);
                break;

            case RaceObstacle.ObstacleType.OrganisedStatic:
                EditorGUILayout.LabelField("Organised Static Cubes Settings", EditorStyles.boldLabel);
                obs.cubePrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Cube Prefab", "Prefab used to generate cubes."), obs.cubePrefab, typeof(GameObject), false);
                obs.minCubes = EditorGUILayout.IntField(new GUIContent("Min Cubes", "Minimum number of cubes to generate."), obs.minCubes);
                obs.maxCubes = EditorGUILayout.IntField(new GUIContent("Max Cubes", "Maximum number of cubes to generate."), obs.maxCubes);
                obs.groupChance = EditorGUILayout.Slider(new GUIContent("Group Chance", "Chance that cubes are grouped together."), obs.groupChance, 0f, 1f);
                obs.groupSpacing = EditorGUILayout.FloatField(new GUIContent("Group Spacing", "Spacing between cubes in a group."), obs.groupSpacing);
                break;

            case RaceObstacle.ObstacleType.Static:
                EditorGUILayout.LabelField("Static Obstacle", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("No parameters to edit.");
                break;
        }

        if (GUI.changed)
            EditorUtility.SetDirty(obs);
    }
}
