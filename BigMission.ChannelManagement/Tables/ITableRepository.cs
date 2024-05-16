namespace BigMission.ChannelManagement.Shared.Tables;

public interface ITableRepository
{
    public Task<IEnumerable<TableMapping>> GetMappingsAsync();
}
