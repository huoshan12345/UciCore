using System.Text.RegularExpressions;
using static Uci.Net.Tests.TestConfigs;

namespace Uci.Net.Tests;

public class UciConfigTests
{
    public record TestCase(string Name, string Input, string Expected)
    {
        public override string? ToString() => Name;
    }

    public static readonly IEnumerable<object[]> TestCases = new TestCase[]
    {
        new(nameof(Dhcp), Dhcp, Dhcp),
        new(nameof(SmartDns), SmartDns, SmartDns),
    }.SelectMany(m => new[]
    {
        m,
        m with { Name = m.Name + "_LF", Input = RegexReplacer.CRLF_TO_LF.Replace(m.Input) },
        m with { Name = m.Name + "_CRLF", Input = RegexReplacer.LF_TO_CRLF.Replace(m.Input) },
    }).Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ToString_Test(TestCase testCase)
    {
        var (_, input, expected) = testCase;
        var config = UciParser.Parse(input);
        Assert.Equal(expected.Trim(), config.ToString().Trim());
    }
}