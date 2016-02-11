using Microsoft.AspNet.SignalR;


namespace OfficeAddInServerAuth.Controllers
{
    public class SocketHub : Hub
    {
        public void SendMessage(string clientId, string message)
        {
            Clients.Client(clientId).sendMessage(message);
        }
    }
}
