@tool
extends Node2D
@export var AUTO_UPDATE = false

@export var SHOW_POINT_NUMBERS = false

@export var max_hits : int = 2

@export var movable_points : int = 0
@export var collision_points : int = 0
@export var base_draw_color : Color = Color8(172, 172, 172, 255)
@export var segments : PackedVector2Array = []
@export var collision_segments : int = -1
@export var springs : PackedVector3Array
@export var pin_springs_ab : PackedVector2Array
@export var pin_springs_os : PackedVector3Array

@export var point_motors : PackedVector3Array = []
@export var draw_list : PackedVector3Array = []

@export var PREGEN_CHAINED = false
@export var font : Font = SystemFont.new()

@export var serialize : bool = false
@export var serialize_png : bool = false

func _ready() -> void:
	pass # Replace with function body.


func _process(delta: float) -> void:
	if AUTO_UPDATE:
		queue_redraw()

func _draw() -> void:
	var p = get_node("p")
	var ps = p.scale
	# *** SPRINGS ***
	if springs != null:
		for s in springs:
			draw_line(ps*p.get_child(s.x).position, ps*p.get_child(s.y).position, Color.BLUE_VIOLET, 2.0)
	# *** PIN SPRINGS ***
	if pin_springs_ab != null && pin_springs_os != null:
		if len(pin_springs_ab) == len(pin_springs_os):
			for s in len(pin_springs_ab):
				draw_line(ps*p.get_child(pin_springs_ab[s].x).position, Vector2(pin_springs_os[s].x, pin_springs_os[s].y), Color.CHARTREUSE, 3.0)
				draw_line(ps*p.get_child(pin_springs_ab[s].y).position, Vector2(pin_springs_os[s].x, pin_springs_os[s].y), Color.CHARTREUSE, 1.5)
	# *** SEGMENTS ***
	if segments != null:
		for s in segments:
			draw_line(ps*p.get_child(s.x).position, ps*p.get_child(s.y).position, base_draw_color, 8)
	# *** POINTS ***
	var ni = 0
	for n in p.get_children():
		draw_circle(ps*n.position, 4.0, Color.BROWN)
		if SHOW_POINT_NUMBERS:
			draw_string(font, ps*n.position, str(ni) ) 
			ni += 1
	
	if PREGEN_CHAINED == true:
		var p_len = p.get_child_count()
		segments.clear()
		springs.clear()
		
		for j in p_len-1:
			segments.append( Vector2(j, j+1) )
			springs.append( Vector3(j, j+1, 1.0) )
		
		PREGEN_CHAINED = false
	
	if serialize == true:
		serialize = false
		var BTS = load("res://scripts/physics/BodyTools.cs")

		var b = BTS.call("body_create_from_preview_node",self)
		BTS.body_serialize(b, "res://serialized/bodies/"+name+".pb", 0)
	#
	if collision_segments == -1:
		collision_segments = len( segments )
	
	
