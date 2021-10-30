using UnityEngine;
using System.Collections;

public class SwitchShadowProjector : MonoBehaviour
{
    private Projector shadowProjector;

    void Start ()
    {
        shadowProjector = transform.GetComponent<Projector>();
    }

	void Update ()
    {
        if(ShadowCheck.isInShadow)
            shadowProjector.enabled = false;
        else
            shadowProjector.enabled = true;
	}
}

