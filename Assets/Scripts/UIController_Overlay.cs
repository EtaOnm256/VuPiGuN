#define ACCURATE_SEEK
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class UIController_Overlay : MonoBehaviour
{
    public RobotController origin;
    [SerializeField]
    public RobotController target;

    public bool lockonmode = false;
    public bool infight = false;
    public bool aiming = false;
    //public bool aim_fixed = false;

    public Dictionary<RobotController, ReticleAndGuideline> robotReticle = new Dictionary<RobotController, ReticleAndGuideline>();

    //public List<Weapon> weapons = new List<Weapon>();

    [System.NonSerialized] public Camera uiCamera;

    public Canvas canvas;

    public GameObject weaponPanel;

    public float distance = 1000.0f;

    public RobotController.LockonState lockonState;

    [SerializeField] GameObject reticle_prefab;
    [SerializeField] GameObject guideline_prefab;
    [SerializeField] GameObject icon_prefab;
    [SerializeField] GameObject inner_prefab;

    public GameObject radarPanel;

    public class ReticleAndGuideline
    {
        public Reticle reticle;
        public Guideline guideline;
    }

    public class Reticle
    {
        public GameObject gameObject;
        public RectTransform rectTfm;
        public Image image;
        public Slider HPslider;
        public RectTransform HPrectTransform;
        public GameObject radaricon_obj;
        public Image radaricon_image;
        public RectTransform radarrectTransform;
        public GameObject outer;
        public Image[] outer_image;

        public GameObject inner_gameObject;
        public Image inner_image;
        public RectTransform inner_rectTfm;

    }
    public class Guideline
    {
        public GameObject gameObject;
        public LineRenderer lineRenderer;
    }

    public LineRenderer guideline_lineRenderer_l;
    public LineRenderer guideline_lineRenderer_r;

    [SerializeField] TextMeshProUGUI orderText;

    public void AddRobot(RobotController robotController)
    {
        if (robotReticle.ContainsKey(robotController))
            return;

        Reticle reticle = new Reticle();

        reticle.gameObject = Instantiate(reticle_prefab,transform);
        reticle.rectTfm = reticle.gameObject.GetComponent<RectTransform>();
        reticle.rectTfm.localScale = Vector3.one;
        reticle.image = reticle.gameObject.GetComponent<Image>();

        reticle.HPrectTransform = reticle.gameObject.transform.Find("HPSlider").gameObject.GetComponent<RectTransform>();
        reticle.HPslider = reticle.gameObject.transform.Find("HPSlider").gameObject.GetComponent<Slider>();

        reticle.outer = reticle.gameObject.transform.Find("Outer").gameObject;
        reticle.outer_image = new Image[4];

        for(int i=0;i<4;i++)
        {
            reticle.outer_image[i] = reticle.outer.transform.Find($"Outer ({i})").GetComponent<Image>();
        }
                
        reticle.inner_gameObject = Instantiate(inner_prefab, transform);
        reticle.inner_image = reticle.inner_gameObject.GetComponent<Image>();
        reticle.inner_rectTfm = reticle.inner_gameObject.GetComponent<RectTransform>();
        reticle.inner_rectTfm.localScale = Vector3.one;

        reticle.radaricon_obj = Instantiate(icon_prefab, radarPanel.transform);
        reticle.radaricon_image = reticle.radaricon_obj.GetComponent<Image>();
        reticle.radarrectTransform = reticle.radaricon_obj.GetComponent<RectTransform>();

        Guideline guideline = new Guideline();

        guideline.gameObject = Instantiate(guideline_prefab, transform);
        guideline.lineRenderer = guideline.gameObject.GetComponent<LineRenderer>();



        robotReticle.Add(robotController,new ReticleAndGuideline { reticle = reticle, guideline = guideline });
    }

    public void RemoveRobot(RobotController robotController)
    {
        if (!robotReticle.ContainsKey(robotController))
            return;

        Destroy(robotReticle[robotController].reticle.gameObject);
        Destroy(robotReticle[robotController].guideline.gameObject);
        Destroy(robotReticle[robotController].reticle.radaricon_obj);
        Destroy(robotReticle[robotController].reticle.inner_gameObject);

        robotReticle.Remove(robotController);
    }

    public void AddWeapon(Weapon weapon)
    {
        weapon.uIController_Overlay = this;

    //    weapons.Add(weapon);
    }

    public void RemoveWeapon(Weapon weapon)
    {
    //    weapons.Remove(weapon);
    }

    private void Awake()
    {
        // reticle_prefab = Resources.Load<GameObject>("UI/Reticle");
        // guideline_prefab = Resources.Load<GameObject>("UI/GuideLine");

        uiCamera = canvas.worldCamera;
    }

    void Start()
    {
       
    }

    private Vector3 AbsToRel(Vector3 abs,Vector3 pos,Quaternion rot)
    {
        return Quaternion.Inverse(rot)* (abs - pos);
    }

    private Vector3 RelToAbs(Vector3 rel, Vector3 pos, Quaternion rot)
    {
        return (rot * rel)+pos;
    }

    private void SetGuideLinePosition(LineRenderer guideline_lineRenderer,Vector3 relative_l,Vector3 relative_f, Vector3 relative_f_far)
    {
       
        Vector3 screenPoint_line_l = RectTransformUtility.WorldToScreenPoint(Camera.main, RelToAbs(relative_l, origin.GetCenter(), Camera.main.transform.rotation));
        screenPoint_line_l.z = 50.0f;
        Vector3 screenPoint_line_l_guide = RectTransformUtility.WorldToScreenPoint(Camera.main, RelToAbs(relative_l + relative_f, origin.GetCenter(), Camera.main.transform.rotation));
        screenPoint_line_l_guide.z = 50.0f;

        guideline_lineRenderer.positionCount = 2;
        guideline_lineRenderer.SetPosition(0, uiCamera.ScreenToWorldPoint(screenPoint_line_l));
        guideline_lineRenderer.SetPosition(1, uiCamera.ScreenToWorldPoint(screenPoint_line_l_guide));

        Vector3 screenPoint_line_l_far = RectTransformUtility.WorldToScreenPoint(Camera.main, RelToAbs(relative_l + relative_f_far, origin.GetCenter(), Camera.main.transform.rotation));
        screenPoint_line_l_far.z = 50.0f;

        float length_on_screen_far = (screenPoint_line_l_far - screenPoint_line_l).magnitude;
        float length_on_screen = (screenPoint_line_l_guide - screenPoint_line_l).magnitude;

        guideline_lineRenderer.endWidth = 0.1f * ( (length_on_screen_far-length_on_screen) / length_on_screen_far);
    }

    void Update()
    {
        if (!origin)
            return;


        foreach(var reticle in robotReticle)
        {
            Vector3 relative = AbsToRel(reticle.Key.GetCenter(), origin.GetCenter(), Camera.main.transform.rotation);

            float z = Camera.main.transform.InverseTransformPoint(reticle.Key.transform.position).z;

            if (reticle.Key.team != origin.team && z >= 0)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, reticle.Key.GetCenter());
                {
                    Vector2 uiPoint;

                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.GetComponent<RectTransform>(),
                        screenPoint,
                        uiCamera,
                        out uiPoint
                    );

                    reticle.Value.reticle.rectTfm.localPosition = uiPoint;
                    //reticle.Value.reticle.image.enabled = true;
                    reticle.Value.reticle.gameObject.SetActive(true);
                }

                if(target == reticle.Key && lockonState != RobotController.LockonState.FREE && aiming && origin && !origin.dead)
                {
                    Vector2 screenPoint_inner;
                    
                    //if(aim_fixed)
                    //    screenPoint_inner = RectTransformUtility.WorldToScreenPoint(Camera.main, reticle.Key.GetTargetedPosition());
                    //else
                        screenPoint_inner = RectTransformUtility.WorldToScreenPoint(Camera.main, origin.virtual_targeting_position_forUI);

                    Vector2 uiPoint;

                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.GetComponent<RectTransform>(),
                        screenPoint_inner,
                        uiCamera,
                        out uiPoint
                    );

                    reticle.Value.reticle.inner_rectTfm.localPosition = uiPoint;
                    //reticle.Value.reticle.image.enabled = true;
                    reticle.Value.reticle.inner_gameObject.SetActive(true);
                }
                else
                    reticle.Value.reticle.inner_gameObject.SetActive(false);

                reticle.Value.guideline.lineRenderer.positionCount = 2;

                float length = relative.magnitude;

                Quaternion relative_q = Quaternion.FromToRotation(Vector3.forward, relative);

                Vector3 relative_ypr = relative_q.eulerAngles;

                relative_ypr.x = -10.0f;
                relative_ypr.y = 0.0f;

                Quaternion relative_q_offset = Quaternion.Euler(relative_ypr);

                Vector3 relative_offset = relative_q_offset * Vector3.forward * length;

                //Vector3 world_offset = looking_transform.TransformPoint(relative_offset);
                Vector3 world_offset = RelToAbs(relative_offset, origin.GetCenter(), Camera.main.transform.rotation);

                Vector3 screenPoint_guide
                    = RectTransformUtility.WorldToScreenPoint(Camera.main, world_offset);
                Vector2 uiPoint_guide;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenPoint_guide,
                uiCamera,
                out uiPoint_guide
                );

                Vector3 screenPoint_line = screenPoint;
                screenPoint_line.z = 50.0f;

                Vector3 screenPoint_guide_line = screenPoint_guide;
                screenPoint_guide_line.z = 50.0f;

                if (!lockonmode && !infight)
                {
                    reticle.Value.guideline.lineRenderer.enabled = true;
                    reticle.Value.guideline.lineRenderer.SetPosition(0, uiCamera.ScreenToWorldPoint(screenPoint_line));
                    reticle.Value.guideline.lineRenderer.SetPosition(1, uiCamera.ScreenToWorldPoint(screenPoint_guide_line));
                }
                else
                    reticle.Value.guideline.lineRenderer.enabled = false;

                if (target == reticle.Key)
                {
                    reticle.Value.reticle.outer.SetActive(lockonmode);

                    switch (lockonState)
                    {
                        case RobotController.LockonState.FREE:
                            reticle.Value.reticle.image.color = reticle.Value.guideline.lineRenderer.startColor = reticle.Value.guideline.lineRenderer.endColor = Color.yellow;

                            foreach(var image in reticle.Value.reticle.outer_image)
                            {
                                image.color = Color.yellow;
                            }
                            break;
                        case RobotController.LockonState.SEEKING:
#if !ACCURATE_SEEK
                            reticle.Value.reticle.image.color = new Color(1.0f, 0.5f, 0.0f);
                            break;
#endif
                        case RobotController.LockonState.LOCKON:
                            reticle.Value.reticle.image.color = 
                                reticle.Value.guideline.lineRenderer.startColor = reticle.Value.guideline.lineRenderer.endColor = Color.red;
                            foreach (var image in reticle.Value.reticle.outer_image)
                            {
                                image.color = Color.red;
                            }
                            break;
                    }


                }
                else
                {
                    reticle.Value.reticle.image.color =
                        reticle.Value.guideline.lineRenderer.startColor = reticle.Value.guideline.lineRenderer.endColor = Color.green;

                    reticle.Value.reticle.outer.SetActive(false);

                }

                    //reticle.Value.reticle.HPslider.enabled = true;
                    reticle.Value.reticle.HPslider.value = reticle.Key.HP;
                reticle.Value.reticle.HPslider.maxValue = reticle.Key.robotParameter.MaxHP;

                reticle.Value.reticle.HPrectTransform.anchoredPosition = new Vector3(0.0f, Mathf.Max(47.0f,2500.0f/z), 0.0f);

            }
            else
            {
                //reticle.Value.reticle.image.enabled = false;
                reticle.Value.guideline.lineRenderer.enabled = false;
                //reticle.Value.reticle.HPslider.enabled = false;

                reticle.Value.reticle.gameObject.SetActive(false);
                reticle.Value.reticle.inner_gameObject.SetActive(false);
            }

            reticle.Value.reticle.radarrectTransform.anchoredPosition = new Vector3(relative.x, relative.z, 0.0f);

            if (reticle.Key.team != origin.team)
            {
                reticle.Value.reticle.radaricon_image.color = Color.red;
            }
            else if (reticle.Key == origin)
            {
                reticle.Value.reticle.radaricon_image.color = Color.blue;
            }
            else
                reticle.Value.reticle.radaricon_image.color = Color.green;
        }

        if (!lockonmode && !infight)
        {
            Vector3 relative_f = Vector3.forward * distance;
            relative_f = Quaternion.AngleAxis(-10.0f, Vector3.right) * relative_f;

            Vector3 relative_f_far = Vector3.forward * 10000.0f;
            relative_f_far = Quaternion.AngleAxis(-10.0f, Vector3.right) * relative_f_far;

            Vector3 relative_l = -Vector3.right * 0.5f;

            SetGuideLinePosition(guideline_lineRenderer_l, relative_l, relative_f, relative_f_far);

            Vector3 relative_r = Vector3.right * 0.5f;

            SetGuideLinePosition(guideline_lineRenderer_r, relative_r, relative_f, relative_f_far);

            guideline_lineRenderer_l.enabled = true;
            guideline_lineRenderer_r.enabled = true;
        }
        else
        {
            guideline_lineRenderer_l.enabled = false;
            guideline_lineRenderer_r.enabled = false;
        }
    }

    public void OnChangeOrderToAI(WorldManager.OrderToAI orderToAI)
    {
        switch(orderToAI)
        {
            case WorldManager.OrderToAI.NORMAL:
                orderText.text = "ÉmÅ[É}Éã";
                break;
            case WorldManager.OrderToAI.FOCUS:
                orderText.text = "èWíÜ";
                break;
            case WorldManager.OrderToAI.SPREAD:
                orderText.text = "ï™éU";
                break;
            case WorldManager.OrderToAI.EVADE:
                orderText.text = "âÒî";
                break;

        }
    }
}