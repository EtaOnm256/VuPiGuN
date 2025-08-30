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
        new ShopItemWeapon{tier=1,name = "�o�Y�[�J",description="����a�̃��P�b�g�����`���[�B",price=1500,prefabname="Bazooka Variant" ,type=ShopItemWeapon.Type.Main},
        new ShopItemWeapon{tier=1,name = "�~�T�C���|�b�h",description="�T�u����B�Œ莮�̃~�T�C�����ˑ��u�B�{�^�����������ŘA�ˉ\�B",price=2000,prefabname="MissilePod Variant",type=ShopItemWeapon.Type.Shoulder },
        new ShopItemWeapon{tier=1,name = "�T�u�}�V���K��",description="���^�̋@�֏e�B�ǉ����͂ŘA�ˉ\�B",price=1500,prefabname="SMG Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=1,name = "�\���b�h���C�t��",description="�R�X�g�̈������e����B",price=1000,prefabname="SolidRifle Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=1,name = "�n���h�L���m��",description="�莝�����̞֒e�C�B�������傫���B",price=2000,prefabname="HandCannon Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=2,name = "�r�[�����C�t��",description="�Η͂Ǝ��񂵂𗼗������r�[������B",price=5000,prefabname="BeamRifle Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=2,name = "�h���[��",description="�T�u����B�q�@���ˏo���A�G���U��������B�{�^�����������Œǉ��ˏo�B",price=5500,prefabname="DronePlatform Variant",type=ShopItemWeapon.Type.Back },
        new ShopItemWeapon{tier=2,name = "�X�v���[�r�[���|�b�h",description="�T�u����B���ɌŒ肷��g�U�r�[���C�B",price=4500,prefabname="BeamSprayCannon",type=ShopItemWeapon.Type.Shoulder },
        new ShopItemWeapon{tier=2,name = "�X�i�C�p�[���C�t��",description="�e���������A�������ˌ��Ɍ��������e����B",price=4000,prefabname="SniperRifle Variant",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=2,name = "�N���X�^�[�����`���[",description="�G�O�ŎU�e���΂�܂��e���𔭎˂���B",price=4500,prefabname="Cluster Launcher" ,type=ShopItemWeapon.Type.Main},
        new ShopItemWeapon{tier=2,name = "�K�g�����O�L���m��",description="�莝���̋@�֖C�B�ǉ����͂ŘA�ˉ\�B",price=5000,prefabname="GatlingCannon",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "���K�r�[�������`���[",description="��^�̃r�[���C�B",price=9000,prefabname="BeamLauncher",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "�r�[���}�O�i��",description="�Η͂��グ���r�[�����C�t���B���ˌ�̐����������B",price=8500,prefabname="BeamMagnum",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "�r�[���T�u�}�V���K��",description="���^�������������r�[������B�ǉ����͂ŘA�ˉ\�B",price=8500,prefabname="BeamMachineGun",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "�o�X�^�[���C�t��",description="���o�͂̃r�[�����Ǝ˂���莝�����̉Ί�B",price=9000,prefabname="BusterRifle",type=ShopItemWeapon.Type.Main },
        new ShopItemWeapon{tier=3,name = "�A�h�o���X�h�h���[��",description="�T�u����B�^�p�\�͂����������h���[������B�{�^�����������Œǉ��ˏo�B",price=9000,prefabname="AdvancedDronePlatform",type=ShopItemWeapon.Type.Back },
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
        new ShopItemParts{tier=1,name = "�g���b�L���O�V�X�e��",description="�ߋ����ɂ�����ˌ��̏Ə����\�����������B",price=1000,itemFlag=RobotController.ItemFlag.TrackingSystem},
        new ShopItemParts{tier=1,name = "�N�C�b�N�C�O�i�C�^�[",description="�X�e�b�v�Ƌ󒆃_�b�V���̏����������Ȃ�B",price=1500,itemFlag=RobotController.ItemFlag.QuickIgniter },
        new ShopItemParts{tier=1,name = "�X�i�C�v�V���b�g",description="�ˌ�+���t���b�N�ŋ��͂Ȏˌ����ł���悤�ɂȂ�B�S�Ă̍U������L�����Z���\�B",price=2000,itemFlag=RobotController.ItemFlag.SnipeShoot},
        new ShopItemParts{tier=1,name = "�`�F�C���t�@�C�A",description="���C���ˌ�����T�u�ˌ��ɃL�����Z���ł���悤�ɂȂ�B�܂��A�T�u�ˌ��̒e���������񕜂���B",price=1000,itemFlag=RobotController.ItemFlag.ChainFire },
        new ShopItemParts{tier=1,name = "�C�A�C�X���b�V��",description="���C���ˌ�����i���ɃL�����Z���ł���悤�ɂȂ�B�󒆂ɂ��鑊��ɂ͕K���󒆊i������������悤�ɂȂ�B",price=1500,itemFlag=RobotController.ItemFlag.IaiSlash },
        new ShopItemParts{tier=1,name = "�N�C�b�N�h���[",description="���C���ˌ��̔��˂������Ȃ�B�܂��A�i�����烁�C���ˌ����͂Œǌ��ł���悤�ɂȂ�B",price=1500,itemFlag=RobotController.ItemFlag.QuickDraw },
        new ShopItemParts{tier=1,name = "�h���b�v�A�T���g",description="�ˌ����ɒ��n����Ƒ����ɍs���ł���悤�ɂȂ�B",price=1000,itemFlag=RobotController.ItemFlag.DropAssault },
        new ShopItemParts{tier=1,name = "�����j���O�e�C�N�I�t",description="�X�e�b�v����W�����v����ƃu�[�X�g������ċ}�㏸���A�c���𔭐�������B\n\n���c���c�X�e�b�v�Ȃǂ̍s�����ɔ������A�U���̒ǔ���U��؂���ʂ�����B",price=1500,itemFlag=RobotController.ItemFlag.RunningTakeOff },
        new ShopItemParts{tier=1,name = "�J�E���^�[���W���[",description="�c���̌��ʎ��Ԃ��{��������B\n\n���c���c�X�e�b�v�Ȃǂ̍s�����ɔ������A�U���̒ǔ���U��؂���ʂ�����B",price=1500,itemFlag=RobotController.ItemFlag.CounterMeasure },
        new ShopItemParts{tier=1,name = "�C���t�@�C�g�u�[�X�g",description="�i���̓��ݍ��݂ƒǏ]�����������B",price=2000,itemFlag=RobotController.ItemFlag.InfightBoost },
        new ShopItemParts{tier=1,name = "�_�b�V���X���b�V��",description="�i��+��t���b�N�Ńu�[�X�g��傫������ă_�b�V���a�肪�ł���悤�ɂȂ�B�ˌ�����L�����Z���\�B�܂��i���R���{���ɏ����+�i���Ń_�b�V���a��ɔh���\�ɂȂ�B",price=2000,itemFlag=RobotController.ItemFlag.DashSlash },
        new ShopItemParts{tier=1,name = "�z�o�[�N���t�g",description="�n��ł̍d�����A��������悤�ɂȂ�B",price=2000,itemFlag=RobotController.ItemFlag.Hovercraft },
        new ShopItemParts{tier=1,name = "���[�����O�X���b�V��",description="�i���R���{���ɉ�����+�i���ō��З͂̉�]�؂�ɔh������B",price=1500,itemFlag=RobotController.ItemFlag.RollingSlash },
        new ShopItemParts{tier=2,name = "�O�����h�u�[�X�g",description="�X�e�b�v���A�u�[�X�g�{�^��������������ƃu�[�X�g������ă_�b�V����������B�r���őO�㍶�E����͂���ƃu�[�X�g������ĕ����]���ł���B",price=3000,itemFlag=RobotController.ItemFlag.GroundBoost },
        new ShopItemParts{tier=2,name = "�t���C�g���j�b�g",description="�󒆂ł��u�[�X�g���񕜂���悤�ɂȂ�A�؋󐫔\�����シ��B�܂��A�ˌ����ł���ɏ㏸�ł���悤�ɂȂ�B",price=4000,itemFlag=RobotController.ItemFlag.FlightUnit },
        new ShopItemParts{tier=2,name = "���[�����O�V���b�g",description="�ˌ�+���t���b�N�Ńu�[�X�g������ĉ�]�������ł���悤�ɂȂ�B�ˌ�����L�����Z���\�B",price=4500,itemFlag=RobotController.ItemFlag.RollingShoot },
        new ShopItemParts{tier=2,name = "�G�A���~���[�W��",description="�󒆃_�b�V���̊J�n���A�c���𔭐�������B\n\n���c���c�X�e�b�v�Ȃǂ̍s�����ɔ������A�U���̒ǔ���U��؂���ʂ�����B",price=3500,itemFlag=RobotController.ItemFlag.AeroMirage },
        new ShopItemParts{tier=2,name = "���[�v�X�g���C�N",description="�i��+���t���b�N�ŃW�����v�a�肪�ł���悤�ɂȂ�B�u�[�X�g��傫�������B�ˌ��܂��̓_�b�V���X���b�V������L�����Z���\�B",price=4000,itemFlag=RobotController.ItemFlag.JumpSlash},
        new ShopItemParts{tier=2,name = "�~���[�W���N���E�h",description="��莞�Ԃ��Ƃɋ��͂Ȏc���𔭐�������B�U�����A��낯�E�_�E�����͖����B\n\n���c���c�X�e�b�v�Ȃǂ̍s�����ɔ������A�U���̒ǔ���U��؂���ʂ�����B",price=5000,itemFlag=RobotController.ItemFlag.MirageCloud },
        new ShopItemParts{tier=2,name = "�V�[�h�I�u�A�[�c",description="�i���A�N�V�������������͖����ƁA���E�������͂��Ȃ���̊i���ɒu�������B",price=3500,itemFlag=RobotController.ItemFlag.SeedOfArts },
        new ShopItemParts{tier=2,name = "�z���C�]���X�C�[�v",description="�i��+���E�t���b�N�ŃT�[�x�����������Ă̓ガ�����U�����s���B",price=3000,itemFlag=RobotController.ItemFlag.HorizonSweep},
        new ShopItemParts{tier=3,name = "���H�C�h�V�t�g",description="����������i���𔭓�����ƁA���G��Ԃŋ}���ڋ߂���B",price=7500,itemFlag=RobotController.ItemFlag.VoidShift },
        new ShopItemParts{tier=3,name = "�l�N�X�g�h���C�u",description="�S�Ă̍s�����󒆃_�b�V���ŃL�����Z���ł���悤�ɂȂ�B�L�����Z���̓u�[�X�g��傫�������B�܂��A�󒆃_�b�V���O�̐��񎞊Ԃ��Ȃ��Ȃ�A�u�[�X�g�e�ʂ�2�{�ɂȂ�B",price=9000,itemFlag=RobotController.ItemFlag.NextDrive },
        new ShopItemParts{tier=3,name = "�G�N�X�g���[���X���C�h",description="�S�Ă̍s�����X�e�b�v�ŃL�����Z���ł���悤�ɂȂ�B�L�����Z���̓u�[�X�g��傫�������B",price=10000,itemFlag=RobotController.ItemFlag.ExtremeSlide },
        new ShopItemParts{tier=3,name = "�}�X�C�����[�W����",description="�U�����A��莞�Ԃ��Ƃɋ��͂Ȏc���𔭐�������B\n\n���c���c�X�e�b�v�Ȃǂ̍s�����ɔ������A�U���̒ǔ���U��؂���ʂ�����B",price=11000,itemFlag=RobotController.ItemFlag.MassIllusion },

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

                //�������Ă��Ȃ��A�C�e���𒊑I���Ƃ��đI��
                if (inventry == null || inventry.Find(x => x.name == item.name) == null)
                {
                    remainItem.Add(item);
                }
                //else
                //    player_alllist.Add(item);
            }

            //�����A�C�e���Ɣr���Ȃ��̂͒��I��₩��폜
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

                // ���I�����A�C�e���Ɣr���Ȃ��̂͒��I��₩��폜
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
            itemPanel.transform.Find("PriceOrEquipped").GetComponent<TextMeshProUGUI>().text = "������";
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
                    buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "�O��";
                else
                    buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "����";

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
                buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "����";
            }
            else
            {
                value = selectedweapon.prefabname;
                buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "�O��";
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
                buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "����";
            }
            else
            {
                gameState.itemFlag |= selectedparts.itemFlag;
                buyOrEquippedButtonObj.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "�O��";
            }
        }

       

        foreach (var itemPanel in inventryItemPanel)
        {
            if (IsEquipped(itemPanel.Item2))
                itemPanel.Item1.transform.Find("PriceOrEquipped").GetComponent<TextMeshProUGUI>().text = "������";
            else
                itemPanel.Item1.transform.Find("PriceOrEquipped").GetComponent<TextMeshProUGUI>().text = "";
        }


        //SwitchToGarage();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Blackin");

        if(gameState.loadingDestination==GameState.LoadingDestination.Intermission_Garage)
        {
           
        }
        else
        {
            gameState.shopWeapons.Clear();
            gameState.shopParts.Clear();
            for (int tier = 3; tier > 0; tier--)
            {
                int count_ThisTier = System.Math.Max(0, 3-( (tier-1) * 3)+(gameState.stage-1));

                LotteryItem_OneGroup<ShopItemWeapon>(shopItemWeapons, gameState.shopWeapons, tier, count_ThisTier, gameState.inventryWeapons);
                LotteryItem_OneGroup<ShopItemParts>(shopItemParts, gameState.shopParts, tier, count_ThisTier, gameState.inventryParts);
            }
            
            
        }

        DrawShop();
        SwitchToShop();

        if(gameState.loadingDestination==GameState.LoadingDestination.Intermission_Garage)
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
        if (finished)
            return;

        audioSource.PlayOneShot(audioClip_Start);

        StartCoroutine("Blackout_MissionStart");

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

    IEnumerator Blackout_MissionStart()
    {

        var wait = new WaitForSeconds(Time.fixedDeltaTime);

        float start = Time.time;
        while (Time.time - start < 1.0f || audioSource.isPlaying)
        {
            yield return wait;
            float fade = Mathf.Max(0.0f, Time.time - start);

            blackout.color = new Color(0.0f, 0.0f, 0.0f, ((float)fade) / 1.0f);


        }

        gameState.loadingDestination = GameState.LoadingDestination.Mission;

        SceneManager.LoadScene("Loading");
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

        gameState.loadingDestination = GameState.LoadingDestination.TestingRoom;

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