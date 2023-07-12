using BigMission.ChannelManagement.Shared.Logic;
using BigMission.TestHelpers;
using Moq;

namespace BigMission.ChannelManagement.Shared.Tests.Logic
{
    [TestClass]
    public class LogicEvaluationTests
    {
        [TestMethod]
        public void ComparisonsTest()
        {
            var channelRepoMock = new Mock<IChannelRepository>();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(1)).Returns(Task.FromResult(cv1));
            var cv2 = new ChannelValue { Id = 1, Value = "2" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(2)).Returns(Task.FromResult(cv2));
            var comparisonRepositoryMock = new Mock<IComparisonStateRepository>();
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepoMock.Object, comparisonRepositoryMock.Object, channelMappingRepositoryMock.Object, dateTimeMock.Object);

            var statement = new Statements();
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.GreaterThan, ChannelComparisonId = 2, UseStaticComparison = false };
            statement.StatementRows = new List<List<Comparison>>
            { new List<Comparison>
                { comp }
            };

            // Greater than
            var t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "2";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "3";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);

            // Greater than or equal
            comp.Logic = LogicType.GreaterThanOrEqualTo;
            cv1.Value = "1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "2";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            cv1.Value = "3";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);

            // Less than
            comp.Logic = LogicType.LessThan;
            cv1.Value = "1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            cv1.Value = "2";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "3";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // Less than or equal
            comp.Logic = LogicType.LessThanOrEqualTo;
            cv1.Value = "1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            cv1.Value = "2";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            cv1.Value = "3";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // Equal
            comp.Logic = LogicType.EqualTo;
            cv1.Value = "1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "2";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            cv1.Value = "3";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // True
            comp.Logic = LogicType.True;
            cv1.Value = "1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            cv1.Value = "0";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "-1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "100";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);

            // False
            comp.Logic = LogicType.False;
            cv1.Value = "1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "0";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            cv1.Value = "-1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            cv1.Value = "100";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
        }

        [TestMethod]
        public void ComparisonStaticValueTest()
        {
            var channelRepoMock = new Mock<IChannelRepository>();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(1)).Returns(Task.FromResult(cv1));
            var comparisonRepositoryMock = new Mock<IComparisonStateRepository>();
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepoMock.Object, comparisonRepositoryMock.Object, channelMappingRepositoryMock.Object, dateTimeMock.Object);

            var statement = new Statements();
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.GreaterThan, UseStaticComparison = true, StaticValueComparison = "2" };
            statement.StatementRows = new List<List<Comparison>>
            { new List<Comparison>
                { comp }
            };

            // Greater than
            var t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "2";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            cv1.Value = "3";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
        }

        [TestMethod]
        public void ChannelUpdatedTest()
        {
            var channelRepoMock = new Mock<IChannelRepository>();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(1)).Returns(Task.FromResult(cv1));
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepoMock.Object, new ComparisonStateMemoryRepository(), channelMappingRepositoryMock.Object, dateTimeMock.Object);

            var statement = new Statements();
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.Updated, UseStaticComparison = false };
            statement.StatementRows = new List<List<Comparison>>
            { new List<Comparison>
                { comp }
            };

            var t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            // No change
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            // Changed
            cv1.Value = "100";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            // No change
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
        }

        [TestMethod]
        public void ChannelChangedByTest()
        {
            var channelRepoMock = new Mock<IChannelRepository>();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(1)).Returns(Task.FromResult(cv1));
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepoMock.Object, new ComparisonStateMemoryRepository(), channelMappingRepositoryMock.Object, dateTimeMock.Object);

            var statement = new Statements();
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.ChangedBy, UseStaticComparison = true, StaticValueComparison = "100" };
            statement.StatementRows = new List<List<Comparison>>
            { new List<Comparison>
                { comp }
            };

            var t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            // No change
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            // Below
            cv1.Value = "99";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            // At threshold
            cv1.Value = "199";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
            // Below threshold
            cv1.Value = "200";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);
            // Above threshold
            cv1.Value = "1000";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
        }

        [TestMethod]
        public void CompoundAndTest()
        {
            var channelRepoMock = new Mock<IChannelRepository>();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(1)).Returns(Task.FromResult(cv1));
            var cv2 = new ChannelValue { Id = 2, Value = "2" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(2)).Returns(Task.FromResult(cv2));
            var comparisonRepositoryMock = new Mock<IComparisonStateRepository>();
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepoMock.Object, comparisonRepositoryMock.Object, channelMappingRepositoryMock.Object, dateTimeMock.Object);

            var statement = new Statements();
            var comp1 = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.GreaterThan, UseStaticComparison = true, StaticValueComparison = "2" };
            var comp2 = new Comparison { ComparisonId = 2, ChannelId = 2, Logic = LogicType.EqualTo, UseStaticComparison = true, StaticValueComparison = "3" };
            statement.StatementRows = new List<List<Comparison>>
            { new List<Comparison>
                // Same row is AND
                { comp1, comp2 }
            };

            // 1 > 2 AND 2 == 3
            // False
            var t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // 3 > 2 AND 2 == 3
            // False
            cv1.Value = "3";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // 3 > 2 AND 3 == 3
            // False
            cv2.Value = "3";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
        }

        /// <summary>
        /// Test 2 rows of comparisons.
        /// </summary>
        [TestMethod]
        public void CompoundOrTest()
        {
            var channelRepoMock = new Mock<IChannelRepository>();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(1)).Returns(Task.FromResult(cv1));
            var cv2 = new ChannelValue { Id = 2, Value = "2" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(2)).Returns(Task.FromResult(cv2));
            var cv3 = new ChannelValue { Id = 3, Value = "3" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(3)).Returns(Task.FromResult(cv3));
            var comparisonRepositoryMock = new Mock<IComparisonStateRepository>();
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
            var dto3 = new ChannelMappingDto { Id = 3, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(3)).Returns(Task.FromResult(new ChannelMapping(dto3)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepoMock.Object, comparisonRepositoryMock.Object, channelMappingRepositoryMock.Object, dateTimeMock.Object);

            var statement = new Statements();
            var comp1 = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.GreaterThan, UseStaticComparison = true, StaticValueComparison = "2" };
            var comp2 = new Comparison { ComparisonId = 2, ChannelId = 2, Logic = LogicType.EqualTo, UseStaticComparison = true, StaticValueComparison = "3" };
            var comp3 = new Comparison { ComparisonId = 3, ChannelId = 3, Logic = LogicType.LessThanOrEqualTo, UseStaticComparison = true, StaticValueComparison = "1" };
            statement.StatementRows = new List<List<Comparison>>
            { new List<Comparison>
                // Same row is AND
                { comp1, comp2 },
                new List<Comparison>
                // Rows are OR
                { comp3 }
            };

            // 1 > 2 AND 2 == 3
            // 3 <= 1
            // False
            var t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // 3 > 2 AND 2 == 3
            // 1 <= 1
            // True
            cv1.Value = "3";
            cv3.Value = "1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);

            // 3 > 2 AND 3 == 3
            // 1 <= 1
            cv2.Value = "3";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
        }

        [TestMethod]
        public void OnForTest()
        {
            var channelRepoMock = new Mock<IChannelRepository>();
            var cv1 = new ChannelValue { Id = 1, Value = "1" };
            channelRepoMock.Setup(c => c.GetChannelValueAsync(1)).Returns(Task.FromResult(cv1));
            var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
            var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
            channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
            var dateTimeMock = new Mock<IDateTimeHelper>();

            var logic = new LogicEvaluation(channelRepoMock.Object, new ComparisonStateMemoryRepository(), channelMappingRepositoryMock.Object, dateTimeMock.Object);

            var statement = new Statements();
            var comp = new Comparison { ComparisonId = 1, ChannelId = 1, Logic = LogicType.EqualTo, UseStaticComparison = true, StaticValueComparison = "1", ForMs = 3000 };
            statement.StatementRows = new List<List<Comparison>>
            { new List<Comparison>
                { comp }
            };

            // 1 == 1 true 
            // Start timer
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 9:00:00pm"));
            var t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // 1 == 1 true 
            // Timer = 1s
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 9:00:01pm"));
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // 1 == 1 true 
            // Timer = 3s
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 9:00:03pm"));
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);

            // 2 == 1 false 
            // Timer = 5s
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 9:00:05pm"));
            cv1.Value = "2";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // 1 == 1 false 
            // Timer = 0s
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 9:00:06pm"));
            cv1.Value = "1";
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsFalse(t.Result);

            // 1 == 1 false 
            // Timer = 4s
            dateTimeMock.Setup(d => d.UtcNow).Returns(DateTime.Parse("1/1/2023 9:00:10pm"));
            t = logic.Evaluate(statement);
            t.Wait();
            Assert.IsTrue(t.Result);
        }
    }
}
