using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Holdable : Stackable
{

    public Quaternion Rotation;

    public Vector3 StartOffset;

    public Placeable placeable { get; private set; }

    // Start is called before the first frame update
    protected override void Start()
    {
        placeable = GetComponent<Placeable>();
        base.Start();
    }

    public override bool Stack(Stackable obj, Transform newParent, bool kinematic = true)
    {
        base.Stack(obj, newParent);

        if (curStack.Count == 1)
        {
            obj.transform.localPosition = StartOffset;
            obj.transform.localRotation = Rotation;
        }

        foreach (var placable in obj.GetComponentsInChildren<Placeable>())
        {
            placable.Remove();
        }

        return true;
    }

}
