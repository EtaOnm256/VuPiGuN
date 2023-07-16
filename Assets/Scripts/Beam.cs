using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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

    public GameObject hitEffect_prefab;

    GameObject[] hitHistory = new GameObject[8];
    RobotController[] hitHistoryRC = new RobotController[8];

    int hitHistoryCount = 0;
    int hitHistoryRCCount = 0;

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
            if (hitHistory.Contains(rayCastHit[i].collider.gameObject))
                continue;

            hitHistory[hitHistoryCount++] = rayCastHit[i].collider.gameObject;

           

            RobotController robotController = rayCastHit[i].collider.gameObject.GetComponentInParent<RobotController>();

            if (robotController != null)
            {
                if (hitHistoryRC.Contains(robotController))
                    continue;

                hitHistoryRC[hitHistoryRCCount++] = robotController;

                robotController.DoDamage(direction,250,false);

                GameObject.Instantiate(hitEffect_prefab, rayCastHit[i].point, Quaternion.identity);
            }

     
        }


        lineRenderer.SetPosition(1, lineRenderer.GetPosition(1) + direction * speed);

        lineRenderer.SetPosition(0, lineRenderer.GetPosition(1) - direction * length);

       
    }
}
