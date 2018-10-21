using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    string score = "";
    foreach (var side in m.turnOrder)
    {
      score += string.Format("{0}-{1}", side.name, m.games[side]);
    }
    gameScore.text = score;
  }

  public void UpdateRules(Match m)
  {
    gameRulesName.text = m.ruleset.name;
    gameWinCondition.text = m.ruleset.winCondition.ToString();
    matchWinCondition.text = string.Format("First to {0} of {1} games", m.ruleset.gamesToWin, m.ruleset.maxGames);
  }

  public void UpdateTurnInstructions(Match m)
  {
    Side s = m.turnOrder[m.turn];
    switch (s.role)
    {
      case Side.Role.Human:
        turnInstructions.text = string.Format("{0} Team's Turn.\nPlease select a tile.", s.name);
        break;
      case Side.Role.AI:
        turnInstructions.text = string.Format("{0} Team's Turn.\nPlease wait");
        break;
    }
    turnInstructions.enabled = true;
  }
}
