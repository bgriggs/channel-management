using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigMission.ChannelManagement.Shared.Math
{
    public class MathParameters
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public MathType Type { get; set; }
        public decimal A { get; set; }
        public decimal B { get; set; }
        public int Channel1Id { get; set; }
        public int Channel2Id { get; set; }
        public int OutputChannelId { get; set;}
        public SimpleOperationType SimpleOperationType { get; set; }
    }
}
