using JayyTee.MassTransitSample.Application.Features.PingPong;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace JayyTee.MassTransitSample.Api.PingPong;

public static class PingPongApi
{
    public static WebApplication MapPingPongApiEndpoints(this WebApplication app)
    {
        app.MapPost("ping", async Task<IResult>([FromServices] IPublishEndpoint publishEndpoint) =>
            {
                await publishEndpoint.Publish(new Ping
                {
                    Message = "Hello world"
                });
                return Results.Ok();
            })
            .WithName("Ping").WithGroupName("PingPong");

        return app;
    }
}
