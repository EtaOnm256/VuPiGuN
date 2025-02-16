using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class IntermissionButton : MonoBehaviour
{
    [SerializeField] GameState gameState;
    [SerializeField] Image blackout;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip audioClip_Start;

    [SerializeField] GameObject ShopPanel;
    [SerializeField] GameObject GaragePanel;

    [SerializeField] GameObject item_prefab;

    TextMeshProUGUI descriptionText;
    GameObject buyOrEquippedButtonObj;

    public interface ShopItem
    {
        public string name { get; set; }
        public int price { get; set; }
        public string description { get; set; }
    }
    [System.Serializable]
    public class ShopItemWeapon : ShopItem
    {
        public string name { get; set; }
        public string description { get; set; }
        public int price { get; set; }
        public string prefabname;
        public enum Type
        {
            Main,
            Shoulder
        }
        public Type type;
    }

    ShopItemWeapon[] shopItemWeapons =
    {
        new ShopItemWeapon{name = "ビームライフル1",description="火力と取り回しを両立したビーム兵器。",price=2500,prefabname="BeamRifle Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{name = "バズーカ1",description="実体弾を発射する両手持ちの火器。",price=1000,prefabname="Bazooka Variant" ,type=ShopItemWeapon.Type.Main},
        new ShopItemWeapon{name = "ミサイルポッド1",description="固定式のミサイル発射装置。",price=1500,prefabname="MissilePod Variant",type=ShopItemWeapon.Type.Shoulder },
    };

    [System.NonSerialized] List<ShopItemWeapon> shopWeapons = new List<ShopItemWeapon>();

    [System.Serializable]
    public class ShopItemParts : ShopItem
    {
        public string name { get; set; }
        public string description { get; set; }
        public int price { get; set; }
        public RobotController.ItemFlag itemFlag;
    }

    ShopItemParts[] shopItemParts =
    {
        new ShopItemParts{name = "ネクストドライブ",description="全ての行動を空中ダッシュでキャンセルできるようになる。",price=5000,itemFlag=RobotController.ItemFlag.NextDrive },
        new ShopItemParts{name = "フライトユニット",description="空中でダッシュも上昇もしていない間は、ブーストが回復するようになる。",price=2500,itemFlag=RobotController.ItemFlag.FlightUnit },
        new ShopItemParts{name = "ホバークラフト",description="地上での硬直中、滑走するようになる。",price=1500,itemFlag=RobotController.ItemFlag.Hovercraft },
    };

    [System.NonSerialized] List<ShopItemParts> shopParts = new List<ShopItemParts>();



    List<(GameObject,ShopItem)> shopItemPanel = new List<(GameObject, ShopItem)>();

    List<(GameObject, ShopItem)> inventryItemPanel = new List<(GameObject, ShopItem)>();

    [SerializeField] TextMeshProUGUI goldText;
    public bool LotteryItem<T>(T[] pool,List<T> chosen, int rarity, int count,List<T> inventry) where T :  ShopItem
    {
        List<T> remainItem = new List<T>();

        //var player_alllist = new List<ShopItemWeapon>();

        foreach (T item in pool)
        {
            //if (MovementBase.itemString[item].rarity > rarity)
            //    continue;

            //if ((MovementBase.itemString[item].exclude_class & 1 << SquadManager.player_class_index) != 0)
            //    continue;

            //所持していないアイテムを抽選候補として選択
            if (inventry == null || inventry.Find(x=>x.name == item.name) == null)
            {
                remainItem.Add(item);
            }
            //else
            //    player_alllist.Add(item);
        }

        //所持アイテムと排他なものは抽選候補から削除
        //foreach (Infantry.ItemFlags playeritem in player_alllist)
        //{
        //    foreach (Infantry.ItemFlags item_target in System.Enum.GetValues(typeof(Infantry.ItemFlags)))
        //    {
        //        if (MovementBase.itemString[playeritem].exclude.HasFlag(item_target))
        //            remainItem.Remove(item_target);
        //   }
        //}



        while (true)
        {
            if (remainItem.Count <= 0)
                break;

            if (chosen.Count >= count)
                break;

            int chosenIndex = UnityEngine.Random.Range(0, remainItem.Count);

            chosen.Add(remainItem[chosenIndex]);

            var chosenItemFlag = remainItem[chosenIndex];

            remainItem.RemoveAt(chosenIndex);

            // 抽選したアイテムと排他なものは抽選候補から削除
            //foreach (ShopItemWeapon item in shopItemWeapons)
            //{
            //    if (MovementBase.itemString[chosenItemFlag].exclude.HasFlag(item))
            //        remainItem.Remove(item);
            //}


        }

        return chosen.Count > 0;
    }

    public void AddItemToShopPanel<T>(GameObject containerPanel, T item) where T: ShopItem
    {
        GameObject itemPanel = Instantiate(item_prefab, containerPanel.transform);
        itemPanel.GetComponent<Button>().onClick.AddListener(()=> { SetSelect_Shop(item, itemPanel); });

        itemPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.name;
        itemPanel.transform.Find("PriceOrEquipped").GetComponent<TextMeshProUGUI>().text = "$"+item.price.ToString();

        shopItemPanel.Add((itemPanel, item)); 
    }

    bool IsEquipped(ShopItem item)
    {
        if (item is ShopItemWeapon)
        {
            ShopItemWeapon weapon = item as ShopItemWeapon;

           

            if (weapon.type == ShopItemWeapon.Type.Main)
                return gameState.rightWeapon_name == weapon.prefabname;
            else
                return gameState.shoulderWeapon_name == weapon.prefabname;
        }
        else
        {
            ShopItemParts parts = item as ShopItemParts;

            return gameState.itemFlag.HasFlag(parts.itemFlag);
        }
        
    }

    public void AddItemToGaragePanel<T>(GameObject containerPanel, T item) where T : ShopItem
    {
        GameObject itemPanel = Instantiate(item_prefab, containerPanel.transform);
        itemPanel.GetComponent<Button>().onClick.AddListener(() => { SetSelect_Garage(item, itemPanel); });

        itemPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.name;

        if (IsEquipped(item))
            itemPanel.transform.Find("PriceOrEquipped").GetComponent<TextMeshProUGUI>().text = "装備中";
        else
            itemPanel.transform.Find("PriceOrEquipped").GetComponent<TextMeshProUGUI>().text = "";
      

        inventryItemPanel.Add((itemPanel,item));
    }

    public void SetSelect_Shop<T>(T selectedItem,GameObject selectedItemPanel) where T:ShopItem
    {
   

        foreach( var itemPair in shopItemPanel)
        {
            Image image = itemPair.Item1.GetComponent<Image>();

            if (itemPair.Item1 == selectedItemPanel)
            {
                image.color = new Color(0.627451f, 0.627451f, 0.0f, 0.75f);
                descriptionText.text = selectedItem.description;
                buyOrEquippedButtonObj.SetActive(true);
                buyOrEquippedButtonObj.GetComponent<Button>().onClick.RemoveAllListeners();
                buyOrEquippedButtonObj.GetComponent<Button>().onClick.AddListener(() => { BuyItem(selectedItem, selectedItemPanel); });
            }
            else
                image.color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
        }

       if(selectedItem==null)
        {
            descriptionText.text = "";
            buyOrEquippedButtonObj.SetActive(false);
        }
    }

    public void SetSelect_Garage<T>(T selectedItem, GameObject selectedItemPanel) where T : ShopItem
    {
        foreach (var itemPair in inventryItemPanel)
        {
            Image image = itemPair.Item1.GetComponent<Image>();

            if (itemPair.Item1 == selectedItemPanel)
            {
                image.color = new Color(0.627451f, 0.627451f, 0.0f, 0.75f);
                descriptionText.text = selectedItem.description;
                buyOrEquippedButtonObj.SetActive(true);

                if (IsEquipped(itemPair.Item2))
                    buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "外す";
                else
                    buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "装備";

                buyOrEquippedButtonObj.GetComponent<Button>().onClick.RemoveAllListeners();
                buyOrEquippedButtonObj.GetComponent<Button>().onClick.AddListener(() => { EquipItem(selectedItem, selectedItemPanel); });
            }
            else
                image.color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
        }

        if (selectedItem == null)
        {
            descriptionText.text = "";
            buyOrEquippedButtonObj.SetActive(false);
        }
    }


    void BuyItem<T>(T selectedItem, GameObject selectedItemPanel) where T : ShopItem
    {
        if (gameState.gold >= selectedItem.price)
        {
            shopItemPanel.Remove((selectedItemPanel, selectedItem));
            GameObject.Destroy(selectedItemPanel);

            gameState.gold -= selectedItem.price;
            goldText.text = $"${gameState.gold.ToString()}";

            if (selectedItem is ShopItemWeapon)
                gameState.inventryWeapons.Add(selectedItem as ShopItemWeapon);
            else
                gameState.inventryParts.Add(selectedItem as ShopItemParts);
        }
    }

    void EquipItem<T>(T selectedItem, GameObject selectedItemPanel) where T : ShopItem
    {
        if (selectedItem is ShopItemWeapon)
        {
            ShopItemWeapon selectedweapon = selectedItem as ShopItemWeapon;

            string value;

            if (IsEquipped(selectedweapon))
            {
                value = null;
                buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "装備";
            }
            else
            {
                value = selectedweapon.prefabname;
                buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "外す";
            }

            if (selectedweapon.type == ShopItemWeapon.Type.Main)
                gameState.rightWeapon_name = value;
            else
                gameState.shoulderWeapon_name = value;
        }
        else
        {
            ShopItemParts selectedparts = selectedItem as ShopItemParts;

    
            if (IsEquipped(selectedparts))
            {
                gameState.itemFlag &= ~selectedparts.itemFlag;
                buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "装備";
            }
            else
            {
                gameState.itemFlag |= selectedparts.itemFlag;
                buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "外す";
            }
        }

       

        foreach (var itemPanel in inventryItemPanel)
        {
            if (IsEquipped(itemPanel.Item2))
                itemPanel.Item1.transform.Find("PriceOrEquipped").GetComponent<TextMeshProUGUI>().text = "装備中";
            else
                itemPanel.Item1.transform.Find("PriceOrEquipped").GetComponent<TextMeshProUGUI>().text = "";
        }


        //SwitchToGarage();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupShop();
        SwitchToShop();
    }
    void SetupShop()
    {
        LotteryItem<ShopItemWeapon>(shopItemWeapons, shopWeapons, 0, 3,gameState.inventryWeapons);

        GameObject weaponListPanel = ShopPanel.transform.Find("WeaponListPanel").Find("Viewport").Find("Content").gameObject;

        for (int i = 0; i < 3; i++)
        {
            if (i < shopWeapons.Count)
                AddItemToShopPanel(weaponListPanel, shopWeapons[i]);
        }

        LotteryItem<ShopItemParts>(shopItemParts, shopParts, 0, 3, gameState.inventryParts);

        GameObject partsListPanel = ShopPanel.transform.Find("UpgradePartsListPanel").Find("Viewport").Find("Content").gameObject;

        for (int i = 0; i < 3; i++)
        {
            if (i < shopParts.Count)
                AddItemToShopPanel(partsListPanel, shopParts[i]);
        }

        goldText.text = $"${gameState.gold.ToString()}";
    }
    void SwitchToShop()
    {
        GameObject weaponListPanel = ShopPanel.transform.Find("WeaponListPanel").Find("Viewport").Find("Content").gameObject;
        GameObject partsListPanel = ShopPanel.transform.Find("UpgradePartsListPanel").Find("Viewport").Find("Content").gameObject;

        descriptionText = ShopPanel.transform.Find("SelectedItemPanel").Find("Description").GetComponent<TextMeshProUGUI>();
        buyOrEquippedButtonObj = ShopPanel.transform.Find("SelectedItemPanel").Find("BuyOrEquipButton").gameObject;

        SetSelect_Shop<ShopItemWeapon>(null, weaponListPanel);
        SetSelect_Shop<ShopItemParts>(null, partsListPanel);
    }

    void SwitchToGarage()
    {

        inventryItemPanel.Clear();

        GameObject weaponListPanel = GaragePanel.transform.Find("WeaponListPanel").Find("Viewport").Find("Content").gameObject;

        for (int i = 0; i < weaponListPanel.transform.childCount; i++)
            GameObject.Destroy(weaponListPanel.transform.GetChild(i).gameObject);


        for (int i = 0; i < gameState.inventryWeapons.Count; i++)
        {
            AddItemToGaragePanel(weaponListPanel, gameState.inventryWeapons[i]);
        }



        GameObject partsListPanel = GaragePanel.transform.Find("UpgradePartsListPanel").Find("Viewport").Find("Content").gameObject;

        for (int i = 0; i < partsListPanel.transform.childCount; i++)
            GameObject.Destroy(partsListPanel.transform.GetChild(i).gameObject);

        for (int i = 0; i < gameState.inventryParts.Count; i++)
        {
            AddItemToGaragePanel(partsListPanel, gameState.inventryParts[i]);
        }

        descriptionText = GaragePanel.transform.Find("SelectedItemPanel").Find("Description").GetComponent<TextMeshProUGUI>();
        buyOrEquippedButtonObj = GaragePanel.transform.Find("SelectedItemPanel").Find("BuyOrEquipButton").gameObject;

        SetSelect_Garage<ShopItemWeapon>(null, weaponListPanel);
        SetSelect_Garage<ShopItemParts>(null, partsListPanel);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnClickMissionStart()
    {
        audioSource.PlayOneShot(audioClip_Start);

        StartCoroutine("Blackout");
    }

    public void OnClickBackToBuild()
    {
        ShopPanel.SetActive(true);
        GaragePanel.SetActive(false);

        SwitchToShop();
    }

    public void OnClickProceedToGarage()
    {
        ShopPanel.SetActive(false);
        GaragePanel.SetActive(true);

        SwitchToGarage();
    }

    IEnumerator Blackout()
    {

        var wait = new WaitForSeconds(Time.deltaTime);

        int count = 0;
        while (count++ < 90 || audioSource.isPlaying)
        {
            int fade = System.Math.Max(0, count);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / 90.0f);
            yield return wait;
        }

        SceneManager.LoadScene("Loading");
    }
}