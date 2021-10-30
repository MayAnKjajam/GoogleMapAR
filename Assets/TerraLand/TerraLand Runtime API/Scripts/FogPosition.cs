using UnityEngine;
using System.Collections;

public class FogPosition : MonoBehaviour
{
	void LateUpdate ()
    {
        transform.position = Camera.main.transform.position;
	}
}

