using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    int timer = 30;
    float spd = 6.0f;
    float delta_y = 32.0f;
    public RectTransform rectTransform;
    public RectTransform canvasTransform;
    public Camera uiCamera;
       public Canvas canvas;

    public Vector3 Position;
    // Start is called before the first frame update
    void Start()
    {

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, Position);
        Vector2 uiPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform,
            screenPoint,
            uiCamera,
            out uiPoint
        );

        rectTransform.localPosition =  uiPoint+Vector2.up*delta_y;
    }

    // Update is called once per frame
    void Update()
    {
    }
    void FixedUpdate()
    {
        if (timer <= 0)
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
           

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, Position);
            Vector2 uiPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasTransform,
                screenPoint,
                uiCamera,
                out uiPoint
            );



            rectTransform.localPosition = uiPoint+Vector2.up*delta_y;
            delta_y += spd;
            spd -= 0.2f;

            timer--;
        }

        
    }
}
