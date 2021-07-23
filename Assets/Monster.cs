using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StatusType
{
    Normal,
    Sleep,
    Die,
}

public enum CharacterTypeEnum
{
    NotInit,
    Player,
    Monster,
}

public class Character : MonoBehaviour //플레이어와 몬스터에 대한 기본적인 정보를 가지고 있는 클래스
{
    public string nickName = "이름";
    public string iconName;
    public float hp =10;
    public float mp = 0;
    public StatusType status;
    public int maxHp = 50;
    public int maxMp = 20;
  

    public int moveDistance = 5; //움직일 수 있는 영역 표시하기 위한 변수

    private void Awake()
    {
        //var attackPoints = GetComponentsInChildren<AttackPoint>(true);


    }
    public virtual CharacterTypeEnum CharacterType { get => CharacterTypeEnum.NotInit; }
}
public class Monster : Character
{
    public override CharacterTypeEnum CharacterType { get => CharacterTypeEnum.Monster; }
        void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        //몬스터가 서있는 블록에 몬스터 타입도 추가
    }

    private void Awake()
    {
        
    }
}
