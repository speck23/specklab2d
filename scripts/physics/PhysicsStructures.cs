using Godot;
using System;
using System.Runtime.CompilerServices;

public struct P{
	public Vector2 pos;
	public Vector2 step = Vector2.Zero;
	public Vector2 side_step = Vector2.Zero;

	public Vector2 hit_step = Vector2.Zero;
	public float velocity = 1.0f;
	public float mass = 1.0f;
	
	public Vector2 shift=Vector2.Zero;

	public Vector2 step_last=Vector2.Zero;
	public float drag = 0.99f;
	public float drag_max = 0.99f;

	public bool colided = false;

	// *** debug ***
	public Vector2 dbl_side_step = Vector2.Zero;
	public Vector2 dbl_shift = Vector2.Zero;
	public Vector2 dbl_step = Vector2.Zero;
	public Vector2 db_shift_bounce = Vector2.Zero;
	//

	public P(Vector2 _position, Vector2 _step){
		pos = _position;
		step = _step;
	}
	public P(Vector2 _position, Vector2 _step, float _drag){
		pos = _position;
		step = _step;
		drag = _drag;
	}
	public P(Vector2 _position){
		pos = _position;
	}
	public P(Vector2 _position, float _drag){
		pos = _position;
		drag = _drag;
	}
	public P(Vector2 _position, float _drag, float _drag_max){
		pos = _position;
		drag = _drag;
		drag_max = _drag_max;
	}
};

public struct CD{
	public int a, b;
	public float dist;
	public float stiff = 1.0f;
	public float energy = 0f;
	public CD(int _a, int _b, float _distance){
		a = _a;
		b = _b;
		dist = _distance;
	}
};

public struct CDE{
	public int a;
	public int pin_point;
	public Vector2 pin_offset;

	public float stiff = 1f;
	public CDE(int _a, int _pin_point,  Vector2 _pin_offset){
		a = _a;
		pin_point = _pin_point;
		pin_offset = _pin_offset;
	}
		public CDE(int _a, int _pin_point,  Vector2 _pin_offset, float _stiff){
		a = _a;
		pin_point = _pin_point;
		pin_offset = _pin_offset;
		stiff = _stiff;
	}
};

public struct SG{
	public int a, b;
	public int type=0;
	public SG(int _a, int _b){
		a = _a;
		b = _b;
	}
};

public struct PM{
	public int a;
	public int b;
	public float f;

	public PM(int _a, int _b, float _f){
		a = _a;
		b = _b;
		f = _f;
	}
};
public struct DL{
	public int a;
	public int b;

	public int type;
	public int val;

	public DL(int _type, int _a, int _b, int _val){
		a = _a;
		b = _b;
		type = _type;
		val = _val;
	}
};
public struct AABB{
	public float min_x;
	public float max_x;
	public float min_y;
	public float max_y;

	public AABB(float _min_x, float _max_x, float _min_y, float _max_y){
		min_x = _min_x;
		max_x = _max_x;
		min_y = _min_y;
		max_y = _max_y;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool has_point(Vector2 point){
		return point.X >= min_x && point.X < max_x && point.Y > min_y && point.Y < max_y;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool intersect(AABB a, AABB b){
		if(a.max_x <= b.min_x || b.max_x <= a.min_x ||
		   a.max_y <= b.min_y || b.max_y <= a.min_y )
		   return false;
		return true;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool intersect(AABB b){
		if(max_x <= b.min_x || b.max_x <= min_x ||
		   max_y <= b.min_y || b.max_y <= min_y )
		   return false;
		return true;
	}

	public void grow(int amount){
		min_x -= amount;
		max_x += amount;
		min_y -= amount;
		max_y += amount;
	}
	public float get_w(){
		return max_x-min_x;
	}
	public float get_h(){
		return max_y-min_y;
	}
};

public struct ExplosionForce{
	public Vector2 pos;
	public float range;
	public float power;
	public float damping;
	public AABB aabb;

    public ExplosionForce(Vector2 _pos, float _range, float _power, float _damping = 1.0f)
    {
		pos = _pos;
		range = _range;
		power = _power;
		damping = _damping;
		aabb = new AABB(pos.X-range, pos.X+range, pos.Y-range, pos.Y+range);
    }
	public void calculate_aabb(){
		aabb = new AABB(pos.X-range, pos.X+range, pos.Y-range, pos.Y+range);
	}
}

public struct SpicyBody{
	int index_start;
	int index_count;
	AABB box;
};
public static class SpicyPhysics
{
}
