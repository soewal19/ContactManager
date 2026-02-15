using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace ContactManager.Web.Config
{
    /// <summary>
    /// Manages WebSocket connections with ping-pong support for connection health monitoring
    /// </summary>
    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly ConcurrentDictionary<string, DateTime> _lastPingTimes = new();
        private readonly ConcurrentDictionary<string, Timer> _pingTimers = new();
        private readonly ILogger<WebSocketConnectionManager> _logger;
        private readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(30);
        private readonly TimeSpan _timeoutThreshold = TimeSpan.FromMinutes(2);

        public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Adds a new WebSocket connection and starts ping monitoring
        /// </summary>
        /// <param name="socket">WebSocket instance</param>
        /// <returns>Connection ID</returns>
        public string AddConnection(WebSocket socket)
        {
            var connectionId = Guid.NewGuid().ToString();
            _connections.TryAdd(connectionId, socket);
            _lastPingTimes.TryAdd(connectionId, DateTime.UtcNow);
            
            // Start ping timer for this connection
            var timer = new Timer(async _ => await SendPingAsync(connectionId), null, _pingInterval, _pingInterval);
            _pingTimers.TryAdd(connectionId, timer);
            
            _logger.LogInformation($"WebSocket connection added: {connectionId}");
            return connectionId;
        }

        /// <summary>
        /// Removes a WebSocket connection and stops ping monitoring
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        public async Task RemoveConnectionAsync(string connectionId)
        {
            if (_pingTimers.TryRemove(connectionId, out var timer))
            {
                timer.Dispose();
            }

            if (_connections.TryRemove(connectionId, out var socket))
            {
                _lastPingTimes.TryRemove(connectionId, out _);
                
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
                socket.Dispose();
                _logger.LogInformation($"WebSocket connection removed: {connectionId}");
            }
        }

        /// <summary>
        /// Sends a ping message to the specified connection
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        private async Task SendPingAsync(string connectionId)
        {
            if (!_connections.TryGetValue(connectionId, out var socket) || socket.State != WebSocketState.Open)
            {
                await RemoveConnectionAsync(connectionId);
                return;
            }

            try
            {
                var pingMessage = new WebSocketMessage(ContactManagerMessageType.Ping, null, "ping");
                await SendToClientAsync(connectionId, pingMessage);
                
                // Check for timeout
                if (_lastPingTimes.TryGetValue(connectionId, out var lastPing))
                {
                    if (DateTime.UtcNow - lastPing > _timeoutThreshold)
                    {
                        _logger.LogWarning($"Connection {connectionId} timed out - no pong received");
                        await RemoveConnectionAsync(connectionId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending ping to connection {connectionId}");
                await RemoveConnectionAsync(connectionId);
            }
        }

        /// <summary>
        /// Handles pong response from client
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        public void HandlePong(string connectionId)
        {
            _lastPingTimes.AddOrUpdate(connectionId, DateTime.UtcNow, (_, __) => DateTime.UtcNow);
            _logger.LogDebug($"Pong received from connection {connectionId}");
        }

        /// <summary>
        /// Sends a message to a specific client
        /// </summary>
        /// <param name="connectionId">Connection ID</param>
        /// <param name="message">Message to send</param>
        public async Task SendToClientAsync(string connectionId, WebSocketMessage message, CancellationToken cancellationToken = default)
        {
            if (_connections.TryGetValue(connectionId, out var socket) && socket.State == WebSocketState.Open)
            {
                var jsonMessage = JsonConvert.SerializeObject(message);
                var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
                var buffer = new ArraySegment<byte>(messageBytes);
                
                await socket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken);
                _logger.LogDebug($"Sent {message.Type} message to connection {connectionId}");
            }
        }

        /// <summary>
        /// Broadcasts a message to all connected clients
        /// </summary>
        /// <param name="message">Message to broadcast</param>
        public async Task BroadcastAsync(object message, CancellationToken cancellationToken = default)
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            var buffer = new ArraySegment<byte>(messageBytes);

            var tasks = _connections.Values
                .Where(socket => socket.State == WebSocketState.Open)
                .Select(socket => socket.SendAsync(buffer, System.Net.WebSockets.WebSocketMessageType.Text, true, cancellationToken))
                .ToArray();

            if (tasks.Length > 0)
            {
                await Task.WhenAll(tasks);
                _logger.LogInformation($"Broadcasted message to {tasks.Length} clients");
            }
        }

        /// <summary>
        /// Gets all active connection IDs
        /// </summary>
        public IEnumerable<string> GetConnectionIds()
        {
            return _connections.Keys;
        }

        /// <summary>
        /// Gets the number of active connections
        /// </summary>
        public int ConnectionCount => _connections.Count;
    }

    /// <summary>
    /// Types of WebSocket messages for Contact Manager
    /// </summary>
    public enum ContactManagerMessageType
    {
        Ping,
        Pong,
        ContactCreated,
        ContactUpdated,
        ContactDeleted,
        ContactsImported,
        Error,
        Info,
        StatisticsUpdated
    }

    /// <summary>
    /// WebSocket message structure
    /// </summary>
    public class WebSocketMessage
    {
        public ContactManagerMessageType Type { get; set; }
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Message { get; set; }

        public WebSocketMessage(ContactManagerMessageType type, object? data = null, string? message = null)
        {
            Type = type;
            Data = data;
            Message = message;
        }
    }
}