// ReSharper disable SuggestVarOrType_SimpleTypes

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace UciCore.Tests;

public class Example
{
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

    [Fact]
    public void ToSerializableJsonObject()
    {
        // ReSharper disable once UseRawString
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
        JsonObject jsonObject = config.ToSerializableJsonObject();
        NetworkConfig? network = jsonObject.Deserialize<NetworkConfig>();

        Assert.NotNull(network);
        Assert.NotNull(network.Lan);

        NetworkInterface lan = network.Lan;

        Assert.Equal("bridge", lan.Type);
        Assert.Equal("eth0", lan.IfName);
        Assert.Equal("static", lan.Proto);
        Assert.Equal("192.168.1.1", lan.IpAddr);
        Assert.Equal("255.255.255.0", lan.NetMask);

        Assert.NotNull(network.Interfaces);
        Assert.Equal(2, network.Interfaces.Length);

        NetworkInterface eth1 = network.Interfaces[0];
        Assert.Equal("eth1", eth1.IfName);

        NetworkInterface eth2 = network.Interfaces[1];
        Assert.Equal("eth2", eth2.IfName);
    }
}