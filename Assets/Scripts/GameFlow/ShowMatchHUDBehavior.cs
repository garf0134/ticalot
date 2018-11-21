using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for making the Match's HUD visible again
/// </summary>
public class ShowMatchHUDBehavior : StateMachineBehaviour
{
  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Moves the GameFlow forward by starting the Match HUD's show animation
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.hud.matchHUD.gameObject.SetActive(true);
    gameFlow.hud.matchHUD.Show();
  }
}
