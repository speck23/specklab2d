using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class PhysicsScene : Node2D
{
	[Export] AudioManager audio_manager;

	Vector2 G_VECTOR = Vector2.Down;
	public v_body[] bodies = new v_body[0];

	[Export] public Color[] COLOR_LUT;
	[Export] public Color DEBUG_SPRINGS_COL;
	[Export] public float DEBUG_SPRINGS_WIDTH = 3f;
	[Export] public float DEBUG_SPRINGS_DASH = 3f;

	[Export] public Color DEBUG_FORCES_COL;
	[Export] public float DEBUG_FORCES_WIDTH = 3f;
	[Export] public float DEBUG_FORCES_WIDTH_C = 1.7f;
	// ===== TOOLS =====
	public void clear_all(){
		bodies = new v_body[0];
		QueueRedraw();
	}
	public void spawn_body(v_body b, Vector2 pos){
		BodyTools.body_move_by(b, pos);
		b.ENERGY_LIMIT = SETUP_ENERGY_LIMIT;
		b.EXTRA_ENERGY_MUL = SETUP_EXTRA_ENERGY_MUL;
		b.GENERAL_DAMPING = SETUP_GENERAL_DAMPING;
		b.drag_power = SETUP_drag_power;

		
		var curr_body_list = new List<v_body>( bodies );
		curr_body_list.Add(b);
		bodies = curr_body_list.ToArray();
		GD.Print("<physics_scene.cs>.spawn_body() curr count: ", bodies.Length);
	}
	public void remove_last_body(){
		if(bodies.Length == 0) return;
		
		var curr_body_list = new List<v_body>( bodies );
		curr_body_list.RemoveAt( curr_body_list.Count-1 );
		bodies = curr_body_list.ToArray();
	}
	public override void _Ready()
	{
		audio_manager = GetNode<AudioManager>("%audio_manager");
	}

	public int active_explosion_forces = 0;
	public ExplosionForce[] explosion_forces = new ExplosionForce[ 64 ];


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void apply_explosion_force(v_body b, ExplosionForce ef){
		if(! ef.aabb.intersect(b.aabb) ) return;
		float scaled_power = ef.power*1.0f/ef.range;
		for(int i=0; i < b.MOVABLE_POINTS; i++){
			Vector2 vec = (b.points[i].pos - ef.pos);
			float d = vec.Length();
			if(d >= ef.range) continue;

			Vector2 dir = vec.Normalized();
			b.points[i].step += (dir * scaled_power * (ef.range-d) )/b.points[i].mass;
		}
	}
	//
	[Export] int PHYSICS_TICK_COUNT = 12;
	public void set_physics_tick_count(int v)=>
		PHYSICS_TICK_COUNT = v;
	[Export] float SETUP_GRAVITY_POWER = 0.1f;
	public void set_gravity_power(float v)=>
		SETUP_GRAVITY_POWER = v;
	
	[Export] float SETUP_ENERGY_LIMIT = 100;
	public void set_energy_limit(float l){
		SETUP_ENERGY_LIMIT = l;
		foreach (var b in bodies)
			b.ENERGY_LIMIT = l;
	}

	[Export] float SETUP_EXTRA_ENERGY_MUL = 0.0f;
	[Export] float SETUP_drag_power = 0.1f;
	[Export] float SETUP_GENERAL_DAMPING = 0.99999f;
	[Export] public int GRAVITY_MODE = 1;
	

	void PhysicsPass(){
	
		for(int i=0; i < active_explosion_forces; i++)
			explosion_forces[i].power *= explosion_forces[i].damping;
		
			//var t = Time.GetTicksMsec();
		for(int i=0; i< bodies.Length; i++)
			bodies[i].update_broad_aabb(28.5f);// might be not safe! but gives like 2% speed, 12.5 should be safe.
		for(int i=0; i< bodies.Length; i++)
			bodies[i].update_broad_aabb_list(bodies);
			//t = Time.GetTicksMsec()-t; GD.Print("broad phase time : ", t, "ms");
		ulong col_time = 0;

		for(int q = 0; q<PHYSICS_TICK_COUNT; q++){
			if(GRAVITY_MODE == 1)
				for(int i=0; i< bodies.Length; i++)
				bodies[i].accumulate_gravity( G_VECTOR*SETUP_GRAVITY_POWER );
			else if(GRAVITY_MODE == 2)
				for(int i=0; i< bodies.Length; i++)
				bodies[i].accumulate_directed_gravity( new Vector2(640, 512), SETUP_GRAVITY_POWER  *2f);
			else if(GRAVITY_MODE == 3){
				for(int j=0; j< bodies.Length; j++){
					var pos =  bodies[j].points[0].pos;
					var vec = SETUP_GRAVITY_POWER  *bodies[j].points[0].mass * bodies[j].points.Length*0.01f;
					for(int i=0; i< bodies.Length; i++)
						bodies[i].accumulate_directed_gravity(pos, vec);
				}
			}

			for(int i=0; i< bodies.Length; i++){
				bodies[i].accumulate_point_motors();
				bodies[i].accumulate_constraints();			
				bodies[i].update_aabb();
			}

				var tt = Time.GetTicksMsec();
			for(int i=0; i< bodies.Length; i++)
				bodies[i].accumulate_collisions_broad_local();
				col_time += Time.GetTicksMsec()-tt;
				

			for(int i=0; i< bodies.Length; i++){
				// scene explosive forces
				for(int j=0; j < active_explosion_forces; j++){
					if(explosion_forces[j].power < 0.005f) continue;
					apply_explosion_force(bodies[i], explosion_forces[j]);
				}
				//
				bodies[i].apply_external_effects();
				// *** HIT IMPACTS ***
				for(int k=0; k<bodies[i].MOVABLE_POINTS; k++){
					float hit = bodies[i].points[k].hit_step.Length();
			
				//	if(hit > 0.001f)GD.Print(hit);
					if( hit > .1f ){		
						audio_manager.impact_hit( hit );
					}
				}
				//
			
				bodies[i].apply_steps_uncolided(1.0f - q*1.0f/16.0f);
			}
		} // q
	}
	
	bool DEBUG_PRINT_TIME = false;
	public override void _Process(double delta)
	{

		if(autostep){
			var t = Time.GetTicksMsec();
			
				PhysicsPass();
			avg_step_time =  (Time.GetTicksMsec()-t)*0.1f + avg_step_time*0.9f;
			if(DEBUG_PRINT_TIME)
				GD.Print("time ", Time.GetTicksMsec()-t, " avg : ", avg_step_time);
			QueueRedraw();
		}


	}
	float avg_step_time = 0.0f;

	bool SHOW_DEBUG_POINTS = false;
	bool SHOW_DEBUG_BBOXES = false;
	bool SHOW_DEBUG_EXPLOSIONS = false;
	bool SHOW_DEBUG_SPRINGS = false;
	[Export] int DEBUG_ADVANCED = 0;
	List<Vector2[]> draw_cashe = new List<Vector2[]>();
	
	void setup_draw_cashe(){
		draw_cashe = new List<Vector2[]>();
		for(int b=0; b< bodies.Length; b++)
			draw_cashe.Add( new Vector2[bodies[b].segments.Length *2] );
	}

	public override void _Draw()
	{
		var t = Time.GetTicksMsec();
		if(bodies != null)
		for(int b=0; b< bodies.Length; b++){
			v_body body = bodies[b];

			// ** DRAW LIST DRAWING **
			if(body.draw_list!=null){
				for(int i = 0; i<body.draw_list.Length; i++){
					
					//color mode closed loop
					int type = body.draw_list[i].type;
					int val = body.draw_list[i].val;
					if(type == 1){ //segment

					}else if(type == 2){ //chain
						int a = body.draw_list[i].a;
						int bb = body.draw_list[i].b;
							
						Vector2[] line = new Vector2[bb];

						for(int j = 0; j<bb; j++)
							line[j] = body.points[a+j].pos;
						DrawPolyline(line, COLOR_LUT[val], -1f);

					}else if(type == 4){ //segments draw

						if(body.segments!=null ){
							Vector2[] draw_segments = new Vector2[body.segments.Length *2];
							Color[] draw_colors = new Color[body.segments.Length ];
							Int32 dsi = 0;
							Int32 a = body.draw_list[i].a;
							Int32 z = body.draw_list[i].b;
							for(int k = a; k<z; k++){
								SG s = body.segments[k];
								Vector2 pa = body.points[s.a].pos;
								Vector2 pb = body.points[s.b].pos;

									var dir = ( body.points[s.b].pos-body.points[s.a].pos ).Normalized();
									draw_segments[dsi++] = body.points[s.a].pos - dir*3f;//*4;
									draw_segments[dsi++] = body.points[s.b].pos + dir*3f;//*4;
									draw_colors[k] = COLOR_LUT[ s.type ];
							}
							Color col = body.base_draw_color;
							col.A8 =  SHOW_DEBUG_POINTS?(byte)192:(byte)255;

							if(draw_segments.Length>0)
							//DrawMultiline(draw_segments, col, 6f);// 8.0f);
							DrawMultilineColors(draw_segments, draw_colors, val);
							if(body.points.Length==1)
							DrawCircle(body.points[0].pos,  6.0f, col);
						//	DrawPolyline(draw_segments, Color.Color8(8, 8, 8, 255), 8.0f , true);
						}	
					
					}else if(type == 5){ //polygon
						int a = body.draw_list[i].a;
						int bb = body.draw_list[i].b;
							
						Vector2[] points = new Vector2[3];
						points[0] = body.points[a].pos;
						Color[] cols = new Color[3];
						cols[0] = COLOR_LUT[val]; cols[1] = COLOR_LUT[val]; cols[2] = COLOR_LUT[val];
						Vector2[] uvs = new Vector2[3];
						uvs[0] = Vector2.Zero; uvs[1] = Vector2.Right; uvs[2] = Vector2.Up;

						for(int j = 1; j<bb-1; j++){
							points[1] = body.points[a+j].pos;
							
							points[2] = body.points[a+j+1].pos;		
							DrawPrimitive(points, cols, uvs);			
						}
						points[1] = body.points[a+bb-1].pos;
						points[2] = body.points[a+1].pos;		
						DrawPrimitive(points, cols, uvs);		
						

					}else if(type == 6){ //polygon
						int a = body.draw_list[i].a;
						int bb = body.draw_list[i].b;
							
						Vector2[] line = new Vector2[bb];

						for(int j = 0; j<bb; j++)
							line[j] = body.points[a+j].pos;
						
						DrawColoredPolygon(line, COLOR_LUT[val]);
					}

				}
			}


			//pinned
			//	for(int i=body.MOVABLE_POINTS; i<body.points.Length; i++)
			//		DrawCircle( body.points[i].pos, 4f, Color.Color8(48, 24, 24, 255));			
			// *** velocity vectors ***
			if(SHOW_DEBUG_SPRINGS)
			if(body.springs != null)
			for(int i = 0; i<body.springs.Length; i++){
					var s = body.springs[i];
					Vector2 pa = body.points[s.a].pos;
					Vector2 pb = body.points[s.b].pos;
					DrawDashedLine(pa, pb, DEBUG_SPRINGS_COL, DEBUG_SPRINGS_WIDTH, DEBUG_SPRINGS_DASH, false);
			}

			if(body.points!=null){
			if(SHOW_DEBUG_POINTS)
				for(int i=0; i<body.points.Length; i++){
					DrawLine(body.points[i].pos, body.points[i].pos+body.points[i].step*64f, DEBUG_FORCES_COL, DEBUG_FORCES_WIDTH);
					DrawCircle( body.points[i].pos+body.points[i].step*64f, DEBUG_FORCES_WIDTH_C, DEBUG_FORCES_COL);
				}
				//
				if(DEBUG_ADVANCED > 0)
				for(int i=0; i<body.points.Length; i++){
				//		DrawLine(body.points[i].pos, body.points[i].pos+body.points[i].dbl_shift*64, Color.Color8(224, 224, 64, 255), 2);
					//	DrawCircle( body.points[i].pos+body.points[i].dbl_shift, 2f, Color.Color8(64, 180, 64, 255));
					if(DEBUG_ADVANCED > 1){
				//		DrawLine(body.points[i].pos, body.points[i].pos+body.points[i].dbl_step*64, Color.Color8(224, 32, 32, 255), 2);
						//DrawCircle( body.points[i].pos+body.points[i].dbl_side_step, 2f, Color.Color8(64, 180, 64, 255));	
					}
					if(DEBUG_ADVANCED > 2){
						DrawLine(body.points[i].pos, body.points[i].pos+body.points[i].db_shift_bounce, Color.Color8(224, 224, 224, 255), 2);
						//DrawCircle( body.points[i].pos+body.points[i].dbl_side_step, 2f, Color.Color8(64, 180, 64, 255));	
					}
				}
			}
			
			// *** bboxes ***
			if(SHOW_DEBUG_BBOXES){
				DrawRect(new Rect2(body.aabb.min_x, body.aabb.min_y, body.aabb.max_x-body.aabb.min_x, body.aabb.max_y-body.aabb.min_y),
					Color.Color8(64,64,64, 255), false);
				DrawRect(new Rect2(body.broad_aabb.min_x, body.broad_aabb.min_y, body.broad_aabb.max_x-body.broad_aabb.min_x, body.broad_aabb.max_y-body.broad_aabb.min_y),
					Color.Color8(128,64,64, 255), false);
			}

		

		if(!SHOW_DEBUG_POINTS) continue;
			if(body.points!=null){
				for(int i = 0; i<body.COLLISION_POINTS; i++){
					DrawCircle(body.points[i].pos, 4.0f, Color.Color8(180,24,24,96));
				}
			}
			if(body.pin_springs!=null){
				for(int i = 0; i<body.pin_springs.Length; i++){
					DrawCircle(body.points[ body.pin_springs[i].pin_point].pos+body.pin_springs[i].pin_offset, 
					-4f, Color.Color8(22,244,24,192));
				}
			}
		}



		for(int i=0; i < active_explosion_forces; i++){
			if ( explosion_forces[i].power < 0.005f ) continue;
			DrawCircle(	explosion_forces[i].pos, explosion_forces[i].range, Color.Color8(160, 128, 128, (byte)(Mathf.Min(16,16 *explosion_forces[i].power))) );
		}
				t = Time.GetTicksMsec() - t;
		//GD.Print("draw time ", t);
	}

	// INPUT
	public bool autostep = false;
			int substep = 0;


	//

	[Export] Node2D virtual_mouse;
	public override void _Input(InputEvent @event)
	{

	//	if (@event.IsActionPressed("ui_focus_next")){
	//		PhysicsPass();
	//		QueueRedraw();
	//	}

		if (@event is InputEventKey)
			if( ((InputEventKey)@event).Keycode == Key.Y && ((InputEventKey)@event).IsReleased()){
					SHOW_DEBUG_POINTS = ! SHOW_DEBUG_POINTS;
			}
		if (@event is InputEventKey)
			if( ((InputEventKey)@event).Keycode == Key.U && ((InputEventKey)@event).IsReleased()){
					DEBUG_PRINT_TIME = ! DEBUG_PRINT_TIME;
			}
		if (@event is InputEventKey)
			if( ((InputEventKey)@event).Keycode == Key.B && ((InputEventKey)@event).IsReleased()){
					SHOW_DEBUG_BBOXES = ! SHOW_DEBUG_BBOXES;
			}
		if (@event is InputEventKey)
			if( ((InputEventKey)@event).Keycode == Key.S && ((InputEventKey)@event).IsReleased()){
					SHOW_DEBUG_SPRINGS = ! SHOW_DEBUG_SPRINGS;
			}	
	//	apply_impulse_at
	}
}



