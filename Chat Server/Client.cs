using Chat_Server.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat_Server
{
    internal class Client
    {
        public string userName {  get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }
        PacketReader _packetReader { get; set; }
        public PacketBuilder PacketBuilder { get; set; }
        public Client(TcpClient client) { 
            PacketBuilder = new PacketBuilder();
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream(), PacketBuilder.PublicKey);

            var opcode = _packetReader.ReadByte();
            /*Validate op code*/
            userName = _packetReader.readMessage();

            Console.WriteLine($"[{DateTime.Now}: Client has connected. User Name: {userName}]");

            Task.Run(() => Process());

        }

        void Process()
        {
            while (true)
            {
                try {
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 5:
                            var msg = _packetReader.readMessage();
                            Console.WriteLine($"[Date: {DateTime.Now}]: Message recieved: {msg}");
                            Program.BroadcastMessage($"[{DateTime.Now}]: [{userName}]: {msg}");
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{userName}]: Disconnected!");
                    Program.BroadcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    break;
                }

            }
        }


    }
}
