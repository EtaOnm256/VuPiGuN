using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class CannonBall : Projectile
{
    public Quaternion initial_direction;

    Vector3 start_pos;

    static public float Speed = 2.0f;

    // Start is called before the first frame update
    protected override void OnStart()
    {
     

        position = start_pos = transform.position;

        initial_direction = Quaternion.LookRotation(direction);

        speed = Speed;

        if (chargeshot)
        {
            speed *= 1.3f;
            damage = (int)(damage * 1.5f);
        }
    }

    RaycastHit[] rayCastHit = new RaycastHit[8];

    public GameObject hitEffect_prefab;

    GameObject[] hitHistory = new GameObject[8];
    RobotController[] hitHistoryRC = new RobotController[8];

    int hitHistoryCount = 0;
    int hitHistoryRCCount = 0;

	int time = 180;

    static public float Gravity = -0.5f;

    public MeshRenderer meshRenderer;

    int damage = 100;
    public bool chargeshot = false;
    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        float Gravity_thisFrame = Gravity;
        {





            if (target != null)
            {
                Vector3 relative = (target.GetTargetedPosition() - transform.position);
                
                float h = relative.y;

                relative.y = 0.0f;

                float l = relative.magnitude;
                Vector3 velocity_h = direction * speed / Time.deltaTime;
                float v = velocity_h.y;

                velocity_h.y = 0.0f;
                
                float t = l / velocity_h.magnitude;

                float g = Gravity / Time.deltaTime;

                float hp = v*t+g * t * t / 2;

                if(hp < h)
                {
                    Gravity_thisFrame = 0.0f;
                }
                else
                    Gravity_thisFrame = Gravity*2;

                Quaternion qDirection = Quaternion.LookRotation(velocity_h, Vector3.up);

                Quaternion qTarget = Quaternion.LookRotation(relative);

                if (Quaternion.Angle(qDirection, qTarget) < 90.0f)
                {
                    Quaternion qDirection_toward = Quaternion.RotateTowards(qDirection, qTarget, 0.5f);
                    Quaternion qDirection_fix = Quaternion.Inverse(qDirection)* qDirection_toward;

                    direction = qDirection_fix * direction;
                }


                /*Quaternion qDirection = Quaternion.LookRotation(direction, Vector3.up);

                Quaternion qTarget = Quaternion.LookRotation(target.GetTargetPosition() - transform.position);

                if (Quaternion.Angle(qDirection, qTarget) < 90.0f)
                {
                    Quaternion qDirection_new = Quaternion.RotateTowards(qDirection, qTarget, 0.5f);

                    Quaternion qDirection_result = Quaternion.RotateTowards(initial_direction, qDirection_new, 10.0f);

                    direction = qDirection_result * Vector3.forward;
                }*/
            }

            Vector3 origin, goal;

            if (first && barrel_origin.x != Mathf.NegativeInfinity)
                origin = barrel_origin;
            else
                origin = transform.position;

            goal = transform.position + direction.normalized * speed;

            Ray ray = new Ray(origin, goal - origin);

            int numhit = Physics.RaycastNonAlloc(ray, rayCastHit, (goal - origin).magnitude, 1 << 6 | WorldManager.layerPattern_Building);

            for (int i = 0; i < numhit; i++)
            {
                if (hitHistory.Contains(rayCastHit[i].collider.gameObject))
                    continue;

                hitHistory[hitHistoryCount++] = rayCastHit[i].collider.gameObject;



                RobotController robotController = rayCastHit[i].collider.gameObject.GetComponentInParent<RobotController>();

                if (robotController != owner || owner == null)
                {
                    if (robotController != null)
                    {
                        if (!robotController.has_hitbox)
                            continue;

                        if (hitHistoryRC.Contains(robotController))
                            continue;

                        hitHistoryRC[hitHistoryRCCount++] = robotController;

                        if (owner == null || robotController.team != owner.team)
                            robotController.TakeDamage(rayCastHit[i].point, direction, damage, RobotController.KnockBackType.Normal, owner);


                    }

                    GameObject explode = GameObject.Instantiate(hitEffect_prefab, rayCastHit[i].point, transform.rotation);
                    explode.transform.localScale = Vector3.one * 0.5f;
                    GameObject.Destroy(gameObject);
                    return;
                }
            }

            Vector3 Velocity = direction * speed;

            Velocity.y += Gravity_thisFrame * Time.deltaTime;

            direction = Velocity.normalized;
            speed = Velocity.magnitude;

            transform.position += direction * speed;
            position = transform.position;
            transform.rotation = Quaternion.LookRotation(transform.position-start_pos);
 
            if (time-- <= 0)
            {
                GameObject.Destroy(gameObject);
            }
        }

        first = false;
    }
}
