using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Microsoft.Bot.Builder.FormFlow;

namespace TechQuiz.Dialogs
{
    [Serializable]
    public class MainMenuDialog : IDialog<object>
    {
        private const string sharePointOption = "SharePoint";

        private const string dotnetOption = "DotNet";

        private const string jqueryOption = "JQuery";

        private const string angularOption = "Angular";

        private const string azureOption = "Azure";

        private const string o365Option = "Office365";

        private const string feedbackOption = "Feedback";

        public async Task StartAsync(IDialogContext context)
        {
            var reply = context.MakeMessage();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = RootDialog.GetCardsAttachments();

            await context.PostAsync(reply);
            context.Wait(this.OnOptionSelected);
        }

        private async Task OnOptionSelected(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            try
            {
                var optionSelected = await result;

                switch (optionSelected.Text)
                {
                    case sharePointOption:
                        context.Call(new Dialogs.SPQuizDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case azureOption:
                        context.Call(new Dialogs.DotNetDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case o365Option:
                        context.Call(new Dialogs.DotNetDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case dotnetOption:
                        context.Call(new Dialogs.DotNetDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case jqueryOption:
                        context.Call(new Dialogs.JQueryDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case angularOption:
                        context.Call(new Dialogs.DotNetDialog(), this.ResumeAfterOptionDialog);
                        break;

                    case feedbackOption:
                        context.Call(new Dialogs.FeedbackDialog(), this.ResumeAfterOptionDialog);
                        break;


                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");

                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            try
            {
                var message = await result;
            }
            catch (Exception ex)
            {
                await context.PostAsync($"Failed with message: {ex.Message}");
            }
            finally
            {
                context.Wait(this.MessageReceivedAsync);
            }
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync($"Some flow went wrong, type go to proceed");
        }
    }
}