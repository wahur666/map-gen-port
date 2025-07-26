namespace MapGen.Terrain;

public class PriorityQueue {
	PathNode? pFirst = null;
	PathNode? pLast = null;

	uint _uniqueId;

	public void set_id (uint id)
	{
		_uniqueId = id;
	}

	public bool empty ()
	{
		return pFirst is null;
	}

	public void push (PathNode pNode)
	{
		pNode.priorityID = _uniqueId;

		if (pFirst is null)
		{
			// add the first object to the list
			pNode.pNext = null;
			pNode.pPrev = null;
			pFirst = pNode;
			pLast = pFirst;
			return;
		}

		// go through the linked list and insert the node in order
		PathNode p = pFirst;

		while (p is not null)
		{
			if (pNode.f <= p.f)
			{
				// insert the node

				// special case if we're to insert at the front of the list
				if (p == pFirst)
				{
					pNode.pPrev = null;
					pNode.pNext = p;
					p.pPrev = pNode;
					pFirst = pNode;
					return; 
				}

				pNode.pNext = p;
				pNode.pPrev = p.pPrev;

				pNode.pPrev.pNext = pNode;
				pNode.pNext.pPrev = pNode;
				return;
			}
			p = p.pNext;
		}

		// if we're here, add it to the back
		pNode.pNext = null;
		pNode.pPrev = pLast;
		pLast.pNext = pNode;
		pLast = pNode;
	}

	PathNode pop () {
		// take the first item off the list
		if (pFirst is not null)
		{
			PathNode pNode = pFirst;
			
			pFirst = pFirst.pNext;
			if (pFirst is not null)
			{
				pFirst.pPrev = null;
			}
			
			pNode.pNext = null;

			MapGen.CQASSERT(pNode.pPrev is null);
			pNode.priorityID = 0;
			return pNode;
		}

		return null;
	}

	void flush (uint id)
	{
		// empty the list
		pFirst = null;
		pLast = null;
		set_id(id);
	}

	bool contains (PathNode pNode)
	{
		return (pNode.priorityID == _uniqueId);
	}

	void remove (PathNode pNode)
	{
		PathNode p = pFirst;

		while (p is not null)
		{
			if (p.currentCell.Equals(pNode.currentCell))
			{
				// remove 'p' from the linked list

				if (p == pFirst)
				{
					// special case:  'p' is first on the list
					pFirst = p.pNext;
					
					if (pFirst is not null)
					{
						pFirst.pPrev = null;
					}
				}
				else if (p == pLast)
				{
					// special case:  'p' is last on the list
					pLast = p.pPrev;

					if (pLast is not null)
					{
						pLast.pNext = null;
					}
				}
				else
				{
					p.pPrev.pNext = p.pNext;
					p.pNext.pPrev = p.pPrev;
				}

				p.pNext = null;
				p.pPrev = null;
				p.priorityID = 0;
				return;
			}
			p = p.pNext;
		}
	}
}
