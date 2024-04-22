
using System.Collections.Generic;

namespace DataBaseQuiz.Scripts
{
    public interface IRepository
    {
        void Init();
        void AddUser(string username);
        void ShowUsers();
        void UpdateValue<T1, T2>(string table, string attribute, T1 newValue, string whereAttribute, T2 searchValue);
        List<string> GetCategoryNames();
        List<int> GetQuestions(string selectedCategory);
        List<int> GetAnswers(int selectedQuestionId);
        //string SelectFromCategories(List<string> categories);
        //int SelectFromQuestions(List<int> questionIds);
        //int SelectFromAnswers(List<int> answerIds);
        T SelectFromList<T>(string writeBeforeCheck, List<T> list);
        void CheckCorrectAnswer(int selectedAsnwerId, int selectedQuestionId, string username);
    }

}
