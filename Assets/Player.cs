using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
        //comment = new SaveString("comment" + ID);
        InitLevelData();
    }

    private void InitLevelData() //레벨과 경험치, 최대 경험치를 초기화하는 함수
    {

        var log = PlayerPrefs.GetString(PlayerDataKey);
        print(log);
        data = JsonUtility.FromJson<PlayerData>(log);
        //exp = new SaveInt("exp" + ID); //키가 항상 달라야 독립된 값을 저장할 수 있기 때문에 ID와 exp를 조합해서 플레이어 각각의 밸류를 가질 수 있따. 
        //level = new SaveInt("level" + ID, 1); //키는 절대 중복되면 안된다!
        //maxExp = level.Value * 10; //보통은 더 복잡한 데이터테이플로 레벨에 따른 값을 설정하지만 일단은 이렇게!
        //maxExp = GlobalData.Instance.playerDatas.Find(x => x.level == level.Value).maxExp;
        //maxExp = GlobalData.Instance.playerDataMap[level.Value].maxExp;
        //리스트의 인덱스 순서와 관계 없이 플레이어의 레벨과 리스트에 있는 레벨이 같아야 호출되는 코드
        //성능 저하가 생길 수도 있는 코드지만 그렇게 성능 저하가 체감될만큼 빈번하게 호출되지 않기 때문에 이렇게 사용.
        //딕셔너리로 하면 리스트의 인덱스가 꼬이더라도 상관이 없지만 딕셔너리로 하게 되면 인스펙터에서 확인이 불가하기 때문에
        //일단은 리스트로 사용.. ! 
        //List와 map을 사용하는 게 딕셔너리의 값을 노출하는 게 List고 map은 실제 데이터를 담는 딕셔너리로 사용하기 위해서였따!
        SetLevelData();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            AddExp(5);
    }

    private void SetLevelData()
    {
        var data = GlobalData.Instance.playerDataMap[level];
        maxExp = data.maxExp;
        hp = maxHp = data.maxHp; //초기화하는 부분이니까 레벨이 변할 때마다 현재 상태와 최대값을 기본값으로 초기화
        mp = maxMp = data.maxMp;
    }

    new protected void OnDestroy()
    {
        base.OnDestroy();
        Players.Remove(this);

        SaveData();
    }

    private void SaveData() //json에 플레이어의 정보를 저장하여 앱을 재시작해도 정보 불러올 수 있게한다. 
    {
        string json = JsonUtility.ToJson(data);

        try
        {
            PlayerPrefs.SetString(PlayerDataKey, json);
            Debug.Log("json:" + json);
        }
        catch(System.Exception err)
        {
            Debug.Log("Got:" + err);
        }
    }

    //[ContextMenu("저장확인 테스트")] //꼭 플레이 중일 때만 테스트 가능

    //void TestFn() //플레이를 멈췄다가 다시 실행했을 때 SaveInt, SaveString한 것들이 살아있는지 테스트 하는 함수
    //{
    //    exp.Value += 1;
    //    //comment.Value += "a";
    //}

    // Start is called before the first frame update
    void Start()
    {
        //SelectedPlayer = this; //왜 주석처리했을까?
        //animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player, this); //플레이어가 처음에 서 있는 블록은 walkable밖에 지정 안되어있다.
        //현재 플레이어가 있는 블록(walkable)에 player타입도 지정
        FollowTarget.Instance.SetTarget(transform);
        //InitLevelData();
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

        //도착한 지점에 아이템이 있다면 획득한다.
        var intPos = transform.position.ToVector2Int();
        //어떤 아이템이 있는지 확인
        int itemID = GroundManager.Instance.blockInfoMap[intPos].dropItemID;
        if(itemID > 0) //itemID가 0보다 크면 아이템이 존재한다는 뜻이므로 획득하게 한다. 
        {
            //플레이어가 아이템을 획득하고
            AddItem(itemID);

            //블럭에서 아이템의 정보를 삭제한다.
            GroundManager.Instance.RemoveItem(transform.position); //아이템 스프라이트, 블럭타입 모두를 삭제하는 메서드 새로 생성
            //GroundManager.Instance.RemoveBlockInfo(transform.position, BlockType.Item);
        }
    }

    public PlayerData data;
    public class PlayerData
    {
        public List<int> myItem = new List<int>(); //가지고 있는 아이템 정보를 담는 리스트
        public int exp;
        public int level;

    }

    string PlayerDataKey => "PlayerData" + ID;

    private void AddItem(int itemID) //다른 로직과 섞이지 않도록 새로 함수 구현!
    {
        data.myItem.Add(itemID); //itemID가 myItem리스트에 들어가도록 한다.
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

            if (monster.dropItemGroup.ratio > Random.Range(0, 1f))
                DropItem(monster.dropItemGroup.dropItemID, monster.transform.position); //몬스터가 가지고 있는 드롭아이템그룹 아이디를 가지고 그 아이템 그룹을 드랍시킨다. 


        }

        StageManager.GameState = GameStateType.SelectPlayer;
        //기존 AttackTarget 코루틴이 끝난 다음에 실행되도록 해야 플레이어가 정상적으로 공격하고 GameState가 변하도록 작동한다. 
        //코루틴으로 한 번 더 감싸서 몬스터와 플레이어가 똑같은 AttackTargetCo 메서드를 사용할 수 있도록 바꿔줬다. 
    }

    [ContextMenu("드랍 테스트")]
    void DropTestTemp()
    {
        DropItem(1);
    }

    private void DropItem(int dropGroupID, Vector3? position = null) //포지션 값이 없으면 null?
    {
        var dropGroup = GlobalData.Instance.dropItemGroupDataMap[dropGroupID];
        var dropItemRatioInfo = dropGroup.dropItems.OrderByDescending(x => x.ratio * Random.Range(0, 1f)).First();
        //드랍확률 * 0~100% 사이의 값 곱한 것 중 하나만 사용한다 First
        //아이템의 드랍확률 정보를 가지고 있는 var
        print($"{dropItemRatioInfo.dropItemID}, {dropItemRatioInfo.ratio}");

        var dropItem = GlobalData.Instance.itemDataMap[dropItemRatioInfo.dropItemID];
        print(dropItem.ToString());

        GroundManager.Instance.AddBlockInfo(position.Value, BlockType.Item, dropItem);
    }

    public int exp
    {
        set { data.exp = value; }
        get { return data.level; }
    }
    public int level
    {
        set { data.exp = value; }
        get { return data.level; }
    }
    //public SaveString comment;
    public int maxExp;

    private void AddExp(int rewardExp)
    {
        //플레이어의 기존 경험치에 몬스터를 죽이면서 얻은 경험치 추가
        exp += rewardExp;

        //최대 경험치를 넘으면 레벨 업!
        if (exp >= maxExp)
        {
            exp = exp - maxExp;  //exp를 0으로 만드는 부분
            //원래는 SetLevelData()밑에서 실행했는데 그렇게 하면 exp값이 음수가 되어버려서 이걸 먼저 실행한 뒤에 
            //SetLevelData()해주도록 위치 변경

            level++; //레벨의 값이 증가해야 맵에서 읽어올 수 있다. 
            SetLevelData(); //레벨이 오르면 hp,mp회복

            CenterNotifyUI.Instance.Show($"Lv{level}이 되었습니다.");
        }

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
