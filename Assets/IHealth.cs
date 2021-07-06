using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth 
{
    public void TakeDamage(float amount);

    public void Heal(float amount);

    public void Kill();
}
