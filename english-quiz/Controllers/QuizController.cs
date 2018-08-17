using ClovaCEKCsharp;
using ClovaCEKCsharp.Models;
using english_quiz.Models;
using english_quiz.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace english_quiz.Controllers
{
    [Route("api/[controller]")]
    public class QuizController : Controller
    {
        private ClovaClient clova;
        private CEKResponse response = new CEKResponse();
        private Quiz currentQuiz;
        private int answer_count;
        private int correct_count;
        private int quizNum;
        private UserQuizCache userQuizCache;

        public QuizController()
        {
            clova = new ClovaClient();          
        }

        // POST api/quiz
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var request = await clova.GetRequest(Request.Headers["SignatureCEK"], Request.Body);
            var userId = request.Session.User.UserId;
            answer_count = request.Session.SessionAttributes.ContainsKey("answer_count") ?
                int.Parse(request.Session.SessionAttributes["answer_count"].ToString()) : 1;
            correct_count = request.Session.SessionAttributes.ContainsKey("correct_count") ?
               int.Parse(request.Session.SessionAttributes["correct_count"].ToString()) : 0;
            quizNum = request.Session.SessionAttributes.ContainsKey("quiz_num") ?
                int.Parse(request.Session.SessionAttributes["quiz_num"].ToString()) : 1;

            userQuizCache = QuizService.GetUserQuizCache(userId);
            currentQuiz = QuizService.GetQuizByNumber(userQuizCache.CurrentQuizNumber);

            switch (request.Request.Type)
            {
                case RequestType.LaunchRequest:
                    userQuizCache = QuizService.InitializeUserQuizCache(userId);
                    currentQuiz = QuizService.GetQuizByNumber(userQuizCache.CurrentQuizNumber);
                    response.AddText($"イングリッシュクイズへようこそ！英語でクイズを出すので、日本語で回答してください！全部で{userQuizCache.UserQuizes.Count}問です。１問につき３度まで回答できます。");
                    response.AddText($"では第{quizNum}問");
                    response.AddUrl("https://eigodequiz.azurewebsites.net/assets/quizintro.mp3");
                    response.AddText(currentQuiz.QuestionType);
                    response.AddText(currentQuiz.Question, Lang.En);
                    SaveSession(quizNum, answer_count, correct_count);
                    SetReprompt();
                    break;
                case RequestType.SessionEndedRequest:
                    response.AddText("また挑戦してください。"); // This doesn't seems to work..
                    break;
                case RequestType.IntentRequest:                   
                    switch (request.Request.Intent.Name)
                    {
                        case "Answer":
                        case "Clova.GuideIntent":
                        case "Clova.YesIntent":
                        case "Clova.NoIntent":
                            var answer = request.Request.Intent.Slots.ContainsKey("Answer") ? request.Request.Intent.Slots["Answer"] : null;
                            if (answer == null || answer.Value != currentQuiz.Answer)
                            {
                                if (answer_count == 3)
                                {
                                    response.AddText($"残念！正解は{currentQuiz.Answer}でした。");
                                    SetNextQuestion();                                   
                                }
                                else
                                {                                   
                                    if(answer_count == 1)
                                    {
                                        if (answer != null && answer.Value == "ヒント")
                                            response.AddText($"１つ目のヒントです。");
                                        else
                                            response.AddText($"{answer_count}回目の不正解です。ヒントです。");
                                        response.AddText(currentQuiz.Hint1, Lang.En);
                                    }
                                    else
                                    {
                                        if (answer != null && answer.Value == "ヒント")
                                            response.AddText($"最後のヒントです。");
                                        else
                                            response.AddText($"{answer_count}回目の不正解です。最後のヒントです。");
                                        response.AddText(currentQuiz.Hint2, Lang.En);
                                    }
                                    answer_count++;
                                    SaveSession(quizNum, answer_count, correct_count);
                                    SetReprompt();
                                }
                            }
                            else
                            {
                                response.AddText($"正解です！");
                                correct_count++;
                                SetNextQuestion();                              
                            }
                            break;
                        }
                    break;
            }
            return new OkObjectResult(response);
        }

        private void SaveSession(int quizNum, int answer_count, int correct_count)
        {
            response.AddSessoin("quiz_num", quizNum);
            response.AddSessoin("answer_count", answer_count);
            response.AddSessoin("correct_count", correct_count);
            response.Response.ShouldEndSession = false;
        }

        private void SetReprompt()
        {
            response.AddRepromptText("あと５秒です。分からないときはヒントと言ってみましょう。");
        }

        private void SetNextQuestion()
        {
            var random = new Random();
            quizNum++;
            userQuizCache.UserQuizes.Remove(userQuizCache.CurrentQuizNumber);
            if (userQuizCache.UserQuizes.Count == 0)
            {
                response.AddText($"問題は以上です。全部で{correct_count}問正解しました！");
            }
            else
            {
                userQuizCache.CurrentQuizNumber =
                    userQuizCache.UserQuizes.Skip(random.Next(0, userQuizCache.UserQuizes.Count - 1)).Take(1).First();
                currentQuiz = QuizService.GetQuizByNumber(userQuizCache.CurrentQuizNumber);

                response.AddText($"第{quizNum}問");
                response.AddUrl("https://eigodequiz.azurewebsites.net/assets/quizintro.mp3");
                response.AddText(currentQuiz.QuestionType);
                response.AddText(currentQuiz.Question, Lang.En);
                SetReprompt();
                SaveSession(quizNum, 1, correct_count);
            }
        }
    }
}
