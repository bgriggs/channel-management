using BigMission.ChannelManagement.Shared.Logic;
using BigMission.ChannelManagement.Shared.Timer;
using BigMission.TestHelpers;
using Moq;

namespace BigMission.ChannelManagement.Shared.Tests.Timer
{
    [TestClass]
    public class TimerEvaluationTests
    {
        [TestMethod]
        public void StartConditionTest()
        {
            var channelRepo = new ChannelMemoryRepository();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            var cv2 = new ChannelValue { Id = 2, Value = "0" };
            channelRepo.SetChannelValueAsync(cv1).Wait();
            channelRepo.SetChannelValueAsync(cv2).Wait();

            var comparisonRepository = new ComparisonStateMemoryRepository();
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepo, comparisonRepository, channelMappingRepositoryMock.Object, dateTimeMock.Object);

            // Initialize logic state for starting the timer
            var statement = new Statements { Id = 1 };
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.GreaterThan, UseStaticComparison = true, StaticValueComparison = "2" };
            statement.StatementRows = new List<List<Comparison>>
            {
                new List<Comparison> { comp }
            };
            var statementRepo = new StatementMemoryRepository();
            statementRepo.SetStatementsAsync(statement).Wait();

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:00pm"));

            var timer = new TimerParameters { Id = 1, OutputChId = 2, StartStatementId = 1 };
            var timerRepo = new TimerMemoryRepository();
            timerRepo.SaveTimersAsync(new[] { timer }).Wait();

            var timerEvaluation = new TimerEvaluation(timerRepo, channelRepo, channelMappingRepositoryMock.Object, dateTimeMock.Object, statementRepo, comparisonRepository);

            // Not true
            timerEvaluation.UpdateTimers().Wait();
            var result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(0, result.Result.GetValueInt());

            // True, start timer
            // 3 > 2
            cv1.SetBaseValue(3);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:01pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(0, result.Result.GetValueInt());

            // Active for 10 seconds
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:11pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(10, result.Result.GetValueInt());

            // Start condition no longer true--should continue since there is not stop condition
            // 0 < 2
            cv1.SetBaseValue(0);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:15pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(14, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:20pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(19, result.Result.GetValueInt());
        }

        [TestMethod]
        public void StopConditionTest()
        {
            var channelRepo = new ChannelMemoryRepository();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            var cv2 = new ChannelValue { Id = 2, Value = "0" };
            channelRepo.SetChannelValueAsync(cv1).Wait();
            channelRepo.SetChannelValueAsync(cv2).Wait();

            var comparisonRepository = new ComparisonStateMemoryRepository();
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepo, comparisonRepository, channelMappingRepositoryMock.Object, dateTimeMock.Object);

            // Initialize logic state for starting the timer
            var startStatement = new Statements { Id = 1 };
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.GreaterThan, UseStaticComparison = true, StaticValueComparison = "2" };
            startStatement.StatementRows = new List<List<Comparison>>
            {
                new List<Comparison> { comp }
            };

            // Initialize logic state for starting the timer
            var stopStatement = new Statements { Id = 2 };
            comp = new Comparison { ComparisonId = 2, ChannelId = 1, Logic = LogicType.LessThan, UseStaticComparison = true, StaticValueComparison = "2" };
            stopStatement.StatementRows = new List<List<Comparison>>
            {
                new List<Comparison> { comp }
            };

            var statementRepo = new StatementMemoryRepository();
            statementRepo.SetStatementsAsync(startStatement).Wait();
            statementRepo.SetStatementsAsync(stopStatement).Wait();

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:00pm"));

            var timer = new TimerParameters { Id = 1, OutputChId = 2, StartStatementId = 1, StopStatementId = 2, StartSeconds = 100, EnableStartSeconds = true };
            var timerRepo = new TimerMemoryRepository();
            timerRepo.SaveTimersAsync(new[] { timer }).Wait();

            var timerEvaluation = new TimerEvaluation(timerRepo, channelRepo, channelMappingRepositoryMock.Object, dateTimeMock.Object, statementRepo, comparisonRepository);

            // True, start timer
            // 3 > 2
            cv1.SetBaseValue(3);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:01pm"));
            timerEvaluation.UpdateTimers().Wait();
            var result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(100, result.Result.GetValueInt());

            // Active for 10 seconds
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:11pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(110, result.Result.GetValueInt());

            // Start condition no longer true--should stop since stop condition is true
            // 1 < 2
            cv1.SetBaseValue(1);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:15pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(114, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:20pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(114, result.Result.GetValueInt());

            // True, start timer--this should reset to 100 since the start value is enabled
            // 3 > 2
            cv1.SetBaseValue(3);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:30pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(100, result.Result.GetValueInt());

            // Stop Timer
            // 1 < 2
            cv1.SetBaseValue(1);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:40pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(110, result.Result.GetValueInt());


            // True, start timer--this should NOT reset to 100 since the start value not enabled
            timer.EnableStartSeconds = false;
            // 3 > 2
            cv1.SetBaseValue(3);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:50pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(110, result.Result.GetValueInt());

            // Stop Timer--this should reset the value to 500
            timer.EnableStopSeconds = true;
            timer.StopSeconds = 500;
            // 1 < 2
            cv1.SetBaseValue(1);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:51pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(500, result.Result.GetValueInt());
        }

        [TestMethod]
        public void CountDownTest()
        {
            var channelRepo = new ChannelMemoryRepository();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            var cv2 = new ChannelValue { Id = 2, Value = "0" };
            channelRepo.SetChannelValueAsync(cv1).Wait();
            channelRepo.SetChannelValueAsync(cv2).Wait();

            var comparisonRepository = new ComparisonStateMemoryRepository();
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepo, comparisonRepository, channelMappingRepositoryMock.Object, dateTimeMock.Object);

            // Initialize logic state for starting the timer
            var statement = new Statements { Id = 1 };
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.GreaterThan, UseStaticComparison = true, StaticValueComparison = "2" };
            statement.StatementRows = new List<List<Comparison>>
            {
                new List<Comparison> { comp }
            };
            var statementRepo = new StatementMemoryRepository();
            statementRepo.SetStatementsAsync(statement).Wait();

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:00pm"));

            var timer = new TimerParameters
            {
                Id = 1,
                OutputChId = 2,
                StartStatementId = 1,
                CountDown = true,
            };
            var timerRepo = new TimerMemoryRepository();
            timerRepo.SaveTimersAsync(new[] { timer }).Wait();

            var timerEvaluation = new TimerEvaluation(timerRepo, channelRepo, channelMappingRepositoryMock.Object, dateTimeMock.Object, statementRepo, comparisonRepository);

            // True, start timer
            // 3 > 2
            cv1.SetBaseValue(3);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:00pm"));
            timerEvaluation.UpdateTimers().Wait();
            var result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(0, result.Result.GetValueInt());

            // Make sure it will not drop below 0
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:05pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(0, result.Result.GetValueInt());

            // Give it 30 second range
            timer.EnableStartSeconds = true;
            timer.StartSeconds = 30;
            timerRepo.SetTimerStateAsync(new TimerState { Id = 1, Started = DateTime.Parse("1/1/2023 1:00:00pm"), StartValue = 30 }).Wait();
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:10pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(20, result.Result.GetValueInt());

            // Should be below min value of 0 and stop
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:31pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(0, result.Result.GetValueInt());

            // Give range of 100 to 90
            timer.EnableStartSeconds = true;
            timer.StartSeconds = 100;
            timer.EnableRollover = true;
            timer.RolloverSeconds = 90;
            timerRepo.SetTimerStateAsync(new TimerState { Id = 1, Started = DateTime.Parse("1/1/2023 1:00:00pm"), StartValue = 100 }).Wait();

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:05pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(95, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:10pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(90, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:11pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(99, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:30pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(90, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:52pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(98, result.Result.GetValueInt());
        }

        [TestMethod]
        public void LimitRolloverCountUpTest()
        {
            var channelRepo = new ChannelMemoryRepository();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            var cv2 = new ChannelValue { Id = 2, Value = "0" };
            channelRepo.SetChannelValueAsync(cv1).Wait();
            channelRepo.SetChannelValueAsync(cv2).Wait();

            var comparisonRepository = new ComparisonStateMemoryRepository();
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepo, comparisonRepository, channelMappingRepositoryMock.Object, dateTimeMock.Object);

            // Initialize logic state for starting the timer
            var statement = new Statements { Id = 1 };
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.GreaterThan, UseStaticComparison = true, StaticValueComparison = "2" };
            statement.StatementRows = new List<List<Comparison>>
            {
                new List<Comparison> { comp }
            };
            var statementRepo = new StatementMemoryRepository();
            statementRepo.SetStatementsAsync(statement).Wait();

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:00pm"));

            var timer = new TimerParameters { Id = 1, OutputChId = 2, StartStatementId = 1, EnableRollover = true, RolloverSeconds = 10 };
            var timerRepo = new TimerMemoryRepository();
            timerRepo.SaveTimersAsync(new[] { timer }).Wait();

            var timerEvaluation = new TimerEvaluation(timerRepo, channelRepo, channelMappingRepositoryMock.Object, dateTimeMock.Object, statementRepo, comparisonRepository);

            // True, start timer
            // 3 > 2
            cv1.SetBaseValue(3);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:00pm"));
            timerEvaluation.UpdateTimers().Wait();
            var result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(0, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:05pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(5, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:10pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(10, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:11pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(1, result.Result.GetValueInt());

            // Force a long rollover where the time change exceeds the rollover
            // It may technically be better to be 10, but unlikely be to here at all
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:20pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(0, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:55pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(5, result.Result.GetValueInt());

            // Enable start seconds
            timer.EnableStartSeconds = true;
            timer.StartSeconds = 5;

            // Should be start (5) + overflow (2) = 7
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:01:02pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(7, result.Result.GetValueInt());
        }

        [TestMethod]
        public void LimitRolloverCountDownTest()
        {
            var channelRepo = new ChannelMemoryRepository();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            var cv2 = new ChannelValue { Id = 2, Value = "0" };
            channelRepo.SetChannelValueAsync(cv1).Wait();
            channelRepo.SetChannelValueAsync(cv2).Wait();

            var comparisonRepository = new ComparisonStateMemoryRepository();
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 0, BaseUnitType = "s", DisplayUnitType = "s", DisplayDecimalPlaces = 0, DataType = "Duration" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepo, comparisonRepository, channelMappingRepositoryMock.Object, dateTimeMock.Object);

            // Initialize logic state for starting the timer
            var statement = new Statements { Id = 1 };
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.GreaterThan, UseStaticComparison = true, StaticValueComparison = "2" };
            statement.StatementRows = new List<List<Comparison>>
            {
                new List<Comparison> { comp }
            };
            var statementRepo = new StatementMemoryRepository();
            statementRepo.SetStatementsAsync(statement).Wait();

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:00pm"));

            var timer = new TimerParameters
            {
                Id = 1,
                OutputChId = 2,
                StartStatementId = 1,
                CountDown = true,
                EnableRollover = true,
                RolloverSeconds = 90,
                EnableStartSeconds = true,
                StartSeconds = 100
            };
            var timerRepo = new TimerMemoryRepository();
            timerRepo.SaveTimersAsync(new[] { timer }).Wait();

            var timerEvaluation = new TimerEvaluation(timerRepo, channelRepo, channelMappingRepositoryMock.Object, dateTimeMock.Object, statementRepo, comparisonRepository);

            // True, start timer
            // 3 > 2
            cv1.SetBaseValue(3);
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:00pm"));
            timerEvaluation.UpdateTimers().Wait();
            var result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(100, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:05pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(95, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:10pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(90, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:11pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(99, result.Result.GetValueInt());

            // Force a long rollover where the time change exceeds the rollover
            // Unlike the count up, this will be at the lower end value before resetting
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:20pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(90, result.Result.GetValueInt());

            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 1:00:55pm"));
            timerEvaluation.UpdateTimers().Wait();
            result = channelRepo.GetChannelValueAsync(cv2.Id);
            result.Wait();
            Assert.AreEqual(95, result.Result.GetValueInt());
        }
    }
}
