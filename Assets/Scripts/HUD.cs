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
  /// <summary>A reference to the class that manages the new match dialog UI</summary>
  public NewMatchDialog newMatch;
  /// <summary>A reference to the class that manages the match HUD UI</summary>
  public MatchHUD matchHUD;
  /// <summary>A reference to the title UI</summary>
  public RectTransform title;
  /// <summary>A reference to the fade-to/fade-from black</summary>
  public CanvasGroup fader;

  /// <summary>The game flow that the UI participates in</summary>
  public GameFlow gameFlow;

  /// <summary>A delegate listener for the OnFadeEnded, OnFadeBegan events</summary>
  public delegate void FadeAnimationEvent();
  /// <summary>The OnFadeBegan event is called when the canvasFader begins its 
  /// fade-from-black animation</summary>
  /// 
  public event FadeAnimationEvent OnFadeBegan;
  /// <sumary> THe OnFadeEnded event is called when the canvasFader end s its fade
  /// -from-black animation</sumary>
  public event FadeAnimationEvent OnFadeEnded;

  /// <summary>
  /// Begins the fade-from-black animation
  /// </summary>
  public void BeginFade()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Begin Fade");
    OnFadeBegan?.Invoke();
  }

  /// <summary>A hook into the fade-from-black animation. Called at the end. 
  /// Triggered by <see cref="BeginFade"/></summary>
  public void FadeEnded()
  {
    fader.blocksRaycasts = false;
    OnFadeEnded?.Invoke();
  }

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
  /// The current match
  /// </summary>
  private Match match {  get { return gameFlow.match; } }

  /// <summary>
  /// The tile that the player is currently hovering over
  /// </summary>
  private Tile hovered;

  /// <summary>
  /// Register listeners for the New Match Dialog's OnNewMatch event.
  /// </summary>
  void Start()
  {
    newMatch.OnNewMatch += OnNewMatch;
    newMatch.OnNewMatch += matchHUD.OnNewMatch;
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
    m.OnTurnEnded += OnTurnEnded;
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
      previousHoveredTileMaterial.color = gameFlow.tileNormalColor;
      hovered = null;
    }

    hovered = t;

    if (t != null && match != null)
    {
      Material m = t.GetComponentInChildren<MeshRenderer>().material;
      if (match.ruleset.ValidateMove(match.board, t, match.turnOrder[match.turn]))
      {
        m.color = gameFlow.tileHoverColor;
      }
      else
      {
        m.color = gameFlow.tileHoverIllegalMoveColor;
      }
    }
  }

  /// <summary>
  /// In the Unity Editor mode, this function also updates the debug text that
  /// appears alongside a hovered tile.
  /// </summary>
  void Update()
  {
#if UNITY_EDITOR
    // Saved for later when this if-statement will prevent 'normal' processing
    // when some special procesing is occuring.
    if (match == null)
    {
      return;
    }

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
