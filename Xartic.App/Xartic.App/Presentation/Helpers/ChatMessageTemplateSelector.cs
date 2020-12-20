using Xamarin.Forms;
using Xartic.App.Domain.Models;

namespace Xartic.App.Presentation.Helpers
{
    public sealed class ChatMessageTemplateSelector : DataTemplateSelector
    {
        #region Properties

        public DataTemplate ServerMessageTemplate { get; set; }

        public DataTemplate UserMessageTemplate { get; set; }

        #endregion

        #region Overrides

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            if (item is ChatMessage chatMessage)
            {
                if (!string.IsNullOrEmpty(chatMessage.Username))
                    return UserMessageTemplate;
            }

            return ServerMessageTemplate;
        }

        #endregion
    }
}
