using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerLevelData
{
    //플레이어의 레벨과 레벨에 해당하는 hp,mp, maxExp를 담는 클래스
    public int level;
    public int hp;
    public int mp;
    public int maxExp;
}

public class GlobalData : SingletonMonoBehavior<GlobalData>
{
    public List<PlayerLevelData> playerDatas;
}
