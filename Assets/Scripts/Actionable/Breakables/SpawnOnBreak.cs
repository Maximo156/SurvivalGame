using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawnOnBreak : Breakable
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

    private static Dictionary<Vector3, int> hitsLeft = new Dictionary<Vector3, int>();

    public void Start()
    {
        if(!hitsLeft.ContainsKey(transform.position))
            hitsLeft[transform.position] = hitsToBreak;
    }

    public override void Break(int numHits = 1)
    {
        hitsLeft[transform.position] -= numHits;

        if(hitsLeft[transform.position] <= 0)
        {
            foreach(var spawnable in spawnables)
            {
                Instantiate(spawnable.obj, transform.position + transform.rotation * spawnable.offset, transform.rotation * spawnable.Rotation, null);
            }
            ObjectManager.RemoveObject(transform.position);
            hitsLeft.Remove(transform.position);
        }
    }
}
