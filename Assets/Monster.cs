using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Monster : Character
{
    public static List<Monster> Monsters = new List<Monster>(); //static이라서 이름을 대문자로 시작
    internal object rewardExp;

    //Animator animator;
    public override CharacterTypeEnum CharacterType { get => CharacterTypeEnum.Monster; }

    new private void Awake()
    {
        base.Awake();
        Monsters.Add(this);
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        //몬스터가 서있는 블록에 몬스터 타입도 추가
    }


    internal IEnumerator AutoAttackCo()
    {
        Player enemyPlayer = GetNearstPlayer(); //가장 가까이에 있는 플레이어를 찾은 다음

        if(IsInAttackableArea(enemyPlayer.transform.position))
        {
            yield return AttackTargetCo(enemyPlayer);
        }
        else
        {
            yield return FindPathCo(enemyPlayer.transform.position.ToVector2Int());

            if(IsInAttackableArea(enemyPlayer.transform.position))
            {
                yield return AttackTargetCo(enemyPlayer);
            }
        }
    }

    private Player GetNearstPlayer()
    {
        var myPos = transform.position;
        var nearestPlayer = Player.Players.Where(x => x.status != StatusType.Die).
            OrderBy(x => Vector3.Distance(x.transform.position, myPos)).First();

        return nearestPlayer;
    }


    private object AttackTargetCo(Player enemyPlayer)
    {
        throw new NotImplementedException();
    }
}
