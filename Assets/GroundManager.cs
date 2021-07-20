using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GroundManager : SingletonMonoBehavior<GroundManager>
{
   
    public Transform player;
    public Dictionary<Vector2Int, BlockType> map = new Dictionary<Vector2Int, BlockType>(); //맵의 좌표로 정보 접근(맵을 지정)
    public Dictionary<Vector2Int, BlockInfo> blockInfoMap = new Dictionary<Vector2Int, BlockInfo>();
    public BlockType passableValues = BlockType.Water | BlockType.Walkable; //갈 수 있는 지역(밟을 수 있는 타일)을 int로 받는 것
    //public Vector2Int goalPos; //목표지점

    public bool useDebugMode = true;
    public GameObject debugTextPrefab;
    public List<GameObject> debugTextGos = new List<GameObject>();//디버그텍스트로그가 클릭할 때마다 생성되던 것 고치는 부분
    //퍼블릭일 때는 new안해도 사용 가능하지만 private으로 바꿀 떄 깜빡할 수 있으니 new써주는 게 좋음

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

    internal void RemoveBlockInfo(Vector3 position, BlockType removeBlockType)
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
