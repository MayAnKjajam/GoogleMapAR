using UnityEngine;
using System.Collections;

public class CameraDepthTexture : MonoBehaviour
{
    void Start ()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }
}

