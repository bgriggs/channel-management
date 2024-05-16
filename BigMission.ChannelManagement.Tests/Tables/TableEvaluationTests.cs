using BigMission.ChannelManagement.Shared.Tables;
using Moq;

namespace BigMission.ChannelManagement.Shared.Tests.Tables
{
    [TestClass]
    public class TableEvaluationTests
    {
        [TestMethod]
        public void StringToStringTest()
        {
            var channelRepo = new ChannelMemoryRepository();
            var cv1 = new ChannelValue { Id = 1, Value = "aaa" };
            channelRepo.SetChannelValueAsync(cv1).Wait();
            var cv2 = new ChannelValue { Id = 2, Value = "" };
            channelRepo.SetChannelValueAsync(cv2).Wait();

            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, IsStringValue = true };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));

            var tableRepoMock = new Mock<ITableRepository>();
            var tableMapping = new TableMapping { Id = 1, IgnoreCase = true, InputChannel = 1, OutputChannel = 2 };
            tableMapping.Mapping.Add(("aaa", "111"));
            tableMapping.Mapping.Add(("bbb", "222"));
            tableMapping.Mapping.Add(("CCC", "333"));
            tableRepoMock.Setup(t => t.GetMappingsAsync()).Returns(Task.FromResult(new[] { tableMapping }.AsEnumerable()));
            var tableEval = new TableEvaluation(tableRepoMock.Object, channelRepo, channelMappingRepositoryMock.Object);

            var t = tableEval.Evaluate();
            t.Wait();
            var output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("111", output.Result.Value);

            // No Map
            cv1.Value = "dfgfhsgfhsffg";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual(string.Empty, output.Result.Value);

            // Ignore case on all lower
            tableMapping.IgnoreCase = false;
            cv1.Value = "bbb";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("222", output.Result.Value);

            // Ignore case on different case
            tableMapping.IgnoreCase = false;
            cv1.Value = "ccc";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual(string.Empty, output.Result.Value);

            // Ignore case on different case
            tableMapping.IgnoreCase = false;
            cv1.Value = "CCC";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("333", output.Result.Value);

            // Ignore case on different case
            tableMapping.IgnoreCase = true;
            cv1.Value = "ccc";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("333", output.Result.Value);
        }

        [TestMethod]
        public void IntegerToStringTest()
        {
            var channelRepo = new ChannelMemoryRepository();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            channelRepo.SetChannelValueAsync(cv1).Wait();
            var cv2 = new ChannelValue { Id = 2, Value = "" };
            channelRepo.SetChannelValueAsync(cv2).Wait();

            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 0, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 0, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));

            var tableRepoMock = new Mock<ITableRepository>();
            var tableMapping = new TableMapping { Id = 1, InputChannel = 1, OutputChannel = 2 };
            tableMapping.Mapping.Add(("1", "111"));
            tableMapping.Mapping.Add(("2", "222"));
            tableMapping.Mapping.Add(("3", "333"));
            tableRepoMock.Setup(t => t.GetMappingsAsync()).Returns(Task.FromResult(new[] { tableMapping }.AsEnumerable()));
            var tableEval = new TableEvaluation(tableRepoMock.Object, channelRepo, channelMappingRepositoryMock.Object);

            var t = tableEval.Evaluate();
            t.Wait();
            var output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("111", output.Result.Value);

            // No Map
            cv1.Value = "123455";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual(string.Empty, output.Result.Value);

            cv1.Value = "2";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("222", output.Result.Value);
        }

        [TestMethod]
        public void InterpolationTest()
        {
            var channelRepo = new ChannelMemoryRepository();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            channelRepo.SetChannelValueAsync(cv1).Wait();
            var cv2 = new ChannelValue { Id = 2, Value = "" };
            channelRepo.SetChannelValueAsync(cv2).Wait();

            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 0, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 0, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));

            var tableRepoMock = new Mock<ITableRepository>();
            var tableMapping = new TableMapping { Id = 1, InputChannel = 1, OutputChannel = 2, InterpolationType = InterpolationType.Linear };
            tableMapping.Mapping.Add(("1", "100"));
            tableMapping.Mapping.Add(("2", "200"));
            tableMapping.Mapping.Add(("3", "3000"));
            tableRepoMock.Setup(t => t.GetMappingsAsync()).Returns(Task.FromResult(new[] { tableMapping }.AsEnumerable()));
            var tableEval = new TableEvaluation(tableRepoMock.Object, channelRepo, channelMappingRepositoryMock.Object);

            // Linear - direct
            var t = tableEval.Evaluate();
            t.Wait();
            var output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("100.0", output.Result.Value);

            // Linear - half
            cv1.Value = "1.5";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("150.0", output.Result.Value);

            // Linear - half
            cv1.Value = "2.5";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("1600.0", output.Result.Value);

            // Linear - 75%
            cv1.Value = "2.75";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("2300.0", output.Result.Value);

            // Linear - out of range
            cv1.Value = "5.75";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("10700.0", output.Result.Value);

            tableMapping.InterpolationType = InterpolationType.CubicSpline;
            // Cubic - direct
            cv1.Value = "1.0";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("100.0", output.Result.Value);

            // Cubic - half
            cv1.Value = "1.5";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("-103.1", output.Result.Value);

            // Cubic - half
            cv1.Value = "2.5";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("1346.9", output.Result.Value);

            // Cubic - 75%
            cv1.Value = "2.75";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("2141.8", output.Result.Value);

            // Cubic - out of range
            cv1.Value = "5.75";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("-1481.6", output.Result.Value);

            tableMapping.InterpolationType = InterpolationType.Polynomial;
            // Polynomial - direct
            cv1.Value = "1.0";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("100.0", output.Result.Value);

            // Polynomial - half
            cv1.Value = "1.5";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("-187.5", output.Result.Value);

            // Polynomial - half
            cv1.Value = "2.5";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("1262.5", output.Result.Value);

            // Polynomial - 75%
            cv1.Value = "2.75";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("2046.9", output.Result.Value);

            // Polynomial - out of range
            cv1.Value = "5.75";
            t = tableEval.Evaluate();
            t.Wait();
            output = channelRepo.GetChannelValueAsync(2);
            output.Wait();
            Assert.AreEqual("24621.9", output.Result.Value);
        }
    }
}
