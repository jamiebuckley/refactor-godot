extends Node


func _ready():
	$AnimatedArrow/AnimationPlayer.play("Play")
	pass

func _set_selected():
	pass
	#var cube = get_node("Mesh").get_node("Cube")
	#var surface_material = cube.mesh.surface_get_material(0)
	
	#var new_mat = SpatialMaterial.new()
	#new_mat.albedo_color =  Color(0.6, 0.9, 0.6, 1);
	#new_mat.albedo_texture = surface_material.albedo_texture
	#cube.set_surface_material(0, new_mat)

func _set_unselected():
	pass
	#var cube = get_node("Mesh").get_node("Cube")
	#cube.set_surface_material(0, null)

func _set_disabled():
	$AnimatedArrow.hide()
	$OfflineArrow.show()

func _set_enabled():
	$AnimatedArrow.show()
	$OfflineArrow.hide()