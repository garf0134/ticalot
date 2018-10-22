using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewMatchDialog : MonoBehaviour
{
  public GameObject matchPrefab;
  public CanvasGroup canvasGroup;
  public InputField rows;
  public InputField columns;
  public InputField matchN;
  public InputField gamesToWin;
  public InputField maxGames;

  public Ruleset r;
  public List<PlayerSettings> playerSettings = new List<PlayerSettings>();
  public List<WinConditionSetting> winConditions = new List<WinConditionSetting>();
  public List<MovementRuleSetting> movementRules = new List<MovementRuleSetting>();

  public delegate void NewMatchEvent(Match m);
  public event NewMatchEvent OnNewMatch;

  // Start is called before the first frame update
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

  // Update is called once per frame
  void Update()
  {
  }

  public void Show()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Show");

    Destroy(r?.m?.gameObject);
    r = null;
  }

  public void OnShowFinished()
  {
    r = ScriptableObject.CreateInstance<Ruleset>();
  }

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

  public void OnMatchStartClick()
  {
    Hide();
  }

  public void Hide()
  {
    Animator animator = GetComponent<Animator>();
    animator.SetTrigger("Hide");
  }

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
