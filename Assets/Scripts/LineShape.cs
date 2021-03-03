using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class LineShape : MonoBehaviour
{
    [SerializeField]
    private Color activeLineColor;
    [SerializeField]
    private Color drawedLineColor;
    [SerializeField]
    private float lineWidth = 1.0f;

    Vector3 choosenPos;
    Vector3 firstPos;
    Vector3 lastPos;
    Vector3 activePos;
    GameObject activeLine;

    private List<GameObject> lines = new List<GameObject>();

    bool hasShape = false;
    bool pointChoosen = false;
    bool drawedClockwise = false;
    int lineCount = 0;

    void Start()
    {

    }


    void Update()
    {
        DrawWithFreeLine();
    }

    private void DrawWithFreeLine()
    {
        SetActivePos();

        if (Input.GetMouseButtonDown(0))
        {
            if (pointChoosen == false)
            {

                choosenPos = GetMouseWorldPosition();
                firstPos = choosenPos;
                pointChoosen = true;
            }
            else if (pointChoosen == true)
            {
                if (hasShape == false)
                {
                    GameObject line = DrawLine(choosenPos, activePos, drawedLineColor, lineWidth);
                    line.transform.parent = GameObject.Find("LineHolder").transform;
                    lines.Add(line);
                    choosenPos = activePos;
                    lastPos = choosenPos;
                    if (choosenPos == firstPos)
                    {
                        //lines.ForEach( l => { Destroy(l); } );
                        pointChoosen = false;
                        lines.Clear();
                    }
                }
            }
        }
        if (pointChoosen == true)
        {
            GameObject activeLine = DrawLine(choosenPos, activePos, activeLineColor, lineWidth);
            activeLine.transform.parent = GameObject.Find("LineHolder").transform;
            GameObject.Destroy(activeLine, Time.deltaTime * 4.0f);
        }
    }

    private void SetActivePos()
    {
        if ((firstPos - GetMouseWorldPosition()).magnitude < 10.0f)
            activePos = firstPos;
        else
            activePos = GetMouseWorldPosition();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        vector.z = 0f;
        return vector;
    }

    private GameObject DrawLine(Vector3 start, Vector3 end, Color color, float width)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start + (end - start) * .5f;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.sortingLayerName = "Lines";
        return myLine;
    }

    Vector3 GetLineVectorFromEnd(int i)
    {
        LineRenderer lr;
        lr = lines[i].GetComponent<LineRenderer>();
        return lr.GetPosition(0) - lr.GetPosition(1);
    }

    Vector3 GetLineVectorFromStart(int i)
    {
        LineRenderer lr;
        lr = lines[i].GetComponent<LineRenderer>();
        return lr.GetPosition(1) - lr.GetPosition(0);
    }
}
