using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{

    private void Start()
    {
        Debug.Log(this.transform.position);
        Debug.Log(this.transform.localPosition);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
