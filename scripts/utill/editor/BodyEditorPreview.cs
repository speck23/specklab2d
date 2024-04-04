using Godot;
using System;

[Tool]
public partial class BodyEditorPreview : Node2D
{	

	[Export] Color[] COLOR_LUT;
	[Export] bool AUTO_UPDATE = false;
	[Export] bool SHOW_POINT_NUMBERS = false;

	[Export] int max_hits = 2;
	[Export] int movable_points = 0;
	[Export] int collision_points = 0;
	[Export] Color base_draw_color = Color.Color8(172, 172, 172, 255);
	[Export] Vector3[] segments;
	[Export] int collision_segments = -1;
	[Export] Vector3[] springs;
	[Export] Vector2[] pin_springs_ab;
	[Export] Vector3[] pin_springs_os;

	[Export] Vector3[] point_motors;
	[Export] Vector3[] draw_list;

	[Export] bool PREGEN_CHAINED = false;
	[Export] Font font = new SystemFont();
	[Export] bool SERIALIZE = false;
	[Export] bool SERIALIZE_PNG = false;

	[Export] bool USE_SCENE_COLOR_LUT = false;
	public override void _Process(double delta)
	{
		if(AUTO_UPDATE) QueueRedraw();
		if(SERIALIZE_PNG){
			SERIALIZE_PNG = false;
			snapshot();
		}
		if(SERIALIZE){
			SERIALIZE = false;
			var b = BodyTools.body_create_from_preview_node(this, true);
			BodyTools.body_serialize(b,  "res://serialized/bodies/"+Name+".pb");
		}
	}

	public override void _Draw()
	{
		var p = GetNode<Node2D>("p");
		var ps = p.Scale;

		// *** DRAWING ***
		if(springs!=null)
		foreach(Vector3 s in springs)
			DrawLine(ps*p.GetChild<Node2D>((int)s.X).Position, ps*p.GetChild<Node2D>((int)s.Y).Position, Color.Color8(24,24,224,255), 3.0f );
		

		if(segments!=null)
		foreach(Vector3 s in segments){
			var col = COLOR_LUT[(int)s.Z];
			if(USE_SCENE_COLOR_LUT)
				col = GetNode<PhysicsScene>("%scene").COLOR_LUT[(int)s.Z];
			DrawLine(ps*p.GetChild<Node2D>((int)s.X).Position, ps*p.GetChild<Node2D>((int)s.Y).Position, col, 8.0f );
		}
		int ni = 0;
		foreach(Node2D n in p.GetChildren())
		{
			DrawCircle(ps*n.Position, 4.0f, Color.Color8(128,64,64,255));
			if(SHOW_POINT_NUMBERS)
				DrawString(font, ps*n.Position, ""+(ni++));
		}
		
		// fixed points
		for(int i=movable_points; i < p.GetChildCount(); i++)
			DrawRect(new Rect2( ps*p.GetChild<Node2D>(i).Position-Vector2.One*5f, 10.0f, 10.0f), Color.Color8(224,64,64,255));

		// *** AUTO CHAINED PREGEN ***
		if(PREGEN_CHAINED){
			PREGEN_CHAINED=false;
			var p_len = p.GetChildCount();
			segments = new Vector3[p_len];
			springs = new Vector3[p_len];
		
			for(int j=0; j<p_len-1;j++){
				segments[j] = new Vector3(j, j+1, 0);
				springs[j] = new Vector3(j, j+1, 1.0f);	
			}
		}



		if(collision_segments == -1)
			collision_segments = segments.Length;
	}
	public void snapshot(){
		var b = BodyTools.body_create_from_preview_node(this, false);
		b.update_aabb();
		BodyTools.body_save_preview(b, GetViewport(), "res://serialized/bodies/"+Name+".png", Vector2.Zero);
	}
}
