# Tic-Attack
Tic-Attack is a game engine for games played on one or more 2-dimensional boards partitioned into tiles.
  - Written in Unity 2018
  - Showcasing general project organization
  - The engine supports multiple modes for win conditions and valid movements
  - A general match-N AI is included
  - A visual per-tile console for communicating internal state of a tile is included in editor mode
  - A match-based game flow is supported
  - Player customization
  - A dialog to configure win conditions, valid movements, player customizations and board size is provided
  
## Authors
  - Joe Garfield
---
## Designers
Not much to describe here yet. There are public fields in the `Hud` class, visible from the scene, that control tile drop rate at the beginning of a match, and the piece-clearing rate at the end of a game. The `Assets/Resources/Default Side Colors.asset` (editable only within the Unity Editor) controls what range of colors are available for teams/sides.

Most of the design knobs are actually in the `New Match Dialog` class, parented to the `Hud` element in the scene. No mode exists currently to bake these in *and* skip the `New Match Dialog` in the game flow.

## Artists
There are prefabs for the board, tiles and game pieces that represent choices made by a player. You can find these in `Assets/Prefabs`. The choice of prefab for the piece belonging to team/side is not yet supported. 
  * Pieces
  
    The origin for pieces should always be centered on the x/z axii and be flush with the the minimum extents on the y axis
  * Tiles
  
    Tiles are necessarily rectangular in shape. The origins are centered on the x/z axii and the origin lies at the maximum extent on the y axis. Tiles need to have colliders and rigidbodies at the top level of the prefab in order to interact with the game board at the beginning of a match. Tiles need to have perfectly flat bottom surfaces.
  * Boards
  
    No real rule for boards exists yet except that there needs to be some thickness on the y-axis so that the pieces don't fall through. The board also needs to be perfectly flat on top where the tiles will fall to.
  * Animations
  
    In an effort to make things artist-driven from the start, Tic-Attack has animations for the show/hide cycle for the `New Match Dialog`, the `Match HUD`, the title UI at the start of a new session, and the overall `HUD` at the beginning of the game. Find the aforementioned game objects in the Scene, switch to the Animator and Animation tabs to view/edit these animations.
    
## Programmers
The code is organized into file/classes split between UI and gameplay code.
  * UI
  
    This principle class `HUD` is responsible for the majority of the game flow. This is handled through co-routines which at times defer to the gameplay code for decisions about whether a valid move has been made, whether the turn/game/match is over, etc... `HUD` oversees two other important UI elements and their controllers, `New Match Dialog` and `Match HUD`. `Match HUD` is a set of descriptors of the current state of the match/game/turn. `New Match Dialog` is responsible for setting up a match, it's board, the sides/teams involved and the ruleset.
  * Gameplay
  
    Gameplay code is comprised of classes for `Match`, `Side`, `PlayerBase`, `Board`, `Tile`, `Piece` and `Ruleset`. `Match` acts as the central dispatcher for all match/game/turn-related events. `Board`,`Tile`,`Piece`,`Ruleset` are abstractions of the Model concept. `PlayerBase`, `HumanPlayer`, `AIPlayer` initiate changes to the game flow by registering moves with the `Match` class to end each turn.
    * The AI code has two strategies, random and normal. 
    
      Normal strategy can play a human to a stalemate in a 3x3 Match-3, Place Anywhere scenario (Tic-Tac-Toe). It works by keeping track of how many consecutive unclaimed and claimed tiles there are in any given direction for each tile on the board. It also tracks similar state for the opposing player. It uses this information to build a score for each tile. The resultant move comes from the highest scoring tile. The score is based on the sum built from a number of observations that human players would make from the data colllected for consecutive claimed/unclaimed tiles.
    
    * Considerations for the future, 
      * A way to visualize the game flow in one place would be preferable to tracking down the different parts of it that are scattered around the `HUD` and the `Match` code.
      * Otherwise things look stable enough to add more win conditions, valid movement choices

## Continuous Integration
No automation of builds, testing or deployment exists yet but they are planned for the future. Research will have to be done to explore what CI options there are/how much it would cost to add support for the options that make the most sense.

## Attributions
 * Some icons were selected from https://www.flaticon.com/packs/video-games-3 designed by Webalys from Flaticon
    
