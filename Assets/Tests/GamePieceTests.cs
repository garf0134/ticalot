using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class GamePieceTests
  {
    private bool onPieceFinishedPlacingCalled = false;

    [UnityTest]
    public IEnumerator Setup()
    {
      // Use the Assert class to test conditions.
      // Use yield to skip a frame.
      var monobehaviourTest = new MonoBehaviourTest<GamePieceTestable>();
      GamePieceTestable piece = monobehaviourTest.component;
      piece.OnPieceFinishedPlacing += OnPieceFinishedPlacing;
      yield return monobehaviourTest;

      Assert.AreEqual(piece.side.name, "Test Side", "Side name");
      Assert.AreEqual(piece.side.color, Color.red, "Side color");
      Assert.AreEqual(piece.side.role, Side.Role.Human, "Side role");
      Assert.IsNotNull(piece.GetComponent<Rigidbody>(), "Rigidbody null status");
      Assert.IsTrue(onPieceFinishedPlacingCalled, "Called OnPieceFinishedPlacing");

      Assert.IsTrue(onPieceFinishedPlacingCalled, "Piece finished placing");
    }

    private void OnPieceFinishedPlacing(Piece piece)
    {
      onPieceFinishedPlacingCalled = true;
    }
  }

  public class GamePieceTestable: Piece, IMonoBehaviourTest
  {
    public bool IsTestFinished { get; private set; }
    private void Start()
    {
      Side s = new Side();
      s.name = "Test Side";
      s.color = Color.red;
      s.aiStrategy = AIPlayer.Strategy.Normal;
      s.role = Side.Role.Human;
      side = s;
      orientation = Tile.TileOrientation.Horizontal;

      rigidbody = gameObject.AddComponent<Rigidbody>();

      FirePieceFinishedPlacing();
      IsTestFinished = true;
    }
  }
}
