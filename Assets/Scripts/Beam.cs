using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beam : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public Vector3 direction;

    public float MaxLength = 15.0f;

    Vector3 start_pos;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer.positionCount = 2;

        start_pos = transform.position;

        lineRenderer.SetPosition(0, start_pos);
        lineRenderer.SetPosition(1, start_pos);
    }

    // Update is called once per frame
    void Update()
    {
        float length = (lineRenderer.GetPosition(1) - start_pos).magnitude;

        length = Mathf.Min(length, MaxLength);

        lineRenderer.SetPosition(1, lineRenderer.GetPosition(1) + direction * 0.3f);

        lineRenderer.SetPosition(0, lineRenderer.GetPosition(1) - direction * length);
    }
}
