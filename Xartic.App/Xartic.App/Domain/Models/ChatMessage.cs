using Xamarin.Forms;

namespace Xartic.App.Domain.Models
{
    public class ChatMessage
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public Color MessageColor { get; set; }

        public ChatMessage(string username, string message, Color messageColor)
        {
            Username = username;
            Message = message;
            MessageColor = messageColor;
        }
    }
}
