using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridField : MonoBehaviour
{
    private static GridField _instance;
    public static GridField Instance
    {
        get
        {
            if(_instance == null)
                Debug.LogError("Grid Field is NULL!");
            return _instance;

        }
    }
    public Grid grid;
    [SerializeField]
    private Color _gridLineColor;
    [SerializeField]
    private int width, height;
    public float cellSize;
    [SerializeField]
    private int startGridPosX, startGridPosY;
    private GameObject _gridLines;
    private GameObject _pointHolder;

    public List<Vector3> gridPoints;

    [SerializeField]
    private GameObject dotPrefab;

    private List<GameObject> dots = new List<GameObject>();



    private void Awake()
    {
        _instance = this;  
    }

    void Start()
    {
        _gridLines = new GameObject("Grid_Lines");
        _pointHolder = new GameObject("Point Holder");
        grid = new Grid(width, height, cellSize, startGridPosX, startGridPosY, _gridLineColor, _gridLines);
        gridPoints = grid.GetGridPoints();
        foreach(var point in gridPoints)
        {
            GameObject obj = Instantiate(dotPrefab, point, Quaternion.identity, _pointHolder.transform);
            obj.GetComponent<SpriteRenderer>().enabled = false;
            dots.Add(obj);
        }

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            foreach(var obj in dots)
            {
                if(obj.GetComponent<SpriteRenderer>().enabled == false)
                    obj.GetComponent<SpriteRenderer>().enabled = true;
            }
        }
        if(Input.GetKeyUp(KeyCode.LeftAlt))
        {
            foreach(var obj in dots)
            {
                if(obj.GetComponent<SpriteRenderer>().enabled == true)
                    obj.GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }
    Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
    {
        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }
}
