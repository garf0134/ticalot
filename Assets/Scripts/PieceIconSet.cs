using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An asset to allow icons to be specified by the model's
/// resource path, camera position, light direction and color.
/// For later, it doesn't have to be just game pieces that
/// this describes.
/// </summary>
[CreateAssetMenu(menuName = "Piece Icon Set")]
public class PieceIconSet : ScriptableObject
{
  /// <summary>
  /// An entry where the model's resource path, camera position,
  /// light direction and color are specified.
  /// </summary>
  [System.Serializable]
  public class PieceIconEntry
  {
    /// <summary>The resource path to the resource containing the mesh</summary>
    public string modelName;
    /// <summary>
    /// The desired camera position. Camera direction will always
    /// point through the center of the model.
    /// </summary>
    public Vector3 cameraPosition;
    /// <summary>The direction of the light</summary>
    public Vector3 lightDirection;
    /// <summary>The color of the light</summary>
    public Color lightColor;
    /// <summary>The output texture result</summary>
    public Texture2D texture;
  }
  /// <summary>
  /// The list of all icon entris in this asset
  /// </summary>
  public PieceIconEntry[] iconEntries = new PieceIconEntry[0];
}
