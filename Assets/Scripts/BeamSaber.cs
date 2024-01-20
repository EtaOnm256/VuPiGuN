using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class BeamSaber : MonoBehaviour
{
    public LineRenderer lineRenderer;

    bool _slashing = false;
    bool _emitting = false;

    RaycastHit[] rayCastHit = new RaycastHit[8];

    public GameObject hitEffect_prefab;

    GameObject[] hitHistory = new GameObject[16];
    RobotController[] hitHistoryRC = new RobotController[16];
    int hitHistoryCount = 0;
    int hitHistoryRCCount = 0;

    public Vector3 dir;

    public bool slashing
    {
        set {
            _slashing = value;

            if (_slashing)
                _emitting = true;

            if (!_slashing)
            {
                hitHistoryCount = 0;
                hitHistoryRCCount = 0;

                System.Array.Fill(hitHistory, null);
                System.Array.Fill(hitHistoryRC, null);
            }

        }
        get { return _slashing; }
    }

    public bool emitting
    {
        set
        {
            _emitting = value;

            if (!_emitting)
                _slashing = false;

            lineRenderer.enabled = _emitting;
            

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

  
    }

    const int num_points = 3;

    Vector3[] prev_points = new Vector3[num_points];
    Vector3[] points = new Vector3[num_points];

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 start = transform.TransformPoint(new Vector3(0.0f, 0.02f, 0.0f));
        Vector3 end = transform.TransformPoint(lineRenderer.GetPosition(1));

        for (int i=0;i<num_points;i++)
        {
            points[i] = Vector3.Lerp(start, end, ((float)i) / (num_points - 1));
        }

      

        if (slashing)
        {
            EvalHit(start, end);


            for (int idx_point=0; idx_point < num_points; idx_point++)
            {
                EvalHit(points[idx_point], prev_points[idx_point]);
            }
           
        }

        points.CopyTo(prev_points,0);
    }

    private void EvalHit(Vector3 p1,Vector3 p2)
    {
        Ray ray = new Ray(p1, p2 - p1);

        float length = (p2 - p1).magnitude;

        int numhit = Physics.RaycastNonAlloc(ray, rayCastHit, length, 1 << 6);

        for (int idx_hit = 0; idx_hit < numhit; idx_hit++)
        {
            if (hitHistory.Contains(rayCastHit[idx_hit].collider.gameObject))
                continue;

            hitHistory[hitHistoryCount++] = rayCastHit[idx_hit].collider.gameObject;



            RobotController robotController = rayCastHit[idx_hit].collider.gameObject.GetComponentInParent<RobotController>();

            if (robotController != null)
            {
                if (hitHistoryRC.Contains(robotController))
                    continue;

                hitHistoryRC[hitHistoryRCCount++] = robotController;

                robotController.DoDamage(dir, damage, strong);

                GameObject.Instantiate(hitEffect_prefab, rayCastHit[idx_hit].point, Quaternion.identity);
            }


        }
    }
}
