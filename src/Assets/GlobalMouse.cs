using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

public class GlobalMouse : MonoBehaviour
{
    public WindowLayout.WindowInfo scene = new WindowLayout.WindowInfo("UnityEditor.SceneView", "Scene", "Window/Scene");
    public WindowLayout.WindowInfo game = new WindowLayout.WindowInfo("UnityEditor.GameView", "Game", "Window/Game");

    public bool RefreshOnMove = false;
    public bool isMouseOnSceneView = false;
    public Vector2 MousePos = new Vector2();

    void Start()
    {

    }

    void Update()
    {
        TryMouseMove();
        // GameView
        if (game.position.Contains(MousePos))
        {
            isMouseOnSceneView = false;
        }
    }

    public void TryMouseMove()
    {
        Vector2 current = GetCursorPosition();
        if (RefreshOnMove)
        {
            if (current != MousePos)
            {
                //Debug.Log("KABLAM! | " + current);
                MousePos = current;
                SceneView.RepaintAll();

                if (scene.position.Contains(current))
                {
                    isMouseOnSceneView = true;
                }
            }
        }
    }

    void OnMouseMove()
    {
        Debug.Log(Input.mousePosition);
    }

    /// <summary>
    /// Struct representing a point.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct POINT
    {
        public int X;
        public int Y;

        public static implicit operator Vector2(POINT point)
        {
            return new Vector2(point.X, point.Y);
        }
    }

    /// <summary>
    /// Retrieves the cursor's position, in screen coordinates.
    /// </summary>
    /// <see>See MSDN documentation for further information.</see>
    [DllImport("user32.dll")]
    static extern bool GetCursorPos(out POINT lpPoint);

    public static Vector2 GetCursorPosition()
    {
        POINT lpPoint;
        GetCursorPos(out lpPoint);

        //bool success = User32.GetCursorPos(out lpPoint);
        // if (!success)

        return lpPoint;
    }

}
