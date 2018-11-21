using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
  public class MatchTests
  {
    [UnityTest]
    public IEnumerator Setup()
    {
      yield return new MonoBehaviourTest<MatchTestable>();
    }
  }

  public class MatchTestable: Match, IMonoBehaviourTest
  {
    public bool IsTestFinished { get; private set; }
    private void Start()
    {
      GameObject boardObject = new GameObject("Board");
      boardObject.transform.SetParent(transform);
      BoardTestable board = boardObject.AddComponent<BoardTestable>();

      IsTestFinished = true;
    }
  }
}
