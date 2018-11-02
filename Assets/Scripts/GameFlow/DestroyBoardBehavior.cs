using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsibile for destroying the board left over from
/// a previous match
/// </summary>
public class DestroyBoardBehavior : StateMachineBehaviour
{
  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Destroys the board (by destroying the parent match). 
  /// Moves the Game Flow forward by setting "Board Visible" to false.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    GameObject.Destroy(gameFlow.match.gameObject);

    animator.SetBool("Board Visible", false);
  }
}
