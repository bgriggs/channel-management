﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigMission.ChannelManagement.Shared.Logic
{
    public class Statements
    {
        public int Id { get; set; }
        public List<List<Comparison>> StatementRows { get; set; }
    }
}
