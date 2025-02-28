# UciCore [![LICENSE](https://img.shields.io/github/license/mashape/apistatus.svg)](LICENSE.TXT) [![Build](https://github.com/huoshan12345/UciCore/actions/workflows/build.yml/badge.svg)](https://github.com/huoshan12345/UciCore/actions/workflows/build.yml)

UciCore is a .NET library for parsing and generating UCI (Unified Configuration Interface) configurations. UCI is a configuration system commonly used in OpenWrt and other embedded Linux systems.

## Features

- Parse UCI configuration files into strongly-typed objects
- Generate UCI configuration files from object models
- Manipulate UCI configurations programmatically
- Full support for UCI syntax including packages, sections, options, and lists

## Latest Builds

||TargetFramework|Package|
|----|----|----|
|UciCore|![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-30a14e.svg) ![net6.0](https://img.shields.io/badge/net-6.0-30a14e.svg) ![net7.0](https://img.shields.io/badge/net-7.0-30a14e.svg) ![net8.0](https://img.shields.io/badge/net-8.0-30a14e.svg) ![net9.0](https://img.shields.io/badge/net-9.0-30a14e.svg) |[![](https://img.shields.io/nuget/v/UciCore?logo=nuget&label=nuget)](https://www.nuget.org/packages/UciCore)|

## Installation

```bash
dotnet add package UciCore
```

## Usage

### Parsing UCI Configuration

```csharp
// Parse configuration from string
string configContent = @"
package network

config interface 'lan'
    option type 'bridge'
    option ifname 'eth0'
    option proto 'static'
    option ipaddr '192.168.1.1'
    option netmask '255.255.255.0'
";

UciConfig config = UciParser.Parse(configContent);

// Access configuration data
Console.WriteLine($"Package: {config.PackageName}");
foreach (var section in config.Sections)
{
    Console.WriteLine($"Section: {section.Type} '{section.Name}'");
    foreach (var option in section.Options)
    {
        Console.WriteLine($"  {(option.IsList ? "list" : "option")} {option.Key} = '{option.Value}'");
    }
}
```

### Creating UCI Configuration

```csharp
var config = new UciConfig
{
    PackageName = "network",
    Sections =
    [
        new UciSection("interface", "lan")
        {
            Options =
            [
                new UciOption("type", "bridge"),
                new UciOption("ifname", "eth0"),
                new UciOption("proto", "static"),
                new UciOption("ipaddr", "192.168.1.1"),
                new UciOption("netmask", "255.255.255.0")
            ]
        },
        new UciSection("interface", "wan")
        {
            Options =
            [
                new UciOption("type", "bridge"),
                new UciOption("ifname", "eth1"),
                new UciOption("proto", "dhcp")
            ]
        }
    ]
};

// Generate UCI configuration string
string outputConfig = config.ToString();
Console.WriteLine(outputConfig);
```

### Deserialize UCI Configuration to custom config type using json

```csharp
// custom config type
public class NetworkConfig
{
    // named interface section
    [JsonPropertyName("lan")]
    public NetworkInterface? Lan { get; set; }

    // unnamed interface sections
    [JsonPropertyName("interface")]
    public NetworkInterface[]? Interfaces { get; set; }
}

public class NetworkInterface
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("ifname")]
    public string? IfName { get; set; }
    [JsonPropertyName("proto")]
    public string? Proto { get; set; }
    [JsonPropertyName("ipaddr")]
    public string? IpAddr { get; set; }
    [JsonPropertyName("netmask")]
    public string? NetMask { get; set; }
}

const string configContent = @"
config interface 'lan'
    option type 'bridge'
    option ifname 'eth0'
    option proto 'static'
    option ipaddr '192.168.1.1'
    option netmask '255.255.255.0'
    
config interface
    option ifname 'eth1'
    
config interface
   option ifname 'eth2'";

UciConfig config = UciParser.Parse(configContent);
// convert UciConfig to JsonObject that is able to be deserialized to custom type.
JsonObject jsonObject = config.ToSerializableJsonObject();
NetworkConfig? network = jsonObject.Deserialize<NetworkConfig>();
```

## TODO

- Support reading and writing comments (current parser just ignores comments)

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
