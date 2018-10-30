using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ConsoleApp1
{
    
// State object for reading client data asynchronously  
    public class StateObject {  
        // Client  socket.  
        public Socket workSocket = null;  
        // Size of receive buffer.  
        public const int BufferSize = 1024;  
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];  
        // Received data string.  
        public StringBuilder sb = new StringBuilder();    
        // Add our stopwatch
        public Stopwatch Stopwatch = new Stopwatch();
    }  

    public class AsynchronousSocketListener {  
        // Thread signal.  
        public static ManualResetEvent allDone = new ManualResetEvent(false);  

        public AsynchronousSocketListener() {  
        }  

        public static void StartListening() {  
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".  
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());  
            IPAddress ipAddress = ipHostInfo.AddressList[0];  
            Console.WriteLine(ipAddress);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 8080);  

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,  
                SocketType.Stream, ProtocolType.Tcp );  

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try {  
                listener.Bind(localEndPoint);  
                listener.Listen(100);  //max number of clients

                while (true) {  
                    // Set the event to nonsignaled state.  
                    allDone.Reset();  

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");  
                    listener.BeginAccept(   
                        new AsyncCallback(AcceptCallback),  
                        listener );  

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();  
                }  

            } catch (Exception e) {  
                Console.WriteLine(e.ToString());  
            }  

            Console.WriteLine("\nPress ENTER to continue...");  
            Console.Read();  

        }  

        public static void AcceptCallback(IAsyncResult ar) {  
            // Signal the main thread to continue.  
            allDone.Set();  

            // Get the socket that handles the client request.  
            Socket listener = (Socket) ar.AsyncState;  
            Socket handler = listener.EndAccept(ar);  

            // Create the state object.  
            StateObject state = new StateObject();  
            state.Stopwatch.Start();
            state.workSocket = handler;  
            handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,  
                new AsyncCallback(ReadCallback), state);  
        }  

        public static void ReadCallback(IAsyncResult ar) {  
            String content = String.Empty;  

            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject) ar.AsyncState;  
            Socket handler = state.workSocket;  

            // Read data from the client socket.   
            int bytesRead = handler.EndReceive(ar);  

            
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
            content = state.sb.ToString();

            string send = "";
            string line;
            try
            {
                using (var streamReader = new StreamReader("Files/" + content + ".txt"))
                {
                    line = streamReader.ReadLine();
                    while (line != null)
                    {
                        send += line + "\n";
                        line = streamReader.ReadLine();
                    }
                }
            }
            catch (IOException e)
            {
                //TODO: send error message back to client, write entry in log
                send = "File does not exist";
                Console.WriteLine("File " + content + " does not exist");
            }
            
            // All the data has been read from the   
            // client. Display it on the console.  
            Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",  
                content.Length, content );  
            // Echo the data back to the client.  
            String clientIp = handler.RemoteEndPoint.ToString();
            Send(handler, send);

            long time = state.Stopwatch.ElapsedMilliseconds;

            File.AppendAllText("Log/log.txt", "IP: " + clientIp + " Time:" + time + " File: " + content + ".txt" + Environment.NewLine);
        }  

        private static void Send(Socket handler, String data) {  
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);  

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,  
                new AsyncCallback(SendCallback), handler); 
            
        }  

        private static void SendCallback(IAsyncResult ar) {  
            try {  
                // Retrieve the socket from the state object.  
                Socket handler = (Socket) ar.AsyncState;  

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);  
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);  

                handler.Shutdown(SocketShutdown.Both);  
                handler.Close();  

            } catch (Exception e) {  
                Console.WriteLine(e.ToString());  
            }  
        }  

        public static int Main(String[] args) {  
            //FileGenerator.run();
            StartListening(); 
            return 0;  
        }  
    }
}