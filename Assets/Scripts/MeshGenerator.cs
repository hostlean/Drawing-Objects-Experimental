using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class MeshGenerator : MonoBehaviour
{
    private static MeshGenerator _instance;
    public static MeshGenerator Instance
    {
        get
        {
            if(_instance == null)
                Debug.LogError("Mesh Generator is NULL!");
            return _instance;
        }
    }
    
    [SerializeField]
    private Color drawLineColor;

    public Text line1Text, line2Text, areaText;

    public Material material;
    public LayerMask _targetLayerForShape;

    public List<GameObject> objs = new List<GameObject>();

    public List<GameObject> lines = new List<GameObject>();
    Ray[] rays = new Ray[9];
    Vector3[] vertices;
    RaycastHit hitInfo, hit;
    Vector3 vertex0, vertex1, vertex2, vertex3;
    GameObject line0, line1, line2, line3;
    GameObject shapeHolder;
    int[] triangles;

    int fieldCount = 0;

    int sortOrder = 0;

    bool drawClockwise = false;
    bool onDraw = false;

    public bool shapeSelected = false;
    public bool onShape = false;
    public bool lineOnTop = false;
    public bool clicked = false;


    private void Awake()
    {
        _instance = this;
        shapeHolder = new GameObject("Shape_Holder");
        shapeHolder.transform.position = Vector3.zero;
    }

    void Start()
    {   
        line1Text.enabled = false;
        line2Text.enabled = false;
        areaText.enabled = false;
    }

    private void Update()
    {   
        LineDetectionLogic();

        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 100.0f, _targetLayerForShape))
        {
            if(hitInfo.collider.gameObject.tag == "Shape")
            {
                onShape = true;
            }
            else {onShape = false;}
        }

        if(onShape == false)
            clicked = false;

        if(Input.GetMouseButtonDown(0))
        {
            if(onShape == true)
            {
                clicked = !clicked;
                if(clicked == true)
                {
                    shapeSelected = true;               
                }
                if(shapeSelected == true)
                {
                    if(clicked == false)
                    {
                        shapeSelected = false;
                        if(hitInfo.collider.gameObject.tag == "Shape")
                            hitInfo.collider.gameObject.transform.position = hitInfo.collider.gameObject.transform.position;                          
                    }                      
                }               
            }
            if(!onDraw && !onShape)
            {
                vertex0 = GridField.Instance.grid.SnapToGridPoint(GetMouseWorldPosition());

                onDraw = true;

            }

            else if(onDraw && !onShape)
            {
                vertex3 = GridField.Instance.grid.SnapToGridPoint(GetMouseWorldPosition());
                vertex1 = new Vector3(vertex0.x, vertex3.y);
                vertex2 = new Vector3(vertex3.x, vertex0.y);

                if(lineOnTop == false)
                    CreateNewShape(material, new Color(Random.value, Random.value, Random.value));
                
                onDraw = false;
                line1Text.enabled = false;
                line2Text.enabled = false;
                areaText.enabled = false;
            }                
        }
        
        if(shapeSelected == true)
            if(hitInfo.collider.gameObject.tag == "Shape")
                hitInfo.collider.gameObject.transform.position = GetMouseWorldPosition();  

        if(onDraw && !onShape)
        {
            line1Text.enabled = true;
            line2Text.enabled = true;
            areaText.enabled = true;
            DrawGuideLines();
        }

        if(Input.GetMouseButtonDown(1))
        {
            onDraw = false;
            line1Text.enabled = false;
            line2Text.enabled = false;
            areaText.enabled = false;
        }            
    }
    private void LineDetectionLogic()
    {
        Vector3 forward = Vector3.forward;
        foreach(var line in lines)
        {
            LineRenderer lr = line.GetComponent<LineRenderer>();
            var startPos = lr.GetPosition(0);
            var endPos = lr.GetPosition(1);

            Vector3 direction = endPos - startPos;

            if(Mathf.Sign(direction.x) > 0)
            {
                for(int x = (int)startPos.x; x < (int)endPos.x; x += (int)GridField.Instance.cellSize)
                if(Physics.Raycast(new Vector3(x, line.transform.position.y, line.transform.position.z), forward, out hit, 100.0f, _targetLayerForShape))
                    if(hit.collider.gameObject.tag == "Shape")
                    {
                        lineOnTop = true;
                        return;
                    }
                        
                    else lineOnTop = false;
            } 
            else if(Mathf.Sign(direction.x) < 0)
            {
                for(int x = (int)startPos.x; x > (int)endPos.x; x -= (int)GridField.Instance.cellSize)
                if(Physics.Raycast(new Vector3(x, line.transform.position.y, line.transform.position.z), forward, out hit, 100.0f, _targetLayerForShape))
                    if(hit.collider.gameObject.tag == "Shape")
                    {
                        lineOnTop = true;
                        return;
                    }
                    else lineOnTop = false;
            }
            if(Mathf.Sign(direction.y) > 0)
            {
                for(int y = (int)startPos.y; y < (int)endPos.y; y += (int)GridField.Instance.cellSize)
                if(Physics.Raycast(new Vector3(line.transform.position.x, y, line.transform.position.z), forward, out hit, 100.0f, _targetLayerForShape))
                    if(hit.collider.gameObject.tag == "Shape")
                    {
                        lineOnTop = true;
                        return;
                    }
                    else lineOnTop = false;
            }
            else if(Mathf.Sign(direction.y) < 0)
            {
                for(int y = (int)startPos.y; y > (int)endPos.y; y -= (int)GridField.Instance.cellSize)
                if(Physics.Raycast(new Vector3(line.transform.position.x, y, line.transform.position.z), forward, out hit, 100.0f, _targetLayerForShape))
                    if(hit.collider.gameObject.tag == "Shape")
                    {
                        lineOnTop = true;
                        return;
                    }
                    else lineOnTop = false;
            }

        }
    }

    private void CreateNewShape(Material material, Color color)
    {
        GameObject obj = new GameObject("Field " + fieldCount);
        Mesh mesh = new Mesh();
        obj.AddComponent<MeshFilter>();
        obj.AddComponent<MeshRenderer>();
        obj.AddComponent<Shape>();
        obj.AddComponent<BoxCollider>();
        obj.transform.parent = shapeHolder.transform;
        obj.layer = 8;
        obj.tag = "Shape";
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        BoxCollider collider = obj.GetComponent<BoxCollider>();
        Vector3 center =  vertex0 + (vertex3 - vertex0) * .5f;
        collider.center = center;       
        collider.size = new Vector3(Vector3.Distance(vertex1, vertex3), Vector3.Distance(vertex0, vertex1), 1);
        obj.GetComponent<MeshFilter>().mesh = mesh;
        renderer.material = material;
        renderer.material.color = color;
        renderer.sortingLayerName = "Fields";
        renderer.sortingOrder = sortOrder;
        CreateShape();
        UpdateMesh(mesh);
        obj.transform.position = obj.transform.TransformPoint(obj.transform.position);
        objs.Add(obj);
        fieldCount++;
        sortOrder++;
    }

    private void DrawGuideLines()
    {
        vertex3 = GridField.Instance.grid.SnapToGridPoint(GetMouseWorldPosition());
        vertex1 = new Vector3(vertex0.x, vertex3.y, -2);
        vertex2 = new Vector3(vertex3.x, vertex0.y, -2);

        line0 = DrawLine(vertex0, vertex1, drawLineColor, 1.0f);
        line1 = DrawLine(vertex2, vertex0, drawLineColor, 1.0f);
        line2 = DrawLine(vertex2, vertex3, drawLineColor, 1.0f);
        line3 = DrawLine(vertex3, vertex1, drawLineColor, 1.0f);

        ShowCalculatedFieldArea(line0, line1, line1Text, line2Text, areaText);

        Destroy(line0, Time.deltaTime * 2);
        Destroy(line1, Time.deltaTime * 2);
        Destroy(line2, Time.deltaTime * 2);
        Destroy(line3, Time.deltaTime * 2);
    }

    private void ShowCalculatedFieldArea(GameObject line1, GameObject line2, Text line1Text, Text line2Text, Text fieldArea)
    {
        Vector3 dot1 = line1.GetComponent<LineRenderer>().GetPosition(0);
        Vector3 dot2 = line1.GetComponent<LineRenderer>().GetPosition(1);
        Vector3 dot3 = line2.GetComponent<LineRenderer>().GetPosition(0);
        Vector3 dot4 = line2.GetComponent<LineRenderer>().GetPosition(1);

        Vector3 line1Vector = dot2 - dot1;
        Vector3 line2Vector = dot3 - dot4;

        float dist2 = line2Vector.magnitude;
        float dist1 = line1Vector.magnitude;
        float area = dist1 * dist2;
        dist1 = Mathf.Round(dist1);
        dist2 = Mathf.Round(dist2);
        area = Mathf.Round(area);

        line1Text.text = Mathf.Round(dist1 * 10) / 10 + "";
        line2Text.text = Mathf.Round(dist2 * 10) / 10 + "";
        fieldArea.text = Mathf.Round(area / 100) * 100 + "";
        

        Vector3 middle = new Vector3(dot4.x - dot1.x, dot4.y - dot1.y, dot4.z - dot1.z);

        line1Text.rectTransform.position = GetMouseWorldPosition() - new Vector3(line2Vector.x, line1Vector.y * .5f, 0);
        line2Text.rectTransform.position = GetMouseWorldPosition() - new Vector3(line2Vector.x * .5f, line1Vector.y, 0);
        fieldArea.rectTransform.position = GetMouseWorldPosition() - new Vector3(line2Vector.x * .5f, line1Vector.y * .5f, 0);      
    }

    void CreateShape()
    {
        vertices = new Vector3[]
        {
            vertex0,
            vertex1,
            vertex2,
            vertex3
        };

        Vector3 direction = vertex3 - vertex0;

        if(Mathf.Sign(direction.x) > 0 && Mathf.Sign(direction.y) > 0 ||
           Mathf.Sign(direction.x) < 0 && Mathf.Sign(direction.y) < 0)
        {
            drawClockwise = true;
            triangles = new int[]
            {
                0, 1, 2,
                2, 1, 3
            };
        }
        else if(Mathf.Sign(direction.x) < 0 && Mathf.Sign(direction.y) > 0 ||
                Mathf.Sign(direction.x) > 0 && Mathf.Sign(direction.y) < 0)
        {
            drawClockwise = false;
            triangles = new int[]
            {
                0, 2, 1,
                1, 2, 3
            };
        }
    }

    void UpdateMesh(Mesh mesh)
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        vector.z = 0f;
        return vector;
    }

    GameObject DrawLine(Vector3 start, Vector3 end, Color color, float width)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start + (end - start) * .5f;
        myLine.AddComponent<LineRenderer>();
        myLine.AddComponent<LineBehaviour>();

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
}
