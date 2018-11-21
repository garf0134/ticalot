using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updates the game counter of the match HUD
/// </summary>
public class UpdateMatchHUDBehavior : StateMachineBehaviour
{
  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Updates the game score for the current match
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.hud.matchHUD.UpdateGameScore(gameFlow.match);
  }
}
