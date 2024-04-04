using Godot;
using System;

public partial class scene_preload : Node2D
{
	[Export] String[] paths;
	[Export] Vector3[] transforms;
	

	void preload_variables(){
		if (paths==null)return;
		for(int i=0;i<paths.Length;i++)
			spawn(paths[i], new Vector2(transforms[i].X, transforms[i].Y), transforms[i].Z);
	}
	void spawn(String PREFAB_PATH, Vector2 pos, float rot){
		GD.Print("spawn_from_path() ", PREFAB_PATH);
		var b = BodyTools.body_deserialize(PREFAB_PATH);

		if(Mathf.Abs(rot) > 0.001f ){
			GD.Print("rotated!");
			for(int i=0;i<b.points.Length;i++)
				b.points[i].pos = b.points[i].pos.Rotated(rot);
		}
		GetNode<PhysicsScene>("%scene").spawn_body(
			b,
			pos
		);
		GetNode<PhysicsScene>("%scene").QueueRedraw();
	}

	public void preload_childs(){

			var childs = GetChildren();
			for(int i=0; i<childs.Count; i++){
				if(! ((Node2D)childs[i]).Visible ) continue;
				if(((Node2D)childs[i]).Name.ToString().StartsWith("g_") ){
					var sub_childs = ((Node2D)childs[i]).GetChildren();
						for(int j=0; j<sub_childs.Count; j++){
							if(! ((Node2D)sub_childs[j]).Visible ) continue;
							var b = BodyTools.body_create_from_preview_node(((Node2D)childs[i]).GetChild<Node2D>(j), true);
							//if( Mathf.Abs( ((Node2D)childs[i]).RotationDegrees )> 1f)
							//	for(int k=0;k<b.points.Length;k++)
							//	b.points[k].pos = b.points[k].pos.Rotated( ((Node2D)childs[i]).Rotation );
							
							GetNode<PhysicsScene>("%scene").spawn_body(
								b,
								((Node2D)childs[i]).GlobalPosition
							);
							GetNode<PhysicsScene>("%scene").QueueRedraw();
						}
				}else{

					var b = BodyTools.body_create_from_preview_node(GetChild<Node2D>(i), true);
					//if( Mathf.Abs( ((Node2D)childs[i]).RotationDegrees )> 1f)
					//	for(int k=0;k<b.points.Length;k++)
					//	b.points[k].pos = b.points[k].pos.Rotated( ((Node2D)childs[i]).Rotation );
					
					GetNode<PhysicsScene>("%scene").spawn_body(
						b,
						((Node2D)childs[i]).GlobalPosition
					);
					GetNode<PhysicsScene>("%scene").QueueRedraw();
				}
			}
		
	}
	
	public override void _Ready()
	{
		if(!Visible) return;
		preload_variables();
		preload_childs();
		Hide();
	}
}


