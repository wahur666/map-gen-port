using System.Collections;

namespace MapGen;

public class InfoIterator : IEnumerator<FootprintInfo> {
	private BlockAllocator<FootprintInfo>.SubNode _node;
	private BlockAllocator<FootprintInfo>.SubNode _prev;
	private readonly InfoList<FootprintInfo> _list;
	private readonly BlockAllocator<FootprintInfo> _blockAlloc;

	public InfoIterator(InfoList<FootprintInfo> list, BlockAllocator<FootprintInfo> blockAlloc) {
		_list = list;
		_blockAlloc = blockAlloc;
		Reset();
	}

	public void Reset() {
		_prev = null;
		_node = _list.List;
	}

	// Equivalent to operator bool()
	public bool IsValid => _node != null;

	// Equivalent to operator++()
	public FootprintInfo MoveNext() {
		_prev = _node;
		if (_node != null)
			_node = _node.Next;

		return _node?.Data;
	}

	// Equivalent to operator->()
	public FootprintInfo Current => _node?.Data;

	// Remove current element and advance to next
	public void Remove() {
		if (_node is not null) {
			if (_prev is not null) {
				_prev.Next = _node.Next;
				if (_prev.Next is null)
					_list.End = _prev;
			} else {
				_list.List = _node.Next;
				if (_list.List is null)
					_list.End = null;
			}

			_blockAlloc.Free(_node);

			_node = _prev != null ? _prev.Next : _list.List;
		}
	}

	// IEnumerator implementation
	object IEnumerator.Current => Current;

	bool IEnumerator.MoveNext() {
		MoveNext();
		return IsValid;
	}

	void IEnumerator.Reset() {
		Reset();
	}

	public void Dispose() {
		// No unmanaged resources to dispose
	}
}
