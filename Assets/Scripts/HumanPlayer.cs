using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : PlayerBase
{
  /// <summary>
  /// Waits for the player to click on a tile that is valid and then registers 
  /// a move with the board.
  /// </summary>
  /// <param name="b">The board where the current game is being played</param>
  /// <param name="r">The ruleset of the current game being played</param>
  /// <returns></returns>
  public override IEnumerator Play(Board b, Ruleset r)
  {
    // Wait one frame for the other OnTurnBegan listeners to be called
    yield return null;

    while (true)
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit))
      {
        Tile t = hit.collider.GetComponentInParent<Tile>();
        if (t != null)
        {
          gameFlow.hud.UpdateHover(t);

          if (Input.GetMouseButtonDown(0))
          {
            Piece piece = CreatePiece();
            if (match.RegisterMove(side, t, piece))
            {
              piece.transform.SetParent(t.transform);
              piece.transform.localPosition = Vector3.zero;

              yield break;
            }
            else
            {
              DestroyImmediate(piece.gameObject);
            }
          }
        }
      }
      else
      {
        gameFlow.hud.UpdateHover(null);
      }

      yield return null;
    }
  }
}
