using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(AnimManager))]
public class AnimManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("Play Pour Animation")){
            AnimManager script = (AnimManager)target;
            script.PlayFlip();
        }
        if(GUILayout.Button("Stop Pour Animation")){
            AnimManager script = (AnimManager)target;
            script.PlayReverse();
        }
    }
}
