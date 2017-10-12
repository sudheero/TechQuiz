using Microsoft.Bot.Connector;
using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Web;
using System.Text;

namespace TechQuiz
{
    public class Utility
    {
        public static void UpdateResults(Activity activity, string category, int correctAnswers, int totalQuestions)
        {
            try
            {
                StateClient sc = activity.GetStateClient();
                BotData userData = sc.BotState.GetPrivateConversationData(
                    activity.ChannelId, activity.Conversation.Id, activity.From.Id);

                string name = userData.GetProperty<string>("UserName");
                string emailAddress = userData.GetProperty<string>("EmailAddress");
                string phoneNumber = userData.GetProperty<string>("MobileNumber");
                string channel = activity.ChannelId;

                ClientContext ctx = new ClientContext("https://wbsharepoint.sharepoint.com/sites/POCs/");
                Web web = ctx.Web;

                string pwd = ConfigurationManager.AppSettings["SPPassword"].ToString();
                string userName = ConfigurationManager.AppSettings["SPUserName"].ToString();
                SecureString passWord = new SecureString();
                foreach (char c in pwd.ToCharArray()) passWord.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials("demouser@wbsharepoint.onmicrosoft.com", passWord);

                List quizResultsList = ctx.Web.Lists.GetByTitle("QBotQuizResults");

                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                ListItem newItem = quizResultsList.AddItem(itemCreateInfo);
                newItem["Title"] = name;
                newItem["EmailAddress"] = emailAddress;
                newItem["PhoneNumber"] = phoneNumber;
                newItem["QuizCategory"] = category;
                newItem["CorrectAnswers"] = correctAnswers;
                newItem["TotalQuestions"] = totalQuestions;

                newItem["QuizDateTime"] = DateTime.Now.ToString();
                newItem["Channel"] = channel;
                newItem.Update();

                ctx.ExecuteQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
        }

        internal static void UpdateResults(Activity activity, string surveyName, StringBuilder feedbackData)
        {
            try
            {
                StateClient sc = activity.GetStateClient();
                BotData userData = sc.BotState.GetPrivateConversationData(
                activity.ChannelId, activity.Conversation.Id, activity.From.Id);

                string name = userData.GetProperty<string>("UserName");
                string emailAddress = userData.GetProperty<string>("EmailAddress");
                string phoneNumber = userData.GetProperty<string>("MobileNumber");
                string channel = activity.ChannelId;

                ClientContext ctx = new ClientContext("https://wbsharepoint.sharepoint.com/sites/POCs/");
                Web web = ctx.Web;

                string pwd = ConfigurationManager.AppSettings["SPPassword"].ToString();
                string userName = ConfigurationManager.AppSettings["SPUserName"].ToString();
                SecureString passWord = new SecureString();
                foreach (char c in pwd.ToCharArray()) passWord.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials("demouser@wbsharepoint.onmicrosoft.com", passWord);

                List quizResultsList = ctx.Web.Lists.GetByTitle("SurveyResults");

                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                ListItem newItem = quizResultsList.AddItem(itemCreateInfo);
                newItem["Title"] = surveyName;
                newItem["UserName"] = name;
                newItem["EmailAddress"] = emailAddress;
                newItem["PhoneNumber"] = phoneNumber;
                newItem["SurveyData"] = feedbackData;
                newItem["Channel"] = channel;
                newItem.Update();

                ctx.ExecuteQuery();
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
        }
    }
}