using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Interact))]
public class Hold : MonoBehaviour, IInteractAction
{
    public delegate void OnObjectChange(Holdable newObj);
    public static OnObjectChange onObjectChange;

    Holdable currentlyHolding;

    public KeyCode Code => KeyCode.E;

    public int MouseButton => -1;

    public void InteractWith(GameObject obj)
    {
        var h = obj.GetComponentInParent<Holdable>();
        if (h != null && obj.transform.parent != transform)
            StackOne(h);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropOne(true, out _);
        }
    }

    void NewHoldable()
    {
        currentlyHolding?.DropAll();
        currentlyHolding = null;
    }

    public bool DropOne(bool reset, out Holdable holdable)
    {
        holdable = null;
        bool empty = currentlyHolding?.DropOne(reset, out holdable) ?? true;
        if (empty)
        {
            currentlyHolding = null;
            onObjectChange(currentlyHolding);
        }
        return holdable != null;
    }

    public void StackOne(Holdable obj)
    {
        if (obj.id != currentlyHolding?.id)
        {
            NewHoldable();
            currentlyHolding = obj;
            onObjectChange(currentlyHolding);
        }
        currentlyHolding.Stack(obj, transform);
    }
}
