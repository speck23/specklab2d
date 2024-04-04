extends Node2D


func _ready() -> void:
	pass

func _process(d : float) -> void:
	pass


@export var MODE = "spawn_creative"
@export var PREFAB_PATH = "spawn"

func set_mode(m):
	print("<mouse_manager.gd>.set_mode()", m)
	MODE = m
func set_prefab_path(pp):
	print("<mouse_manager.gd>.set_prefab_path()", pp)
	PREFAB_PATH = pp

func spawn_creative(e : InputEvent):
	if e is InputEventMouseButton:
		if e.button_index == 1:
			if e.is_released():
				print("click 1")
				
				get_node("%scene").spawn_from_path(PREFAB_PATH, get_global_mouse_position())

func _input(e : InputEvent) -> void:
	if MODE != null:
		if has_method(MODE):
			call(MODE, e)

