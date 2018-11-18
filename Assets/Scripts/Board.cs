using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
  /// <summary>The player-facing name of the board</summary>
  public string boardName;

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

  /// <summary>
  /// The number of rows of tiles in the board. This value is only relevant after
  /// <see cref="ScanForTiles"/> is called.
  /// </summary>
  public int rows { get { return tiles.GetLength(0); } }

  /// <summary>
  /// The number of columns of tiles in the board. This value is only relevant after
  /// <see cref="ScanForTiles"/> is called.
  /// </summary>
  public int cols { get { return tiles.GetLength(1); } }

  /// <summary>
  /// The private backing of row-major ordering of the tiles belonging to the board
  /// </summary>
  protected Tile[,] tiles = new Tile[0, 0];

  /// <summary>
  /// Returns the tile represented by the row, column index. Only relevant after
  /// <see cref="ScanForTiles"/> is called.
  /// </summary>
  /// <param name="row">The row for the tile</param>
  /// <param name="col">The column for the tile</param>
  /// <returns>A <see cref="Tile"/> non-null instance</returns>
  public Tile this[int row, int col] { get { return tiles[row, col]; } }

  /// <summary>
  /// Scans the GameObject scene graph beneath the GameObject associated with
  /// this Board class instance. All Tile Components found in the first scan
  /// are evaluated for the maximum value of row and column. The 2-dimensional
  /// Tile array is allocated and a second scan of the scene graph associates
  /// each Tile commponent to the correct index in the 2-dimensional grid.
  /// </summary>
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
  /// (Half optimized) check for a win condition for the given Side 
  /// <paramref name="s"/>. Taking advantage of the symmetry of the win condition
  /// we only check four of the eight cardinal directions for the win condition. 
  /// We have to iterate in a consistent manner, always monotonically increasing
  /// the indices to avoid missing any areas of the grid 
  /// <see cref="Neighbor(Tile, Direction)"/>.
  /// Handles the generic match-N where N is the number of consectuive tiles occupied
  /// by a piece belong to the given Side.
  /// </summary>
  /// <param name="s">The side to evaluate the win condition for</param>
  /// <param name="consecutiveTiles">The number of consecutive tiles required by 
  /// the win condition</param>
  /// <returns>True if there is a sequence of tiles occupied by pieces belong to
  /// the given side that is <paramref name="consecutiveTiles"/> in length. False, 
  /// otherwise.</returns>
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


  /// <summary>
  /// Recursively checks a direction along a given tile. The terminating
  /// condition is when the number of iterations is 1. At that point we
  /// can return true to indicate that a match has been found. If there 
  /// is no tile, nor piece, nor matching side then false is returned. 
  /// Otherwise we find the neighboring piece in the given direction and
  /// recursively call CheckDirection with one fewer iteration to look for.
  /// </summary>
  /// <param name="t">The tile to be checked in the current recursion</param>
  /// <param name="d">The direction to check</param>
  /// <param name="s">The side to compare pieces encountered for</param>
  /// <param name="numIterations">The lenght of consecutive piecese belonging
  /// to a specific side to check for</param>
  /// <returns>True if the method found a sequence in the given direction,
  /// for the given side, for the given number of tiles.</returns>
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

  /// <summary>
  /// A lookup method to retrieve a tile that is contiguous to the given tile 
  /// in a specific direction. Since <see cref="Board.Direction"/> only looks
  /// forward, this method follows suit and only scans in a 'forward' direction
  /// as well.
  /// </summary>
  /// <param name="t">The tile used a frame of reference</param>
  /// <param name="d">The direction to look</param>
  /// <returns>Instance of Tile that is a neighbor in the direction requested
  /// or null if the Tile instance in the direction requested is outside 
  /// the board</returns>
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
}
