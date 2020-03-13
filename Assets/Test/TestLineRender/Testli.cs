using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testli : MonoBehaviour
{
    public LineRenderer _lineRenderer;
    public Transform transform1;
    public Transform transform2;
    public Transform transform3;
    public int layerOrder = 0;

    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer.positionCount = 3;
        _lineRenderer.sortingLayerID = layerOrder;
        _lineRenderer.SetPosition(0, transform1.position);
        _lineRenderer.SetPosition(1, transform2.position);
        _lineRenderer.SetPosition(2, transform3.position);
    }

    // Update is called once per frame
    void Update()
    {
        _lineRenderer.SetPosition(0, transform1.position);
        _lineRenderer.SetPosition(1, transform2.position);
        _lineRenderer.SetPosition(2, transform3.position);
    }
}
