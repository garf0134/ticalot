using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Piece Icon Set")]
public class PieceIconSet : ScriptableObject
{
  [System.Serializable]
  public class PieceIconEntry
  {
    public string modelName;
    public Vector3 cameraPosition;
    public Vector3 lightDirection;
    public Color lightColor;
    public Texture2D texture;
  }

  public PieceIconEntry[] iconEntries = new PieceIconEntry[0];
}
