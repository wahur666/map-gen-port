namespace MapGen;

public class BlockAllocator<T> {
	public const int SUBBLOCK_NODES = 16;
	
	public class SubNode(T data, SubNode next) {
		public T Data { get; set; } = data;
		public SubNode Next { get; set; } = next;
	}

	public class SubBlock {
		public SubBlock Next;
		public SubNode[] node = new SubNode[16];
	}

	//
	// list items
	// 
	public SubBlock pBlockList;

	public SubNode pFreeNodeList;
	//
	// methods
	//

	~BlockAllocator() {
		reset();
	}

	// WARNING: make sure you have shut down all FootprintList's that reference us
	public void reset() {
		SubBlock node = pBlockList;
		while (node is not null) {
			pBlockList = node.Next;
			node = pBlockList;
		}

		pFreeNodeList = null;
	}

	void addBlock() {
		SubBlock node = new SubBlock {
			Next = pBlockList
		};
		pBlockList = node;

		for (var i = 0; i < SUBBLOCK_NODES - 1; i++) {
			node.node[i].Next = node.node[i + 1];
		}

		node.node[SUBBLOCK_NODES - 1].Next = pFreeNodeList;
		pFreeNodeList = node.node[0];
	}

	public SubNode alloc() {
		SubNode result = pFreeNodeList;
		if (result is null) {
			addBlock();
			result = pFreeNodeList;
		}

		pFreeNodeList = pFreeNodeList.Next;
		return result;
	}

	public void Free(SubNode node) {
		node.Next = pFreeNodeList;
		pFreeNodeList = node;
	}
}
