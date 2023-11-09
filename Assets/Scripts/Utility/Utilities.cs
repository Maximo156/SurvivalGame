using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    public static bool RaycastForType<T>(Vector3 pos, Vector3 dir, float reach, out T hitObject)
    {
        if(Physics.Raycast(pos, dir, out var hitInfo) && hitInfo.distance < reach)
        {
            hitObject = hitInfo.collider.GetComponentInParent<T>();
            return hitObject != null;
        }
        hitObject = default;
        return false;
    }

    public static bool GetLookPosition(Vector3 pos, Vector3 dir, float reach, out Vector3 position)
    {
        if (Physics.Raycast(pos, dir, out var hitInfo) && hitInfo.distance < reach)
        {
            position = hitInfo.point;
            return true;
        }
        position = Vector3.zero;
        return false;
    }

    public static Vector2 Vector3ToXZ(Vector3 input)
    {
        return new Vector2(input.x, input.z);
    }
}
