using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HUD : MonoBehaviour
{
  public CanvasGroup canvasFader;

  public NewMatchDialog newMatch;
  public MatchHUD matchHUD;
  public RectTransform title;

  public GameObject piecePrefab;
  public GameObject tilePrefab;
  public GameObject playerPrefab;

  public Color tileHoverColor;
  public Color tileHoverIllegalMoveColor;
  public Color tileNormalColor;
  public float tileDimensions;
  public float tileDropRate = 4.0f;
  public float tileDestroyRate = 10.0f;

  private Match match;
  private Tile hovered;

  private Coroutine tileSetup;
  private Coroutine tileCleanup;
  private Coroutine turnIntermission;

  public List<PlayerBase> players = new List<PlayerBase>();

  // Start is called before the first frame update
  void Start()
  {
    newMatch.OnNewMatch += OnNewMatch;
    newMatch.OnNewMatch += matchHUD.OnNewMatch;
  }

  public void OnShowFinished()
  {
    canvasFader.gameObject.SetActive(false);
    StartCoroutine(WaitForInput());
  }

  IEnumerator SetupTiles(Ruleset ruleset, Board b)
  {
    transform.position += Vector3.up * 4.0f;
    yield return new WaitForSecondsRealtime(1.0f);
    for (int r = 0; r < ruleset.rows; r++)
    {
      for (int c = 0; c < ruleset.cols; c++ )
      {
        GameObject tileInstance = Instantiate<GameObject>(tilePrefab);
        Tile t = tileInstance.GetComponent<Tile>();
        t.name = string.Format("{0},{1}", r, c);
        t.row = r;
        t.column = c;
        Material m = tileInstance.GetComponent<MeshRenderer>().material;
        m.color = tileNormalColor;

        Vector3 finalBoardPosition = Vector3.right * (c - ruleset.cols / 2.0f) * tileDimensions + Vector3.forward * ( (ruleset.rows - r - 1) - ruleset.rows / 2.0f) * tileDimensions;
        t.transform.SetParent(b.transform);
        t.transform.position = b.transform.position + finalBoardPosition + Vector3.up * 10.0f;

        yield return new WaitForSecondsRealtime(1 / tileDropRate);
      }
    }

    yield return new WaitForSecondsRealtime(1.0f);
    matchHUD.Show();
    yield return new WaitForSecondsRealtime(0.5f);

    b.ScanForTiles();
    match.BeginMatch();

    yield return new WaitForSecondsRealtime(0.5f);
    match.BeginGame();

    yield return new WaitForSecondsRealtime(0.5f);
    match.BeginTurn();

    tileSetup = null;
  }

  private IEnumerator CleanupPieces(Board b)
  {
    yield return turnIntermission;

    foreach (Tile t in b.GetComponentsInChildren<Tile>())
    {
      if (t.piece)
      {
        Destroy(t.piece.gameObject);
        t.piece = null;
      }

      yield return new WaitForSecondsRealtime(1 / tileDestroyRate) ;
    }

    OnPiecesCleanedUp();

    tileCleanup = null;
    
  }

  private void OnPiecesCleanedUp()
  {
    if (!match.HasWinner())
    {
      match.BeginGame();

      turnIntermission = StartCoroutine(TurnIntermission());
    }
  }

  private void OnNewMatch(Match m)
  {
    match = m;
    match.OnTurnBegan += OnTurnBegan; ;
    match.OnTurnEnded += OnTurnEnded;
    match.OnGameEnded += OnGameEnded;

    foreach (Side s in m.turnOrder)
    {
      GameObject playerObject = new GameObject(s.name);
      PlayerBase player = null;
      switch (s.role)
      {
        case Side.Role.Human:
          player = playerObject.AddComponent<HumanPlayer>();
          break;
        case Side.Role.AI:
          AIPlayer aiPlayer = playerObject.AddComponent<AIPlayer>();
          aiPlayer.strategy = s.aiStrategy;
          player = aiPlayer;
          break;
      }

      if (player == null)
      {
        throw new System.ArgumentException("Invalid role argument");
      }
      player.side = s;
      player.match = m;
      player.hud = this;
      player.piecePrefab = piecePrefab;
      player.transform.SetParent(match.transform);

      players.Add(player);
    }
    tileSetup = StartCoroutine(SetupTiles(m.ruleset, m.board));
  }

  private void OnGameEnded(Match m, Board b, Side winner)
  {
    UpdateHover(null);

    if (!m.HasWinner())
    {
      StartCoroutine(CleanupPieces(b));
    }
    else
    {
      StartCoroutine(WaitForInput());
    }
  }

  private IEnumerator WaitForInput()
  {
    title.gameObject.SetActive(true);

    while (!Input.GetMouseButtonDown(0))
    {
      yield return null;
    }

    if ( match != null)
    {
      Destroy(match.gameObject);
      match = null;
    }

    title.gameObject.SetActive(false);
    
    newMatch.Show();
  }

  private void OnTurnBegan(Match m, int turn, Side[] sides)
  {

  }

  private void OnTurnEnded(Match m, int turn, Side[] sides)
  {
    UpdateHover(null);

    turnIntermission = StartCoroutine(TurnIntermission());
  }

  private IEnumerator TurnIntermission()
  {
    yield return new WaitForSecondsRealtime(1.0f);
    if (match != null)
    {
      match.BeginTurn();
    }
  }

  public void UpdateHover(Tile t)
  {
    if (hovered != null)
    {
      Material previousHoveredTileMaterial = hovered.GetComponentInChildren<MeshRenderer>().material;
      previousHoveredTileMaterial.color = tileNormalColor;
      hovered = null;
    }

    hovered = t;

    if (t != null && match != null)
    {
      Material m = t.GetComponentInChildren<MeshRenderer>().material;
      if (match.ruleset.ValidateMove(match.board, t, match.turnOrder[match.turn]))
      {
        m.color = tileHoverColor;
      }
      else
      {
        m.color = tileHoverIllegalMoveColor;
      }
    }
  }

  // Update is called once per frame
  void Update()
  {
    // Saved for later when this if-statement will prevent 'normal' processing
    // when some special procesing is occuring.
    if (match == null || tileSetup != null || tileCleanup != null )
    {
      return;
    }
  }
}
