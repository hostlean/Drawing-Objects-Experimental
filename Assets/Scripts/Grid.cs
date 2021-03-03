using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Grid 
{
    const int sortingOrderDefault = 5000;
        
    private int width;
    private int height;
    private float cellSize;

    private List<Vector3> gridPoints;
    //this is how we define multi dimensional array with two dimensions
    private int[,] gridArray;
    private TextMesh[,] debugTextArray;
    public Grid(int width, int height, float cellSize, int startGridPosX, int startGridPosY, Color gridLineColor, GameObject parent)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;

        gridArray = new int[width, height];
        debugTextArray = new TextMesh[width, height];
        gridPoints = new List<Vector3>();


        for(int x = startGridPosX; x < gridArray.GetLength(0); x++)
        {
            for(int y = startGridPosY; y < gridArray.GetLength(1); y++)
            {
                //debugTextArray[x, y] = CreateWorldText(gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f , 20, Color.white, TextAnchor.MiddleCenter);
                //Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100.0f);
                //Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100.0f);
                DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), gridLineColor, .7f, parent.transform);
                DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), gridLineColor, .7f, parent.transform);
                gridPoints.Add(new Vector3(x * cellSize, y * cellSize));

            }

            //Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100.0f);
            //Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100.0f);
            DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), gridLineColor, .7f, parent.transform);
            DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), gridLineColor, .7f, parent.transform);
        }

    }

    public List<Vector3> GetGridPoints()
    {
        return gridPoints;
    }
    private Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }

    public void SetValue(int x, int y, int value)
    {
        if(x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            //debugTextArray[x, y].text = gridArray[x, y].ToString();
        }                 
    }

    public void SetValue(Vector3 worldPos, int value)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        SetValue(x, y, value);
    }

    public int GetValue(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
            return 0;
    }

    public int GetValue(Vector3 worldPos)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        return GetValue(x, y);
    }

    private void GetXY(Vector3 worldPos, out int x, out int y)
    {
        x = Mathf.FloorToInt(worldPos.x / cellSize);
        y = Mathf.FloorToInt(worldPos.y / cellSize);
    }


    public Vector3 SnapToGridPoint(Vector3 worldPos)
    {
        return new Vector3(Mathf.Round(worldPos.x / cellSize) * cellSize, Mathf.Round(worldPos.y / cellSize) * cellSize);
    }

    GameObject DrawLine(Vector3 start, Vector3 end, Color color, float width, Transform parent)
    {
        GameObject myLine = new GameObject();
        myLine.transform.parent = parent;
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Unlit/Color"));
        lr.material.color = color;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.sortingLayerName = "Grid";
        return myLine;
    }

    TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

    TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = sortingOrderDefault)
    {
        if (color == null) color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
    }
}
