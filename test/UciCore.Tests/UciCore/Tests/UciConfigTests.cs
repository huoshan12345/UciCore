using static UciCore.Tests.TestConfigs;

namespace UciCore.Tests;

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
        new(nameof(SectionOverride), SectionOverride, SectionOverrideUci),
    }.SelectMany(m => new[]
    {
        m,
        m with { Name = m.Name + "_LF", Input = m.Input.CrLfToLf() },
        m with { Name = m.Name + "_CRLF", Input = m.Input.LfToCrLf() },
    }).Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void ToString_Test(TestCase testCase)
    {
        var (_, input, expected) = testCase;
        var config = UciParser.Parse(input);
        Assert.Equal(expected.Trim(), config.ToString().Trim());
    }

    [Fact]
    public void Sections_SameName_DifferentTypes_Throw()
    {
        const string text = """
                            config interface 'lan'
                               option type 'bridge'
                               
                            config dhcp 'lan'
                               option ifname 'eth1'
                            """;
        var ex = Assert.Throws<UciException>(() => UciParser.Parse(text));
        Assert.Contains("Parse error (section of different type 'dhcp' overwrites prior section 'interface' with same name 'lan')", ex.Message);
    }
}