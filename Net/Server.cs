using Chat_App.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chat_App.Net
{
    internal class Server
    {
        TcpClient _client;
        public PacketReader packetReader;

        public event Action connectedEvent;
        public event Action msgRecievedEvent;
        public event Action userDisconnectEvent;

        public PacketBuilder packetBuilder;
        public byte[] publicKey;

        public Server()
        {
            _client = new TcpClient();
            packetBuilder = new PacketBuilder();
        }

        public void ConnectToServer(string userName)
        {
            if (!_client.Connected)
            {
                _client.Connect("127.0.0.1", 5353);
                packetReader = new PacketReader(_client.GetStream(), packetBuilder._publicKey);

                if (!string.IsNullOrEmpty(userName))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(userName);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                ReadPackets();
            }
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var opcode = packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 5:
                            msgRecievedEvent?.Invoke();
                            break;
                        case 10:
                            userDisconnectEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine("uh oh...");
                            break;
                    }
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteMessage(message);

            _client.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}
