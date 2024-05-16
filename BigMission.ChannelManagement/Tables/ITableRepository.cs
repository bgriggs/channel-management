namespace BigMission.ChannelManagement.Tables;

public interface ITableRepository
{
    public Task<IEnumerable<TableMapping>> GetMappingsAsync();
}
