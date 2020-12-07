using Xamarin.Forms;
using Xartic.App.Domain.Models;

namespace Xartic.App.Presentation.Helpers
{
    public class ChatMessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ServerMessageTemplate { get; set; }

        public DataTemplate UserMessageTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if(item is ChatMessage chatMessage)
            {
                if (!string.IsNullOrEmpty(chatMessage.Username))
                    return UserMessageTemplate;
            }

            return ServerMessageTemplate;
        }
    }
}
