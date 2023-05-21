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

    RaycastHit[] rayCastHit = new RaycastHit[8];

    // Update is called once per frame
    void Update()
    {
        const float speed = 0.3f;

        float length = (lineRenderer.GetPosition(1) - start_pos).magnitude;

        length = Mathf.Min(length, MaxLength);

        Ray ray = new Ray(lineRenderer.GetPosition(1), direction);

        int numhit = Physics.RaycastNonAlloc(ray, rayCastHit,speed,1 << 6);

        for (int i = 0; i < numhit; i++)
        {
            GameObject.Destroy(gameObject);
            return;
        }


        lineRenderer.SetPosition(1, lineRenderer.GetPosition(1) + direction * speed);

        lineRenderer.SetPosition(0, lineRenderer.GetPosition(1) - direction * length);

       
    }
}
