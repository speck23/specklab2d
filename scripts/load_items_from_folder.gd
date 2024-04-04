extends Container

@export var item_folder = "res://serialized/bodies/"
@export var SKIP_DEFAULT = 0
@export var EXCLUSIVE_LIST : PackedStringArray 
var item_prefab = preload("res://prefabs/ui/item_icon_prefab.tscn")

func load_exclusive_list():
	for file_name_base in EXCLUSIVE_LIST:
		var ob = item_prefab.instantiate()
		ob.editor_description = item_folder+""+file_name_base+".pb"
		ob.icon = load(item_folder+""+file_name_base+".png")
		if ob.has_node("name_label"):
			ob.get_node("name_label").text = file_name_base+".pb"
		add_child(ob)
		print("setting icon from path: ", item_folder+""+file_name_base)

func load_folder_bodies():
	var i=0
	for c in get_children():
		if i < SKIP_DEFAULT: continue
		i+=1
		remove_child(c)
		c.queue_free()
	

	
	var dir = DirAccess.open(item_folder)
	if dir:
		dir.list_dir_begin()
		var file_name = dir.get_next()
		while file_name != "":
			if dir.current_is_dir():
				print("skipping dir for now " + file_name)
			else:
				print("found file" + file_name)
	
				#
				if file_name.ends_with(".png"):
					var ob = item_prefab.instantiate()
					ob.editor_description = item_folder+""+file_name.get_basename()+".pb"
					ob.icon = load(item_folder+""+file_name)
					if ob.has_node("name_label"):
						ob.get_node("name_label").text = file_name
					add_child(ob)
					print("setting icon from path: ", item_folder+""+file_name)
				#
			file_name = dir.get_next()
	else:
		print("couldnt open bodies directory")
	
func _ready() -> void:
	if len(EXCLUSIVE_LIST) > 0:
		load_exclusive_list()
	else:
		load_folder_bodies()
