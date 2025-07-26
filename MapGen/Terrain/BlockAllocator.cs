using System;

namespace MapGen.Terrain;

public class BlockAllocator<T> : IDisposable
{
    private const int SUBBLOCK_NODES = 16;
    
    public class SubNode
    {
        public T Data { get; set; }
        public SubNode Next { get; set; }
        
        public SubNode()
        {
            Data = default(T);
        }
        
        public SubNode(T data)
        {
            Data = data;
        }
    }
    
    private class SubBlock
    {
        public SubBlock Next { get; set; }
        public SubNode[] Nodes { get; set; }
        
        public SubBlock()
        {
            Nodes = new SubNode[SUBBLOCK_NODES];
            for (int i = 0; i < SUBBLOCK_NODES; i++)
            {
                Nodes[i] = new SubNode();
            }
        }
    }
    
    // List items
    private SubBlock _blockList;
    private SubNode _freeNodeList;
    
    // Constructor
    public BlockAllocator()
    {
        _blockList = null;
        _freeNodeList = null;
    }
    
    // Destructor equivalent
    ~BlockAllocator()
    {
        Dispose(false);
    }
    
    // WARNING: make sure you have shut down all InfoList's that reference us
    public void Reset()
    {
        SubBlock node = _blockList;
        while (node != null)
        {
            _blockList = node.Next;
            // In C#, we don't need to explicitly delete - GC handles it
            node = _blockList;
        }
        
        _freeNodeList = null;
    }
    
    private void AddBlock()
    {
        SubBlock node = new SubBlock();
        node.Next = _blockList;
        _blockList = node;
        
        // Link all nodes in the block into a free list
        for (int i = 0; i < SUBBLOCK_NODES - 1; i++)
        {
            node.Nodes[i].Next = node.Nodes[i + 1];
        }
        node.Nodes[SUBBLOCK_NODES - 1].Next = _freeNodeList;
        _freeNodeList = node.Nodes[0];
    }
    
    public SubNode Alloc()
    {
        SubNode result = _freeNodeList;
        if (result == null)
        {
            AddBlock();
            result = _freeNodeList;
        }
        _freeNodeList = _freeNodeList.Next;
        
        // Clear the next pointer for the allocated node
        result.Next = null;
        
        return result;
    }
    
    public SubNode Alloc(T data)
    {
        SubNode result = Alloc();
        result.Data = data;
        return result;
    }
    
    public void Free(SubNode node)
    {
        if (node != null)
        {
            // Clear the data (optional, for security/cleanup)
            node.Data = default(T);
            
            // Add back to free list
            node.Next = _freeNodeList;
            _freeNodeList = node;
        }
    }
    
    // IDisposable implementation
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Reset();
        }
    }
}
