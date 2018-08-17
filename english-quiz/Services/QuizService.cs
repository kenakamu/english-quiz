using english_quiz.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace english_quiz.Services
{
    public static class QuizService
    {
        private static int quiz_num = 5;
        public static Dictionary<string, UserQuizCache> UserQuizCache;
        
        private static List<Quiz> quizes;
        public static List<Quiz> Quizes
        {
            get
            {
                if (quizes == null)
                    quizes = LoadCSV();
                return quizes;
            }
        }

        static QuizService()
        {
            UserQuizCache = new Dictionary<string, UserQuizCache>();
        }

        public static UserQuizCache GetUserQuizCache(string userId)
        {
            var random = new Random();
            if (!UserQuizCache.ContainsKey(userId) || UserQuizCache[userId].UserQuizes.Count == 0)
                InitializeUserQuizCache(userId);

            return UserQuizCache[userId];
        }

        public static UserQuizCache InitializeUserQuizCache(string userId)
        {
            var random = new Random();
            if (UserQuizCache.ContainsKey(userId))
                UserQuizCache.Remove(userId);

            var userQuizes = Quizes.OrderBy(x => random.Next()).Take(quiz_num).Select(x => x.Number).ToList();
            var currentQuizNumber = userQuizes.Skip(random.Next(0, userQuizes.Count - 1)).Take(1).First();
            var userQuizCache = new UserQuizCache()
            {
                UserQuizes = userQuizes,
                CurrentQuizNumber = currentQuizNumber
            };
            UserQuizCache.Add(userId, userQuizCache);

            return UserQuizCache[userId];
        }

        public static Quiz GetQuizByNumber(int number)
        {
            return Quizes.Where(x => x.Number == number).First();
        }

        private static List<Quiz> LoadCSV()
        {
            var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Quiz", "quiz.csv");
            var quizContents = new List<Quiz>();
            var quizes = File.ReadLines(csvPath);
          
            foreach(var quiz in quizes)
            {
                var number = int.Parse(quiz.Split('|')[0]);
                var questionType = quiz.Split('|')[1];
                var question = quiz.Split('|')[2];
                var hint1 = quiz.Split('|')[3];
                var hint2 = quiz.Split('|')[4];
                var answer = quiz.Split('|')[5];
                quizContents.Add(new Quiz() { Number = number, QuestionType = questionType, Question = question, Hint1 = hint1, Hint2 = hint2, Answer = answer });
            }

            return quizContents;
        }
    }    
}
