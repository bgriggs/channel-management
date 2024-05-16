using BigMission.ChannelManagement.Math;
using Moq;

namespace BigMission.ChannelManagement.Tests.Math;

[TestClass]
public class MathEvaluationTests
{
    /// <summary>
    /// value1 / (value1 + value2)
    /// </summary>
    [TestMethod]
    public void BiasTest()
    {
        var mathRepositoryMock = new Mock<IMathRepository>();
        var parameters = new[] { new MathParameters { Id = 1, Order = 1, Type = MathType.Bias, Channel1Id = 1, Channel2Id = 2, OutputChannelId = 3 } }.AsEnumerable();
        mathRepositoryMock.Setup(m => m.GetParameters()).Returns(Task.FromResult(parameters));

        var channelRepo = new ChannelMemoryRepository();
        var cv1 = new ChannelValue { Id = 1, Value = "1" };
        var cv2 = new ChannelValue { Id = 2, Value = "2" };
        var cv3 = new ChannelValue { Id = 3, Value = "0" };
        channelRepo.SetChannelValueAsync(cv1).Wait();
        channelRepo.SetChannelValueAsync(cv2).Wait();
        channelRepo.SetChannelValueAsync(cv3).Wait();

        var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
        var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
        var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 2, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
        var dto3 = new ChannelMappingDto { Id = 3, BaseDecimalPlaces = 5, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(3)).Returns(Task.FromResult(new ChannelMapping(dto3)));

        var me = new MathEvaluation(mathRepositoryMock.Object, channelRepo, channelMappingRepositoryMock.Object);

        // output = 1 / (1 + 2)
        me.RunCalculations().Wait();

        var output = channelRepo.GetChannelValueAsync(3);
        output.Wait();
        Assert.AreEqual("0.33333", output.Result.Value);
    }

    /// <summary>
    /// (value1 * a) + b
    /// </summary>
    [TestMethod]
    public void LinearCorrectorTest()
    {
        var mathRepositoryMock = new Mock<IMathRepository>();
        var parameters = new[] { new MathParameters { Id = 1, Order = 1, Type = MathType.LinearCorrector, Channel1Id = 1, OutputChannelId = 2, A = 3.5M, B = 9.349057M } }.AsEnumerable();
        mathRepositoryMock.Setup(m => m.GetParameters()).Returns(Task.FromResult(parameters));

        var channelRepo = new ChannelMemoryRepository();
        var cv1 = new ChannelValue { Id = 1, Value = "5.334" };
        var cv2 = new ChannelValue { Id = 2, Value = "0" };
        channelRepo.SetChannelValueAsync(cv1).Wait();
        channelRepo.SetChannelValueAsync(cv2).Wait();

        var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
        var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 3, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
        var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 3, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));

        var me = new MathEvaluation(mathRepositoryMock.Object, channelRepo, channelMappingRepositoryMock.Object);

        // (5.334 * 3.5) + 9.34957 = 28.018057
        me.RunCalculations().Wait();

        var output = channelRepo.GetChannelValueAsync(2);
        output.Wait();
        Assert.AreEqual("28.018", output.Result.Value);
    }

    /// <summary>
    /// Output = value1 +-*/ value2
    /// </summary>
    [TestMethod]
    public void SimpleOperationTest()
    {
        var mathRepositoryMock = new Mock<IMathRepository>();
        var param1 = new MathParameters { Id = 1, Order = 1, Type = MathType.SimpleOperation, SimpleOperationType = SimpleOperationType.Add, Channel1Id = 1, OutputChannelId = 3, A = 3.5M };
        var param2 = new MathParameters { Id = 2, Order = 2, Type = MathType.SimpleOperation, SimpleOperationType = SimpleOperationType.Subtract, Channel1Id = 1, OutputChannelId = 4, A = 1.5M };
        var param3 = new MathParameters { Id = 3, Order = 3, Type = MathType.SimpleOperation, SimpleOperationType = SimpleOperationType.Multiply, Channel1Id = 1, Channel2Id = 2, OutputChannelId = 5 };
        var param4 = new MathParameters { Id = 4, Order = 4, Type = MathType.SimpleOperation, SimpleOperationType = SimpleOperationType.Divide, Channel1Id = 1, Channel2Id = 2, OutputChannelId = 6 };
        var parameters = new[] { param1, param2, param3, param4 }.AsEnumerable();
        mathRepositoryMock.Setup(m => m.GetParameters()).Returns(Task.FromResult(parameters));

        var channelRepo = new ChannelMemoryRepository();
        var cv1 = new ChannelValue { Id = 1, Value = "1" };
        var cv2 = new ChannelValue { Id = 2, Value = "2.34" };
        var cv3 = new ChannelValue { Id = 3, Value = "0" };
        var cv4 = new ChannelValue { Id = 4, Value = "0" };
        var cv5 = new ChannelValue { Id = 5, Value = "0" };
        var cv6 = new ChannelValue { Id = 6, Value = "0" };
        channelRepo.SetChannelValueAsync(cv1).Wait();
        channelRepo.SetChannelValueAsync(cv2).Wait();
        channelRepo.SetChannelValueAsync(cv3).Wait();
        channelRepo.SetChannelValueAsync(cv4).Wait();
        channelRepo.SetChannelValueAsync(cv5).Wait();
        channelRepo.SetChannelValueAsync(cv6).Wait();

        var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
        var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
        var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 2, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));
        var dto3 = new ChannelMappingDto { Id = 3, BaseDecimalPlaces = 4, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(3)).Returns(Task.FromResult(new ChannelMapping(dto3)));
        var dto4 = new ChannelMappingDto { Id = 4, BaseDecimalPlaces = 4, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(4)).Returns(Task.FromResult(new ChannelMapping(dto3)));
        var dto5 = new ChannelMappingDto { Id = 5, BaseDecimalPlaces = 4, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(5)).Returns(Task.FromResult(new ChannelMapping(dto3)));
        var dto6 = new ChannelMappingDto { Id = 6, BaseDecimalPlaces = 4, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(6)).Returns(Task.FromResult(new ChannelMapping(dto3)));

        var me = new MathEvaluation(mathRepositoryMock.Object, channelRepo, channelMappingRepositoryMock.Object);

        // 1 + 3.5
        // 1 - 1.5
        // 1 * 2.34
        // 1 / 2.34
        me.RunCalculations().Wait();

        var output = channelRepo.GetChannelValueAsync(3);
        output.Wait();
        Assert.AreEqual("4.5000", output.Result.Value);

        output = channelRepo.GetChannelValueAsync(4);
        output.Wait();
        Assert.AreEqual("-0.5000", output.Result.Value);

        output = channelRepo.GetChannelValueAsync(5);
        output.Wait();
        Assert.AreEqual("2.3400", output.Result.Value);

        output = channelRepo.GetChannelValueAsync(6);
        output.Wait();
        Assert.AreEqual("0.4274", output.Result.Value);
    }

    /// <summary>
    /// trunc(value1 / a)
    /// </summary>
    [TestMethod]
    public void DivisionIntegerTest()
    {
        var mathRepositoryMock = new Mock<IMathRepository>();
        var parameters = new[] { new MathParameters { Id = 1, Order = 1, Type = MathType.DivisionInteger, Channel1Id = 1, OutputChannelId = 2, A = 3.5M } }.AsEnumerable();
        mathRepositoryMock.Setup(m => m.GetParameters()).Returns(Task.FromResult(parameters));

        var channelRepo = new ChannelMemoryRepository();
        var cv1 = new ChannelValue { Id = 1, Value = "5.334" };
        var cv2 = new ChannelValue { Id = 2, Value = "0" };
        channelRepo.SetChannelValueAsync(cv1).Wait();
        channelRepo.SetChannelValueAsync(cv2).Wait();

        var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
        var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 3, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
        var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 0, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));

        var me = new MathEvaluation(mathRepositoryMock.Object, channelRepo, channelMappingRepositoryMock.Object);

        // trunc(5.334 / 3.5)
        me.RunCalculations().Wait();

        var output = channelRepo.GetChannelValueAsync(2);
        output.Wait();
        Assert.AreEqual("1", output.Result.Value);
    }

    /// <summary>
    /// value1 % a
    /// </summary>
    [TestMethod]
    public void DivisionModuloTest()
    {
        var mathRepositoryMock = new Mock<IMathRepository>();
        var parameters = new[] { new MathParameters { Id = 1, Order = 1, Type = MathType.DivisionModulo, Channel1Id = 1, OutputChannelId = 2, A = 3.5M } }.AsEnumerable();
        mathRepositoryMock.Setup(m => m.GetParameters()).Returns(Task.FromResult(parameters));

        var channelRepo = new ChannelMemoryRepository();
        var cv1 = new ChannelValue { Id = 1, Value = "5.334" };
        var cv2 = new ChannelValue { Id = 2, Value = "0" };
        channelRepo.SetChannelValueAsync(cv1).Wait();
        channelRepo.SetChannelValueAsync(cv2).Wait();

        var channelMappingRepositoryMock = new Mock<IChannelMappingRepository>();
        var dto1 = new ChannelMappingDto { Id = 1, BaseDecimalPlaces = 3, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(1)).Returns(Task.FromResult(new ChannelMapping(dto1)));
        var dto2 = new ChannelMappingDto { Id = 2, BaseDecimalPlaces = 1, BaseUnitType = "ft", DisplayUnitType = "ft", DisplayDecimalPlaces = 1, DataType = "Length" };
        channelMappingRepositoryMock.Setup(c => c.GetChannelMappingAsync(2)).Returns(Task.FromResult(new ChannelMapping(dto2)));

        var me = new MathEvaluation(mathRepositoryMock.Object, channelRepo, channelMappingRepositoryMock.Object);

        // 5.334 % 3.5
        me.RunCalculations().Wait();

        var output = channelRepo.GetChannelValueAsync(2);
        output.Wait();
        Assert.AreEqual("1.0", output.Result.Value);
    }
}
