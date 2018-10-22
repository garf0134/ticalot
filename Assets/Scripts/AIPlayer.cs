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

  private string LongestRunToString(Board.Direction d)
  {
    int directionAsInt = (int)d;
    StringBuilder builder = new StringBuilder();

    for (int r = 0; r < longestRuns.GetLength(0); r++)
    {
      for (int c = 0; c < longestRuns.GetLength(1); c++)
      {
        builder.AppendFormat("{0,-2}", longestRuns[r, c].run[(int)d]);
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

    string horizontalRepresentation = LongestRunToString(Board.Direction.Horizontal);
    string verticalRepresentation = LongestRunToString(Board.Direction.Vertical);
    string diagonalTopDownRepresentation = LongestRunToString(Board.Direction.Diagonal_TopDown);
    // Handle diagonals runs from the left side 
    string diagonalBottomUpRepresentation = LongestRunToString(Board.Direction.Diagonal_BottomUp);
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

        diagonalBottomUpRepresentation = LongestRunToString(Board.Direction.Diagonal_BottomUp);
        diagonalTopDownRepresentation = LongestRunToString(Board.Direction.Diagonal_TopDown);
      }
    }
  }

  /// <summary>
  /// A callback used to both signal that the scan through a 2-dimensional data set is finished as 
  /// well as providing write access to the underlying data in the 3rd dimension.
  /// </summary>
  /// <param name="count">The writeable count for the index given in the call to <see cref="Scan(int[,,], (int, int), (int, int), int, ScanCallback)"/></param>
  /// <returns>True if the end condition was encountered. False, if the scan should continue.</returns>
  private delegate bool ScanCallback(RunInfo info, int index);

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
  private List<RunInfo> Scan(RunInfo[,] model, (int, int) pos, (int, int) step, int index, ScanCallback scanCallback)
  {
    List<RunInfo> ret = new List<RunInfo>();
    (int, int) cursor = pos;
    while (cursor.Item1 >= 0 && cursor.Item1 < model.GetLength(0) &&
      cursor.Item2 >= 0 && cursor.Item2 < model.GetLength(1))
    {
      ret.Add(model[cursor.Item1, cursor.Item2]);

      if (model[cursor.Item1, cursor.Item2].run[index] < 1)
      {
        break;
      }
      cursor.Item1 += step.Item1;
      cursor.Item2 += step.Item2;
    }

    return ret;
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
      UpdateRuns(t, p.side, opponentRuns);
    }
    else
    {
      UpdateRuns(t, p.side, longestRuns);
    }
  }

  private void UpdateRuns(Tile t, Side focusSide, RunInfo[,] runs)
  {
    runs[t.row, t.column].run[(int)Board.Direction.Horizontal] = 0;
    runs[t.row, t.column].run[(int)Board.Direction.Vertical] = 0;
    runs[t.row, t.column].run[(int)Board.Direction.Diagonal_BottomUp] = 0;
    runs[t.row, t.column].run[(int)Board.Direction.Diagonal_TopDown] = 0;

    string longestRunRepresentation = LongestRunToString(Board.Direction.Horizontal);

    ScanCallback countAnyAvailable = (RunInfo runInfo, int index) => runInfo.run[index] < 1;

    // Horizontal
    int direction = (int)Board.Direction.Horizontal;
    var span = Scan(runs, (t.row, t.column - 1), (0, -1), direction, countAnyAvailable);
    var numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == side);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count; runInfo.claimed[direction] = numClaimed; }

    span = Scan(runs, (t.row, t.column + 1), (0, 1), direction, countAnyAvailable);
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == side);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count; runInfo.claimed[direction] = numClaimed; }

    // Vertical
    direction = (int)Board.Direction.Vertical;
    span = Scan(runs, (t.row - 1, t.column), (-1, 0), direction, countAnyAvailable);
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == side);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count; runInfo.claimed[direction] = numClaimed; }

    span = Scan(runs, (t.row + 1, t.column), (1, 0), (int)Board.Direction.Vertical, countAnyAvailable);
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == side);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count; runInfo.claimed[direction] = numClaimed; }

    // Diagonal_TopDown
    direction = (int)Board.Direction.Diagonal_TopDown;
    span = Scan(runs, (t.row - 1, t.column - 1), (-1, -1), direction, countAnyAvailable);
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == side);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count; runInfo.claimed[direction] = numClaimed; }

    span = Scan(runs, (t.row + 1, t.column + 1), (1, 1), direction, countAnyAvailable);
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == side);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count; runInfo.claimed[direction] = numClaimed; }

    // Diagonal_BottomUp
    direction = (int)Board.Direction.Diagonal_BottomUp;
    span = Scan(runs, (t.row + 1, t.column - 1), (1, -1), direction, countAnyAvailable);
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == side);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count; runInfo.claimed[direction] = numClaimed; }

    span = Scan(runs, (t.row - 1, t.column + 1), (-1, 1), direction, countAnyAvailable);
    numClaimed = span.Count((RunInfo runInfo) => runInfo.tile?.piece?.side == side);
    foreach (var runInfo in span) { runInfo.run[direction] = span.Count; runInfo.claimed[direction] = numClaimed; }
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
    List<(int,Tile)> scores = new List<(int, Tile)>();

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
          int score = 0;
          for (int i = 0; i < System.Enum.GetValues(typeof(Board.Direction)).Length; i++)
          {
            score += runInfo.run[i] + runInfo.claimed[i] + opponentRunInfo.claimed[i];
          }
          scores.Add((score, t));
          break;
      }
    }

    int highestScore = scores
      .OrderBy(((int, Tile) pair) => pair.Item1, Comparer<int>.Create((int left, int right) => right.CompareTo(left)))
      .Select( ((int,Tile) pair) => pair.Item1)
      .First() ;
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
