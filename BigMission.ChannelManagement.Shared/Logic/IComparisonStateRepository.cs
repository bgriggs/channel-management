using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigMission.ChannelManagement.Shared.Logic
{
    public interface IComparisonStateRepository
    {
        public Task SetStateAsync(ComparisonState state);
        public Task<ComparisonState> GetStateAsync(int comparisonId);
    }
}
