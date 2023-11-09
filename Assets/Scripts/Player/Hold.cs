using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Hold : MonoBehaviour
{
    public delegate void OnObjectChange(Holdable newObj);
    public static OnObjectChange onObjectChange;

    public float grabDistance;

    Holdable currentlyHolding;
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Utilities.RaycastForType<Holdable>(cam.transform.position, cam.transform.forward, grabDistance, out var obj) && obj.transform.parent != transform)
            {
                StackOne(obj);
            }
        }
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
