using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Break : MonoBehaviour
{
    public float hitDistance;
    Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(Utilities.RaycastForType<Breakable>(cam.transform.position, cam.transform.forward, hitDistance, out var obj))
            {
                obj.Break();
            }
        }
    }
}
