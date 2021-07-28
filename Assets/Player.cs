﻿using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : Character
{
    public static List<Player> Players = new List<Player>(); //static이라서 리스트 이름을 대문자로 시작해준다.
    public override CharacterTypeEnum CharacterType { get => CharacterTypeEnum.Player; }

    static public Player SelectedPlayer;
    //Animator animator;

    public int ID; //SaveInt의 키로 사용할 플레이어 고유의 ID

    new protected void Awake()
    {
        base.Awake();
        Players.Add(this);
        exp = new SaveInt("exp" + ID); //키가 항상 달라야 독립된 값을 저장할 수 있기 때문에 ID와 exp를 조합해서 플레이어 각각의 밸류를 가질 수 있따. 
        level = new SaveInt("level" + ID);
        comment = new SaveString("comment" + ID);
    }
    new protected void OnDestroy()
    {
        base.OnDestroy();
        Players.Remove(this);
    }

    [ContextMenu("저장확인 테스트")] //꼭 플레이 중일 때만 테스트 가능

    void TestFn() //플레이를 멈췄다가 다시 실행했을 때 SaveInt, SaveString한 것들이 살아있는지 테스트 하는 함수
    {
        exp.Value += 1;
        comment.Value += "a";
    }

    // Start is called before the first frame update
    void Start()
    {
        //SelectedPlayer = this; //왜 주석처리했을까?
        //animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player, this); //플레이어가 처음에 서 있는 블록은 walkable밖에 지정 안되어있다.
        //현재 플레이어가 있는 블록(walkable)에 player타입도 지정
        FollowTarget.Instance.SetTarget(transform);
    }

    internal void MoveToPosition(Vector3 position)
    {
        Vector2Int findPos = position.ToVector2Int();
        FindPath(findPos);
    }

    //public Transform goal;

    //[ContextMenu("길 찾기 테스트")] //컨텍스트 메뉴로도 코루틴 작동하는지?
    void FindPath(Vector2Int goalPos)
    {
        StopAllCoroutines();
        StartCoroutine(FindPathCo(goalPos));
    }
    internal bool ShowAttackableArea()
    {
        //bool existEnemy = false; //적이 존재하는지 확인
        Vector2Int currentPos = transform.position.ToVector2Int(); //현재 위치에서 공격 가능한 범위를 확인
        var map = GroundManager.Instance.blockInfoMap;

        foreach (var item in attackableLocalPositions) //공격 가능한 위치에 적이 있는지 확인???? 
        {
            Vector2Int pos = item + currentPos; //item(공격가능한 지점)의 월드 위치와 플레이어의 위치!

            if (map.ContainsKey(pos)) //position 키가 있을 때만 조건으로 들어가도록 (비어있지 않은 땅에 대해서만 검사)
            {
                if (IsEnemyExist(map[pos])) //map[pos]에 적이 있는지 확인
                {
                    enemyExistPoint.Add(map[pos]);
                }
            }
        }
        enemyExistPoint.ForEach(x => x.ChangeColor(Color.red));

        return enemyExistPoint.Count > 0;
    }

    public List<BlockInfo> enemyExistPoint = new List<BlockInfo>();

    protected override void OnCompleteMove()
    {
        bool existAttackTarget = ShowAttackableArea();
        if (existAttackTarget)
            StageManager.GameState = GameStateType.SelectAttackTarget;
        else
            StageManager.GameState = GameStateType.SelectPlayer;
    }
    public void ClearEnemyExistPoint()
    {
        enemyExistPoint.ForEach(x => x.ChangeToOriginalColor());
        enemyExistPoint.Clear();
    }

    public Ease moveEase = Ease.Linear;

    internal bool CanAttackTarget(Character enemy)
    {
        if (enemy.CharacterType != CharacterTypeEnum.Monster) //다른 플레이어를 공격하지 않게한다.
            return false;
        if (IsInAttackableArea(enemy.transform.position) == false) //공격 가능한 범위 안에 있는지 확인하고 
            return false;

        return true;
    }

    internal void AttackTarget(Monster character)
    {
        ClearEnemyExistPoint();

        StartCoroutine(AttackTargetCo_(character));
    }

    public override BlockType GetBlockType()
    {
        return BlockType.Player;
    }

    protected IEnumerator AttackTargetCo_(Monster monster)
    {
        yield return AttackTargetCo(monster);

        if (monster.status == StatusType.Die)
        {
            AddExp(monster.rewardExp);
        }

        StageManager.GameState = GameStateType.SelectPlayer;
        //기존 AttackTarget 코루틴이 끝난 다음에 실행되도록 해야 플레이어가 정상적으로 공격하고 GameState가 변하도록 작동한다. 
        //코루틴으로 한 번 더 감싸서 몬스터와 플레이어가 똑같은 AttackTargetCo 메서드를 사용할 수 있도록 바꿔줬다. 
    }

    public SaveInt exp, level;
    public int maxExp;
    public SaveString comment;

    private void AddExp(int rewardExp)
    {
        //플레이어의 기존 경험치에 몬스터를 죽이면서 얻은 경험치 추가


        //최대 경험치를 넘으면 레벨 업!


        //PlayerPrefs.SetInt("exp", exp); //플레이어의 경험치를 저장
        //PlayerPrefs.Save(); //SetInt에서 지정한 키의 exp 값을 저장

        //exp = PlayerPrefs.GetInt("exp"); // PlayerPrefs로 저장한 값을 불러오는 것
    }

    private bool IsEnemyExist(BlockInfo blockInfo)
    {
        if (blockInfo.blockType.HasFlag(BlockType.Monster) == false)
            return false;

        Debug.Assert(blockInfo.character != null, "캐릭터는 꼭 있어야합니다");

        return true;
    }


    internal bool OnMoveable(Vector3 position, int maxDistance)
    {
        Vector2Int goalPos = position.ToVector2Int();
        Vector2Int playerPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap; //모든 블록정보를 가지고 있는 map
        var path = PathFinding2D.find4(playerPos, goalPos, (Dictionary<Vector2Int, BlockInfo>)map, passableValues);
        //블록 정보를 이용해서 해당 블록과 플레이어 사이의 경로를 찾는다.


        if (path.Count == 0 || path.Count > maxDistance + 1) //처음 path의 값은 자기 자신의 위치값이므로 maxDistance+1을 해준다.. 
            return false;

        //if (path.Count == 0)
        //    Debug.Log("길이 없다!");
        //else if (path.Count > maxDistance + 1)
        //    Debug.Log("이동할 수 없다!");
        //else
        //    return true;

        return true;
    }

    protected override void OnDie()
    {
        //플레이어가 죽은 경우에는
        //모든 플레이어가 죽었는지 확인하고 모든 플레이어가 죽었다면 게임오버
        if (Players.Where(x => x.status != StatusType.Die).Count() == 0)
        {
            CenterNotifyUI.Instance.Show("게임오버");
        }
    }
}
