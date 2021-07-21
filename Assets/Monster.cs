using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StatusType
{
    Normal,
    Sleep,
    Die,
}

public class Character : MonoBehaviour
{
    public string nickName;
    public float hp;
    public float mp;
    public StatusType status;
    internal int maxMp;
    internal int maxHp;
}
public class Monster : Character
{
    void Start()
    {
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Monster, this);
    }

    private void Awake()
    {
        
    }
}
