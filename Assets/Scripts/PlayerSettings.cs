using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PlayerSettings : MonoBehaviour
{
  public Sprite sprite;
  public Toggle sideEnabled;
  public Dropdown roleDropdown;
  public Dropdown iconDropdown;
  public Dropdown colorDropdown;

  private DropdownColorChoice[] selectableColors;
  private ColorSet defaultSideColors;

  // Start is called before the first frame update
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

    defaultSideColors = Resources.Load<ColorSet>("Default Side Colors");
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
  }

  public string SelectedName()
  {
    return colorDropdown.options[colorDropdown.value].text;
  }

  public Color SelectedColor()
  {
    return colorDropdown.options[colorDropdown.value].image.texture.GetPixel(0, 0);
  }

  public string SelectedIcon()
  {
    return iconDropdown.options[iconDropdown.value].text;
  }

  public Side.Role SelectedRole()
  {
    string roleText = roleDropdown.options[roleDropdown.value].text;
    foreach (var role in System.Enum.GetValues(typeof(Side.Role)))
    {
      if ( ((Side.Role)role).ToString() == roleText)
      {
        return (Side.Role)role;
      }
    }
    return Side.Role.Human;
  }

  // Update is called once per frame
  void Update()
  {

  }
}
