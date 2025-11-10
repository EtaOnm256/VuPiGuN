using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;
public class WorldMap : MonoBehaviour
{
    [SerializeField] Image blackout;
    [SerializeField] AudioSource audioSource;

    [SerializeField] GameObject node_prefab;
    [SerializeField] GameObject edge_prefab;

    [SerializeField] GameState gameState;

    [SerializeField] RectTransform scenePanel_rectTransform;
    [SerializeField] Image scenePanel_Image;

    [SerializeField] CanvasScaler canvasScaler;

    bool fadecomplete = false;
    WorldMapNode destinationNode = null;
    WorldMapEdge destinationEdge = null;

    
    
    private void Awake()
    {

        var next = gameState.GetNextStage_UpdateSkyIndex();

        if (next.Item1 == GameState.Destination.Garage)
        {
            scenePanel_Image.sprite = Resources.Load<Sprite>($"WorldMap/{next.Item2}_{gameState.skyindex}");
        }
        else
            scenePanel_Image.sprite = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Blackin");

        int current_x=160;

        int height = 240;
        int center_y = 320;

        int spacing_x = 240;
        int spacing_x_short = 160;

        List<WorldMapNode> list_node = new List<WorldMapNode>();

        foreach (var chapter in gameState.chapters)
        {
            List<WorldMapNode> list_node_in_chapter = new List<WorldMapNode>();

            if (chapter.stages.Length > 0)
            {
                int spacing_y = 0;
                int current_y = center_y - height / 2;

                if (chapter.stages.Length > 1)
                {
                    spacing_y = height / (chapter.stages.Length - 1);
                    current_y = center_y - height / 2;
                }
                else
                    current_y = center_y;



                foreach (var stage in chapter.stages)
                {
                    GameObject node_obj = GameObject.Instantiate(node_prefab, Vector3.back,Quaternion.identity);

                    WorldMapNode node = node_obj.GetComponent<WorldMapNode>();

                    node.Pos = new Vector3(current_x, current_y);

                    list_node_in_chapter.Add(node);
                    current_y += spacing_y;
                }

                foreach(var idx in gameState.journeyPlan.Find(x=>x.name==chapter.name).index_in_chapter)
                {
                    list_node.Add(list_node_in_chapter[idx]);
                }

                current_x += spacing_x;
            }

            /*GameObject node_obj_interlude = GameObject.Instantiate(node_prefab, Vector3.back, Quaternion.identity);

            WorldMapNode node_interludes = node_obj_interlude.GetComponent<WorldMapNode>();

            node_interludes.Pos = new Vector3(current_x, center_y);
            node_interludes.Radius *= 1.0f;
            list_node.Add(node_interludes);
            current_x += spacing_x_short;*/

            GameObject node_obj_boss = GameObject.Instantiate(node_prefab, Vector3.back, Quaternion.identity);

            WorldMapNode node_boss = node_obj_boss.GetComponent<WorldMapNode>();

            node_boss.Pos = new Vector3(current_x, center_y);
            node_boss.Radius *= 1.25f;
            node_boss.InnerColor = new Color(1.0f, 1.0f, 1.0f);
            list_node.Add(node_boss);
            current_x += spacing_x_short;

            GameObject node_obj_base = GameObject.Instantiate(node_prefab, Vector3.back, Quaternion.identity);

            WorldMapNode node_base = node_obj_base.GetComponent<WorldMapNode>();

            node_base.Pos = new Vector3(current_x, center_y);
            node_base.Radius *= 1.0f;
            node_base.InnerColor = new Color(0.5f, 1.0f, 0.5f);
            list_node.Add(node_base);
            current_x += spacing_x;
        }

        destinationNode = list_node[gameState.progress-1];
        desitination_orgCol = destinationNode.InnerColor;


        for (int i = 0; i < gameState.progress-1; i++)
        {
            GameObject edge_obj = GameObject.Instantiate(edge_prefab, Vector3.zero, Quaternion.identity);
            WorldMapEdge edge = edge_obj.GetComponent<WorldMapEdge>();

            edge.from = list_node[i].Pos;
            edge.to = list_node[i + 1].Pos;

            if (i == gameState.progress - 2)
            {
                destinationEdge = edge;
                destinationEdge.t = 0.0f;
            }
        }

        float destination_X = RectTransformUtility.WorldToScreenPoint(Camera.main, destinationNode.Pos).x;

        float offset = destination_X - Screen.width * 3 / 4;

        if (offset > 0.0f)
        {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x+offset, Camera.main.transform.position.y, Camera.main.transform.position.z);
        }

        scenePanel_rectTransform.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
     
    }

    float time = 0.0f;

    enum State
    {
        PROCEED,
        BLINK,
        ZOOMUP,
        WAIT,
        FADEOUT
    }

    State state = State.PROCEED;

    private void SetPanelLocation(float time)
    {
        Vector2 pos = RectTransformUtility.WorldToScreenPoint(Camera.main, destinationNode.Pos);

        float scale = 1080.0f / Screen.height;

        
        float PanelHeight_End = Screen.height * 3 / 4;
        float PanelWidth_End = PanelHeight_End*4/3;

        //float x = pos.x * 1920 / Screen.width;
        float sl = pos.x;
        float st = pos.y;
        float sr = pos.x;
        float sb = pos.y;

        /*float el = 50.0f;
        float et = (Screen.height - 50.0f);
        float er = (Screen.width - 50.0f);
        float eb = 50.0f;*/

        float el = Screen.width/2- PanelWidth_End/2;
        float et = Screen.height/2+ PanelHeight_End/2;
        float er = Screen.width/2+PanelWidth_End / 2;
         float eb = Screen.height / 2 - PanelHeight_End / 2;

        /*float el = pos.x + 32.0f+16.0f;
        float et = pos.y +128.0f/2.0f;
        float er = pos.x + 32.0f+16.0f+128.0f;
        float eb = pos.y -128.0f/ 2.0f;*/

        float l = Mathf.Lerp(sl, el, time);
        float r = Mathf.Lerp(sr, er, time);
        float t = Mathf.Lerp(st, et, time);
        float b = Mathf.Lerp(sb, eb, time);

        scenePanel_rectTransform.anchoredPosition = new Vector2(l, b)* scale;
        scenePanel_rectTransform.sizeDelta = new Vector2(r - l, t - b)* scale;
    }

    Color desitination_orgCol;

    private void BlinkDestination()
    {
        float blink_time = time * 2.0f;

        float f = Mathf.Abs(Unity.Mathematics.math.frac(blink_time) - 0.5f) * 2.0f;

        destinationNode.InnerColor.r = Mathf.Lerp(desitination_orgCol.r, 1.0f, f);
        destinationNode.InnerColor.g = Mathf.Lerp(desitination_orgCol.g, 0.5f, f);
        destinationNode.InnerColor.b = Mathf.Lerp(desitination_orgCol.b, 0.0f, f);
    }
    private void FixedUpdate()
    {
        if (fadecomplete)
        {
            switch(state)
            {
                case State.PROCEED:
                    {
                        if (destinationEdge != null)
                            destinationEdge.t = time / 1.0f;
                        else
                            time = 2.0f;
                    }

                    if (time > 1.0f)
                    {
                        state = State.BLINK;
                        time = 0.0f;
                    }

                    break;
                case State.BLINK:

                    BlinkDestination();

                    if (time > 1.0f)
                    {
                        if (scenePanel_Image.sprite != null)
                        {
                            state = State.ZOOMUP;
                            SetPanelLocation(0.0f);
                            scenePanel_rectTransform.gameObject.SetActive(true);
                        }
                        else
                        {
                            state = State.WAIT;
                        }
                        time = 0.0f;
                    }

                    break;

                case State.ZOOMUP:

                    BlinkDestination();

                    {
                        SetPanelLocation(time);

                    }
                    if (time > 1.0f)
                    {
                        state = State.WAIT;
                        time = 0.0f;
                    }
                    break;
                case State.WAIT:
                    BlinkDestination();
                    SetPanelLocation(1.0f);
                    if (time > 1.0f)
                    {
                        state = State.FADEOUT;
                        StartCoroutine("Blackout");
                        time = 0.0f;
                    }
                    break;
                case State.FADEOUT:
                    BlinkDestination();
                    SetPanelLocation(1.0f);
                    break;
            }

            time += Time.fixedDeltaTime;

            
        }
    }

    IEnumerator Blackin()
    {
        blackout.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float start = Time.time;
        while (Time.time - start < 1.0f || audioSource.isPlaying)
        {
            yield return wait;
            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, 1.0f - ((float)fade / 1.0f));


        }

        fadecomplete = true;
    }

    IEnumerator Blackout()
    {

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float start = Time.time;
        while (Time.time - start < 1.0f || audioSource.isPlaying)
        {
            yield return wait;
            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / 1.0f);


        }

        gameState.destination = gameState.GetNextStage_UpdateSkyIndex().Item1;
        gameState.subDestination_Intermission = GameState.SubDestination_Intermission.FromWorldMap;
        SceneManager.LoadScene("Intermission");
    }
}
