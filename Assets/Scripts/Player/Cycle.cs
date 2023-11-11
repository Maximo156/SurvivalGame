using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cycle : MonoBehaviour, IInteractAction
{
    public KeyCode Code => KeyCode.None;

    public int MouseButton => 1;

    public void InteractWith(GameObject obj)
    {
        obj.GetComponentInParent<Cycleable>()?.Cycle();
    }
}
