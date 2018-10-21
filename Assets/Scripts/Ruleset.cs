using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NamedAttribute: System.Attribute
{
  public string name { get; set; }
  public string description { get; set; }
}

[System.Serializable]
public class Ruleset: ScriptableObject
{ 
  public Match m;
  public int maxGames = 3;
  public int gamesToWin = 2;
  public int consecutiveTiles = 3;
  public int rows = 3;
  public int cols = 3;

  public enum WinCondition
  {
    [Named(name = "Match N", description ="Match {0} Consecutive Tiles")]
    MatchN
  }
  public WinCondition winCondition;
  public enum ValidMove
  { 
    [Named(name = "Anywhere!", description = "Place your pieces anywhere there is an unoccupied tile")]
    Anywhere, // i.e. Tic-Tac-Toe
    [Named(name ="Drop", description ="Place your pieces above an occupied tile")]
    OnTop // i.e. Connect-N
  }
  public ValidMove validMove;

  // Start is called before the first frame update
  public void SetMatch(Match match)
  {
    m = match;
    m.OnTurnEnded += OnTurnEnded;
    m.OnGameEnded += OnGamEnded;
    m.OnMoveAttempted += OnMoveAttempted;
  }

  private bool OnMoveAttempted(Board b, Tile t, Piece p)
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

  private void OnGamEnded(Match m, Board b, Side winner)
  {
    int gamesWonBySide = m.games[winner];
    if (gamesWonBySide >= gamesToWin)
    {
      m.EndMatch(winner);
    }
  }

  private void OnTurnEnded(Match m, int turn, Side[] sides)
  {
    switch (winCondition)
    {
      case WinCondition.MatchN:
        if (m.board.CheckConsecutiveTiles(sides[turn], consecutiveTiles))
        {
          m.EndGame(sides[turn]);
        }
        break;
    }
  }
}
