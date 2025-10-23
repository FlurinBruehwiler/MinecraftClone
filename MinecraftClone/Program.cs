using System.Diagnostics;
using MinecraftClone;


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

Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);    // Window configuration flags
// SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

Raylib.InitWindow(screenWidth, screenHeight, "3dtest");

Raylib.DisableCursor();

//Raylib.SetTargetFPS(120);
// Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint);
Raylib.SetExitKey(KeyboardKey.F11);
// RaylibSetTraceLogLevel(TraceLogLevel.LOG_ERROR);


AssetLoader.LoadAssets();

var player = new Player();

var game = new Game(player);

CurrentWorld = new World(game);

game.GameLoop();

Raylib.CloseWindow();

