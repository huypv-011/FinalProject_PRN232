using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BookingVilla.DTOs;

public class ODataListResponse<T>
{
    [JsonPropertyName("@odata.count")]
    public int? Count { get; set; }

    [JsonPropertyName("value")]
    public List<T> Value { get; set; } = new();
}

