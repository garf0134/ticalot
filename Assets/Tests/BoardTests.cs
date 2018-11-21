using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class BoardTests
  {
    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator Setup()
    {
      // Use the Assert class to test conditions.
      // Use yield to skip a frame.
      yield return new MonoBehaviourTest<BoardTestable>();
      BoardTestable boardTestable = BoardTestable.instance;
      Assert.That(boardTestable.RowsObserved == 3, "Number of rows");
      Assert.That(boardTestable.ColumnsObserved == 3, "Number of columns");

      for (int row = 0; row < 3; row++)
      {
        for (int col = 0; col < 3; col++)
        {
          var tile = boardTestable[row, col];
          Assert.That(tile != null, "Null status of index ({0}, {1})", row, col);
          Assert.That(tile != null && tile.row == row, "Row equivalence at index ({0},{1})", row, col);
          Assert.That(tile != null && tile.column == col, "Column equivalence at index({0},{1})", row, col);
        }
      }
    }

    [UnityTest]
    public IEnumerator Neighbors()
    {
      yield return new MonoBehaviourTest<BoardTestable>();
      BoardTestable boardTestable = BoardTestable.instance;

      var upperLeftCorner = boardTestable[0, 0];
      var upperLeftCornerHorizontalNeighbor = boardTestable.Neighbor(upperLeftCorner, Board.Direction.Horizontal);
      var upperLeftCornerVerticalNeighbor = boardTestable.Neighbor(upperLeftCorner, Board.Direction.Vertical);
      var upperLeftCornerTopDownNeighbor = boardTestable.Neighbor(upperLeftCorner, Board.Direction.Diagonal_TopDown);
      var upperLeftCornerBottomUpNeighbor = boardTestable.Neighbor(upperLeftCorner, Board.Direction.Diagonal_BottomUp);

      Assert.IsNotNull(upperLeftCorner, "Null status of tile in upper left corner");
      Assert.IsNull(upperLeftCornerBottomUpNeighbor, "Null status of bottom up neighbor of upper left corner");
      Assert.IsNotNull(upperLeftCornerTopDownNeighbor, "Null status of top down neighbor of upper left corner");
      Assert.AreEqual(upperLeftCornerTopDownNeighbor.row, 1, "Row index of upper left corner's diagonal top-down neighbor");
      Assert.AreEqual(upperLeftCornerTopDownNeighbor.column, 1, "Column index of upper left corner's diagonal top-down neighbor");
      Assert.IsNotNull(upperLeftCornerHorizontalNeighbor, "Null status of horizontal neighbor of upper left corner");
      Assert.AreEqual(upperLeftCornerHorizontalNeighbor.row, 0, "Row index of upper left corner's horizontal neighbor");
      Assert.AreEqual(upperLeftCornerHorizontalNeighbor.column, 1, "Column index of upper left corner's horizontal neighbor");
      Assert.IsNotNull(upperLeftCornerVerticalNeighbor, "Null status of vertical neighbor of upper left corner");
      Assert.AreEqual(upperLeftCornerVerticalNeighbor.row, 1, "Row index of upper left corner's vertical neighbor");
      Assert.AreEqual(upperLeftCornerVerticalNeighbor.column, 0, "Column index of upper left corner's vertical neighbor");

      var upperRightCorner = boardTestable[0, 2];
      var upperRightCornerHorizontalNeighbor = boardTestable.Neighbor(upperRightCorner, Board.Direction.Horizontal);
      var upperRightCornerVerticalNeighbor = boardTestable.Neighbor(upperRightCorner, Board.Direction.Vertical);
      var upperRightCornerTopDownNeighbor = boardTestable.Neighbor(upperRightCorner, Board.Direction.Diagonal_TopDown);
      var upperRightCornerBottomUpNeighbor = boardTestable.Neighbor(upperRightCorner, Board.Direction.Diagonal_BottomUp);

      Assert.IsNotNull(upperRightCorner, "Null status of tile in upper right corner");
      Assert.AreEqual(upperRightCorner.row, 0, "Row index of upper right corner");
      Assert.AreEqual(upperRightCorner.column, 2, "Column index of upper right corner");
      Assert.IsNull(upperRightCornerBottomUpNeighbor, "Null status of bottom up neighbor of upper right corner");
      Assert.IsNull(upperRightCornerTopDownNeighbor, "Null status of top down neighbor of upper right corner");
      Assert.IsNull(upperRightCornerHorizontalNeighbor, "Null status of horizontal neighbor of upper right corner");
      Assert.IsNotNull(upperRightCornerVerticalNeighbor, "Null status of vertical neighbor of upper right corner");
      Assert.AreEqual(upperRightCornerVerticalNeighbor.row, 1, "Row index of upper right corner's vertical neighbor");
      Assert.AreEqual(upperRightCornerVerticalNeighbor.column, 2, "Column index of upper right corner's vertical neighbor");

      var bottomRightCorner = boardTestable[2, 2];
      var bottomRightCornerHorizontalNeighbor = boardTestable.Neighbor(bottomRightCorner, Board.Direction.Horizontal);
      var bottomRightCornerVerticalNeighbor = boardTestable.Neighbor(bottomRightCorner, Board.Direction.Vertical);
      var bottomRightCornerTopDownNeighbor = boardTestable.Neighbor(bottomRightCorner, Board.Direction.Diagonal_TopDown);
      var bottomRightCornerBottomUpNeighbor = boardTestable.Neighbor(bottomRightCorner, Board.Direction.Diagonal_BottomUp);

      Assert.IsNotNull(bottomRightCorner, "Null status of tile in bottom right corner");
      Assert.AreEqual(bottomRightCorner.row, 2, "Row index of upper right corner");
      Assert.AreEqual(bottomRightCorner.column, 2, "Column index of upper right corner");
      Assert.IsNull(bottomRightCornerHorizontalNeighbor, "Null status of horizontal neighbor of bottom right corner");
      Assert.IsNull(bottomRightCornerVerticalNeighbor, "Null status of vertical neighbor of bottom right corner");
      Assert.IsNull(bottomRightCornerTopDownNeighbor, "Null status of top down diagonal neighbor of bottom right corner");
      Assert.IsNull(bottomRightCornerBottomUpNeighbor, "Null status of bottom up diagonal neighbor of bottom right corner");

      var bottomLeftCorner = boardTestable[2, 0];
      var bottomLeftCornerHorizontalNeighbor = boardTestable.Neighbor(bottomLeftCorner, Board.Direction.Horizontal);
      var bottomLeftCornerVerticalNeighbor = boardTestable.Neighbor(bottomLeftCorner, Board.Direction.Vertical);
      var bottomLeftCornerTopDownNeighbor = boardTestable.Neighbor(bottomLeftCorner, Board.Direction.Diagonal_TopDown);
      var bottomLeftCornerBottomUpNeighbor = boardTestable.Neighbor(bottomLeftCorner, Board.Direction.Diagonal_BottomUp);

      Assert.IsNotNull(bottomLeftCorner, "Null status of tile in bottom left corner");
      Assert.AreEqual(bottomLeftCorner.row, 2, "Row index of upper right corner");
      Assert.AreEqual(bottomLeftCorner.column, 0, "Column index of upper right corner");
      Assert.IsNotNull(bottomLeftCornerHorizontalNeighbor, "Null status of horizontal neighbor of bottom left corner");
      Assert.AreEqual(bottomLeftCornerHorizontalNeighbor.row, 2, "Row index of horizontal neighbor of bottom left corner");
      Assert.AreEqual(bottomLeftCornerHorizontalNeighbor.column, 1, "Column index of horizontal neighbor of bottom left corner");
      Assert.IsNull(bottomLeftCornerVerticalNeighbor, "Null status of vertical neighbor of bottom left corner");
      Assert.IsNull(bottomLeftCornerTopDownNeighbor, "Null status of top down diagonal neighbor of bottom left corner");
      Assert.IsNotNull(bottomLeftCornerBottomUpNeighbor, "Null status of bottom up diagonal neighbor of bottom left corner");
      Assert.AreEqual(bottomLeftCornerBottomUpNeighbor.row, 1, "Row index of bottom up diagonal neighbor of bottom left corner");
      Assert.AreEqual(bottomLeftCornerBottomUpNeighbor.column, 1, "Column index of bottom up diagonal neighbor of bottom left corner");

      var middle = boardTestable[1, 1];
      var middleHorizontalNeighbor = boardTestable.Neighbor(middle, Board.Direction.Horizontal);
      var middleVerticalNeighbor = boardTestable.Neighbor(middle, Board.Direction.Vertical);
      var middleTopDownNeighbor = boardTestable.Neighbor(middle, Board.Direction.Diagonal_TopDown);
      var middleBottomUpNeighbor = boardTestable.Neighbor(middle, Board.Direction.Diagonal_BottomUp);

      Assert.IsNotNull(middle, "Null status of tile in middle");
      Assert.AreEqual(middle.row, 1, "Row index of upper right corner");
      Assert.AreEqual(middle.column, 1, "Column index of upper right corner");
      Assert.IsNotNull(middleHorizontalNeighbor, "Null status of horizontal neighbor of middle");
      Assert.AreEqual(middleHorizontalNeighbor.row, 1, "Row index of horizontal neighbor of middle");
      Assert.AreEqual(middleHorizontalNeighbor.column, 2, "Column index of horizontal neighbor of middle");
      Assert.IsNotNull(middleVerticalNeighbor, "Null status of vertical neighbor of middle");
      Assert.AreEqual(middleVerticalNeighbor.row, 2, "Row index of vertical neighbor of middle");
      Assert.AreEqual(middleVerticalNeighbor.column, 1, "Column index of vertical neighbor of middle");
      Assert.IsNotNull(middleTopDownNeighbor, "Null status of top down diagonal neighbor of middle");
      Assert.AreEqual(middleTopDownNeighbor.row, 2, "Row index of top down diagonal neighbor of middle");
      Assert.AreEqual(middleTopDownNeighbor.column, 2, "Column index of top down diagonal neighbor of middle");
      Assert.IsNotNull(middleBottomUpNeighbor, "Null status of bottom up diagonal neighbor of middle");
      Assert.AreEqual(middleBottomUpNeighbor.row, 0, "Row index of bottom up diagonal neighbor of middle");
      Assert.AreEqual(middleBottomUpNeighbor.column, 2, "Column index of bottom up diagonal neighbor of middle");
    }

    [UnityTest]
    public IEnumerator CheckDirection()
    {
      var monobehaviourTest = new MonoBehaviourTest<BoardTestable>();
      monobehaviourTest.component.PlacePieces = true;
      yield return monobehaviourTest;

      BoardTestable board = monobehaviourTest.component;
      Assert.IsTrue(monobehaviourTest.component.CheckConsecutiveTiles(board.side, 3), "Consecutive Tiles Test");
    }
  }

  public class BoardTestable: Board, IMonoBehaviourTest
  {
    public static BoardTestable instance;
    public bool IsTestFinished { get; private set; }
    public int RowsObserved { get; private set; } = 0;
    public int ColumnsObserved { get; private set; } = 0;
    public bool PlacePieces { get; set; } = false;
    public Side side { get; private set; } = null;

    private void Start()
    {
      instance = this;      
      for (int r = 0; r < 3; r++)
      {
        for (int c = 0; c < 3; c++)
        {
          GameObject obj = new GameObject(string.Format("Tile {0},{1}", r, c));
          Tile t = obj.AddComponent<Tile>();
          t.row = r;
          t.column = c;
          obj.transform.SetParent(transform);
        }
      }
      ScanForTiles();

      RowsObserved = tiles.GetLength(0);
      ColumnsObserved = tiles.GetLength(1);

      IsTestFinished = true;
    }

    private void Update()
    {
      if (PlacePieces)
      {
        side = new Side();
        side.color = Color.red;
        side.role = Side.Role.Human;

        for (int r = 0; r < 3; r++)
        {
          for (int c = 0; c < 3; c++)
          {
            if (r == c)
            {
              Tile t = tiles[r, c];
              GameObject pieceObj = new GameObject(string.Format("Piece"));
              pieceObj.transform.SetParent(t.transform);
              Piece piece = pieceObj.AddComponent<Piece>();
              piece.side = side;
              t.piece = piece;
            }
          }
        }
        PlacePieces = false;
      }
    }
  }
}
