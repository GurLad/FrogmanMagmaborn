using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISuspendable<T>
{
    T SaveToSuspendData();

    void LoadFromSuspendData(T data);
}
