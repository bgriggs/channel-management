using BigMission.ChannelManagement.Shared.Logic;
using BigMission.TestHelpers;

namespace BigMission.ChannelManagement.Shared.Timer
{
    /// <summary>
    /// Runs and manages timer instances.
    /// </summary>
    public class TimerEvaluation
    {
        private readonly ITimerRepository timerRepository;
        private readonly IChannelRepository channelRepository;
        private readonly IDateTimeHelper dateTime;
        private readonly IStatementRepository statementRepository;
        private readonly LogicEvaluation logicEvaluation;

        public TimerEvaluation(ITimerRepository timerRepository, IChannelRepository channelRepository, IChannelMappingRepository channelMappingRepository,
            IDateTimeHelper dateTime, IStatementRepository statementRepository, IComparisonStateRepository comparisonStateRepository)
        {
            this.timerRepository = timerRepository;
            this.channelRepository = channelRepository;
            this.dateTime = dateTime;
            this.statementRepository = statementRepository;

            logicEvaluation = new LogicEvaluation(channelRepository, comparisonStateRepository, channelMappingRepository, dateTime);
        }


        /// <summary>
        /// Increment timers.
        /// </summary>
        public async Task UpdateTimers()
        {
            var parameters = await timerRepository.GetTimersAsync();
            foreach (var parameter in parameters)
            {
                var outputCh = await channelRepository.GetChannelValueAsync(parameter.OutputChId);
                var startStatements = await statementRepository.GetStatementsAsync(parameter.StartStatementId);
                var stopStatements = await statementRepository.GetStatementsAsync(parameter.StopStatementId);
                var timerState = await timerRepository.GetTimerStateAsync(parameter.Id);

                // Timer is not running, check start conditions
                if (timerState.Started == null)
                {
                    var startActive = await logicEvaluation.Evaluate(startStatements);
                    if (startActive)
                    {
                        timerState.Started = dateTime.UtcNow;
                        await timerRepository.SetTimerStateAsync(timerState);
                        
                        int output = outputCh.GetValueInt();
                        if (parameter.EnableStartSeconds)
                        {
                            output = parameter.StartSeconds;
                        }

                        timerState.StartValue = output;
                        await timerRepository.SetTimerStateAsync(timerState);

                        // Update output channel
                        outputCh.SetBaseValue(output);
                        await channelRepository.SetChannelValueAsync(outputCh);
                    }
                }
                // Timer is running
                else
                { 
                    var diff = dateTime.UtcNow - timerState.Started.Value;
                    int output = (int)diff.TotalSeconds;

                    // Increment timer
                    if (parameter.CountDown)
                    {
                        output = timerState.StartValue - output;
                        if (parameter.EnableRollover && output < parameter.RolloverSeconds)
                        {
                            // Determine the available range and get remainder on total seconds
                            var timerRange = parameter.StartSeconds - parameter.RolloverSeconds;
                            var overflow = timerRange - (output % timerRange);
                            if (parameter.EnableStartSeconds)
                            {
                                output = parameter.StartSeconds - overflow;
                            }
                        }
                    }
                    else // Count up
                    {
                        output = timerState.StartValue + output;
                        if (parameter.EnableRollover && output > parameter.RolloverSeconds)
                        {
                            var overflow = output % parameter.RolloverSeconds;
                            if (parameter.EnableStartSeconds)
                            {
                                output = parameter.StartSeconds + overflow;
                            }
                            else
                            {
                                output = overflow;
                            }
                        }
                    }

                    // Check stop conditions
                    if (stopStatements.StatementRows?.Any() ?? false)
                    {
                        var stopActive = await logicEvaluation.Evaluate(stopStatements);
                        if (stopActive)
                        {
                            timerState.Started = null;

                            if (parameter.EnableStopSeconds)    
                            {
                                output = parameter.StopSeconds;
                            }
                        }
                    }

                    if (output < 0)
                    {
                        output = 0;
                    }

                    // Update output channel
                    outputCh.SetBaseValue(output);
                    await channelRepository.SetChannelValueAsync(outputCh);
                }
            }
        }
    }
}
