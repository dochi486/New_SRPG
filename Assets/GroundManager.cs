using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GroundManager : SingletonMonoBehavior<GroundManager>
{
    public Dictionary<Vector2Int, int> map = new Dictionary<Vector2Int, int>(); //맵의 좌표로 정보 접근
    public List<int> passableValues = new List<int>(); //갈 수 있는 지역(밟을 수 있는 타일)
    public Vector2Int goalPos; //목표지점

    internal void OnTouch(Vector3 position)
    {

        Vector2Int findPos = new Vector2Int((int)position.x, (int)position.z);
        FindPath(findPos);
    }

    public Vector2Int playerPos; //시작지점

    public Transform player;
    //public Transform goal;


    //[ContextMenu("길 찾기 테스트")] //컨텍스트 메뉴로도 코루틴 작동하는지?
    void FindPath(Vector2Int goalPos)
    {
        StopAllCoroutines();
        StartCoroutine(FindPathCo(goalPos));
    }

    IEnumerator FindPathCo(Vector2Int goalPos)
    {
        passableValues = new List<int>();
        passableValues.Add((int)BlockType.Walkable);

        //자식 오브젝트의 blockinfo 가져오기
        var blockInfos = GetComponentsInChildren<BlockInfo>();

        //map 딕셔너리를 채운다
        foreach (var item in blockInfos)
        {
            var pos = item.transform.position;
            Vector2Int intPos = new Vector2Int((int)pos.x, (int)pos.z);
            map[intPos] = (int)item.blockType; //맵 정보 초기화
        }
        playerPos.x = (int)player.position.x;
        playerPos.y = (int)player.position.z; //벡터2를 쓰고 있지만 실제로 y말고 z축의 값을 사용하고 있기 때문에 암시적 형변환+z사용

        //goalPos.x = (int)goal.position.x;
        //goalPos.y = (int)goal.position.z;

        var path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);
        if (path.Count == 0)
            Debug.Log("길이 없다.");
        else
        {
            Player.SelectedPlayer.PlayAnimation("Walk");
            FollowTarget.Instance.SetTarget(Player.SelectedPlayer.transform);
            foreach (var item in path)
            {
                Vector3 playerNewPos = new Vector3(item.x, 0, item.y);
                player.LookAt(playerNewPos);
                //player.position = playerNewPos;
                player.DOMove(playerNewPos, moveTimePerUnit).SetEase(moveEase);
                yield return new WaitForSeconds(moveTimePerUnit);
            }
            Player.SelectedPlayer.PlayAnimation("Idle");
            FollowTarget.Instance.SetTarget(null) ;
        }
    }
    public Ease moveEase = Ease.InBounce;
    public float moveTimePerUnit = 0.3f; //한 칸 이동할 때 걸리는 시간
}
