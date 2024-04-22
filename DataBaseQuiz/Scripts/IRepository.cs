using System.Collections.Generic;

namespace DataBaseQuiz.Scripts
{
    public interface IRepository
    {
        /// <summary>
        /// Start Repository
        /// </summary>
        void Init();
        /// <summary>
        /// Adds the user to the specific storage solution
        /// </summary>
        /// <param name="username"></param>
        void AddUser(string username);
        /// <summary>
        /// Shows the users to the console
        /// </summary>
        void ShowUsers();
        /// <summary>
        /// Updates a specific value in the data
        /// </summary>
        /// <typeparam name="T1">The new value that should be inserted into the table</typeparam>
        /// <typeparam name="T2">The value thats used to search for in the table</typeparam>
        /// <param name="table">The table that should be looked in</param>
        /// <param name="attribute">The attribute that should be changed in the table</param>
        /// <param name="newValue">The new value of the attribute</param>
        /// <param name="whereAttribute">The search attribute</param>
        /// <param name="searchValue">The search value that needs to be something unique like a id</param>
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
