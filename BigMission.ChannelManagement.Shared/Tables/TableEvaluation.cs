using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

namespace BigMission.ChannelManagement.Shared.Tables;

/// <summary>
/// Convert from input value to output channel using a table. 
/// This can resolve int or string input to another string/enum.
/// This can resolve double to double, such as analog voltage to specified units, like temperature.
/// </summary>
public class TableEvaluation
{
    private readonly ITableRepository tableRepository;
    private readonly IChannelRepository channelRepository;
    private readonly IChannelMappingRepository channelMappingRepository;

    public TableEvaluation(ITableRepository tableRepository, IChannelRepository channelRepository, IChannelMappingRepository channelMappingRepository)
    {
        this.tableRepository = tableRepository;
        this.channelRepository = channelRepository;
        this.channelMappingRepository = channelMappingRepository;
    }

    public async Task Evaluate()
    {
        var mappings = await tableRepository.GetMappingsAsync();
        foreach (var mapping in mappings)
        {
            var inputCh = await channelRepository.GetChannelValueAsync(mapping.InputChannel);
            var inputMap = await channelMappingRepository.GetChannelMappingAsync(mapping.InputChannel);
            var outputMap = await channelMappingRepository.GetChannelMappingAsync(mapping.OutputChannel);
            var outputValue = new ChannelValue { Id = mapping.OutputChannel };

            // String, straight mapping, e.g. enum
            // String -> string
            if (inputMap.Dto.IsStringValue)
            {
                var c = StringComparison.Ordinal;
                if (mapping.IgnoreCase)
                {
                    c = StringComparison.OrdinalIgnoreCase;
                }

                foreach (var m in mapping.Mapping)
                {
                    if (string.Compare(m.input, inputCh.Value, c) == 0)
                    {
                        outputValue.Value = m.output;
                        break;
                    }
                }
            }
            // Integers -> string
            else if (inputMap.Dto.BaseDecimalPlaces == 0)
            {
                foreach (var m in mapping.Mapping)
                {
                    if (int.TryParse(m.input, out int inVal) && inVal == inputCh.GetValueInt())
                    {
                        outputValue.Value = m.output;
                        break;
                    }
                }
            }
            // Double -> double: interpolate with the table
            else
            {
                IInterpolation interpolate = null;
                switch (mapping.InterpolationType)
                {
                    case InterpolationType.Linear:
                        interpolate = Interpolate.Linear(mapping.InputPoints, mapping.OutputValues);
                        break;
                    case InterpolationType.CubicSpline:
                        interpolate = Interpolate.CubicSpline(mapping.InputPoints, mapping.OutputValues);
                        break;
                    case InterpolationType.Polynomial:
                        interpolate = Interpolate.Polynomial(mapping.InputPoints, mapping.OutputValues);
                        break;
                }
                
                var interpolatedOutput = interpolate.Interpolate(inputCh.GetValueDouble());
                outputValue.SetBaseValue(interpolatedOutput, outputMap);
            }

            await channelRepository.SetChannelValueAsync(outputValue);
        }
    }
}
