using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Board Configuration")]
public class BoardConfigurationSet: ScriptableObject
{
  [System.Serializable]
  public class BoardConfiguration
  {
    public Ruleset.ValidMove[] allowedMoves;
    public string[] allowedBoards;
    public string[] allowedTiles;
    public enum TileOrientation
    {
      Flat,
      Vertical
    }
    public TileOrientation tileOrientation;
    public string name;
  }
  public BoardConfiguration[] configurations;
}
