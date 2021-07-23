using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public override CharacterTypeEnum CharacterType { get => CharacterTypeEnum.Player; }

    static public Player SelectedPlayer;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        SelectedPlayer = this;
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player, this);
        //현재 플레이어가 있는 블록(walkable)에 player타입도 지정
        FollowTarget.Instance.SetTarget(transform);
    }
    public void PlayAnimation(string nodName)
    {
        animator.Play(nodName, 0, 0);
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

    public BlockType passableValues = BlockType.Walkable | BlockType.Water;

    IEnumerator FindPathCo(Vector2Int goalPos)
    {
        //passableValues = new List<int>(); 
        //passableValues.Add((int)BlockType.Walkable); //지나갈 수 있는 타일을 int로 저장
        Transform player = transform;
        Vector2Int playerPos = new Vector2Int(Mathf.RoundToInt(player.position.x), Mathf.RoundToInt(player.position.z)); //시작지점
        //자식 오브젝트의 blockinfo 가져오기

        //map 딕셔너리를 채운다

        playerPos.x = Mathf.RoundToInt(player.position.x); // 플레이어의 위치 저장
        playerPos.y = Mathf.RoundToInt(player.position.z); //벡터2를 쓰고 있지만 실제로 y말고 z축의 값을 사용하고 있기 때문에 암시적 형변환+z사용
        var map = GroundManager.Instance.blockInfoMap;
        //goalPos.x = (int)goal.position.x;
        //goalPos.y = (int)goal.position.z;
        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, (Dictionary<Vector2Int, BlockInfo>)map, passableValues);

        if (path.Count == 0)
            Debug.Log("길이 없다.");
        else
        {
            GroundManager.Instance.RemoveBlockInfo(Player.SelectedPlayer.transform.position, BlockType.Player);
            //플레이어가 이동하면 원래 있던 위치의 블록타입에서 Player타입 제거
            Player.SelectedPlayer.PlayAnimation("Walk");
            // FollowTarget의 SetTarget을 실행시켜 선택된 캐릭터를 카메라가 따라가게 하자
            FollowTarget.Instance.SetTarget(Player.SelectedPlayer.transform); //FollowTarget의 SetTarget을 실행하여 선택한 캐릭터를 카메라가 따라간다
            path.RemoveAt(0); //처음에 자기가 위치한 블럭의 인덱스를 삭제해서 제자리에서 애니메이션하지 않도록
            foreach (var item in path) //길이 있다면 path에 저장된 위치를 하나씩 불러와 이동시키는 것
            {
                Vector3 playerNewPos = new Vector3(item.x, 0, item.y);
                player.LookAt(playerNewPos);
                // 플레이어가 움직일 때 자연스럽게 움직이도록 하자
                // DOMove함수는 DOTween을 임포트하여 가져온 함수
                //player.position = playerNewPos;
                player.DOMove(playerNewPos, moveTimePerUnit).SetEase(moveEase);
                // 움직이는 시간 만큼 기다리자
                yield return new WaitForSeconds(moveTimePerUnit);
            }
            Player.SelectedPlayer.PlayAnimation("Idle");
            // 이동이 끝나면 Idle애니메이션을 실행시키자
            FollowTarget.Instance.SetTarget(null);
            // null을 주어 카메라가 따라가지 않도록 하자
            GroundManager.Instance.AddBlockInfo(Player.SelectedPlayer.transform.position, BlockType.Player, this);
            // 이동한 위치에는 플레이어 정보 추가

            bool existAttackTarget = ShowAttackableArea();
            if (existAttackTarget)
                StageManager.GameState = GameStateType.SelectAttackTarget;
            else
                StageManager.GameState = GameStateType.SelectPlayer;
        }
    }


    public Ease moveEase = Ease.Linear;
    public float moveTimePerUnit = 0.3f; //한 칸 이동할 때 걸리는 시간

    internal bool CanAttackTarget(Character character)
    {
        if (character.CharacterType != CharacterTypeEnum.Monster)
            return false;

        return true;
    }

    internal void AttackTarget(Character character)
    {
        throw new NotImplementedException();
    }


    internal bool ShowAttackableArea()
    {
        bool existEnemy = false; //적이 존재하는지 확인
        Vector2Int currentPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;

        foreach (var item in attackablePoints) //공격 가능한 위치에 적이 있는지 확인???? 
        {
            Vector2Int pos = item + currentPos; //item(공격가능한 지점)의 월드 위치와 플레이어의 위치!

            if (map.ContainsKey(pos)) //position 키가 있을 때만 조건으로 들어가도록 (비어있지 않은 땅에 대해서만 검사)
            {
                if (IsEnemyExist(map[pos]))
                {
                    map[pos].ChangeColor(Color.red);
                    existEnemy = true;
                }
            }
        }
        return existEnemy;
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
        if (path.Count == 0)
            Debug.Log("길이 없다!");
        else if (path.Count > maxDistance + 1)
            Debug.Log("이동할 수 없다!");
        else
            return true;

        return false;
    }
}
