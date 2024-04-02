using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VineGrowthController))]
public class VineGrowthControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VineGrowthController tar = (VineGrowthController)target;

        base.OnInspectorGUI();

        if (GUILayout.Button("Reset Boundary Texture") && Application.isPlaying)
        {
            tar.ResetBoundaryTexture();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Vines") && Application.isPlaying)
        {
            tar.GenerateVines();
        }

        if (GUILayout.Button("Draw Vines") && Application.isPlaying)
        {
            tar.DrawVines(false);
        }

        if (GUILayout.Button("Animate Vines") && Application.isPlaying)
        {
            tar.DrawVines(true);
        }

        //tar.x = EditorGUILayout.FloatField("Name", tar.x);

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Save Texture", EditorStyles.boldLabel);

        tar.TextureName = EditorGUILayout.TextField("Filename", tar.TextureName);

        if (GUILayout.Button("Save Texture") && Application.isPlaying)
        {
            tar.SaveTexture();
        }
    }

}
