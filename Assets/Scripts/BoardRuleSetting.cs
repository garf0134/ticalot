using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to help associate a dropdown board option with the 
/// board resource
/// </summary>
[System.Serializable]
public class BoardRuleSetting : Dropdown.OptionData
{
  /// <summary>The resource referene for the board</summary>
  public string boardResource;
}
