using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Army : ScriptableObject
{
    public enum Type
    {
        Sequence,
        Group
    }

    public Type type;

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

    [System.Serializable]
    public class UnitGroup
    {
        public GameObject variant;
        //public Vector3 pos;
        //public Quaternion rot;
        public int count;

        [System.Serializable]
        public class Condition
        {
            public enum Type
            {
                None,
                PowerLessThan
            }

            public Type type;

            public float param;
        }

        public Condition condition;

        public int reinforce_count = 0;

        public bool boss = false;
    }

    public List<UnitGroup> groups;

    public int power;  
}
