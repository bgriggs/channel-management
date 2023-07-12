using System;

namespace BigMission.TestHelpers
{
    /// <summary>
    /// DateTime mockable interface to enable unit testing.
    /// </summary>
    public interface IDateTimeHelper
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}
