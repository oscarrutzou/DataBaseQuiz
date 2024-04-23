using System;
using System.Collections.Generic;
using DataBaseQuiz.Scripts;

namespace DataBaseQuiz
{
    internal class Program
    {
        #region Properties
        private static IRepository postRep;

        private static int currentUserIndex = 0;
        private static List<string> usernames = new List<string>();
        
        private static string selectedCategory;
        private static int selectedQuestionId;
        private static int selectedAnswerId;
        #endregion

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8; // Allow the console to use special characters

            postRep = new PostgresRep();
            postRep.Init(); // Start the repositorie and initilize the database

            StartGame(); // Starts the game

            Console.ReadKey();
        }

        private static void StartGame()
        {
            Console.WriteLine("Velkommen til vores QUIZZZZ");

            Console.WriteLine("Start med at skrive hver spillers brugernavne. Hvis I har nok spillere, skal i blot skrive intet for at gå videre.");

            // Gets the current usernames
            int playerCount = 1;
            while (playerCount <= 4)
            {
                Console.WriteLine($"Skriv spiller {playerCount}'s brugernavn:");
                string username = Console.ReadLine();

                if (username == "" && playerCount > 1) break;
                
                playerCount++;

                usernames.Add(username);
                postRep.AddUser(username);
            }

            while (true) //Base loop
            {
                Console.Clear();

                postRep.ShowUsers(); // Writes each user and their points
                
                Console.WriteLine($"\nSpiller {usernames[currentUserIndex]}'s turn:");

                selectedCategory = UserSelectCategory(); // Show and select a category
                
                selectedQuestionId = UserSelectQuestion(selectedCategory); // Use the category to show and select a question, thats under the category

                selectedAnswerId = UserSelectAnswer(selectedQuestionId); // Use the question to show and select a answer

                // Use the answer and question to check if its correct - And add points if its correct
                postRep.CheckCorrectAnswer(selectedAnswerId, selectedQuestionId, usernames[currentUserIndex]);

                Console.WriteLine("Tryk en knap for at starte næste runde");
                Console.ReadKey(true);

                // Makes sure the index cant get over the amount of current users, since it resets to 0 if it does.
                currentUserIndex = currentUserIndex < usernames.Count - 1 ? currentUserIndex + 1 : 0;
            }
        }


        private static int UserSelectAnswer(int selectedQuestionId)
        {
            List<int> answerIds = postRep.GetAnswers(selectedQuestionId);

            string writeBeforeCheck = "Vælg et svar ved at skrive dets tilsvarende nummer:";

            return postRep.SelectFromList(writeBeforeCheck, answerIds);
        }

        private static string UserSelectCategory()
        {
            List<string> categoryNames = postRep.GetCategoryNames();

            string writeBeforeCheck = "Vælg en kategori ved at skrive dens tilsvarende nummer:";

            return postRep.SelectFromList(writeBeforeCheck, categoryNames);
        }


        private static int UserSelectQuestion(string selectedCategory)
        {
            List<int> questionIds = postRep.GetQuestions(selectedCategory);

            while(questionIds.Count == 0) // To make sure you dont get a category with no questions.
            {
                Console.Clear();

                Console.WriteLine("Du har valgt en kategori med ingen spørgsmål, prøv en anden kategori...\n");

                // We dont use the selectedCategory in the StartGame method, so its fine to not set it there again.
                selectedCategory = UserSelectCategory(); 
                questionIds = postRep.GetQuestions(selectedCategory);
            }


            string writeBeforeCheck = "Vælg et spørgsmål ved at skrive deres tilsvarende nummer til deres sværhedsgrad:";

            return postRep.SelectFromList(writeBeforeCheck, questionIds);
        }
    }



   
    
}
