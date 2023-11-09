using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHeight : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var height = GameObject.Find("Island").GetComponent<Island>().WaterLevel;
        transform.position = new Vector3(0, height, 0);
    }
}
