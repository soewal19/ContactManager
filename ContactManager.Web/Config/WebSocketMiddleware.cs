using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace ContactManager.Web.Config
{
    /// <summary>
    /// Middleware for handling WebSocket connections with ping-pong support
    /// </summary>
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly ILogger<WebSocketMiddleware> _logger;

        public WebSocketMiddleware(RequestDelegate next, WebSocketConnectionManager connectionManager, ILogger<WebSocketMiddleware> logger)
        {
            _next = next;
            _connectionManager = connectionManager;
            _logger = logger;
        }

        /// <summary>
        /// Processes WebSocket requests
        /// </summary>
        /// <param name="context">HTTP context</param>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var connectionId = _connectionManager.AddConnection(webSocket);

                try
                {
                    await HandleWebSocketAsync(webSocket, connectionId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error handling WebSocket connection: {connectionId}");
                }
                finally
                {
                    await _connectionManager.RemoveConnectionAsync(connectionId);
                }
            }
            else
            {
                await _next(context);
            }
        }

        /// <summary>
        /// Handles WebSocket communication with ping-pong support
        /// </summary>
        /// <param name="webSocket">WebSocket instance</param>
        /// <param name="connectionId">Connection ID</param>
        private async Task HandleWebSocketAsync(WebSocket webSocket, string connectionId)
        {
            var buffer = new ArraySegment<byte>(new byte[4096]);

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);

                    if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer.Array ?? Array.Empty<byte>(), 0, result.Count);
                        await ProcessMessageAsync(connectionId, message);
                    }
                    else if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Connection closed by client", CancellationToken.None);
                        break;
                    }
                    else if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Binary)
                    {
                        // Handle binary messages (ping/pong frames)
                        await HandleBinaryMessageAsync(connectionId, buffer.Array, result.Count);
                    }
                }
                catch (WebSocketException ex)
                {
                    _logger.LogWarning(ex, $"WebSocket exception for connection: {connectionId}");
                    break;
                }
            }
        }

        /// <summary>
        /// Processes incoming text messages
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        /// <param name="message">Received message</param>
        private async Task ProcessMessageAsync(string connectionId, string message)
        {
            try
            {
                _logger.LogInformation($"Received text message from {connectionId}: {message}");

                // Try to parse as JSON message
                try
                {
                    var wsMessage = JsonConvert.DeserializeObject<WebSocketMessage>(message);
                    if (wsMessage != null)
                    {
                        await HandleWebSocketMessageAsync(connectionId, wsMessage);
                        return;
                    }
                }
                catch (JsonException)
                {
                    // Not a JSON message, treat as plain text
                }

                // Echo the message back for now (can be extended for specific commands)
                var response = new WebSocketMessage(ContactManagerMessageType.Info, null, $"Echo: {message}");
                await _connectionManager.SendToClientAsync(connectionId, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message from {connectionId}");
                var errorResponse = new WebSocketMessage(ContactManagerMessageType.Error, null, "Error processing message");
                await _connectionManager.SendToClientAsync(connectionId, errorResponse);
            }
        }

        /// <summary>
        /// Handles WebSocket-specific messages
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        /// <param name="message">WebSocket message</param>
        private async Task HandleWebSocketMessageAsync(string connectionId, WebSocketMessage message)
        {
            switch (message.Type)
            {
                case ContactManagerMessageType.Pong:
                    _connectionManager.HandlePong(connectionId);
                    break;

                case ContactManagerMessageType.Ping:
                    // Respond with pong
                    var pongResponse = new WebSocketMessage(ContactManagerMessageType.Pong, null, "pong");
                    await _connectionManager.SendToClientAsync(connectionId, pongResponse);
                    break;

                default:
                    // Handle other message types or broadcast to all clients
                    await _connectionManager.BroadcastAsync(message);
                    break;
            }
        }

        /// <summary>
        /// Handles binary messages (for potential future use)
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        /// <param name="data">Binary data</param>
        /// <param name="length">Data length</param>
        private async Task HandleBinaryMessageAsync(string connectionId, byte[]? data, int length)
        {
            if (data == null || length == 0) return;

            // For now, just log binary messages
            _logger.LogInformation($"Received binary message from {connectionId}: {length} bytes");
            
            // Echo back as binary
            // Note: For binary messages, we'll use the connection manager's broadcast functionality
            // since we don't have direct access to individual sockets here
            _logger.LogInformation($"Binary message from {connectionId}: {length} bytes (echo not implemented for binary)");
        }
    }

    /// <summary>
    /// Extension methods for WebSocket middleware
    /// </summary>
    public static class WebSocketMiddlewareExtensions
    {
        /// <summary>
        /// Adds WebSocket support to the application
        /// </summary>
        /// <param name="builder">Web application builder</param>
        public static void AddWebSocketSupport(this WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton<WebSocketConnectionManager>();
            builder.Services.AddWebSocketManager();
        }

        /// <summary>
        /// Adds WebSocket manager services
        /// </summary>
        /// <param name="services">Service collection</param>
        private static void AddWebSocketManager(this IServiceCollection services)
        {
            services.AddSingleton<WebSocketConnectionManager>();
        }

        /// <summary>
        /// Uses WebSocket middleware in the application pipeline
        /// </summary>
        /// <param name="app">Web application</param>
        public static void UseWebSocketMiddleware(this WebApplication app)
        {
            app.UseWebSockets();
            app.UseMiddleware<WebSocketMiddleware>();
        }
    }
}