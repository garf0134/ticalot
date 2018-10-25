using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class AIPlayer : PlayerBase
{
  public enum Strategy
  {
    PureRandom,
    Normal
  }

  public Strategy strategy;
  public class RunInfo
  {
    public int[] run = new int[4];
    public int[] claimed = new int[4];
    public Tile tile;

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
  RunInfo[,] longestRuns;
  RunInfo[,] opponentRuns;

  protected override void SetMatch(Match m)
  {
    base.SetMatch(m);
    m.OnGameBegan += OnGameBegan;
    m.OnGameEnded += OnGameEnded;
  }

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

  private string LongestRunToString(RunInfo[,] run)
  {
    StringBuilder builder = new StringBuilder();
    for (int d = 0; d < 4; d++)
    {
      string header = ((Board.Direction)d).ToString();
      if (header.Length > 9)
      {
        header = header.Substring(header.Length - 9, 9);
      }
      builder.AppendFormat("{0,-9}\t",  header);
    }
    builder.AppendLine();

    for (int r = 0; r < run.GetLength(0); r++)
    {
      for (int d = 0; d < 4; d++)
      {
        for (int c = 0; c < run.GetLength(1); c++)
        {
          builder.AppendFormat("{0,-3}", run[r, c].run[(int)d]);
        }

        builder.Append("\t");
      }
      builder.AppendLine();
    }
    return builder.ToString();
  }

  private string LongestClaimedToString(RunInfo[,] run)
  {
    StringBuilder builder = new StringBuilder();
    for (int d = 0; d < 4; d++)
    {
      string header = ((Board.Direction)d).ToString();
      if (header.Length > 9)
      {
        header = header.Substring(header.Length - 9, 9);
      }
      builder.AppendFormat("{0,-9}\t", header);
    }
    builder.AppendLine();

    for (int r = 0; r < run.GetLength(0); r++)
    {
      for (int d = 0; d < 4; d++)
      {
        for (int c = 0; c < run.GetLength(1); c++)
        {
          builder.AppendFormat("{0,-3}", run[r, c].claimed[(int)d]);
        }
        builder.Append('\t');
      }
      builder.AppendLine();
    }
    return builder.ToString();
  }
  
  protected void OnGameBegan(Match m, int game, Board b, Side[] sides)
  {
    longestRuns = new RunInfo[m.board.rows, m.board.cols];
    opponentRuns = new RunInfo[m.board.rows, m.board.cols];

    InitializeRuns(m, b, longestRuns);
    InitializeRuns(m, b, opponentRuns);
  }

  private void InitializeRuns(Match m, Board b, RunInfo[,] runs)
  {
    // Handle vertical and horizontal runs
    for (int r = 0; r < m.board.rows; r++)
    {
      for (int c = 0; c < m.board.cols; c++)
      {
        runs[r, c] = new RunInfo();
        runs[r, c].run[(int)Board.Direction.Horizontal] = m.board.rows;
        runs[r, c].run[(int)Board.Direction.Vertical] = m.board.cols;
        runs[r, c].tile = m.board[r, c];
        b[r, c].OnPiecePlaced += OnPiecePlaced;
      }
    }

    // Handle diagonals runs from the left side 
    for (int r = 0; r < m.board.rows; r++)
    {
      int c = 0, k = r;
      int sum = 0;
      while (k-- >= 0 && c++ < m.board.cols) { sum++; }
      c = 0;
      k = r;
      while (k >= 0 && c < m.board.cols)
      {
        runs[k, c].run[(int)Board.Direction.Diagonal_BottomUp] = sum;
        runs[m.board.rows - k - 1, c].run[(int)Board.Direction.Diagonal_TopDown] = sum;
        k--;
        c++;
      }
    }

    // Handle diagonal runs from the bottom edge
    for (int c = 0; c < m.board.cols; c++)
    {
      int r = m.board.rows - 1;
      int k = c;
      int sum = 0;
      while (r-- >= 0 && k++ < m.board.cols) { sum++; }

      r = m.board.rows - 1;
      k = c;
      while (r >= 0 && k < m.board.cols)
      {
        runs[r, k].run[(int)Board.Direction.Diagonal_BottomUp] = sum;
        runs[m.board.rows - r - 1, k].run[(int)Board.Direction.Diagonal_TopDown] = sum;
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
  /// Scans through 2 of the 3 dimensions of a model storing an array of integers in the third dimension. This
  /// array represents the longest run of empty or consecutive pieces owned by the given Side at a given coordinate
  /// and in one of the four directions supported by the code.
  /// </summary>
  /// <param name="model">The 3-dimensional array of an integer run count sum per 2D coordinates, and a direction </param>
  /// <param name="pos">The 2-dimensional coordinate to start scanning from</param>
  /// <param name="step">The 2-dimensional vector to step by. Usual values are -1, 0 or 1</param>
  /// <param name="index">The index into the third dimension <see cref="Board.Direction"/></param>
  /// <param name="callback">A callback to signal the stop of the scan.</param>
  /// <returns></returns>
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

  private Tile RandomStrategy(Board b, List<(int, int)> validMoves)
  {
    var randomeTileCoords = validMoves[UnityEngine.Random.Range(0, validMoves.Count)];
    Tile chosenMove = b[randomeTileCoords.Item1, randomeTileCoords.Item2];
    return chosenMove;
  }

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
