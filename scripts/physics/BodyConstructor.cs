using Godot;
using System;
using System.Collections.Generic;
public static class BodyConstructor
{	const float SL = 16;
	const float AL = 24;
	const float LL = 24;
	public static P[] STICKMAN_POINTS = new P[]{
			// - base spine -
			new P( new Vector2( 0,  0    ) ), 	//[0] BASE
			new P( new Vector2( 0, -SL   ) ),	//[1] BASE_UP
			new P( new Vector2( 0, -SL*2 ) ),	//[2] BASE_UP_UP

			// - arms -
			new P( new Vector2( -AL  , -SL*2 ) ), //[3] left arm
			new P( new Vector2( -AL*2, -SL*2 ), 0.7f ), //[4] left forearm
			new P( new Vector2( AL  , -SL*2 ) ), //[5] right arm
			new P( new Vector2( AL*2, -SL*2 ), 0.7f  ), //[6] right forearm

			// - legs -
			new P( new Vector2( 0, LL   ) ), 	//[7] left leg
			new P( new Vector2( 0, LL*2 ), 0.7f ),	//[8] left foreleg
			new P( new Vector2( 0, LL ) ), 	//[9] right leg
			new P( new Vector2( 0, LL*2 ), 0.7f ), 	//[10] right foreleg

			// - head -
			new P( new Vector2( 0,    -SL*2-8 ) ), 	   //[11] neck
			new P( new Vector2(-8,    -SL*2-8-24 ) ),  //[12] head0 (back)
			new P( new Vector2( 0,    -SL*2-8-24*2 ) ),//[13] head1 (up)
			new P( new Vector2( 24,   -SL*2-8-24 ) ),  //[14] head2 (front)	
			new P( new Vector2( 24-8, -SL*2-8) ),      //[15] head3 (jaw)

			new P( new Vector2( 24-8*2, -SL*2-8-24) ),      //[16] head4 (center)

			new P( new Vector2( 0, LL*2+4 ) ), // [17] ground sensor 
		//	new P( new Vector2( +8, LL*2 ) ), // [18] ground sensor R
		};
	public static SG[] STICKMAN_SEGMENTS = new SG[]{
			// - base spine -
			new SG(0, 1), new SG(1, 2),
			// - arms -
			new SG(2, 3), new SG(3, 4),
			new SG(2, 5), new SG(5, 6),
			// - legs -
			new SG(0, 7), new SG(7, 8),
			new SG(0, 9), new SG(9, 10),

			// - head -
			new SG(2, 11),
			new SG(11, 12), new SG(12, 13), new SG(13, 14),
			new SG(14, 15), new SG(14, 15), new SG(15, 11),
		};
	public static CD[] STATIC_STICKMAN_SPRINGS_BASE = new CD[]{
			// - base spine -
			new CD(0, 1, SL), 
			new CD(1, 2, SL),
			// - arms -
			new CD(2, 3, AL), new CD(3, 4, AL),
			new CD(2, 5, AL), new CD(5, 6, AL),
			// - legs -
			new CD(0, 7, LL), 
			new CD(7, 8, LL),

			new CD(0, 9, LL), 
			new CD(9, 10,LL),

			// - head -
			new CD(2, 11, 8),
			new CD(11, 12, 16), new CD(12, 13, 16), new CD(13, 14, 16),
			new CD(14, 15, 16), new CD(15, 11, 16),
			// head center
			new CD(16, 11, 16), new CD(16, 12, 16), new CD(16, 13, 16),
			new CD(16, 14, 16), new CD(16, 15, 16),
		};

	public static v_body construct_stickman_body(){
		v_body b = new v_body();
		const float SL = 16;
		const float AL = 24;
		const float LL = 24;
		const float D = 0.999f;
		b.points = new P[]{
			// - base spine -
			new P( new Vector2( 0,  0    ) ,D,D),	//[0] BASE
			new P( new Vector2( 0, -SL   ) ,D,D),	//[1] BASE_UP
			new P( new Vector2( 0, -SL*2 ) ,D,D),	//[2] BASE_UP_UP

			// - arms -
			new P( new Vector2( -AL  , -SL*2 ) ,D,D), //[3] left arm
			new P( new Vector2( -AL*2, -SL*2 ) ,D,D), //[4] left forearm
			new P( new Vector2( AL  , -SL*2 ) ,D,D), //[5] right arm
			new P( new Vector2( AL*2, -SL*2 ),D, D), //[6] right forearm

			// - legs -
			new P( new Vector2( 0, LL   ) ,D,D), 	//[7] left leg
			new P( new Vector2( 0, LL*2 ) ,D,D),	//[8] left foreleg
			new P( new Vector2( 0, LL ) ,D,D), 	//[9] right leg
			new P( new Vector2( 0, LL*2 )  ,D,D), 	//[10] right foreleg

			// - head -
			new P( new Vector2( 0,    -SL*2-8 ) ,D,D), 	   //[11] neck
			new P( new Vector2(-8,    -SL*2-8-24 ) ,D,D),  //[12] head0 (back)
			new P( new Vector2( 0,    -SL*2-8-24*2 ) ,D,D),//[13] head1 (up)
			new P( new Vector2( 24,   -SL*2-8-24 ) ,D,D),  //[14] head2 (front)	
			new P( new Vector2( 24-8, -SL*2-8) ,D,D),      //[15] head3 (jaw)

			new P( new Vector2( 24-8*2, -SL*2-8-24) ,D,D),      //[16] head4 (center)

			new P( new Vector2( 0, LL*2-4 ) ,D,D), // [17] ground sensor 
		//	new P( new Vector2( +8, LL*2 ) ), // [18] ground sensor R
		};
		b.segments = new SG[]{
			// - base spine -
			new SG(0, 1), new SG(1, 2),
			// - arms -
			new SG(2, 3), new SG(3, 4),
			new SG(2, 5), new SG(5, 6),
			// - legs -
			new SG(0, 7), new SG(7, 8),
			new SG(0, 9), new SG(9, 10),

			// - head -
			new SG(2, 11),
			new SG(11, 12), new SG(12, 13), new SG(13, 14),
			new SG(14, 15), new SG(14, 15), new SG(15, 11),
		};
		b.springs = new CD[]{
			// - base spine -
			new CD(0, 1, SL), 
			new CD(1, 2, SL),
			// - arms -
			new CD(2, 3, AL), new CD(3, 4, AL),
			new CD(2, 5, AL), new CD(5, 6, AL),
			// - legs -
			new CD(0, 7, LL), 
			new CD(7, 8, LL),

			new CD(0, 9, LL), 
			new CD(9, 10,LL),

			// - head -
			new CD(2, 11, 8),
			new CD(11, 12, 16), new CD(12, 13, 16), new CD(13, 14, 16),
			new CD(14, 15, 16), new CD(15, 11, 16),
			// head center
			new CD(16, 11, 16), new CD(16, 12, 16), new CD(16, 13, 16),
			new CD(16, 14, 16), new CD(16, 15, 16),

			//new CD(17, 0,  LL*2+1),//
		};
		int CP = 17;
		Vector2 CPp = b.points[CP].pos;
		const float STF = 1.0f;
		b.pin_springs = new CDE[]{
			// - base spine -
			new CDE(0, CP, b.points[0].pos - CPp, STF),
			new CDE(1, CP, b.points[1].pos - CPp, STF),
			new CDE(2, CP, b.points[2].pos - CPp, STF),
			// - arms -
			new CDE(3, CP, b.points[3].pos - CPp, 0.5f), new CDE(4, CP, b.points[4].pos - CPp, 0.25f),
			new CDE(5, CP, b.points[5].pos - CPp, 0.5f), new CDE(6, CP, b.points[6].pos - CPp, 0.25f),
			// - legs -
			new CDE(7, CP, b.points[7].pos - CPp+new Vector2(4,0), STF), new CDE(8,  CP, b.points[8].pos - CPp, STF),
			new CDE(9, CP, b.points[9].pos - CPp+new Vector2(4,0), STF), new CDE(10, CP, b.points[10].pos - CPp, STF),

			// - head -
			new CDE(16, CP, b.points[16].pos - CPp, STF),

		};

		b.COLLISION_POINTS = b.points.Length;
		b.COLLISION_SEGMENTS = b.segments.Length;	
		b.MOVABLE_POINTS = b.points.Length;
		return b;
	}

		public static v_body construct_fixed_body(Vector2[] verts, float stiff, Vector2 fix_point){
			Int32 count = verts.Length;
			v_body b = new v_body();

			b.points = new P[count+1];
			for(int i=0; i<count; i++)
				b.points[i] = new P(verts[i], 0.000f, 0.0f);
				b.points[count] = new P(fix_point, 0.000f, 0.0f);
			
			b.segments = new SG[count];		
			for(int i=0; i<count-1; i++)
				b.segments[i] = new SG(i, i+1);
		    	b.segments[count-1] = new SG(count-1, 0);
			
			b.pin_springs = new CDE[count];
			for(int i=0; i<count; i++){
				b.pin_springs[i] = new CDE(i, count, b.points[i].pos - fix_point);
				b.pin_springs[i].stiff = stiff;
			}
			b.MOVABLE_POINTS = count;
			b.COLLISION_POINTS = count;

			return b;
		}
		public static v_body construct_polygon(Vector2[] verts, int[] composites){
			Int32 count = verts.Length;
			v_body b = new v_body();

			b.points = new P[count];
			for(int i=0; i<count; i++)
				b.points[i] = new P(verts[i]);

			b.segments = new SG[count];		
			for(int i=0; i<count-1; i++)
				b.segments[i] = new SG(i, i+1);
		    	b.segments[count-1] = new SG(count-1, 0);
			
			b.springs = new CD[count + composites.Length];		
			for(int i=0; i<count-1; i++)
				b.springs[i] = new CD(i, i+1, b.points[i].pos.DistanceTo( b.points[i+1].pos ) );
		    	b.springs[count-1] = new CD(count-1, 0, b.points[count-1].pos.DistanceTo( b.points[0].pos ) );
			
			
			b.COLLISION_POINTS = b.points.Length;
			b.MOVABLE_POINTS = b.points.Length;
			return b;
		}
}
