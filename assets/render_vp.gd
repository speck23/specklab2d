@tool
extends SubViewport


@export var go : bool = false
@export var path : String = "res://assets/icons/icon.png"

func do():
	var img = get_texture().get_image()
	img.save_png(path)


func _process(delta: float) -> void:
	if go:
		do()
		go = false
