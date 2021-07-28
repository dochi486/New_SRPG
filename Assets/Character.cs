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

    public List<Vector2Int> attackableLocalPositions = new List<Vector2Int>(); //공격 가능한 위치를 모아두는 리스트
    public int moveDistance = 5; //움직일 수 있는 영역 표시하기 위한 변수

    protected Animator animator;
    protected void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        var attackPoints = GetComponentsInChildren<AttackPoint>(true); //파라미터를 true로 줬기 때문에 오브젝트가 꺼져있더라도 가져올 수 있다
        //공격 가능한 범위를 모아두는 부분
        foreach (var item in attackPoints) //바로 앞에 있는 공격 가능한 지점
            attackableLocalPositions.Add(item.transform.localPosition.ToVector2Int());

        //오른쪽에 있는 공격 가능한 지점들
        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints) //오른쪽에 있는 공격 가능한 지점
            attackableLocalPositions.Add((item.transform.position - transform.position).ToVector2Int());

        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints) //아래쪽에 있는 공격 가능한 지점
            attackableLocalPositions.Add((item.transform.position - transform.position).ToVector2Int());

        transform.Rotate(0, 90, 0);
        foreach (var item in attackPoints) //왼쪽에 있는 공격 가능한 지점
            attackableLocalPositions.Add((item.transform.position - transform.position).ToVector2Int());

        transform.Rotate(0, 90, 0); //다시 원래 방향(앞)을 보도록 회전 시킨다

    }
    public virtual CharacterTypeEnum CharacterType { get => CharacterTypeEnum.NotInit; }

    public float takeHitTime = 0.3f;

    public BlockType passableValues = BlockType.Walkable | BlockType.Water;
    public float moveTimePerUnit = 0.3f; //한 칸 이동할 때 걸리는 시간

    public float attackTime = 1;
    internal IEnumerator TakeHitCo(int power)
    {
        GameObject damageTextGoInResource = (GameObject)Resources.Load("DamageText");
        var pos = transform.position; //공격 당한 캐릭터의 위치값
        pos.y = 1.71f; //리소스에서 불러오는 데미지텍스트의 y축 값을 항상 고정하여 똑같은 위치에 생성 되도록한다. 
        GameObject damageTextGo = Instantiate(damageTextGoInResource, 
            pos, damageTextGoInResource.transform.rotation, transform); //transform(몬스터)를 부모로 하여 생성된다.
        //damageTextGoInResource.transform.localPosition = new Vector3(0, 1.71f, 0); //몬스터로부터 y축으로 1.3f만큼 위에 데미지텍스트 프리팹 생성
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

    protected IEnumerator FindPathCo(Vector2Int destPos) //destination Position
    {
        //passableValues = new List<int>(); 
        //passableValues.Add((int)BlockType.Walkable); //지나갈 수 있는 타일을 int로 저장
        Transform myTr = transform; //이동하기 전 자기자신의 transform을 넣어준다.
        Vector2Int myPos = myTr.position.ToVector2Int(); //시작지점(이동하기 전 자기 자신의 위치 저장)
        //자식 오브젝트의 blockinfo 가져오기 <- map 사용 안하게 리팩토링하면서 지운 것
        //map 딕셔너리를 채운다 <- map 사용 안하게 리팩토링하면서 지운 것

        Vector3 myPosVector3 = myTr.position;
        //myPos.x = Mathf.RoundToInt(myTr.position.x); // 플레이어의 위치 저장
        //myPos.y = Mathf.RoundToInt(myTr.position.z); //벡터2를 쓰고 있지만 실제로 y말고 z축의 값을 사용하고 있기 때문에 암시적 형변환+z사용
        var map = GroundManager.Instance.blockInfoMap;
        //goalPos.x = (int)goal.position.x;
        //goalPos.y = (int)goal.position.z;
        List<Vector2Int> path = PathFinding2D.find4(myPos, destPos, map, passableValues);

        if (path.Count == 0)
            Debug.Log("길이 없다.");
        else
        {
            GroundManager.Instance.RemoveBlockInfo(myPosVector3,GetBlockType()); 
            //Player에 있던 FindPath를 부모 클래스로 끌어오기 하면서 플레이어만 사용하는 게 아니라 몬스터도 쓸 수 있도록 범용함수로 리팩토링
            //플레이어가 이동하면 원래 있던 위치의 블록타입에서 Player타입 제거
            PlayAnimation("Walk");
            // FollowTarget의 SetTarget을 실행시켜 선택된 캐릭터를 카메라가 따라가게 하자
            FollowTarget.Instance.SetTarget(myTr); //FollowTarget의 SetTarget을 실행하여 선택한 캐릭터를 카메라가 따라간다
            path.RemoveAt(0); //처음에 자기가 위치한 블럭의 인덱스를 삭제해서 제자리에서 애니메이션하지 않도록

            if (CharacterType == CharacterTypeEnum.Monster)
                path.RemoveAt(path.Count - 1); //캐릭터가 몬스터일 때 path의 마지막 인덱스 값(플레이어의 위치)을 삭제해야 플레이어 위치와 겹치지 않게 이동한다. 

            if (path.Count > moveDistance) //path에 담긴 위치들(path.Count)이 실제로 이동할 수 있는 범위보다 크면 
                path.RemoveRange(moveDistance, path.Count - moveDistance); 
                //RemoveRange(자를 범위 시작하는 인덱스, 자를 갯수);
                //moveDistance만큼만 움직일 수 있게 나머지 부분 절삭

            foreach (var item in path) //길이 있다면 path에 저장된 위치를 하나씩 불러와 이동시키는 것
            {
                Vector3 playerNewPos = new Vector3(item.x, myPosVector3.y, item.y);
                myTr.LookAt(playerNewPos);
                // 플레이어가 움직일 때 자연스럽게 움직이도록 하자
                // DOMove함수는 DOTween을 임포트하여 가져온 함수
                //player.position = playerNewPos;
                myTr.DOMove(playerNewPos, moveTimePerUnit);
                // 움직이는 시간 만큼 기다리자
                yield return new WaitForSeconds(moveTimePerUnit);
            }
            PlayAnimation("Idle");
            // 이동이 끝나면 Idle애니메이션을 실행시키자
            FollowTarget.Instance.SetTarget(null);
            // null을 주어 카메라가 따라가지 않도록 하자
            GroundManager.Instance.AddBlockInfo(myTr.position, GetBlockType(), this);
            // 이동한 위치에는 플레이어 정보 추가

            completeAct = true;

            OnCompleteMove();
        }
    }


    protected bool IsInAttackableArea(Vector3 enemyPosition) 
    {
        //적과 자신의 위치를 Vector2Int로 변환한다
        Vector2Int enemyPositionVector2 = enemyPosition.ToVector2Int(); 
        Vector2Int currentPos = transform.position.ToVector2Int();

        foreach (var item in attackableLocalPositions) //공격 가능한 범위에 적이 있는지 모든 공격 가능한 위치에 적이 있는지 확인
        {
            //pos는 공격 가능한 월드 포지션
            //item에는 공격 가능한 범위의 로컬 포지션이 들어있으니 현재 포지션을 더해서 월드 포지션으로 바꾼다. ?
            Vector2Int pos = item + currentPos;

            if (pos == enemyPositionVector2) //공격할 위치와 적의 위치가 같으면
                return true; //공격 가능한 범위에 적이 있다고 true반환
        }
        return false;
    }
    protected IEnumerator AttackTargetCo(Character attackTarget)
    {
        transform.LookAt(attackTarget.transform); 

        animator.Play("Attack");
        StartCoroutine(attackTarget.TakeHitCo(power)); //공격 타겟의 피격코루틴 TakeHitCo를 실행
        yield return new WaitForSeconds(attackTime);

        completeAct = true;
    }

}