using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetYaxisasZero : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetAtZero());
    }

    IEnumerator SetAtZero()
    {
        yield return new WaitForSeconds(3f);
        int count = this.transform.childCount;
        Debug.Log(count);
        for (int i = 0; i < count; i++)
        {
            transform.GetChild(i).transform.position = new Vector3(transform.GetChild(i).transform.position.x, 0, transform.GetChild(i).transform.position.z);
        }
    }    
    
    // Update is called once per frame
    void Update()
    {
      
    }
}
