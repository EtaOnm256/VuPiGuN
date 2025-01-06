using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageText : Pausable
{
    int timer = 30;
    float spd = 6.0f;
    float delta_y = 32.0f;
    public RectTransform rectTransform;
    public RectTransform canvasTransform;
    public Camera uiCamera;
       public Canvas canvas;

    public Vector3 Position;

    [SerializeField]TMPro.TextMeshProUGUI text;

    public int damage;
    public bool from_player;

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

        text.text = damage.ToString();

        
/*
        if (!from_player)
        {
            text.fontSize = 36;
            text.faceColor = new Color(1.0f, 1.0f, 1.0f);
            text.outlineColor = new Color(0.25f, 0.25f, 0.25f);
        }
        else
        {
            text.fontSize = 48;
            text.faceColor = new Color(1.0f, 0.0f, 0.0f);
            text.outlineColor = new Color(0.0f, 0.0f, 0.0f);
        }*/

        
    }

    // Update is called once per frame
    void Update()
    {

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, Position);
        float z = Camera.main.transform.InverseTransformPoint(Position).z;

        if (z <= 0.0f)
        {
            text.enabled = false;
        }
        else
        {
            text.enabled = true;
        }

        Vector2 uiPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform,
            screenPoint,
            uiCamera,
            out uiPoint
        );




        rectTransform.localPosition = uiPoint + Vector2.up * delta_y;
    }
    protected override void OnFixedUpdate()
    {
        if (timer <= 0)
        {
            GameObject.Destroy(gameObject);
        }
        else
        {
           

            delta_y += spd;
            spd -= 0.2f;

            timer--;
        }
    }
}
