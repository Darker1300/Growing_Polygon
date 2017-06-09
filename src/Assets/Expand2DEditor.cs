using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Expand2D))]
public class Expand2DEditor : Editor
{
    Vector3 drawPosition = new Vector3();

    void OnSceneGUI()
    {
        if (Event.current.isMouse)
        {
            Expand2D ex2d = (target as Expand2D);
            if (ex2d.editorDebugDraw)
            {
                {
                    Debug.Log("Event!");
                    Vector2 screenPos = WorldSpaceScreenPosition();
                    drawPosition.x = screenPos.x;
                    drawPosition.y = screenPos.y;
                    drawPosition.z = ex2d.transform.position.z;
                    ex2d.editorMousePos = drawPosition;
                    ex2d.editorScreenPos = Event.current.mousePosition;
                    UnityEditor.SceneView.RepaintAll();
                }
            }
        }
    }

    Vector3 WorldSpaceScreenPosition()
    {
        Vector2 screenPos = Event.current.mousePosition;
        screenPos.y = Camera.current.pixelHeight - screenPos.y;
        return Camera.current.ScreenToWorldPoint(screenPos);
    }
}
