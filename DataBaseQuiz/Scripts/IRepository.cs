
using System.Collections.Generic;

namespace DataBaseQuiz.Scripts
{
    public interface IRepository
    {
        void Init();
        void AddUser(string username);
        int ReturnUserScore(string username);
        void AddToUserScore(string username, int scoreToAdd);
        void ShowUsers();
        List<string> GetCategoryNames();
        List<int> GetQuestions(string selectedCategory);
        List<int> GetAnswers(int selectedQuestionId);
        string SelectFromCategories(List<string> categories);
        int SelectFromQuestions(List<int> questionIds);
        void SelectFromAnswers(List<int> answerIds, int selectedQuestionId, string username);
    }

}
