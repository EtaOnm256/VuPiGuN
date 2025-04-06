using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Beam : Projectile
{


    public LineRenderer lineRenderer;

    public Quaternion initial_direction;



    public float MaxLength = 15.0f;

    Vector3 start_pos;

    [SerializeField]int damage = 100;

    // Start is called before the first frame update
    protected override void OnStart()
    {
        lineRenderer.positionCount = 2;

        position = start_pos = transform.position;

        lineRenderer.SetPosition(0, start_pos);
        lineRenderer.SetPosition(1, start_pos);

        initial_direction = Quaternion.LookRotation(direction);

        if (chargeshot)
        {
            speed *= 1.3f;
            damage = (int)(damage*1.5f);
        }
    }

    RaycastHit[] rayCastHit = new RaycastHit[8];

    public GameObject hitEffect_prefab;

    GameObject[] hitHistory = new GameObject[8];
    RobotController[] hitHistoryRC = new RobotController[8];

    int hitHistoryCount = 0;
    int hitHistoryRCCount = 0;

	int time = 120;

    public bool chargeshot = false;

    [SerializeField] float homing_strength = 1.0f;
    [SerializeField] float homing_limit = 10.0f;

    [SerializeField] float hitsphere_width = -1.0f;
    [SerializeField] float hiteffect_scale = 1.0f;

    [SerializeField] RobotController.KnockBackType KnockBackType = RobotController.KnockBackType.Normal;

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {


        if (!dead)
        {

            



            if (target != null)
            {
                Quaternion qDirection = Quaternion.LookRotation(direction, Vector3.up);

                Quaternion qTarget = Quaternion.LookRotation(target.GetCenter() - lineRenderer.GetPosition(1));

                if (Quaternion.Angle(qDirection, qTarget) < 90.0f)
                {
                    Quaternion qDirection_new = Quaternion.RotateTowards(qDirection, qTarget, homing_strength);

                    Quaternion qDirection_result = Quaternion.RotateTowards(initial_direction, qDirection_new, homing_limit);

                    direction = qDirection_result * Vector3.forward;
                }
            }

            int numhit = 0;

            if (hitsphere_width <= 0.0f)
            {
                Ray ray = new Ray(lineRenderer.GetPosition(1), direction);
                numhit = Physics.RaycastNonAlloc(ray, rayCastHit, speed, 1 << 6 | 1 << 3);
            }
            else
            {
                numhit = Physics.SphereCastNonAlloc(lineRenderer.GetPosition(1), hitsphere_width,direction,rayCastHit, speed, 1 << 6 | 1 << 3);
            }

            for (int i = 0; i < numhit; i++)
            {
                if (hitHistory.Contains(rayCastHit[i].collider.gameObject))
                    continue;

                if (hitHistoryCount >= hitHistory.Length)
                    break;

                hitHistory[hitHistoryCount++] = rayCastHit[i].collider.gameObject;



                RobotController robotController = rayCastHit[i].collider.gameObject.GetComponentInParent<RobotController>();

                if (robotController != null)
                {
                    if (hitHistoryRC.Contains(robotController))
                        continue;

                    if (hitHistoryRCCount >= hitHistoryRC.Length)
                        break;

                    hitHistoryRC[hitHistoryRCCount++] = robotController;

                    robotController.TakeDamage(rayCastHit[i].point,direction, damage, KnockBackType, owner);

                    
                }
                else
                {
                    dead = true;
                }

                GameObject hitEffect_obj = GameObject.Instantiate(hitEffect_prefab, rayCastHit[i].point, Quaternion.identity);
                hitEffect_obj.transform.localScale = new Vector3(hiteffect_scale, hiteffect_scale, hiteffect_scale);
            }

            position = lineRenderer.GetPosition(1) + direction * speed;

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
