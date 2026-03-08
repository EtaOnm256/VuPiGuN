using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusterBeamRenderer : MonoBehaviour
{
    [SerializeField] GameObject cylinder_prefab;
    [SerializeField] GameObject cap_prefab;

    GameObject cylinder;
    GameObject cap_front;
    GameObject cap_end;

    Vector3 base_cylinder_scale;
    Vector3 base_cap_scale;

    public float widthMultiplier = 100.0f; //base_cap_scale外のAwakeで使われるので、Awakeでbase_cap_scaleから初期化するのは不安がある

    private void OnEnable()
    {
        //cylinderはUpdateRenderingの中で
        cap_front.SetActive(true);
        cap_end.SetActive(true);

        UpdateRendering();
    }

    private void OnDisable()
    {
        cylinder.SetActive(false);
        cap_front.SetActive(false);
        cap_end.SetActive(false);
    }

    private void Awake()
    {
        cylinder = GameObject.Instantiate(cylinder_prefab);
        base_cylinder_scale = cylinder.transform.localScale;
        cap_front = GameObject.Instantiate(cap_prefab);
        cap_end = GameObject.Instantiate(cap_prefab);
        base_cap_scale = cap_front.transform.localScale;
    }

    Vector3[] pos = new Vector3[2];

    void UpdateRendering()
    {
        if (pos[0] == pos[1])
        {
            cylinder.SetActive(false);
            cap_front.transform.position = pos[0];
            cap_front.transform.rotation = Quaternion.identity;
            cap_end.transform.position = pos[0];
            cap_end.transform.rotation = Quaternion.Euler(0.0f,180.0f,0.0f);
        }
        else
        {
            cylinder.SetActive(true);

            Vector3 center = (pos[0] + pos[1]) / 2.0f;
            Vector3 dir = pos[1] - pos[0];
            float length = (pos[1] - pos[0]).magnitude;

            Quaternion lookQ = Quaternion.LookRotation(dir, Vector3.up);

            cylinder.transform.position = center;
            cylinder.transform.rotation = lookQ;
            cylinder.transform.localScale = new Vector3(widthMultiplier, widthMultiplier, base_cylinder_scale.z*length*0.5f);

            cap_front.transform.position = pos[0];
            cap_front.transform.rotation = lookQ;
            cap_front.transform.localScale = Vector3.one*widthMultiplier;
            cap_end.transform.position = pos[1];
            cap_end.transform.rotation = lookQ * Quaternion.Euler(0.0f, 180.0f, 0.0f);
            cap_end.transform.localScale = Vector3.one * widthMultiplier;
        }
    }

    public void SetPosition(int idx,Vector3 newpos)
    {
        if (idx > 1)
            throw new System.NotSupportedException();

        pos[idx] = newpos;

        UpdateRendering();
    }
    
    public Vector3 GetPosition(int idx)
    {
        if (idx > 1)
            throw new System.NotSupportedException();

        return pos[idx];
    }
}
