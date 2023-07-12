using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigMission.ChannelManagement.Shared.Tables
{
    public interface ITableRepository
    {
        public Task<IEnumerable<TableMapping>> GetMappingsAsync();
    }
}
