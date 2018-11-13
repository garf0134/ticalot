using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
  /// <summary>The dropdown controlling the type of board</summary>
  public Dropdown boardType;
  /// <summary>The dropdown controlling the type of tile</summary>
  public Dropdown tileType;
  
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
  /// A delegate for listeners of UI animation events
  /// </summary>
  public delegate void AnimationEvent();
  /// <summary>The show animation has finished</summary>
  public event AnimationEvent OnShowFinished;
  /// <summary>The hide animation has finished</summary>
  public event AnimationEvent OnHideFinished;
  /// <summary>All board configurations supported</summary>
  public BoardConfigurationSet boardConfigurations;

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
    foreach (var movementRuleSetting in movementRules)
    {
      Toggle toggle = movementRuleSetting.GetComponent<Toggle>();
      toggle.onValueChanged.AddListener((bool newValue) => {
        if (newValue)
        {
          boardType.onValueChanged?.Invoke(boardType.value);
        }
      });
    }

    boardConfigurations = Resources.Load<BoardConfigurationSet>("Default Board Configurations");
    BuildBoardDropdownOptions();

    boardType.onValueChanged.AddListener(newIndex =>
    {
      BoardRuleSetting boardRuleSetting = boardType.options[newIndex] as BoardRuleSetting;
      if (r != null)
      {
        r.boardResource = boardRuleSetting.boardResource;
      }

      BuildTileDropdownOptions();
      tileType.onValueChanged?.Invoke(tileType.value);
      var selectedBoardConfiguration = SelectedBoardConfiguration();
      foreach (var playerSetting in playerSettings)
      {
        playerSetting.OnBoardConfigurationChanged(selectedBoardConfiguration);
      }
    });

    tileType.onValueChanged.AddListener(newIndex =>
    {
      if (r != null)
      {
        TileRuleSetting tileRuleSetting = tileType.options[newIndex] as TileRuleSetting;
        r.tileResource = tileRuleSetting.tileResource;
      }
    });

    // Randomize the sides icons and colors and add callbacks. Also set the first two
    // player settings toggles to enabled and read-only
    List<int> colorsNotChosen = new List<int>();

    for (int i = 0; i < playerSettings.Count; i++)
    {
      var playerSetting = playerSettings[i];
      if (i < 2)
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
      playerSetting.pieceDropdown.value = Random.Range(0, playerSetting.pieceDropdown.options.Count);
      playerSetting.pieceDropdown.onValueChanged?.Invoke(playerSetting.pieceDropdown.value);

      // The first playersetting gets all the color choices
      if (i == 0)
      {
        colorsNotChosen.AddRange(System.Linq.Enumerable.Range(0, playerSetting.colorDropdown.options.Count));
      }

      int chosen = colorsNotChosen[Random.Range(0, colorsNotChosen.Count)];
      if (playerSetting.colorDropdown.value != chosen)
      {
        playerSetting.colorDropdown.value = chosen;
      }
      else
      {
        playerSetting.colorDropdown.onValueChanged?.Invoke(chosen);
      }
      colorsNotChosen.Remove(playerSetting.colorDropdown.value);
    }

    rows.onValidateInput += ValidateRowsAndColumns;
    rows.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.rows = int.Parse(newValue); } });

    columns.onValidateInput += ValidateRowsAndColumns;
    columns.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.cols = int.Parse(newValue); } });

    matchN.onValidateInput += ValidateMatchN;
    matchN.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.consecutiveTiles = int.Parse(newValue); } });

    maxGames.onValidateInput += ValidateMaxGames;
    maxGames.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.maxGames = int.Parse(newValue); } });

    gamesToWin.onValidateInput += ValidateGamesToWin;
    gamesToWin.onValueChanged.AddListener((string newValue) => { if (newValue.Length > 0) { r.gamesToWin = int.Parse(newValue); } });

    var movementRule = movementRules.FirstOrDefault((ruleSetting) => { return ruleSetting.GetComponent<Toggle>().isOn; });
    if (movementRule != null)
    {
      movementRule.GetComponent<Toggle>().onValueChanged?.Invoke(true);
    }
  }

  /// <summary>
  /// Taking the currently selected movement rule, build a set of compatible boards and
  /// add their player-facing names to the boardType dropdown
  /// </summary>
  private void BuildBoardDropdownOptions()
  {
    List<Dropdown.OptionData> boardTypeOptions = new List<Dropdown.OptionData>();
    boardType.ClearOptions();

    MovementRuleSetting selectedRuleSetting = 
      movementRules.Find(setting => {
      return setting.GetComponent<Toggle>().isOn;
    });
    foreach (var boardConfiguration in boardConfigurations.configurations)
    {
      bool validMove = boardConfiguration.allowedMoves.Any(move => { return move == selectedRuleSetting.movement; });
      if (validMove)
      {
        foreach (var allowedBoard in boardConfiguration.allowedBoards)
        {
          GameObject boardObject = Resources.Load<GameObject>(allowedBoard);
          Board b = boardObject.GetComponent<Board>();
          BoardRuleSetting optionData = new BoardRuleSetting();
          optionData.text = b.boardName;
          optionData.boardResource = allowedBoard;
          boardTypeOptions.Add(optionData);
        }
      }
    }

    boardType.AddOptions(boardTypeOptions);
    boardType.value = 0;
  }

  /// <summary>
  /// Calculate the currently selected board configuration
  /// </summary>
  /// <returns>The currently selected board configuration</returns>
  private BoardConfigurationSet.BoardConfiguration SelectedBoardConfiguration()
  {
    BoardRuleSetting boardRuleSetting = (BoardRuleSetting)boardType.options[boardType.value];
    string selectedBoardResource = boardRuleSetting.boardResource;
    MovementRuleSetting selectedRuleSetting =
      movementRules.Find(setting => {
        return setting.GetComponent<Toggle>().isOn;
      });
    foreach (var boardConfiguration in boardConfigurations.configurations)
    {
      bool validMove = boardConfiguration.allowedMoves.Any(
        (movementRule) => { return movementRule == selectedRuleSetting.movement; });
      bool validResource = boardConfiguration.allowedBoards.Any(
        (resourcePath) => { return resourcePath == selectedBoardResource; });
      if (validMove && validResource )
      {
        return boardConfiguration;
      }
    }
    return null;
  }

  /// <summary>
  /// Calculates the currently selected tile configuration
  /// </summary>
  /// <returns>The resource path to the currently selected tile's resource</returns>
  private string SelectedTileConfiguration()
  {
    return tileType.options[tileType.value].text;
  }

  /// <summary>
  /// Using the currently selected board configuration, build a set of tile options
  /// that are used in the board configuration.
  /// </summary>
  private void BuildTileDropdownOptions()
  {
    List<Dropdown.OptionData> tileTypeOptions = new List<Dropdown.OptionData>();
    tileType.ClearOptions();
    BoardConfigurationSet.BoardConfiguration boardConfiguration = SelectedBoardConfiguration();
    foreach (var tileName in boardConfiguration.allowedTiles)
    {
      GameObject prefab = Resources.Load<GameObject>(tileName);
      Tile tile = prefab.GetComponent<Tile>();

      TileRuleSetting optionData = new TileRuleSetting();
      optionData.text = tile.tileFamilyName;
      optionData.tileResource = tileName;
      tileTypeOptions.Add(optionData);
    }
    tileType.AddOptions(tileTypeOptions);
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
  /// Calculates the selected movement rule
  /// </summary>
  /// <returns>The currently selected movement rule. Presently hardcoded to 
  /// <see cref="Ruleset.ValidMove.Anywhere"/></returns>
  private Ruleset.ValidMove SelectedMovementRule()
  {
    Ruleset.ValidMove ret = Ruleset.ValidMove.Anywhere;

    return ret;
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
    return (newMaxGames > 0 && newMaxGames >= int.Parse(gamesToWin.text)) ? addedChar : '\0';
  }

  /// <summary>
  /// Triggers the show animation which ends with the new match dialog visible and
  /// interactable
  /// </summary>
  public void Show()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Show");

    r = null;
  }

  /// <summary>
  /// A Unity animation hook for when the show animation has finished.
  /// Used to create a new ruleset.
  /// </summary>
  public void ShowFinished()
  {
    OnShowFinished?.Invoke();
    r = ScriptableObject.CreateInstance<Ruleset>();
    r.rows = int.Parse(rows.text);
    r.cols = int.Parse(columns.text);
    r.consecutiveTiles = int.Parse(matchN.text);
    r.maxGames = int.Parse(maxGames.text);
    r.gamesToWin = int.Parse(gamesToWin.text);

    // Assumes that there is one movement rule setting and win condition toggled on
    r.validMove = movementRules.First((movementRuleSetting) => { return movementRuleSetting.GetComponent<Toggle>().isOn; }).movement;
    r.winCondition = winConditions.First((winCondition) => { return winCondition.GetComponent<Toggle>().isOn; }).winCondition;
    r.boardResource = (boardType.options[boardType.value] as BoardRuleSetting).boardResource;
    r.tileResource = (tileType.options[tileType.value] as TileRuleSetting).tileResource;
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
      if (t.isOn && r != null)
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
  public void HideFinished()
  {
    OnHideFinished?.Invoke();

    GameObject matchInstance = Object.Instantiate<GameObject>(matchPrefab);
    Match m = matchInstance.GetComponent<Match>();
    foreach (PlayerSettings playerSetting in playerSettings)
    {
      Side s = new Side();
      s.color = playerSetting.SelectedColor();
      s.role = playerSetting.SelectedRole();
      s.iconName = playerSetting.SelectedIcon();
      s.pieceResource = playerSetting.SelectedPieceResource();
      s.name = playerSetting.SelectedName();
      s.aiStrategy = playerSetting.SelectedAIRoleStrategy();

      m.RegisterSide(s);
    }
    r.SetMatch(m);
    m.ruleset = r;

    OnNewMatch?.Invoke(m);
  }
}
