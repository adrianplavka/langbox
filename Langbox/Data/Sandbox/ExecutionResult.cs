using Newtonsoft.Json;
using OneOf;

namespace Langbox.Data.Sandbox
{
    public abstract class ExecutionResult 
        : OneOfBase<
            ExecutionResult.TestSucceeded,
            ExecutionResult.Timeout,
            ExecutionResult.BuildFailed,
            ExecutionResult.TestFailed,
            ExecutionResult.UnknownFailure>
    {
        [JsonObject(MemberSerialization.OptIn)]
        public class TestSucceeded : ExecutionResult
        {
            [JsonProperty]
            public string Type { get; set; } = "";

            [JsonProperty]
            public string Stdout { get; set; } = "";
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Timeout : ExecutionResult
        {
            [JsonProperty]
            public string Type { get; set; } = "";
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class BuildFailed : ExecutionResult
        {
            [JsonProperty]
            public string Type { get; set; } = "";

            [JsonProperty]
            public string Stdout { get; set; } = "";

            [JsonProperty]
            public string Stderr { get; set; } = "";
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class TestFailed : ExecutionResult
        {
            [JsonProperty]
            public string Type { get; set; } = "";

            [JsonProperty]
            public string Stdout { get; set; } = "";

            [JsonProperty]
            public string Stderr { get; set; } = "";
        }

        public class UnknownFailure : ExecutionResult
        {

        }
    }
}
