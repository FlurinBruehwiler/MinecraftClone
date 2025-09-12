using RayLib3dTest;

const int screenWidth = 1800;
const int screenHeight = 1000;

InitWindow(screenWidth, screenHeight, "3dtest");

DisableCursor();
SetTargetFPS(120);
SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);
// SetTraceLogLevel(TraceLogLevel.LOG_ERROR);

CurrentWorld = new World();

var player = new Player();

var cameraManager = new CameraManager(player);

var gamus = new Game(cameraManager);

gamus.GameLoop();

CloseWindow();

