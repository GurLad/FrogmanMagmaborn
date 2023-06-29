using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnitListener
{
    void OnSpawn();
    void OnHit();
    void OnMiss();
    void OnDamaged();
    void OnBlocked();
    void OnDodged();
}
