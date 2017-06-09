using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class WindowLayout
{
    [MenuItem("Window/LAYOUT TEST", false, 99)]
    public static void LayoutTestCommand()
    {
        // initialize window-specific title data and such (this can be cached)
        WindowInfos windows = new WindowInfos();
        // print some position
        if (windows.game.isOpen)
            Debug.Log("game window was open at " + windows.game.position);
        else
            Debug.Log("game window was closed");
        // move some windows to various places, and open them if they aren't already open
        windows.game.position = new Rect(100, 100, 300, 300); // left,top,width,height
        windows.inspector.position = new Rect(400, 200, 200, 400);
        windows.hierarchy.position = new Rect(600, 100, 100, 400);
        windows.project.position = new Rect(700, 100, 100, 400);
        windows.console.position = new Rect(200, 550, 600, 150);
        // ensure a window is open without moving it from its previous position
        windows.scene.isOpen = true;

        // close a window if it was open
        windows.animation.isOpen = false;
    }

    public class WindowInfos
    {
        // note: some of this data might need to change a little between different versions of Unity
        public WindowInfo scene = new WindowInfo("UnityEditor.SceneView", "Scene", "Window/Scene");
        public WindowInfo game = new WindowInfo("UnityEditor.GameView", "Game", "Window/Game");
        public WindowInfo inspector = new WindowInfo("UnityEditor.InspectorWindow", "Inspector", "Window/Inspector");
        public WindowInfo hierarchy = new WindowInfo("UnityEditor.HierarchyWindow", "Hierarchy", "Window/Hierarchy");
        public WindowInfo project = new WindowInfo("UnityEditor.ProjectWindow", "Project", "Window/Project");
        public WindowInfo animation = new WindowInfo("UnityEditor.AnimationWindow", "Animation", "Window/Animation");
        public WindowInfo profiler = new WindowInfo("UnityEditor.ProfilerWindow", "Profiler", "Window/Profiler");
        public WindowInfo console = new WindowInfo("UnityEditor.ConsoleWindow", "Console", "Window/Console");
        public WindowInfo navigation = new WindowInfo("UnityEditor.NavMeshEditorWindow", "Navigation", "Window/Navigation");
        public WindowInfo occlusion = new WindowInfo("UnityEditor.OcclusionCullingWindow", "Occlusion", "Window/Occlusion Culling");
        public WindowInfo lightmapping = new WindowInfo("UnityEditor.LightmappingWindow", "Lightmapping", "Window/Lightmapping");
        public WindowInfo assetServer = new WindowInfo("UnityEditor.ASMainWindow", "Server", "Window/Asset Server");
        public WindowInfo assetStore = new WindowInfo("UnityEditor.AssetStoreWindow", "Asset Store", "Window/Asset Store");
        public WindowInfo particle = new WindowInfo("UnityEditor.ParticleSystemWindow", "Particle Effect", "Window/Particle Effect");
        public MainWindow main = new MainWindow();
    }
    public class WindowInfo
    {
        string defaultTitle;
        string menuPath;
        Type type;
        public WindowInfo(string typeName, string defaultTitle = null, string menuPath = null, System.Reflection.Assembly assembly = null)
        {
            this.defaultTitle = defaultTitle;
            this.menuPath = menuPath;
            if (assembly == null)
                assembly = typeof(UnityEditor.EditorWindow).Assembly;
            type = assembly.GetType(typeName);
            if (type == null)
                Debug.LogWarning("Unable to find type \"" + typeName + "\" in assembly \"" + assembly.GetName().Name + "\".\nYou might want to update the data in WindowInfos.");
        }
        public EditorWindow[] FindAll()
        {
            if (type == null)
                return new EditorWindow[0];
            return (EditorWindow[])(Resources.FindObjectsOfTypeAll(type));
        }
        public EditorWindow FindFirst()
        {
            foreach (EditorWindow window in FindAll())
                return window;
            return null;
        }
        public EditorWindow FindFirstOrCreate()
        {
            EditorWindow window = FindFirst();
            if (window != null)
                return window;
            if (type == null)
                return null;
            if (menuPath != null && menuPath.Length != 0)
                EditorApplication.ExecuteMenuItem(menuPath);
            window = EditorWindow.GetWindow(type, false, defaultTitle);
            return window;
        }
        // shortcut for setting/getting the position and size of the first window of this type.
        // when setting the position, if the window doesn't exist it will also be created.
        public Rect position
        {
            get
            {
                EditorWindow window = FindFirst();
                if (window == null)
                    return new Rect(0, 0, 0, 0);
                return window.position;
            }
            set
            {
                EditorWindow window = FindFirstOrCreate();
                if (window != null)
                    window.position = value;
            }
        }

        // shortcut for deciding if any windows of this type are open,
        // or for opening/closing windows
        public bool isOpen
        {
            get
            {
                return FindAll().Length != 0;
            }
            set
            {
                if (value)
                    FindFirstOrCreate();
                else
                    foreach (EditorWindow window in FindAll())
                        window.Close();
            }
        }
    }
    // experimental support for getting at the main Unity window
    public class MainWindow
    {
        Type type;
        UnityEngine.Object window;

        public MainWindow()
        {
            type = typeof(UnityEditor.EditorWindow).Assembly.GetType("UnityEditor.ContainerWindow");
            if (type != null)
            {
                foreach (UnityEngine.Object w in Resources.FindObjectsOfTypeAll(type))
                {
                    object parent = type.InvokeMember("get_mainView", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance, null, w, null);
                    if (parent != null && parent.GetType().Name.Contains("MainWindow"))
                    {
                        window = w;
                        break;
                    }
                }
            }
            if (window == null)
                Debug.LogWarning("Unable to find main window.\nMaybe you'll need to update the MainWindow constructor for your version of Unity.");
        }

        public Rect position
        {
            get
            {
                if (window == null)
                    return new Rect(0, 0, 0, 0);
                return (Rect)type.InvokeMember("get_position", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance, null, window, null);
            }
            set
            {
                if (window != null)
                    type.InvokeMember("set_position", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Instance, null, window, new object[] { value });
            }
        }
        public bool isOpen
        {
            get
            {
                return window != null;
            }
            set
            {
                if (!value)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorApplication.Exit(0);
                }
            }
        }
    }
}