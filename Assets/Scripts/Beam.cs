using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public Vector3 direction;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.positionCount = 2;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(1, lineRenderer.GetPosition(1) + direction * 0.5f);
    }
}
