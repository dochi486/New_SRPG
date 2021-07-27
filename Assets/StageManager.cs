using System;
using System.Collections;
using UnityEngine;


public enum GameStateType
{
    NotInit, //게임이 시작되지 않았다(초기화 되지 않았음)
    SelectPlayer, //조정할 아군 플레이어를 선택
    SelectMoveBlockOrAttackTarget, //이동할 블럭 또는 공격할 타겟 선택
    PlayerMoving, //플레이어 이동하고 있는 상태
    SelectAttackTarget, //이동한 뒤에 공격할 타겟을 선택. 공격할 타겟이 없으면 SelectPlayer로 변경
    AttackToTarget, //공격하고 있는 중
    MonsterTurn,
}
public class StageManager : SingletonMonoBehavior<StageManager>
{
    [SerializeField] GameStateType gameState;

    static public GameStateType GameState
    {
        get => Instance.gameState;
        set
        {
            Debug.Log($"{Instance.gameState} => {value}");

            NotifyUI.Instance.Show(value.ToString(), 10);
            Instance.gameState = value;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        OnStartTurn();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            ContextMenuUI.Instance.Show(Input.mousePosition);
    }
    internal void EndTurnPlayer()
    {
        GameState = GameStateType.MonsterTurn;

        StartCoroutine(MonsterTurnCo()); //중첩코루틴!
    }
    IEnumerator MonsterTurnCo()
    {
        foreach (var monster in Monster.Monsters)
        {
            FollowTarget.Instance.SetTarget(monster.transform);
            yield return monster.AutoAttackCo();
        }
        ProcessNextTurn();
    }

    private void ProcessNextTurn()
    {
        ClearTurnInfo(); //턴 정보를 초기화해준다.
        turn++;
        OnStartTurn();
    }

    private void ClearTurnInfo() //플레이어와 몬스터의 턴이 끝나면 bool 조건 false로 바꿔서 초기화 하는 메서드
    {
        Player.Players.ForEach(x =>
        {
            x.completeAct = false;
            x.completeMove = false;
        });

        Monster.Monsters.ForEach(x =>
        {
            x.completeAct = false;
            x.completeMove = false;
        });

    }

    private void OnStartTurn()
    {
        FollowTarget.Instance.SetTarget(Player.Players[0].transform); //0번째 플레이어가 맨 처음 플레이어라서??
        ShowCurrentTurn();
        GameState = GameStateType.SelectPlayer;
        //CenterNotifyUI.Instance.Show("게임이 시작되었습니다.", 1.5f);
    }
    int turn = 1;
    private void ShowCurrentTurn()
    {
        CenterNotifyUI.Instance.Show($"{turn}턴이 시작되었습니다");
    }

}
