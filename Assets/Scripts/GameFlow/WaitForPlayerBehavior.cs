using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Waits for the current player to register a move
/// </summary>
public class WaitForPlayerBehavior : StateMachineBehaviour
{
  /// <summary> A reference to the GameFlow's animator </summary>
  Animator animator;

  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Sets up listeners for <see cref="Match.OnTurnEnded"/>
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    this.animator = animator;

    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    //Debug.LogFormat("Adding WaitForPlayerBehavior.OnTurnEnded as a listener to the current match.");
    gameFlow.match.OnTurnEnded += OnTurnEnded;
  }
  
  /// <summary>
  /// Trigger the "Turn Played" trigger when a turn has ended
  /// </summary>
  /// <param name="m">The current match</param>
  /// <param name="turn">The turn that just ended</param>
  /// <param name="sides">The sides playing in the current match</param>
  private void OnTurnEnded(Match m, int turn, Side[] sides)
  {
    //Debug.LogFormat("Captured turn ended event for {0}'s turn of game {1}", sides[turn].name, m.game);
    animator.SetTrigger("Turn Played");
  }

  /// <summary>
  /// The callback for a StateMachineBehaviour's exit event
  /// Sets up the GameFlow's event-based notifications from the current match.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateExit(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    //Debug.LogFormat("Removing WaitForPlayerBehavior.OnTurnEnded as a listener to the current match.");

    gameFlow.match.OnTurnEnded -= OnTurnEnded;
  }
}
