namespace BigMission.ChannelManagement.Shared.Logic;

public class ComparisonStateMemoryRepository : IComparisonStateRepository
{
    private readonly Dictionary<int, ComparisonState> state = new();

    public Task<ComparisonState> GetStateAsync(int comparisonId)
    {
        _ = state.TryGetValue(comparisonId, out ComparisonState comparisonState);
        return Task.FromResult(comparisonState);
    }

    public Task SetStateAsync(ComparisonState cs)
    {
        state[cs.ComparisonId] = cs;
        return Task.CompletedTask;
    }
}
