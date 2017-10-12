using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Microsoft.Bot.Builder.FormFlow;
using System.Globalization;

namespace TechQuiz.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
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

            //var reply = context.MakeMessage();

            //reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            //reply.Attachments = GetCardsAttachments();

            //await context.PostAsync(reply);
            //context.Wait(this.OnOptionSelected);
            //DateTime utcdate = DateTime.ParseExact(new DateTime, "M/dd/yyyy h: mm:ss tt", CultureInfo.InvariantCulture);
            
            context.Wait(MessageReceivedAsync);

            //return Task.CompletedTask;

            
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;


            StateClient sc = activity.GetStateClient();
            BotData userData = sc.BotState.GetPrivateConversationData(
                activity.ChannelId, activity.Conversation.Id, activity.From.Id);
            var boolRegistrationComplete = userData.GetProperty<bool>("RegistrationComplete");

            if (!boolRegistrationComplete)
            {
                var myform = new FormDialog<UserRegistrationForm>(new UserRegistrationForm(), UserRegistrationForm.BuildForm, FormOptions.PromptInStart, null);

                context.Call<UserRegistrationForm>(myform, MessageReceivedData);

            }
            else
            {
                string strUserName = "Welcome " + userData.GetProperty<string>("UserName");

                var heroCard = new ThumbnailCard
                {
                    Title = strUserName,
                    Subtitle = "I am Quiz BOT",
                    Text = "I can evaluate your technical skills in SharePoint, DotNet, Jquery, Angular, Azure and Office365.",
                    Images = new List<CardImage> { new CardImage("https://techquizbot.azurewebsites.net/images/techquiz.png") },
                    Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Get Started", value: "Get Started") }
                };

                var message = context.MakeMessage();
                message.Attachments.Add(heroCard.ToAttachment());
                await context.PostAsync(message);

                context.Wait(MessageReceivedStart);
            }
        }

        private async Task MessageReceivedData(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
        }
        private async Task MessageReceivedStart(IDialogContext context, IAwaitable<object> result)
        {
            var reply = context.MakeMessage();

            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;
            reply.Attachments = GetCardsAttachments();

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

        public static IList<Attachment> GetCardsAttachments()
        {
            return new List<Attachment>()
            {
                GetHeroCard(
                    "SharePoint",
                    "",
                    "SharePoint is Microsoft's document management and collaboration tool with a software-as-a-service strategy at its core.",
                    new CardImage(url: "http://techquizbot.azurewebsites.net/images/SPIcon.png"),
                    new CardAction(ActionTypes.ImBack, "Go", value: "SharePoint")),
                GetHeroCard(
                    "JQuery",
                    "",
                    "jQuery is a fast, small, and feature-rich JavaScript library.",
                    new CardImage(url: "http://techquizbot.azurewebsites.net/images/JqueryICon.png"),
                    new CardAction(ActionTypes.ImBack, "Go", value: "JQuery")),
                GetHeroCard(
                    "DotNet",
                    "",
                    "A programming infrastructure created by Microsoft for building, deploying, and running applications and services.",
                    new CardImage(url: "http://techquizbot.azurewebsites.net/images/DotNet.png"),
                    new CardAction(ActionTypes.ImBack, "DotNet", value: "DotNet")),
                GetHeroCard(
                    "AngularJS",
                    "",
                    "AngularJS is a structural framework for dynamic web apps.",
                    new CardImage(url: "http://techquizbot.azurewebsites.net/images/AngularIcon.jpg"),
                    new CardAction(ActionTypes.ImBack, "Angular", value: "Angular")),
                GetHeroCard(
                    "Office365",
                    "",
                    "Office 365 is the brand name Microsoft uses for a group of software and services subscriptions",
                    new CardImage(url: "http://techquizbot.azurewebsites.net/images/O365Icon.png"),
                    new CardAction(ActionTypes.ImBack, "Office365", value: "Office365")),
                GetHeroCard(
                    "Azure",
                    "",
                    "Azure is a cloud computing service for building, testing, deploying, and managing applications and services.",
                    new CardImage(url: "http://techquizbot.azurewebsites.net/images/MicrosoftAzureIcon.png"),
                    new CardAction(ActionTypes.ImBack, "Azure", value: "Azure")),
                GetHeroCard(
                    "Feedback",
                    "",
                    "Please Provide feedback on Quiz which you have taken",
                    new CardImage(url: "http://techquizbot.azurewebsites.net/images/feedback.png"),
                    new CardAction(ActionTypes.ImBack, "Feedback", value: "Feedback")),
            };
        }

        public static Attachment GetHeroCard(string title, string subtitle, string text, CardImage cardImage, CardAction cardAction)
        {
            var heroCard = new ThumbnailCard
            {
                Title = title,
                //Subtitle = subtitle,
                Text = text,
                Images = new List<CardImage>() { cardImage  },
                Buttons = new List<CardAction>() { cardAction },
            };

            return heroCard.ToAttachment();
        }
    }
}