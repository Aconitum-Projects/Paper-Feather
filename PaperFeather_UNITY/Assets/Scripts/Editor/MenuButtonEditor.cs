using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MenuButton))]
public class MenuButtonEditor : Editor
{
    private SerializedProperty buttonActionProp;
    private SerializedProperty sceneToLoadProp;
    private SerializedProperty loadingCanvasPrefabProp;
    private SerializedProperty canvasToShowProp;
    private SerializedProperty canvasesToHideProp;

    private void OnEnable()
    {
        buttonActionProp = serializedObject.FindProperty("buttonAction");
        sceneToLoadProp = serializedObject.FindProperty("sceneToLoad");
        loadingCanvasPrefabProp = serializedObject.FindProperty("loadingCanvasPrefab");
        canvasToShowProp = serializedObject.FindProperty("canvasToShow");
        canvasesToHideProp = serializedObject.FindProperty("canvasesToHide");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(buttonActionProp, new GUIContent("Button Action"));

        MenuButton.ButtonAction action = (MenuButton.ButtonAction)buttonActionProp.enumValueIndex;

        switch (action)
        {
            case MenuButton.ButtonAction.Play:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Play Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(sceneToLoadProp, new GUIContent("Scene To Load"));
                EditorGUILayout.PropertyField(loadingCanvasPrefabProp, new GUIContent("Loading Canvas Prefab"));
                break;

            case MenuButton.ButtonAction.SwitchCanvas:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Switch Canvas Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(canvasToShowProp, new GUIContent("Canvas To Show"));
                EditorGUILayout.PropertyField(canvasesToHideProp, new GUIContent("Canvases To Hide"));
                break;

            case MenuButton.ButtonAction.Quit:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Quit Settings", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("No extra parameters are needed for Quit.", MessageType.Info);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
