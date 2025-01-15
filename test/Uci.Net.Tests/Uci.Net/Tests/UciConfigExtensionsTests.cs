using static Uci.Net.Tests.TestConfigs;

namespace Uci.Net.Tests;

public class UciConfigExtensionsTests(ITestOutputHelper output)
{
    public record TestCase(string Name, string Input, string ExpectedJson, string ExpectedUci)
    {
        public override string? ToString() => Name;
    }

    public static readonly IEnumerable<object[]> TestCases = new TestCase[]
    {
        new(nameof(OpenClash), OpenClash, OpenClashJson, OpenClashUci),
        new(nameof(SmartDns), SmartDns, SmartDnsJson, SmartDnsUci),
    }.SelectMany(m => new[]
    {
        m,
        m with { Name = m.Name + "_LF", Input = CRLF_TO_LF.Replace(m.Input) },
        m with { Name = m.Name + "_CRLF", Input = LF_TO_CRLF.Replace(m.Input) },
    }).Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ToStructuredJObject_Test(TestCase testCase)
    {
        var (_, input, expected, _) = testCase;
        var config = UciParser.Parse(input);
        var actual = config.ToStructuredJsonObject().ToJsonString(new JsonOptions { Indented = true });
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ToIniConfig_Test(TestCase testCase)
    {
        var (_, input, _, expected) = testCase;
        var config = UciParser.Parse(input);
        var actual = config.ToStructuredJsonObject().ToUciConfig().ToString();
        Assert.Equal(expected, actual);
    }
}
