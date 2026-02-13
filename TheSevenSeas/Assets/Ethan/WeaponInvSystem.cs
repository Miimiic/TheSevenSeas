using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WeaponInvSystem : MonoBehaviour
{
    [Header("GunList")]
    public Transform[] weapons;
    [Header("KeyList")]
    public KeyCode[] keys;
    [Header("settings")]
    public float switchTime;
    private int selectedWeapon;
    private float timeSincelastSwitched;




    private void Start()
    {
        SetWeapons();
        selectWeapons(selectedWeapon);
        timeSincelastSwitched = 0f;
    }
    private void SetWeapons() 
    { 
        weapons = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            weapons[i] = transform.GetChild(i);


        }
        if (keys == null)
        {
            keys = new KeyCode[weapons.Length];
        }
    }
    private void Update()
    {
        int previousWeapon = selectedWeapon;

        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]) && timeSincelastSwitched >= switchTime)
            {
                selectedWeapon = i;
            }
        }
        if (previousWeapon != selectedWeapon)
        {
            selectWeapons(selectedWeapon);
            
        }
        timeSincelastSwitched += Time.deltaTime;


    }
    private void selectWeapons(int weaponIndex)
    {
        for (int i = 0; i < weapons.Length; i++) 
        {
            weapons[i].gameObject.SetActive(i== weaponIndex);
        
        
        }
        timeSincelastSwitched = 0f;
        onWeaponSelect();

    }
    void onWeaponSelect()
    {
        print("weaponSwaped");
    }

    


}
