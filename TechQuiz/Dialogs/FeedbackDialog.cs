using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using System.Security;
using System.Configuration;
using System.Text;

namespace TechQuiz.Dialogs
{
    [Serializable]
    public class FeedbackDialog : IDialog<object>
    {
        protected List<SurveyEntity> feedback = new List<SurveyEntity>();
        protected int i = 0;
        StringBuilder sb = new StringBuilder();

        public async Task StartAsync(IDialogContext context)
        {
            //context.Wait(MessageReceivedAsync);

            //return Task.CompletedTask;
            ClientContext ctx = new ClientContext("https://wbsharepoint.sharepoint.com/sites/POCs/");
            Web web = ctx.Web;

            string pwd = ConfigurationManager.AppSettings["SPPassword"].ToString();
            string userName = ConfigurationManager.AppSettings["SPUserName"].ToString();
            SecureString passWord = new SecureString();
            foreach (char c in pwd.ToCharArray()) passWord.AppendChar(c);
            ctx.Credentials = new SharePointOnlineCredentials("demouser@wbsharepoint.onmicrosoft.com", passWord);

            List quizList = ctx.Web.Lists.GetByTitle("SurveyList");
            CamlQuery query = CamlQuery.CreateAllItemsQuery(100);
            ListItemCollection items = quizList.GetItems(query);
            ctx.Load(items);
            ctx.ExecuteQuery();
            foreach (ListItem listItem in items)
            {
                // We have all the list item data. For example, Title. 
                feedback.Add(new SurveyEntity
                {
                    Question = listItem["Title"].ToString(),
                    Option1 = listItem["Option1"]?.ToString(),
                    Option2 = listItem["Option2"]?.ToString(),
                    Option3 = listItem["Option3"]?.ToString(),
                    Option4 = listItem["Option4"]?.ToString(),
                    TypeOfQuestion = listItem["TypeOfQuestion"].ToString()
                });
            }

            var heroCard = new ThumbnailCard
            {
                Title = "Feedback",
                Subtitle = "",
                Text = "Total number of feedback questions are " + feedback.Count + ". Click on Feedback button to provide feedback.",
                Images = new List<CardImage> { new CardImage("https://techquizbot.azurewebsites.net/images/feedback.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Feedback", value: "Feedback") }
            };

            var message = context.MakeMessage();
            message.Attachments.Add(heroCard.ToAttachment());
            await context.PostAsync(message);

            context.Wait(MessageReceivedStartQuestions);
        }

        private async Task MessageReceivedStartQuestions(IDialogContext context, IAwaitable<object> result)
        {
            var message = await result;

            var questionMessage = context.MakeMessage();

            if(i < feedback.Count)
            {
                var questionNumber = (i + 1) + " of " + feedback.Count + ". ";

                if (feedback[i].TypeOfQuestion == "ChooseOne")
                {
                    var heroCard1 = new HeroCard
                    {
                        Subtitle = questionNumber + feedback[i].Question,
                        Buttons = new List<CardAction>() {
                            new CardAction { Title = feedback[i].Option1, Type = "imBack", Value = feedback[i].Option1 },
                            new CardAction { Title = feedback[i].Option2, Type = "imBack", Value = feedback[i].Option2 },
                            new CardAction { Title = feedback[i].Option3, Type = "imBack", Value = feedback[i].Option3 },
                            new CardAction { Title = feedback[i].Option4, Type = "imBack", Value = feedback[i].Option4 }
                        }
                    };
                    questionMessage.Attachments.Add(heroCard1.ToAttachment());
                    await context.PostAsync(questionMessage);
                }
                else if (feedback[i].TypeOfQuestion == "FreeText")
                {
                    await context.PostAsync(questionNumber + feedback[i].Question);
                }

                i = i + 1;

                context.Wait(MessageReceivedNextQuestions);
            }
        }


        private async Task MessageReceivedNextQuestions(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            sb.Append(i + ". " + message.Text + "## ");


            var questionMessage = context.MakeMessage();

            if (i < feedback.Count)
            {
                var questionNumber = (i + 1) + " of " + feedback.Count + ". ";

                if (feedback[i].TypeOfQuestion == "ChooseOne")
                {
                    var heroCard1 = new HeroCard
                    {
                        Subtitle = questionNumber + feedback[i].Question,
                        Buttons = new List<CardAction>() {
                            new CardAction { Title = feedback[i].Option1, Type = "imBack", Value = feedback[i].Option1 },
                            new CardAction { Title = feedback[i].Option2, Type = "imBack", Value = feedback[i].Option2 },
                            new CardAction { Title = feedback[i].Option3, Type = "imBack", Value = feedback[i].Option3 },
                            new CardAction { Title = feedback[i].Option4, Type = "imBack", Value = feedback[i].Option4 }
                        }
                    };
                    questionMessage.Attachments.Add(heroCard1.ToAttachment());
                    await context.PostAsync(questionMessage);
                }
                else if (feedback[i].TypeOfQuestion == "FreeText")
                {
                    await context.PostAsync(questionNumber + feedback[i].Question);
                }
                i = i + 1;

                context.Wait(MessageReceivedNextQuestions);
            }
            else
            {
                //await context.PostAsync(sb.ToString());
                var activity = await result as Activity;
                Utility.UpdateResults(activity, "FeedbackQBot", sb);
                List<CardAction> cardButtons = new List<CardAction>();
                cardButtons.Add(new CardAction
                {
                    Title = "Main Menu",
                    Value = "Home",
                    Type = "imBack"
                });

                var heroCard = new ThumbnailCard
                {
                    Title = "Feedback",
                    Subtitle = "",
                    Text = "Thank you for your Valuable feedback.",
                    Images = new List<CardImage> { new CardImage("https://techquizbot.azurewebsites.net/images/feedback.png") },
                    Buttons = cardButtons
                };

                var replyMessage = context.MakeMessage();

                Microsoft.Bot.Connector.Attachment plAttachment = heroCard.ToAttachment();
                replyMessage.Attachments.Add(plAttachment);

                await context.PostAsync(replyMessage);

                

                sb = new StringBuilder();
                feedback = new List<SurveyEntity>();
                i = 0;
                //context.Call(new Dialogs.RootDialog(), this.ResumeAfterOptionDialog);
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            context.Call(new Dialogs.MainMenuDialog(), this.ResumeAfterOptionDialog);
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

    }
}