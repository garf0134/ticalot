using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="ColorSet")]
public class ColorSet : ScriptableObject
{
  [System.Serializable]
  public struct ColorEntry
  {
    public string name;
    public Color color;
  }

  public ColorEntry[] colors;
}
