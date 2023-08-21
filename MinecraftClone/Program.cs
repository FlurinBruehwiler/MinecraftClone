using RayLib3dTest;

const int screenWidth = 1800;
const int screenHeight = 1000;

InitWindow(screenWidth, screenHeight, "3dtest");

DisableCursor();
SetTargetFPS(120);
SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

var blocks = new Blocks();
var textures = new Textures();
var merger = new ThinkTexture(textures);
merger.Merge();

var texture = LoadTexture("resources/textureatlas.png");

var globalBoy = new GlobalBoy(texture);

var colcol = new Colcol(globalBoy);
var sirPhysics = new SirPhysics(colcol);

var mrPerlin = new MrPerlin(0);

var chunker = new Chunker(globalBoy, textures, mrPerlin);
var player = new Player(sirPhysics, colcol, globalBoy);
var cameraManager = new CameraManager(player);

var gamus = new Gamus();

gamus.RegisterDraw2d(new Debuggerus());

gamus.RegisterDraw3d(chunker);

gamus.re

gamus.GameLoop();

CloseWindow();

