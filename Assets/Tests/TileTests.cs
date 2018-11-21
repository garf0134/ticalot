using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class TileTests
  {
    private bool wasPieceAssignedCalled = false;

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator Setup()
    {
      var tileTest = new MonoBehaviourTest<TileTestable>();
      yield return tileTest;

      Tile tile = tileTest.component;
      Assert.AreEqual(tile.row, 1, "Row index of tile");
      Assert.AreEqual(tile.column, 1, "Column index of tile");
      Assert.AreEqual(tile.tileFamilyName, "Flat", "Family name of tile");
      Assert.AreEqual(tile.tileOrientation, Tile.TileOrientation.Horizontal);

      tile.OnPiecePlaced += OnPiecePlaced;

      GameObject pieceObject = new GameObject("Piece");
      Piece piece = pieceObject.AddComponent<Piece>();
      tile.piece = piece;
      Assert.AreEqual(tile.piece, piece);
      Assert.IsTrue(wasPieceAssignedCalled, "Piece assigned event callback, called");
    }

    void OnPiecePlaced(Tile tile, Piece piece)
    {
      wasPieceAssignedCalled = true;
    }
  }

  public class TileTestable: Tile, IMonoBehaviourTest
  {
    public bool IsTestFinished { get; private set; }
    private void Start()
    {
      tileFamilyName = "Flat";
      row = 1;
      column = 1;
      tileOrientation = TileOrientation.Horizontal;

      IsTestFinished = true;
    }
  }
}
