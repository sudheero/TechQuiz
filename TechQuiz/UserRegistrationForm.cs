using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Dialogs;

namespace TechQuiz
{
    [Serializable]
    public class UserRegistrationForm
    {
        [Prompt("Enter your Name? {||}")]
        public string UserName;

        [Prompt("Enter your Email Address? {||}")]
        public string EmailAddress;

        [Prompt("Enter your Mobile Number? {||}")]
        public string MobileNumber;

        public static IForm<UserRegistrationForm> BuildForm()
        {
            return new FormBuilder<UserRegistrationForm>()
                    .Message("Please enter your details to Register!")
                    .OnCompletion(async (context, registrationForm) =>
                    {
                        // Set BotUserData
                        context.PrivateConversationData.SetValue<bool>(
                        "RegistrationComplete", true);
                        context.PrivateConversationData.SetValue<string>(
                            "UserName", registrationForm.UserName);
                        context.PrivateConversationData.SetValue<string>(
                            "EmailAddress", registrationForm.EmailAddress);
                        context.PrivateConversationData.SetValue<string>(
                            "MobileNumber", registrationForm.MobileNumber);
                        // Tell the user that the form is complete
                        await context.PostAsync("Your Registration is complete. Type go to Proceed");
                    })
                    .Build();
        }
    }
}