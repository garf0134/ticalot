using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a side (aka "Team") which posseses a unique color, an icon 
/// string reference, a name and, if needed, an AI strategy if the role is 
/// for an AI.
/// </summary>
[System.Serializable]
public class Side
{
  /// <summary>
  /// The role for the side as a Human or an AI
  /// </summary>
  public enum Role
  {
    Human,
    AI
  }

  /// <summary>The role of the side</summary>
  public Role role;

  /// <summary>The color of the side</summary>
  public Color color;

  /// <summary>The string reference to an icon in the Resources folder for the side</summary>
  public string iconName;

  /// <summary>The name of the side</summary>
  public string name;

  /// <summary>The strategy that the AI will choose if the role of the side is AI</summary>
  public AIPlayer.Strategy aiStrategy;
}
