using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    static public Player SelectedPlayer;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        SelectedPlayer = this;
        animator = GetComponentInChildren<Animator>();
        GroundManager.Instance.AddBlockInfo(transform.position, BlockType.Player);
    }

    public void PlayAnimation(string nodName)
    {
        animator.Play(nodName, 0, 0);
    }
}
