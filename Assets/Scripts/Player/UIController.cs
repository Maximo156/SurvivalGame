using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class UIController : MonoBehaviour
{
    [Serializable]
    public class Controllable
    {
        public GameObject obj;
        public KeyCode toggle;
        public bool stopInteract;
        public bool stopMovement;
        public bool stopRotation;
        public bool startEnabled = false;
        public bool enableMouse = true;
    }

    public List<Controllable> controlables;

    class ControllableEqualityComparer : IEqualityComparer<Controllable>
    {
        public bool Equals(Controllable b1, Controllable b2)
        {
            if (ReferenceEquals(b1, b2))
                return true;

            if (b2 is null || b1 is null)
                return false;

            return b1.toggle == b2.toggle;
        }

        public int GetHashCode(Controllable cont) => cont.GetHashCode();
    }

    private void Start()
    {
        foreach (var controllable in controlables)
        {
            controllable.obj.SetActive(controllable.startEnabled);
        }
        controlables = controlables.Distinct(new ControllableEqualityComparer()).ToList();
    }

    // Update is called once per frame
    void Update()
    {
        var controllable = controlables.FirstOrDefault(c => Input.GetKeyDown(c.toggle));
        if(controllable != null)
        {
            controllable.obj.SetActive(!controllable.obj.activeSelf);

            LockableInput.Interact = !controllable.obj.activeSelf || !controllable.stopInteract;
            LockableInput.Movement = !controllable.obj.activeSelf || !controllable.stopMovement;
            LockableInput.Rotation = !controllable.obj.activeSelf || !controllable.stopRotation;

            EnableMouse(controllable.obj.activeSelf && controllable.enableMouse);

            foreach(var disable in controlables.Where(c => c!= controllable))
            {
                disable.obj.SetActive(false);
            }
        }
    }

    public void EnableMouse(bool enable)
    {
        Cursor.lockState = enable ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = enable;
    }
}
