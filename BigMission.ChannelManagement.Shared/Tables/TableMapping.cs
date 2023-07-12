using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigMission.ChannelManagement.Shared.Tables
{
    public class TableMapping
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsEnum { get; set; }
        public bool IgnoreCase { get; set; }
        public int InputChannel { get; set; }
        public int OutputChannel { get; set; }
        public InterpolationType InterpolationType { get; set; }

        public List<(string input, string output)> Mapping { get; } = new List<(string input, string output)>();


        private IEnumerable<double> inputPoints= null;
        private IEnumerable<double> outputValues = null;

        public IEnumerable<double> InputPoints
        {
            get
            {
                inputPoints ??= Mapping.Select(m => double.Parse(m.input)).ToArray();
                return inputPoints;
            }
        }

        public IEnumerable<double> OutputValues
        {
            get
            {
                outputValues ??= Mapping.Select(m => double.Parse(m.output)).ToArray();
                return outputValues;
            }
        }
    }
}
