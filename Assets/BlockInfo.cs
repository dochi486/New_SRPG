using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


[Flags] //중복체크가 가능한 flag로 인스펙터에서 보임

public enum BlockType
{
    None = 0,
    Walkable = 1 << 0,
    Water = 1 << 1,
    Player = 1 << 2,
    Monster = 1 << 3,
}

public class BlockInfo : MonoBehaviour
{

    Renderer m_Renderer;
    private Color m_MouseOverColor = Color.red;
    private Color m_OriginalColor;

    private void Awake()
    {
        m_Renderer = GetComponentInChildren<Renderer>(); //블록의 렌더러를 가져온다. 메테리얼은 바로 접근이 불가해서!
        m_OriginalColor = m_Renderer.material.color; //메테리얼의 원래 색을 저장
    }
    private void OnMouseOver() //마우스가 블럭에 들어오면 실행
    {
        //ChangeColorToRed(); //렌더러가 가지고 있는 메테리얼의 색이 바뀐다
        if (character)
        {
            CharacterStateUI.Instance.Show(character);
        }
    }

    public void ChangeColorToRed()
    {
        m_Renderer.material.color = m_MouseOverColor;
    }

    public void ChangeToOriginalColor()
    {
        m_Renderer.material.color = m_OriginalColor;
    }

    private void OnMouseExit() //마우스가 블록을 빠져나가면 실행되는 부분
    {
        m_Renderer.material.color = m_OriginalColor;
        if (character)
        {
            CharacterStateUI.Instance.Close();
        }
    }

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

        switch (StageManager.GameState)
        {

            case GameStateType.SelectPlayer:
                SelectPlayer();
                break;
            case GameStateType.SelectMoveBlockOrAttackTarget:
                SelectMoveBlockOrAttackTarget();
                break;
            case GameStateType.SelectAttackTarget:
                SelectAttackTarget();
                break;
            case GameStateType.AttackToTarget:
                AttackToTarget();
                break;
            case GameStateType.NotInit:
            case GameStateType.PlayerMoving:
            case GameStateType.MonsterTurn:
                Debug.Log($"블럭을 클릭할 수 없는 상태입니다:" + $" {StageManager.GameState}");
                break;
        }

        //이미 빨간 블럭 상태일 때 다시 선택하면 빨간 블럭의 색을 메테리얼의 원래 색으로 돌리기

        //현재 있는 블럭에 몬스터가 있다면 공격하기


        //여기는 왜 안 쓰는지 영상 보면서 확인
        //if (character && character == Player.SelectedPlayer) //선택한 블록에 character정보가 있고 플레이어가 선택한 플레이어라면
        //{
        //    //    //선택된 플레이어가 캐릭터 스크립트를 상속 받았을 때 이동 가능한 영역을 표시
        //    //    //character.moveDistance
        //    ShowMoveableDistance(character.moveDistance);
        //}
        ////GroundManager를 싱글턴으로 만들어서 마우스 다운되면.. 이동하게!
        ////clickDistance보다 작으면 GroundManager의 OnTouch함수를 실행하자
        //Player.SelectedPlayer.OnTouch(transform.position);
    }

    private void AttackToTarget()
    {
        throw new NotImplementedException();
    }

    private void SelectAttackTarget()
    {
        throw new NotImplementedException();
    }

    private void SelectMoveBlockOrAttackTarget()
    {
        if (character) //공격 대상이 있다면 공격한다. 
        {

        }
        else
        {

        }
    }

    /// <summary>
    /// 지정한 캐릭터가 몬스터가 아니라 플레이어라면 
    /// 
    /// </summary>
    private void SelectPlayer()
    {
        if (character == null)
            return;
        if (character.GetType() == typeof(Player)) //character의 타입이 플레이어가 맞는지 확인
        {
            Player.SelectedPlayer = character as Player; //맞다면 선택된 플레이어로 지정한다 형변환하는 새로운 방법!
            //Player.SelectedPlayer = (Player)character;

            //이동 가능한 영역을 표시한다
            ShowMoveableDistance(Player.SelectedPlayer.moveDistance);

            //현재 위치에서 공격이 가능한 영역을 표시한다
            //Player.SelectedPlayer.ShowAttackArea(); //플레이어가 공격 가능한 영역을 보여주는 함수를 만들자
            StageManager.GameState = GameStateType.SelectMoveBlockOrAttackTarget;
        }
    }

    public LayerMask layerMask;
    private void ShowMoveableDistance(int moveDistance)
    {
        //Vector2Int currentPos = transform.position.ToVector2Int();
        Quaternion rotate = Quaternion.Euler(0, 45, 0); //여긴 왜 회전시키는 거지?
        Vector3 halfExtents = (moveDistance / Mathf.Sqrt(2)) * 0.99f * Vector3.one;

        var blocks = Physics.OverlapBox(transform.position, halfExtents, rotate);
        //블록 위치에서 플레이어가 이동 가능한 영역의 충돌체를 가져온다?
        foreach (var item in blocks)
        {
            if (Player.SelectedPlayer.OnMoveable(item.transform.position, moveDistance))
            {
                var block = item.GetComponent<BlockInfo>();
                if (block)
                {
                    block.ChangeColorToRed();
                    highLightedMoveableArea.Add(block);
                }
            }
        }
        //Vector2Int currenPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        //List<List<Vector2Int>> lines = new List<List<Vector2Int>>(); //갈 수 있는 길을 이은 리스트를 또 한 번 더 리스트에 담아준다
        //for (int i = 0; i < moveDistance; i++)
        //{
        //    List<Vector2Int> line = new List<Vector2Int>(); //이동할 수 있는 칸을 선으로 이은 것을 리스트에 담기 위한 것

    }

    static List<BlockInfo> highLightedMoveableArea = new List<BlockInfo>();

    private void ClearMoveableArea()
    {
        highLightedMoveableArea.ForEach(x => x.ChangeToOriginalColor());
        highLightedMoveableArea.Clear();
    }

    string debugTextPrefab = "DebugTextPrefab"; // 리소스에서 생성할 DebugTextPrefab의 이름 저장
    GameObject debugTextGos; // DebugTextPrefab를 생성해서 게임오브젝트로 저장할 변수
    internal Character character;

    internal void UpdateDebugInfo()
    {
        // 생성된 DebugTextPrefab 오브젝트가 없다면
        if (debugTextGos == null)
        {
            GameObject textMeshGo = Instantiate((GameObject)Resources.Load(debugTextPrefab), transform); //생성
            debugTextGos = textMeshGo;
            textMeshGo.transform.localPosition = Vector3.zero;
            //로컬 포지션이 0이어야 블록에 딱 붙어서 텍스트가 생성된다
        }
        // 블록의 정보를 저장하자
        var intPos = transform.position.ToVector2Int();
        name = $"{intPos.x}:{intPos.y}"; //타일 블록 게임오브젝트 하나하나의 이름을 좌표로 변경한다

        StringBuilder debugText = new StringBuilder();

        ContainingText(debugText, BlockType.Water);
        ContainingText(debugText, BlockType.Player);
        ContainingText(debugText, BlockType.Monster);

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
