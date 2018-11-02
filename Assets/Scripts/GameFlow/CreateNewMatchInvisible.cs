using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oversees the appearance of the NewMatchDialog from within the GameFlow
/// </summary>
public class CreateNewMatchInvisible : StateMachineBehaviour
{
  /// <summary>
  /// The animator associated with this StateMachineBehaviour
  /// </summary>
  Animator animator;

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
    
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.hud.newMatch.OnShowFinished += OnShowFinished;
    gameFlow.hud.newMatch.Show();

    this.animator = animator;
  }
  
  /// <summary>
  /// A hook for the UnityAnimation event
  /// Moves the GameFlow forward when the <see cref="NewMatchDialog"/> finishes 
  /// its show animation.
  /// </summary>
  private void OnShowFinished()
  {
    animator.SetBool("New Match Dialog Visible", true);
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
    gameFlow.hud.newMatch.OnShowFinished -= OnShowFinished;
  }
}
