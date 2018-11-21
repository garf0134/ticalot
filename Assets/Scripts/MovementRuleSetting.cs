using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Associates the WinCondition toggle box with a <see cref="Ruleset.WinCondition"/> 
/// </summary>
public class MovementRuleSetting : MonoBehaviour
{
  /// <summary>
  /// The win condition associated with the UI object
  /// </summary>
  public Ruleset.ValidMove movement;
}
