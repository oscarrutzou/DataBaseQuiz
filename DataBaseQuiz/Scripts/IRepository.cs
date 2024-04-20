
using System.Collections.Generic;

namespace DataBaseQuiz.Scripts
{
    public interface IRepository
    {
        void Init();
        void AddUser(string username);
        int ReturnUserScore(string username);

        void ShowUsers();
        List<string> GetCategories();
        void GetQuestions();
        void GetAnswers();
        string SelectFromCategories(List<string> categories);
        void SelectFromQuestions(int index);
        void SelectFromAnswers(int index);
    }

}
