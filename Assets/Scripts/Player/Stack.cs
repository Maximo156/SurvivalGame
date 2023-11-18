using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[RequireComponent(typeof(Hold))]
public class Stack : MonoBehaviour, IInteractAction
{
    public KeyCode Code => KeyCode.None;

    public int MouseButton => 1;

    Hold hold;

    private Place place;

    public void InteractWith(GameObject obj)
    {
        if (!place.build)
        {
            var stack = obj.GetComponentsInParent<GroundStackable>().FirstOrDefault(c => c.enabled);
            if (stack != null && stack.id == hold.currentlyHolding.id)
            {
                hold.DropOne(true, out var newStackable);
                newStackable.GetComponent<GroundStackable>().enabled = false;
                if(!stack.Stack(newStackable, stack.transform))
                {
                    hold.StackOne(newStackable as Holdable);
                }
            }
        }
    }

    void Start()
    {
        place = GetComponent<Place>();
        hold = GetComponent<Hold>();
    }
}
