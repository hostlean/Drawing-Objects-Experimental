using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTextture2D : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Texture2D texture = new Texture2D(1, 1);
        GetComponent<LineRenderer>().material.mainTexture = texture;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                Color color = ((x & y) != 0 ? Color.white : Color.gray);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
