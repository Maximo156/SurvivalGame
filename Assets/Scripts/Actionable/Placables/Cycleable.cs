using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cycleable : MonoBehaviour
{
    public bool cycleHorizontal;
    int curChild = 0;
    Placeable placeable;
    // Start is called before the first frame update
    void OnEnable()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        for (int i = 1; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void Start()
    {
        placeable = GetComponentInParent<Placeable>();
    }

    public void Cycle()
    {
        if (placeable.placedHorizontal == cycleHorizontal)
        {
            transform.GetChild(curChild).gameObject.SetActive(false);

            curChild = (curChild + 1) % transform.childCount;

            transform.GetChild(curChild).gameObject.SetActive(true);
        }
    }
}
