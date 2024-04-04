extends Control

@export var SCENE : Node
@export var play_stop_btn : Button


@export var play_scene_icon : Texture2D
@export var stop_scene_icon : Texture2D

@export var gravity_btn : Button
@export var gravity_x_icon : Texture2D
@export var gravity_down_icon : Texture2D
@export var gravity_center_icon : Texture2D
@export var gravity_multi_icon : Texture2D

func scene_play_stop():
	if SCENE.autostep == true:
		SCENE.autostep = false
		play_stop_btn.icon = play_scene_icon
	else:
		SCENE.autostep = true
		play_stop_btn.icon = stop_scene_icon

func scene_play():
	SCENE.autostep = true
func scene_stop():
	SCENE.autostep = false

func toggle_gravity():
	if SCENE.GRAVITY_MODE == 0:
		SCENE.GRAVITY_MODE = 1
		gravity_btn.icon = gravity_down_icon
	elif SCENE.GRAVITY_MODE == 1:
		SCENE.GRAVITY_MODE = 2
		gravity_btn.icon = gravity_center_icon
	elif SCENE.GRAVITY_MODE == 2:
		SCENE.GRAVITY_MODE = 3
		gravity_btn.icon = gravity_multi_icon
	elif SCENE.GRAVITY_MODE == 3:
		SCENE.GRAVITY_MODE = 0
		gravity_btn.icon = gravity_x_icon
func _ready() -> void:
	play_stop_btn.button_up.connect(scene_play_stop)
	gravity_btn.button_up.connect(toggle_gravity)

func _input(e : InputEvent) -> void:
	if e is InputEventKey:
		if e.key_label == KEY_G and e.is_released():
			toggle_gravity()
	
	if e.is_action("ui_accept") and e.is_released():
		print("enter?!")
		scene_play_stop()
	if e.is_action("ui_focus_next") and e.is_released():
		SCENE.PhysicsPass()
		SCENE.queue_redraw()
	
func _process(delta: float) -> void:
	pass
