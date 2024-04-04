using Godot;
using System;
using System.Runtime.CompilerServices;

public static class GeometryTools 
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	static double distance(Vector2 a, Vector2 b){
		float dx = b.X-a.X;
		float dy = b.Y-a.Y;
		return Math.Sqrt(dx*dx + dy*dy);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float point_segment_distance(Vector2 p, Vector2 sa, Vector2 sb, out Vector2 scp)
	{
		Vector2 ab = sb-sa;
		Vector2 ap = p-sa;

		float proj = ap.Dot(ab);
		float ab_len_sq = ab.LengthSquared();
		float d = proj / ab_len_sq;

		if (d <= 0)
			scp = sa;
		else if (d >= 1)
			scp = ab;
		else 
			scp = sa + ab * d;
		//	GD.Print("()() dist: ", p.DistanceTo(scp));
		return p.DistanceTo(scp); //can be squared!
	}

	public static double segment_segment_distance(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        double dx1 = p1.X - p0.X;
        double dy1 = p1.Y - p0.Y;
        double dx2 = p3.X - p2.X;
        double dy2 = p3.Y - p2.Y;
        double dx3 = p0.X - p2.X;
        double dy3 = p0.Y - p2.Y;

        double dot = dx1 * dx2 + dy1 * dy2;
        double lenSq1 = dx1 * dx1 + dy1 * dy1;
        double lenSq2 = dx2 * dx2 + dy2 * dy2;

        if (lenSq1 == 0 || lenSq2 == 0)
        {
            return Math.Sqrt(Math.Min(lenSq1, lenSq2));
        }

        double param = (dx3 * dx1 + dy3 * dy1) / lenSq1;

        if (param <= 0)
        {
            return Math.Sqrt(dx3 * dx3 + dy3 * dy3);
        }
        else if (param >= 1)
        {
            return Math.Sqrt((p3.X - p1.X) * (p3.X - p1.X) + (p3.Y - p1.Y) * (p3.Y - p1.Y));
        }

        double xx = p0.X + param * dx1;
        double yy = p0.Y + param * dy1;

        return Math.Sqrt((p2.X - xx) * (p2.X - xx) + (p2.Y - yy) * (p2.Y - yy));
    }
	
	public static bool segment_segment_intersection(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		Vector2 ab = b - a;
		Vector2 cd = d - c;

		float crossABCD = ab.Cross(cd);
		float crossACAB = (a - c).Cross(ab);

		if (Math.Abs(crossABCD) < float.Epsilon)
		{
			if (Math.Abs(crossACAB) < float.Epsilon)
				return true;
			else
				return false;
		}
		float t = (c - a).Cross(cd) / crossABCD;
		float u = (c - a).Cross(ab) / crossABCD;
		if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
			return true;
		else
			return false;
	}

	public static bool ray_segment_intersection(Vector2 ro, Vector2 rd, Vector2 sa, Vector2 sb)
	{
		float x1 = sa.X;
		float y1 = sa.Y;
		float x2 = sb.X;
		float y2 = sb.Y;
		float x3 = ro.X;
		float y3 = ro.Y;
		float x4 = ro.X + rd.X;
		float y4 = ro.Y + rd.Y;

		float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);

		if (denominator == 0)
		{
			return false;
		}

		float t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denominator;
		float u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denominator;

		return t >= 0 && t <= 1 && u >= 0 && u <= 1;
	}

	public static bool segment_segment_point(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 ret_p)
	{
		float denominator = (b.X - a.X) * (d.Y - c.Y) - (b.Y - a.Y) * (d.X - c.X);
		
		//parallel
		if (denominator == 0.0) {
			ret_p = Vector2.Zero;
			return false;
		}
		
		float t = ((c.X - a.X) * (d.Y - c.Y) - (c.Y - a.Y) * (d.X - c.X)) / denominator;
		float u = ((c.X - a.X) * (b.Y - a.Y) - (c.Y - a.Y) * (b.X - a.X)) / denominator;
		
		if (t >= 0.0 && t <= 1.0 && u >= 0.0 && u <= 1.0) {
			ret_p = a + t * (b - a);
		} else {
			ret_p = Vector2.Zero;
			return false;
		}
		
		return true;
	}

	public static bool point_in_triangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c){
		float s = (a.X-c.X) * (p.Y-c.Y) - (a.Y-c.Y) * (p.X-c.X);
		float t = (b.X-a.X) * (p.Y-a.Y) - (b.Y-a.Y) * (p.X-a.X);

		if( (s<0) != (t<0) && s != 0 && t!= 0)
			return  false;
		float d = (c.X - b.X) * (p.Y - b.Y) - (c.Y - b.Y) * (p.X - b.X);
		return d==0 || (d<0) == (s+t <=0);
	}
	public static bool point_in_triangle2(Vector2 p, Vector2 a, Vector2 b, Vector2 c){
		float dx = p.X-c.X;
		float dy = p.Y-c.Y;
		float dx21 = c.X-b.X;
		float dy12 = b.Y-c.Y;
		float det = dy12*(a.X-c.X) + dx21*(a.Y-c.Y);
		float s = dy12*dx + dx21*dy;
		float t = (c.Y-a.Y)*dx + (a.X-c.X)*dy;
		if(det<0) return ( s<=0 && t<=0 && s+t>=0 );
		return s>=0 && t>=0 && s+t<=0;
	}

	//
	public static Vector2 point_rotated_around(Vector2 point, Vector2 pivot, float r_angle){
		float sin = MathF.Sin( r_angle );
		float cos = MathF.Cos( r_angle );
		point -= pivot;
		Vector2 rotated = new Vector2(
			point.X * cos - point.Y * sin,
			point.X * sin + point.Y * cos
		);
		return rotated + pivot;
	}
}
