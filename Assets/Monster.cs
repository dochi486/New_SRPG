
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Monster : Character
{
    public static List<Monster> Monsters = new List<Monster>(); //static이라서 이름을 대문자로 시작
    public int rewardExp = 5;

    //Animator animator;
    public override CharacterTypeEnum CharacterType { get => CharacterTypeEnum.Monster; }

    new protected void Awake()
    {
        base.Awake();
        Monsters.Add(this);
    }
    new private void OnDestroy()
    {
        base.OnDestroy();
        Monsters.Remove(this);
    }
    protected void Start()
    {
        //animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        //몬스터가 서있는 블록에 몬스터 타입도 추가
    }



    internal IEnumerator AutoAttackCo()
    {
        Player enemyPlayer = GetNearstPlayer(); //가장 가까이에 있는 플레이어를 찾은 다음

        if (IsInAttackableArea(enemyPlayer.transform.position)) //공격 가능한 위치에 있다면 바로 공격한다.
        {
            yield return AttackTargetCo(enemyPlayer); //바로 공격하는 부분
        }
        else
        {
            yield return FindPathCo(enemyPlayer.transform.position.ToVector2Int()); //공격 가능한 범위에 없다면 공격할 플레이어 쪽으로 이동한다.

            if (IsInAttackableArea(enemyPlayer.transform.position))
            {
                yield return AttackTargetCo(enemyPlayer); //공격할 수 있는 범위로 들어오면 공격한다. 
            }
        }
    }

    public override BlockType GetBlockType()
    {
        return BlockType.Monster;
    }

    private Player GetNearstPlayer()
    {
        var myPos = transform.position;
        var nearestPlayer = Player.Players.Where(x => x.status != StatusType.Die).
            OrderBy(x => Vector3.Distance(x.transform.position, myPos)).First();

        return nearestPlayer;
    }

    protected override void OnDie()
    {
        //몬스터가 죽은 경우에는
        GroundManager.Instance.RemoveBlockInfo(transform.position, BlockType.Monster);
        //몬스터를 죽인(막타를 친) 플레이어의 경험치가 상승하고
        //몬스터 GameObject를 파괴하고
        Destroy(gameObject, 1); //Die 애니메이션을 한 다음에 destroy하도록 1초 기다린다. 
        //모든 몬스터가 죽었는지 확인한다. 
        if (Monsters.Where(x => x.status != StatusType.Die).Count() == 0)
        {
            CenterNotifyUI.Instance.Show("Stage Clear");
        }
        //모든 몬스터가 죽었다면 스테이지 클리어
    }
}
