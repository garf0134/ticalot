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
  public Piece piece;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {

  }
}
