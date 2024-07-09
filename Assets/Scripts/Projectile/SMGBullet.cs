using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class SMGBullet : Projectile
{
    public LineRenderer lineRenderer;

    public Quaternion initial_direction;

    public float MaxLength = 3.0f;

    Vector3 start_pos;

    const int positionCount = 4;


    // Start is called before the first frame update
    protected override void OnStart()
    {
        //lineRenderer.positionCount = positionCount;

        start_pos = transform.position;

        for (int i = 0; i < positionCount; i++)
        {
            lineRenderer.SetPosition(i, start_pos);
        }

        initial_direction = Quaternion.LookRotation(direction);

        speed = 1.2f;
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


        if (!dead)
        {

            lineRenderer.SetPosition(positionCount - 1, lineRenderer.GetPosition(positionCount - 1) + direction * speed);

            float length = (lineRenderer.GetPosition(positionCount - 1) - start_pos).magnitude;

            length = Mathf.Min(length, MaxLength);

            Vector3 view_dir = (lineRenderer.GetPosition(positionCount - 1) - start_pos).normalized;

            lineRenderer.SetPosition(0, lineRenderer.GetPosition(positionCount - 1) - view_dir * length);



            if (target != null)
            {
                Quaternion qDirection = Quaternion.LookRotation(direction, Vector3.up);

                Quaternion qTarget = Quaternion.LookRotation(target.GetCenter() - lineRenderer.GetPosition(1));

                if (Quaternion.Angle(qDirection, qTarget) < 90.0f)
                {
                    Quaternion qDirection_new = Quaternion.RotateTowards(qDirection, qTarget, 0.30f);

                    Quaternion qDirection_result = Quaternion.RotateTowards(initial_direction, qDirection_new, 3.0f);

                    direction = qDirection_result * Vector3.forward;
                }
            }

            Ray ray = new Ray(lineRenderer.GetPosition(positionCount - 1), direction);

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

                    robotController.DoDamage(direction, 10, RobotController.KnockBackType.None);

                    dead = true;
                }
                else
                {
                    dead = true;
                }

                GameObject hitEffect = GameObject.Instantiate(hitEffect_prefab, rayCastHit[i].point, Quaternion.LookRotation(view_dir,Vector3.up));

                hitEffect.transform.localScale = Vector3.one * 0.5f;
            }



         


            if (time-- <= 0)
            {
                dead = true;
            }
        }
        else
        {
            Vector3 view_dir = (lineRenderer.GetPosition(positionCount - 1) - start_pos).normalized;

            if ((lineRenderer.GetPosition(positionCount - 1) - lineRenderer.GetPosition(0)).magnitude <= speed)
            {
                GameObject.Destroy(gameObject);
            }
            else
            {
                lineRenderer.SetPosition(0, lineRenderer.GetPosition(0) + view_dir * speed);
            }

            
        }

        for(int i=1;i< positionCount - 1;i++)
        {
            lineRenderer.SetPosition(i, lineRenderer.GetPosition(0)+ (lineRenderer.GetPosition(positionCount - 1)- lineRenderer.GetPosition(0))*i/(positionCount-i));
        }
    }
}
