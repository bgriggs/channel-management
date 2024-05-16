﻿namespace BigMission.ChannelManagement.Timer;

public interface ITimerRepository
{
    public Task<IEnumerable<TimerParameters>> GetTimersAsync();
    public Task SaveTimersAsync(IEnumerable<TimerParameters> timers);
    public Task<TimerState> GetTimerStateAsync(int timerId);
    public Task SetTimerStateAsync(TimerState timerState);
}
