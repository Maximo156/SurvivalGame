using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnOnBreakMoveable : Breakable
{
    public int hitsToBreak = 1;

    [Serializable]
    public class Spawnable
    {
        public GameObject obj;
        public Vector3 offset;
        public Quaternion Rotation;
    }
    public List<Spawnable> spawnables;

    private int hitsLeft;

    public void Start()
    {
        hitsLeft = hitsToBreak;
    }

    public override void Break(int numHits = 1)
    {
        if (CanBreak)
        {
            hitsLeft -= numHits;

            if (hitsLeft <= 0)
            {
                foreach (var spawnable in spawnables)
                {
                    Instantiate(spawnable.obj, transform.position + transform.rotation * spawnable.offset, transform.rotation * spawnable.Rotation, null); ;
                }
                Destroy(gameObject);
            }
        }
    }
}
