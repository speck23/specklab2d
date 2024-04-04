extends Node2D

@export var particle_prefab : Node2D


func explosion_effect_at(pos : Vector2):
	particle_prefab.global_position = pos
	particle_prefab.restart()

