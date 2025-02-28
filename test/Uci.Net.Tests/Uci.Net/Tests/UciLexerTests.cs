using static Uci.Net.Tests.TestConfigs;

namespace Uci.Net.Tests;

public class UciLexerTests
{
    public record TestCase(string Name, string Input, UciToken[] Expected)
    {
        public override string? ToString() => Name;
    }

    // UciTokenType.SectionType.Make\("(\.+)"\), UciTokenType.SectionName.Make\("(\.+)"\)
    // ..UciTokens.Section("$1", "$2")
    public static readonly IEnumerable<object[]> TestCases = new TestCase[]
    {
        new(nameof(Empty1), Empty1, []),
        new(nameof(Empty2), Empty2, []),
        new("package name", "package pkg_name", [UciTokenType.PackageName.Make("pkg_name")]),
        new("package single quoted name", "package 'pkg_name'", [UciTokenType.PackageName.Make("pkg_name")]),
        new("package double quoted name", "package \"pkg_name\"", [UciTokenType.PackageName.Make("pkg_name")]),
        new("package mismatch quoted name", "package \"pkg_name'", [UciTokenType.Error.Make("unterminated quoted string")]),
        new(nameof(Quoted), Quoted,
        [
            ..UciTokens.Section("example", "test"),
            ..UciTokens.List("example", "value1"),
            ..UciTokens.List("example", "value2"),
            ..UciTokens.List("example", "value3"),
            ..UciTokens.List("example", "value4"),
            ..UciTokens.List("example", "value5"),
        ]),
        new(nameof(Simple), Simple,
        [
            ..UciTokens.Section("sectiontype", "sectionname"),
            ..UciTokens.Option("optionname", "optionvalue"),
        ]),
        new(nameof(MultiLine), MultiLine,
        [
            UciTokenType.PackageName.Make("pkg_name"),
            ..UciTokens.Section("empty"),
            ..UciTokens.Section("squoted", "sqname"),
            ..UciTokens.Section("dquoted", "dqname"),
            ..UciTokens.Option("multiline", $"line1\\{Environment.NewLine}\tline2"),
        ]),
        new(nameof(Unquoted), Unquoted,
        [
            ..UciTokens.Section("foo", "bar"),
            ..UciTokens.Option("answer", "42"),
        ]),
        new(nameof(Unnamed), Unnamed,
        [
            ..UciTokens.Section("foo", "named"),
            ..UciTokens.Option("pos", "0"),
            ..UciTokens.Option("unnamed", "0"),
            ..UciTokens.List("list", "0"),

            ..UciTokens.Section("foo"),
            ..UciTokens.Option("pos", "1"),
            ..UciTokens.Option("unnamed", "1"),
            ..UciTokens.List("list", "10"),

            ..UciTokens.Section("foo"),
            ..UciTokens.Option("pos", "2"),
            ..UciTokens.Option("unnamed", "1"),
            ..UciTokens.List("list", "20"),

            ..UciTokens.Section("foo", "named2"),
            ..UciTokens.Option("pos", "3"),
            ..UciTokens.Option("unnamed", "0"),
            ..UciTokens.List("list", "30"),
        ]),
        new(nameof(Hyphenated), Hyphenated,
        [
            ..UciTokens.Section("wifi-device", "wl0"),
            ..UciTokens.Option("type", "broadcom"),
            ..UciTokens.Option("channel", "6"),
            ..UciTokens.Section("wifi-iface", "wifi0"),
            ..UciTokens.Option("device", "wl0"),
            ..UciTokens.Option("mode", "ap"),
        ]),
        new(nameof(Comment), Comment,
        [
            UciTokenType.Comment.Make("# heading"),
            UciTokenType.Comment.Make("# another heading"),
            ..UciTokens.Section("foo"),
            ..UciTokens.Option("opt1", "1"),
            UciTokenType.Comment.Make("# option opt1 2"),
            ..UciTokens.Option("opt2", "3"),
            UciTokenType.Comment.Make("# baa"),
            ..UciTokens.Option("opt3", "hello"),
            UciTokenType.Comment.Make("# a comment block spanning"),
            UciTokenType.Comment.Make("# multiple lines, surrounded"),
            UciTokenType.Comment.Make("# by empty lines"),
            UciTokenType.Comment.Make("# eof"),
        ]),
        new(nameof(Invalid), Invalid,
        [
            UciTokenType.Error.Make("expected keyword (package, config, option, list) or eof but got: <?xml vers..."),
        ]),
        new(nameof(IncompletePackage), IncompletePackage,
        [
            UciTokenType.Error.Make("An identifier cannot be empty"),
        ]),
        new(nameof(UnterminatedQuoted), UnterminatedQuoted,
        [
            UciTokenType.SectionType.Make("foo"),
            UciTokenType.Error.Make("unterminated quoted string"),
        ]),
        new(nameof(SectionOverride), SectionOverride,
        [
            ..UciTokens.Section("interface", "lan"),
            ..UciTokens.Option("ifname", "eth0"),
            ..UciTokens.List("tag", "1"),
            ..UciTokens.Section("interface"),
            ..UciTokens.Option("ifname", "eth1"),
            ..UciTokens.Section("interface"),
            ..UciTokens.Option("ifname", "eth2"),
            ..UciTokens.Section("interface", "lan"),
            ..UciTokens.Option("ifname", "eth1"),
            ..UciTokens.List("tag", "2"),
        ]),
    }.Select(m => new object[] { m });

    [Theory]
    [MemberData(nameof(TestCases))]
    public void LexTokens_Test(TestCase testCase)
    {
        var (_, input, expected) = testCase;
        var lexer = new UciLexer(input);
        var i = 0;

        foreach (var it in lexer.LexTokens())
        {
            if (it.Type == UciTokenType.LineFeed)
                continue;

            if (i >= expected.Length)
            {
                Assert.Fail($"token {i}, unexpected item: {it}");
            }

            var item = expected[i];

            if (it.Type != item.Type)
            {
                Assert.Fail($"token {i}:\n expected: {item}\n actual:   {it}");
            }
            else if (it.Value != item.Value)
            {
                if (it.Type == UciTokenType.Error && it.Value.TrimEnd("...").StartsWith(item.Value.TrimEnd("...")))
                {
                    // ignore the rest of the error message
                }
                else
                {
                    Assert.Fail($"token {i} of {it.Type}:\n expected: {item.Value}\n actual:   {it.Value}");
                }
            }

            i++;
        }

        if (expected.Length is var len && len != i)
        {
            Assert.Fail($"expected to lex {len} items, actually lexed {i}");
        }
    }
}