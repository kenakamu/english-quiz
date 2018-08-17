using System.Collections.Generic;

namespace english_quiz.Models
{
    public class Quiz
    {
        public int Number { get; set; }
        public string QuestionType { get; set; }
        public string Question { get; set; }
        public string Hint1 { get; set; }
        public string Hint2 { get; set; }
        public string Answer { get; set; }
    }
}
