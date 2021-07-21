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
    public float hp =10;
    public float mp = 0;
    public StatusType status;
    public int maxHp = 50;
    public int maxMp = 20;
  

    public int moveDistance = 5; //움직일 수 있는 영역 표시하기 위한 변수
    
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
