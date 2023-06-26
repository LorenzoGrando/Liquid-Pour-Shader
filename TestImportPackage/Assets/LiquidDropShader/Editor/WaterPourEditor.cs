using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaterPour))]
public class WaterPourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Play Animation")){
            WaterPour script = (WaterPour)target;
            script.CallDrop();
        }
    }
}
