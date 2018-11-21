using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for triggering a new game to begin within a match
/// </summary>
public class NewGameBehavior : StateMachineBehaviour
{
  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Calls <see cref="Match.BeginGame"/> to begin a new game.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.match.BeginGame();
  }
}
