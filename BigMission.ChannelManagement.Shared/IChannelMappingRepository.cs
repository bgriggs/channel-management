namespace BigMission.ChannelManagement.Shared;

public interface IChannelMappingRepository
{
    public Task<ChannelMapping> GetChannelMappingAsync(int channelId);
}
