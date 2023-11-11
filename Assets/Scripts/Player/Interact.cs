using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IInteractAction
{
    public void InteractWith(GameObject obj);

    public KeyCode Code { get; }

    public int MouseButton { get; }
}

public class Interact : MonoBehaviour
{
    public float reach;
    Camera cam;

    IInteractAction[] actions;
    // Start is called before the first frame update
    void Start()
    {
        actions = GetComponentsInChildren<IInteractAction>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        var frameActions = actions.Where(a => (a.Code != KeyCode.None && Input.GetKeyDown(a.Code)) || (a.MouseButton != -1 && Input.GetMouseButtonDown(a.MouseButton)));
        if(frameActions.Count() != 0 && Physics.Raycast(cam.transform.position, cam.transform.forward, out var hitInfo, reach))
        {
            var hitObject = hitInfo.collider.gameObject;
            foreach(var action in frameActions)
            {
                action.InteractWith(hitObject);
            }
        }
    }
}
