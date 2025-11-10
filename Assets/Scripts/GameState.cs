using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
[CreateAssetMenu]
public class GameState : ScriptableObject
{
    public int progress = -1;
    public int skyindex = 0;

    public enum Destination
    {
        Mission,
        TestingRoom,
        Intermission,
        Garage,
        WorldMap,
        Title
    }

    public enum SubDestination_Intermission
    {
        FromWorldMap,
        FromTestRoom
    }

  

    [System.Serializable]
    public class Chapter
    {
        public string name;
        public string[] stages;
        public string bossStage;
    }

    public Chapter[] chapters;

    [System.Serializable]
    public class JourneyInChapter
    {
        public string name;
        public List<int> index_in_chapter;
    }

    public List<JourneyInChapter> journeyPlan = new List<JourneyInChapter>();

    public Destination destination = Destination.WorldMap;
    public Loading.Destination loadingDestination;
    public SubDestination_Intermission subDestination_Intermission = SubDestination_Intermission.FromWorldMap;

    public int gold = 2000;
    public List<IntermissionButton.ShopItemWeapon> inventryWeapons = new List<IntermissionButton.ShopItemWeapon>();
    public List<IntermissionButton.ShopItemParts> inventryParts = new List<IntermissionButton.ShopItemParts>();
    public string rightWeapon_name;
    public string shoulderWeapon_name;
    public IntermissionButton.ShopItemWeapon.Type subWeaponType;

    public RobotController.ItemFlag itemFlag;

    public GameObject player_variant;
    public List<IntermissionButton.ShopItemWeapon> shopWeapons = new List<IntermissionButton.ShopItemWeapon>();
    public List<IntermissionButton.ShopItemParts> shopParts = new List<IntermissionButton.ShopItemParts>();
    public void Reset()
    {
        progress = 0;
        destination = Destination.Intermission;
        subDestination_Intermission = GameState.SubDestination_Intermission.FromWorldMap;
        gold = 3000;
        inventryWeapons.Clear();
        inventryParts.Clear();
        rightWeapon_name = "";
        shoulderWeapon_name = "";
        itemFlag = 0;

        MakeJourneyPlan();
    }

    private void MakeJourneyPlan()
    {
        journeyPlan.Clear();

        foreach(var chapter in chapters)
        {
            JourneyInChapter journeyInChapter = new JourneyInChapter { index_in_chapter = Enumerable.Range(0, chapter.stages.Length).OrderBy(x => System.Guid.NewGuid()).ToList()
            , name = chapter.name };

            journeyPlan.Add(journeyInChapter);
        }
    }
    [ContextMenu("Save")]
    private void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    public int GetMaxProgress()
    {
        int chapterInterval = chapters.Length - 1;
        int stageCount = 0;
        foreach(var chapter in chapters)
        {
            stageCount += chapter.stages.Length;
            stageCount += 1; //boss;
        }
        return chapterInterval+ stageCount;
    }



    public (Destination, string) GetNextStage_UpdateSkyIndex()
    {
        int progress_seek = 1;

        foreach (var chapter in chapters)
        {
            skyindex = 0;

            if (chapter.stages.Length > 0)
            {
                foreach (var idx in journeyPlan.Find(x => x.name == chapter.name).index_in_chapter)
                {
                    if (progress_seek == progress)
                        return (Destination.Garage,chapter.stages[idx]);

                    skyindex++;

                    progress_seek++;
                }
            }

            /**/

            skyindex = 0;

            if (progress_seek == progress)
                return (Destination.Garage, chapter.bossStage);

            progress_seek++;

            if (progress_seek == progress)
                return (Destination.Intermission, "Intermission");

            progress_seek++;
        }

        return (Destination.Title,"");
    }
}
