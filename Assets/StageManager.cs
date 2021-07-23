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
        GameState = GameStateType.SelectPlayer;
        CenterNotifyUI.Instance.Show("게임이 시작되었습니다.", 1.5f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            ContextMenuUI.Instance.Show(Input.mousePosition);
    }

}
