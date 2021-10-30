using UnityEngine;
using System.Collections;

public class DepthmapPass : MonoBehaviour
{
    public Camera cam;
    private Material material;

    void Awake ()
    {
        material = new Material(Shader.Find("Hidden/GlobalFog"));
    }

    void Start ()
    {
        cam.depthTextureMode = DepthTextureMode.Depth;
    }

    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }
}

