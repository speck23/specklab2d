using Godot;
using System;

public static class SceneTools 
{
  


    // *** FORCES ***
    public static void add_explosion_force(PhysicsScene ps, Vector2 pos, float range, float power, float damping = 0.99f){
		ps.explosion_forces[ps.active_explosion_forces].pos = pos;
		ps.explosion_forces[ps.active_explosion_forces].range = range;
		ps.explosion_forces[ps.active_explosion_forces].power = power;
		ps.explosion_forces[ps.active_explosion_forces].damping = damping;
		ps.explosion_forces[ps.active_explosion_forces].calculate_aabb();
		ps.active_explosion_forces++;
		if( ps.active_explosion_forces >=ps.explosion_forces.Length )
			ps.active_explosion_forces = 0;
	}
	public static void bodies_register_impulse_at(v_body[] bodies, int body_count, Vector2 pos, float power, float range){
	   for(int i = 0; i<body_count; i++)
		bodies[i].register_impulse_at(pos, power, range);
	}
	public static void bodies_register_body_impulse_at(v_body[] bodies, int body_count, Vector2 pos, float power, float range){
	   for(int i = 0; i<body_count; i++)
		bodies[i].register_body_impulse_at(pos, power, range);
	}
	

    public static float raycast(Vector2 a, Vector2 b, v_body[] bodies, out Vector2 hit){
		hit = Vector2.Inf;
		Vector2 best = Vector2.Zero;
		float closest = float.MaxValue;
		if(bodies != null)
		for(int i = 0; i<bodies.Length; i++){
			for(int s = 0; s<bodies[i].segments.Length; s++){
				if(GeometryTools.segment_segment_point(a,b, 
					bodies[i].points[ bodies[i].segments[s].a ].pos, 
					bodies[i].points[ bodies[i].segments[s].b ].pos, out hit )
				){				
					float d = a.DistanceSquaredTo(hit);

					if(d < closest){
						closest = d;
						best = hit;
					}
				}
			}
		}
		hit = best;
		return closest;
	}
    public static int bodies_get_closest_point_at(v_body[] bodies, Vector2 pos, out v_body b_owner, out float out_d){
		int best_b = 0;
		int best_p = -1;
		float best_d = float.MaxValue;
		for(int i = 0; i<bodies.Length; i++)
		if(bodies[i].aabb.has_point(pos))
		for(int j =0; j< bodies[i].points.Length; j++)
		{
			float d = pos.DistanceSquaredTo(bodies[i].points[j].pos);
			if(d < best_d){
				best_d = d;
				best_p = j;
				best_b = i;
			}
		}
		b_owner = bodies[best_b];
		out_d = best_d;
		return best_p;
	}
	public static int bodies_get_closest_segment_at(v_body[] bodies, Vector2 pos, out v_body b_owner, out float out_d){
		int best_b = 0;
		int best_s = -1;
		float best_d = float.MaxValue;
		for(int i = 0; i<bodies.Length; i++)
		if(bodies[i].aabb.has_point(pos))
		for(int j =0; j< bodies[i].segments.Length; j++)
		{	
			Vector2 hit;
			SG s =  bodies[i].segments[j];
			Vector2 pa = bodies[i].points[ s.a ].pos;
			Vector2 pb = bodies[i].points[ s.b ].pos;
			var d = GeometryTools.point_segment_distance(pos, pa, pb, out hit);

			if(d < best_d){
				best_d = d;
				best_s = j;
				best_b = i;
			}
		}
		b_owner = bodies[best_b];
		out_d = best_d;
		return best_s;
	}
	public static int bodies_get_closest_spring_at(v_body[] bodies, Vector2 pos, out v_body b_owner, out float out_d){
		int best_b = 0;
		int best_s = -1;
		float best_d = float.MaxValue;
		for(int i = 0; i<bodies.Length; i++)
		if(bodies[i].aabb.has_point(pos))
		if(bodies[i].springs != null)
		for(int j =0; j< bodies[i].springs.Length; j++)
		{	
			Vector2 hit;
			CD s =  bodies[i].springs[j];
			Vector2 pa = bodies[i].points[ s.a ].pos;
			Vector2 pb = bodies[i].points[ s.b ].pos;
			var d = GeometryTools.point_segment_distance(pos, pa, pb, out hit);

			if(d < best_d){
				best_d = d;
				best_s = j;
				best_b = i;
			}
		}
		b_owner = bodies[best_b];
		out_d = best_d;
		return best_s;
	}
}
