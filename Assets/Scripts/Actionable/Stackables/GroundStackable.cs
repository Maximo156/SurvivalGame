using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GroundStackable : Stackable
{
    public override bool Stack(Stackable obj, Transform newParent, bool kinematic = true)
    {
        if (curStack.Count == 0)
        {
            transform.up = Vector3.up;
            base.Stack(this, null, false);
        }

        bool res = base.Stack(obj, newParent);
        if(res)
            Destroy(obj.GetComponent<Rigidbody>());

        return res;
    }

    public override bool DropOne(bool reset, out Stackable dropped)
    {
        var last = curStack.LastOrDefault();
        if (last != null && last.gameObject.GetComponent<Rigidbody>() == null)
        {
            var rb = last.gameObject.AddComponent<Rigidbody>();
            last.rb = rb;
        }

        var res = base.DropOne(reset, out dropped);

        return res;
    }
}
