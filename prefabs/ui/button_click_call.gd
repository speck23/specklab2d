extends Button


@export var call_nodes = ["/root/base/mouse/mouse_manager", "/root/base/mouse/mouse_manager"]

@export var call_methods : PackedStringArray  = ["set_mode", "set_prefab_path"]

@export var argument_nodes  : PackedStringArray = ["val", ""]
@export var argument_names  : PackedStringArray = ["spawn_creative","editor_description"]

func go():
	print("<button_click_call.gd>.go()")
	for i in len(call_nodes):
		var arg
		if argument_nodes[i] == "":
			arg = get(argument_names[i])
		else:
			if argument_nodes[i] == "val":
				arg = argument_names[i]
			elif argument_nodes[i] == ".":
				arg = get(argument_names[i])
			else:
				var n = get_node(argument_nodes[i])
				if n == null:
					continue
				arg = n.get(argument_names[i])
				
					
		if call_nodes[i] == "input":
			Input.call(call_methods[i], arg)

		else:
			var n = get_node(call_nodes[i])
			if n != null:
				n.call(call_methods[i], arg)
	
func _ready() -> void:
	owner = get_tree().get_root()
	button_up.connect(go)
