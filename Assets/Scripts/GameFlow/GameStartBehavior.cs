using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Starts a new Game within a Match
/// </summary>
public class GameStartBehavior : StateMachineBehaviour
{
  /// <summary>A reference to the GameFlow's animator</summary>
  private Animator animator;

  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// The entry point for the GameFlow. Begins the fade from black
  /// screen transition.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    this.animator = animator;

    GameFlow gameFlow = animator.GetComponent<GameFlow>();

    gameFlow.hud.OnFadeEnded += OnFadeEnded;
    gameFlow.hud.BeginFade();
  }

  /// <summary>
  /// A listener for <see cref="HUD.OnFadeEnded"/>
  /// Moves the GameFlow forward by setting the trigger "Fade From Black Complete" when the
  /// fade animation ends.
  /// </summary>
  private void OnFadeEnded()
  {
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    animator.SetTrigger("Fade From Black Complete");
  }
}
