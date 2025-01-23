// Assets/Scripts/Weapons/WeaponManager.cs
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public WeaponBase[] weapons;
    private int currentWeaponIndex = 0;

    void Start()
    {
        EquipWeapon(0);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            EquipWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            EquipWeapon(1);
        }

        if (Input.GetMouseButtonDown(0))
        {
            weapons[currentWeaponIndex].UseWeapon();
        }
    }

    void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(i == index);
        }

        currentWeaponIndex = index;
    }
}
