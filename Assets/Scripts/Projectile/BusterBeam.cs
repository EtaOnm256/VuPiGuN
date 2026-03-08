using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class BusterBeam : Projectile
{


    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] BusterBeamRenderer busterBeamRenderer;

    public Quaternion initial_direction;

    bool enabled = false;

    public float MaxLength = 15.0f;

    Vector3 start_pos;

    [SerializeField]int damage = 100;

    int base_damage;
    float base_speed;
    float base_width;

    [SerializeField] bool pierce = true;

    bool _emitting = false;

    public bool emitting
    {
        set
        {
            if(value)
            {
                if (!_emitting)
                {

                    position = start_pos = transform.position;

                    for (int i = 0; i < lineRenderer.positionCount; i++)
                    {
                        lineRenderer.SetPosition(i, start_pos);
                        busterBeamRenderer.SetPosition(i, start_pos);
                    }

                    initial_direction = Quaternion.LookRotation(direction);

                    switch (shotModifier)
                    {
                        case Weapon.ShotModifier.CHARGED:
                            speed = base_speed*1.3f;
                            damage = (int)(base_damage * 2.0f);
                            break;
                        default:
                            speed = base_speed;
                            damage = base_damage;
                            break;
                    }

                    //if(itemFlag.HasFlag(RobotController.ItemFlag.TrackingSystem))
                    //{
                    //    homing_limit *= 1.5f;
                    //    homing_strength *= 1.5f;
                    //}


                    enabled = busterBeamRenderer.enabled = true;

                    lineRenderer.enabled = false;

                    dead = false;
                    //time = 120;

                  
                }
            }
            else
            {
                enabled = busterBeamRenderer.enabled = lineRenderer.enabled = false;
                dead = true;
            }

            _emitting = value;
        }
    }

    

    RaycastHit[] rayCastHit = new RaycastHit[32];
    RaycastHit[] rayCastHit_NoOrder = new RaycastHit[32];

    public GameObject hitEffect_prefab;

    GameObject[] hitHistory = new GameObject[32];
    RobotController[] hitHistoryRC = new RobotController[32];

    int hitHistoryCount = 0;
    int hitHistoryRCCount = 0;

    //int time = 120;

    public Weapon.ShotModifier shotModifier = Weapon.ShotModifier.NORMAL;

    [SerializeField] float homing_strength = 1.0f;
    [SerializeField] float homing_limit = 10.0f;

    [SerializeField] float hitsphere_width = -1.0f;
    [SerializeField] float hiteffect_scale = 1.0f;

    [SerializeField] RobotController.KnockBackType KnockBackType = RobotController.KnockBackType.Normal;

    int wave_timer = 0;

    private void Awake()
    {
        base_damage = damage;
        base_speed = speed;
        base_width = busterBeamRenderer.widthMultiplier;
    }

    // Update is called once per frame
    protected override void OnFixedUpdate()
    {


        if (!dead && enabled)
        {
            hitHistoryCount = 0;
            hitHistoryRCCount = 0;

            for (int i = 0; i < hitHistory.Length; i++)
            {
                hitHistory[i] = null;
                hitHistoryRC[i] = null;
            }

            int numhit = 0;

            Vector3 origin, goal;

            origin = barrel_origin;

            //goal = lineRenderer.GetPosition(lineRenderer.positionCount - 1) + direction.normalized * speed;
            goal = busterBeamRenderer.GetPosition(1) + direction.normalized * speed;


            float hitsphere_width_now;

            if (shotModifier == Weapon.ShotModifier.CHARGED)
                hitsphere_width_now = hitsphere_width * 2.0f;
            else
                hitsphere_width_now = hitsphere_width;

            if (hitsphere_width <= 0.0f)
            {
                Ray ray = new Ray(origin, goal - origin);
                numhit = Physics.RaycastNonAlloc(ray, rayCastHit_NoOrder, (goal - origin).magnitude, 1 << 6 | WorldManager.layerPattern_Building);
            }
            else
            {
                numhit = Physics.SphereCastNonAlloc(origin, hitsphere_width_now, (goal - origin).normalized, rayCastHit_NoOrder, (goal - origin).magnitude, 1 << 6 | WorldManager.layerPattern_Building);
            }

            bool coli = false;
            Vector3 coli_pos = Vector3.zero;

            rayCastHit = rayCastHit_NoOrder.Take(numhit).OrderBy(x => x.distance).ToArray();

            for (int i = 0; i < numhit; i++)
            {
                if (hitHistory.Contains(rayCastHit[i].collider.gameObject))
                    continue;

                if (hitHistoryCount >= hitHistory.Length)
                    break;

                if (rayCastHit[i].distance == 0.0f)
                {
                    rayCastHit[i].point = origin;
                }

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

                        if (hitHistoryRCCount >= hitHistoryRC.Length)
                            break;

                        hitHistoryRC[hitHistoryRCCount++] = robotController;

                        robotController.DoHitStop(5);

                        if (owner == null || robotController.team != owner.team)
                            robotController.TakeDamage(rayCastHit[i].point, direction, damage, KnockBackType, owner);

                        //if(!pierce)
                        //    dead = true;
                    }
                    else
                    {
                        //dead = true;



                        coli_pos = origin + (goal - origin).normalized * rayCastHit[i].distance;
                        coli = true;

                        
                    }

                    GameObject hitEffect_obj = GameObject.Instantiate(hitEffect_prefab, rayCastHit[i].point, Quaternion.identity);
                    hitEffect_obj.transform.localScale = new Vector3(hiteffect_scale, hiteffect_scale, hiteffect_scale);

                    if (coli)
                        break;
                }
            }

            if (coli)
            {
                position = coli_pos;
            }
            else
            { 
                //float length = Vector3.Dot(lineRenderer.GetPosition(lineRenderer.positionCount - 1) - transform.position, direction.normalized);
                float length = Vector3.Dot(busterBeamRenderer.GetPosition(1) - transform.position, direction.normalized);

                length += speed;

                position = transform.position + direction * length;
            }
            //lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
            busterBeamRenderer.SetPosition(1, position);


            //lineRenderer.SetPosition(0, transform.position);
            busterBeamRenderer.SetPosition(0, transform.position);

            if (wave_timer > 3)
                wave_timer = 0;
            else
                wave_timer++;

            float f = Mathf.Abs(wave_timer - 2) * 0.1f+1.0f;

            if (shotModifier == Weapon.ShotModifier.CHARGED)
                //lineRenderer.widthMultiplier = base_width * 2.0f* f;
                busterBeamRenderer.widthMultiplier = base_width * 2.0f * f;
            else
                //lineRenderer.widthMultiplier = base_width* f;
                busterBeamRenderer.widthMultiplier = base_width * f;
        }
     
        first = false;
    }
}
