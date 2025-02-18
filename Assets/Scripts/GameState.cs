using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu]
public class GameState : ScriptableObject
{
    public int stage = -1;
    public bool intermission = true;
    public int gold = 2000;
    public List<IntermissionButton.ShopItemWeapon> inventryWeapons = new List<IntermissionButton.ShopItemWeapon>();
    public List<IntermissionButton.ShopItemParts> inventryParts = new List<IntermissionButton.ShopItemParts>();
    public string rightWeapon_name;
    public string shoulderWeapon_name;
    public IntermissionButton.ShopItemWeapon.Type subWeaponType;

    public RobotController.ItemFlag itemFlag;

    public GameObject player_variant;

    public void Reset()
    {
        stage = 1;
        intermission = true;
        gold = 2000;
        inventryWeapons.Clear();
        inventryParts.Clear();
        rightWeapon_name = "";
        shoulderWeapon_name = "";
        itemFlag = 0;
    }
}
