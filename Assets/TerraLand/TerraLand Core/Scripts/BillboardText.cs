#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BillboardText : MonoBehaviour
{
    private Camera cameraToLookAt;

    private void Start()
    {
        if (Application.isEditor && !Application.isPlaying)
            cameraToLookAt = SceneView.lastActiveSceneView.camera;
        else
            cameraToLookAt = Camera.main;
    }

    void Update()
    {
        if (Application.isEditor && !Application.isPlaying)
            cameraToLookAt = SceneView.lastActiveSceneView.camera;

        if (cameraToLookAt == null)
            return;

        Vector3 v = cameraToLookAt.transform.position - transform.position;
        v.x = v.z = 0.0f;
        Vector3 targetPos = cameraToLookAt.transform.position - v;
        transform.LookAt(targetPos);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y - 180, transform.localEulerAngles.z);
    }
}
#endif

