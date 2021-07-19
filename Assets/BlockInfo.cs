using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType
{
    Walkable,
    Water,
}

public class BlockInfo : MonoBehaviour
{
    public BlockType blockType;
    Vector3 downMousePosition;
    public float clickDistance = 1f;

    private void OnMouseDown()
    {
        // 마우스가 클릭되면 position을 저장하자
        downMousePosition = Input.mousePosition;  
    }

    private void OnMouseUp()
    {

        // 마우스를 떼면 실행
        // 마우스 뗀 위치를 저장하자
        var upMousePosition = Input.mousePosition;

        //처음 클릭했던 위치와 비교하여 clickDistance보다 크다면 나가자
        if (Vector3.Distance(downMousePosition, upMousePosition) > clickDistance)
        {
            Debug.Log($"downMousePosition : {downMousePosition}" + $"upMousePosition : {upMousePosition}");
            return;
        }
        //GroundManager를 싱글턴으로 만들어서 마우스 다운되면.. 이동하게!
        //clickDistance보다 작으면 GroundManager의 OnTouch함수를 실행하자
        GroundManager.Instance.OnTouch(transform.position);
    }
}
