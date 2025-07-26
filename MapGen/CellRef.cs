namespace MapGen;

public class CellRef(int x, int y) : IEquatable<CellRef> {

	public static CellRef[] gCellDirections = {
		new (0, 1), 
		new (1, 0), 
		new (0, -1), 
		new (-1, 0), 
		new (1, 1), 
		new (1, -1), 
		new (-1, -1),
		new (-1, 1)
	};

	public static float[] gCellCosts = [TerrainMap.AXIAL_LENGTH, TerrainMap.DIAGONAL_LENGTH];
	
	public int X = x;
	public int Y = y;

	public CellRef() : this(0, 0) {
	}

	// public CellRef operator += (CellRef c) {
	// 	X += c.x;
	// 	Y += c.y;
	// 	return this;
	// }

	public static CellRef operator +(CellRef rhs, CellRef lhs)
	{
		return new CellRef(rhs.X + lhs.X, rhs.Y + lhs.Y);
	}

	public bool Equals(CellRef? other)
	{
		if (other is null) {
			return false;
		}

		if (ReferenceEquals(this, other)) {
			return true;
		}

		return X == other.X && Y == other.Y;
	}

	public override bool Equals(object? obj)
	{
		if (obj is null) {
			return false;
		}

		if (ReferenceEquals(this, obj)) {
			return true;
		}

		if (obj.GetType() != GetType()) {
			return false;
		}

		return Equals((CellRef)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(X, Y);
	}
}
