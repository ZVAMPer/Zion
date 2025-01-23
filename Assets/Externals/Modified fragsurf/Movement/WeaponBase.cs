// Assets/Scripts/Weapons/WeaponBase.cs
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public string weaponName;
    public float knockMeterIncrement;
    public float knockbackForce;
    public float range;

    public abstract void UseWeapon();
}
