using UnityEngine;
using UnityEngine.UI;
public class UIController_Overlay : MonoBehaviour
{

    [SerializeField]
    public Transform targetTfm;

    private RectTransform myRectTfm;
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    void Start()
    {
        myRectTfm = GetComponent<RectTransform>();
    }

    void Update()
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