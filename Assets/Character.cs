using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum StatusType
{
    Normal,
    Sleep,
    Die,
}

public enum CharacterTypeEnum
{
    NotInit,
    Player,
    Monster,
}

public class Character : MonoBehaviour //플레이어와 몬스터에 대한 기본적인 정보를 가지고 있는 클래스
{
    public string nickName = "이름";
    public string iconName;
    public int power = 10;
    public float hp = 10;
    public float mp = 0;
    public StatusType status;
    public int maxHp = 50;
    public int maxMp = 20;

    public bool completeMove;
    public bool completeAct;

    public bool CompleteTurn { get => completeMove && completeAct; }

    public List<Vector2Int> attackablePoints = new List<Vector2Int>(); //공격 가능한 위치를 모아두는 리스트
    public int moveDistance = 5; //움직일 수 있는 영역 표시하기 위한 변수

    protected Animator animator;
    protected void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        var attackPoints = GetComponentsInChildren<AttackPoint>(true); //파라미터를 true로 줬기 때문에 오브젝트가 꺼져있더라도 가져올 수 있다
        //공격 가능한 범위를 모아두는 부분
        foreach (var item in attackPoints) //바로 앞에 있는 공격 가능한 지점
            attackablePoints.Add(item.transform.localPosition.ToVector2Int());

        //오른쪽에 있는 공격 가능한 지점들
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints) //오른쪽에 있는 공격 가능한 지점
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int());

        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints) //아래쪽에 있는 공격 가능한 지점
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int());

        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints) //왼쪽에 있는 공격 가능한 지점
            attackablePoints.Add((item.transform.position - transform.position).ToVector2Int());

        transform.Rotate(0, 90, 0); //다시 원래 방향(앞)을 보도록 회전 시킨다

    }
    public virtual CharacterTypeEnum CharacterType { get => CharacterTypeEnum.NotInit; }

    public float takeHitTime = 0.3f;

    public BlockType passableValues = BlockType.Walkable | BlockType.Water;
    internal IEnumerator TakeHit(int power)
    {
        GameObject damageTextGoInResource = (GameObject)Instantiate(Resources.Load("DamageText"));
        var pos = transform.position;
        pos.y = 1.71f; //리소스에서 불러오는 데미지텍스트의 y축 값을 항상 고정하여 똑같은 위치에 생성 되도록한다. 
        GameObject damageTextGo = Instantiate(damageTextGoInResource, pos, damageTextGoInResource.transform.rotation, transform); //transform(몬스터)를 부모로 하여 생성된다.
        damageTextGoInResource.transform.localPosition = new Vector3(0, 1.71f, 0); //몬스터로부터 y축으로 1.3f만큼 위에 데미지텍스트 프리팹 생성
        damageTextGo.GetComponent<TextMeshPro>().text = power.ToString(); //플레이어의 power를 string으로 변환하여 텍스트메쉬프로에 대입한다
        Destroy(damageTextGo, 2); //Destroy의 첫번째 파라미터는 파괴할 대상, 두 번째 파라미터는 딜레이시간

        hp -= power;
        animator.Play("TakeHit");

        yield return new WaitForSeconds(takeHitTime);

        if (hp <= 0)
        {
            animator.Play("Die");
            status = StatusType.Die;

            OnDie();
        }
    }

    protected virtual void OnDie()
    {
        Debug.LogError("자식 클래스에서 오버라이드 해서 사용해야한다. 여기서 호출되면 안됨");
    }

    protected virtual void OnDestroy()
    {
        GroundManager.Instance.RemoveBlockInfo(transform.position, GetBlockType());
    }

    public virtual BlockType GetBlockType()
    {
        Debug.LogError("자식에서 GetBlockType함수 오버라이드 필수!");
        return BlockType.None;
    }

    protected virtual void OnCompleteMove()
    {
        //부모 클래스에서 virtual로 선언해놓고 실제로는 자식 클래스에서 override해서 사용
    }
    public void PlayAnimation(string nodName)
    {
        animator.Play(nodName, 0, 0);
    }

    protected IEnumerator FindPathCo(Vector2Int goalPos)
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

            completeAct = true;
        }
    }

    internal bool ShowAttackableArea()
    {
        //bool existEnemy = false; //적이 존재하는지 확인
        Vector2Int currentPos = transform.position.ToVector2Int();
        var map = GroundManager.Instance.blockInfoMap;

        foreach (var item in attackablePoints) //공격 가능한 위치에 적이 있는지 확인???? 
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

}