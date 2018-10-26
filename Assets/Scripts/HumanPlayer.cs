using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : PlayerBase
{
  /// <summary>
  /// The concrete implementation starts the <see cref="Play(Board, Ruleset)"/>
  /// coroutine which just waits for the human player to click on a valid tile
  /// on the board.
  /// </summary>
  /// <param name="m">The current match being played</param>
  /// <param name="turn">The current turn</param>
  /// <param name="sides">The sides involved in the match. <code>sides[turn]</code> is the current side</param>
  protected override void OnTurnBegan(Match m, int turn, Side[] sides)
  {
    if (sides[turn] == side)
    {
      StartCoroutine(Play(m.board, m.ruleset));
    }
  }

  /// <summary>
  /// Waits for the player to click on a tile that is valid and then registers 
  /// a move with the board.
  /// </summary>
  /// <param name="b">The board where the current game is being played</param>
  /// <param name="r">The ruleset of the current game being played</param>
  /// <returns></returns>
  public override IEnumerator Play(Board b, Ruleset r)
  {
    while (true)
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (Physics.Raycast(ray, out hit))
      {
        Tile t = hit.collider.GetComponentInParent<Tile>();
        if (t != null)
        {
          hud.UpdateHover(t);

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
        hud.UpdateHover(null);
      }

      yield return null;
    }
  }
}
