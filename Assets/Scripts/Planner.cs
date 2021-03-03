using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planner : MonoBehaviour
{
    private Ray _ray;
    private RaycastHit hitInfo;
    [SerializeField]
    private LayerMask _targetLayer;
    [SerializeField]
    private GameObject _squarePrefab;
    void Start()
    {
        
    }


    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            AddSquare();
        }
    }

    public void AddSquare()
    {
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out hitInfo, 100.0f, _targetLayer))
        {
            GameObject square = Instantiate(_squarePrefab, GameObject.FindWithTag("SquareHolder").transform);
            square.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y, -1.0f);
        }
    }
}
