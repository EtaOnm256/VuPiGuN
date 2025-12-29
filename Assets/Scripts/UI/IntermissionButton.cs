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
    GameObject buyOrEquippedButtonObj_Garage;

   
    [SerializeField] GameObject backToShopButton;
    [SerializeField] TextMeshProUGUI departureText;

    [SerializeField] Button departureButton_Garage;
    [SerializeField] Button departureButton_Shop;
    [SerializeField] Button testButton;
    [SerializeField] TextMeshProUGUI garageText;
    [SerializeField] TextMeshProUGUI toGarageText;
    [SerializeField] TextMeshProUGUI shopText;
    [SerializeField] TextMeshProUGUI toShopText;
    [SerializeField] TextMeshProUGUI buyOrEquippedButtonText_Shop;
    [SerializeField] TextMeshProUGUI goldLabel;
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
        new ShopItemWeapon{tier=1,name = "バズーカ",description="大口径のロケットランチャー。",price=1500,prefabname="Bazooka Variant" ,type=ShopItemWeapon.Type.Main},
        new ShopItemWeapon{tier=1,name = "ミサイルポッド",description="サブ武器。固定式のミサイル発射装置。ボタン押し続けで連射可能。",price=2000,prefabname="MissilePod Variant",type=ShopItemWeapon.Type.Shoulder },
        new ShopItemWeapon{tier=1,name = "サブマシンガン",description="小型の機関銃。追加入力で連射可能。",price=1500,prefabname="SMG Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=1,name = "ソリッドライフル",description="コストの安い実弾兵器。",price=1000,prefabname="SolidRifle Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=1,name = "ハンドキャノン",description="手持ち式の榴弾砲。反動が大きい。",price=2000,prefabname="HandCannon Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=2,name = "ビームライフル",description="火力と取り回しを両立したビーム兵器。",price=5000,prefabname="BeamRifle Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=2,name = "ドローン",description="サブ武器。子機を射出し、敵を攻撃させる。ボタン押し続けで追加射出。",price=5500,prefabname="DronePlatform Variant",type=ShopItemWeapon.Type.Back },
        new ShopItemWeapon{tier=2,name = "スプレービームポッド",description="サブ武器。肩に固定する拡散ビーム砲。",price=4500,prefabname="BeamSprayCannon",type=ShopItemWeapon.Type.Shoulder },
        new ShopItemWeapon{tier=2,name = "スナイパーライフル",description="弾速が速く、長距離射撃に向いた実弾兵器。",price=4000,prefabname="SniperRifle Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=2,name = "クラスターランチャー",description="敵前で散弾をばらまく弾頭を発射する。",price=4500,prefabname="Cluster Launcher" ,type=ShopItemWeapon.Type.Main},
        new ShopItemWeapon{tier=2,name = "ガトリングキャノン",description="手持ちの機関砲。追加入力で連射可能。",price=5000,prefabname="GatlingCannon",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "メガビームランチャー",description="大型のビーム砲。",price=9000,prefabname="BeamLauncher",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "ビームマグナム",description="火力を上げたビームライフル。発射後の制動が長い。",price=8500,prefabname="BeamMagnum",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "ビームサブマシンガン",description="小型化を実現したビーム兵器。追加入力で連射可能。",price=8500,prefabname="BeamMachineGun",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "バスターライフル",description="高出力のビームを照射する手持ち式の火器。",price=9000,prefabname="BusterRifle",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "アドバンスドドローン",description="サブ武器。運用能力を強化したドローン兵器。ボタン押し続けで追加射出。",price=9000,prefabname="AdvancedDronePlatform",type=ShopItemWeapon.Type.Back },
    };



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
        new ShopItemParts{tier=1,name = "トラッキングシステム",description="近距離における射撃の照準性能が強化される。",price=1000,itemFlag=RobotController.ItemFlag.TrackingSystem},
        new ShopItemParts{tier=1,name = "クイックイグナイター",description="ステップと空中ダッシュの初速が速くなる。",price=1500,itemFlag=RobotController.ItemFlag.QuickIgniter },
        new ShopItemParts{tier=1,name = "スナイプショット",description="射撃+下フリックで強力な射撃ができるようになる。全ての攻撃からキャンセル可能。",price=2000,itemFlag=RobotController.ItemFlag.SnipeShoot},
        new ShopItemParts{tier=1,name = "チェインファイア",description="メイン射撃からサブ射撃にキャンセルできるようになる。また、サブ射撃の弾数が早く回復する。",price=1000,itemFlag=RobotController.ItemFlag.ChainFire },
        new ShopItemParts{tier=1,name = "イアイスラッシュ",description="メイン射撃から格闘にキャンセルできるようになる。空中にいる相手には必ず空中格闘が発動するようになる。",price=1500,itemFlag=RobotController.ItemFlag.IaiSlash },
        new ShopItemParts{tier=1,name = "クイックドロー",description="メイン射撃の発射が早くなる。また、格闘からメイン射撃入力で追撃できるようになる。",price=1500,itemFlag=RobotController.ItemFlag.QuickDraw },
        new ShopItemParts{tier=1,name = "ソフトランディング",description="射撃中に着地すると即座に行動できるようになる。",price=1000,itemFlag=RobotController.ItemFlag.SoftLanding },
        new ShopItemParts{tier=1,name = "ランニングテイクオフ",description="ステップからジャンプするとブーストを消費して急上昇し、残像を発生させる。\n\n※残像…ステップなどの行動時に発生し、攻撃の追尾を振り切る効果がある。",price=1500,itemFlag=RobotController.ItemFlag.RunningTakeOff },
        new ShopItemParts{tier=1,name = "カウンターメジャー",description="残像の効果時間が倍増させる。\n\n※残像…ステップなどの行動時に発生し、攻撃の追尾を振り切る効果がある。",price=1500,itemFlag=RobotController.ItemFlag.CounterMeasure },
        new ShopItemParts{tier=1,name = "インファイトブースト",description="格闘の踏み込みと追従が強化される。",price=2000,itemFlag=RobotController.ItemFlag.InfightBoost },
        new ShopItemParts{tier=1,name = "ダッシュスラッシュ",description="格闘+上フリックでブーストを大きく消費してダッシュ斬りができるようになる。射撃からキャンセル可能。また格闘コンボ中に上入力+格闘でダッシュ斬りに派生可能になる。",price=2000,itemFlag=RobotController.ItemFlag.DashSlash },
        new ShopItemParts{tier=1,name = "ホバークラフト",description="地上での硬直中、滑走するようになる。",price=2000,itemFlag=RobotController.ItemFlag.Hovercraft },
        new ShopItemParts{tier=1,name = "ローリングスラッシュ",description="格闘コンボ中に下入力+格闘で高威力の回転切りに派生する。",price=1500,itemFlag=RobotController.ItemFlag.RollingSlash },
        new ShopItemParts{tier=2,name = "グランドブースト",description="ステップ中、ブーストボタンを押し続けるとブーストを消費してダッシュし続ける。途中で前後左右を入力すると方向転換でき、射撃を行うと滑走する。",price=3000,itemFlag=RobotController.ItemFlag.GroundBoost },
        new ShopItemParts{tier=2,name = "フライトユニット",description="空中でもブーストが回復するようになり、滞空性能も向上する。また、射撃中でも常に上昇できるようになる。",price=4000,itemFlag=RobotController.ItemFlag.FlightUnit },
        new ShopItemParts{tier=2,name = "ローリングショット",description="射撃+横フリックでブーストを消費して回転撃ちができるようになる。射撃からキャンセル可能。",price=4500,itemFlag=RobotController.ItemFlag.RollingShoot },
        new ShopItemParts{tier=2,name = "エアロミラージュ",description="空中ダッシュの開始時、残像を発生させる。\n\n※残像…ステップなどの行動時に発生し、攻撃の追尾を振り切る効果がある。",price=3500,itemFlag=RobotController.ItemFlag.AeroMirage },
        new ShopItemParts{tier=2,name = "リープストライク",description="格闘+下フリックでジャンプ斬りができるようになる。ブーストを大きく消費する。射撃またはダッシュスラッシュからキャンセル可能。",price=4000,itemFlag=RobotController.ItemFlag.JumpSlash},
        new ShopItemParts{tier=2,name = "ミラージュクラウド",description="一定時間ごとに強力な残像を発生させる。攻撃中、よろけ・ダウン中は無効。\n\n※残像…ステップなどの行動時に発生し、攻撃の追尾を振り切る効果がある。",price=5000,itemFlag=RobotController.ItemFlag.MirageCloud },
        new ShopItemParts{tier=2,name = "シードオブアーツ",description="格闘アクションが方向入力無しと、左右方向入力しながらの格闘に置き換わる。",price=3500,itemFlag=RobotController.ItemFlag.SeedOfArts },
        new ShopItemParts{tier=2,name = "ホライゾンスイープ",description="格闘+左右フリックでサーベルを延長しての薙ぎ払い攻撃を行う。ヒット時、格闘へキャンセル可能。",price=3000,itemFlag=RobotController.ItemFlag.HorizonSweep},
        new ShopItemParts{tier=3,name = "ヴォイドシフト",description="遠距離から格闘を発動すると、無敵状態で急速接近する。",price=7500,itemFlag=RobotController.ItemFlag.VoidShift },
        new ShopItemParts{tier=3,name = "ネクストドライブ",description="全ての行動を空中ダッシュでキャンセルできるようになる。キャンセルはブーストを大きく消費する。また、空中ダッシュ前の旋回時間がなくなり、ブースト容量が2倍になる。",price=9000,itemFlag=RobotController.ItemFlag.NextDrive },
        new ShopItemParts{tier=3,name = "エクストリームスライド",description="全ての行動をステップでキャンセルできるようになる。キャンセルはブーストを大きく消費する。",price=10000,itemFlag=RobotController.ItemFlag.ExtremeSlide },
        new ShopItemParts{tier=3,name = "マスイリュージョン",description="攻撃中、一定時間ごとに強力な残像を発生させる。\n\n※残像…ステップなどの行動時に発生し、攻撃の追尾を振り切る効果がある。",price=11000,itemFlag=RobotController.ItemFlag.MassIllusion },

    };





    List<(GameObject,ShopItem)> shopItemPanel = new List<(GameObject, ShopItem)>();

    List<(GameObject, ShopItem)> inventryItemPanel = new List<(GameObject, ShopItem)>();

    [SerializeField] TextMeshProUGUI goldText;

    bool finished = false;

    public void LotteryItem_OneGroup<T>(T[] pool,List<T> chosen, int Tier, int count,List<T> inventry) where T :  ShopItem
    {
        //if (count <= 0)
        //    return false;

        //var player_alllist = new List<ShopItemWeapon>();

        //for (int tier = Tier; tier > 0; tier--)
        {
            List<T> remainItem = new List<T>();

            foreach (T item in pool)
            {
                if (item.tier != Tier)
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

        //return chosen.Count > 0;
    }

    public void AddItemToShopPanel<T>(GameObject containerPanel, T item) where T: ShopItem
    {
        GameObject itemPanel = Instantiate(item_prefab, containerPanel.transform);
        itemPanel.GetComponent<Button>().onClick.AddListener(()=> { SetSelect_Shop(item, itemPanel); });

        itemPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = item.name;

        if (gameState.destination == GameState.Destination.Reward)
            itemPanel.transform.Find("PriceOrEquipped").GetComponent<TextMeshProUGUI>().text = "";
        else
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
                buyOrEquippedButtonObj_Garage.SetActive(true);
                buyOrEquippedButtonObj_Garage.GetComponent<Button>().onClick.RemoveAllListeners();
                buyOrEquippedButtonObj_Garage.GetComponent<Button>().onClick.AddListener(() => { BuyItem(selectedItem, selectedItemPanel); });
            }
            else
                image.color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
        }

       if(selectedItem==null)
        {
            descriptionText.text = "";
            buyOrEquippedButtonObj_Garage.SetActive(false);
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
                buyOrEquippedButtonObj_Garage.SetActive(true);

                if (IsEquipped(itemPair.Item2))
                    buyOrEquippedButtonObj_Garage.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "外す";
                else
                    buyOrEquippedButtonObj_Garage.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "装備";

                buyOrEquippedButtonObj_Garage.GetComponent<Button>().onClick.RemoveAllListeners();
                buyOrEquippedButtonObj_Garage.GetComponent<Button>().onClick.AddListener(() => { EquipItem(selectedItem, selectedItemPanel); });
            }
            else
                image.color = new Color(0.0f, 0.0f, 0.0f, 0.75f);
        }

        if (selectedItem == null)
        {
            descriptionText.text = "";
            buyOrEquippedButtonObj_Garage.SetActive(false);
        }
    }


    void BuyItem<T>(T selectedItem, GameObject selectedItemPanel) where T : ShopItem
    {
        if (gameState.destination == GameState.Destination.Reward)
        {
            if (selectedItem is ShopItemWeapon)
                gameState.inventryWeapons.Add(selectedItem as ShopItemWeapon);
            else
            {
                ShopItemParts selectedParts = selectedItem as ShopItemParts;

                gameState.itemFlag |= selectedParts.itemFlag;
                gameState.inventryParts.Add(selectedParts);
            }

            OnClickDeparture();
        }
        else
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
                buyOrEquippedButtonObj_Garage.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "装備";
            }
            else
            {
                value = selectedweapon.prefabname;
                buyOrEquippedButtonObj_Garage.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "外す";
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
                buyOrEquippedButtonObj_Garage.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "装備";
            }
            else
            {
                gameState.itemFlag |= selectedparts.itemFlag;
                buyOrEquippedButtonObj_Garage.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "外す";
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

        if(gameState.destination==GameState.Destination.Garage)
        {
            departureText.text = "出撃";
        }
        else if(gameState.destination == GameState.Destination.Reward)
        {
            departureButton_Garage.gameObject.SetActive(false);
            departureButton_Shop.gameObject.SetActive(false);
            testButton.gameObject.SetActive(false);
            garageText.text = "所持品";
            toGarageText.text = "所持品";
            shopText.text = "戦利品獲得";
            toShopText.text = "戻る";
            buyOrEquippedButtonText_Shop.text = "確定";
            goldLabel.enabled = false;

            gameState.shopWeapons.Clear();
            gameState.shopParts.Clear();

            int[] count_TierMap_weapon;
            int[] count_TierMap_parts;

            if (gameState.progressStage < 4) // ステージ1終了時に2、3終了時に4
            {
                count_TierMap_weapon = new int[3] { 1, 0, 0 };
                count_TierMap_parts = new int[3] { 3, 0, 0 };
            }
            else if(gameState.progressStage == 4)
            {
                count_TierMap_weapon = new int[3] { 0, 1, 0 };
                count_TierMap_parts = new int[3] { 0, 2, 0 };
            }
            else if(gameState.progressStage < 7)
            {
                count_TierMap_weapon = new int[3] { 1, 1, 0 };
                count_TierMap_parts = new int[3] { 3, 2, 0 };
            }
            else if (gameState.progressStage == 7)
            {
                count_TierMap_weapon = new int[3] { 0, 0, 1 };
                count_TierMap_parts = new int[3] { 0, 0, 1 };
            }
            else
            {
                count_TierMap_weapon = new int[3] { 1, 1, 1 };
                count_TierMap_parts = new int[3] { 3, 2, 1 };
            }

            for (int tier = 3; tier > 0; tier--)
            {
                int count_ThisTier_weapon = count_TierMap_weapon[tier-1];
                int count_ThisTier_parts = count_TierMap_parts[tier - 1];

                LotteryItem_OneGroup<ShopItemWeapon>(shopItemWeapons, gameState.shopWeapons, tier, count_ThisTier_weapon, gameState.inventryWeapons);
                LotteryItem_OneGroup<ShopItemParts>(shopItemParts, gameState.shopParts, tier, count_ThisTier_parts, gameState.inventryParts);
            }
        }
        else
        {
            departureText.text = "出発";

            if (gameState.subDestination_Intermission == GameState.SubDestination_Intermission.FromWorldMap)
            {

                gameState.shopWeapons.Clear();
                gameState.shopParts.Clear();
                for (int tier = 3; tier > 0; tier--)
                {
                    int count_ThisTier = System.Math.Max(0, 3 - (tier * 3) + gameState.progressStage+2);

                    LotteryItem_OneGroup<ShopItemWeapon>(shopItemWeapons, gameState.shopWeapons, tier, count_ThisTier, gameState.inventryWeapons);
                    LotteryItem_OneGroup<ShopItemParts>(shopItemParts, gameState.shopParts, tier, count_ThisTier, gameState.inventryParts);
                }
            }
            
        }

        DrawShop();
        SwitchToShop();

        if(gameState.destination==GameState.Destination.Garage)
        {
            OnClickProceedToGarage();
            backToShopButton.SetActive(false);
        }
        else if (gameState.destination == GameState.Destination.Reward)
        {
            
        }
        else if(gameState.subDestination_Intermission == GameState.SubDestination_Intermission.FromTestRoom)
        {
            OnClickProceedToGarage();
        }

        Cursor.lockState = CursorLockMode.None;
    }

    void DrawShop()
    {
        GameObject weaponListPanel = ShopPanel.transform.Find("WeaponListPanel").Find("Viewport").Find("Content").gameObject;

        for (int i = 0; i < gameState.shopWeapons.Count; i++)
        {
            if(gameState.inventryWeapons.Find(x => x == gameState.shopWeapons[i]) == null)
                AddItemToShopPanel(weaponListPanel, gameState.shopWeapons[i]);
        }

        GameObject partsListPanel = ShopPanel.transform.Find("UpgradePartsListPanel").Find("Viewport").Find("Content").gameObject;

        for (int i = 0; i < gameState.shopParts.Count; i++)
        {
            if (gameState.inventryParts.Find(x => x == gameState.shopParts[i]) == null)
                AddItemToShopPanel(partsListPanel, gameState.shopParts[i]);
        }

        if(gameState.destination == GameState.Destination.Reward)
            goldText.text = "";
        else
            goldText.text = $"${gameState.gold.ToString()}";
    }
       
    void SwitchToShop()
    {
        GameObject weaponListPanel = ShopPanel.transform.Find("WeaponListPanel").Find("Viewport").Find("Content").gameObject;
        GameObject partsListPanel = ShopPanel.transform.Find("UpgradePartsListPanel").Find("Viewport").Find("Content").gameObject;

        descriptionText = ShopPanel.transform.Find("SelectedItemPanel").Find("Description").GetComponent<TextMeshProUGUI>();
        buyOrEquippedButtonObj_Garage = ShopPanel.transform.Find("SelectedItemPanel").Find("BuyOrEquipButton").gameObject;

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
        buyOrEquippedButtonObj_Garage = GaragePanel.transform.Find("SelectedItemPanel").Find("BuyOrEquipButton").gameObject;

        SetSelect_Garage<ShopItemWeapon>(null, weaponListPanel);
        SetSelect_Garage<ShopItemParts>(null, partsListPanel);
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnClickDeparture()
    {
        if (finished)
            return;

        if (gameState.destination == GameState.Destination.Garage)
            audioSource.PlayOneShot(audioClip_Start);

        StartCoroutine("Blackout_Departure");

        finished = true;
    }

    public void OnClickTestingRoom()
    {
        if (finished)
            return;

        StartCoroutine("Blackout_TestingRoom");

        finished = true;
    }

    public void OnClickBackToBuild()
    {
        if (finished)
            return;

        ShopPanel.SetActive(true);
        GaragePanel.SetActive(false);

        SwitchToShop();
    }

    public void OnClickProceedToGarage()
    {
        if (finished)
            return;

        ShopPanel.SetActive(false);
        GaragePanel.SetActive(true);

        SwitchToGarage();
    }

    IEnumerator Blackout_Departure()
    {
        float k;

        if (gameState.destination == GameState.Destination.Garage)
            k = 1.0f;
        else
            k = 0.5f;

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float start = Time.time;
        while (Time.time - start < k || audioSource.isPlaying)
        {
            yield return wait;
            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / k);


        }

        switch (gameState.destination)
        {
            case GameState.Destination.Garage:
                gameState.loadingDestination = Loading.Destination.Mission;
                SceneManager.LoadScene("Loading");
                break;
            case GameState.Destination.Intermission:
            case GameState.Destination.Reward:
                gameState.progress++;
                SceneManager.LoadScene("WorldMap");
                break;
        }
    }

    IEnumerator Blackout_TestingRoom()
    {

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float start = Time.time;
        while (Time.time - start < 0.5f || audioSource.isPlaying)
        {
            yield return wait;

            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / 0.5f);


        }

        gameState.loadingDestination = Loading.Destination.TestingRoom;
        SceneManager.LoadScene("Loading");
    }

    IEnumerator Blackin()
    {
        blackout.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float start = Time.time;
        while (Time.time - start < 0.5f || audioSource.isPlaying)
        {
            yield return wait;
            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, 1.0f-((float)fade / 0.5f));


        }
    }
}