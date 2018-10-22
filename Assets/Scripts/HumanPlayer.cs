using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : PlayerBase
{
  // Start is called before the first frame update
  protected override void Start()
  {

  }

  // Update is called once per frame
  protected override void Update()
  {

  }

  protected override void OnTurnBegan(Match m, int turn, Side[] sides)
  {
    if (sides[turn] == side)
    {
      StartCoroutine(Play(m.board, m.ruleset));
    }
  }

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
