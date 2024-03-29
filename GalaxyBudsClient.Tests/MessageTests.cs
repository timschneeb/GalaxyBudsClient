using System.Reflection;
using FluentAssertions;
using GalaxyBudsClient.Message;
using GalaxyBudsClient.Message.Decoder;
using GalaxyBudsClient.Model.Constants;

namespace GalaxyBudsClient.Tests;

[TestFixture]
public abstract class MessageTests<T> where T : BaseMessageParser
{
    protected abstract string TestDataGroup { get; }
    
    public class TestCase
    {
        public required int Revision { init; get; }
        public required Models Model { init; get; }
        public required T ExpectedResult { init; get; }
    }
    
    protected void DecodeAndVerify(TestCase input)
    {
        var data = GetTestData(input.Model, input.Revision);
        var message = SppMessage.DecodeMessage(data, input.Model);
        
        var parser = (message.BuildParser() as T)!;
        parser.ParseMessage(message);
        
        parser.Should().BeEquivalentTo(input.ExpectedResult);
    }
    
    private byte[] GetTestData(Models model, int revision)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"GalaxyBudsClient.Tests.TestData.{TestDataGroup}.{model}_rev{revision}.bin";
        return ReadAll(assembly.GetManifestResourceStream(resourceName) ?? 
               throw new Exception($"Resource {resourceName} not found"));
    }
    
    private static byte[] ReadAll(Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
}