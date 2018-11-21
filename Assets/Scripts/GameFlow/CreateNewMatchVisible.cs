using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oversees the dismissal of the NewMatchDialog from within the GameFlow
/// </summary>
public class CreateNewMatchVisible : StateMachineBehaviour
{
  /// <summary>
  /// The animator associated with the GameFlow
  /// </summary>
  private Animator animator;

  /// <summary>
  /// The callback for a StateMachineBehaviour's start event
  /// Adds listeners for the events in <see cref="NewMatchDialog"/>. Advances
  /// the game flow when the NewMatchDialog has finished animating to a 
  /// visible state.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    this.animator = animator;

    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.hud.newMatch.OnHideFinished += OnHideFinished;
    gameFlow.hud.newMatch.OnNewMatch += OnNewMatch;
  }

  /// <summary>
  /// A listener for <see cref="NewMatchDialog.OnNewMatch"/>
  /// Assigns the GameFlow match to the new match
  /// </summary>
  /// <param name="m"></param>
  private void OnNewMatch(Match m)
  {
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.match = m;
  }

  /// <summary>
  /// A UnityAnimation event hook
  /// Called when the NewMatchDialog finishes hiding
  /// Used to move the GameFlow forward
  /// </summary>
  private void OnHideFinished()
  {
    animator.SetBool("New Match Dialog Visible", false);
  }

  /// <summary>
  /// The callback for a StateMachineBehaviour's exit event
  /// Removes listeners for the events in <see cref="NewMatchDialog"/>. 
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateExit(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.hud.newMatch.OnHideFinished -= OnHideFinished;
    gameFlow.hud.newMatch.OnNewMatch -= OnNewMatch;
    gameFlow.hud.newMatch.gameObject.SetActive(false);
  }
}
