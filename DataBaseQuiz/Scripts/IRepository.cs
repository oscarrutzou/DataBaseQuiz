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

        /// <summary>
        /// Have to return a list that contains the category name in the categories table
        /// </summary>
        /// <returns></returns>
        List<string> GetCategoryNames();

        /// <summary>
        /// Return the questions that is a part of the category.
        /// </summary>
        /// <param name="selectedCategory">Used to find the questions, since it needs the selected category from the user</param>
        /// <returns></returns>
        List<int> GetQuestions(string selectedCategory);

        /// <summary>
        /// Returns the answers thats in each question.
        /// </summary>
        /// <param name="selectedQuestionId">The selected question id, thats used to find the answers</param>
        /// <returns></returns>
        List<int> GetAnswers(int selectedQuestionId);

        /// <summary>
        /// Generic list to make it easy to work with multiple datatypes
        /// </summary>
        /// <typeparam name="T">The return value, will be a specefic item from the list</typeparam>
        /// <param name="writeBeforeCheck">Whats writtin in the console before each check, so the same pops up, even if the user writes a wrong number</param>
        /// <param name="list">The list that the value should be returned from</param>
        /// <returns></returns>
        T SelectFromList<T>(string writeBeforeCheck, List<T> list);

        /// <summary>
        /// To check if the selected answer is correct, by looking at the forgein key in the question
        /// </summary>
        /// <param name="selectedAnswerId">The selected answer id</param>
        /// <param name="selectedQuestionId">The selected question, that the answer is a part of</param>
        /// <param name="username">The username of the current user, so they can get points if they are correct</param>
        void CheckCorrectAnswer(int selectedAnswerId, int selectedQuestionId, string username);
    }

}
