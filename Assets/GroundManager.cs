using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GroundManager : SingletonMonoBehavior<GroundManager>
{
    public Vector2Int playerPos; //시작지점
    public Transform player;
    public Dictionary<Vector2Int, BlockType> map = new Dictionary<Vector2Int, BlockType>(); //맵의 좌표로 정보 접근(맵을 지정)
    public Dictionary<Vector2Int, BlockInfo> blockInfoMap = new Dictionary<Vector2Int, BlockInfo>();
    public BlockType passableValues = BlockType.Water | BlockType.Walkable; //갈 수 있는 지역(밟을 수 있는 타일)을 int로 받는 것
    //public Vector2Int goalPos; //목표지점

    public bool useDebugMode = true;
    public GameObject debugTextPrefab;

    new private void Awake()
    {
        base.Awake();

        var blockInfos = GetComponentsInChildren<BlockInfo>();


        debugTextGos.ForEach(x => Destroy(x));
        debugTextGos.Clear(); //디버그텍스트로그가 클릭할 때마다 생성되던 것 고치는 부분

        foreach (var item in blockInfos)
        {
            var pos = item.transform.position;
            Vector2Int intPos = new Vector2Int((int)pos.x, (int)pos.z); // 블록들의 x,z 좌표 저장
            map[intPos] = item.blockType; //맵 정보 초기화(dictionary에 (블록의 위치, 블록의 타입) 저장)

            if (useDebugMode)
            {
                item.UpdateDebugInfo();
                //StringBuilder debugText = new StringBuilder();/*= $"{intPos.x}:{intPos.y}"*/;
                ////ContainingText(debugText, item, BlockType.Walkable);

                ////item.name = $"{item.name}: {posString}";

                //GameObject textMeshGo = Instantiate(debugTextPrefab, item.transform); // item은 blockinfo
                //debugTextGos.Add(textMeshGo); //디버그텍스트로그가 클릭할 때마다 생성되던 것 고치는 부분
                //textMeshGo.transform.localPosition = Vector3.zero;
                //TextMesh textMesh = textMeshGo.GetComponentInChildren<TextMesh>();
                //textMesh.text = debugText.ToString();
            }
            blockInfoMap[intPos] = item;
        }
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

    public List<GameObject> debugTextGos = new List<GameObject>();//디버그텍스트로그가 클릭할 때마다 생성되던 것 고치는 부분
    //퍼블릭일 때는 new안해도 사용 가능하지만 private으로 바꿀 떄 깜빡할 수 있으니 new써주는 게 좋음

    IEnumerator FindPathCo(Vector2Int goalPos)
    {
        //passableValues = new List<int>(); 
        //passableValues.Add((int)BlockType.Walkable); //지나갈 수 있는 타일을 int로 저장

        //자식 오브젝트의 blockinfo 가져오기
        
        //map 딕셔너리를 채운다
       
        playerPos.x = Mathf.RoundToInt(player.position.x); // 플레이어의 위치 저장
        playerPos.y = Mathf.RoundToInt(player.position.z); //벡터2를 쓰고 있지만 실제로 y말고 z축의 값을 사용하고 있기 때문에 암시적 형변환+z사용

        //goalPos.x = (int)goal.position.x;
        //goalPos.y = (int)goal.position.z;

        List<Vector2Int> path = PathFinding2D.find4(playerPos, goalPos, map, passableValues);

        if (path.Count == 0)
            Debug.Log("길이 없다.");
        else
        {
            RemoveBlockInfo(Player.SelectedPlayer.transform.position, BlockType.Player);
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
        }
    }



    public Ease moveEase = Ease.Linear;
    public float moveTimePerUnit = 0.3f; //한 칸 이동할 때 걸리는 시간

    internal void AddBlockInfo(Vector3 position, BlockType addBlockType)
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        if(map.ContainsKey(pos) == false)
        {
            Debug.LogError($"{pos} 위치에 맵이 없다");
        }

        map[pos] |= addBlockType;
        blockInfoMap[pos].blockType |= addBlockType;
        if (useDebugMode)
            blockInfoMap[pos].UpdateDebugInfo();
    }

    private void RemoveBlockInfo(Vector3 position, BlockType removeBlockType)
    {
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        if (map.ContainsKey(pos) == false)
        {
            Debug.LogError($"{pos} 위치에 맵이 없다");
        }

        map[pos] &= ~removeBlockType;               // 기존 값에서 삭제하겠다.
        blockInfoMap[pos].blockType &= ~removeBlockType;
        if (useDebugMode)
            blockInfoMap[pos].UpdateDebugInfo();
    }
}
