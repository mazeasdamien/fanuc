using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FRRobot;
using System.Collections.Concurrent;

namespace FanucRobotServer
{
    public class TCPServer
    {
        private TcpListener _server;
        private IPAddress _localAddr;
        private int _port;
        private FRCRobot _robot;
        private ConcurrentDictionary<TcpClient, byte> _clients = new ConcurrentDictionary<TcpClient, byte>();
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private FRCJoint _prevJoint;
        private FRCXyzWpr _prevXyzWpr;

        public TCPServer(int port)
        {
            _localAddr = IPAddress.Parse("127.0.0.1");
            _port = port;
            _server = new TcpListener(_localAddr, _port);
            ConnectToRobot();
        }

        public void ConnectToRobot()
        {
            try
            {
                _robot = new FRCRobot();
                //_robot.ConnectEx("127.0.0.1", false, 10, 1);
                _robot.ConnectEx("192.168.1.20", false, 10, 1);
                Console.WriteLine("Connected to robot successfully.");
                FRCAlarms fRCAlarms = _robot.Alarms;
                FRCTasks mobjTasks = _robot.Tasks;
                FRCPrograms fRCPrograms = _robot.Programs;
                Thread.Sleep(500);
                mobjTasks.AbortAll();
                Thread.Sleep(500);
                fRCAlarms.Reset();
                fRCPrograms.Selected = "DAMIEN";
                FRCTPProgram fRCProgram = (FRCTPProgram)fRCPrograms[fRCPrograms.Selected, Type.Missing, Type.Missing];
                fRCProgram.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to connect to robot: " + e.Message);
            }
        }

        public void Start()
        {
            _server.Start();
            Console.WriteLine("Server started on " + _localAddr + ":" + _port);
            WaitForClientConnect();

            // Continuously send data to clients in a separate task
            Task.Run(async () => await SendDataToClientContinuously(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
            _server.Stop();
            Console.WriteLine("Server stopped.");
        }

        private void WaitForClientConnect()
        {
            _server.BeginAcceptTcpClient(new AsyncCallback(ClientConnected), null);
        }

        private void ClientConnected(IAsyncResult ar)
        {
            TcpClient client = _server.EndAcceptTcpClient(ar);
            Console.WriteLine("Client connected");

            // Add the connected client to the clients dictionary
            _clients.TryAdd(client, 0);

            ThreadPool.QueueUserWorkItem(HandleClientComm, client);

            WaitForClientConnect();
        }


        private void RemoveClientFromList(TcpClient client)
        {
            Console.WriteLine("Client disconnected");

            // Remove the disconnected client from the clients dictionary
            byte removed;
            _clients.TryRemove(client, out removed);
        }

        private async Task SendDataToClientContinuously(CancellationToken cancellationToken)
        {
            string previousMessage = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                var curPosition = _robot.CurPosition;
                var groupPositionJoint = curPosition.Group[1, FRECurPositionConstants.frJointDisplayType];
                var groupPositionWorld = curPosition.Group[1, FRECurPositionConstants.frWorldDisplayType];
                groupPositionJoint.Refresh(); // Refresh the joint position
                groupPositionWorld.Refresh(); // Refresh the world position
                var joint = (FRCJoint)groupPositionJoint.Formats[FRETypeCodeConstants.frJoint];
                var xyzWpr = (FRCXyzWpr)groupPositionWorld.Formats[FRETypeCodeConstants.frXyzWpr];

                string message = $"{joint[1]:F4},{joint[2]:F4},{joint[3]:F4},{joint[4]:F4},{joint[5]:F4},{joint[6]:F4},{xyzWpr.X:F4},{xyzWpr.Y:F4},{xyzWpr.Z:F4},{xyzWpr.W:F4},{xyzWpr.P:F4},{xyzWpr.R:F4}";

                if (previousMessage == null || previousMessage != message)
                {
                    SendDataToClient(message + "\n");
                    previousMessage = message;
                }

                // Adjust the delay as needed to control the frequency of updates
                await Task.Delay(1, cancellationToken);
            }
        }


        private void SendDataToClient(string message)
        {
            // Check if there are any clients in the dictionary before sending the message
            if (_clients.Count > 0)
            {
                foreach (TcpClient client in _clients.Keys)
                {
                    try
                    {
                        NetworkStream stream = client.GetStream();

                        //Console.WriteLine("Sending to Unity: " + message); // Add this line to display the sent data
                        byte[] msg = Encoding.ASCII.GetBytes(message);
                        stream.Write(msg, 0, msg.Length);
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Client disconnected: " + ex.Message);
                        RemoveClientFromList(client);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine("Client disconnected: " + ex.Message);
                        RemoveClientFromList(client);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error sending message to client: " + ex.Message);
                        // You can decide whether to remove the client from the list or not, depending on the exception type and your use case
                    }
                }
            }
        }

        private void HandleClientComm(object clientObj)
        {
            TcpClient client = (TcpClient)clientObj;
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;

            try
            {
                // Keep the connection alive
                while (client.Connected)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        string[] values = receivedData.Split(',');
                        Console.WriteLine("Data received from client: " + receivedData);

                        FRCSysPositions fRCTPPositions = _robot.RegPositions;
                        FRCSysPosition sysPosition = fRCTPPositions[3];
                        FRCSysGroupPosition sysGroupPosition = sysPosition.Group[1];
                        FRCXyzWpr xyzWpr = sysGroupPosition.Formats[FRETypeCodeConstants.frXyzWpr];

                        xyzWpr.X = float.Parse(values[0]);
                        xyzWpr.Y = float.Parse(values[1]);
                        xyzWpr.Z = float.Parse(values[2]);
                        xyzWpr.W = float.Parse(values[3]);
                        xyzWpr.P = float.Parse(values[4]);
                        xyzWpr.R = float.Parse(values[5]);

                        if (sysGroupPosition.IsReachable[Type.Missing, FREMotionTypeConstants.frJointMotionType, FREOrientTypeConstants.frAESWorldOrientType, Type.Missing, out _])
                        {
                            sysGroupPosition.Update();
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Client disconnected: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in HandleClientComm: " + ex.Message);
            }
            finally
            {
                RemoveClientFromList(client);
                client.Close();
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            int port = 5000;
            TCPServer server = new TCPServer(port);
            server.Start();
            Console.WriteLine("CTRL + C to stop the server...");
            Console.ReadLine();
            server.Stop();
        }
    }
}