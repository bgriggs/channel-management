namespace BigMission.ChannelManagement;

public interface IChannelMappingRepository
{
    public Task<ChannelMapping> GetChannelMappingAsync(int channelId);
}
