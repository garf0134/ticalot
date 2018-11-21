using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For any future code that needs to do something when the turn is ended
/// </summary>
public class EndTurnBehavior : StateMachineBehaviour
{
  /// <summary>The GameFlow animator</summary>
  Animator animator;

  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Starts the hide turn instruction animation of the match HUD.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();

    this.animator = animator;

    gameFlow.hud.matchHUD.OnHideTurnInstructionsFinished += OnHideTurnInstructionsFinished;
    gameFlow.hud.matchHUD.HideTurnInstructions();
  }

  /// <summary>
  /// A hook into the hide turn instructions animation. Called at the end. 
  /// Triggered by <see cref="MatchHUD.HideTurnInstructions"/>
  /// </summary>
  private void OnHideTurnInstructionsFinished()
  {
    animator.SetBool("Turn Instructions Visible", false);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.hud.matchHUD.OnShowTurnInstructionsFinished -= OnHideTurnInstructionsFinished;
  }
}
