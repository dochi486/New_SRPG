using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Monster : Character
{
    public static List<Monster> Monsters = new List<Monster>(); //static이라서 이름을 대문자로 시작
    Animator animator;
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

    internal override void TakeHit(int power)
    {

        GameObject damageTextGo = (GameObject)Instantiate(Resources.Load("DamageText"), transform); //transform(몬스터)를 부모로 하여 생성된다.
        damageTextGo.transform.localPosition = new Vector3(0, 1.71f, 0); //몬스터로부터 y축으로 1.3f만큼 위에 데미지텍스트 프리팹 생성
        damageTextGo.GetComponent<TextMeshPro>().text = power.ToString(); //플레이어의 power를 string으로 변환하여 텍스트메쉬프로에 대입한다
        Destroy(damageTextGo, 2); //Destroy의 첫번째 파라미터는 파괴할 대상, 두 번째 파라미터는 딜레이시간


        hp -= power;
        animator.Play("TakeHit");
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

    private object FindPathCo(Vector2Int vector2Int)
    {
        throw new NotImplementedException();
    }

    private object AttackTargetCo(Player enemyPlayer)
    {
        throw new NotImplementedException();
    }

    private bool IsInAttackableArea(Vector3 position)
    {
        throw new NotImplementedException();
    }

}
