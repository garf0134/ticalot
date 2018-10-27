using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Organizes the UI for the entire game and is responsible for 
/// starting up the game flow
/// </summary>
public class HUD : MonoBehaviour
{
  /// <summary>
  /// The CanvasGroup attached to the black rectangle. This is used to
  /// perform fade to/from black transitions.
  /// </summary>
  public CanvasGroup canvasFader;

  /// <summary>A reference to the class that manages the new match dialog UI</summary>
  public NewMatchDialog newMatch;
  /// <summary>A reference to the class that manages the match HUD UI</summary>
  public MatchHUD matchHUD;
  /// <summary>A reference to the title UI</summary>
  public RectTransform title;

  /// <summary>The 3d object used as the template for all pieces</summary>
  public GameObject piecePrefab;
  /// <summary>The 3d object used for each tile</summary>
  public GameObject tilePrefab;
  /// <summary>The mostly invisible player GameObject prefab</summary>
  public GameObject playerPrefab;

#if UNITY_EDITOR
  /// <summary>Which named Input Buttons should be checked. These correspond to the 
  /// string keys used in <see cref="AIPlayer.OnPiecePlaced(Tile, Piece)"/>
  /// </summary>
  public string[] modes = new string[0];
  /// <summary> The currently chosen mode from <see cref="modes"/></summary>
  public string debugMode;
  /// <summary> A reference to the UI used to store the registered (<see cref="Tile.RegisterDebug(string, string)"/> 
  /// text 
  /// </summary>
  public TMP_Text debugText;
#endif
  /// <summary>
  /// A designer variable for the color to be shown when the player
  /// hovers over a tile that is a legal move
  /// </summary>
  public Color tileHoverColor;
  /// <summary>
  /// A designer variable for the color to be shown when the player 
  /// hovers over a tile that is an illegal move
  /// </summary>
  public Color tileHoverIllegalMoveColor;
  /// <summary>A designer variable for the color to be shown when the tile is idle</summary>
  public Color tileNormalColor;
  /// <summary>
  /// A designer variable to denote the dimensions of a tile.
  /// Probably should calculate this from the mesh bounds of the prefab or refer to it
  /// as a tile interval since some gaps betwen tiles would look nice.
  /// </summary>
  public float tileDimensions;

  /// <summary>
  /// A designer variable to control how fast tiles will drop at the beginning
  /// of a game
  /// </summary>
  public float tileDropRate = 4.0f;
  /// <summary>
  /// A designer variable to control how quickly tiles are remove from the board
  /// at the end of a game
  /// </summary>
  public float tileDestroyRate = 10.0f;

  /// <summary>
  /// The current match
  /// </summary>
  private Match match;
  /// <summary>
  /// The tile that the player is currently hovering over
  /// </summary>
  private Tile hovered;

  /// <summary>
  /// The reference to the coroutine responsible for handling the game flow
  /// from the start of a game to the beginning of the first turn
  /// </summary>
  private Coroutine tileSetup;
  /// <summary>
  /// The reference to the coroutine responsible for handling the game flow
  /// at the end of the game 
  /// </summary>
  private Coroutine tileCleanup;
  /// <summary>
  /// The reference to the coroutine responsible for handling the game flow
  /// from the end of a turn to the beginning of the next turn
  /// </summary>
  private Coroutine turnIntermission;

  /// <summary>
  /// The list of players, one per side
  /// </summary>
  public List<PlayerBase> players = new List<PlayerBase>();

  /// <summary>
  /// Register listeners for the New Match Dialog's OnNewMatch event.
  /// </summary>
  void Start()
  {
    newMatch.OnNewMatch += OnNewMatch;
    newMatch.OnNewMatch += matchHUD.OnNewMatch;
  }

  /// <summary>
  /// An animation event listener for when the animation triggered by
  /// <see cref="Show"/> is finished
  /// </summary>
  public void OnShowFinished()
  {
    canvasFader.gameObject.SetActive(false);
    StartCoroutine(WaitForInput());
  }

  /// <summary>
  /// The coroutine to handle setting up tiles at the beginning of a game
  /// Also handles the gameflow from the start of a match to the beginning
  /// of the first turn
  /// </summary>
  /// <param name="ruleset">The ruleset of the next match</param>
  /// <param name="b">The board to set up tiles on</param>
  /// <returns>An enumerator that signals the caller when/where to resume
  /// the method</returns>
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

  /// <summary>
  /// Cleans up the pieces on the board by destroying each piece found 
  /// with a tile at a rate of <see cref="tileDestroyRate"/> pieces per
  /// second.
  /// </summary>
  /// <param name="b"></param>
  /// <returns></returns>
  private IEnumerator CleanupPieces(Board b)
  {
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

  /// <summary>
  /// Defers the game flow between games 1,2,3...,n-1 of a match
  /// to a new coroutine controled by <see cref="TurnIntermission"/>
  /// </summary>
  private void OnPiecesCleanedUp()
  {
    if (!match.HasWinner())
    {
      match.BeginGame();

      turnIntermission = StartCoroutine(TurnIntermission());
    }
  }

  /// <summary>
  /// A listener for the new match dialog's <see cref="NewMatchDialog.OnNewMatch"/>
  /// event
  /// Sets up listeners for the match's important events, sets up players from the
  /// match's turn order and kicks off the coroutine <see cref="SetupTiles(Ruleset, Board)"/>
  /// </summary>
  /// <param name="m"></param>
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

  /// <summary>
  /// A listener for the match's <see cref="Match.OnGameEnded"/>
  /// Resets the hovered tile, stops the turn intermission if it is running,
  /// cleans up pieces if the match is not over yet. If the match is over
  /// switch to the title screen.
  /// </summary>
  /// <param name="m"></param>
  /// <param name="b"></param>
  /// <param name="winner"></param>
  private void OnGameEnded(Match m, Board b, Side winner)
  {
    UpdateHover(null);

    if (turnIntermission != null)
    {
      StopCoroutine(turnIntermission);
      turnIntermission = null;
    }

    if (!m.HasWinner())
    {
      tileCleanup = StartCoroutine(CleanupPieces(b));
    }
    else
    {
      StartCoroutine(WaitForInput());
    }
  }

  /// <summary>
  /// A coroutine that is run between matches.
  /// Enables the title UI, waits for a mouse button press. When the mouse 
  /// button is pressed, destroys the match (if still active) and sets the
  /// tile to disabled and starts the show animation for the new match dialog.
  /// </summary>
  /// <returns></returns>
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

  /// <summary>
  /// A listener for the <see cref="Match.OnTurnBegan"/> event
  /// </summary>
  /// <param name="m">The current match</param>
  /// <param name="turn">The current turn</param>
  /// <param name="sides">The sides playing in the match, the current side is
  /// <code>sides[turn]</code></param>
  private void OnTurnBegan(Match m, int turn, Side[] sides)
  {

  }

  /// <summary>
  /// A listener for the <see cref="Match.OnTurnEnded"/> event
  /// </summary>
  /// <param name="m">The current match</param>
  /// <param name="turn">The current turn</param>
  /// <param name="sides">The sides playing in the match, the current side is 
  /// <code>sides[turn]</code></param>
  private void OnTurnEnded(Match m, int turn, Side[] sides)
  {
    UpdateHover(null);

    if ( tileCleanup == null )
    {
      turnIntermission = StartCoroutine(TurnIntermission());
    }
  }

  /// <summary>
  /// A couroutine that handles the time between turns.
  /// Pauses for one second before beginning the next turn
  /// </summary>
  /// <returns>An enumerator whose value tells the caller when/where
  /// to resume</returns>
  private IEnumerator TurnIntermission()
  {
    yield return new WaitForSecondsRealtime(1.0f);
    if (match != null)
    {
      match.BeginTurn();
    }
  }

  /// <summary>
  /// Reset which tile is hovered exclusively to all other tiles
  /// </summary>
  /// <param name="t">The tile to set as hovered. If null, clears the hovered 
  /// tile state.</param>
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

  /// <summary>
  /// Primary purpose is to update which tile is the hovered tile. In the 
  /// Unity Editor mode, this function also updates the debug text that
  /// appears alongside a hovered tile.
  /// </summary>
  void Update()
  {
    // Saved for later when this if-statement will prevent 'normal' processing
    // when some special procesing is occuring.
    if (match == null || tileSetup != null || tileCleanup != null )
    {
      return;
    }

#if UNITY_EDITOR
    foreach (string mode in modes)
    {
      if (Input.GetButtonDown(mode))
      {
        debugMode = (debugMode != null && debugMode.Length > 0 && debugMode == mode) ? null : mode;
      }
    }

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;
    if (Physics.Raycast(ray, out hit))
    {
      Tile t = hit.collider.GetComponentInParent<Tile>();
      if (t != null)
      {
        if (debugMode != null && debugMode.Length > 0)
        {
          string textToDisplay = t.RegisteredDebugText(debugMode);
          if (textToDisplay != null)
          {
            debugText.text = string.Format("{0}\n{1}\n", debugMode, t.RegisteredDebugText(debugMode));
            MeshRenderer meshRenderer = t.GetComponent<MeshRenderer>();

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(t.transform.position);
            RectTransform parent = debugText.rectTransform.parent as RectTransform;
            parent.anchoredPosition = new Vector2(screenPoint.x, screenPoint.y);
            parent.gameObject.SetActive(true);
            debugText.gameObject.SetActive(true);
          }
          else
          {
            debugText.gameObject.SetActive(false);
          }
        }
        else
        {
          debugText.gameObject.SetActive(false);
        }
      }
    }

#endif
  }
}
