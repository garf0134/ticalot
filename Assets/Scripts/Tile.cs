using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Tile class provides an abstraction for the referential location of a tile on the game board
/// and it's state.
/// </summary>
public class Tile : MonoBehaviour
{
  public int row;
  public int column;
  /// <value>Who occupies the tile, or null if tile is unoccupied. </value>
  public Piece piece
  {
    get { return _piece; }
    set {
      _piece = value;
      OnPiecePlaced?.Invoke(this, value);
    }
  }
  private Piece _piece;

  public delegate void PiecePlacedEvent(Tile tile, Piece piece);
  public event PiecePlacedEvent OnPiecePlaced;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }

#if UNITY_EDITOR
  private Dictionary<string, string> registeredDebugStrings = new Dictionary<string, string>();
  public void RegisterDebug(string mode, string text)
  {
    registeredDebugStrings[mode] = text;
  }

  public string RegisteredDebugText(string mode)
  {
    string ret = null;
    registeredDebugStrings.TryGetValue(mode, out ret);
    return ret;
  }
#endif
}
