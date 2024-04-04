extends TextureRect

@export var SPLITS_X : int = 32
@export var SPLITS_Y : int = 32
@export var COLOR : Color = Color.WEB_GRAY
@export var WIDTH = -1
var lines : PackedVector2Array = []

func create_lines():
	var dist_y = size.y/SPLITS_Y
	var dist_x = size.x/SPLITS_X
	
	for y in SPLITS_Y:
		lines.append(Vector2(0,      y*dist_y))
		lines.append(Vector2(size.x, y*dist_y))
	for x in SPLITS_X:
		lines.append(Vector2(x*dist_x,0))
		lines.append(Vector2(x*dist_x, size.y))

func _ready() -> void:
	create_lines()

func _draw() -> void:
	draw_multiline(lines, COLOR, WIDTH)
