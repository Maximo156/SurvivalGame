using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Holdable : MonoBehaviour
{
    static Dictionary<string, int> names = new Dictionary<string, int>();
    public int id { get; private set; }

    private List<Holdable> curStack = new List<Holdable>();

    public Quaternion Rotation;

    public Vector3 StartOffset;

    public List<Vector3> Offsets;

    private Rigidbody rb;

    public Placeable placeable { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        placeable = GetComponent<Placeable>();
        rb = GetComponent<Rigidbody>();
        if(!names.TryGetValue(name, out var _id))
        {
            names[name] = id = names.Count;
        }
        else
        {
            id = _id;
        }
    }

    public bool Stack(Holdable obj, Transform newParent)
    {
        if (curStack.Count - 1 == Offsets.Count) return false;

        obj.transform.parent = newParent;

        obj.rb.isKinematic = true;
        curStack.Add(obj);
        if (curStack.Count == 1)
        {
            obj.transform.localPosition = StartOffset;
            obj.transform.localRotation = Rotation;
        }
        else
        {
            obj.transform.localPosition = StartOffset + Offsets[curStack.Count - 2];
            obj.transform.rotation = transform.rotation;
        }

        foreach (var placable in obj.GetComponentsInChildren<Placeable>())
        {
            placable.Remove();
        }

        return true;
    }

    public void DropAll()
    {
        foreach(var obj in curStack)
        {
            obj.transform.parent = null;
            obj.rb.isKinematic = false;
        }
        curStack.Clear();
    }

    public bool DropOne(bool reset, out Holdable dropped)
    {
        var obj = curStack.Last();
        obj.transform.parent = null;
        if (reset)
        {
            obj.rb.isKinematic = false;
        }
        dropped = obj;
        curStack.Remove(obj);
        return curStack.Count == 0;
    }
}
