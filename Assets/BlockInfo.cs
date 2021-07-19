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

    private void OnMouseDown()
    {
        //GroundManager를 싱글턴으로 만들어서 마우스 다운되면.. 이동하게!
        GroundManager.Instance.OnTouch(transform.position);
    }
}
