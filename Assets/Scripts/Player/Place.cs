using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hold))]
public class Place : MonoBehaviour
{
    public float placeDistance;
    Camera cam;
    bool build = false;
    Holdable holdable;
    Hold hold;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        hold = GetComponent<Hold>();
        Hold.onObjectChange += GetNewHoldable;
    }

    private void Update()
    {
        if (holdable?.placeable != null)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                holdable?.placeable?.DeletePreview(null);
                build = !build;
            }

            if (build && Input.GetMouseButtonDown(1))
            {
                if(hold.DropOne(false, out var dropped))
                {
                    if(!dropped?.placeable?.Place(cam, placeDistance) ?? true)
                    {
                        hold.StackOne(dropped);
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (build) holdable?.placeable?.Preview(cam, placeDistance);
    }

    private void GetNewHoldable(Holdable obj)
    {
        holdable = obj;
        if (holdable?.placeable == null) build = false;
    }
}
