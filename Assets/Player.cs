using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    static public Player SelectedPlayer;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        SelectedPlayer = this;
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player);
    }
    internal void OnTouch(Vector3 position)
    {
        Vector2Int findPos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
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
        var map = GroundManager.Instance.map;
        //goalPos.x = (int)goal.position.x;
        //goalPos.y = (int)goal.position.z;

        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);

        if (path.Count == 0)
            Debug.Log("길이 없다.");
        else
        {
            GroundManager.Instance.RemoveBlockInfo(Player.SelectedPlayer.transform.position, BlockType.Player);
            Player.SelectedPlayer.PlayAnimation("Walk");
            // FollowTarget의 SetTarget을 실행시켜 선택된 캐릭터를 카메라가 따라가게 하자
            FollowTarget.Instance.SetTarget(Player.SelectedPlayer.transform);
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
            GroundManager.Instance.AddBlockInfo(Player.SelectedPlayer.transform.position, BlockType.Player);
        }
    }


    public Ease moveEase = Ease.Linear;
    public float moveTimePerUnit = 0.3f; //한 칸 이동할 때 걸리는 시간

    public void PlayAnimation(string nodName)
    {
        animator.Play(nodName, 0, 0);
    }
}
