using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.ComponentModel;
using TMPro;

/// <summary>
/// A UI focused class that manages the view for the state of a match
/// </summary>
public class MatchHUD : MonoBehaviour
{
  ///<summary>The label displaying the rules of the game</summary>
  public TMP_Text gameRulesName;
  ///<summary>The label displaying the game's win conditions</summary>
  public TMP_Text gameWinCondition;
  ///<summary>The label displaying the games won by each side</summary>
  public TMP_Text gameScore;
  ///<summary>The label displaying the match's win condition</summary>
  public TMP_Text matchWinCondition;
  ///<summary>The label displaying the turn's instructions</summary>
  public TMP_Text turnInstructions;

  ///<summary>The canvas group that controls fading/disabling the entire match HUD</summary>
  public CanvasGroup canvasGroup;
  ///<summary>The current match. Can be null if betwen matches.</summary>
  private Match match;

  /// <summary>
  /// Unity's event hook for startup
  /// Sets up the inital state at the start of the app so that the match hud is hidden
  /// and the turn instructions label is disabled.
  /// </summary>
  private void Start()
  {
    canvasGroup.alpha = 0;
    canvasGroup.interactable = false;
    CanvasGroup turnInstructionsCanvasGroup = turnInstructions.GetComponent<CanvasGroup>();
    turnInstructionsCanvasGroup.alpha = 0;
  }

  /// <summary>
  /// A listener for <see cref="NewMatchDialog"/>'s OnNewMatch event
  /// Sets the current match, initializes the text labels and sets up
  /// listeners for important match events.
  /// </summary>
  /// <param name="m">The new match</param>
  public void OnNewMatch(Match m)
  {
    match = m;

    UpdateRules(m);
    UpdateGameScore(m);

    m.OnGameEnded += OnGameEnded;
    m.OnTurnBegan += OnTurnBegan;
    m.OnTurnEnded += OnTurnEnded;
    m.OnMatchEnded += OnMatchEnded;
  }

  /// <summary>
  /// A listener for <see cref="Match"/>'s OnMatchEnded event
  /// Hides the match HUD now that there is no longer a match
  /// </summary>
  /// <param name="m">The match that just ended</param>
  /// <param name="winner">The winning side</param>
  private void OnMatchEnded(Match m, Side winner)
  {
    Hide();
  }

  /// <summary>
  /// Shows the Match HUD by starting the "Show" animation assigned
  /// to the animator belonging to this GameObject
  /// </summary>
  public void Show()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Show");
  }

  /// <summary>
  /// Hides the Match HUD by starting the "Hide" animation assigned
  /// to the animator belonging to this GameObject
  /// </summary>
  public void Hide()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Hide");
  }

  /// <summary>
  /// A listener for <see cref="Match"/>'s OnGameEndedEvent
  /// Updates the games won per side label
  /// </summary>
  /// <param name="m">The current match</param>
  /// <param name="b">The board of the game that was just won</param>
  /// <param name="winner">The winner of the game</param>
  private void OnGameEnded(Match m, Board b, Side winner)
  {
    UpdateGameScore(m);
  }

  /// <summary>
  /// A listener for <see cref="Match"/>'s OnTurnBegan event
  /// Updates the turn instructions depending if a human is playing or 
  /// and AI.
  /// </summary>
  /// <param name="m">The current match</param>
  /// <param name="turn">The new turn index</param>
  /// <param name="sides">The sides that are playing. The new side is <code>sides[turn]</code></param>
  private void OnTurnBegan(Match m, int turn, Side[] sides)
  {
    turnInstructions.enabled = true;
    UpdateTurnInstructions(m);
    //Debug.LogFormat("Starting {0}'s turn of game {1}", sides[turn].name, m.game);
  }

  public delegate void TurnInstructionEvent();
  public event TurnInstructionEvent OnShowTurnInstructionsFinished;
  public event TurnInstructionEvent OnHideTurnInstructionsFinished;

  public void ShowTurnInstructions()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Show Turn Instructions");
  }

  public void ShowTurnInstructionsFinished()
  {
    OnShowTurnInstructionsFinished?.Invoke();
  }

  public void HideTurnInstructions()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Hide Turn Instructions");
  }

  public void HideTurnInstructionsFinished()
  {
    OnHideTurnInstructionsFinished?.Invoke();
  }

  /// <summary>
  /// A listener for <see cref="Match"/>'s OnTurnEned event
  /// Hides turn instructions when a turn ends. This is only until the next turn
  /// begins a short while later.
  /// </summary>
  /// <param name="m"></param>
  /// <param name="turn"></param>
  /// <param name="sides"></param>
  private void OnTurnEnded(Match m, int turn, Side[] sides)
  {
    //Debug.LogFormat("Ending {0}'s of game {1}", sides[turn].name, m.game);
  }

  /// <summary>
  /// Updates the match's per-side game score
  /// </summary>
  /// <param name="m">The current match</param>
  public void UpdateGameScore(Match m)
  {
    string score = string.Join(" | ", m.turnOrder.Select((Side s) => { return string.Format("{0}-{1}", s.name, m.games[s]); }));
    gameScore.text = score;
  }

  /// <summary>
  /// Updates the rules for the win conditions for the game and match and 
  /// what constitutes a valid move.
  /// </summary>
  /// <param name="m">The current match</param>
  public void UpdateRules(Match m)
  {
    var winCondition = m.ruleset.winCondition.GetAttribute<NamedAttribute>();
    var validMove = m.ruleset.validMove.GetAttribute<NamedAttribute>();

    gameRulesName.text = string.Format(winCondition.name, m.ruleset.consecutiveTiles);
    gameWinCondition.text = string.Format(winCondition.description, m.ruleset.consecutiveTiles);
    matchWinCondition.text = string.Format("First to {0} of {1} games", m.ruleset.gamesToWin, m.ruleset.maxGames);
  }

  /// <summary>
  /// Updates the turn instructions depending on the current turn
  /// </summary>
  /// <param name="m">The current match</param>
  public void UpdateTurnInstructions(Match m)
  {
    Side s = m.turnOrder[m.turn];
    switch (s.role)
    {
      case Side.Role.Human:
        turnInstructions.text = string.Format("<sprite name=\"{0}\"> {1} Team's Turn.\nPlease select a tile.", s.iconName, s.name);
        break;
      case Side.Role.AI:
        turnInstructions.text = string.Format("<sprite name=\"{0}\"> {1} Team's Turn.\nPlease wait", s.iconName, s.name);
        break;
    }
  }
}
