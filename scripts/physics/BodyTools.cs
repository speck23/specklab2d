using Godot;
using System;
using System.Collections.Generic;

public static class BodyTools
{
	// =========== HELPERS ==========
	 

	public static v_body body_create_from_preview_node(Node2D preview_ob, bool local_coordinates){
		v_body body = new v_body();
		body.points = new P[preview_ob.GetNode("p").GetChildCount()];

		Vector2 off = Vector2.Zero;
		if(local_coordinates)off = -preview_ob.GlobalPosition;

		for(int i=0; i<body.points.Length; i++){
			body.points[i] = new P( preview_ob.GetNode("p").GetChild<Node2D>(i).GlobalPosition+off );
			body.points[i].mass = preview_ob.GetNode("p").GetChild<Node2D>(i).Scale.X;
		}
		
		// *** SEGMENTS ***
		var seg_load = (Vector3[]) preview_ob.Get("segments");
		body.segments = new SG[ seg_load.Length ];
		for(int i=0; i<body.segments.Length; i++){
			body.segments[i] = new SG( (int)seg_load[i].X, (int)seg_load[i].Y );
			body.segments[i].type = (int)seg_load[i].Z;
		}
		
		// *** SPRINGS ***
		var spr_load = (Vector3[]) preview_ob.Get("springs");
		body.springs = new CD[ spr_load.Length ];
		for(int i=0; i<body.springs.Length; i++){
			var d =  body.points[(int)spr_load[i].X].pos.DistanceTo(  body.points[(int)spr_load[i].Y].pos );
			body.springs[i] = new CD((int)spr_load[i].X, (int)spr_load[i].Y, d);
			body.springs[i].stiff = spr_load[i].Z;
		}
		
		// *** PIN SPRINGS ***
		var pin_spr_ab_load = (Vector2[]) preview_ob.Get("pin_springs_ab");	//pin_a, pin_b
		var pin_spr_os_load = (Vector3[]) preview_ob.Get("pin_springs_os"); //offset, stiff

		body.pin_springs = new CDE[ pin_spr_ab_load.Length ];
		for(int i=0; i<body.pin_springs.Length; i++){
			body.pin_springs[i] = new CDE(
				(int)pin_spr_ab_load[i].X,
				(int)pin_spr_ab_load[i].Y,
				new Vector2(pin_spr_os_load[i].X, pin_spr_os_load[i].Y),
				pin_spr_os_load[i].Z
			);
		}
		//
		// *** POINT MOTORS ***
		var point_motors = (Vector3[]) preview_ob.Get("point_motors");
		body.motors = new PM[ point_motors.Length ];
		for(int i=0; i<body.motors.Length; i++){
			var pm = point_motors[i];
			body.motors[i] = new PM((int)pm.X, (int)pm.Y, pm.Z);
		}
		//
		// *** DRAW LISTS ***
		var draw_list = (Vector3[]) preview_ob.Get("draw_list");
		body.draw_list = new DL[ draw_list.Length ];
		for(int i=0; i<body.draw_list.Length; i++){
			var pm = draw_list[i];

		
				if((int)pm.Z == -1) pm.Z = body.points.Length;
				if((int)pm.Z == -2) pm.Z = body.segments.Length;

			body.draw_list[i] = new DL((int)(pm.X)/100, (int)pm.Y, (int)pm.Z,  (int)(pm.X)%100);
		}
		//
		body.base_draw_color = (Color) preview_ob.Get("base_draw_color");
		body.COLLISION_POINTS = (int) preview_ob.Get("collision_points");
		body.COLLISION_SEGMENTS = (int) preview_ob.Get("collision_segments");	
		body.MOVABLE_POINTS = (int) preview_ob.Get("movable_points");

			var mh = (int) preview_ob.Get("max_hits");
			if(mh == 0 || mh == -1) mh = body.COLLISION_SEGMENTS;
		body.MAX_HITS = mh;

		body.aabb = new AABB();
		body.broad_aabb = new AABB();
		return body;
	}

	//
	public static void body_setup_box(v_body b,  Vector2 position){
		var rnd = new Random();
		UInt32 s = (UInt32) ( rnd.Next(16) +16 );

		b.points = new P[4]{
			new P(new Vector2(0, s), Vector2.Zero),//0
			new P(new Vector2(s, s), Vector2.Zero),//1
			new P(new Vector2(s, 0), Vector2.Zero),//2
			new P(new Vector2(0, 0), Vector2.Zero),//3
		};
		for(int i=0; i< b.points.Length; i++)
			b.points[i].pos += position;

		b.segments = new SG[4]{
			new SG( 0, 1),
			new SG( 1, 2),
			new SG( 2, 3),
			new SG( 3, 0)
		};
		b.springs = new CD[6]{
			new CD( 0, 1, s),
			new CD( 1, 2, s),
			new CD( 2, 3, s),
			new CD( 3, 0, s),
			new CD( 0, 2, s*Mathf.Sqrt(2)),
			new CD( 1, 3, s*Mathf.Sqrt(2)),
		};

		b.COLLISION_POINTS = b.points.Length;
		b.COLLISION_SEGMENTS = b.segments.Length;
		b.MOVABLE_POINTS = b.points.Length;

		b.aabb = new AABB();
		b.broad_aabb = new AABB();
	}
	// =========== UTILITY ==========

	public static void body_save_preview(v_body body, Viewport v, String path, Vector2 resize){
		var img = v.GetTexture().GetImage();
		int w = (int) ( body.aabb.max_x - body.aabb.min_x );
		int h = (int) ( body.aabb.max_y - body.aabb.min_y );
		Image img2 = Image.Create(w, h, false, img.GetFormat());

		var rect = new Rect2I((int)body.aabb.min_x, (int)body.aabb.min_y, w, h);
		rect.Position += (Vector2I)v.GlobalCanvasTransform.Origin;

		img2.BlitRect(img, rect, Vector2I.Zero);
		
		//remove alpha?
		img2.Convert(Image.Format.Rgba8);
		for(int y=0;y<h;y++)
		for(int x=0;x<w;x++)
		if(img2.GetPixel(x,y).R8 +img2.GetPixel(x,y).G8+img2.GetPixel(x,y).B8 > 3*164)
			img2.SetPixel(x,y, Colors.Transparent);

		if(resize.X > 1)
			img2.Resize( (int)resize.X, (int)resize.Y );
		

		img2.SavePng(path);
	}

	public static void body_move_by(v_body body, Vector2 move){
		if (body.points==null) return;
		for(int i=0; i<body.points.Length; i++)
			body.points[i].pos += move;
	//	if (body.pin_springs!=null) 
	//	for(int i=0; i<body.pin_springs.Length; i++)
	//		body.pin_springs[i].pin_pos += move;
	}

	public static void body_add_point(v_body body, Vector2 point_pos){
		if (body.points==null) body.points= new P[0];
		Array.Resize(ref body.points, body.points.Length+1);
		body.points[body.points.Length-1] = new P(point_pos);
	}
	public static void body_add_segment(v_body body, int pa, int pb){
		if (body.segments==null) body.segments= new SG[0];
		Array.Resize(ref body.segments, body.segments.Length+1);
		body.segments[body.segments.Length-1] = new SG(pa, pb);
	}
	public static void body_add_spring(v_body body, int pa, int pb, float dist, float stiff){
		if (body.springs==null) body.springs= new CD[0];
		Array.Resize(ref body.springs, body.springs.Length+1);
		body.springs[body.springs.Length-1] = new CD(pa, pb, dist);
		body.springs[body.springs.Length-1].stiff = stiff;
	}
	public static void body_add_ex_spring(v_body body, int pa, int pin_point, Vector2 pin_offset, float dist, float stiff){
		if (body.pin_springs==null) body.pin_springs= new CDE[0];
		Array.Resize(ref body.pin_springs, body.pin_springs.Length+1);
		body.pin_springs[body.pin_springs.Length-1] = new CDE(pa, pin_point, pin_offset);
		body.pin_springs[body.pin_springs.Length-1].stiff = stiff;
	}
	//
	public static int body_find_spring_from_points(v_body body, int pa, int pb){
		if (body.springs==null) return -1;
		for (int i=0; i< body.springs.Length; i++)
			if ( (body.springs[i].a == pa && body.springs[i].b == pb) || (body.springs[i].b == pa && body.springs[i].a == pb) )
				return i;
		return -1;
	}
	public static int body_find_segment_from_points(v_body body, int pa, int pb){
		if (body.segments==null) return -1;
		for (int i=0; i< body.segments.Length; i++)
			if ( (body.segments[i].a == pa && body.segments[i].b == pb) || (body.segments[i].b == pa && body.segments[i].a == pb) )
				return i;
		return -1;
	}
	//
	public static void body_remove_spring(v_body body, int index){
		if (body.springs==null) return;
		if (index>=body.springs.Length) return;
	//	if (body.body_modifier !=null) return;
		//
		body.body_modifier += () =>{
		
		var list = new List<CD>( body.springs );
			if(index < body.springs.Length){
				list.RemoveAt(index);
				body.springs = list.ToArray();
			}
		};
	}
	public static void body_remove_spring(v_body body, int pa, int pb){
		int index = body_find_segment_from_points(body, pa, pb);
		if(index < 0) return;

		body.body_modifier += () =>{		
		var list = new List<CD>( body.springs );
			if(index < body.springs.Length){
				list.RemoveAt(index);
				body.springs = list.ToArray();
			}
		};
	}
	public static void body_remove_segment(v_body body, int index){
		if (body.segments==null) return;
		if (index>=body.segments.Length) return;
		//if (body.body_modifier !=null) return;

		body.body_modifier += () =>{
			if(index < body.segments.Length){
				body.COLLISION_SEGMENTS --;
				var list = new List<SG>( body.segments );
				list.RemoveAt(index);
				body.segments = list.ToArray();
			}
		};
	}
	public static void body_remove_segment(v_body body, int pa, int pb){
		int index = body_find_segment_from_points(body, pa, pb);
		if(index < 0) return;
		body.body_modifier += () =>{
			if(index < body.segments.Length){
				body.COLLISION_SEGMENTS --;
				var list = new List<SG>( body.segments );
				list.RemoveAt(index);
				body.segments = list.ToArray();
			}
		};
	}

	public static void body_remove_draw_segment(v_body body, int pa, int pb){
		List<DL> add_list = new List<DL>();
		List<int> remove_list = new List<int>();
		GD.Print("rem ",pa,", ",pb);
		for(int i=0; i<body.draw_list.Length; i++){
			int a = body.draw_list[i].a;
			int l = body.draw_list[i].b;
			int type = body.draw_list[i].type;
			if(type == 1){
				if(pa == a && pb == a+l){
					remove_list.Add(i);
				}
			}
			else if(type == 2 && pb>pa){
				if(pa >= a && pb <= a+l){
					GD.Print("t2 between ",a,", ",a+l-1);
					if(pa == a){
						GD.Print("case begin");
						if(l > 2){
							GD.Print("decrease from l:", l);
							body.draw_list[i].a = pb;
							body.draw_list[i].b --;
						}else remove_list.Add(i);
							
					}else{
						if(pb < a+l-1){
							GD.Print("case mid (add end)");
							var cp = body.draw_list[i];
							cp.a = pb;
							cp.b = a+l - pb;
							add_list.Add(cp);
							
							body.draw_list[i].b = pa-a+1;
						}else{
							GD.Print("case mid-end");
							body.draw_list[i].b = pa-a+1; // 2 -6 / r:3-4 // 
						}
					}
				}
			}
			else if(type == 3){
				if(pa>pb){
					GD.Print(" just remove loop ");
					body.draw_list[i].type = 2;
				}
				else if(pa >= a && pb <= a+l){
					body.draw_list[i].type = 2;//anyway?

					GD.Print("t2 between ",a,", ",a+l-1);
					if(pa == a){
						GD.Print("case begin");
						if(l > 2){
							GD.Print("decrease from l:", l);
							body.draw_list[i].a = pb;
							body.draw_list[i].b --;
						}else remove_list.Add(i);
							
					}else{
						if(pb < a+l-1){
							GD.Print("case mid (add end)");
							var cp = body.draw_list[i];
							cp.a = pb;
							cp.b = a+l - pb;
							add_list.Add(cp);
							
							body.draw_list[i].b = pa-a+1;
						}else{
							GD.Print("case mid-end");
							body.draw_list[i].b = pa-a+1; // 2 -6 / r:3-4 // 
						}
					}
				}
			}
		}
		//

		if(remove_list.Count > 0){
			var list = new List<DL>( body.draw_list );
			for(int j=remove_list.Count-1; j>=0; j--)
				list.RemoveAt( remove_list[j] );
			body.draw_list = list.ToArray();
		}
		if(add_list.Count > 0){
			var list = new List<DL>( body.draw_list );
			list.AddRange( add_list );
			body.draw_list = list.ToArray();
		}
		GD.Print("after rem: ", body.draw_list, ", len: ", body.draw_list.Length);
		for(int i=0; i<body.draw_list.Length; i++) 
			GD.Print(i, ", ab:", body.draw_list[i].a, ",", body.draw_list[i].b );

	}

	public static int body_select_point_at(v_body body, Vector2 pos){
		float D = float.MaxValue;
		int best_i = -1;
		for(int i=0; i<body.MOVABLE_POINTS; i++){
			var d = body.points[i].pos.DistanceTo(pos);
			if(d<D){
				D=d; best_i=i;
			}
		}
		if(D < 32) return best_i;
		else return -1;
	}
	public static void body_remove_at(v_body body, Vector2 pos){
		//find closest element
		float D = float.MaxValue;
		int best_i = -1;
		int best_type = -1;

		if(body.points!=null)
		for(int i=0; i<body.points.Length; i++){
			var d = body.points[i].pos.DistanceTo(pos);
			if(d<D){
				D=d; best_i=i; best_type=0;
			}
		}

		if(body.segments!=null)
		for(int i=0; i<body.segments.Length; i++){
			var seg = body.segments[i];
			var sa = body.points[seg.a].pos;
			var sb = body.points[seg.b].pos; var hit = new Vector2(0,0);
			var d = GeometryTools.point_segment_distance(pos, sa, sb, out hit);
			if(d<D){
				D=d; best_i=i; best_type=1;
			}
		}

		if(body.springs!=null)
		for(int i=0; i<body.springs.Length; i++){
			var spr = body.springs[i];
			var sa = body.points[spr.a].pos;
			var sb = body.points[spr.b].pos; var hit = new Vector2(0,0);
			var d = GeometryTools.point_segment_distance(pos, sa, sb, out hit);
			if(d<D){
				D=d; best_i=i; best_type=2;
			}	
		}
		if(body.pin_springs!=null)
		for(int i=0; i<body.pin_springs.Length; i++){
			var spr = body.pin_springs[i];
			var sa = body.points[spr.a].pos;
			var sb =  body.points[ body.pin_springs[i].pin_point ].pos + body.pin_springs[i].pin_offset; var hit = new Vector2(0,0);
			var d = GeometryTools.point_segment_distance(pos, sa, sb, out hit);
			if(d<D){
				D=d; best_i=i; best_type=3;
			}	
		}
		//
		if(best_i < 0) return;
		if(D > 32) return;
		//
		if(best_type == 0){
			var lst = new List<P>(body.points);
			lst.RemoveAt(best_i);
			body.points = lst.ToArray();
		}
		if(best_type == 1){
			var lst = new List<SG>(body.segments);
			lst.RemoveAt(best_i);
			body.segments = lst.ToArray();
		}
		if(best_type == 2){
			var lst = new List<CD>(body.springs);
			lst.RemoveAt(best_i);
			body.springs = lst.ToArray();
		}
		if(best_type == 3){
			var lst = new List<CDE>(body.pin_springs);
			lst.RemoveAt(best_i);
			body.pin_springs = lst.ToArray();
		}
	}

	//
	public static void body_set_alignment(v_body body, int center_mode=0){
		//top-left corner allignement
		body.update_aabb();
		if(center_mode == 0){
			body_move_by(body, -new Vector2(body.aabb.min_x, body.aabb.min_y) );
		}
		body.update_aabb();
	}
	// ========== SERIALIZATION ==========
	public static ConfigFile create_body_config_file(v_body body){
		// Create new ConfigFile object.
		var config = new ConfigFile();

		Vector2[] point_pos = new Vector2[ body.points.Length];
		for(int i = 0;i<body.points.Length;i++)point_pos[i] = body.points[i].pos;
		config.SetValue("points", "pos", point_pos );


		float[] point_m = new float[ body.points.Length];
		for(int i = 0;i<body.points.Length;i++)point_m[i] = body.points[i].mass;
		config.SetValue("points", "m", point_m );

		//care for some wild rounding
		if(body.segments!=null){
		Vector3[] sg_abt = new Vector3[ body.segments.Length];
		for(int i = 0;i<body.segments.Length;i++)sg_abt[i] = new Vector3( body.segments[i].a,  body.segments[i].b, body.segments[i].type);
		config.SetValue("segments", "abt", sg_abt );
		}

		if(body.springs!=null){
		Vector2[] sp_ab = new Vector2[ body.springs.Length];
		for(int i = 0;i<body.springs.Length;i++)sp_ab[i] = new Vector2( body.springs[i].a,  body.springs[i].b);
		config.SetValue("springs", "ab", sp_ab );
		Vector2[] sp_ds = new Vector2[ body.springs.Length];
		for(int i = 0;i<body.springs.Length;i++)sp_ds[i] = new Vector2( body.springs[i].dist,  body.springs[i].stiff);
		config.SetValue("springs", "ds", sp_ds );
		}

		if(body.pin_springs!=null){
		Vector2[] esp_ab = new Vector2[ body.pin_springs.Length];
		for(int i = 0;i<body.pin_springs.Length;i++)esp_ab[i] = new Vector2( body.pin_springs[i].a,  body.pin_springs[i].pin_point);
		config.SetValue("pin_springs", "ab", esp_ab );

		Vector2[] esp_off = new Vector2[ body.pin_springs.Length];
		for(int i = 0;i<body.pin_springs.Length;i++)esp_off[i] = body.pin_springs[i].pin_offset;
		config.SetValue("pin_springs", "off", esp_off );

		float[] esp_s = new float[ body.pin_springs.Length];
		for(int i = 0;i<body.pin_springs.Length;i++)esp_s[i] =  body.pin_springs[i].stiff;
		config.SetValue("pin_springs", "s", esp_s );
		}

		if(body.draw_list!=null){
			Vector3[] dl_tab = new Vector3[ body.draw_list.Length ];
			for(int i = 0;i<dl_tab.Length;i++)dl_tab[i] = new Vector3( body.draw_list[i].type,  body.draw_list[i].a, body.draw_list[i].b);
			config.SetValue("draw_list", "tab", dl_tab );
			GD.Print("dunno its RIDICUYLUS ", dl_tab[0]);

				float[] dl_v = new float[ body.draw_list.Length ];
				for(int i = 0;i<dl_v.Length;i++)dl_v[i] = body.draw_list[i].val;
				config.SetValue("draw_list", "v", dl_v );
		}
		
		if(body.motors!=null){
			Vector3[] m_abf = new Vector3[ body.motors.Length];
			for(int i = 0;i<body.motors.Length;i++) m_abf[i] = new Vector3( body.motors[i].a, body.motors[i].b, body.motors[i].f );

			config.SetValue("motors", "abf", m_abf );
		}
		//
			config.SetValue("other", "collision_points", body.COLLISION_POINTS);
			config.SetValue("other", "movable_points", body.MOVABLE_POINTS);
			config.SetValue("other", "collision_segments", body.COLLISION_SEGMENTS);

		return config;
	}
	public static void body_serialize(v_body body, String path, int center_mode=0){
		var config = create_body_config_file(body);
		config.Save(path);
	}
	public static String body_serialize_to_string(v_body body, int center_mode=0){
		ConfigFile config = create_body_config_file(body);
		return config.EncodeToText();
	}
	public static v_body body_deserialize(String path){
		var config = new ConfigFile();
		Error err = config.Load(path);
		if (err != Error.Ok) return new v_body();
		
		Vector2[] points_pos = (Vector2[])config.GetValue("points", "pos");
		float[] points_m = (float[])config.GetValue("points", "m");
	
		//
		v_body b = new v_body();
		if(points_pos!=null){ 
			if(points_pos.Length==0){GD.Print("[LOAD] fail, points is empty!"); return new v_body(); }
		b.points = new P[points_pos.Length];
		for(int i = 0;i<b.points.Length;i++){ 
			b.points[i].pos = points_pos[i];
			b.points[i].mass = points_m[i];
		}
		}else {GD.Print("[LOAD] fail, points is null!"); return new v_body(); }

		if(config.HasSection("segments")){
			Vector3[] segments_abt = (Vector3[])config.GetValue("segments", "abt");
			if(segments_abt!=null){ b.segments = new SG[segments_abt.Length];
			for(int i = 0;i<b.segments.Length;i++){
				b.segments[i].a = (int)segments_abt[i].X;  
				b.segments[i].b = (int)segments_abt[i].Y; 
				b.segments[i].type = (int)segments_abt[i].Z; 			
				}
			}
		}

		if(config.HasSection("springs")){
			Vector2[] springs_ab = (Vector2[])config.GetValue("springs", "ab");
			Vector2[] springs_ds = (Vector2[])config.GetValue("springs", "ds");
			if(springs_ab!=null){ b.springs = new CD[springs_ab.Length];
			for(int i = 0;i<b.springs.Length;i++){
				b.springs[i].a = (int)springs_ab[i].X;  
				b.springs[i].b = (int)springs_ab[i].Y; 
				b.springs[i].dist = springs_ds[i].X; 
				b.springs[i].stiff = springs_ds[i].Y; 
			}
			}
		}

		if(config.HasSection("pin_springs")){
			Vector2[] pin_springs_ab = (Vector2[])config.GetValue("pin_springs", "ab");
			Vector2[] pin_springs_off = (Vector2[])config.GetValue("pin_springs", "off");
			float[] pin_springs_s = (float[])config.GetValue("pin_springs", "s");
			if(pin_springs_ab!=null){ b.pin_springs = new CDE[pin_springs_ab.Length];
			for(int i = 0; i<b.pin_springs.Length; i++){
				b.pin_springs[i].a = (int)pin_springs_ab[i].X;  
				b.pin_springs[i].pin_point = (int)pin_springs_ab[i].Y;  		
				b.pin_springs[i].pin_offset =  pin_springs_off[i];
				b.pin_springs[i].stiff = pin_springs_s[i]; 
			}
			}
		}
		if(config.HasSection("draw_list")){
			Vector3[] draw_list_abt = (Vector3[])config.GetValue("draw_list", "tab");
			b.draw_list = new DL[draw_list_abt.Length];
			float[] draw_list_v = (float[])config.GetValue("draw_list", "v");
			for(int i=0; i<draw_list_abt.Length; i++)
				b.draw_list[i] = new DL( (int) draw_list_abt[i].X, (int) draw_list_abt[i].Y, (int) draw_list_abt[i].Z, (int)draw_list_v[i]);
		
		
		}


		if(config.HasSection("motors")){
			Vector3[] motors_abf = (Vector3[])config.GetValue("motors", "abf");

			if(motors_abf!=null){ 
				b.motors = new PM[motors_abf.Length];
				for(int i = 0; i<b.motors.Length; i++){
					b.motors[i].a = (int)motors_abf[i].X; 
					b.motors[i].b = (int)motors_abf[i].Y; 
					b.motors[i].f =  motors_abf[i].Z; 
				}
			}
		}
		if(config.HasSection("other")){
			b.COLLISION_POINTS = 	(int)config.GetValue("other", "collision_points");
			b.MOVABLE_POINTS =   	(int)config.GetValue("other", "movable_points");
			b.COLLISION_SEGMENTS =  (int)config.GetValue("other", "collision_segments");
		}else{
			b.COLLISION_POINTS = b.points.Length;
			b.MOVABLE_POINTS = b.points.Length;
			b.COLLISION_SEGMENTS =  b.segments.Length;
		}

		GD.Print("[load] points (count) ", b.points.Length);
		GD.Print("[load] segments (count) ", b.segments.Length);
		GD.Print("[load] springs (count) ", b.springs.Length);

		GD.Print("[load] COLLISION_POINTS (count) ", b.COLLISION_POINTS);
		GD.Print("[load] MOVABLE_POINTS (count) ", b.MOVABLE_POINTS);
		GD.Print("[load] COLLISION_SEGMENTS (count) ", b.COLLISION_SEGMENTS);

		b.update_aabb();
	
		return b;
	}
}
