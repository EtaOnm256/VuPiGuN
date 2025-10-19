using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkySwitcher : MonoBehaviour
{
    [System.Serializable]
    class Variant
    {
        public Material skybox;
        public Color fogColor;
        public Color ambientColor;

        public List<Light> lights;
    }

    [SerializeField] List<Variant> variants;
    [SerializeField] GameState gameState;

    private void Awake()
    {
        int idx = gameState.skyindex;

        RenderSettings.skybox = variants[idx].skybox;
        RenderSettings.fogColor = variants[idx].fogColor;
        RenderSettings.ambientLight = variants[idx].ambientColor;

        foreach(var variant in variants)
        foreach(var light in variant.lights)
        {
            light.enabled = variant == variants[idx];
        }
    }
 
}
