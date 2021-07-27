using DG.Tweening;
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

    new protected void Awake()
    {
        base.Awake();
        Players.Add(this);
    }
    new protected void OnDestroy()
    {
        base.OnDestroy();
        Players.Remove(this);
    }

    // Start is called before the first frame update
    void Start()
    {
        //SelectedPlayer = this; //왜 주석처리했을까?
        //animator = GetComponentInChildren<Animator>();
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

        //if (monster.status == StatusType.Die)
        //{
        //    AddExp(monster.rewardExp);
        //}

        StageManager.GameState = GameStateType.SelectPlayer;
        //기존 AttackTarget 코루틴이 끝난 다음에 실행되도록 해야 플레이어가 정상적으로 공격하고 GameState가 변하도록 작동한다. 
        //코루틴으로 한 번 더 감싸서 몬스터와 플레이어가 똑같은 AttackTargetCo 메서드를 사용할 수 있도록 바꿔줬다. 
    }

    private void AddExp(object rewardExp)
    {
        throw new NotImplementedException();
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
