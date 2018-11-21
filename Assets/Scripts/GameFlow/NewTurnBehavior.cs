using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for beginning a new turn
/// </summary>
public class NewTurnBehavior : StateMachineBehaviour
{
  /// <summary>The GameFlow animator</summary>
  Animator animator;

  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Moves the GameFlow forward by calling <see cref="Match.BeginTurn"/>.
  /// Starts the show turn instruction animation
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();

    this.animator = animator;

    gameFlow.match.BeginTurn();
    gameFlow.hud.matchHUD.OnShowTurnInstructionsFinished += OnShowTurnInstructionsFinished;
    gameFlow.hud.matchHUD.ShowTurnInstructions();
  }

  /// <summary>
  /// A hook into the show turn instructions animation. Called at the end. 
  /// Triggered by <see cref="MatchHUD.ShowTurnInstructions"/>
  /// </summary>
  private void OnShowTurnInstructionsFinished()
  {
    animator.SetBool("Turn Instructions Visible", true);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.hud.matchHUD.OnShowTurnInstructionsFinished -= OnShowTurnInstructionsFinished;
  }
}
