using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GlobalMouse))]
public class GlobalMouseEditor : Editor
{
    void OnSceneGUI()
    {
        if (Event.current.isMouse)
        {
            GlobalMouse gm = (target as GlobalMouse);
            if (gm.RefreshOnMove)
            {
                gm.isMouseOnSceneView = true;
                gm.TryMouseMove(); // SceneView
            }
        }
    }
}