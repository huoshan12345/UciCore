using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using static UciCore.Tests.TestConfigs;

namespace UciCore.Tests;

public class UciConfigExtensionsTests(ITestOutputHelper output)
{
    public record TestCase(string Name, string Input, string ExpectedJson, string ExpectedUci)
    {
        public override string ToString() => Name;
    }

    public static readonly IEnumerable<object[]> TestCases = new TestCase[]
    {
        new(nameof(Dhcp), Dhcp, DhcpJson, DhcpUci),
        new(nameof(SmartDns), SmartDns, SmartDnsJson, SmartDnsUci),
        new(nameof(SectionOverride), SectionOverride, SectionOverrideJson, SectionOverrideUciFromJson),
    }.SelectMany(m => new[]
    {
        m,
        m with { Name = m.Name + "_LF", Input = RegexReplacer.CRLF_TO_LF.Replace(m.Input) },
        m with { Name = m.Name + "_CRLF", Input = RegexReplacer.LF_TO_CRLF.Replace(m.Input) },
    }).Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ToSerializableJsonObject_Test(TestCase testCase)
    {
        var (_, input, expected, _) = testCase;
        var config = UciParser.Parse(input);
        var actual = config.ToSerializableJsonObject().ToJsonString(new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ToUciConfig_Test(TestCase testCase)
    {
        var (_, input, _, expected) = testCase;
        var config = UciParser.Parse(input);
        var actual = config.ToSerializableJsonObject().ToUciConfig().ToString();
        output.WriteLine(actual);
        Assert.Equal(expected, actual);
    }
}
