using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Missile : Projectile
{
    public Quaternion initial_direction;

    Vector3 start_pos;

    // Start is called before the first frame update
    protected override void OnStart()
    {
     

        start_pos = transform.position;

        initial_direction = Quaternion.LookRotation(direction);

        speed = 1.2f;
    }

    RaycastHit[] rayCastHit = new RaycastHit[8];

    public GameObject hitEffect_prefab;

    GameObject[] hitHistory = new GameObject[8];
    RobotController[] hitHistoryRC = new RobotController[8];

    int hitHistoryCount = 0;
    int hitHistoryRCCount = 0;

	int time = 180;

    bool dead = false;

    public MeshRenderer meshRenderer;
    public Effekseer.EffekseerEmitter boostEmitter;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (!dead)
        {





            if (target != null)
            {
                Quaternion qDirection = Quaternion.LookRotation(direction, Vector3.up);

                Quaternion qTarget = Quaternion.LookRotation(target.GetCenter() - transform.position);

                if (Quaternion.Angle(qDirection, qTarget) < 90.0f)
                {
                    Quaternion qDirection_new = Quaternion.RotateTowards(qDirection, qTarget, 0.75f);

                    Quaternion qDirection_result = Quaternion.RotateTowards(initial_direction, qDirection_new, 45.0f);

                    direction = qDirection_result * Vector3.forward;
                }
            }

            Ray ray = new Ray(transform.position, direction);

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

                    robotController.DoDamage(direction, 100, RobotController.KnockBackType.Weak);


                }

                GameObject explode = GameObject.Instantiate(hitEffect_prefab, rayCastHit[i].point, Quaternion.identity);

                explode.transform.localScale = Vector3.one * 0.5f;

                boostEmitter.SendTrigger(0);
                dead = true;
            }



            transform.position += direction * speed;
            transform.rotation = Quaternion.LookRotation(direction);

            if (time-- <= 0)
            {
                dead = true;
                boostEmitter.SendTrigger(0);
            }
        }
        else
        {
            meshRenderer.enabled = false;
            boostEmitter.SendTrigger(0);

            if (boostEmitter.instanceCount <= 0)
                GameObject.Destroy(gameObject);
        }
    }
}
