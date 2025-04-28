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
     

        position = start_pos = transform.position;

        initial_direction = Quaternion.LookRotation(direction);

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

    [SerializeField] float homing_strength = 0.75f;
    [SerializeField] float homing_limit = 45.0f;
   

    public MeshRenderer meshRenderer;
    public Effekseer.EffekseerEmitter boostEmitter;

    public bool chargeshot = false;
    [SerializeField]int damage = 100;
    [SerializeField] bool cluster = false;
    [SerializeField] GameObject cluster_prefab = null;

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {
        if (!dead)
        {
            if (target != null)
            {
                Quaternion qDirection = Quaternion.LookRotation(direction, Vector3.up);

                Quaternion qTarget = Quaternion.LookRotation(target.GetTargetedPosition() - transform.position);

                float angle = Quaternion.Angle(qDirection, qTarget);

                if (angle < 90.0f)
                {
                    Quaternion qDirection_new = Quaternion.RotateTowards(qDirection, qTarget, homing_strength);

                    Quaternion qDirection_result = Quaternion.RotateTowards(initial_direction, qDirection_new, homing_limit);

                    direction = qDirection_result * Vector3.forward;

                    if(cluster)
                    {
                        float dot = Vector3.Dot(target.GetTargetedPosition() - transform.position, direction.normalized);

                        if(dot < 25.0f)
                        {
                            for (int i = 0; i < 32; i++)
                            {
                                GameObject beam_obj = GameObject.Instantiate(cluster_prefab, position, transform.rotation);

                                SMGBullet beam = beam_obj.GetComponent<SMGBullet>();

                                Vector3 dir_rel = Quaternion.Euler(Random.value * 30.0f - 15.0f, Random.value * 30.0f - 15.0f, 0.0f) * Vector3.forward;

                                Quaternion dir_origin;

                                dir_origin = transform.rotation;

                                beam.direction = dir_origin * dir_rel;
                                //beam.target = Target_Robot;
                                beam.target = null;
                                beam.team = owner.team;
                                beam.owner = owner;
                                beam.chargeshot = chargeshot;
                            }
                            dead = true;
                        }
                    }
                }
            }

            if (!dead)
            {

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

                        robotController.TakeDamage(rayCastHit[i].point, direction, damage, RobotController.KnockBackType.Normal, owner);


                    }

                    GameObject explode = GameObject.Instantiate(hitEffect_prefab, rayCastHit[i].point, Quaternion.identity);

                    explode.transform.localScale = Vector3.one * 0.5f;

                    boostEmitter.SendTrigger(0);
                    dead = true;
                }



                position = transform.position += direction * speed;
                transform.rotation = Quaternion.LookRotation(direction);

                if (time-- <= 0)
                {
                    dead = true;
                    boostEmitter.SendTrigger(0);
                }
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

    private void Awake()
    {
        WorldManager.current_instance.pausables.Add(this);

    }

    public override void OnPause()
    {
        boostEmitter.paused = true;
    }

    public override void OnUnpause()
    {
        boostEmitter.paused = false;
    }

    private void OnDestroy()
    {
        WorldManager.current_instance.pausables.Remove(this);
    }
}
