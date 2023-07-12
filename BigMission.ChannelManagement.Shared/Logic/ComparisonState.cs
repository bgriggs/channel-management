using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigMission.ChannelManagement.Shared.Logic
{
    /// <summary>
    /// State information from a statement evaluation.
    /// </summary>
    public class ComparisonState
    {
        public int ComparisonId { get; set; }
        
        /// <summary>
        /// Is the statement active.
        /// </summary>
        public bool IsTrue { get; set; }
        
        /// <summary>
        /// Channel value for the Updated logic.
        /// </summary>
        public string LastValue { get; set; }

        /// <summary>
        /// The timestamp at which the statement went true. 
        /// This is used for the 'on for' clause.
        /// </summary>
        public DateTime? ForTimestamp { get; set; }
    }
}
