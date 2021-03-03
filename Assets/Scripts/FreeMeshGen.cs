using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FreeMeshGen : MonoBehaviour
{
    private static FreeMeshGen _instance;
    public static FreeMeshGen Instance
    {
        get
        {
            if(_instance == null)
                Debug.LogError("Free Mesh Gen is NULL!");
            return _instance;
        }
    }

    [SerializeField]
    private Material material;

    Vector3 choosenGridPos;
    Vector3 firstGridPos;
    Vector3 lastGridPos;
    Vector3 activePos;
    GameObject activeLine;

    private List<Vector3> vertices = new List<Vector3>();

    private List<int> triangles = new List<int>();

    private List<GameObject> lines = new List<GameObject>();

    bool hasShape = false;
    bool pointChoosen = false;
    bool drawedClockwise = false;
    int lineCount = 0;
    int fieldCount = 0;
    int sortOrder = 0;

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        
    }

    private void DrawWithFreeLine()
    {
        //activePos = GridField.Instance.grid.SnapToGridPoint(GetMouseWorldPosition());
        activePos = GetMouseWorldPosition();

        if (Input.GetMouseButtonDown(0))
        {
            if(pointChoosen == false)
            {
                //choosenGridPos = GridField.Instance.grid.SnapToGridPoint(GetMouseWorldPosition());
                
                choosenGridPos = GetMouseWorldPosition();
                firstGridPos = choosenGridPos;
                vertices.Add(choosenGridPos);
                triangles.Add(0);
                pointChoosen = true;
            }
            else if(pointChoosen == true)
            {
                if(hasShape == false)
                {

                    if ((activePos - lines[0].GetComponent<LineRenderer>().GetPosition(0)).magnitude < 10.0f)
                        choosenGridPos = lines[0].GetComponent<LineRenderer>().GetPosition(0);
                    else
                        choosenGridPos = activePos;
                    lastGridPos = choosenGridPos;
                    vertices.Add(lastGridPos);
                    if(choosenGridPos == firstGridPos)
                    {
                        vertices.Remove(lastGridPos);
                        //lines.ForEach( l => { Destroy(l); } );
                        pointChoosen = false;
                        vertices.Clear();
                        triangles.Clear();
                        lines.Clear();
                    }
                }    
            }

        }
    }

    private void CreateComplexShape(List<Vector3> vertices, Color color)
    {   

        GameObject obj = new GameObject("Field " + fieldCount);
        Mesh mesh = new Mesh();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<Shape>();
        obj.layer = 8;
        obj.tag = "Shape";
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        obj.GetComponent<MeshFilter>().mesh = mesh;
        renderer.material = material;
        renderer.material.color = color;
        renderer.sortingLayerName = "Fields";
        renderer.sortingOrder = sortOrder;
        //CreateShape(vertices);
        UpdateMesh(mesh);
        fieldCount++;
        sortOrder++;
    }

    //void CreateShape(List<Vector3> vertices)
    //{

    //    for (int i = 1; i < vertices.Count; i++)
    //    {
    //        if (i <= 2)
    //            triangles.Add(i);
    //        if (i >= 3)
    //        {
    //            if (DotProduct(GetLineVectorFromEnd(i - 1), GetLineVectorFromStart(i)) <= 0)
    //            {
    //                triangles.Add(i - 1);
    //                triangles.Add(i);
    //                if (vertices.Count >= i + 1)
    //                {
    //                    if (vertices[i] == vertices[0])
    //                        triangles.Add(0);
    //                    else
    //                        triangles.Add(i + 1);
    //                }

    //            }

    //            // else if(DotProduct(GetLineVectorFromEnd(i - 1), GetLineVectorFromStart(i)) > 0)
    //            // {
    //            //     triangles.Add(i -1);
    //            //     triangles.Add(i);
    //            //     triangles.Add(i + 1);
    //            // }

    //        }

    //    }
    //    Debug.Log(triangles.Count);
    //}

    void UpdateMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        vector.z = 0f;
        return vector;
    }



    private float DotProduct(Vector3 vector1, Vector3 vector2)
    {
        return (vector1.x * vector2.x + vector1.y * vector2.y);
    }

    // private Vector3 CenterOfMass()
    // {

    // }
}
