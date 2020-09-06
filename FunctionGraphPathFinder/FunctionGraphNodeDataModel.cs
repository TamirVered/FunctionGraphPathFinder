using System.Text.Json.Serialization;

namespace FunctionGraphPathFinder
{
    /// <summary>
    /// Reflects the JSON structure of a function in a global call-graph.
    /// </summary>
    public class FunctionGraphNodeDataModel
    {
        /// <summary>
        /// Specifies te name of the function represented by this instance of <see cref="FunctionGraphNodeDataModel"/>.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Specifies the size of the function represented by this instance of <see cref="FunctionGraphNodeDataModel"/>, in bytes.
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// Specifies a collection of functions being used by the function represented by this instance of <see cref="FunctionGraphNodeDataModel"/>.
        /// </summary>
        [JsonPropertyName("imports")]
        public string[] Imports { get; set; }
    }
}
