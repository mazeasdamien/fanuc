using FRRobot;
using OpenAI_API.Chat;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Numerics;

namespace FanucRobotServer
{
    public class TCPServer
    {
        private readonly TcpListener _server;
        private readonly IPAddress _localAddr;
        private readonly int _port;
        private FRCRobot _real_robot;
        private readonly ConcurrentDictionary<TcpClient, byte> _clients = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly FRCIOTypes IOTypes;
        private bool isReachable = true;
        public string unity_cmd;

        string openai_api_key = "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";
        string jsonFileName = "RobotData.json";
        string jsonDirectory = Path.Combine(Path.GetTempPath(), "RobotData"); // Create a subdirectory in the temp directory
        string jsonPath;

        public Vector3 cameraPosition;
        public Vector3 boundingBox_center;
        public Vector3 boundingBox_point1;
        public Vector3 boundingBox_point2;
        public float distanceCameraSurface;

        // Add this field to keep track of the chat history
        private List<ChatMessage> chatHistory = new List<ChatMessage>();

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
                _real_robot = new FRCRobot();
                _real_robot.ConnectEx("192.168.1.20", false, 10, 1);
                Console.WriteLine("Connected to real robot successfully.");
                FRCAlarms fRCAlarmsREAL = _real_robot.Alarms;
                FRCTasks mobjTasksREAL = _real_robot.Tasks;
                FRCPrograms fRCProgramsREAL = _real_robot.Programs;

                Thread.Sleep(500);

                FRCSysPositions fRCTPPositions = _real_robot.RegPositions;
                FRCSysPosition sysPosition = fRCTPPositions[3];
                FRCSysGroupPosition sysGroupPosition = sysPosition.Group[1];
                FRCXyzWpr xyzWpr = sysGroupPosition.Formats[FRETypeCodeConstants.frXyzWpr];

                xyzWpr.X = 1133;
                xyzWpr.Y = 400;
                xyzWpr.Z = 1060;
                xyzWpr.W = -10;
                xyzWpr.P = 0;
                xyzWpr.R = 90;

                Thread.Sleep(500);
                sysGroupPosition.Update();
                Thread.Sleep(500);

                Thread.Sleep(500);
                mobjTasksREAL.AbortAll();
                Thread.Sleep(500);
                fRCAlarmsREAL.Reset();
                fRCProgramsREAL.Selected = "DAMIEN";
                FRCTPProgram fRCProgramREAL = (FRCTPProgram)fRCProgramsREAL[fRCProgramsREAL.Selected, Type.Missing, Type.Missing];
                fRCProgramREAL.Run();
                Console.WriteLine("Program Damien real robot started.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to connect to robot: " + e.Message);
            }
        }

        public void Start()
        {
            Directory.CreateDirectory(jsonDirectory);
            jsonPath = Path.Combine(jsonDirectory, jsonFileName);

            string Assistant1 =
                "This chat is a path planning assitant for an industrial robot equiped with inspection RGBD camera, bellow are the specifation for the path generation\n" +
                "Create a JSON file list named positions and each position is X Y Z in meters with the following limitations:\n" +
                $"X limits: {boundingBox_point1.X} to {boundingBox_point2.X}\n" +
                $"Y limits: {boundingBox_point1.Y} to {boundingBox_point2.Y}\n" +
                $"Z limits: {boundingBox_point1.Z} to {boundingBox_point2.Z}\n" +
                $"The center coordinate of the operating area is X: {boundingBox_center.X} Y: {boundingBox_center.Y} Z: {boundingBox_center.Z}\n" +
                $"The camera location is currently: X: {cameraPosition.X} Y: {cameraPosition.Y} Z: {cameraPosition.Z}\n" +
                $"The distance depth recorded from the RGBD camera to the surface is : {distanceCameraSurface} cm\n";
            chatHistory.Add(new ChatMessage { Role = ChatMessageRole.Assistant, Content = Assistant1 });

            string user1 = "create a perfect circle in the limitations";
            chatHistory.Add(new ChatMessage { Role = ChatMessageRole.User, Content = user1 });

            string Assistant2 = "{\r\n    \"positions\": [\r\n        {\r\n            \"X\": -1.0555,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.416\r\n        },\r\n        {\r\n            \"X\": -1.0158,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.4773\r\n        },\r\n        {\r\n            \"X\": -0.9964,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.549\r\n        },\r\n        {\r\n            \"X\": -1.0057,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.6232\r\n        },\r\n        {\r\n            \"X\": -1.0444,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.6872\r\n        },\r\n        {\r\n            \"X\": -1.1055,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.7326\r\n        },\r\n        {\r\n            \"X\": -1.1755,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.7523\r\n        },\r\n        {\r\n            \"X\": -1.2462,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.7429\r\n        },\r\n        {\r\n            \"X\": -1.3065,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.7057\r\n        },\r\n        {\r\n            \"X\": -1.3462,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.6458\r\n        },\r\n        {\r\n            \"X\": -1.3598,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.5716\r\n        },\r\n        {\r\n            \"X\": -1.3458,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.4964\r\n        },\r\n        {\r\n            \"X\": -1.3071,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.4324\r\n        },\r\n        {\r\n            \"X\": -1.246,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.387\r\n        },\r\n        {\r\n            \"X\": -1.1755,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.3673\r\n        },\r\n        {\r\n            \"X\": -1.1055,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.3767\r\n        },\r\n        {\r\n            \"X\": -1.0555,\r\n            \"Y\": 1.3205,\r\n            \"Z\": -0.416\r\n        }\r\n    ]\r\n}\r\n";
            chatHistory.Add(new ChatMessage { Role = ChatMessageRole.Assistant, Content = Assistant2 });

            string user2 = "create a perfect sqaure in the limitations";
            chatHistory.Add(new ChatMessage { Role = ChatMessageRole.User, Content = user2 });

            string Assistant3 = "{\r\n    \"positions\": [\r\n        {\r\n            \"X\": -0.8,\r\n            \"Y\": 1.185,\r\n            \"Z\": -0.5\r\n        },\r\n        {\r\n            \"X\": -1.2,\r\n            \"Y\": 1.25,\r\n            \"Z\": 0.2\r\n        },\r\n        {\r\n            \"X\": -1.35,\r\n            \"Y\": 1.35,\r\n            \"Z\": 0.4\r\n        },\r\n        {\r\n            \"X\": -1.4,\r\n            \"Y\": 1.45,\r\n            \"Z\": 0.6\r\n        },\r\n        {\r\n            \"X\": -1.45,\r\n            \"Y\": 1.543001,\r\n            \"Z\": 0.8\r\n        }\r\n    ]\r\n}\r\n";
            chatHistory.Add(new ChatMessage { Role = ChatMessageRole.Assistant, Content = Assistant3 });

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
            _clients.TryRemove(client, out _);
        }

        private async Task SendDataToClientContinuously(CancellationToken cancellationToken)
        {
            string? previousMessage = null;

            while (!cancellationToken.IsCancellationRequested)
            {
                var curPosition = _real_robot.CurPosition;
                var groupPositionJoint = curPosition.Group[1, FRECurPositionConstants.frJointDisplayType];
                var groupPositionWorld = curPosition.Group[1, FRECurPositionConstants.frWorldDisplayType];
                groupPositionJoint.Refresh();
                groupPositionWorld.Refresh();
                var joint = (FRCJoint)groupPositionJoint.Formats[FRETypeCodeConstants.frJoint];
                var xyzWpr = (FRCXyzWpr)groupPositionWorld.Formats[FRETypeCodeConstants.frXyzWpr];

                string message = $"{joint[1]:F1},{joint[2]:F1},{joint[3]:F1},{joint[4]:F1},{joint[5]:F1},{joint[6]:F1},{xyzWpr.X:F1},{xyzWpr.Y:F1},{xyzWpr.Z:F1},{xyzWpr.W:F1},{xyzWpr.P:F1},{xyzWpr.R:F1}";

                string messageReachability = $"{isReachable}";
                SendDataToClient(messageReachability + "\n");


                if (previousMessage == null || previousMessage != message)
                {
                    //Console.WriteLine(message);
                    SendDataToClient(message + "\n");
                    previousMessage = message;
                }

                // Adjust the delay as needed to control the frequency of updates
                await Task.Delay(0, cancellationToken);
            }
        }

        private void SendDataToClient(string message)
        {
            // Check if there are any clients in the dictionary before sending the message
            if (!_clients.IsEmpty)
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
                        //Console.WriteLine("Data received from client: " + receivedData);

                        if (values.Length == 6)
                        {
                            //real
                            FRCSysPositions fRCTPPositions = _real_robot.RegPositions;
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
                                isReachable = true;
                                sysGroupPosition.Update();
                            }
                            else
                            {
                                isReachable = false;
                            }
                        }
                        else if (values.Length == 13)
                        {
                            cameraPosition.X = float.Parse(values[0]);
                            cameraPosition.Y = float.Parse(values[1]);
                            cameraPosition.Z = float.Parse(values[2]);
                            boundingBox_point1.X = float.Parse(values[3]);
                            boundingBox_point1.Y = float.Parse(values[4]);
                            boundingBox_point1.Z = float.Parse(values[5]);
                            boundingBox_point2.X = float.Parse(values[6]);
                            boundingBox_point2.Y = float.Parse(values[7]);
                            boundingBox_point2.Z = float.Parse(values[8]);
                            boundingBox_center.X = float.Parse(values[9]);
                            boundingBox_center.Y = float.Parse(values[10]);
                            boundingBox_center.Z = float.Parse(values[11]);
                            distanceCameraSurface = float.Parse(values[12]);
                        }
                        else if (receivedData.Trim() == "run")
                        {
                            Console.WriteLine("Run command received");
                            FRCAlarms fRCAlarmsREAL = _real_robot.Alarms;
                            FRCTasks mobjTasksREAL = _real_robot.Tasks;
                            FRCPrograms fRCProgramsREAL = _real_robot.Programs;

                            Thread.Sleep(500);

                            FRCSysPositions fRCTPPositions = _real_robot.RegPositions;
                            FRCSysPosition sysPosition = fRCTPPositions[3];
                            FRCSysGroupPosition sysGroupPosition = sysPosition.Group[1];
                            FRCXyzWpr xyzWpr = sysGroupPosition.Formats[FRETypeCodeConstants.frXyzWpr];

                            xyzWpr.X = 1133;
                            xyzWpr.Y = 400;
                            xyzWpr.Z = 1060;
                            xyzWpr.W = -10;
                            xyzWpr.P = 0;
                            xyzWpr.R = 90;

                            Thread.Sleep(500);
                            sysGroupPosition.Update();
                            Thread.Sleep(500);

                            Thread.Sleep(500);
                            mobjTasksREAL.AbortAll();
                            Thread.Sleep(500);
                            fRCAlarmsREAL.Reset();
                            fRCProgramsREAL.Selected = "DAMIEN";
                            FRCTPProgram fRCProgramREAL = (FRCTPProgram)fRCProgramsREAL[fRCProgramsREAL.Selected, Type.Missing, Type.Missing];
                            fRCProgramREAL.Run();
                            Console.WriteLine("Program Damien real robot started.");
                        }
                        else if (receivedData.Trim() == "reset")
                        {
                            Console.WriteLine("Reset command received");
                            FRCAlarms fRCAlarmsREAL = _real_robot.Alarms;
                            fRCAlarmsREAL.Reset();
                        }
                        else if (receivedData.Trim() == "stop")
                        {
                            Console.WriteLine("Stop command received");
                            FRCTasks mobjTasksREAL = _real_robot.Tasks;
                            mobjTasksREAL.AbortAll();
                        }
                        else if (receivedData.Trim() == "home")
                        {
                            Console.WriteLine("Home command received");
                            Thread.Sleep(500);

                            FRCSysPositions fRCTPPositions = _real_robot.RegPositions;
                            FRCSysPosition sysPosition = fRCTPPositions[3];
                            FRCSysGroupPosition sysGroupPosition = sysPosition.Group[1];
                            FRCXyzWpr xyzWpr = sysGroupPosition.Formats[FRETypeCodeConstants.frXyzWpr];

                            xyzWpr.X = 1133;
                            xyzWpr.Y = 400;
                            xyzWpr.Z = 1060;
                            xyzWpr.W = -10;
                            xyzWpr.P = 0;
                            xyzWpr.R = 90;

                            Thread.Sleep(500);
                            sysGroupPosition.Update();
                            Thread.Sleep(500);

                        }
                        else if (values.Length == 1) // PRONPT MUST BE WITHOUT COMMA 
                        {
                            unity_cmd = values[0];
                            Console.WriteLine("Unity prompt message received: " + unity_cmd);

                            // This will start the execution of the long-running operation asynchronously, on another thread
                            Task.Run(async () =>
                            {

                                string message = unity_cmd;
                                chatHistory.Add(new ChatMessage { Role = ChatMessageRole.User, Content = message });

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine("The prompt is sent to GPT, waiting response... ");
                                Console.ResetColor();

                                // Pass the list of ChatMessage to GetResponseFromGPT
                                string response = await GPT.GetResponseFromGPT(openai_api_key, chatHistory, "gpt-3.5-turbo");

                                // Save the JSON file to the specified path
                                GPT.SaveResult(jsonPath, response);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("Successfully updated the trajectory JSON file at: " + jsonPath);
                                Console.ResetColor();
                            });
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
}
