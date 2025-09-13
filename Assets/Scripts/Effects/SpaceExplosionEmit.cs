using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceExplosionEmit : MonoBehaviour
{
    [SerializeField]GameObject spaceExplosion_prefab = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 pos = Quaternion.Euler(Random.Range(-90.0f, 30.0f), Random.Range(0, 360.0f),0.0f)*Vector3.forward * 500;
        float scale = Random.Range(2.0f, 10.0f);
        float angle = Random.Range(0.0f, 360.0f);



        var obj = GameObject.Instantiate(spaceExplosion_prefab, Camera.main.transform.position+pos, Quaternion.identity);

        obj.GetComponent<Billboard>().relativePos = pos;
        obj.GetComponent<Billboard>().angle = angle;
        obj.GetComponent<Billboard>().transform.localScale = Vector3.one*scale;
    }
}
