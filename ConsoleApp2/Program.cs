using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Windows.Forms;

namespace WebSocketServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://*:8181/");
            try
            {
                listener.Start();
                Console.WriteLine("WebSocket server listening on port 8181...");
                while (true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    if (!request.IsWebSocketRequest)
                    {
                        response.StatusCode = 400;
                        response.Close();
                        continue;
                    }

                    WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = webSocketContext.WebSocket;

                    Console.WriteLine("Client connected.");

                    while (webSocket.State == WebSocketState.Open)
                    {
                        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string message = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                            Console.WriteLine($"Received: {message}");

                            MessageBox.Show(message);

                            // Invia una risposta al client
                            await webSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes("Echo: " + message)), WebSocketMessageType.Text, true, CancellationToken.None);
                        }
                    }

                    Console.WriteLine("Client disconnected.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

