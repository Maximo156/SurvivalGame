using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PlaceableAnchor : MonoBehaviour, Trackable
{
    public bool PlacesHorizontal;

    public bool ValidWhenHorizontal;

    [Serializable]
    public class PlaceableTypeInfo
    {
        public Placeable.PlacableType type;
        public Vector3 Offset;
        public Quaternion Rotation;
    }

    public List<PlaceableTypeInfo> AcceptableTypes;

    private Dictionary<Placeable.PlacableType, PlaceableTypeInfo> AcceptableTypesDict = new Dictionary<Placeable.PlacableType, PlaceableTypeInfo>();

    private static Quadtree<PlaceableAnchor> instances;

    public void Start()
    {
        if (instances == null)
        {
            var island = GameObject.Find("Island").GetComponent<Island>();
            var bounds = new Rect(Vector2.one * -island.islandWidth / 2, Vector2.one * island.islandWidth);

            instances = new Quadtree<PlaceableAnchor>(bounds);
        }

        foreach(var t in AcceptableTypes)
        {
            AcceptableTypesDict[t.type] = t;
        }
    }

    public void Add()
    {
        instances.Insert(this);
    }

    public void Remove()
    {
        instances.Remove(this);
    }

    public bool TryUse(Placeable placed, out (Vector3, Quaternion, PlaceableAnchor) info)
    {
        info = default;
        if(AcceptableTypesDict.TryGetValue(placed.type, out var typeInfo))
        {
            info = ( transform.rotation * typeInfo.Offset + transform.position, transform.rotation * typeInfo.Rotation, this);
            return true;
        }
        else
        {
            return false;
        }
    }

    public static List<PlaceableAnchor> GetClosest(Vector3 pos, float dist)
    {
        return instances?.FindClosest(Utilities.Vector3ToXZ(pos), dist).ToList();
    }

    public Vector2 Location()
    {
        return Utilities.Vector3ToXZ(transform.position);
    }
}
