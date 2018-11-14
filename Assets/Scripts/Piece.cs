using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A visual piece used on the game board. It is owned by a specific <see cref="Side"/>
/// </summary>
public class Piece : MonoBehaviour
{
  /// <summary>
  /// The side that the piece belongs to. Set at instantiation.
  /// </summary>
  /// <value>the side that the piece belongs to</value>
  public Side side { get; set; }
  /// <summary>
  /// The orientation that the piece is designed for
  /// </summary>
  public Tile.TileOrientation orientation;

  /// <summary>
  /// The rigidbody attached to gameObject
  /// </summary>
  private new Rigidbody rigidbody;

  /// <summary>
  /// A delegate for when a piece has finished placing.
  /// </summary>
  public delegate void PieceFinishedPlacing(Piece piece);
  /// <summary>
  /// An event that fires when a piece has finished all movement.
  /// </summary>
  public event PieceFinishedPlacing OnPieceFinishedPlacing;

  /// <summary>
  /// The Unity3d hook for initialization
  /// </summary>
  private void Start()
  {
    rigidbody = GetComponent<Rigidbody>();
  }

  /// <summary>
  /// The Unity3D hook for notification when collided objects stay 
  /// collided with this piece.
  /// </summary>
  /// <param name="collision"></param>
  private void OnCollisionStay(Collision collision)
  {
    Board board = collision.gameObject.GetComponent<Board>();
    Piece piece = collision.gameObject.GetComponent<Piece>();

    if (board != null || piece != null)
    {
      rigidbody.Sleep();
    }
  }

  /// <summary>
  /// The Unity3D hook for per-frame updates
  /// </summary>
  private void Update()
  {
    if (rigidbody.IsSleeping() && rigidbody.isKinematic == false)
    {
      OnPieceFinishedPlacing?.Invoke(this);
      rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
      rigidbody.isKinematic = true;
    }
  }
}
