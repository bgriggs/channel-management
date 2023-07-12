﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigMission.ChannelManagement.Shared.Logic
{
    public class Comparison
    {
        public int ComparisonId { get; set; }
        public int ChannelId { get; set; }
        public LogicType Logic { get; set; }
        public bool UseStaticComparison { get; set; }
        public string StaticValueComparison { get; set; }
        public int ChannelComparisonId { get; set; }
        public int ForMs { get; set; }
        public bool ReverseResult { get; set; }
    }
}