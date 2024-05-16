namespace BigMission.ChannelManagement;

public interface IChannelRepository
{
    public Task<ChannelValue> GetChannelValueAsync(int channelId);
    public Task SetChannelValueAsync(ChannelValue ch);
}
