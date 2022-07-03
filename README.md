# MassTransit Sample Solution

This repo serves as a proving ground and sample for boilerplate required to build/run/test an API/Worker combo with MassTransit on RabbitMQ.

Contrived examples are offered to demonstrate different mechanismns to test message-based interactions with a headless background service.

## Testing scenarios
| Scenario | Explanation |
| ---------|-------------|
| Ping/Pong | Asserts that when a message is placed on a bus, an operation occurs and eventually another message is raised in response |
| Password Reset |  This scenario tests a requirement that when a command is sent to an endpoint, eventually an email notification is sent to a known recipient. |

### Ping/Pong
This scenario is simplistic, though demonstrates a scenario whereby a test can temporarily wire itself up to a message bus and manipulate a service whilst listening out for expected responses which can be consumed and inspected by the test.

This could be used as a smoke-test of sorts against a candidate service prior to (or after) deployment.

### Password Reset
This scenario mimics a real-world situation whereby a password reset is requested and message placed on a queue to send a notification email to the user.

In this repo, two tests are provided to cover the above with slight variations:
1) A test that asserts that [eventually] an acutal email is sent to an isolated email service, and
2) A test that utilise the self-hosted service to inject a stunt-double email provider.

The use of one style over another would depend on circumstance and the acceptability of tradeoffs: On one hand, the in-process replacement of a service allows complete isolation of the service but does leak some implementation knowledge to the test. Alternatively, the other test relies on a fully isolated docker container to test side-effects exiting the service.


## References
| Author/Repo | URL |
| -- | -- |
| greyhamwoohoo: Test-dotnet-core-api-3 |	https://github.com/greyhamwoohoo/test-dotnet-core-api-3 |