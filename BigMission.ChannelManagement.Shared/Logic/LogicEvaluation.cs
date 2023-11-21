using BigMission.TestHelpers;
using UnitsNet;

namespace BigMission.ChannelManagement.Shared.Logic;

/// <summary>
/// Determines whether a statement is true or false. 
/// A statement includes a list of AND conditions and rows that are OR'd.
/// </summary>
public class LogicEvaluation
{
    private readonly IChannelRepository channelRepository;
    private readonly IComparisonStateRepository comparisonRepository;
    private readonly IChannelMappingRepository channelMappingRepository;
    private readonly IDateTimeHelper dateTime;

    public LogicEvaluation(IChannelRepository channelRepository, IComparisonStateRepository comparisonRepository, IChannelMappingRepository channelMappingRepository, IDateTimeHelper dateTime)
    {
        this.channelRepository = channelRepository;
        this.comparisonRepository = comparisonRepository;
        this.channelMappingRepository = channelMappingRepository;
        this.dateTime = dateTime;
    }

    public async Task<bool> Evaluate(Statements statements)
    {
        foreach (var statement in statements.StatementRows)
        {
            // All conditions in a statement must be true
            bool statementResult = true;
            foreach (var condition in statement)
            {
                var result = await CheckComparison(condition);
                statementResult &= result;
                if (!result) { break; }
            }

            // Any row can be true for the statement to be true
            if (statementResult)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> CheckComparison(Comparison comp)
    {
        var state = await comparisonRepository.GetStateAsync(comp.ComparisonId) ?? new ComparisonState { ComparisonId = comp.ComparisonId };
        var channel = await channelRepository.GetChannelValueAsync(comp.ChannelId);
        var mapping = await channelMappingRepository.GetChannelMappingAsync(channel.Id);
        var chQuantity = channel.GetDisplayQuantity(mapping);

        // Resolve value to compare with
        IQuantity checkQuantity = null;
        if (comp.UseStaticComparison)
        {
            var v = double.Parse(comp.StaticValueComparison);
            checkQuantity = Quantity.From(v, mapping.DisplayUnitType);
        }
        else if (comp.ChannelComparisonId > 0)
        {
            var compCh = await channelRepository.GetChannelValueAsync(comp.ChannelComparisonId);
            var compMap = await channelMappingRepository.GetChannelMappingAsync(compCh.Id);
            checkQuantity = compCh.GetDisplayQuantity(compMap);
        }

        // Apply logic
        bool result = false;
        switch (comp.Logic)
        {
            case LogicType.GreaterThan:
                result = chQuantity.Value > checkQuantity.Value;
                break;
            case LogicType.GreaterThanOrEqualTo:
                result = chQuantity.Value >= checkQuantity.Value;
                break;
            case LogicType.LessThan:
                result = chQuantity.Value < checkQuantity.Value;
                break;
            case LogicType.LessThanOrEqualTo:
                result = chQuantity.Value <= checkQuantity.Value;
                break;
            case LogicType.EqualTo:
                result = chQuantity.Value == checkQuantity.Value;
                break;
            case LogicType.True:
                result = chQuantity.Value > 0;
                break;
            case LogicType.False:
                result = chQuantity.Value <= 0;
                break;
            case LogicType.ChangedBy:
                if (!string.IsNullOrEmpty(state.LastValue))
                {
                    var v = double.Parse(state.LastValue);
                    Quantity.TryFrom(v, mapping.DisplayUnitType, out IQuantity last);
                    var diff = System.Math.Abs(((double)chQuantity.Value) - (double)last.Value);
                    result = diff >= checkQuantity.Value;
                }
                else
                {
                    result = false;
                }
                break;
            // Value changed
            case LogicType.Updated:
                if (!string.IsNullOrEmpty(state.LastValue))
                {
                    var v = double.Parse(state.LastValue);
                    Quantity.TryFrom(v, mapping.DisplayUnitType, out IQuantity last);
                    result = chQuantity.Value != last.Value;
                }
                else
                {
                    result = false;
                }
                break;
        }

        // Apply time constraint if set
        if (comp.ForMs > 0)
        {
            // Result is true, start timer
            if (result && state.ForTimestamp == null)
            {
                state.ForTimestamp = dateTime.UtcNow;
                result = false;
            }
            // Result is true and timer is already running,
            // see if it is longer than the specified duration
            else if (result && state.ForTimestamp != null)
            {
                var diff = dateTime.UtcNow - state.ForTimestamp.Value;
                if (diff.TotalMilliseconds < comp.ForMs)
                {
                    result = false;
                }
            }
            // Result is false, reset the timer
            else
            {
                state.ForTimestamp = null;
                result = false;
            }
        }

        // Track comparison state
        state.LastValue = chQuantity.Value.ToString();
        state.IsTrue = result;
        await comparisonRepository.SetStateAsync(state);
        
        return result;
    }
}
