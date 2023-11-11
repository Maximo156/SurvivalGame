using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Breakable : MonoBehaviour
{
    public bool CanBreak { get; private set; } = true;

    public void SetCanBreak(bool n)
    {
        CanBreak = n;
    }

    public abstract void Break(int hits = 1);
}
