using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class BeamSaber : MonoBehaviour
{
    public LineRenderer lineRenderer;

    bool _emitting = false;

    RaycastHit[] rayCastHit = new RaycastHit[8];

    public GameObject hitEffect_prefab;

    GameObject[] hitHistory = new GameObject[16];
    RobotController[] hitHistoryRC = new RobotController[16];
    int hitHistoryCount = 0;
    int hitHistoryRCCount = 0;

    public Vector3 dir;

    public bool emitting
    {
        set {
            _emitting = value;

            lineRenderer.enabled = _emitting;

            if(!_emitting)
            {
                hitHistoryCount = 0;
                hitHistoryRCCount = 0;

                System.Array.Fill(hitHistory, null);
                System.Array.Fill(hitHistoryRC, null);
            }

        }
        get { return _emitting; }
    }

    public bool strong = false;
    public int damage = 250;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 start = transform.TransformPoint(new Vector3(0.0f,0.02f,0.0f));
        Vector3 end = transform.TransformPoint(lineRenderer.GetPosition(1));

        length = (end - start).magnitude;
    }

    float length;

    // Update is called once per frame
    void FixedUpdate()
    {
        if(emitting)
        {

            Vector3 start = transform.TransformPoint(new Vector3(0.0f, 0.02f, 0.0f));
            Vector3 end = transform.TransformPoint(lineRenderer.GetPosition(1));

            Ray ray = new Ray(start,end- start);

            int numhit = Physics.RaycastNonAlloc(ray, rayCastHit, length, 1 << 6);

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

                    robotController.DoDamage(dir, damage, strong);

                    GameObject.Instantiate(hitEffect_prefab, rayCastHit[i].point, Quaternion.identity);
                }


            }
        }
    }
}
