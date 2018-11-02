using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For any future code that needs to do something when the turn is ended
/// </summary>
public class EndTurnBehavior : StateMachineBehaviour
{
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
  }
}
