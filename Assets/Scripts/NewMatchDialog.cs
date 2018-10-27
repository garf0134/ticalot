using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the UI for the New Match Dialog
/// </summary>
public class NewMatchDialog : MonoBehaviour
{
  /// <summary>The match prefab that is used to instantiate a new match</summary>
  public GameObject matchPrefab;
  /// <summary>The canvas group that is animated to show/hide the dialog</summary>
  public CanvasGroup canvasGroup;
  /// <summary>The number of rows that the board the matches games use</summary>
  public InputField rows;
  /// <summary>The number of columns that the board the matches games use</summary>
  public InputField columns;
  /// <summary>The number of consecutive tiles that are needed if the game's
  /// win condition is set to MatchN</summary>
  public InputField matchN;
  /// <summary>The number of games required to win</summary>
  public InputField gamesToWin;
  /// <summary>The maximum number of games that are played</summary>
  public InputField maxGames;

  /// <summary>The ruleset being built for the next match</summary>
  public Ruleset r;
  /// <summary>The PlayerSettings UI controller for each player</summary>
  public List<PlayerSettings> playerSettings = new List<PlayerSettings>();
  /// <summary>The WinConditionSetting UI controller for each win condition</summary>
  public List<WinConditionSetting> winConditions = new List<WinConditionSetting>();
  /// <summary>The MovementRuleSetting UI controller for each movement rule</summary>
  public List<MovementRuleSetting> movementRules = new List<MovementRuleSetting>();

  /// <summary>
  /// A delegate for the <see cref="OnNewMatch"/> event
  /// </summary>
  /// <param name="m">The new match</param>
  public delegate void NewMatchEvent(Match m);
  public event NewMatchEvent OnNewMatch;

  /// <summary>
  /// The Unity initialization hook.
  /// Populates playerSettings, winConditions and movementRules
  /// Sets up the validators for the text input fields
  /// </summary>
  void Start()
  {
    playerSettings.AddRange(GetComponentsInChildren<PlayerSettings>());
    winConditions.AddRange(GetComponentsInChildren<WinConditionSetting>());
    movementRules.AddRange(GetComponentsInChildren<MovementRuleSetting>());

    // Randomize the sides icons and colors and add callbacks. Also set the first two
    // player settings toggles to enabled and read-only
    List<int> colorsNotChosen = new List<int>();

    for (int i = 0; i < playerSettings.Count; i++)
    {
      var playerSetting = playerSettings[i];
      if ( i < 2 )
      {
        playerSetting.sideEnabled.isOn = true;
        playerSetting.sideEnabled.interactable = false;
      }

      playerSetting.colorDropdown.onValueChanged.AddListener((int value) =>
      {
        // Build a list of all the options
        List<int> valuesNotChosen = new List<int>();
        for (int j = 0; j < playerSetting.colorDropdown.options.Count; j++)
        {
          valuesNotChosen.Add(j);
        }

        // Remove all options that are taken already by some other player
        foreach (var pSetting in playerSettings)
        {
          if (pSetting == playerSetting)
          {
            continue;
          }
          valuesNotChosen.Remove(pSetting.colorDropdown.value);
        }

        if (valuesNotChosen.Count < 1)
        {
          throw new System.ArgumentOutOfRangeException("colors", "Not enough side colors for each side");
        }

        // If the requested values is not available, choose the first available
        if (valuesNotChosen.IndexOf(value) < 0)
        {
          playerSetting.colorDropdown.value = valuesNotChosen[0];
        }
      });

      
      playerSetting.iconDropdown.value = Random.Range(0, playerSetting.iconDropdown.options.Count);

      // The first playersetting gets all the color choices
      if ( i == 0)
      {
        colorsNotChosen.AddRange(System.Linq.Enumerable.Range(0, playerSetting.colorDropdown.options.Count));
      }

      playerSetting.colorDropdown.value = colorsNotChosen[Random.Range(0, colorsNotChosen.Count)];
      colorsNotChosen.Remove(playerSetting.colorDropdown.value);
    }

    rows.onValidateInput += ValidateRowsAndColumns;
    rows.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.rows = int.Parse(newValue); } });

    columns.onValidateInput += ValidateRowsAndColumns;
    columns.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.cols = int.Parse(newValue); }  });

    matchN.onValidateInput += ValidateMatchN;
    matchN.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.consecutiveTiles = int.Parse(newValue); } });

    maxGames.onValidateInput += ValidateMaxGames;
    maxGames.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.maxGames = int.Parse(newValue); }  });

    gamesToWin.onValidateInput += ValidateGamesToWin;
    gamesToWin.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.gamesToWin = int.Parse(newValue); } });
  }

  /// <summary>
  /// A listener for Unity's OnValidateInput event
  /// Ensures that the converted integer value of the text field is 3 or greater
  /// </summary>
  /// <param name="s">The string as it was</param>
  /// <param name="charIndex">The index of the character that was changed/added</param>
  /// <param name="addedChar">The value of the character</param>
  /// <returns>Returns the <paramref name="addedChar"/> if validated, '\0' otherwise</returns>
  private char ValidateRowsAndColumns(string s, int charIndex, char addedChar)
  {
    System.Text.StringBuilder builder = new System.Text.StringBuilder(s);
    if (charIndex < builder.Length)
    {
      builder[charIndex] = addedChar;
    }
    else
    {
      builder.Append(addedChar);
    }
    return int.Parse(builder.ToString()) >= 3 ? addedChar : '\0';
  }

  /// <summary>
  /// A listener for Unity's OnValidateInput event
  /// Ensures that the converted integer value of the text field is no greater than the
  /// lesser of the number of rows or columns of the board.
  /// </summary>
  /// <param name="s">The string as it was</param>
  /// <param name="charIndex">The index of the character that was changed/added</param>
  /// <param name="addedChar">The value of the character</param>
  /// <returns>Returns the <paramref name="addedChar"/> if validated, '\0' otherwise</returns>
  private char ValidateMatchN(string s, int charIndex, char addedChar)
  {
    System.Text.StringBuilder builder = new System.Text.StringBuilder(s);
    if (charIndex < builder.Length)
    {
      builder[charIndex] = addedChar;
    }
    else
    {
      builder.Append(addedChar);
    }
    int lesserOfRowsAndColumns = Mathf.Min(int.Parse(rows.text), int.Parse(columns.text));
    int newN = int.Parse(builder.ToString());
    return (newN > 0 && newN <= lesserOfRowsAndColumns) ? addedChar : '\0';
  }

  /// <summary>
  /// A listener for Unity's OnValidateInput event
  /// Ensures that the games needed to win is greater than 0 and less than the current 
  /// value of the maximum number of games
  /// </summary>
  /// <param name="s">The string as it was</param>
  /// <param name="charIndex">The index of the character that was changed/added</param>
  /// <param name="addedChar">The value of the character</param>
  /// <returns>Returns the <paramref name="addedChar"/> if validated, '\0' otherwise</returns>
  private char ValidateGamesToWin(string s, int charIndex, char addedChar)
  {
    System.Text.StringBuilder builder = new System.Text.StringBuilder(s);
    if (charIndex < builder.Length)
    {
      builder[charIndex] = addedChar;
    }
    else
    {
      builder.Append(addedChar);
    }

    int newGamesToWin = int.Parse(builder.ToString());
    return (newGamesToWin > 0 && newGamesToWin <= int.Parse(maxGames.text)) ? addedChar : '\0';
  }

  /// <summary>
  /// A listener for Unity's OnValidateInput event
  /// Ensures that the maximum number of games is ???
  /// </summary>
  /// <param name="s">The string as it was</param>
  /// <param name="charIndex">The index of the character that was changed/added</param>
  /// <param name="addedChar">The value of the character</param>
  /// <returns>Returns the <paramref name="addedChar"/> if validated, '\0' otherwise</returns>
  private char ValidateMaxGames(string s, int charIndex, char addedChar)
  {
    System.Text.StringBuilder builder = new System.Text.StringBuilder(s);
    if (charIndex < builder.Length)
    {
      builder[charIndex] = addedChar;
    }
    else
    {
      builder.Append(addedChar);
    }

    int newMaxGames = int.Parse(builder.ToString());
    return (newMaxGames > 0 && newMaxGames <= int.Parse(maxGames.text)) ? addedChar : '\0';
  }

  /// <summary>
  /// Triggers the show animation which ends with the new match dialog visible and
  /// interactable
  /// </summary>
  public void Show()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Show");

    Destroy(r?.m?.gameObject);
    r = null;
  }

  /// <summary>
  /// A Unity animation hook for when the show animation has finished.
  /// Used to create a new ruleset.
  /// </summary>
  public void OnShowFinished()
  {
    r = ScriptableObject.CreateInstance<Ruleset>();
  }

  /// <summary>
  /// Updates the ruleset's winCondition whenever one of the 
  /// toggles in the win condition toggle group is toggled on.
  /// </summary>
  /// <param name="b">True if the toggle was turned on directly by the user</param>
  public void OnMatchWinConditionToggled(bool b)
  {
    foreach (var condition in winConditions)
    {
      Toggle t = condition.GetComponent<Toggle>();
      if (t.isOn)
      {
        r.winCondition = condition.winCondition;
        break;
      }
    }
  }

  /// <summary>
  /// Updates the ruleset's match move rule whenever one of the 
  /// toggles in the match move condition toggle group is toggled on.
  /// </summary>
  /// <param name="b">True if the toggle was turned on directly by the user</param>
  public void OnMatchMoveRuleToggled(bool b)
  {
    foreach (var moveRule in movementRules)
    {
      Toggle t = moveRule.GetComponent<Toggle>();
      if (t.isOn)
      {
        r.validMove = moveRule.movement;
      }
    }
  }

  /// <summary>
  /// A listener for the Unity UI Button event
  /// Hides the new match dialog
  /// </summary>
  public void OnMatchStartClick()
  {
    Hide();
  }

  /// <summary>
  /// Starts the hide animation
  /// </summary>
  public void Hide()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Hide");
  }

  /// <summary>
  /// A Unity Animation event hook 
  /// Is called when the hide animation is finished.
  /// Instantiates a new match and converts player setting UI values
  /// into new Side instances and re-assigns the ruleset from the
  /// dialog to the match.
  /// Finally, <see cref="OnNewMatch"/> is invoked
  /// </summary>
  public void OnHideFinished()
  {
    GameObject matchInstance = Object.Instantiate<GameObject>(matchPrefab);
    Match m = matchInstance.GetComponent<Match>();
    foreach (PlayerSettings playerSetting in playerSettings)
    {
      Side s = new Side();
      s.color = playerSetting.SelectedColor();
      s.role = playerSetting.SelectedRole();
      s.iconName = playerSetting.SelectedIcon();
      s.name = playerSetting.SelectedName();
      s.aiStrategy = playerSetting.SelectedAIRoleStrategy();

      m.RegisterSide(s);
    }
    r.SetMatch(m);
    m.ruleset = r;

    OnNewMatch?.Invoke(m);
  }
}
