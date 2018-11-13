using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A set of configurations for compatible boards, tiles and moves.
/// </summary>
[CreateAssetMenu(menuName ="Board Configuration")]
public class BoardConfigurationSet: ScriptableObject
{
  /// <summary>
  /// A configuration for compatible boards, tiles and moves.
  /// </summary>
  [System.Serializable]
  public class BoardConfiguration
  {
    /// <summary>Which moves are allowed in this configuration</summary>
    public Ruleset.ValidMove[] allowedMoves;
    /// <summary>Which boards are allowed in this configuration</summary>
    public string[] allowedBoards;
    /// <summary>Which tiles are allowed in this configuration</summary>
    public string[] allowedTiles;
    /// <summary>The tile orientation</summary>
    public Tile.TileOrientation tileOrientation;
    /// <summary>The name of the configuration</summary>
    public string name;
  }
  /// <summary>All the configurations</summary>
  public BoardConfiguration[] configurations;
}
