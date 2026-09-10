public class HistoryManager
{
    public class HistoryManager
{
    public List<HistoryEntry> HistoryEntries { get; set; } = new List<HistoryEntry>();

    // 1. Load history from CSV
    public void LoadHistory(string filePath)
    {
     
        if (!File.Exists(filePath))
        {
            //File doesn't exist yet, just return empty list
            return;
        }        
    }

    // 2. Save history to CSV
    public void SaveHistory(string filePath) { }

    // 3. Check if a show aired recently
    public bool HasRecentlyAired(string showName, int weeks) { }
}
}