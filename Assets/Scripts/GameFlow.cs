using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFlow : MonoBehaviour
{
  /// <summary>The current match managed by this flow</summary>
  public Match match;

  /// <summary>The HUD UI</summary>
  public HUD hud;

  /// <summary>The match object used as the template for all new matches</summary>
  public GameObject matchPrefab;


  #region LookAndFeel
  /// <summary>
  /// A designer variable for the color to be shown when the player
  /// hovers over a tile that is a legal move
  /// </summary>
  public Color tileHoverColor;

  /// <summary>
  /// A designer variable for the color to be shown when the player 
  /// hovers over a tile that is an illegal move
  /// </summary>
  public Color tileHoverIllegalMoveColor;

  /// <summary>A designer variable for the color to be shown when the tile is idle</summary>
  public Color tileNormalColor;

  /// <summary>
  /// A designer variable to denote the dimensions of a tile.
  /// Probably should calculate this from the mesh bounds of the prefab or refer to it
  /// as a tile interval since some gaps betwen tiles would look nice.
  /// </summary>
  public float tileDimensions;

  /// <summary>
  /// A designer variable to control how fast tiles will drop at the beginning
  /// of a game
  /// </summary>
  public float tileDropRate = 4.0f;

  /// <summary>
  /// A designer variable to control how quickly tiles are remove from the board
  /// at the end of a game
  /// </summary>
  public float tileDestroyRate = 10.0f;
  #endregion

  /// <summary>
  /// The list of players, one per side
  /// </summary>
  public List<PlayerBase> players = new List<PlayerBase>();

  #region MatchListeners
  /// <summary>
  /// A listener for the <see cref="Match.OnMatchBegan"/> event.
  /// Builds a list of <see cref="PlayerBase"/> instances (with associated objects)
  /// from information collected elsewhere in the form of a list of <see cref="Side"/>
  /// </summary>
  /// <param name="m">The match which the sides in <paramref name="sides"/>belong</param>
  /// <param name="sides">The list of sides who are competing in the match.</param>
  public void OnMatchBegan(Match m, Side[] sides)
  {
    foreach (Side s in sides)
    {
      GameObject playerObject = new GameObject(s.name);
      PlayerBase player = null;
      switch (s.role)
      {
        case Side.Role.Human:
          player = playerObject.AddComponent<HumanPlayer>();
          break;
        case Side.Role.AI:
          AIPlayer aiPlayer = playerObject.AddComponent<AIPlayer>();
          aiPlayer.strategy = s.aiStrategy;
          player = aiPlayer;
          break;
      }

      if (player == null)
      {
        throw new System.ArgumentException("Invalid role argument");
      }
      player.side = s;
      player.match = m;
      player.gameFlow = this;
      player.pieceResource = s.pieceResource;
      player.transform.SetParent(m.transform);
      OnBeginTurnPlay += player.OnBeginTurnPlay;
      players.Add(player);
    }

    Animator animator = GetComponent<Animator>();
    animator.SetBool("Match Won", false);
  }

  /// <summary>
  /// A listener for the <see cref="Match.OnMatchEnded"/> event
  /// Sets the "Match Won" flag to progress the game flow out of the match
  /// </summary>
  /// <param name="m">The match that ended</param>
  /// <param name="winner">The side that won the match</param>
  public void OnMatchEnded(Match m, Side winner)
  {
    Animator animator = GetComponent<Animator>();
    animator.SetBool("Match Won", true);
    foreach (PlayerBase player in players)
    {
      OnBeginTurnPlay -= player.OnBeginTurnPlay;
    }
  }

  /// <summary>
  /// A listener for the <see cref="Match.OnGameBegan"/> event
  /// Sets the "Game Began" flag to progress the game flow out of the
  /// turns
  /// </summary>
  /// <param name="m">The current match</param>
  /// <param name="game">The game that has begun</param>
  /// <param name="b">The board that the game will be played on</param>
  /// <param name="sides">The sides that are participating in the game</param>
  public void OnGameBegan(Match m, int game, Board b, Side[] sides)
  {
    Animator animator = GetComponent<Animator>();
    animator.SetBool("Game Ended", false);
  }

  /// <summary>
  /// A listener for the <see cref="Match.OnGameEnded"/> event
  /// Sets the "Game Ended" flag to progress the game flow out of the
  /// turns
  /// </summary>
  /// <param name="m">The current match</param>
  /// <param name="b">The board on which the game that just ended was played</param>
  /// <param name="winner">The winner of the game</param>
  public void OnGameEnded(Match m, Board b, Side winner)
  {
    Animator animator = GetComponent<Animator>();
    animator.SetBool("Game Ended", true);
  }
  #endregion
  #region Delegates and Events
  /// <summary>
  /// The delegate called when the GameFlow is ready for a player move
  /// </summary>
  /// <param name="m">The current match</param>
  /// <param name="s">The side who's turn it is</param>
  public delegate void BeginTurnPlayCallback(Match m, Side s);
  /// <summary>
  /// The event to signal that the GameFlow is ready for a player move
  /// </summary>
  public event BeginTurnPlayCallback OnBeginTurnPlay;
  /// <summary>
  /// Called by <see cref="WaitForPlayerBehavior"/> when that state is entered
  /// </summary>
  public void BeginTurnPlay()
  {
    OnBeginTurnPlay?.Invoke(match, match.turnOrder[match.turn]);
  }
  #endregion
}
