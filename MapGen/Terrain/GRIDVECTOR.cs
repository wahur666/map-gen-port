using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace MapGen.Terrain;

public class GRIDVECTOR : IEquatable<GRIDVECTOR> {
	public static int GRIDSIZE = 4096;
	public static int HALFGRID = 2048;

	protected long X;
	protected long Y;
	
	public GRIDVECTOR(Vector2 vec) {
		X = (((long)vec.X * 4)+((GRIDSIZE-1)/2)) / GRIDSIZE;
		Y = (((long)vec.Y * 4)+((GRIDSIZE-1)/2)) / GRIDSIZE;
	}

	public GRIDVECTOR(Vector3 vector3) : this(new Vector2(vector3.X, vector3.Y)) {}
	
	public GRIDVECTOR init (float x, float y)		// in terms of squares
	{
		X = (long)x*4;		// leave these the same, round down expected
		Y = (long)y*4;
		return this;
	}
	
	public GRIDVECTOR quarterpos ()
	{
		X |= 1;
		Y |= 1;
		return this;
	}

	public GRIDVECTOR centerpos() {
		X &= ~3;
		X |= 2;
		Y &= ~3;
		Y |= 2;
		return this;
	}
	
	public GRIDVECTOR cornerpos ()
	{
		X &= ~3;
		Y &= ~3;
		return this;
	}
	
	public GRIDVECTOR zero ()
	{
		X = Y = 0;
		return this;
	}

	public Vector3 AsVector3() {
		return new Vector3(X * GRIDSIZE / 4, Y * GRIDSIZE / 4, 0);
	}

	public float getX() {
		return X * 0.25f;
	}

	public float getY() {
		return Y * 0.25f;
	}

	public int getIntX() {
		return (int)X >> 2;
	}

	public int getIntY() {
		return (int)Y >> 2;
	}

	public override bool Equals(object? vec) {
		if (vec is GRIDVECTOR vec1) {
			return vec1.X == X && vec1.Y == Y;
		}
		return false;
	}

	public bool Equals(GRIDVECTOR? other)
	{
		if (other is null) {
			return false;
		}

		if (ReferenceEquals(this, other)) {
			return true;
		}

		return X == other.X && Y == other.Y;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(X, Y);
	}

	public static bool operator ==(GRIDVECTOR a, GRIDVECTOR b) {
		return a.Equals(b);
	}
	
	public static bool operator != (GRIDVECTOR a, GRIDVECTOR b) {
		return !a.Equals(b);
	}

	public bool isZero() {
		return X == 0 && Y == 0;
	}
	
	public bool isMostlyEqual (GRIDVECTOR vec)
	{
		return (((X&~3)==(vec.X&~3)) && ((Y&~3)==(vec.Y&~3)));
	}

	public static GRIDVECTOR Create(Vector3 vector3) {
		return new GRIDVECTOR(vector3);
	} 
}
