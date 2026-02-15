/**
 * WebSocket client for real-time communication with Contact Manager backend
 * Provides connection management, message handling, ping-pong support, and event broadcasting
 */
class ContactManagerWebSocket {
    constructor(url = 'ws://localhost:5000/ws') {
        this.url = url;
        this.socket = null;
        this.reconnectInterval = 5000;
        this.shouldReconnect = true;
        this.messageHandlers = new Map();
        this.connectionId = null;
        this.pingInterval = 30000; // 30 seconds
        this.pingTimer = null;
        this.lastPongTime = DateTime.now();
        this.isWaitingForPong = false;
        
        this.setupMessageHandlers();
    }

    /**
     * Sets up default message handlers for different message types
     */
    setupMessageHandlers() {
        // Handle pong responses
        this.on('Pong', (message) => {
            this.lastPongTime = Date.now();
            this.isWaitingForPong = false;
            console.log('Pong received');
        });

        // Handle info messages
        this.on('Info', (message) => {
            console.log('Info:', message.Message);
            this.showNotification(message.Message, 'info');
        });

        // Handle error messages
        this.on('Error', (message) => {
            console.error('Error:', message.Message);
            this.showNotification(message.Message, 'error');
        });

        // Handle contact created
        this.on('ContactCreated', (message) => {
            console.log('Contact created:', message.Data);
            this.updateContactList();
            this.showNotification('New contact added', 'success');
        });

        // Handle contact updated
        this.on('ContactUpdated', (message) => {
            console.log('Contact updated:', message.Data);
            this.updateContactList();
            this.showNotification('Contact updated', 'success');
        });

        // Handle contact deleted
        this.on('ContactDeleted', (message) => {
            console.log('Contact deleted:', message.Data);
            this.updateContactList();
            this.showNotification('Contact deleted', 'success');
        });

        // Handle contacts imported
        this.on('ContactsImported', (message) => {
            console.log('Contacts imported:', message.Data);
            this.updateContactList();
            this.updateStatistics();
            this.showNotification(`Imported ${message.Data?.count || 0} contacts`, 'success');
        });

        // Handle statistics updated
        this.on('StatisticsUpdated', (message) => {
            console.log('Statistics updated:', message.Data);
            this.updateStatistics(message.Data);
        });
    }

    /**
     * Connects to the WebSocket server
     */
    connect() {
        try {
            this.socket = new WebSocket(this.url);
            
            this.socket.onopen = (event) => {
                console.log('WebSocket connected');
                this.showNotification('Connected to server', 'success');
                this.startPingPong();
                this.onConnected(event);
            };

            this.socket.onmessage = (event) => {
                this.handleMessage(event.data);
            };

            this.socket.onclose = (event) => {
                console.log('WebSocket disconnected');
                this.showNotification('Disconnected from server', 'warning');
                this.stopPingPong();
                this.onDisconnected(event);
                
                if (this.shouldReconnect) {
                    setTimeout(() => this.connect(), this.reconnectInterval);
                }
            };

            this.socket.onerror = (error) => {
                console.error('WebSocket error:', error);
                this.showNotification('WebSocket connection error', 'error');
                this.onError(error);
            };
        } catch (error) {
            console.error('Failed to create WebSocket:', error);
            this.showNotification('Failed to create WebSocket connection', 'error');
        }
    }

    /**
     * Starts ping-pong mechanism
     */
    startPingPong() {
        this.stopPingPong(); // Stop any existing timer
        
        this.pingTimer = setInterval(() => {
            if (this.socket && this.socket.readyState === WebSocket.OPEN) {
                // Check if we're waiting for a pong for too long
                if (this.isWaitingForPong && (Date.now() - this.lastPongTime > 60000)) {
                    console.warn('Pong timeout - connection may be dead');
                    this.socket.close();
                    return;
                }
                
                // Send ping
                this.send('Ping', null, 'ping');
                this.isWaitingForPong = true;
                console.log('Ping sent');
            }
        }, this.pingInterval);
    }

    /**
     * Stops ping-pong mechanism
     */
    stopPingPong() {
        if (this.pingTimer) {
            clearInterval(this.pingTimer);
            this.pingTimer = null;
        }
    }

    /**
     * Disconnects from the WebSocket server
     */
    disconnect() {
        this.shouldReconnect = false;
        this.stopPingPong();
        if (this.socket) {
            this.socket.close();
        }
    }

    /**
     * Handles incoming messages
     */
    handleMessage(data) {
        try {
            const message = JSON.parse(data);
            console.log('Received message:', message);

            // Store connection ID if provided
            if (message.connectionId) {
                this.connectionId = message.connectionId;
            }

            // Call registered handlers
            const handlers = this.messageHandlers.get(message.Type);
            if (handlers) {
                handlers.forEach(handler => handler(message));
            }
        } catch (error) {
            console.error('Error parsing message:', error);
        }
    }

    /**
     * Sends a message to the server
     */
    send(type, data = null, message = null) {
        if (this.socket && this.socket.readyState === WebSocket.OPEN) {
            const payload = {
                Type: type,
                Data: data,
                Message: message,
                Timestamp: new Date().toISOString()
            };
            
            this.socket.send(JSON.stringify(payload));
            console.log('Sent message:', payload);
        } else {
            console.warn('WebSocket is not connected');
        }
    }

    /**
     * Registers a message handler
     */
    on(messageType, handler) {
        if (!this.messageHandlers.has(messageType)) {
            this.messageHandlers.set(messageType, []);
        }
        this.messageHandlers.get(messageType).push(handler);
    }

    /**
     * Unregisters a message handler
     */
    off(messageType, handler) {
        const handlers = this.messageHandlers.get(messageType);
        if (handlers) {
            const index = handlers.indexOf(handler);
            if (index > -1) {
                handlers.splice(index, 1);
            }
        }
    }

    /**
     * Called when connection is established
     */
    onConnected(event) {
        // Override in subclasses or assign externally
    }

    /**
     * Called when connection is lost
     */
    onDisconnected(event) {
        // Override in subclasses or assign externally
    }

    /**
     * Called on connection error
     */
    onError(error) {
        // Override in subclasses or assign externally
    }

    /**
     * Updates the contact list (to be implemented by the page)
     */
    updateContactList() {
        // This should be overridden by the specific page implementation
        console.log('updateContactList called - override this method');
        
        // For the contacts page, reload the data
        if (typeof reloadContacts === 'function') {
            reloadContacts();
        }
    }

    /**
     * Updates statistics (to be implemented by the page)
     */
    updateStatistics(data = null) {
        // This should be overridden by the specific page implementation
        console.log('updateStatistics called - override this method');
        
        // For the contacts page, reload statistics
        if (typeof reloadStatistics === 'function') {
            reloadStatistics(data);
        }
    }

    /**
     * Shows a notification to the user
     */
    showNotification(message, type = 'info') {
        // Simple notification implementation
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        notification.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        
        document.body.appendChild(notification);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 5000);
    }

    /**
     * Gets connection status
     */
    get isConnected() {
        return this.socket && this.socket.readyState === WebSocket.OPEN;
    }

    /**
     * Gets connection ID
     */
    get connectionId() {
        return this.connectionId;
    }

    /**
     * Gets connection health status
     */
    get connectionHealth() {
        if (!this.isConnected) return 'disconnected';
        if (this.isWaitingForPong && (Date.now() - this.lastPongTime > 30000)) return 'unhealthy';
        return 'healthy';
    }
}

// Global WebSocket instance
let contactManagerWS = null;

/**
 * Initializes the WebSocket connection
 */
function initializeWebSocket() {
    if (contactManagerWS) {
        contactManagerWS.disconnect();
    }
    
    contactManagerWS = new ContactManagerWebSocket();
    contactManagerWS.connect();
    
    return contactManagerWS;
}

/**
 * Gets the current WebSocket instance
 */
function getWebSocket() {
    return contactManagerWS;
}

/**
 * Disconnects the WebSocket
 */
function disconnectWebSocket() {
    if (contactManagerWS) {
        contactManagerWS.disconnect();
        contactManagerWS = null;
    }
}

/**
 * Manually send a ping (for testing)
 */
function sendPing() {
    if (contactManagerWS && contactManagerWS.isConnected) {
        contactManagerWS.send('Ping', null, 'manual ping');
        console.log('Manual ping sent');
    } else {
        console.warn('WebSocket is not connected');
    }
}

/**
 * Get connection health status
 */
function getConnectionHealth() {
    return contactManagerWS ? contactManagerWS.connectionHealth : 'disconnected';
}

// Auto-initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initializeWebSocket();
});

// Cleanup on page unload
window.addEventListener('beforeunload', function() {
    disconnectWebSocket();
});