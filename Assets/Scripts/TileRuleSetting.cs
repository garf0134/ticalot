using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Appends information about the tile resource path to a dropdown option
/// data class
/// </summary>
[System.Serializable]
public class TileRuleSetting : Dropdown.OptionData
{
  /// <summary> The path to the tile resource for this option in the dropdown </summary>
  public string tileResource;
}
