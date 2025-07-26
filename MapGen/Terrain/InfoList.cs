namespace MapGen.Terrain;

public class InfoList<T> {
	public BlockAllocator<FootprintInfo>.SubNode List = null;
	public BlockAllocator<FootprintInfo>.SubNode End = null;

	~InfoList() {
		Reset();
	}

	void Reset() {
		BlockAllocator<FootprintInfo>.SubNode node = List;
		while (node is not null) {
			List = node.Next;
			TerrainMap._blockAllocator.Free(node);
			node = List;
		}

		End = null;
	}

	public void Add(BlockAllocator<FootprintInfo>.SubNode data) {
		BlockAllocator<FootprintInfo>.SubNode node = TerrainMap._blockAllocator.Alloc();

		node = data;

		node.Next = null;
		if (End is null) {
			End = node;
			List = node;
		} else {
			End.Next = node;
			End = node;
		}
	}
}
