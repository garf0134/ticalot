using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// The AIPlayer class encapsulates the code necessary for an AI that 
/// can make decisions about moves. There are two strategies to pick from
/// Normal and PureRandom. The normal strategy will build scores from 
/// looking at how many consecutive tiles can be played in any direction. The
/// higher the score, the better move a tile becomes.
/// </summary>
public class AIPlayer : PlayerBase
{
  /// <summary>
  /// The strategy the AI Player will use to choose moves
  /// </summary>
  public enum Strategy
  {
    PureRandom,
    Normal
  }

  /// <summary> The strategy selected for the AI player </summary>
  public Strategy strategy;

  /// <summary>
  /// Tracks the number of consecutive tiles available to a side in any of the 
  /// four directions specified in <seealso cref="Board.Direction"/>. The class
  /// also tracks the number of claimed tiles in any run.
  /// </summary>
  public class RunInfo
  {
    /// <summary>The number of consecutive tiles in a direction</summary>
    public int[] run = new int[4];
    /// <summary>The number of claimed tiles in a direction</summary>
    public int[] claimed = new int[4];
    /// <summary>The tile at the center of the run</summary>
    public Tile tile;

    /// <summary>
    /// Overrides the ToString() method to return a full summary of the state of the RunInfo object
    /// </summary>
    /// <returns>A string representing the state of the RunInfo object</returns>
    public override string ToString()
    {
      return string.Format(
        "Vertical-{0}/{1}\nHorizontal-{2}/{3}\nDiagTopDown-{4}/{5}\nDiagBotUp-{6}/{7}",
        claimed[0], run[0],
        claimed[1], run[1],
        claimed[2], run[2],
        claimed[3], run[3]);
    }
  }
  /// <summary> A two dimensional array allocated to store runs for the AI from each tile on the board </summary>
  private RunInfo[,] longestRuns;
  /// <summary> A two dimensional array allocated to store runs for the AI's opponent from each tile on the board </summary>
  private RunInfo[,] opponentRuns;

  /// <summary>
  /// Sets the match that the AI player is playing in
  /// </summary>
  /// <param name="m">The match that the player is playing in</param>
  protected override void SetMatch(Match m)
  {
    base.SetMatch(m);
    m.OnGameBegan += OnGameBegan;
    m.OnGameEnded += OnGameEnded;
  }

  /// <summary>
  /// A hook for a match's OnGameEnded event. Allows the AI player to detach
  /// from every tile's OnPiecePlaced event.
  /// </summary>
  /// <param name="m">The match that the game that just ended is a part of</param>
  /// <param name="b">The board that the game was played on</param>
  /// <param name="winner">The side that was the winner of the game that just ended</param>
  private void OnGameEnded(Match m, Board b, Side winner)
  {
    for (int r = 0; r < b.rows; r++)
    {
      for (int c = 0; c < b.cols; c++)
      {
        b[r, c].OnPiecePlaced -= OnPiecePlaced;
      }
    }
  }
  /// <summary>
  /// A hook for a match's OnGameBegan event. Allows the AI player to initialize
  /// the RunInfo two-dimensional arrays.
  /// </summary>
  /// <param name="m">The match the new game belongs to</param>
  /// <param name="game">The game index of the game about to begin</param>
  /// <param name="b">The board that the game index is being played on</param>
  /// <param name="sides">The sides playing the game</param>
  protected void OnGameBegan(Match m, int game, Board b, Side[] sides)
  {
    longestRuns = new RunInfo[b.rows, b.cols];
    opponentRuns = new RunInfo[b.rows, b.cols];

    InitializeRuns(b, longestRuns);
    InitializeRuns(b, opponentRuns);
  }

  /// <summary>
  /// Set each direction of each tile to the number of consecutive tiles
  /// available in that direction
  /// </summary>
  /// <param name="b">The board that all tiles belong to</param>
  /// <param name="runs">The two dimensional array corresponding to a board's tile layout.</param>
  private void InitializeRuns(Board b, RunInfo[,] runs)
  {
    // Handle vertical and horizontal runs
    for (int r = 0; r < b.rows; r++)
    {
      for (int c = 0; c < b.cols; c++)
      {
        runs[r, c] = new RunInfo();
        runs[r, c].run[(int)Board.Direction.Horizontal] = b.rows;
        runs[r, c].run[(int)Board.Direction.Vertical] = b.cols;
        runs[r, c].tile = b[r, c];
        b[r, c].OnPiecePlaced += OnPiecePlaced;
      }
    }

    // Handle diagonals runs from the left side 
    for (int r = 0; r < b.rows; r++)
    {
      int c = 0, k = r;
      int sum = 0;
      while (k-- >= 0 && c++ < b.cols) { sum++; }
      c = 0;
      k = r;
      while (k >= 0 && c < b.cols)
      {
        runs[k, c].run[(int)Board.Direction.Diagonal_BottomUp] = sum;
        runs[b.rows - k - 1, c].run[(int)Board.Direction.Diagonal_TopDown] = sum;
        k--;
        c++;
      }
    }

    // Handle diagonal runs from the bottom edge
    for (int c = 0; c < b.cols; c++)
    {
      int r = b.rows - 1;
      int k = c;
      int sum = 0;
      while (r-- >= 0 && k++ < b.cols) { sum++; }

      r = b.rows - 1;
      k = c;
      while (r >= 0 && k < b.cols)
      {
        runs[r, k].run[(int)Board.Direction.Diagonal_BottomUp] = sum;
        runs[b.rows - r - 1, k].run[(int)Board.Direction.Diagonal_TopDown] = sum;
        k++;
        r--;
      }
    }
  }

  /// <summary>
  /// Updates our strategic model of the board. This looks like a lot of work is being done but
  /// we're only doing the minimum (up to O(N) in the worst case, albeit 4 times).
  /// </summary>
  /// <param name="t">The tile that was changed</param>
  /// <param name="p">The piece that was added</param>
  private void OnPiecePlaced(Tile t, Piece p)
  {
    if (p == null)
    {
      return;
    }

    if (p.side == side)
    {
      // We've made a move, update the runs in our opponent's model
      UpdateRuns(t, opponentRuns);
      // But update the claims in our model
      UpdateClaims(t, p.side, longestRuns);

    }
    else //if (p.side == opponent)
    {
      // The opponent has made a move, update our model
      UpdateRuns(t, longestRuns);
      // But update the claims in our opponent's model
      UpdateClaims(t, p.side, opponentRuns);
    }

#if UNITY_EDITOR
    foreach (RunInfo runInfo in longestRuns)
    {
      runInfo.tile?.RegisterDebug("AI Runs", runInfo.ToString());
    }
    foreach (RunInfo runInfo in opponentRuns)
    {
      runInfo.tile?.RegisterDebug("AI Opponent Runs", runInfo.ToString());
    }

    for (int r = 0; r < match.board.rows; r++)
    {
      for (int c = 0; c < match.board.cols; c++)
      {
        match.board[r, c].RegisterDebug("AI Score", ScoreDescription(match.board[r, c]).ToString());
      }
    }
#endif
  }

  /// <summary>
  /// Scans through the two dimensional array of the AI's working model of the game board.
  /// </summary>
  /// <param name="model">The 2-dimensional array of RunInfo instances, each corresponding to a tile on the game board </param>
  /// <param name="pos">The 2-dimensional coordinate to start scanning from</param>
  /// <param name="step">The 2-dimensional vector to step by. Usual values for the components are -1, 0 or 1</param>
  /// <param name="index">The index of the direction to search in <see cref="Board.Direction"/></param>
  /// <returns>Returns a list of tiles that were found in the scan</returns>
  private List<RunInfo> Scan(RunInfo[,] model, (int, int) pos, (int, int) step, int index)
  {
    List<RunInfo> ret = new List<RunInfo>();
    (int, int) cursor = pos;
    while (cursor.Item1 >= 0 && cursor.Item1 < model.GetLength(0) &&
      cursor.Item2 >= 0 && cursor.Item2 < model.GetLength(1) &&
      model[cursor.Item1, cursor.Item2].run[index] > 0)
    {
      ret.Add(model[cursor.Item1, cursor.Item2]);
      cursor.Item1 += step.Item1;
      cursor.Item2 += step.Item2;
    }

    return ret;
  }

  /// <summary>
  /// Update the number of claimed tiles in the span for each direction for each tile.
  /// </summary>
  /// <param name="t">The tile where the scan starts</param>
  /// <param name="s">The side we are updating claims for</param>
  /// <param name="runs">The 2-dimensional array corresponding to the tiles of a game board</param>
  private void UpdateClaims(Tile t, Side s, RunInfo[,] runs)
  {
    // Horizontal
    int direction = (int)Board.Direction.Horizontal;
    var span = Scan(runs, (t.row, t.column), (0, -1), direction).
      Concat(Scan(runs, (t.row, t.column + 1), (0, 1), direction)).ToList();
    var numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == s);
    foreach (var runInfo in span) { runInfo.claimed[direction] = numClaimed; }

    // Vertical
    direction = (int)Board.Direction.Vertical;
    span = Scan(runs, (t.row, t.column), (-1, 0), direction).
      Concat(Scan(runs, (t.row + 1, t.column), (1, 0), (int)Board.Direction.Vertical)).ToList();
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == s);
    foreach (var runInfo in span) {  runInfo.claimed[direction] = numClaimed; }

    // Diagonal_TopDown
    direction = (int)Board.Direction.Diagonal_TopDown;
    span = Scan(runs, (t.row, t.column), (-1, -1), direction).
      Concat(Scan(runs, (t.row + 1, t.column + 1), (1, 1), direction)).ToList();
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == s);
    foreach (var runInfo in span) {  runInfo.claimed[direction] = numClaimed; }

    // Diagonal_BottomUp
    direction = (int)Board.Direction.Diagonal_BottomUp;
    span = Scan(runs, (t.row, t.column), (1, -1), direction)
      .Concat(Scan(runs, (t.row - 1, t.column + 1), (-1, 1), direction)).ToList();
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == s);
    foreach (var runInfo in span) {  runInfo.claimed[direction] = numClaimed; }
  }

  /// <summary>
  /// Update the count for each direction for the given tile with the number of
  /// consecutive tiles unclaimed by the opponent.
  /// </summary>
  /// <param name="t">The tile to calculate the spans for</param>
  /// <param name="runs">The 2-dimensional array that corresponds to the tiles of Board</param>
  private void UpdateRuns(Tile t, RunInfo[,] runs)
  {
    runs[t.row, t.column].run[(int)Board.Direction.Horizontal] = 0;
    runs[t.row, t.column].run[(int)Board.Direction.Vertical] = 0;
    runs[t.row, t.column].run[(int)Board.Direction.Diagonal_BottomUp] = 0;
    runs[t.row, t.column].run[(int)Board.Direction.Diagonal_TopDown] = 0;

    // Horizontal
    int direction = (int)Board.Direction.Horizontal;
    var span = Scan(runs, (t.row, t.column - 1), (0, -1), direction);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count;  }

    span = Scan(runs, (t.row, t.column + 1), (0, 1), direction);    
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count;  }

    // Vertical
    direction = (int)Board.Direction.Vertical;
    span = Scan(runs, (t.row - 1, t.column), (-1, 0), direction);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count;  }

    span = Scan(runs, (t.row + 1, t.column), (1, 0), (int)Board.Direction.Vertical);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count;  }

    // Diagonal_TopDown
    direction = (int)Board.Direction.Diagonal_TopDown;
    span = Scan(runs, (t.row - 1, t.column - 1), (-1, -1), direction);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count;  }

    span = Scan(runs, (t.row + 1, t.column + 1), (1, 1), direction);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count;  }

    // Diagonal_BottomUp
    direction = (int)Board.Direction.Diagonal_BottomUp;
    span = Scan(runs, (t.row + 1, t.column - 1), (1, -1), direction);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count;  }

    span = Scan(runs, (t.row - 1, t.column + 1), (-1, 1), direction);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count;  }
  }

  /// <summary>
  /// The concrete implementation of the method that simulates the amount
  /// of time it takes a human player to think about a move before calculating
  /// the AI's move and registering it with the board.
  /// </summary>
  /// <param name="b">The board the game is being played on</param>
  /// <param name="ruleset">The ruleset that governs the rules of the game</param>
  /// <returns></returns>
  public override IEnumerator Play(Board b, Ruleset ruleset)
  {
    // Add human-like delay
    yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.5f, 1.2f));

    // Decide on move
    List<System.ValueTuple<int, int>> validMoves = new List<System.ValueTuple<int, int>>();
    for (int r = 0; r < b.rows; r++)
    {
      for (int c = 0; c < b.cols; c++)
      {
        if (ruleset.ValidateMove(b, b[r, c], side))
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
        chosenMove = RandomStrategy(b, validMoves);
        break;
      case Strategy.Normal:
        chosenMove = NormalStrategy(b, validMoves);
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

  /// <summary>
  /// The random strategy picks a tile from the list of valid moves at random.
  /// </summary>
  /// <param name="b">The board that the game is played on</param>
  /// <param name="validMoves">A list of row,column coordinates representing acceptable move choices</param>
  /// <returns></returns>
  private Tile RandomStrategy(Board b, List<(int, int)> validMoves)
  {
    var randomeTileCoords = validMoves[UnityEngine.Random.Range(0, validMoves.Count)];
    Tile chosenMove = b[randomeTileCoords.Item1, randomeTileCoords.Item2];
    return chosenMove;
  }

  /// <summary>
  /// The normal strategy picks a tile from the highest scoring tile in a list of valid moves
  /// </summary>
  /// <param name="b">The board that the game is played on</param>
  /// <param name="validMoves">A list of row,column coordinates representing acceptable move choices</param>
  /// <returns></returns>
  private Tile NormalStrategy(Board b, List<(int, int)> validMoves)
  {
    List<(int, Tile)> scores = new List<(int, Tile)>();

    foreach (var coord in validMoves)
    {
      Tile t = b[coord.Item1, coord.Item2];
      switch (match.ruleset.winCondition)
      {
        case Ruleset.WinCondition.MatchN:
          var runInfo = longestRuns[t.row, t.column];
          var opponentRunInfo = opponentRuns[t.row, t.column];

          // We value potential spots not yet taken, spots that have a high number of claimed
          // tiles (ours or theirs)
          scores.Add((Score(t), t));
          break;
      }
    }

    int highestScore = scores
      .OrderBy(((int, Tile) pair) => pair.Item1, Comparer<int>.Create((int left, int right) => right.CompareTo(left)))
      .Select(((int, Tile) pair) => pair.Item1)
      .First();
    List<Tile> highestTiles = scores
      .Where(((int, Tile) pair) => pair.Item1 == highestScore)
      .Select(((int, Tile) pair) => pair.Item2)
      .ToList();

    Tile chosenMove = null;
    if (highestTiles.Count > 0)
    {
      chosenMove = highestTiles[Random.Range(0, highestTiles.Count)];
    }
    return chosenMove;
  }

  /// <summary>
  /// Scores a tile, returning an integer representinga series of choices and summations
  /// </summary>
  /// <param name="t">The tile to calculate the score for</param>
  /// <returns>The score of the tile</returns>
  private int Score(Tile t)
  {
    int score = 0;
    RunInfo runInfo = longestRuns[t.row, t.column];
    RunInfo opponentRunInfo = opponentRuns[t.row, t.column];
    int N = match.ruleset.consecutiveTiles;

    for (int i = 0; i < System.Enum.GetValues(typeof(Board.Direction)).Length; i++)
    {
      // If there's no chance to finish the run don't consider it
      int directionScore = 0;
      int delta = runInfo.run[i] - runInfo.claimed[i];
      if (opponentRunInfo.claimed[i] >= (N - 1))
      {
        directionScore = 4 * opponentRunInfo.claimed[i];
      }
      else if (delta >= 1 && runInfo.run[i] >= match.ruleset.consecutiveTiles)
      {
        directionScore += runInfo.run[i] + 3*runInfo.claimed[i];
      }
      
      score += directionScore;
    }

    return score;
  }

#if UNITY_EDITOR
  /// <summary>
  /// BUilds a string with a detailed description of how the score is built for debugging purposes.
  /// Only useful within the context of the Unity editor.
  /// </summary>
  /// <param name="t">The tile we are scoring</param>
  /// <returns>The string representation of how the score was built in <see cref="Score(Tile)"/></returns>
  private string ScoreDescription(Tile t)
  {
    StringBuilder ret = new StringBuilder();
    RunInfo runInfo = longestRuns[t.row, t.column];
    RunInfo opponentRunInfo = opponentRuns[t.row, t.column];
    int N = match.ruleset.consecutiveTiles;
    int score = 0;
    for (int i = 0; i < 4; i++)
    {
      ret.AppendFormat("  {0}-", ((Board.Direction)i).ToString());
      if (opponentRunInfo.claimed[i] >= (N - 1))
      {
        ret.AppendFormat(" + 4*{0}", opponentRunInfo.claimed[i]);
        score += 4 * opponentRunInfo.claimed[i];
      }
      else
      {
        int delta = runInfo.run[i] - runInfo.claimed[i];
        if (delta < 1)
        {
          ret.Append("<0>");
        }
        else if (runInfo.run[i] < match.ruleset.consecutiveTiles)
        {
          ret.Append("[0]");
        }

        else
        {
          int directionScore = runInfo.run[i] + 3 * runInfo.claimed[i];
          ret.AppendFormat("{0} + {1}*{2}", runInfo.run[i], 3, runInfo.claimed[i]);
          score += directionScore;
        }
      }
      ret.AppendLine();
    }
    ret.AppendFormat("Sum: {0}", score);
    return ret.ToString();
  }
#endif
}
