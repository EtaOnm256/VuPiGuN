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

        public int tier { get; set; }
        public string description { get; set; }
    }
    [System.Serializable]
    public class ShopItemWeapon : ShopItem
    {
        public string name { get; set; }
        public string description { get; set; }
        public int price { get; set; }
        public int tier { get; set; }
        public string prefabname;
        public enum Type
        {
            Main,
            Shoulder,
            Back
        }
        public Type type;
    }

    ShopItemWeapon[] shopItemWeapons =
    {
        new ShopItemWeapon{name = "ビームライフル",description="火力と取り回しを両立したビーム兵器。",price=3500,tier=2,prefabname="BeamRifle Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{name = "バズーカ",description="大口径のロケットランチャー。",price=1500,tier=1,prefabname="Bazooka Variant" ,type=ShopItemWeapon.Type.Main},
        new ShopItemWeapon{name = "ミサイルポッド",description="固定式のミサイル発射装置。",price=2000,tier=1,prefabname="MissilePod Variant",type=ShopItemWeapon.Type.Shoulder },
        new ShopItemWeapon{name = "サブマシンガン",description="小型の機関銃。追加入力で連射可能。",price=1500,tier=1,prefabname="SMG Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{name = "ソリッドライフル",description="コストの安い実弾兵器。",price=1000,tier=1,prefabname="SolidRifle Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{name = "ハンドキャノン",description="手持ち式の榴弾砲。反動が大きい。",price=2000,tier=1,prefabname="HandCannon Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{name = "ドローン",description="子機を射出し、敵を攻撃させる。",price=3000,tier=2,prefabname="DronePlatform Variant",type=ShopItemWeapon.Type.Back },
        new ShopItemWeapon{name = "スプレービームキャノン",description="肩に固定する拡散ビーム砲。",price=2500,tier=2,prefabname="BeamCannon Variant",type=ShopItemWeapon.Type.Shoulder },
    };

    [System.NonSerialized] List<ShopItemWeapon> shopWeapons = new List<ShopItemWeapon>();

    [System.Serializable]
    public class ShopItemParts : ShopItem
    {
        public string name { get; set; }
        public string description { get; set; }
        public int price { get; set; }
        public int tier { get; set; }
        public RobotController.ItemFlag itemFlag;
    }

    ShopItemParts[] shopItemParts =
    {
        new ShopItemParts{name = "ネクストドライブ",description="全ての行動を空中ダッシュでキャンセルできるようになる。",price=4000,tier=3,itemFlag=RobotController.ItemFlag.NextDrive },
        new ShopItemParts{name = "エクストリームスライド",description="全ての行動をステップでキャンセルできるようになる。",price=5000,tier=3,itemFlag=RobotController.ItemFlag.ExtremeSlide },
        new ShopItemParts{name = "グランドブースト",description="ステップ中、ブーストボタンを推し続けるとブーストを消費してダッシュし続ける。",price=1500,tier=2,itemFlag=RobotController.ItemFlag.GroundBoost },
        new ShopItemParts{name = "バーティカルバーニア",description="ステップ中に方向転換できるようになる。また、空中ダッシュ前の旋回時間がなくなる",price=1000,tier=1,itemFlag=RobotController.ItemFlag.VerticalVernier },
        new ShopItemParts{name = "クイックイグナイター",description="ステップと空中ダッシュの初速が速くなる。",price=1000,tier=1,itemFlag=RobotController.ItemFlag.QuickIgniter },
        new ShopItemParts{name = "ホバークラフト",description="地上での硬直中、滑走するようになる。",price=2000,tier=1,itemFlag=RobotController.ItemFlag.Hovercraft },
        new ShopItemParts{name = "フライトユニット",description="空中でもブーストが回復するようになる。",price=2500,tier=2,itemFlag=RobotController.ItemFlag.FlightUnit },
        new ShopItemParts{name = "ローリングショット",description="射撃+横フリックで回転撃ちができるようになる。射撃からキャンセル可能。",price=3000,tier=2,itemFlag=RobotController.ItemFlag.RollingShoot },
        new ShopItemParts{name = "ダッシュスラッシュ",description="格闘+上フリックでダッシュ斬りができるようになる。射撃または格闘からキャンセル可能。",price=3000,tier=2,itemFlag=RobotController.ItemFlag.DashSlash },
        new ShopItemParts{name = "チェインファイア",description="メイン射撃からサブ射撃にキャンセルできるようになる。",price=1000,tier=1,itemFlag=RobotController.ItemFlag.ChainFire },
        new ShopItemParts{name = "イアイスラッシュ",description="メイン射撃から格闘にキャンセルできるようになる。",price=1500,tier=1,itemFlag=RobotController.ItemFlag.IaiSlash },
        new ShopItemParts{name = "クイックドロー",description="格闘からメイン射撃入力で追撃できるようになる。",price=1500,tier=1,itemFlag=RobotController.ItemFlag.QuickDraw},
        new ShopItemParts{name = "リープストライク",description="格闘+下フリックでジャンプ斬りができるようになる。射撃または格闘からキャンセル可能。",price=2500,tier=2,itemFlag=RobotController.ItemFlag.JumpSlash},
        new ShopItemParts{name = "スナイプショット",description="射撃+下フリックで強力な射撃ができるようになる。全ての攻撃からキャンセル可能。",price=2000,tier=2,itemFlag=RobotController.ItemFlag.SnipeShoot},
    };

    [System.NonSerialized] List<ShopItemParts> shopParts = new List<ShopItemParts>();



    List<(GameObject,ShopItem)> shopItemPanel = new List<(GameObject, ShopItem)>();

    List<(GameObject, ShopItem)> inventryItemPanel = new List<(GameObject, ShopItem)>();

    [SerializeField] TextMeshProUGUI goldText;
    public bool LotteryItem<T>(T[] pool,List<T> chosen, int maxTier, int count,List<T> inventry) where T :  ShopItem
    {
        

        //var player_alllist = new List<ShopItemWeapon>();

        for (int tier = maxTier; tier > 0; tier--)
        {
            List<T> remainItem = new List<T>();

            foreach (T item in pool)
            {
                if (item.tier != tier)
                    continue;

                //if ((MovementBase.itemString[item].exclude_class & 1 << SquadManager.player_class_index) != 0)
                //    continue;

                //所持していないアイテムを抽選候補として選択
                if (inventry == null || inventry.Find(x => x.name == item.name) == null)
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

            int chosen_thisTier = 0;

            while (true)
            {
                if (remainItem.Count <= 0)
                    break;

                if (chosen_thisTier >= count)
                    break;

                int chosenIndex = UnityEngine.Random.Range(0, remainItem.Count);

                chosen.Add(remainItem[chosenIndex]);
                chosen_thisTier++;

                var chosenItemFlag = remainItem[chosenIndex];

                remainItem.RemoveAt(chosenIndex);

                // 抽選したアイテムと排他なものは抽選候補から削除
                //foreach (ShopItemWeapon item in shopItemWeapons)
                //{
                //    if (MovementBase.itemString[chosenItemFlag].exclude.HasFlag(item))
                //        remainItem.Remove(item);
                //}


            }
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
            {
                ShopItemParts selectedParts = selectedItem as ShopItemParts;

                gameState.itemFlag |= selectedParts.itemFlag;
                gameState.inventryParts.Add(selectedParts);
            }
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
            {
                gameState.shoulderWeapon_name = value;
                if (value != null)
                    gameState.subWeaponType = selectedweapon.type;
            }
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
        StartCoroutine("Blackin");
        SetupShop();
        SwitchToShop();

        if(gameState.loadingDestination==GameState.LoadingDestination.Intermission_Garage)
        {
            OnClickProceedToGarage();
        }

        Cursor.lockState = CursorLockMode.None;
    }
    void SetupShop()
    {
        LotteryItem<ShopItemWeapon>(shopItemWeapons, shopWeapons, (gameState.stage+1)/2, 2,gameState.inventryWeapons);

        GameObject weaponListPanel = ShopPanel.transform.Find("WeaponListPanel").Find("Viewport").Find("Content").gameObject;

        for (int i = 0; i < shopWeapons.Count; i++)
        {
            AddItemToShopPanel(weaponListPanel, shopWeapons[i]);
        }

        LotteryItem<ShopItemParts>(shopItemParts, shopParts, (gameState.stage+1) / 2, 2, gameState.inventryParts);

        GameObject partsListPanel = ShopPanel.transform.Find("UpgradePartsListPanel").Find("Viewport").Find("Content").gameObject;

        for (int i = 0; i < shopParts.Count; i++)
        {
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

        StartCoroutine("Blackout_MissionStart");
    }

    public void OnClickTestingRoom()
    {
        StartCoroutine("Blackout_TestingRoom");
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

    IEnumerator Blackout_MissionStart()
    {

        var wait = new WaitForSeconds(Time.deltaTime);

        int count = 0;
        while (count++ < 90 || audioSource.isPlaying)
        {
            int fade = System.Math.Max(0, count);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / 90.0f);
            yield return wait;
        }

        gameState.loadingDestination = GameState.LoadingDestination.Mission;

        SceneManager.LoadScene("Loading");
    }

    IEnumerator Blackout_TestingRoom()
    {

        var wait = new WaitForSeconds(Time.deltaTime);

        int count = 0;
        while (count++ < 60 || audioSource.isPlaying)
        {
            int fade = System.Math.Max(0, count);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / 60.0f);
            yield return wait;
        }

        gameState.loadingDestination = GameState.LoadingDestination.TestingRoom;

        SceneManager.LoadScene("Loading");
    }

    IEnumerator Blackin()
    {

        var wait = new WaitForSeconds(Time.deltaTime);

        int count = 60;
        while (count-- >= 0)
        {
            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)count) / 60.0f);
            yield return wait;
        }
    }
}