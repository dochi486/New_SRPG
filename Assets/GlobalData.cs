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

public class GlobalData : SingletonMonoBehavior<GlobalData>
{
    [SerializeField]public List<PlayerLevelData> playerDatas;
    public Dictionary<int, PlayerLevelData> playerDataMap;

    protected override void OnInit()
    {
        playerDataMap = playerDatas.ToDictionary(x=> x.level);
    }
}

