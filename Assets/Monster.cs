using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Character
{
    public override CharacterTypeEnum CharacterType { get => CharacterTypeEnum.Monster; }
        void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
        //몬스터가 서있는 블록에 몬스터 타입도 추가
    }

    internal override void TakeHit(int power)
    {
        base.TakeHit(power);
    }
}
