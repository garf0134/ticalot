using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for beginning a new turn
/// </summary>
public class NewTurnBehavior : StateMachineBehaviour
{
  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Moves the GameFlow forward by calling <see cref="Match.BeginTurn"/>.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.match.BeginTurn();
  }
}
