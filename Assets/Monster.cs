using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Monster : Character
{
    Animator animator;
    public override CharacterTypeEnum CharacterType { get => CharacterTypeEnum.Monster; }
        void Start()
    {
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        //몬스터가 서있는 블록에 몬스터 타입도 추가
    }

    internal override void TakeHit(int power)
    {

        GameObject damageTextGo =  (GameObject)Instantiate(Resources.Load("DamageText"), transform); //transform(몬스터)를 부모로 하여 생성된다.
        damageTextGo.transform.localPosition = new Vector3(0, 1.71f, 0); //몬스터로부터 y축으로 1.3f만큼 위에 데미지텍스트 프리팹 생성
        damageTextGo.GetComponent<TextMeshPro>().text = power.ToString(); //플레이어의 power를 string으로 변환하여 텍스트메쉬프로에 대입한다
        Destroy(damageTextGo, 2); //Destroy의 첫번째 파라미터는 파괴할 대상, 두 번째 파라미터는 딜레이시간


        hp -= power;
        animator.Play("TakeHit");
    }
}
