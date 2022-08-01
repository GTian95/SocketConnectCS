using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IObserver
{
    public abstract void Notify(byte[] data, int len);
}
