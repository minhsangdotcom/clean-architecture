using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.SubcutaneousTests.Dtos.Requests;

public class HttpTestRequest
{
    public string Uri { get; set; } = string.Empty;
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public object? Payload { get; set; }
    public string ContentType { get; set; } = "application/json";
    public string? Token { get; set; }
}
