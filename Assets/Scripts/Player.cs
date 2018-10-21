using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBase: MonoBehaviour
{
  public HUD hud;
  public Match match {
    get { return _match; }
    set {
      _match = value;
      if (_match != null)
      {
        _match.OnTurnBegan += OnTurnBegan;
      }
    }
  }

  public Side side;
  public GameObject piecePrefab;

  private Match _match;

  // Start is called before the first frame update
  protected virtual void Start()
  {

  }

  // Update is called once per frame
  protected virtual void Update()
  {

  }

  public abstract IEnumerator Play(Board b, Ruleset r);
  protected abstract void OnTurnBegan(Match m, int turn, Side[] sides);
  protected Piece CreatePiece()
  {
    GameObject pieceInstance = Instantiate<GameObject>(piecePrefab);
    Piece piece = pieceInstance.GetComponent<Piece>();
    piece.side = side;
    piece.GetComponentInChildren<MeshRenderer>().material.color = side.color;
    return piece;
  }
}

public class HumanPlayer : PlayerBase
{
  // Start is called before the first frame update
  protected override void Start()
  {

  }

  // Update is called once per frame
  protected override void Update()
  {

  }

  protected override void OnTurnBegan(Match m, int turn, Side[] sides)
  {
    if (sides[turn] == side )
    {
      StartCoroutine(Play(m.board, m.ruleset));
    }
  }

  public override IEnumerator Play(Board b, Ruleset r)
  {
    while (true)
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit))
      {
        Tile t = hit.collider.GetComponentInParent<Tile>();
        if (t != null)
        {
          hud.UpdateHover(t);

          if (Input.GetMouseButtonDown(0))
          {
            Piece piece = CreatePiece();

            if (match.RegisterMove(side, t, piece))
            {
              piece.transform.SetParent(t.transform);
              piece.transform.localPosition = Vector3.zero;

              yield break;
            }
            else
            {
              DestroyImmediate(piece.gameObject);
            }

            
          }
        }
      }
      else
      {
        hud.UpdateHover(null);
      }

      yield return null;
    }
  }
}

public class AIPlayer : PlayerBase
{
  public enum Strategy
  {
    PureRandom
  }
  public Strategy strategy;

  public override IEnumerator Play(Board b, Ruleset ruleset)
  {
    // Add human-like delay
    yield return new WaitForSecondsRealtime(Random.Range(0.5f, 1.2f));

    // Decide on move
    List<System.ValueTuple<int, int>> validMoves = new List<System.ValueTuple<int, int>>();
    for (int r = 0; r < b.rows; r++)
    {
      for (int c = 0; c < b.cols; c++)
      {
        if (ruleset.ValidateMove(b, b[r,c], side) )
        {
          var tuple = (r, c);
          validMoves.Add(tuple);
        }
      }
    }

    Tile chosenMove = null;
    switch (strategy)
    {
      case Strategy.PureRandom:
        var randomeTileCoords = validMoves[Random.Range(0, validMoves.Count)];
        chosenMove = b[randomeTileCoords.Item1, randomeTileCoords.Item2];
        break;
    }

    if (chosenMove != null)
    {
      Piece piece = CreatePiece();

      if (match.RegisterMove(side, chosenMove, piece))
      {
        piece.transform.SetParent(chosenMove.transform);
        piece.transform.localPosition = Vector3.zero;
      }
      else
      {
        DestroyImmediate(piece.gameObject);
      }
    }
  }

  protected override void OnTurnBegan(Match m, int turn, Side[] sides)
  {
    if (sides[turn] == side)
    {
      StartCoroutine(Play(m.board, m.ruleset));
    }
  }

  // Start is called before the first frame update
  protected override void Start()
  {

  }

  // Update is called once per frame
  protected override void Update()
  {

  }

}

