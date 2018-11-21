using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Clears tiles of the board after a game is won or tied
/// </summary>
public class ClearTilesBehavior : StateMachineBehaviour
{
  /// <summary>The time left until another tile is cleared</summary>
  private float clearTileCounter = 0;
  /// <summary>The list of tiles that exist at the beginning when this behavior started</summary>
  private Tile[] tiles = null;
  /// <summary>The index into the <see cref="tiles"/> list</summary>
  private int tileToCleanup = 0;

  /// <summary>
  /// The callback for a StateMachineBehaviour's start event
  /// Gets the list of tiles and sets the tile index to 0
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    tiles = gameFlow.match.board.GetComponentsInChildren<Tile>();
    tileToCleanup = 0;
    
  }

  /// <summary>
  /// The callback for a StateMachineBehaviour's Update event
  /// At a fixed rate, one tile at a time is cleared of any pieces. When all tiles
  /// have been cleared the "Cleared Tiles" trigger is set moving the game flow
  /// forward.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateUpdate(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();

    if (tileToCleanup >= tiles.Length)
    {
      //Debug.LogFormat("Tiles to check : {0}", tiles.Length - tileToCleanup);
      animator.SetTrigger("Cleared Tiles");
      return;
    }

    clearTileCounter -= Time.deltaTime;
    if (clearTileCounter > 0)
    {
      return;
    }

    Tile t = tiles[tileToCleanup++];
    clearTileCounter = 1/gameFlow.tileDestroyRate;
    if (t.piece)
    {
      Destroy(t.piece.gameObject);
      t.piece = null;
    }
  }
}
