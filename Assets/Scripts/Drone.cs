using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Drone : MonoBehaviour
{
    public RobotController target = null;
    // Start is called before the first frame update

    public RobotController owner; // ���˂����܂Őݒ肳��Ȃ��̂Œ���

    Quaternion orient = Quaternion.Euler(90.0f, 0.0f, 0.0f);

    GameObject beam_prefab;

    public GameObject anchor;
    const float speed = 1.2f;

    public GameObject firePoint;

    public enum State

    {
        Ready,
        Going,
        Firing,
        Homing,
        Backing
    }

    public Vector3 offset = new Vector3(0.0f, 0.0f, 0.005f);

    public State state = State.Ready;

    int launch_timer = 0;

    void Awake()
    {
        beam_prefab = Resources.Load<GameObject>("Projectile/Beam");
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case State.Ready:
                if (target != null && target && !target.dead)
                {
                    transform.parent = null;
                    state = State.Going;

                    orient = Quaternion.Euler(45.0f, Random.Range(0.0f, 360.0f), 0.0f);
                    launch_timer = 0;
                }
                break;
            case State.Going:
                if (!(target && !target.dead))
                {
                    state = State.Homing;
                }
                else
                {

                    Vector3 goal_pos = target.GetCenter()
                        + orient * Vector3.back * 10.0f;
    

                    if (Vector3.Distance(goal_pos, transform.position) <= speed)
                    {
                        state = State.Firing;
                    }
                    else
                    {
                        Vector3 goal_dir = (goal_pos - transform.position).normalized;

                        float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(goal_dir, Vector3.up));

                        Quaternion q = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(goal_dir, Vector3.up), 10.0f);

                        float factor = Mathf.LerpAngle(0.0f,1.0f,Mathf.Clamp(90.0f-angle, 0.0f, 10.0f)/10.0f);
                        
                        if (launch_timer > 30)
                        {
                            transform.position += q * Vector3.forward * speed * factor;
                        }
                        else
                            transform.position += q * Vector3.forward * speed;

                        transform.rotation = q;
                    }
                }
                launch_timer++;
                break;
            case State.Firing:
                if (!(target && !target.dead))
                {
                    state = State.Homing;
                }
                else
                {
                    Vector3 goal_pos = target.GetCenter();
                    Vector3 goal_dir = (goal_pos - transform.position).normalized;

                    float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(goal_dir, Vector3.up));
                    Quaternion q = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(goal_dir, Vector3.up), 10.0f);

                    //Quaternion qGoal = Quaternion.FromToRotation(transform.rotation * Vector3.forward, goal_dir) * transform.rotation;
                    //float angle = Quaternion.Angle(transform.rotation, qGoal);
                    //Quaternion q = Quaternion.RotateTowards(transform.rotation, qGoal, 10.0f);

                    transform.rotation = q;

                    if (angle < 10.0f)
                    {

                        GameObject beam_obj = GameObject.Instantiate(beam_prefab, firePoint.transform.position, firePoint.transform.rotation);

                        Beam beam = beam_obj.GetComponent<Beam>();

                        beam.direction = gameObject.transform.forward;
                        beam.target = target;
                        beam.team = owner.team;
                        beam.worldManager = owner.worldManager;
                        state = State.Homing;
                    }
                }
                break;
            case State.Homing:
                {
                    Vector3 goal_pos = anchor.transform.position;
                    Vector3 goal_dir = (goal_pos - transform.position).normalized;

                    float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(goal_dir, Vector3.up));

                    Quaternion q = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(goal_dir, Vector3.up), 10.0f);

                    transform.rotation = q;

                    if (angle < 10.0f)
                    {
                        state = State.Backing;
                    }
                    else
                    {
                       
                    }
                }
                break;
            case State.Backing:
                {
                    Vector3 goal_pos = anchor.transform.position;
       
                    Vector3 goal_dir = (goal_pos - transform.position).normalized;


                  

                    if(Vector3.Distance(goal_pos, transform.position) <= speed)
                    {
                        transform.parent = anchor.transform;
                        transform.localPosition = offset;
                        transform.localRotation = Quaternion.identity;
                        target = null;
                        state = State.Ready;
                    }
                    else
                    {
                        transform.position += goal_dir * speed;
                        transform.rotation = Quaternion.LookRotation(goal_dir, Vector3.up);
                    }
                }
                break;

        }





    }
}
