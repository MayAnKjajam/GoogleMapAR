using UnityEngine;
using UnityEditor;

public class MeshRendererShadows : EditorWindow
{
    [UnityEditor.MenuItem("Tools/TerraUnity/Common/Mesh Renderer Settings", false, 2)]
    public static void Init()
    {
        MeshRendererShadows window = (MeshRendererShadows)EditorWindow.GetWindow(typeof(MeshRendererShadows));
        window.position = new Rect(5, 135, 400, 600);
    }

    GameObject target;
    bool receiveShadows = true;
    Component[] meshRenderers;
    string[] castingMode = new string[] { "OFF", "ON", "TWO SIDED", "Shadow ONLY" };
    int castingIndex = 1;

    public void OnGUI ()
    {
        GUILayout.Space(60);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("GAMEOBJECT WITH RENDERERS", MessageType.None, true);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        target = EditorGUILayout.ObjectField(target, typeof(GameObject), true) as GameObject;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(40);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("CAST SHADOWS", MessageType.None, true);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = UnityEngine.Color.white;

        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        castingIndex = GUILayout.SelectionGrid(castingIndex, castingMode, 4);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUI.backgroundColor = UnityEngine.Color.white;

        GUILayout.Space(40);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("RECEIVE SHADOWS", MessageType.None, true);
        receiveShadows = EditorGUILayout.Toggle(receiveShadows);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(40);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("\nSET RENDERERS\n"))
            SetRenderers();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void SetRenderers ()
    {
        meshRenderers = target.transform.GetComponentsInChildren(typeof(MeshRenderer));

        if(meshRenderers != null)
        {
            foreach(MeshRenderer meshRenderer in meshRenderers)
            {
                if(castingIndex == 0)
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                else if(castingIndex == 1)
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                else if(castingIndex == 2)
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
                else if(castingIndex == 3)
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                meshRenderer.receiveShadows = receiveShadows;
            }
        }
        else
        {
            // Try again, looking for inactive GameObjects
            Component[] meshRenderersInactive = target.transform.GetComponentsInChildren(typeof(MeshRenderer), true);

            foreach(MeshRenderer meshRenderer in meshRenderersInactive)
            {
                if(castingIndex == 0)
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                else if(castingIndex == 1)
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                else if(castingIndex == 2)
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
                else if(castingIndex == 3)
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

                meshRenderer.receiveShadows = receiveShadows;
            }
        }
    }
}

