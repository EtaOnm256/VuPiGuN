using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Army : ScriptableObject
{
    [System.Serializable]
    public class OneSpawn
    {
        public GameObject variant;
        //public Vector3 pos;
        //public Quaternion rot;
        public int squadCount;
        public bool loop = false;
        public bool burst = false; // 0ëÃÇ…Ç»ÇÈÇ‹Ç≈ÉXÉ|Å[ÉìÇ≥ÇπÇ»Ç¢
        public bool boss = false;
    }

    public List<OneSpawn> spawns;
    public int power;  
}
