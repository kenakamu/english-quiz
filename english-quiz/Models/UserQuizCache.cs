using System.Collections.Generic;

namespace english_quiz.Models
{
    public class UserQuizCache
    {
        public List<int> UserQuizes { get; set; }
        public int CurrentQuizNumber { get; set; }
    }
}
