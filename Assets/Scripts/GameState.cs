using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu]
public class GameState : ScriptableObject
{
    public int stage = -1;
    public int gold = 7500;
    public List<IntermissionButton.ShopItemWeapon> inventryWeapons = new List<IntermissionButton.ShopItemWeapon>();
    public string rightWeapon_name;
    public string shoulderWeapon_name;

    public GameObject player_variant;
}
