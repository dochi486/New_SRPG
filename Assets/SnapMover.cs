using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SnapMover : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(Application.isPlaying)
        {
            Destroy(this); //플레이 중일 때는 이 컴포넌트 snap mover만 파괴하겠다는 의미
            //Destroy(gameObject)가 아니라 Destroy(this);를 해주는 이유 
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position.ToVector3Snap();
    }
}
