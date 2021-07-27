using UnityEngine;

public class FollowTarget : SingletonMonoBehavior<FollowTarget>
{
    Transform target;
    public Vector3 offset = new Vector3(0,0,-7);
    public void SetTarget(Transform target) // 타겟의 transform을 가져와 target멤버변수 값 할당
    {
        this.target = target;
        if(target)
        {
            var pos = target.position; //카메라의 기존 높이를 유지해야 카메라가 땅 밑으로 가는 버그를 막을 수 있다. 

            transform.position = pos + offset;
        }
    }

    //void LateUpdate() //모든 Update함수가 호출 된 다음에 레이트업데이트
    //{
    //    if (target == null)
    //        return;

    //    var newPos = target.position + offset;

    //    new
    //    newPos.y = transform.position.y;
    //    transform.position = newPos;
    //}
}
