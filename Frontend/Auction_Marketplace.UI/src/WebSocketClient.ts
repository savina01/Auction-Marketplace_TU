class WebSocketClient {
    socket: WebSocket;
  
    constructor(endpoint: string) {
      this.socket = new WebSocket(endpoint);
    }
  
    sendMessage(message: string) {
      this.socket.send(message);
    }
  
    closeConnection() {
      this.socket.close();
    }
  }
  
  export default WebSocketClient;
  