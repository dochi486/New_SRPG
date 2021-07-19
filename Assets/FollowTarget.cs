using UnityEngine;

public class FollowTarget : SingletonMonoBehavior<FollowTarget>
{
    Transform target;
    public Vector3 offset = new Vector3(0,0,-7);
    public void SetTarget(Transform target)
    {
        this.target = target;
    }
    // Update is called once per frame
    void LateUpdate() //모든 Update함수가 호출 된 다음에 레이트업데이트
    {
        if (target == null)
            return;

        var newPos = target.position + offset;
        newPos.y = transform.position.y;
        transform.position = newPos;
    }
}
