using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Starts a new match given a previously initialized <see cref="Match"/> attached
/// to the <see cref="GameFlow"/>.
/// Sets up multiple listeners located within GameFlow to the match's various game/match events.
/// </summary>
public class NewMatchBehavior : StateMachineBehaviour
{
  /// <summary>
  /// The callback for a StateMachineBehaviour's enter event
  /// Sets up the GameFlow's event-based notifications from the current match.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.match.OnMatchBegan += gameFlow.OnMatchBegan;
    gameFlow.match.OnMatchEnded += gameFlow.OnMatchEnded;
    gameFlow.match.OnGameEnded += gameFlow.OnGameEnded;
    gameFlow.match.OnGameBegan += gameFlow.OnGameBegan;
    gameFlow.match.BeginMatch();
  }
}
