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
        downMousePosition = Input.mousePosition;  
    }

    private void OnMouseUp()
    {
        var upMousePosition = Input.mousePosition;
        if (Vector3.Distance(downMousePosition, upMousePosition) > clickDistance)
        {
            Debug.Log($"downMousePosition : {downMousePosition}" + $"upMousePosition : {upMousePosition}");
            return;
        }
        //GroundManager를 싱글턴으로 만들어서 마우스 다운되면.. 이동하게!
        GroundManager.Instance.OnTouch(transform.position);
    }
}
