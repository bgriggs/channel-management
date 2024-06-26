﻿namespace BigMission.ChannelManagement.Logic;

public interface IComparisonStateRepository
{
    public Task SetStateAsync(ComparisonState state);
    public Task<ComparisonState?> GetStateAsync(int comparisonId);
}
