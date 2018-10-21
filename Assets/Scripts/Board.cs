using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
  /// <summary>
  /// The direction enum exists to provide geometric structure and ordering to the tiles on a board.
  /// </summary>
  public enum Direction
  {
    Vertical,
    Horizaontal,
    Diagonal_TopDown,
    Diagonal_BottomUp
  }

  public int rows { get { return tiles.GetLength(0); } }
  public int cols { get { return tiles.GetLength(1); } }
  private Tile[,] tiles;
  public Tile this[int row, int col] { get { return tiles[row, col]; } }

  // Start is called before the first frame update
  void Start()
  {
    
  }

  public void ScanForTiles()
  {
    int maxRow = 0;
    int maxCol = 0;
    foreach (Tile t in GetComponentsInChildren<Tile>())
    {
      if (t.row > maxRow) { maxRow = t.row; }
      if (t.column > maxCol) { maxCol = t.column; }
    }

    tiles = new Tile[maxRow + 1, maxCol + 1]; 
    foreach (Tile t in GetComponentsInChildren<Tile>())
    {
      tiles[t.row, t.column] = t;
    }
  }

  /// <summary>
  /// (Unoptimized) check for a win condition for the given Side <paramref name="s"/>. 
  /// Handles the generic match-N where N is the number of consectuive tiles occupied by a piece belong to the given Side.
  /// </summary>
  /// <param name="s"></param>
  /// <returns></returns>
  public bool CheckConsecutiveTiles(Side s, int consecutiveTiles)
  {
    foreach ( Tile t in tiles )
    {
      if (CheckDirection(t, Direction.Diagonal_BottomUp, s, consecutiveTiles) ||
        CheckDirection(t, Direction.Diagonal_TopDown, s, consecutiveTiles) ||
        CheckDirection(t, Direction.Horizaontal, s, consecutiveTiles) ||
        CheckDirection(t, Direction.Vertical, s, consecutiveTiles) )
      {
        return true;
      }
    }
    return false;
  }

  private bool CheckDirection(Tile t, Direction d, Side s, int numIterations)
  {
    if (t != null && t.piece != null && t.piece.side == s)
    {
      if (numIterations == 0)
      {
        return true;
      }

      return CheckDirection(Neighbor(t, d), d, s, numIterations - 1);
    }
    return false;
  }

  public Tile Neighbor(Tile t, Direction d)
  {
    switch (d)
    {
      case Direction.Vertical:
        return tiles[t.row + 1, t.column];
      case Direction.Horizaontal:
        return tiles[t.row, t.column + 1];
      case Direction.Diagonal_TopDown:
        return tiles[t.row + 1, t.column + 1];
      case Direction.Diagonal_BottomUp:
        return tiles[t.row - 1, t.column + 1];
    }
    return null;
  }

  // Update is called once per frame
  void Update()
  {

  }
}
