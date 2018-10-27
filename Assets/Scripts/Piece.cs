using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A visual piece used on the game board. It is owned by a specific <see cref="Side"/>
/// </summary>
public class Piece : MonoBehaviour
{
  /// <summary>
  /// The side that the piece belongs to. Set at instantiation.
  /// </summary>
  /// <value>the side that the piece belongs to</value>
  public Side side { get; set; }
}
