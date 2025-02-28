namespace UciCore.Tests;

public static class TestConfigs
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");

    public const string Empty1 = "";
    public const string Empty2 = "  \n\t\n\n \n ";

    public const string Simple = """
                                       config sectiontype 'sectionname'
                                           option optionname 'optionvalue'
                                       """;

    public const string MultiLine = $"""
                                       package "pkg_name"
                                       config empty
                                       config squoted 'sqname'
                                       config dquoted "dqname"
                                       option multiline 'line1\
                                       {"\t"}line2'
                                       """;

    public const string Unquoted = "config foo bar\noption answer 42\n";
    
    public const string Unnamed = """
                                    config foo named
                                        option pos '0'
                                        option unnamed '0'
                                        list list 0

                                    config foo
                                        option pos '1'
                                        option unnamed '1'
                                        list list 10

                                    config foo
                                        option pos '2'
                                        option unnamed '1'
                                        list list 20

                                    config foo named2
                                        option pos '3'
                                        option unnamed '0'
                                        list list 30
                                    """;


    public const string Hyphenated = """
                                           config wifi-device wl0
                                               option type    'broadcom'
                                               option channel '6'

                                           config wifi-iface wifi0
                                               option device 'wl0'
                                               option mode 'ap'
                                           """;


    public const string Comment = """
                                   # heading

                                   # another heading
                                   config foo
                                   	option opt1 1
                                   	# option opt1 2
                                   	option opt2 3# baa
                                   	option opt3 hello

                                   # a comment block spanning
                                   # multiple lines, surrounded
                                   # by empty lines

                                   # eof
                                   """;


    public const string Invalid = """
                                     <?xml version="1.0">
                                     <error message="not a UCI file" />
                                     """;


    public const string IncompletePackage = "package";


    public const string UnterminatedQuoted = """
                                              config foo "bar
                                              """;

    public const string Quoted = """
                                  config 'example' 'test'
                                      list example   value1
                                      list  example  "value2"
                                      list 'example'  value3
                                      list 'example' "value4"
                                      list "example" 'value5'
                                  """;

    private static string ReadTestFile(string fileName)
    {
        return TestDataDir
            .Pipe(m => Path.Combine(m, fileName))
            .Pipe(File.ReadAllText);
    }

    public static readonly string Dhcp = ReadTestFile("dhcp");
    public static readonly string DhcpUci = ReadTestFile("dhcp.uci");
    public static readonly string DhcpJson = ReadTestFile("dhcp.json");

    public static readonly string SmartDns = ReadTestFile("smartdns");
    public static readonly string SmartDnsUci = ReadTestFile("smartdns.uci");
    public static readonly string SmartDnsJson = ReadTestFile("smartdns.json");

    public static readonly string SectionOverride = ReadTestFile("section_override");
    public static readonly string SectionOverrideUci = ReadTestFile("section_override.uci");
    public static readonly string SectionOverrideJson = ReadTestFile("section_override.json");
    public static readonly string SectionOverrideUciFromJson = ReadTestFile("section_override.json.uci");
}