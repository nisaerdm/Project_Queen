#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CarSkinData))]
public class CarSkinDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CarSkinData skinData = (CarSkinData)target;
        serializedObject.Update();

        SerializedProperty isCustomProp = serializedObject.FindProperty("isCustomDesign");
        EditorGUILayout.PropertyField(isCustomProp, new GUIContent("Özel Çizim mi?"));

        EditorGUILayout.Space();

        if (skinData.isCustomDesign)
        {
            SerializedProperty customPartsProp = serializedObject.FindProperty("customPartMaterials");
            EditorGUILayout.PropertyField(customPartsProp, new GUIContent("Özel Parça Materyalleri"), true);
        }
        else
        {
            SerializedProperty normalMatProp = serializedObject.FindProperty("normalMaterial");
            EditorGUILayout.PropertyField(normalMatProp, new GUIContent("Normal Renk Materyali"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif