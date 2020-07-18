using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy
{

    void OnHit(float damage, Vector3 attackerPosition);

    void SetLashing(bool value, float lashCost);
}
