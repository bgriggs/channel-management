using System;

namespace BigMission.TestHelpers
{
    /// <summary>
    /// DateTime wrapper used for dependency injection
    /// so unit testing is possible.
    /// </summary>
    public class DateTimeHelper : IDateTimeHelper
    {
        public DateTime Now => DateTime.Now;

        public DateTime UtcNow => DateTime.UtcNow;
    }
}
