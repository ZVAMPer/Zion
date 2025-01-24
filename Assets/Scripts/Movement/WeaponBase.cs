// Assets/Scripts/Weapons/WeaponBase.cs
using Unity.Netcode;
using UnityEngine;

public abstract class WeaponBase : NetworkBehaviour
{
    public string weaponName;
    public float knockMeterIncrement;
    public float knockbackForce;
    public float range;

    public abstract void UseWeapon();
}
