using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


[Flags] //중복체크가 가능한 flag로 인스펙터에서 보임

public enum BlockType
{
    None           =0,
    Walkable     = 1<<0,
    Water          = 1 <<1,
    Player         = 1 << 2,
    Monster       = 1<< 3,
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
            //Debug.Log($"downMousePosition : {downMousePosition}" + $"upMousePosition : {upMousePosition}");
            return;
        }
        //GroundManager를 싱글턴으로 만들어서 마우스 다운되면.. 이동하게!
        //clickDistance보다 작으면 GroundManager의 OnTouch함수를 실행하자
        Player.SelectedPlayer.OnTouch(transform.position);
    }

    string debugTextPrefab = "DebugTextPrefab"; // 리소스에서 생성할 DebugTextPrefab의 이름 저장
    GameObject debugTextGos; // DebugTextPrefab를 생성해서 게임오브젝트로 저장할 변수
    internal void UpdateDebugInfo()
    {
        // 생성된 DebugTextPrefab 오브젝트가 없다면
        if (debugTextGos == null)
        {
            GameObject textMeshGo = Instantiate((GameObject)Resources.Load(debugTextPrefab), transform); //생성
            debugTextGos = textMeshGo;
            textMeshGo.transform.localPosition = Vector3.zero;
        }
        // 블록의 정보를 저장하자
        StringBuilder debugText = new StringBuilder();

        ContainingText(debugText, BlockType.Water);
        ContainingText(debugText, BlockType.Player);
        ContainingText(debugText,  BlockType.Monster);

        // block 오브젝트의 자식 중 text 컴포넌트를 찾아 debugText의 정보를 String형으로 반환 시켜 넣어 준다.(자신의 타입이 들어 갈 것임)
        GetComponentInChildren<TextMesh>().text = debugText.ToString();
    }
    private void ContainingText(StringBuilder sb, BlockType walkable)
    {
        if (blockType.HasFlag(walkable)) //만약 block의 BlockType과 walkable이 같은 타입이라면
        {
            sb.AppendLine(walkable.ToString()); // debugText에 값을 넣어준다.
        }
    }
}
