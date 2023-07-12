using BigMission.ChannelManagement.Shared;
using System.Globalization;
using UnitsNet;

namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            ////foreach( var q in Quantity.Names)
            ////{
            ////    Console.WriteLine(q);
            ////}

            ////var usEnglish = new CultureInfo("en-US");
            ////Thread.CurrentThread.CurrentCulture = usEnglish;
            //var dto = new ChannelMappingDto { Id = 1, Name = "test", DataType = "Length", BaseUnitType = "cm", BaseDecimalPlaces = 2, DisplayUnitType = "m", DisplayDecimalPlaces = 4 };
            //var ch = new ChannelMapping(dto);
            //var v = new ChannelValue() { Value = "30330" };
            //double d = double.Parse(v.Value);
            //if (dto.BaseDecimalPlaces > 0)
            //{
            //    for (int i = 0; i < dto.BaseDecimalPlaces; i++)
            //    {
            //        d /= 10;
            //    }
            //}

            //var av = Quantity.From(d, ch.BaseUnitType);


            ////string q = "Length";
            ////QuantityInfo qi = Quantity.ByName[q];
            ////var unit = UnitParser.Default.Parse("cm", qi.UnitType);


            //var dv = av.As(ch.DisplayUnitType);
            //Console.WriteLine($"{av} {dv}");

            double v = 5.123456;
            Console.WriteLine($"{v:0}");

        }
    }
}