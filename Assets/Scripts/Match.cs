using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match : MonoBehaviour
{
  ///<summary>The current game of the match</summary>
  public int game = 0;
  ///<summary>The current turn of the game. The turn resets to 0 when the last side has played.</summary>
  public int turn = 0;

  ///<summary>The ruleset of the current game</summary>
  public Ruleset ruleset;
  ///<summary>The board used in the current game</summary>
  public Board board;
  ///<summary>The list of sides of the current game</summary>
  public List<Side> turnOrder = new List<Side>();
  ///<summary>The map of the current game</summary>
  public Dictionary<Side, int> games = new Dictionary<Side, int>();

  ///<summary>The delegate for game begin events</summary>
  public delegate void GameBeganEvent(Match m, int game, Board b, Side[] sides);
  ///<summary>The delegate for game end events</summary>
  public delegate void GameEndEvent(Match m, Board b, Side winner);
  ///<summary>The delegate for match began events</summary>
  public delegate void MatchBeganEvent(Match m, Side[] sides);
  ///<summary>The delegate for match end events</summary>
  public delegate void MatchEndEvent(Match m, Side winner);
  ///<summary>The delegate for any turn events</summary>
  public delegate void TurnEvent(Match m, int turn, Side[] sides);
  ///<summary>The delegate for evaluating for a valid move</summary>
  public delegate bool ValidMovePredicate(Board b, Tile t, Side s);

  ///<summary>This event notifies listeners when a turn begins</summary>
  public event TurnEvent OnTurnBegan;
  ///<summary>This event notifies listeners when a turn ends</summary>
  public event TurnEvent OnTurnEnded;

  ///<summary>This event notifies listeners when a game begins</summary>
  public event GameBeganEvent OnGameBegan;
  ///<summary>This event notifies listeners when a game ends</summary>
  public event GameEndEvent OnGameEnded;

  ///<summary>This event notifies listeners when a match begins</summary>
  public event MatchBeganEvent OnMatchBegan;
  ///<summary>This event notifies listeners when a match begins</summary>
  public event MatchEndEvent OnMatchEnded;

  ///<summary>
  ///This delegate notifies listeners when a move is attempted.
  ///</summary>
  public ValidMovePredicate OnMoveAttempted;

  /// <summary>
  /// Registers the side. The order that a side is registerd becomes the turn
  /// order.
  /// </summary>
  /// <param name="s">The side to register, appending to the turn order</param>
  public void RegisterSide(Side s)
  {
    turnOrder.Add(s);
    games[s] = 0;
  }
  
  /// <summary>
  /// Begins the turn and notifies listeners of the OnTurnBegan event
  /// </summary>
  public void BeginTurn()
  {
    OnTurnBegan?.Invoke(this, turn, turnOrder.ToArray());
  }

  /// <summary>
  /// Ends the turn, notifies the listeners of the OnTurnEnded event
  /// and advances the turn index
  /// </summary>
  public void EndTurn()
  {
    OnTurnEnded?.Invoke(this, turn, turnOrder.ToArray());
    turn = (turn + 1) % turnOrder.Count;
  }

  /// <summary>
  /// Begins a match, notifies listeners of the OnMatchBegan event and
  /// resets the game index to 0
  /// </summary>
  public void BeginMatch()
  {
    OnMatchBegan?.Invoke(this, turnOrder.ToArray());
    game = 0;
  }

  /// <summary>
  /// Ends a match, notifies listeners of the OnMatchEnded event
  /// </summary>
  /// <param name="winner"></param>
  public void EndMatch(Side winner)
  {
    OnMatchEnded?.Invoke(this, winner);
  }

  /// <summary>
  /// Begins a game, increments the game counter, resets the turn index
  /// and notifies listeners of the OnGameBegan event
  /// </summary>
  public void BeginGame()
  {
    game++;
    turn = 0;
    OnGameBegan?.Invoke(this, game, board, turnOrder.ToArray());
  }

  /// <summary>
  /// Ends a game, notifying the listeners of the OnGameEnded event
  /// </summary>
  /// <param name="winner">The side that won the game</param>
  public void EndGame(Side winner)
  {
    if ( winner != null)
    {
      games[winner]++;
    }
    
    
    OnGameEnded?.Invoke(this, board, winner);
  }

  /// <summary>
  /// Registers a move from a side, ending the current turn if the move
  /// is declared valid by the delegate assigned to <see cref="OnMoveAttempted"/>
  /// </summary>
  /// <param name="s">The side that is registering the move</param>
  /// <param name="t">The tile that the side is claiming</param>
  /// <param name="p">The piece that the side is using to claim the tile</param>
  /// <returns></returns>
  public bool RegisterMove(Side s, Tile t, Piece p)
  {
    if (OnMoveAttempted?.Invoke(board, t, s)  == false)
    {
      return false;
    }

    p.OnPieceFinishedPlacing += OnPieceFinishedPlacing;
    t.piece = p;

    return true;
  }

  /// <summary>
  /// A listener for the event fired by a game piece when it stops moving
  /// </summary>
  /// <param name="p">The piece that finished moving</param>
  private void OnPieceFinishedPlacing(Piece p)
  {
    Debug.LogFormat("Finished Placing Piece");
    EndTurn();
    p.OnPieceFinishedPlacing -= OnPieceFinishedPlacing;
  }

  /// <summary>
  /// Checks if there is a stalemate (no more moves left, all tiles have pieces)
  /// </summary>
  /// <returns>True if there is no more moves to be made. False, otherwise.</returns>
  public bool Stalemate()
  {
    bool unoccupiedTile = false;
    for (int r=0; r < board.rows; r++)
    {
      for (int c=0; c < board.cols; c++)
      {
        if (board[r,c].piece == null)
        {
          unoccupiedTile = true;
          break;
        }
      }
    }

    return !unoccupiedTile;
  }

  /// <summary>
  /// Checks to see if there is a match winner
  /// </summary>
  /// <returns>True if a side has gained the requisite number of games to win</returns>
  public bool HasWinner()
  {
    foreach (var pair in games)
    {
      if (pair.Value >= ruleset.gamesToWin)
      {
        return true;
      }
    }

    return false;
  }
}
