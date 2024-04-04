using Godot;
using System;
using System.Collections.Generic;

public enum tag{
	teleportable
};
public partial class v_body
{
	public v_body(){}

	public void register_body_impulse(Vector2 vector){
		has_body_impulse = true;
		body_impulse_vector += vector;
	}
	public Vector2 get_body_impulse_vector(){
		return body_impulse_vector;
	}
	public void register_body_impulse_at(Vector2 pos, float power, float range){
		has_body_impulse = true;

		Vector2 aabb_center = new Vector2(aabb.min_x+aabb.get_w()*0.5f, aabb.min_y+aabb.get_h()*0.5f);
		body_impulse_vector = (aabb_center-pos).Normalized();
	}

	public void register_impulse_at(Vector2 pos, float power, float range){
		impulse_range = range;
		impulse_power = power;
		impulse_pos = pos;
	}
	
	public tag[] tags;
	public Color base_draw_color = Color.Color8(172, 172, 172, 255);
	public bool has_tag(tag t){
		for(int i=0; i<tags.Length; i++)
			if(tags[i] == t) return true;
		return false;
	}
	float impulse_range = 128.0f;
	float impulse_damping = 0.95f;
	float impulse_power = 0.0f;
	Vector2 impulse_pos = Vector2.Zero;

	float body_impulse_damping = 0.99f;
	bool has_body_impulse = false;
	Vector2 body_impulse_vector = Vector2.Zero;
	//
	public delegate void body_modifier_delegate();
	public body_modifier_delegate body_modifier = null;
	public bool body_modifier_clear = false;
	public void apply_external_effects(){
		if(has_body_impulse){
			for(int i = 0; i<MOVABLE_POINTS; i++)
				points[i].step += body_impulse_vector.LimitLength();
			body_impulse_vector*= body_impulse_damping;
			if(body_impulse_vector.LengthSquared() < 0.001f)
				has_body_impulse = false;
		}
		if(body_modifier != null){
			body_modifier();
			if(body_modifier_clear){
				body_modifier = null;
				body_modifier_clear = false;
			}
		}
	}
	//
	public int MAX_HITS = 2;
	public AABB aabb;
	public P[] points = new P[0];
	public int COLLISION_POINTS = 0;
	public int COLLISION_SEGMENTS = 0;
	public int MOVABLE_POINTS = 0;
	public CD[] springs = new CD[0];
	public CDE[] pin_springs; 
	public SG[] segments = new SG[0];
	public DL[] draw_list;

	public PM[] motors = new PM[0];
	public AABB broad_aabb;
	public static int BROAD_MAX_COUNT = 256;
	public int broad_aabb_list_count = 0;
	public v_body[] broad_aabb_list = new v_body[ BROAD_MAX_COUNT ];
	//
	public void update_broad_aabb(float border){
		broad_aabb.min_x = aabb.min_x-border;
		broad_aabb.max_x = aabb.max_x+border;
		broad_aabb.min_y = aabb.min_y-border;
		broad_aabb.max_y = aabb.max_y+border;
	}
	public void update_broad_aabb_list(v_body[] bodies){
		broad_aabb_list_count = 0;
		for(int i=0; i < bodies.Length; i++)
			if(bodies[i] != this && broad_aabb.intersect( bodies[i].broad_aabb ))
				broad_aabb_list[ broad_aabb_list_count++ ] = bodies[i];

	}
	
	public void update_aabb(){
		float min_x=float.MaxValue, max_x=float.MinValue, min_y=float.MaxValue, max_y=float.MinValue;
		for(int i = 0; i<points.Length; i++){
			min_x = Mathf.Min(min_x, points[i].pos.X);
			max_x = Mathf.Max(max_x, points[i].pos.X);
			min_y = Mathf.Min(min_y, points[i].pos.Y);
			max_y = Mathf.Max(max_y, points[i].pos.Y);	
		}
		aabb = new AABB(min_x-8.5f, max_x+8.5f, min_y-8.5f, max_y+8.5f);
	}
	public void accumulate_gravity(Vector2 g_vec){
		for(int i = 0; i<MOVABLE_POINTS; i++)
			points[i].step += g_vec;
	}
	public void accumulate_point_motors(){
		for(int i = 0; i<motors.Length; i++)
			points[motors[i].a].step += points[motors[i].a].pos.DirectionTo( points[motors[i].b].pos ) * motors[i].f;
	}	
	public void accumulate_directed_gravity(Vector2 target, float power){
		for(int i = 0; i<MOVABLE_POINTS; i++)
			points[i].step += points[i].pos.DirectionTo( target ) * power;
	}


	
	public void accumulate_constraints(){
		if(springs != null)
		for(int i = springs.Length-1; i>=0; i--){
			Vector2 pa = points[springs[i].a].pos;
			Vector2 pb = points[springs[i].b].pos;
			Vector2 diff  = pa-pb;
		
			float diff_len = diff.Length();
			if(diff_len < 0.0001f) continue;
	
			var err =  (springs[i].dist - diff_len)*0.5f; 
			Vector2 offset  = diff.Normalized()*err  *springs[i].stiff;

			points[ springs[i].a ].side_step += offset;
			points[ springs[i].b ].side_step -= offset;
			

			// AUTOBREAKING?
			if(false)
			if(Mathf.Abs( err ) > springs[i].dist * 0.5f *0.5f){
				//GD.Print("bt.body_remove_spring ", i);
				BodyTools.body_remove_spring(this, i);

				int seg = BodyTools.body_find_segment_from_points(this, springs[i].a, springs[i].b);
				if(seg>=0)
					BodyTools.body_remove_segment(this, seg);
				
				body_modifier_clear = true;
			}

			// E. SHARING?
			var ratio = Mathf.Min(0.25f, err * 0.1f);
		//	points[ springs[i].b ].side_step += points[ springs[i].a ].step *points[ springs[i].a ].mass * ratio;
		//	points[ springs[i].a ].side_step += points[ springs[i].b ].step *points[ springs[i].b ].mass * ratio;
		}

		if (pin_springs != null ) //probably to refactor
		for(int i = 0; i<pin_springs.Length; i++){
			Vector2 pa =  points[pin_springs[i].a].pos;
			Vector2 pb =  points[pin_springs[i].pin_point].pos + pin_springs[i].pin_offset;
			Vector2 diff  = pa-pb;

			float diff_len = diff.Length();
			if(diff_len < 0.0001f) continue;
		
			Vector2 offset  = diff.Normalized()* diff_len*pin_springs[i].stiff ;
	
			points[ pin_springs[i].a ].side_step -= offset;
			points[ pin_springs[i].pin_point ].step += offset;
		}
	}
	[Export] int BOUNCE_MODE = 12;
	[Export] float BOUNCE_POWER = 0.5f;
	public float GENERAL_DAMPING = 0.999f;	

 	public static double log2map(float input, float inputMin, float inputMax)
    {
        input = Math.Max(Math.Min(input, inputMax), inputMin);
        double logMin = Math.Log2(Math.Abs(inputMin) + 1); // Adding 1 to avoid Log2(0)
        double logMax = Math.Log2(Math.Abs(inputMax) + 1);
        double logValue = Math.Log2(Math.Abs(input) + 1);
        double normalizedValue = 2 * (logValue - logMin) / (logMax - logMin) - 1;
       // normalizedValue *= Math.Sign(input);
	    if(input < 0) normalizedValue *= -1f;
        return normalizedValue;
    }
	public void apply_steps_uncolided( float damp ){
	   for(int i = 0; i<MOVABLE_POINTS; i++){
		points[i].step = points[i].step.LimitLength( ENERGY_LIMIT );

		// *** save debug ***
		var side = (points[i].side_step  * 1f).LimitLength( );
		const float POS_MLT = 2.0f;
		if(!points[i].hit_step.IsZeroApprox()){//0.05f
			points[i].pos += points[i].shift.LimitLength();	
			
			var hit = (
				points[i].hit_step * drag_power + 
				points[i].shift.Normalized()*points[i].hit_step.Length() * (1.0f-drag_power)
			);

			points[i].step += hit/points[i].mass;			

			var mov =  points[i].step*0.5f;
			mov += side*0.5f;
			points[i].step += side*0.05f;
				mov = mov.LimitLength(.3f);//hmmm, ??
			points[i].pos += mov;// *0.5f;//1f/max_energ	
			
		}else{
			
			var mov =  points[i].step*0.5f;
			mov += side*0.5f;
			points[i].step += side*0.05f;
				mov = mov.LimitLength();//hmmm, ??
			points[i].pos += mov;// *0.5f;//1f/max_energ	
		}
				
		points[i].step *= (0.9999f+points[i].mass*0.00009f);

		points[i].side_step = Vector2.Zero;	
		points[i].hit_step  = Vector2.Zero;	
		points[i].shift = Vector2.Zero;
	   }  
	}
	// *** CONFIG ***
	public float ENERGY_LIMIT = 100f;
	public float EXTRA_ENERGY_MUL = 0.0f;
	public float drag_power = 0.1f;
	public void accumulate_collisions_broad_local(){
			
		for(int p=0; p<COLLISION_POINTS; p++){
			Vector2 point_pos = points[p].pos;
			for(int n=0; n < broad_aabb_list_count; n++){
				int hits = 0;

				if(!broad_aabb_list[n].aabb.has_point(point_pos)) continue;
				
				for (int i=0; i < broad_aabb_list[n].COLLISION_SEGMENTS; i++){
					//if(hits > 1){//GD.Print(i, "/", broad_aabb_list[n].COLLISION_SEGMENTS); 
					//	break; }
					SG s = broad_aabb_list[n].segments[i];					
					Vector2 hit;
				
					float d = GeometryTools.point_segment_distance(point_pos, 
						broad_aabb_list[n].points[s.a].pos, 
						broad_aabb_list[n].points[s.b].pos, 
						out hit );
					
					if(d < 8f){
			
						hits ++;
						Vector2 dir = (point_pos-hit).LimitLength()* (8.01f-d);//0.01f extra offset in solving -,-
						//
						//var ab = (broad_aabb_list[n].points[s.b].pos-broad_aabb_list[n].points[s.a].pos).Normalized();
						//var vec = (hit-point_pos).Normalized();
						//var reflect = vec.Bounce(ab);
						//
						float s_len = points[p].side_step.LengthSquared() +0.1f;
						float len_sum = s_len + (
										broad_aabb_list[n].points[s.a].side_step.LengthSquared() 			
										+
										broad_aabb_list[n].points[s.b].side_step.LengthSquared() 
										)*0.5f
										+0.1f;

						float ratio = s_len/len_sum;
						float ratio2 = 1.0f-ratio;

						//float min_drag = Mathf.Min(points[p].drag, broad_aabb_list[n].points[s.a].drag);
						//*** segment ratio ***
						float sd1 = hit.DistanceSquaredTo( broad_aabb_list[n].points[s.a].pos );
						float sd2 = hit.DistanceSquaredTo( broad_aabb_list[n].points[s.b].pos );
						float sdsum = sd1+sd2;
						float r1 = sd1/sdsum;
						float r2 = 1.0f- r1;//(float)Math.Round(r1, 1);
						//*** segment ratio ***			
						points[p].shift += dir*ratio2;
						broad_aabb_list[n].points[s.a].shift -= dir*ratio *r2;
						broad_aabb_list[n].points[s.b].shift -= dir*ratio *r1;

						// ** hit steps **
						// ** just energy mix **
						var seg_step = (broad_aabb_list[n].points[s.a].step + broad_aabb_list[n].points[s.b].step) *0.5f;		
						var seg_side_step = (broad_aabb_list[n].points[s.a].side_step + broad_aabb_list[n].points[s.b].side_step) *0.5f;		
						var seg_mass = (broad_aabb_list[n].points[s.a].mass + broad_aabb_list[n].points[s.b].mass) *0.5f;			
						
						// ** share ratio based on velocity **
						var p_share_ratio = points[p].step.LengthSquared() 
						/ (points[p].step.LengthSquared()+seg_step.LengthSquared());
						var s_share_ratio = 1.0f - p_share_ratio;

						// **[!] mul by contact area and distance (need to do someday) **
						// [test] .25* had <0, 0.3> range for drag stack.
						float SHARE_MLT = .25f +.75f* (8f-d) + EXTRA_ENERGY_MUL;//  + 1.0f; // d should be <0,8>
						//GD.Print( Math.Round(SHARE_MLT, 1) );

							var mass_limit = points[p].mass<seg_mass?points[p].mass:seg_mass;
		
						var pt_share_e = (points[p].step* mass_limit * p_share_ratio) * SHARE_MLT;
						var sg_share_e = (seg_step* mass_limit * s_share_ratio)  * SHARE_MLT;

						points[p].hit_step += (sg_share_e - pt_share_e);
						broad_aabb_list[n].points[s.a].hit_step += (pt_share_e-sg_share_e) *r2;
						broad_aabb_list[n].points[s.b].hit_step += (pt_share_e-sg_share_e) *r1;
				
						// ** ENERGY SHARING or not -,- **
						var seg_energy = seg_step+seg_side_step;
						var point_energy = points[p].step+points[p].side_step;
						var step_dot = ( points[p].step*0.01f+points[p].side_step).LimitLength().Dot(seg_step.LimitLength() );
					
						// **collision dot**
						var col_dot = (points[p].step.LimitLength()).Dot(dir.LimitLength());
					}
				} // :i
			} // :n
		} // :p
	} // f()
}
