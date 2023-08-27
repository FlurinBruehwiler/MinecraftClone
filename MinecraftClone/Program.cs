using RayLib3dTest;

const int screenWidth = 1800;
const int screenHeight = 1000;

InitWindow(screenWidth, screenHeight, "3dtest");

DisableCursor();
SetTargetFPS(120);
SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);
// SetTraceLogLevel(TraceLogLevel.LOG_ERROR);

var textures = new Textures();
var merger = new ThinkTexture(textures);
merger.Merge();

var texture = LoadTexture("resources/textureatlas.png");

var globalBoy = new GlobalBoy(texture);

var colcol = new Colcol(globalBoy);
var sirPhysics = new SirPhysics(colcol);

var mrPerlin = new MrPerlin(0);
var debuggerus = new Debuggerus();
var chunker = new Chunker(globalBoy, textures, mrPerlin, debuggerus);
var player = new Player(sirPhysics, colcol, globalBoy, debuggerus);

var cameraManager = new CameraManager(player, chunker, debuggerus);

var gamus = new Gamus(cameraManager, chunker, cameraManager, debuggerus, player);

gamus.GameLoop();

CloseWindow();

