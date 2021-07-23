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
    public int power = 10;
    public float hp = 10;
    public float mp = 0;
    public StatusType status;
    public int maxHp = 50;
    public int maxMp = 20;

    public bool completeMove;
    public bool completeAct;

    public bool CompleteTurn { get => completeMove && completeAct; }

    public List<Vector2Int> attackablePoints = new List<Vector2Int>();
    public int moveDistance = 5; //움직일 수 있는 영역 표시하기 위한 변수

    private void Awake()
    {
        var attackPoints = GetComponentsInChildren<AttackPoint>(true); //파라미터를 true로 줬기 때문에 오브젝트가 꺼져있더라도 가져올 수 있다
        //공격 가능한 범위를 모아두는 부분
        foreach (var item in attackPoints) //바로 앞에 있는 공격 가능한 지점
            attackablePoints.Add(item.transform.localPosition.ToVector2Int());

        //오른쪽에 있는 공격 가능한 지점들
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints) //오른쪽에 있는 공격 가능한 지점
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int());

        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints) //아래쪽에 있는 공격 가능한 지점
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int());

        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints) //왼쪽에 있는 공격 가능한 지점
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int());

        transform.Rotate(0, 90, 0); //다시 원래 방향(앞)을 보도록 회전 시킨다

    }
    public virtual CharacterTypeEnum CharacterType { get => CharacterTypeEnum.NotInit; }

    internal virtual void TakeHit(int power)
    {
        hp -= power;
    }
}