using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ShadowCheck : MonoBehaviour
{
    public Light sun;
    public LayerMask checkingLayer;
    public Vector3 originOffset = Vector3.zero;
    public bool debug = false;

    private Light sunLight;
    private Ray ray;
    private RaycastHit hit;
    private Vector3 origin;
    private Vector3 direction;

    public static bool isInShadow;

    void Start ()
    {
        sunLight = sun.GetComponent<Light>();
    }

    void Update ()
    {
        CheckShadow();
    }

    private void CheckShadow ()
    {
        origin = transform.position + originOffset;
        direction = sun.transform.position - origin;
        ray = new Ray(origin, direction);

        if(Physics.Raycast(ray, out hit, Mathf.Infinity, checkingLayer))
        {
            isInShadow = true;
            
            if(debug)
                Debug.DrawLine(ray.origin, hit.point, UnityEngine.Color.red);
        }
        else
        {
            isInShadow = false;

            if(debug)
                Debug.DrawRay(origin, direction, UnityEngine.Color.green);
        }

        if(isInShadow)
            sunLight.enabled = false;
        else
            sunLight.enabled = true;
    }
}

