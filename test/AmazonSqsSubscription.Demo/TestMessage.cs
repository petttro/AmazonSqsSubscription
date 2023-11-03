using Newtonsoft.Json;

namespace Package.Demo;

public class TestMessage
{
    public const string MessageType = "test_message";

    [JsonProperty("application")]
    public string Application { get; set; }

    [JsonProperty("action")]
    public string Action { get; set; }

    [JsonProperty("lastUpdateDateTime")]
    public string LastUpdateDateTime { get; set; }
}

