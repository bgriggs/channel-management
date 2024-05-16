namespace BigMission.ChannelManagement.Math;

public interface IMathRepository
{
    public Task<IEnumerable<MathParameters>> GetParameters();
}
