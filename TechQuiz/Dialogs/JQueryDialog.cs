using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using System.Security;
using System.Configuration;

namespace TechQuiz.Dialogs
{
    [Serializable]
    public class JQueryDialog : IDialog<object>
    {
        protected List<QuizEntity> quiz = new List<QuizEntity>();
        protected int i = 0;
        protected int correctAnswers = 0;

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

            List quizList = ctx.Web.Lists.GetByTitle("SPQuiz");
            CamlQuery query = CamlQuery.CreateAllItemsQuery(100);
            query.ViewXml = "<View><Query><Where><Eq><FieldRef Name='Category'/>" +
                "<Value Type='Text'>JQuery</Value></Eq></Where></Query><RowLimit>100</RowLimit></View>";
            ListItemCollection items = quizList.GetItems(query);
            ctx.Load(items);
            ctx.ExecuteQuery();
            foreach (ListItem listItem in items)
            {
                // We have all the list item data. For example, Title. 
                quiz.Add(new QuizEntity
                {
                    Question = listItem["Title"].ToString(),
                    Option1 = listItem["Option1"].ToString(),
                    Option2 = listItem["Option2"].ToString(),
                    Option3 = listItem["Option3"]?.ToString(),
                    Option4 = listItem["Option4"]?.ToString(),
                    Answer = listItem["Answer"].ToString()
                });
            }

            var heroCard = new ThumbnailCard
            {
                Title = "JQuery",
                Subtitle = "Welcome to Jquery Quiz",
                Text = "Total number of questions are "+ quiz.Count +". Type go to start the Quiz.",
                Images = new List<CardImage> { new CardImage("https://techquizbot.azurewebsites.net/images/JqueryICon.png") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.ImBack, "Take Quiz", value: "Take Quiz") }
            };

            var message = context.MakeMessage();
            message.Attachments.Add(heroCard.ToAttachment());
            await context.PostAsync(message);

            //await context.PostAsync($"Welcome to JQuery Quiz. Total number of questions are {quiz.Count}. Type go to start the Quiz.");

            context.Wait(MessageReceivedStartQuestions);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            context.Call(new Dialogs.MainMenuDialog(), this.ResumeAfterOptionDialog);

        }

        private async Task MessageReceivedStartQuestions(IDialogContext context, IAwaitable<object> result)
        {
            
            var message = await result;
            var questionMessage = context.MakeMessage();

            if (i < quiz.Count)
            {
                var questionNumber = (i+1) + " of " + quiz.Count + ". ";
                if (quiz[i].Option3 == null)
                {
                    var heroCard = new HeroCard
                    {
                        Subtitle = questionNumber + quiz[i].Question,
                        Buttons = new List<CardAction>() {
                        new CardAction { Title = quiz[i].Option1, Type = "imBack", Value = quiz[i].Option1 },
                        new CardAction { Title = quiz[i].Option2, Type = "imBack", Value = quiz[i].Option2 },
                    }
                    };
                    questionMessage.Attachments.Add(heroCard.ToAttachment());
                }
                else {
                    var heroCard = new HeroCard
                    {
                        Subtitle = questionNumber + quiz[i].Question,
                        Buttons = new List<CardAction>() {
                        new CardAction { Title = quiz[i].Option1, Type = "imBack", Value = quiz[i].Option1 },
                        new CardAction { Title = quiz[i].Option2, Type = "imBack", Value = quiz[i].Option2 },
                        new CardAction { Title = quiz[i].Option3, Type = "imBack", Value = quiz[i].Option3 },
                        new CardAction { Title = quiz[i].Option4, Type = "imBack", Value = quiz[i].Option4 }
                    }
                    };
                    questionMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(questionMessage);
                context.Wait(MessageReceivedCorrection);
            }
            else
            {

                List<CardAction> cardButtons = new List<CardAction>();
                cardButtons.Add(new CardAction
                {
                    Title = "Main Menu",
                    Value = "Home",
                    Type = "imBack"
                });

                var heroCard = new ThumbnailCard
                {
                    Title = "JQuery Quiz is completed",
                    Subtitle = "Total number of Wrong Answers: " + Convert.ToString(quiz.Count - correctAnswers),
                    Text = "Total number of Correct Answers: " + correctAnswers.ToString() + " Out of " + quiz.Count.ToString(),
                    Images = new List<CardImage> { new CardImage("https://techquizbot.azurewebsites.net/images/JqueryICon.png") },
                    Buttons = cardButtons
                };

                var replyMessage = context.MakeMessage();

                Microsoft.Bot.Connector.Attachment plAttachment = heroCard.ToAttachment();
                replyMessage.Attachments.Add(plAttachment);
                var activity = await result as Activity;
                Utility.UpdateResults(activity, "JQuery", correctAnswers, quiz.Count);

                await context.PostAsync(replyMessage);

                quiz = new List<QuizEntity>();
                i = 0;
                correctAnswers = 0;
                context.Wait(MessageReceivedAsync);
            }

        }

        private async Task MessageReceivedCorrection(IDialogContext context, IAwaitable<IMessageActivity> result)
        {

            var message = await result;
            var questionMessage = context.MakeMessage();
            if (i < quiz.Count)
            {
                if (message.Text.Equals(quiz[i].Answer))
                {
                    await context.PostAsync($"Correct Answer");
                    i = i + 1;
                    correctAnswers = correctAnswers + 1;

                    var questionNumber = (i+1) + " of " + quiz.Count + ". ";

                    if (i < quiz.Count)
                    {
                        if (quiz[i].Option3 == null)
                        {
                            var heroCard = new HeroCard
                            {
                                Subtitle = questionNumber + quiz[i].Question,
                                Buttons = new List<CardAction>() {
                                    new CardAction { Title = quiz[i].Option1, Type = "imBack", Value = quiz[i].Option1 },
                                    new CardAction { Title = quiz[i].Option2, Type = "imBack", Value = quiz[i].Option2 },
                                }
                            };
                            questionMessage.Attachments.Add(heroCard.ToAttachment());
                        }
                        else {
                            var heroCard = new HeroCard
                            {
                                Subtitle = questionNumber + quiz[i].Question,
                                Buttons = new List<CardAction>() {
                                    new CardAction { Title = quiz[i].Option1, Type = "imBack", Value = quiz[i].Option1 },
                                    new CardAction { Title = quiz[i].Option2, Type = "imBack", Value = quiz[i].Option2 },
                                    new CardAction { Title = quiz[i].Option3, Type = "imBack", Value = quiz[i].Option3 },
                                    new CardAction { Title = quiz[i].Option4, Type = "imBack", Value = quiz[i].Option4 }
                                }
                            };
                            questionMessage.Attachments.Add(heroCard.ToAttachment());
                        }
                        await context.PostAsync(questionMessage);
                    }

                }
                else
                {
                    await context.PostAsync($"Wrong Answer");
                    i = i + 1;
                    if (i < quiz.Count)
                    {
                        var questionNumber = (i + 1) + " of " + quiz.Count + ". "; 

                        if (quiz[i].Option3 == null)
                        {
                            var heroCard = new HeroCard
                            {
                                Subtitle = questionNumber + quiz[i].Question,
                                Buttons = new List<CardAction>() {
                                    new CardAction { Title = quiz[i].Option1, Type = "imBack", Value = quiz[i].Option1 },
                                    new CardAction { Title = quiz[i].Option2, Type = "imBack", Value = quiz[i].Option2 },
                                }
                            };
                            questionMessage.Attachments.Add(heroCard.ToAttachment());
                        }
                        else
                        {
                            var heroCard = new HeroCard
                            {
                                Subtitle = questionNumber + quiz[i].Question,
                                Buttons = new List<CardAction>() {
                                    new CardAction { Title = quiz[i].Option1, Type = "imBack", Value = quiz[i].Option1 },
                                    new CardAction { Title = quiz[i].Option2, Type = "imBack", Value = quiz[i].Option2 },
                                    new CardAction { Title = quiz[i].Option3, Type = "imBack", Value = quiz[i].Option3 },
                                    new CardAction { Title = quiz[i].Option4, Type = "imBack", Value = quiz[i].Option4 }
                                }
                            };
                            questionMessage.Attachments.Add(heroCard.ToAttachment());
                        }
                        await context.PostAsync(questionMessage);
                    }
                }
                context.Wait(MessageReceivedCorrection);
            }
            else
            {
                List<CardAction> cardButtons = new List<CardAction>();
                cardButtons.Add(new CardAction
                {
                    Title = "Main Menu",
                    Value = "Home",
                    Type = "imBack"
                });

                var heroCard = new ThumbnailCard
                {
                    Title = "JQuery Quiz is completed",
                    Subtitle = "Total number of Wrong Answers: " + Convert.ToString(quiz.Count - correctAnswers),
                    Text = "Total number of Correct Answers: " + correctAnswers.ToString() + " Out of " + quiz.Count.ToString(),
                    Images = new List<CardImage> { new CardImage("https://techquizbot.azurewebsites.net/images/JqueryICon.png") },
                    Buttons = cardButtons
                };

                var replyMessage = context.MakeMessage();

                Microsoft.Bot.Connector.Attachment plAttachment = heroCard.ToAttachment();
                replyMessage.Attachments.Add(plAttachment);

                var activity = await result as Activity;
                Utility.UpdateResults(activity, "JQuery", correctAnswers, quiz.Count);

                await context.PostAsync(replyMessage);

                quiz = new List<QuizEntity>();
                i = 0;
                correctAnswers = 0;
                //context.Call(new Dialogs.RootDialog(), this.ResumeAfterOptionDialog);
                context.Wait(MessageReceivedAsync);
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
    }
}