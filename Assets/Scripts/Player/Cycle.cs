using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cycle : MonoBehaviour, IInteractAction
{
    public KeyCode Code => KeyCode.None;

    public int MouseButton => 1;

    private Place place;

    void Start()
    {
        place = GetComponent<Place>();
    }

    public void InteractWith(GameObject obj)
    {
        if(!place.build)
            obj.GetComponentInParent<Cycleable>()?.Cycle();
    }
}
