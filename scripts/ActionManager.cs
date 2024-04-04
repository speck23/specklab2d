using Godot;
using System;

public partial class ActionManager : Node2D
{
	
	[Export] PhysicsScene SCENE;
	[Export] TextureRect PREVIEW_OB;
	[Export] string MODE = "spawn_creative";
	[Export] string  PREFAB_PATH = "spawn";


	public void set_mode(string m){
		MODE = m;
		GetNode<AudioManager>("%audio_manager").click_hit(0);
	}
	public void set_prefab_path(string pp) {
		PREFAB_PATH = pp;
		if(pp != ""){
		PREVIEW_OB.Texture = ResourceLoader.Load<Texture2D>(PREFAB_PATH.GetBaseName()+".png");
		PREVIEW_OB.Show();
		}else PREVIEW_OB.Hide();
	}

	void input_keys(InputEvent @e){
		if (@e.IsActionPressed("ui_right"));
		if (@e.IsActionPressed("ui_left"));
		if (@e.IsActionPressed("ui_up"));
		if (@e.IsActionPressed("ui_down"));
	}

	void spawn_creative(InputEvent @e){
		if(@e is InputEventMouseButton){
			InputEventMouseButton me = (InputEventMouseButton)@e;
			if((int)me.ButtonIndex == 1 && e.IsReleased()){
				GD.Print("spawn_from_path() ", PREFAB_PATH);
				var b = BodyTools.body_deserialize(PREFAB_PATH);

				GD.Print("loaded ok? ", b);
				if(Mathf.Abs(RotationDegrees) >1 ){
					GD.Print("rotated!");
					for(int i=0;i<b.points.Length;i++)
						b.points[i].pos = b.points[i].pos.Rotated(Rotation);
					
				}
				GetNode<PhysicsScene>("%scene").spawn_body(
					b,
					GetGlobalMousePosition()
				);
				GetNode<PhysicsScene>("%scene").QueueRedraw();

				GetNode<AudioManager>("%audio_manager").click_hit(2);
			}
		}

		if (@e is InputEventKey k && @e.IsReleased()){
			if(k.Keycode == Key.Q){
				Rotate(-Mathf.Pi*0.125f);
				GetNode<AudioManager>("%audio_manager").click_hit(1);
			} 
			if(k.Keycode == Key.E){
				Rotate(Mathf.Pi*0.125f);
				GetNode<AudioManager>("%audio_manager").click_hit(1);
			} 
		}
			
		if (@e.IsActionPressed("ui_undo")){
			GetNode<PhysicsScene>("%scene").remove_last_body();
			GetNode<PhysicsScene>("%scene").QueueRedraw();
		}
			
	}

	// ** INTERACTIVE **
	v_body SELECTED_BODY;
	int SELECTED_POINT = -1;

	[Export] float INPUT_explosion_power = 128;
	[Export] float INPUT_explosion_range = 800;
	[Export] float INPUT_explosion_damp = 0.75f;

	void interact_creative(InputEvent @event){

		if (@event is InputEventMouseButton e)
			if(e.Pressed == true){
				if(e.ButtonIndex == (MouseButton)1){
					float d = 0;
					SELECTED_POINT =  SceneTools.bodies_get_closest_point_at( SCENE.bodies, GlobalPosition, out SELECTED_BODY, out d);
					if(SELECTED_BODY!=null)
					if(SELECTED_POINT>=0 && SELECTED_POINT < SELECTED_BODY.points.Length)
					if(SELECTED_BODY.points[ SELECTED_POINT ].pos.DistanceTo(  GlobalPosition ) > 64){
						SELECTED_BODY = null;
						SELECTED_POINT = -1;
					}
				}
			}else if(e.Pressed == false){
				if(e.ButtonIndex == (MouseButton)1){
					SELECTED_BODY = null;
					SELECTED_POINT = -1;
				}
				if(e.ButtonIndex == (MouseButton)3){
					v_body b;
					int p =-1;// bodies_get_closest_point_at( bodies, GetGlobalMousePosition(), out b);
					float d = -1f;
					int s = SceneTools.bodies_get_closest_segment_at(SCENE.bodies, GetGlobalMousePosition(), out b, out d);
					if(p>=0 && false){
						GD.Print("selected body ", b, " point: ", p);
						b.points[p].pos += Vector2.Right*64f;
					}
					
					if(s>=0 && d<32f){
						//BodyTools.body_remove_draw_segment(b, b.segments[s].a , b.segments[s].b);
						GD.Print("selected body ", b, " segment: ", s);
						int spr = BodyTools.body_find_spring_from_points(b, b.segments[s].a , b.segments[s].b);
						BodyTools.body_remove_segment(b, s);
						BodyTools.body_remove_spring(b, spr);
						b.body_modifier_clear = true;
					}else{ 
						s =  SceneTools.bodies_get_closest_spring_at(SCENE.bodies, GetGlobalMousePosition(), out b, out d);

						if(d<32f)
						if(s>=0){
							//BodyTools.body_remove_draw_segment(b, b.springs[s].a , b.springs[s].b);
							BodyTools.body_remove_spring(b, s);
						}
					}
				}
				
				if(e.ButtonIndex == (MouseButton)2){
					SceneTools.add_explosion_force(SCENE, GetGlobalMousePosition(), INPUT_explosion_range, INPUT_explosion_power, INPUT_explosion_damp);
					GetNode("%effect_manager").Call("explosion_effect_at", GetGlobalMousePosition());
					GetNode<AudioManager>("%audio_manager").explosion_hit();
				}
			}
	}
	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		if(SELECTED_BODY!=null)	
		if(SELECTED_BODY.points!=null)
		if(SELECTED_POINT<SELECTED_BODY.points.Length)
		if(SELECTED_POINT>=0){
				var dir = SELECTED_BODY.points[ SELECTED_POINT ].pos.DirectionTo(  GlobalPosition );
				
				var dist =  SELECTED_BODY.points[ SELECTED_POINT ].pos.DistanceTo( GlobalPosition )* 0.025f; //100px - 1f
				SELECTED_BODY.points[ SELECTED_POINT ].step = dir * 0.99f * Mathf.Min(9.9f, dist);
				SELECTED_BODY.points[ SELECTED_POINT ].side_step = dir * 0.09f * Mathf.Min(9.9f, dist);
			}
	}

    public override void _UnhandledInput(InputEvent @e)
    {
		if(MODE == "spawn_creative")
			spawn_creative(@e);
		else if(MODE == "interact_creative")
			interact_creative(@e);
		
    }
}
