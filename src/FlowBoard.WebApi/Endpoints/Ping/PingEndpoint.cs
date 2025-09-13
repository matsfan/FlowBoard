using FastEndpoints;

namespace FlowBoard.WebApi.Endpoints.Ping;

public class PingEndpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Get("/ping");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Health style ping endpoint";
            s.Description = "Quick connectivity check returns 'pong'";
            s.Responses[200] = "pong response";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.StringAsync("pong", cancellation: ct);
    }
}
