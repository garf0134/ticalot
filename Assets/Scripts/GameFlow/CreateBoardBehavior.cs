using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateBoardBehavior : StateMachineBehaviour
{
  /// <summary>
  /// The callback for a StateMachineBehaviour's start event
  /// Instantiates a game object with a Board from a prefab template.
  /// Translates the board so that it drops from a short height.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();

    GameObject boardInstance = Instantiate<GameObject>(Resources.Load<GameObject>(gameFlow.match.ruleset.boardResource));
    Board b = boardInstance.GetComponent<Board>();
    gameFlow.match.board = b;

    b.transform.SetParent(gameFlow.match.transform);
    b.transform.localPosition = Vector3.zero + Vector3.up * 2.0f;

    animator.SetBool("Board Visible", true);
  }
}
