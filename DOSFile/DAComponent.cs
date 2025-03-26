using DACOM;

namespace DOSFile;

public class DAComponent<T> where T : class, new() {
	public uint ref_count;
	private readonly T _instance;
	public T? I => _instance; // Expose the instance if needed
	public DAComponent ()
	{
		ref_count=1;
		_instance = new T(); // Instantiate the generic type
	}
}
