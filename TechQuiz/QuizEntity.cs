using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TechQuiz
{
    [Serializable]
    public class QuizEntity
    {
        public string Question { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public string Answer { get; set; }
    }

    [Serializable]
    public class SurveyEntity
    {
        public string Question { get; set; }
        public string TypeOfQuestion { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
    }
}