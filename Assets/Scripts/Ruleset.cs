using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An attribute for assigning a name and description
/// </summary>
public class NamedAttribute: System.Attribute
{
  public string name { get; set; }
  public string description { get; set; }
}

/// <summary>
/// A collection of parameters and choices for how a match should be played and won
/// </summary>
[System.Serializable]
public class Ruleset: ScriptableObject
{ 
  ///<summary>The match that is associated with this ruleset</summary>
  public Match m;
  ///<summary>The maximum number of games that can be played before a draw is called</summary>
  public int maxGames = 3;
  ///<summary>The side to reach gamesToWin number of games won first, wins the match</summary>
  public int gamesToWin = 2;
  ///<summary>
  ///If the win condition is set to Match-N, the number of consecutive tiles
  ///that need to be claimed to win a game
  ///</summary>
  public int consecutiveTiles = 3;
  ///<summary>The number of rows the board has</summary>
  public int rows = 3;
  ///<summary>The number of columns the board has</summary>
  public int cols = 3;

  /// <summary>A reference to the 3d object used for each tile</summary>
  public string tileResource;
  /// <summary>A reference to the 3d object used for a board</summary>
  public string boardResource;

  /// <summary>
  /// The way a game is won
  /// </summary>
  public enum WinCondition
  {
    ///<summary>Requires N consecutive tiles to win <see cref="consecutiveTiles"/></summary>
    [Named(name = "Match N", description ="Match {0} Consecutive Tiles")]
    MatchN
  }
  ///<summary>The WinCondition assigned to this ruleset</summary>
  public WinCondition winCondition;

  /// <summary>
  /// A valid move
  /// </summary>
  public enum ValidMove
  { 
    ///<summary>Anywhere there's an unclaimed tile</summary>
    [Named(name = "Anywhere!", description = "Place your pieces anywhere there is an unoccupied tile")]
    Anywhere, // i.e. Tic-Tac-Toe
    ///<summary>
    ///Anywhere there's an unclaimed tile and there is a piece 'beneath' the chosen 
    ///tile or the chosen tile is the 'bottom-most' row
    ///</summary>
    [Named(name ="Drop", description = "Place your pieces above an occupied tile")]
    OnTop // i.e. Connect-N
  }

  ///<summary>The valid move assigned to this ruleset</summary>
  public ValidMove validMove;

  ///<summary>
  ///Set the match associated with this ruleset, adding listeners to 
  ///OnTurnEnded, OnGameEnded and ValidateMove events/delegate.
  ///</summary>
  ///<param name="match">The match to associate this ruleset with</param>
  public void SetMatch(Match match)
  {
    m = match;
    m.OnTurnEnded += OnTurnEnded;
    m.OnGameEnded += OnGamEnded;
    m.OnMoveAttempted = ValidateMove;
  }

  /// <summary>
  /// Called when the match is trying to determine if a move registered
  /// by a side is valid or not.
  /// </summary>
  /// <param name="b">The board where the current game is being played on</param>
  /// <param name="t">The tile from the board which is intended to be claimed by the Side</param>
  /// <param name="s">The side that is claiming the tile</param>
  /// <returns>True if the move is valid according to the <see cref="validMove"/>. False, otherwise</returns>
  public bool ValidateMove(Board b, Tile t, Side s)
  {
    switch (validMove)
    {
      case ValidMove.Anywhere:
        return t.piece == null;
      case ValidMove.OnTop:
        Tile verticalNeighbor = b.Neighbor(t, Board.Direction.Vertical);
        return t.piece == null && (verticalNeighbor == null || verticalNeighbor.piece != null);
    }
    return false;
  }

  /// <summary>
  /// Listener for the <see cref="Match"/> match's OnGameEnded event.
  /// Declares a match as over if the games won by the winning side exceeds
  /// the threshold for the number of games required to win.
  /// </summary>
  /// <param name="m">The match that the game belongs to</param>
  /// <param name="b">The board that the game was won on</param>
  /// <param name="winner">The winning side</param>
  private void OnGamEnded(Match m, Board b, Side winner)
  {
    if (winner != null)
    {
      int gamesWonBySide = m.games[winner];
      if (gamesWonBySide >= gamesToWin)
      {
        m.EndMatch(winner);
      }
    }
  }

  /// <summary>
  /// Listener for the <see cref="Match"/> match's OnTurnEnded event.
  /// Checks for a stalemate or winner when a turn is declared over.
  /// </summary>
  /// <param name="m">The current match</param>
  /// <param name="turn">The turn index associated with the turn that just ended</param>
  /// <param name="sides">The sides, the side that ended the turn is <code>sides[turn]</code></param>
  private void OnTurnEnded(Match m, int turn, Side[] sides)
  {
    if ( m.Stalemate())
    {
      m.EndGame(null);
    }

    switch (winCondition)
    {
      case WinCondition.MatchN:
        if (m.board.CheckConsecutiveTiles(sides[turn], consecutiveTiles) )
        {
          m.EndGame(sides[turn]);
        }
        break;
    }
  }
}
