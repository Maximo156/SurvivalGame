using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Interact))]
public class Break : MonoBehaviour, IInteractAction
{

    public KeyCode Code => KeyCode.None; 

    public int MouseButton => 0; 

    public void InteractWith(GameObject obj)
    {
        obj.GetComponentInParent<Breakable>()?.Break();
    }
}
