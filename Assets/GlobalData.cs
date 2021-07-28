using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerLevelData
{
    //플레이어의 레벨과 레벨에 해당하는 hp,mp, maxExp를 담는 클래스
    public int level;
    public int maxHp;
    public int maxMp;
    public int maxExp;
}


[System.Serializable]
public class ItemData
{
    //아이템의 착용 가능레벨, 판매 가격, 구입 가격, 아이콘 이름을 담는 클래스
    public int ID;
    public int equipLevel;
    public int sellPrice;
    public int buyPrice;
    public string iconName;
}


public class GlobalData : SingletonMonoBehavior<GlobalData>
{
    [SerializeField]public List<PlayerLevelData> playerDatas;
    public Dictionary<int, PlayerLevelData> playerDataMap;

    [SerializeField] public List<ItemData> itemDatas;
    public Dictionary<int, ItemData> itemDataMap;

    protected override void OnInit()
    {
        playerDataMap = playerDatas.ToDictionary(x=> x.level); //레벨을 기준으로 리스트와 맵 초기화
        itemDataMap = itemDatas.ToDictionary(x => x.ID); //아이템 ID기준으로 리스트, 맵 초기화
    }
}

