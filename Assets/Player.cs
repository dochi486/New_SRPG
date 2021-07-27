using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public static List<Player> Players = new List<Player>();
    public override CharacterTypeEnum CharacterType { get => CharacterTypeEnum.Player; }

    static public Player SelectedPlayer;
    //Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        //SelectedPlayer = this; //왜 주석처리했을까?
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player, this);
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
        Vector2Int currentPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;

        foreach (var item in attackableLocalPositions) //공격 가능한 위치에 적이 있는지 확인???? 
        {
            Vector2Int pos = item + currentPos; //item(공격가능한 지점)의 월드 위치와 플레이어의 위치!

            if (map.ContainsKey(pos)) //position 키가 있을 때만 조건으로 들어가도록 (비어있지 않은 땅에 대해서만 검사)
            {
                if (IsEnemyExist(map[pos]))
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

    internal bool CanAttackTarget(Character character)
    {
        if (character.CharacterType != CharacterTypeEnum.Monster)
            return false;

        return true;
    }

    internal void AttackTarget(Character character)
    {
        ClearEnemyExistPoint();

        StartCoroutine(AttackTargetCo(character));
    }

    public float attackTime = 1;
    private IEnumerator AttackTargetCo(Character attackTarget)
    {
        transform.LookAt(attackTarget.transform); 

        animator.Play("Attack");
        attackTarget.TakeHit(power);
        yield return new WaitForSeconds(attackTime);

        completeAct = true;
        StageManager.GameState = GameStateType.SelectPlayer;

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
        var map = GroundManager.Instance.blockInfoMap;
        var path = PathFinding2D.find4(playerPos, goalPos, (Dictionary<Vector2Int, BlockInfo>)map, passableValues);

        if (path.Count == 0 || path.Count > maxDistance + 1)
            return false;

        //if (path.Count == 0)
        //    Debug.Log("길이 없다!");
        //else if (path.Count > maxDistance + 1)
        //    Debug.Log("이동할 수 없다!");
        //else
        //    return true;

        return true;
    }
}
