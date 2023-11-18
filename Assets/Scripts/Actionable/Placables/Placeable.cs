using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Placeable : MonoBehaviour
{
    public float SnapDistance = 5;
    public PlacableType type;
    public bool isFoundation = false;

    public Vector3 StartingOffset;
    public Quaternion StartingRot;

    public Material previewMat;

    public GameObject Display;
    public GameObject PlacedDisplay;

    public bool placedHorizontal { get; private set; }

    public enum PlacableType
    {
        Log,
        Plank
    }

    GroundStackable stackable;
    Breakable breakable;

    void Start()
    {
        Hold.onObjectChange += DeletePreview;
        TransformToHoldable();
        stackable = GetComponent<GroundStackable>();
        breakable = GetComponent<Breakable>();
    }

    public void DeletePreview(Holdable _)
    {
        Destroy(preview);
    }

    public bool Place(Camera cam, float distance)
    {
        if (Utilities.GetLookPosition(cam.transform.position, cam.transform.forward, distance, out var HitLocation) && GetPosAndRotation(HitLocation, cam.transform.position, out var info))
        {
            (var loc, var rot, var usedAnchor) = info;
            transform.parent = null;
            transform.position = loc;
            transform.rotation = rot;

            placedHorizontal = usedAnchor?.PlacesHorizontal ?? false;

            foreach (var a in GetComponentsInChildren<PlaceableAnchor>().Where(a => usedAnchor == null || (usedAnchor.PlacesHorizontal  && a.ValidWhenHorizontal ) || (!usedAnchor.PlacesHorizontal && a.ValidWhenVertical)))
            {
                a.Add();
            }
            TransformToPlaced();
            return true;
        }
        return false;
    }

    public void Remove()
    {
        foreach (var anchor in GetComponentsInChildren<PlaceableAnchor>())
        {
            anchor.Remove();
        }
        TransformToHoldable();
    }

    public virtual void TransformToPlaced()
    {
        if (stackable != null)
            stackable.enabled = false;
        breakable?.SetCanBreak(false);
        if (Display != null) Display.SetActive(false);
        if (PlacedDisplay != null) PlacedDisplay.SetActive(true);
    }

    public virtual void TransformToHoldable()
    {
        if (stackable != null)
            stackable.enabled = true;
        breakable?.SetCanBreak(true);
        if (PlacedDisplay != null) PlacedDisplay.SetActive(false);
        if (Display != null) Display.SetActive(true);
    }

    GameObject preview;
    public void Preview(Camera cam, float distance)
    {

        if (Utilities.GetLookPosition(cam.transform.position, cam.transform.forward, distance, out var HitLocation) && GetPosAndRotation(HitLocation, cam.transform.position, out var info))
        {
            (var loc, var rot, _) = info;

            if (preview == null)
            {
                preview = Instantiate(gameObject);
                preview.GetComponent<Placeable>().TransformToPlaced();
                var comps = preview.GetComponentsInChildren<Component>();
                foreach (var component in comps)
                {
                    if (!(component is Transform) && !(component is MeshFilter) && !(component is MeshRenderer))
                    {
                        try
                        {
                            Destroy(component);
                        }
                        catch (Exception) { }
                    }
                    else if (component is MeshRenderer)
                    {
                        (component as MeshRenderer).material = previewMat;
                    }
                }
            }
            preview.transform.position = loc;
            preview.transform.rotation = rot;
        }
        else if (preview != null)
        {
            Destroy(preview);
        }
        
    }

    private bool GetPosAndRotation(Vector3 hitPosition, Vector3 lookPosition, out (Vector3, Quaternion, PlaceableAnchor) placementInfo )
    {
        var closest = PlaceableAnchor.GetClosest(hitPosition, SnapDistance).OrderBy(c => Vector3.Distance(c.transform.position, hitPosition)).ToList();

        placementInfo = default;
        if (closest.Count == 0)
        {
            placementInfo = (hitPosition + StartingOffset, StartingRot, null);
            return isFoundation;
        }

        foreach(var anchor in closest)
        {
            if(!Physics.Raycast(lookPosition, anchor.transform.position - lookPosition, out var info, Vector3.Distance(anchor.transform.position, lookPosition)) && anchor.TryUse(this, out placementInfo))
            {
                return true;
            }
        }

        placementInfo = (hitPosition + StartingOffset, StartingRot, null);
        return isFoundation;
    }
}
