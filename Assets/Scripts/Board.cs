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
    Horizontal,
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
  /// (Half optimized) check for a win condition for the given Side <paramref name="s"/>. 
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
        CheckDirection(t, Direction.Horizontal, s, consecutiveTiles) ||
        CheckDirection(t, Direction.Vertical, s, consecutiveTiles) )
      {
        return true;
      }
    }
    return false;
  }

  public int SumDirection(Tile t, Direction d, Side s)
  {
    if ( t != null && (t.piece == null || t.piece.side == s) )
    {
      return 1 + SumDirection(Neighbor(t, d), d, s);
    }

    return 0;
  }

  private bool CheckDirection(Tile t, Direction d, Side s, int numIterations)
  {
    if (t != null && t.piece != null && t.piece.side == s)
    {
      if (numIterations == 1)
      {
        return true;
      }

      return CheckDirection(Neighbor(t, d), d, s, numIterations - 1);
    }
    return false;
  }

  public void Neighbors(Tile t, Direction d, ref List<Tile> forwards, ref List<Tile> backwards)
  {
    int row = t.row;
    int column = t.column;

    // Forward direction first
    switch (d)
    {
      case Direction.Vertical:
        for (int r = row, c = column; r >= 0 && c >= 0; r--) { backwards.Add(this[r, c]); }
        for (int r = row, c = column; r < rows && c < cols; r++) { forwards.Add(this[r, c]); }
        break;
      case Direction.Horizontal:
        for (int r = row, c = column; r >= 0 && c >= 0; c--) { backwards.Add(this[r, c]); }
        for (int r = row, c = column; r < rows && c < cols; c++) { forwards.Add(this[r, c]); }
        break;
      case Direction.Diagonal_TopDown:
        for (int r = row, c = column; r >= 0 && c >= 0; r--,c--) { backwards.Add(this[r, c]); }
        for (int r = row, c = column; r < rows && c < cols; r++,c++) { forwards.Add(this[r, c]); }
        break;
      case Direction.Diagonal_BottomUp:
        for (int r = row, c = column; r >= 0 && c >= 0; r++, c--) { backwards.Add(this[r, c]); }
        for (int r = row, c = column; r < rows && c < cols; r--, c++) { forwards.Add(this[r, c]); }
        break;
    }
  }

  public Tile Neighbor(Tile t, Direction d)
  {
    switch (d)
    {
      case Direction.Vertical:
        if ((t.row + 1) < rows) { return tiles[t.row + 1, t.column]; }
        break;
      case Direction.Horizontal:
        if ((t.column + 1) < cols) { return tiles[t.row, t.column + 1]; }
        break;
      case Direction.Diagonal_TopDown:
        if ((t.row + 1) < rows && (t.column + 1) < cols) { return tiles[t.row + 1, t.column + 1]; }
        break;
      case Direction.Diagonal_BottomUp:
        if ((t.row - 1) >= 0 && (t.column + 1) < cols) { return tiles[t.row - 1, t.column + 1]; }
        break;
    }
    return null;
  }

  // Update is called once per frame
  void Update()
  {

  }
}
