using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// The interface to adjust side/player specific settings like AI/Human roles,
/// icons, colors. Accessors for these values are are provided to the
/// calling environment as a convenience.
/// </summary>
public class PlayerSettings : MonoBehaviour
{
  /// <summary> The UI reference to the asset used for the icon </summary>
  public Sprite sprite;
  /// <summary> A toggle to enable/disable the side from participation in the match </summary>
  public Toggle sideEnabled;
  /// <summary> A dropdown to select the Human, Random AI, Normal AI role for the side </summary>
  public Dropdown roleDropdown;
  /// <summary> A dropdown to select the icon to be used for the side during the match</summary>
  public Dropdown iconDropdown;
  /// <summary> A dropdown to select the color to be used for the side during the match </summary>
  public Dropdown colorDropdown;
  /// <summary> A dropdown to select the piece to be used for the side during the match </summary>
  public Dropdown pieceDropdown;

  /// <summary> A set of colors read in from the Resources folder </summary>
  private ColorSet defaultSideColors;
  /// <summary> A set of pieces read in from the Resources folder</summary>
  private PieceIconSet defaultPieces;
  /// <summary> The list of pieces filtered by board type</summary>
  private PieceIconSet.PieceIconEntry[] filteredPieces;

  /// <summary> Start is called before the first frame update </summary>
  void Start()
  {
    iconDropdown.ClearOptions();
    List<Dropdown.OptionData> iconOptions = new List<Dropdown.OptionData>();

    foreach (var sprite in Resources.LoadAll<Sprite>("Sides"))
    {
      Dropdown.OptionData data = new Dropdown.OptionData();
      data.image = sprite;
      data.text = sprite.name;
      iconOptions.Add(data);
    }

    iconDropdown.AddOptions(iconOptions);

    defaultSideColors = Instantiate<ColorSet>(Resources.Load<ColorSet>("Default Side Colors"));
    colorDropdown.ClearOptions();
    List<Dropdown.OptionData> colorOptions = new List<Dropdown.OptionData>();
    foreach (var color in defaultSideColors.colors)
    {
      Dropdown.OptionData data = new Dropdown.OptionData();
      Texture2D texture = new Texture2D(1, 1);
      texture.SetPixel(0, 0, color.color);
      texture.Apply();

      Sprite s = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0, 0));
      data.image = s;
      data.text = color.name;
      colorOptions.Add(data);
    }
    colorDropdown.AddOptions(colorOptions);

    defaultPieces = Instantiate<PieceIconSet>(Resources.Load<PieceIconSet>("Default Pieces"));
    filteredPieces = defaultPieces.iconEntries.ToArray();

    colorDropdown.onValueChanged.AddListener(index => {
      RebuildGamePieceOptions(SelectedColor());
    });
  }

  /// <summary>
  /// </summary>
  public void OnBoardConfigurationChanged(BoardConfigurationSet.BoardConfiguration boardConfiguration)
  {
    filteredPieces = defaultPieces.iconEntries.Where(iconEntry => {
      GameObject prefab = Resources.Load<GameObject>(iconEntry.modelName);
      Piece piece = prefab.GetComponentInChildren<Piece>();
      return boardConfiguration.tileOrientation == piece.orientation;
    }).ToArray();
    RebuildGamePieceOptions(SelectedColor());
  }
  /// <summary>
  /// Creates new textures, assigns them to new sprites used by a new
  /// list of game piece options and then defers to <see cref="IconRenderer.RenderIcons(PieceIconSet.PieceIconEntry[], Color)"/>
  /// to build out thumbnails of the game pieces.
  /// </summary>
  /// <param name="color">The color that each game piece should be rendered with</param>
  private void RebuildGamePieceOptions( Color color)
  {
    pieceDropdown.ClearOptions();
    List<Dropdown.OptionData> piecesOptions = new List<Dropdown.OptionData>();

    GameObject iconCameraObject = GameObject.Find("Icon Camera");
    IconRenderer iconRenderer = iconCameraObject.GetComponent<IconRenderer>();
    iconRenderer.RenderIcons(filteredPieces, color);

    foreach (var piece in filteredPieces)
    {
      Sprite s = Sprite.Create(piece.texture, new Rect(0, 0, 256, 256), Vector2.zero);
      s.name = piece.modelName;

      Dropdown.OptionData data = new Dropdown.OptionData()
      {
        image = s,
        text = piece.modelName
      };
      piecesOptions.Add(data);
    }
    pieceDropdown.AddOptions(piecesOptions);
    if (piecesOptions.Count  > 0)
    {
      pieceDropdown.value = 0;
    }
  }

  /// <summary>
  /// Gets the color name of the currently selected color
  /// </summary>
  /// <returns>The currently selected name of the side</returns>
  public string SelectedName()
  {
    return colorDropdown.options[colorDropdown.value].text;
  }

  /// <summary>
  /// Gets the Color instance for the currently selected color
  /// </summary>
  /// <returns>The Color instance for the currently selected color</returns>
  public Color SelectedColor()
  {
    return colorDropdown.options[colorDropdown.value].image.texture.GetPixel(0, 0);
  }

  /// <summary>
  /// Gets the resource reference for the currently selected icon
  /// </summary>
  /// <returns>The resource reference for the currently selected icon</returns>
  public string SelectedIcon()
  {
    return iconDropdown.options[iconDropdown.value].text;
  }

  /// <summary>
  /// Gets the resource reference for the selected game piece model
  /// </summary>
  /// <returns>The resource reference to the game piece model for this side</returns>
  public string SelectedPieceResource()
  {
    if (pieceDropdown.options.Count > 0)
    {
      return pieceDropdown.options[pieceDropdown.value].text;
    }
    Debug.LogWarning("Did not have any options to choose from, returning null");
    return null;
  }

  /// <summary>
  /// Gets the currently selected role
  /// </summary>
  /// <returns>The currently selected role</returns>
  public Side.Role SelectedRole()
  {
    string roleText = roleDropdown.options[roleDropdown.value].text;
    foreach (var role in System.Enum.GetValues(typeof(Side.Role)))
    {
      if (roleText.IndexOf(((Side.Role)role).ToString()) >= 0)
      {
        return (Side.Role)role;
      }
    }
    return Side.Role.Human;
  }

  /// <summary>
  /// Gets the currently selected  AI strategy
  /// </summary>
  /// <returns>Returns the currently selected AI strategy</returns>
  public AIPlayer.Strategy SelectedAIRoleStrategy()
  {
    string roleText = roleDropdown.options[roleDropdown.value].text;
    foreach (var strategy in System.Enum.GetValues(typeof(AIPlayer.Strategy)))
    {
      if (roleText.IndexOf(((AIPlayer.Strategy)strategy).ToString()) >= 0)
      {
        return (AIPlayer.Strategy)strategy;
      }
    }
    return AIPlayer.Strategy.Normal;
  }
}
