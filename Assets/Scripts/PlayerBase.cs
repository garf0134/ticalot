using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a player. Derived concrete classes are responsible for implementing
/// the <see cref="Play(Board, Ruleset)"/> and <see cref="OnTurnBegan(Match, int, Side[])"/>
/// methods. This base class will handle some of the shared tasks such as creating 
/// the game pieces and setting the match.
/// </summary>
public abstract class PlayerBase : MonoBehaviour
{
  /// <summary>
  /// Used primarily to get access to the <see cref="HUD.UpdateHover(Tile)"/>
  /// method used by the derived concrete classes.
  /// </summary>
  public HUD hud;

  /// <summary>
  /// The match that the player is playing in currently
  /// <value>The value of the new match</value>
  /// </summary>
  public Match match
  {
    get { return _match; }
    set
    {
      SetMatch(value);
    }
  }

  /// <summary>
  /// The side that the player plays for
  /// </summary>
  public Side side;

  /// <summary>
  /// The GameObject that represents this player's game piece.
  /// </summary>
  public GameObject piecePrefab;

  /// <summary>
  /// The match that the player is playing in
  /// </summary>
  private Match _match;

  /// <summary>
  /// Sets the internal reference to the current match that the player is playing in
  /// as well as adding our event handlers to a subset of the match's events.
  /// </summary>
  /// <param name="m">The match that the player is playing in or null</param>
  protected virtual void SetMatch(Match m)
  {
    _match = m;
    if (_match != null)
    {
      _match.OnTurnBegan += OnTurnBegan;
    }
  }

  /// <summary>
  /// A Player should implement this method with the expectation that it must
  /// register a move with the passed in board. The Ruleset is passed in to provide
  /// the parameters (like consecutive matched tiles) that the AI should adhere to.
  /// </summary>
  /// <param name="b">The board to register a move with</param>
  /// <param name="r">The ruleset the current game is using</param>
  /// <returns></returns>
  public abstract IEnumerator Play(Board b, Ruleset r);
  
  /// <summary>
  /// A handler for the match's OnTurnBegan event. Derived classes must implement
  /// this method to begin the turn via <see cref="Play(Board, Ruleset)"/> but they
  /// may also use the method to take care of setup that happens at the start of 
  /// a turn.
  /// </summary>
  /// <param name="m">The match that the player is playing in</param>
  /// <param name="turn">The current turn (a player's turn counts as a turn)</param>
  /// <param name="sides">The list of sides/teams. <code>sides[turn]</code> would be 
  /// the current side.</param>
  protected abstract void OnTurnBegan(Match m, int turn, Side[] sides);

  /// <summary>
  /// Creates a game piece, storing information about which side its on and
  /// setting the instance's mesh renderer material's color to the team color.
  /// </summary>
  /// <returns>A new Piece component that belongs to a new GameObject</returns>
  protected Piece CreatePiece()
  {
    GameObject pieceInstance = Instantiate<GameObject>(piecePrefab);
    Piece piece = pieceInstance.GetComponent<Piece>();
    piece.side = side;
    piece.GetComponentInChildren<MeshRenderer>().material.color = side.color;
    return piece;
  }
}