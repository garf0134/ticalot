using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerBase : MonoBehaviour
{
  public HUD hud;
  public Match match
  {
    get { return _match; }
    set
    {
      SetMatch(value);
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

  protected virtual void SetMatch(Match m)
  {
    _match = m;
    if (_match != null)
    {
      _match.OnTurnBegan += OnTurnBegan;
    }
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