using System.Collections.Generic;
using UnityEngine;

static public class GroundExtension //Mathf.RoundToInt 매번 쓰니까 확장함수로 작성
{
    static public Vector2Int ToVector2Int(this Vector3 v3)
    {
        return new Vector2Int(Mathf.RoundToInt(v3.x), Mathf.RoundToInt(v3.z));
    }
    static public Vector3 ToVector2Int(this Vector2Int v2Int, int y)
    {
        return new Vector3(v2Int.x, y, v2Int.y);
    }
}


public class GroundManager : SingletonMonoBehavior<GroundManager>
{
    public Transform player;
    // 지나갈 수 있는 타입을 미리 저장해 맵 정보에 사용할 수 있도록 하자. 전의 코드는 int형으로 저장을 했었다
    // Walkable 과 Water(둘중 하나라도? 아마 "|" 때문에)로 지정된 블록은 지나다닐 수 있는 블록이다.
    public Dictionary<Vector2Int, BlockType> map = new Dictionary<Vector2Int, BlockType>(); //맵의 좌표로 정보 접근(맵을 지정)
    // A*에서 사용
    public Dictionary<Vector2Int, BlockInfo> blockInfoMap = new Dictionary<Vector2Int, BlockInfo>();// 맵 정보 에서 사용
    
    public BlockType passableValues = BlockType.Water | BlockType.Walkable; //갈 수 있는 지역(밟을 수 있는 타일)을 int로 받는 것
    //public Vector2Int goalPos; //목표지점

    public bool useDebugMode = true;
    public GameObject debugTextPrefab;
    public List<GameObject> debugTextGos = new List<GameObject>();//디버그텍스트로그가 클릭할 때마다 생성되던 것 고치는 부분
    //퍼블릭일 때는 new안해도 사용 가능하지만 private으로 바꿀 떄 깜빡할 수 있으니 new써주는 게 좋음

    new private void Awake()
    {
        base.Awake();

        var blockInfos = GetComponentsInChildren<BlockInfo>(); //블록들의 정보들을 가져온 리스트


        debugTextGos.ForEach(x => Destroy(x));
        debugTextGos.Clear(); //디버그텍스트로그가 클릭할 때마다 생성되던 것 고치는 부분

        foreach (var item in blockInfos)
        {
            var pos = item.transform.position;
            Vector2Int intPos = new Vector2Int((int)pos.x, (int)pos.z); // 블록들의 x,z 좌표 저장
            map[intPos] = item.blockType; //맵 정보 초기화(dictionary에 (블록의 위치, 블록의 타입) 저장)

            if (useDebugMode)
            {
                item.UpdateDebugInfo(); //블록들의 UpdateDebugINfo를 실행시켜 3D Text에 정보를 넣어 활성화

                //StringBuilder debugText = new StringBuilder();/*= $"{intPos.x}:{intPos.y}"*/;
                ////ContainingText(debugText, item, BlockType.Walkable);

                ////item.name = $"{item.name}: {posString}";

                //GameObject textMeshGo = Instantiate(debugTextPrefab, item.transform); // item은 blockinfo
                //debugTextGos.Add(textMeshGo); //디버그텍스트로그가 클릭할 때마다 생성되던 것 고치는 부분
                //textMeshGo.transform.localPosition = Vector3.zero;
                //TextMesh textMesh = textMeshGo.GetComponentInChildren<TextMesh>();
                //textMesh.text = debugText.ToString();
            }
            blockInfoMap[intPos] = item; //dictionary에 블록의 위치값, blockInfos(블록정보) 값을 넣는다.
        }
    }


    //블록에 추가로 타입을 넣어주기 위한 함수 
    internal void AddBlockInfo(Vector3 position, BlockType addBlockType, Character character)
    {
        // 실행한 곳의 position 정보를 담고 있는 pos를 생성
        Vector2Int pos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
        // 만일 pos의 값이 map에 저장한 블록들의 위치와 일치하는게 없다면
        if (map.ContainsKey(pos) == false)
        {
            Debug.LogError($"{pos} 위치에 맵이 없다");
        }

        map[pos] |= addBlockType; //맵 정보를 담고 있는 딕셔너리에 AddBlockInfo를 실행한 블록의 블록타입을 넣는다
        blockInfoMap[pos].blockType |= addBlockType;
        blockInfoMap[pos].character = character;
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

        map[pos] &= ~removeBlockType;  // 기존 값에서 삭제하겠다.
        blockInfoMap[pos].blockType &= ~removeBlockType; //비트 연산자? 플래그를 제거하는 부분 &= ~
        blockInfoMap[pos].character = null; //캐릭터를 null로 비워준다
        if (useDebugMode)
            blockInfoMap[pos].UpdateDebugInfo();
    }
}
