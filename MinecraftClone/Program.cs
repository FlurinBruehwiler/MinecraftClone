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

CurrentWorld = new World(texture);

var chunker = new Chunkloader(textures);
var player = new Player();

var cameraManager = new CameraManager(player, chunker);

var gamus = new Game(cameraManager);

gamus.GameLoop();

CloseWindow();

