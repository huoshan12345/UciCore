using static Uci.Net.Tests.TestConfigs;

namespace Uci.Net.Tests;

public class UciParserTests
{
    public record TestCase(string Name, string Input, UciConfig Expected)
    {
        public override string ToString() => Name;
    }

    public static readonly IEnumerable<object[]> TestCases = new TestCase[]
    {
        new(nameof(Empty1), Empty1, new()),
        new(nameof(Empty2), Empty2, new()),
        new(nameof(Simple), Simple, new()
        {
            Sections =
            [
                new("sectiontype", "sectionname", new UciOption("optionname", "optionvalue")),
            ],
        }),
        new(nameof(MultiLine), MultiLine, new()
        {
            PackageName = "pkg_name",
            Sections =
            [
                new("empty"),
                new("squoted", "sqname"),
                new("dquoted", "dqname", new UciOption("multiline", $"line1\\{Environment.NewLine}\tline2")),
            ],
        }),
        new(nameof(Unquoted), Unquoted, new()
        {
            Sections =
            [
                new("foo", "bar", new UciOption("answer", "42")),
            ],
        }),
        new(nameof(Unnamed), Unnamed, new()
        {
            Sections =
            [
                new("foo", "named",
                    new("pos", "0"),
                    new("unnamed", "0"),
                    new("list", "0", true)),
                new("foo", "",
                    new("pos", "1"),
                    new("unnamed", "1"),
                    new("list", "10", true)),
                new("foo", "",
                    new("pos", "2"),
                    new("unnamed", "1"),
                    new("list", "20", true)),
                new("foo", "named",
                    new("pos", "3"),
                    new("unnamed", "0"),
                    new("list", "30", true)),
            ],
        }),
        new(nameof(Hyphenated), Hyphenated, new()
        {
            Sections =
            [
                new("wifi-device", "wl0",
                    new("type", "broadcom"),
                    new("channel", "6")),
                new("wifi-iface", "wifi0",
                    new("device", "wl0"),
                    new("mode", "ap")),
            ],
        }),
        new(nameof(Comment), Comment, new()
        {
            Sections =
            [
                new("foo", "",
                    new("opt1", "1"),
                    new("opt2", "3"),
                    new("opt3", "hello")),
            ],
        }),
        new(nameof(Quoted), Quoted, new()
        {
            Sections =
            [
                new("example", "test",
                    new("example", "value1"),
                    new("example", "value2"),
                    new("example", "value3"),
                    new("example", "value4"),
                    new("example", "value5")),
            ],
        }),
    }.Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void Parse_Test(TestCase testCase)
    {
        var (_, input, expected) = testCase;
        var actual = UciParser.Parse(input);
        Assert.Equivalent(expected, actual);
    }
}