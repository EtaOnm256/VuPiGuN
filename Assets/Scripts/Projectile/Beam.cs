using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Beam : MonoBehaviour
{
    public RobotController target = null;

    public LineRenderer lineRenderer;

    public Quaternion initial_direction;

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

        initial_direction = Quaternion.LookRotation(direction);
    }

    RaycastHit[] rayCastHit = new RaycastHit[8];

    public GameObject hitEffect_prefab;

    GameObject[] hitHistory = new GameObject[8];
    RobotController[] hitHistoryRC = new RobotController[8];

    int hitHistoryCount = 0;
    int hitHistoryRCCount = 0;

	int time = 120;

    bool dead = false;

    // Update is called once per frame
    void FixedUpdate()
    {
        const float speed = 1.6f;

        if (!dead)
        {

            



            if (target != null)
            {
                Quaternion qDirection = Quaternion.LookRotation(direction, Vector3.up);

                Quaternion qTarget = Quaternion.LookRotation(target.GetCenter() - lineRenderer.GetPosition(1));

                if (Quaternion.Angle(qDirection, qTarget) < 90.0f)
                {
                    Quaternion qDirection_new = Quaternion.RotateTowards(qDirection, qTarget, 1.0f);

                    Quaternion qDirection_result = Quaternion.RotateTowards(initial_direction, qDirection_new, 10.0f);

                    direction = qDirection_result * Vector3.forward;
                }
            }

            Ray ray = new Ray(lineRenderer.GetPosition(1), direction);

            int numhit = Physics.RaycastNonAlloc(ray, rayCastHit, speed, 1 << 6 | 1 << 3);

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

                    robotController.DoDamage(direction, 100, false);

                    
                }
                else
                {
                    dead = true;
                }

                GameObject.Instantiate(hitEffect_prefab, rayCastHit[i].point, Quaternion.identity);
            }



            lineRenderer.SetPosition(1, lineRenderer.GetPosition(1) + direction * speed);

            float length = (lineRenderer.GetPosition(1) - start_pos).magnitude;

            length = Mathf.Min(length, MaxLength);

            Vector3 view_dir = (lineRenderer.GetPosition(1) - start_pos).normalized;

            lineRenderer.SetPosition(0, lineRenderer.GetPosition(1) - view_dir * length);


            if (time-- <= 0)
            {
                dead = true;
            }
        }
        else
        {
            Vector3 view_dir = (lineRenderer.GetPosition(1) - start_pos).normalized;

            if ((lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)).magnitude <= speed)
            {
                GameObject.Destroy(gameObject);
            }
            else
            {
                lineRenderer.SetPosition(0, lineRenderer.GetPosition(0) + view_dir * speed);
            }

            
        }
    }
}