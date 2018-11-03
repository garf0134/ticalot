using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hides everything but the Game's Title UI
/// </summary>
public class TitleBehavior : StateMachineBehaviour
{
  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Clears GameFlow flags that were set in a previous match and activates the Title UI
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    Animator titleAnimator = gameFlow.hud.title.GetComponent<Animator>();
    titleAnimator.SetTrigger("Show");

    animator.SetBool("Match Won", false);
    animator.SetBool("Game Ended", false);
  }

  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Waits for the player to click anywhere on the screen.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateUpdate(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();

    if (Input.GetMouseButtonDown(0))
    {
      animator.SetTrigger("Click Anywhere");
    }
  }

  /// <summary>
  /// The callback for a StateMachineBehaviour's exit event
  /// Sets the hud's title object to inactive
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateExit(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    Animator titleAnimator = gameFlow.hud.title.GetComponent<Animator>();
    titleAnimator.SetTrigger("Hide");
  }
}
