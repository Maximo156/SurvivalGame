using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cycleable : MonoBehaviour
{
    int curChild = 0;
    // Start is called before the first frame update
    void Awake()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        for (int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void Cycle()
    {
        transform.GetChild(curChild).gameObject.SetActive(false);

        curChild = ( curChild + 1 ) % transform.childCount;

        transform.GetChild(curChild).gameObject.SetActive(true);
    }
}
