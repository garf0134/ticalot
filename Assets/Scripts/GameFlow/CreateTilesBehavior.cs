using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates tiles in a grid pattern according to the rule's definition for
/// rows and columns
/// </summary>
public class CreateTilesBehavior : StateMachineBehaviour
{
  /// <summary>
  /// Creates a new tile when the timer is decremented below zero
  /// </summary>
  private float tileDropTimer = 0.0f;
  /// <summary>The row where the new Tile will be created</summary>
  private int row = 0;
  /// <summary>The column where the new Tile will be created</summary>
  private int col = 0;
  /// <summary>The tile prefab</summary>
  GameObject tilePrefab;

  /// <summary>
  /// The callback for a StateMachineBehaviour's start event
  /// Initializes the row, column and timer to their starting values
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateEnter(animator, stateInfo, layerIndex);
    row = 0;
    col = 0;
    tileDropTimer = 0.0f;
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    tilePrefab = Resources.Load<GameObject>(gameFlow.match.ruleset.tileResource);
  }

  /// <summary>
  /// A callback for a StateMachineBehaviour's update event
  /// Creates a tile one at a time at intervals defined by <see cref="GameFlow.tileDropRate"/> for
  /// every row and column defined in the current match's ruleset.
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateUpdate(animator, stateInfo, layerIndex);
    tileDropTimer -= Time.deltaTime;
    GameFlow gameFlow = animator.GetComponent<GameFlow>();

    if (tileDropTimer > 0.0f || row >= gameFlow.match.ruleset.rows )
    {
      return;
    }


    GameObject tileInstance = Instantiate<GameObject>(tilePrefab);
    Tile t = tileInstance.GetComponent<Tile>();
    t.name = string.Format("{0},{1}", row, col);
    t.row = row;
    t.column = col;
    Material m = tileInstance.GetComponentInChildren<MeshRenderer>().material;
    m.color = gameFlow.tileNormalColor;

    Vector3 finalBoardPosition = Vector3.right * (col - gameFlow.match.ruleset.cols / 2.0f) * gameFlow.tileDimensions + Vector3.forward * ((gameFlow.match.ruleset.rows - row - 1) - gameFlow.match.ruleset.rows / 2.0f) * gameFlow.tileDimensions;
    t.transform.SetParent(gameFlow.match.board.transform);
    t.transform.position = gameFlow.match.board.transform.position + finalBoardPosition + Vector3.up * 10.0f;

    tileDropTimer = 1 / gameFlow.tileDropRate;
    col++;
    if (col >= gameFlow.match.ruleset.cols)
    {
      col = 0;
      row++;
      if ( row == gameFlow.match.ruleset.rows)
      {
        animator.SetTrigger("Tiles Placed");
      }
    }
  }

  /// <summary>
  /// The callback for a StateMachineBehaviour's exit event
  /// Makes sure that the <see cref="Board"/> instances learns about each <see cref="Tile"/> instance
  /// that was created. 
  /// </summary>
  /// <param name="animator">The animator</param>
  /// <param name="stateInfo">The current state info</param>
  /// <param name="layerIndex">The current layer</param>
  public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    base.OnStateExit(animator, stateInfo, layerIndex);
    GameFlow gameFlow = animator.GetComponent<GameFlow>();
    gameFlow.match.board.ScanForTiles();
  }
}
