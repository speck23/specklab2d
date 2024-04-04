using Godot;
using System;

public partial class AudioManager : AudioStreamPlayer
{
	[Export] AudioStreamPlayer exclusive_player;
	[Export] AudioStream step_sound;
	[Export] AudioStream jump_sound;

	[Export] AudioStream impact_sound;

		[Export] AudioStream explosion_sound;
		[Export] AudioStream click_a_sound;
		[Export] AudioStream click_b_sound;
		[Export] AudioStream click_c_sound;

		[Export] float volume_base = 0f;
	public override void _Ready(){}

	byte tick = 0;
	public override void _Process(double delta)
	{
		tick ++;
		if(tick % 7 == 0){
			impact_hit_count--;
		}
	}
	
	public void click_hit(int type){
		if(type==0)exclusive_player.Stream = click_a_sound;
		if(type==1)exclusive_player.Stream = click_b_sound;
		if(type==2)exclusive_player.Stream = click_c_sound;
		exclusive_player.VolumeDb = 0f;
		exclusive_player.PitchScale = 1f;
		exclusive_player.Play();
	}
	public void explosion_hit(){
		
		exclusive_player.Stream = explosion_sound;
		exclusive_player.VolumeDb = 0f;
		exclusive_player.PitchScale = 1f;
		exclusive_player.Play();
	}
	int impact_hit_count = 0;
	public void impact_hit(float impact_power ){
		//if(Playing) return;
	 
		if(impact_hit_count >= 16) return;
		impact_hit_count++;
		VolumeDb = -40.0f + Mathf.Min(40.0f, impact_power * 40f) +volume_base;
	//	PitchScale = 0.9f + Mathf.Min(.2f, impact_power * 0.1f);
		Stream = impact_sound;
		Play();
	}
	public void move_step(float speed = 1.0f){
		//if(Playing) return;
		//PitchScale = speed;
		VolumeDb = volume_base;
		Stream = step_sound;
		Play();
	}
	public void move_jump(float speed = 1.0f){
		//if(Playing) return;
		//PitchScale = speed;
		VolumeDb = volume_base;
		Stream = jump_sound;
		Play();
	}
}
