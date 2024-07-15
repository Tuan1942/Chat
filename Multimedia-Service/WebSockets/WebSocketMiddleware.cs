using Microsoft.EntityFrameworkCore;
using MultimediaService.Context;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace MultimediaService.WebSockets
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private static ConcurrentDictionary<string, ConcurrentBag<WebSocket>> _sockets = new ConcurrentDictionary<string, ConcurrentBag<WebSocket>>();

        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var userId = context.Request.Query["UserID"].ToString();

                using (var scope = context.RequestServices.CreateScope())
                {
                    var userContext = scope.ServiceProvider.GetRequiredService<UserContext>();
                    var user = await Validate(userContext, userId);

                    if (user == null)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("No user found.");
                        return;
                    }

                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    if (!_sockets.ContainsKey(userId))
                    {
                        _sockets[userId] = new ConcurrentBag<WebSocket>();
                    }

                    _sockets[userId].Add(webSocket);

                    await HandleWebSocketAsync(webSocket, user);
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleWebSocketAsync(WebSocket webSocket, User user)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                var receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = JsonSerializer.Deserialize<Dictionary<string, string>>(receivedText);

                Console.WriteLine($"Received from {user.Username}: {message["content"]}");
                var responseText = $"Echo from {user.Username}: {message["content"]}";
                var responseBytes = Encoding.UTF8.GetBytes(responseText);
                await webSocket.SendAsync(new ArraySegment<byte>(responseBytes, 0, responseBytes.Length), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async Task<User> Validate(UserContext userContext, string userId)
        {
            return await userContext.Users.FirstOrDefaultAsync(u => u.Id == int.Parse(userId));
        }

        public static async Task NotifyUser(string userId, string message)
        {
            if (_sockets.TryGetValue(userId, out var userSockets))
            {
                var responseBytes = Encoding.UTF8.GetBytes(message);
                foreach (var socket in userSockets)
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        await socket.SendAsync(new ArraySegment<byte>(responseBytes, 0, responseBytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }
    }
}
