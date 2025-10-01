using System.Diagnostics;
using RayLib3dTest;


/*
 * Ideas (TODOs)
 *
 * # FBR
 * - Improve Save/Load
 * - Multiplayer
 * - Improved World Gen
 * - Entity Animations
 *
 *
 * # AKA:
 * - Hotbar
 * - Inventory
 * - Command Line
 *
 */

const int screenWidth = 1200;
const int screenHeight = 800;

SetConfigFlags(ConfigFlags.ResizableWindow);    // Window configuration flags
// SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

InitWindow(screenWidth, screenHeight, "3dtest");

if (!Debugger.IsAttached)
    DisableCursor();

SetTargetFPS(120);
SetConfigFlags(ConfigFlags.Msaa4xHint);
SetExitKey(KeyboardKey.Back);
// SetTraceLogLevel(TraceLogLevel.LOG_ERROR);



var player = new Player();

var game = new Game(player);

CurrentWorld = new World(game);

game.GameLoop();

CloseWindow();

