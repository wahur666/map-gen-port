namespace MapGen;

public class FootprintList {
	private readonly InfoList<FootprintInfo> _fpInfoList;
    private readonly BlockAllocator<FootprintInfo> _blockAlloc;
    
    private int _count;
    private uint _allFlags;
    
    // Needed for path finding
    public PathNode Node { get; set; }
    
    public FootprintList()
    {
        _fpInfoList = new InfoList<FootprintInfo>();
        _blockAlloc = new BlockAllocator<FootprintInfo>();
        _count = 0;
        _allFlags = 0;
        Node = new PathNode();
    }
    
    public void Add(FootprintInfo fpInfo) {
	    var data = new BlockAllocator<FootprintInfo>.SubNode(fpInfo, null);
        _fpInfoList.Add(data);
        _count++;
        
        _allFlags |= fpInfo.Flags;
    }
    
    public void Undo(FootprintInfo fpInfo)
    {
        var it = new InfoIterator(_fpInfoList, _blockAlloc);
        
        while (it.IsValid)
        {
            if (it.Current.MissionID == fpInfo.MissionID)
            {
                // Delete it from the list
                _count--;
                it.Remove();
            }
            else
            {
                it.MoveNext();
            }
        }
        ResetFlags();
    }
    
    public int GetFootprintCount()
    {
        return _count;
    }
    
    public void ResetFlags()
    {
        _allFlags = 0;
        var it = new InfoIterator(_fpInfoList, _blockAlloc);
        
        while (it.IsValid)
        {
            _allFlags |= it.Current.Flags;
            it.MoveNext();
        }
    }
    
    public uint GetMissionFlags(uint missionID, uint cornerID = 0)
    {
        uint retFlag = 0;
        var it = new InfoIterator(_fpInfoList, _blockAlloc);
        
        while (it.IsValid)
        {
            // Get all flags associated with the mission ID
            if (cornerID != 0)
            {
                if ((it.Current.MissionID == missionID) && 
                    (it.Current.Flags & cornerID) != 0)
                {
                    retFlag |= it.Current.Flags;
                }
            }
            else
            {
                if (it.Current.MissionID == missionID)
                {
                    retFlag |= it.Current.Flags;
                }
            }
            
            it.MoveNext();
        }
        return retFlag;
    }
    
}
