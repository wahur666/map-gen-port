using System.Collections;

namespace MapGen.Terrain;

public class InfoIteratorEnumerable : IEnumerable<FootprintInfo>
{
	private readonly InfoList<FootprintInfo> _list;
    
	public InfoIteratorEnumerable(InfoList<FootprintInfo> list)
	{
		_list = list;
	}
    
	public IEnumerator<FootprintInfo> GetEnumerator()
	{
		var current = _list.List;
		while (current is not null)
		{
			yield return current.Data;
			current = current.Next;
		}
	}
    
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
