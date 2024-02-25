using UnityEngine;
using UnityEngine.UI;
public class UIController_Overlay : MonoBehaviour
{
    public Transform originTrm;
    [SerializeField]
    public Transform targetTfm;

    private RectTransform myRectTfm;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private Image image;

    [SerializeField] private bool mode_current = false;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    void Start()
    {
        myRectTfm = GetComponent<RectTransform>();
    }

    private Vector3 AbsToRel(Vector3 abs,Vector3 pos,Quaternion rot)
    {
        return Quaternion.Inverse(rot)* (abs - pos);
    }

    private Vector3 RelToAbs(Vector3 rel, Vector3 pos, Quaternion rot)
    {
        return (rot * rel)+pos;
    }

    void Update()
    {
        if (mode_current)
        {
            image.enabled = true;

            if (targetTfm != null)
            {
           


                //Vector3 relative = looking_transform.InverseTransformPoint(targetTfm.position + offset);
                Vector3 relative = AbsToRel(targetTfm.position, originTrm.position, Camera.main.transform.rotation);

                float length = relative.magnitude;

                Quaternion relative_q = Quaternion.FromToRotation(Vector3.forward, relative);

                Vector3 relative_ypr = relative_q.eulerAngles;

                relative_ypr.x = -10.0f;
                relative_ypr.y = 0.0f;

                Quaternion relative_q_offset = Quaternion.Euler(relative_ypr);

                Vector3 relative_offset = relative_q_offset*Vector3.forward* length;

                //Vector3 world_offset = looking_transform.TransformPoint(relative_offset);
                Vector3 world_offset =  RelToAbs(relative_offset, originTrm.position, Camera.main.transform.rotation);

                myRectTfm.position
                    = RectTransformUtility.WorldToScreenPoint(Camera.main, world_offset + offset);

            }
            else
            {
                myRectTfm.position
                    = new Vector2(0.5f, 0.5f);
            }    
        }
        else
        {
            
            if (targetTfm != null)
            {
                myRectTfm.position
                    = RectTransformUtility.WorldToScreenPoint(Camera.main, targetTfm.position + offset);
                image.enabled = true;
            }
            else
                image.enabled = false;
        }

    }
}