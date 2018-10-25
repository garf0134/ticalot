using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.ComponentModel;
using TMPro;

public class MatchHUD : MonoBehaviour
{
  public TMP_Text gameRulesName;
  public TMP_Text gameWinCondition;
  public TMP_Text gameScore;
  public TMP_Text matchWinCondition;
  public TMP_Text turnInstructions;

  public CanvasGroup canvasGroup;
  private Match match;

  // Start is called before the first frame update
  void Start()
  {
    canvasGroup.alpha = 0;
    canvasGroup.interactable = false;
  }

  // Update is called once per frame
  void Update()
  {

  }

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

  private void OnMatchEnded(Match m, Side winner)
  {
    Hide();
  }

  public void Show()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Show");
  }

  public void Hide()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Hide");
  }

  private void OnGameEnded(Match m, Board b, Side winner)
  {
    UpdateGameScore(m);
  }

  private void OnTurnBegan(Match m, int turn, Side[] sides)
  {
    UpdateTurnInstructions(m);
  }

  private void OnTurnEnded(Match m, int turn, Side[] sides)
  {
    turnInstructions.enabled = false;
  }

  public void UpdateGameScore(Match m)
  {
    string score = string.Join(" | ", m.turnOrder.Select((Side s) => { return string.Format("{0}-{1}", s.name, m.games[s]); }));
    gameScore.text = score;
  }

  public void UpdateRules(Match m)
  {
    var winCondition = m.ruleset.winCondition.GetAttribute<NamedAttribute>();
    var validMove = m.ruleset.validMove.GetAttribute<NamedAttribute>();

    gameRulesName.text = string.Format(winCondition.name, m.ruleset.consecutiveTiles);
    gameWinCondition.text = string.Format(winCondition.description, m.ruleset.consecutiveTiles);
    matchWinCondition.text = string.Format("First to {0} of {1} games", m.ruleset.gamesToWin, m.ruleset.maxGames);
  }

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
    turnInstructions.enabled = true;
  }
}
