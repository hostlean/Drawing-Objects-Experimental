using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GenerateCircleMesh : MonoBehaviour
{
    [Header("Circle Properties")]
    private float perimeter = 360f;
    [Range((int)3f, (int)180f)]
    [SerializeField]
    int lineCount = 2;
    [SerializeField]
    float radius = 20f;

    [Header("Mesh Properties")]
    [SerializeField]
    private Material material;
    [SerializeField]
    private Color color;

    Mesh mesh;
    Mesh filterMesh;
    Vector3[] vertices;
    Vector2[] uv;
    Vector3[] normals;
    int[] triangles;

    private MeshRenderer meshRenderer;

    void Start()
    {
        mesh = new Mesh();
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        CreateCircle();
        UpdateMesh(mesh);
    }


    void Update()
    {
        filterMesh = GetComponent<MeshFilter>().mesh;
        vertices = filterMesh.vertices;
        normals = filterMesh.normals;

        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] += normals[i] * Mathf.Sin(Time.time);
        }

        filterMesh.vertices = vertices;
    }

    public void CreateCircle()
    { 

        Vector3 origin = Vector3.zero;

        float angle = 0f;
        float angleIncrease = perimeter / lineCount;


        GetComponent<MeshFilter>().mesh = mesh;
        filterMesh = GetComponent<MeshFilter>().mesh;
        meshRenderer.material = material;
        meshRenderer.material.color = color;

        vertices = new Vector3[lineCount + 1 + 1];

        uv = new Vector2[vertices.Length];

        triangles = new int[lineCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;

        int triangleIndex = 0;

        for (int i = 0; i <= lineCount; i++)
        {
            Vector3 vertex = origin + GetVectorFromAngle(angle) * radius;
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }
            vertexIndex++;
            angle -= angleIncrease;
            normals = filterMesh.normals;
        }


    }

    void UpdateMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.normals = normals;
    }

    private Vector3 GetVectorFromAngle(float angle)
    {
        //angle 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }


}
