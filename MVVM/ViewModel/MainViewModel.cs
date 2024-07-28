using Chat_App.MVVM.Core;
using Chat_App.MVVM.Model;
using Chat_App.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Chat_App.MVVM.ViewModel
{
    internal class MainViewModel
    {
        public ObservableCollection<UserModel> _users{ get; set; }
        public ObservableCollection<string> _messages { get; set; }

        public RelayCommand SendMessageCommand { get; set; }

        public string userName {  get; set; }
        public string Message { get; set; }
        public RelayCommand ConnectToServerCommand {  get; set; }
        private Server _server;
        public MainViewModel() {
            _users = new ObservableCollection<UserModel>();
            _messages = new ObservableCollection<string>();
            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.msgRecievedEvent += MessageRecieved;
            _server.userDisconnectEvent += UserDisconnect;
            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(userName), o=>!string.IsNullOrEmpty(userName));

            SendMessageCommand = new RelayCommand(o => _server.SendMessageToServer(Message), o=>!string.IsNullOrEmpty(Message));
        }

        private void UserDisconnect()
        {
            var uid = _server.packetReader.readMessage();
            var user = _users.Where(x => x.UID == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => _users.Remove(user));

        }

        private void MessageRecieved()
        {
            var msg = _server.packetReader.readMessage();
            Application.Current.Dispatcher.Invoke(() => _messages.Add(msg));
        }

        private void UserConnected()
        {
            var user = new UserModel
            {
                Username = _server.packetReader.readMessage(),
                UID = _server.packetReader.readMessage()
            };

            if(!_users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => _users.Add(user));
            }

        }
    }
}
