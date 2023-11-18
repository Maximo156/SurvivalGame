using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class Stackable : MonoBehaviour
{
    protected static Dictionary<string, int> names = new Dictionary<string, int>();
    public int id { get; private set; }

    public List<Vector3> Offsets;

    private Rigidbody rbpriv;

    public Rigidbody rb { get {
            if (rbpriv == null)
                rbpriv = GetComponent<Rigidbody>();
            return rbpriv;
        }
        set => rbpriv = value; }

    protected List<Stackable> curStack = new List<Stackable>();

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!names.TryGetValue(name, out var _id))
        {
            names[name] = id = names.Count;
        }
        else
        {
            id = _id;
        }
    }

    public virtual bool Stack(Stackable obj, Transform newParent, bool kinematic = true)
    {
        if (curStack.Count - 1 == Offsets.Count) return false;

        obj.transform.parent = newParent;

        if(obj.rb != null)
            obj.rb.isKinematic = kinematic;
        else
            print("null rb");
        curStack.Add(obj);

        if(curStack.Count > 1)
        {
            var yDir = transform.up.normalized;
            var zDir = transform.forward.normalized;
            var xDir = Vector3.Cross(zDir, yDir).normalized;
            obj.transform.position = transform.position + xDir * Offsets[curStack.Count - 2].x + yDir * Offsets[curStack.Count - 2].y + zDir * Offsets[curStack.Count - 2].z ;
            obj.transform.rotation = transform.rotation;
        }

        return true;
    }

    public virtual void DropAll()
    {
        foreach (var obj in curStack)
        {
            obj.transform.parent = null;
            obj.rb.isKinematic = false;
        }
        curStack.Clear();
    }
    public virtual bool DropOne(bool reset, out Stackable dropped)
    {
        var obj = curStack.LastOrDefault();

        if (obj == null)
        {
            dropped = this;
            return true;
        }

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
