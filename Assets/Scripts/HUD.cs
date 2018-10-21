using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
  public CanvasGroup canvasFader;

  public NewMatchDialog newMatch;
  public MatchHUD matchHUD;
  public GameObject piecePrefab;
  public GameObject tilePrefab;
  public Color tileHoverColor;
  public Color tileNormalColor;
  public float tileDimensions;
  public float tileDropRate;

  private Match match;
  private bool awaitingPlayerInput;
  private Tile hovered;

  private Coroutine tileSetup;

  // Start is called before the first frame update
  void Start()
  {
    newMatch.OnNewMatch += OnNewMatch;
    newMatch.OnNewMatch += matchHUD.OnNewMatch;

    canvasFader.alpha = 1;
  }

  public void OnShowFinished()
  {
    canvasFader.gameObject.SetActive(false);
    newMatch.Show();
  }

  IEnumerator SetupTiles(Board b)
  {
    for (int r = 0; r < b.rows; r++)
    {
      for (int c = 0; c < b.cols; c++ )
      {
        GameObject tileInstance = Instantiate<GameObject>(tilePrefab);
        Tile t = tileInstance.GetComponent<Tile>();
        Material m = tileInstance.GetComponent<Material>();
        m.color = tileNormalColor;

        Vector3 finalBoardPosition = Vector3.right * (c - b.cols / 2) * tileDimensions + Vector3.forward * (r - b.rows / 2) * tileDimensions;
        t.transform.SetParent(b.transform);
        t.transform.localPosition = finalBoardPosition;

        yield return new WaitForSecondsRealtime(1 / tileDropRate);
      }

      b.ScanForTiles();
      match.BeginMatch();
    }
  }

  private void OnNewMatch(Match m)
  {
    match = m;
    tileSetup = StartCoroutine(SetupTiles(m.board));
  }

  private void OnTurnBegan(Match m, int turn, Side[] sides)
  {
    if ( sides[turn].role == Side.Role.Human )
    {
      awaitingPlayerInput = true;
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (match != null && tileSetup != null)
    {
      return;
    }

    if (awaitingPlayerInput)
    {
      Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
      RaycastHit hit;
      if (Physics.Raycast(r, out hit))
      {
        Tile t = hit.collider.GetComponentInParent<Tile>();
        if (t != null)
        {
          if (hovered != null)
          {
            Material previousHoveredTileMaterial = hovered.GetComponentInChildren<Material>();
            previousHoveredTileMaterial.color = tileNormalColor;
            hovered = null;
          }

          Material m = t.GetComponentInChildren<Material>();
          m.color = tileHoverColor;
          hovered = t;

          if (Input.GetMouseButtonDown(0))
          {
            GameObject pieceInstance = Instantiate<GameObject>(piecePrefab);
            Piece piece = pieceInstance.GetComponent<Piece>();
            if (match.RegisterMove(match.turnOrder[match.turn], t, piece))
            {
              piece.transform.SetParent(t.transform);
              piece.transform.localPosition = Vector3.zero;
            }
            else
            {
              DestroyImmediate(pieceInstance);
              pieceInstance = null;
            }
          }
        }
      }
      else
      {
        Material m = hovered.GetComponentInChildren<Material>();
        m.color = tileNormalColor;
        hovered = null;
      }
    }
  }
}
