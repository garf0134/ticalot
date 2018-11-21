using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A ColorSet is a sequence of color,name associations for use by
/// code that handles team colors.
/// <seealso cref="CreateAssetMenuAttribute"/>You can create an asset
/// of this type by finding the Create menu and selecting "ColorSet".
/// </summary>
[CreateAssetMenu(menuName ="ColorSet")]
public class ColorSet : ScriptableObject
{
  /// <summary>
  /// ColorEntry is the association between color name and definition
  /// </summary>
  [System.Serializable]
  public struct ColorEntry
  {
    /// <summary>
    /// The name of the color
    /// </summary>
    public string name;

    /// <summary>
    /// The color definition
    /// </summary>
    public Color color;
  }

  /// <summary>
  /// A list of colors, the length of which is controlled by the
  /// Unity Editor
  /// </summary>
  public ColorEntry[] colors;
}
