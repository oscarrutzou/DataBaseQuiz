using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataBaseQuiz.Scripts
{
    public class PostgreRep : IRepository
    {
        private readonly string connectionString = "Host=localhost;Username=postgres;Password=;DataBase=quizGame";
        private NpgsqlDataSource dateSource;

        public void Init()
        {
            dateSource = NpgsqlDataSource.Create(connectionString);
            new PostgresRepInitData(dateSource, this); // To fill in the data
        }

        public void AddUser(string username)
        {
            // Have no check for if the username exists in the users.
            // Can use the GetValue to see if the username already exits and then make some error handling
            NpgsqlCommand cmd = dateSource.CreateCommand("INSERT INTO users (username) VALUES ($1);");
            cmd.Parameters.AddWithValue(username);
            cmd.ExecuteNonQuery();
        }

        public void ShowUsers()
        {
            NpgsqlCommand cmdAllUsers = dateSource.CreateCommand("SELECT * FROM users;");

            Console.WriteLine("Users:");

            using (NpgsqlDataReader reader = cmdAllUsers.ExecuteReader())
            {
                while (reader.Read())
                {
                    string username = reader.GetString(0);
                    int score = reader.GetInt32(1);
                    Console.WriteLine($"    - {username}, Score: {score}");
                }
            }
            Console.WriteLine();
        }

        public List<string> GetCategoryNames()
        {
            List<string> categories = new List<string>();

            NpgsqlCommand cmdAllCategories = dateSource.CreateCommand("SELECT * FROM categories;");

            Console.WriteLine("Kategorier:");

            using (NpgsqlDataReader reader = cmdAllCategories.ExecuteReader())
            {
                int index = 1; // Index shows the player what button they should press to select the category
                while (reader.Read())
                {
                    string category = reader.GetString(0);
                    string description = reader.GetString(1);
                    Console.WriteLine($"    {index}. {category}: {description}");
                    categories.Add(category);
                    index++;
                }
            }

            Console.WriteLine();
            return categories;
        }

        public List<int> GetQuestions(string selectedCategory)
        {
            List<Question> questions = new List<Question>(); // Adds the questions to this list, so we can sort them first based of difficulty
            NpgsqlCommand cmd = dateSource.CreateCommand("SELECT (question_id) FROM cat_has_questions WHERE category_name = $1;");
            cmd.Parameters.AddWithValue(selectedCategory);

            Console.WriteLine($"Du har valgt categorien {selectedCategory}:");
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int question_id = reader.GetInt32(0);

                    if (GetValue<bool, int>("questions", "picked", "question_id", question_id)) //Skip the question if the question has been picked
                    {
                        continue;
                    }

                    int difficuly = GetValue<int, int>("questions", "difficulty", "question_id", question_id);

                    string description = GetValue<string, int>("questions", "description", "question_id", question_id);

                    questions.Add(new Question(question_id, difficuly, description));
                }
            }

            questions = questions.OrderBy(x => x.difficulty).ToList(); // Sorts the list ASC

            for (int i = 0; i < questions.Count; i++) // Writes it into the console
            {
                Console.WriteLine($"    {i + 1}. {questions[i].difficulty * 100} points: {questions[i].description}");
            }

            List<int> questionIds = questions.Select(x => x.question_id).ToList(); // Gets the ids since its needed to select one of the questions

            Console.WriteLine();
            return questionIds;
        }

        public List<int> GetAnswers(int selectedQuestionId)
        {
            List<int> answerIds = new List<int>();
            NpgsqlCommand cmd = dateSource.CreateCommand("SELECT (answer_id) FROM question_has_answers WHERE question_id = $1;");
            cmd.Parameters.AddWithValue(selectedQuestionId);

            string questionDescription = GetValue<string, int>("questions", "description", "question_id", selectedQuestionId);

            Console.WriteLine($"\nDu har valgt spørgsmålet: \n      - {questionDescription}:");

            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int answer_id = reader.GetInt32(0);

                    answerIds.Add(answer_id);
                }
            }

            Random rnd = new Random();

            List<int> randomizedAnswerIds = answerIds.OrderBy(x => rnd.Next()).ToList(); //Randomizer the answers

            for (int i = 0; i < randomizedAnswerIds.Count; i++) // Writes the answers into the console
            {
                string description = GetValue<string, int>("answers", "description", "answer_id", randomizedAnswerIds[i]); 
                
                Console.WriteLine($"    {i + 1}. {description}");
            }

            Console.WriteLine();
            return randomizedAnswerIds;
        }

        public void CheckCorrectAnswer(int selectedAnswerId, int selectedQuestionId, string username)
        {
            UpdateValue("questions", "picked", true, "question_id", selectedQuestionId); //Sets the question to have been picked / chosen, so it wont show again

            //Check om svaret er rigtigt og giv derefter difficulty til spillerens score.
            string answerDescription = GetValue<string, int>("answers", "description", "answer_id", selectedAnswerId);
            Console.WriteLine($"\nDu har valgt svaret: {answerDescription}");

            int correct_answer_id = GetValue<int, int>("questions", "correct_answer_id", "question_id", selectedQuestionId);

            if (correct_answer_id == selectedAnswerId) //If the user is correct
            {
                // Need to first get the difficulty of the current question
                int difficulty = GetValue<int, int>("questions", "difficulty", "question_id", selectedQuestionId);
                
                // Get the current score since we want to use a  single value, and not make a method just for incrementing the user score
                int currentScore = GetValue<int, string>("users", "total_score", "username", username);
                
                // We then multiply it, so the player can get to see some big numbers
                int score = difficulty * 100;

                currentScore += score;

                // We updates the new value on the player
                UpdateValue("users", "total_score", currentScore, "username", username);  // Sets the new score
                Console.WriteLine($"Det er korrekt, +{score} points til spiller {username}\n"); // Shows the score that the user earned
            }
            else
            {
                Console.WriteLine($"Svaret er ikke korrekt. Det rigtige svar var:");
                string description = GetValue<string, int>("answers", "description", "answer_id", correct_answer_id);
                Console.WriteLine($"    - {description}\n");
            }
        }

        public T SelectFromList<T>(string writeBeforeCheck, List<T> list)
        {
            Console.WriteLine($"\n{writeBeforeCheck}");
            string input = Console.ReadLine();

            // Check if the input is a number
            if (int.TryParse(input, out int index) && index > 0 && index <= list.Count)
            {
                return list[index - 1];
            }
            else
            {
                Console.WriteLine("Invalid input. Prøv igen.");
                return SelectFromList(writeBeforeCheck, list); // Return the result of the recursive call
            }
        }

        /// <summary>
        /// Updates a single value in a table
        /// </summary>
        /// <typeparam name="T1">The new value typ</typeparam>
        /// <typeparam name="T2">The search value type</typeparam>
        /// <param name="table"></param>
        /// <param name="attribute">The attribute the new value should change</param>
        /// <param name="newValue">The new changed value</param>
        /// <param name="whereAttribute">The search condition</param>
        /// <param name="searchValue">The search condition value</param>
        public void UpdateValue<T1, T2>(string table, string attribute, T1 newValue, string whereAttribute, T2 searchValue)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand(
                $"UPDATE {table} SET {attribute} = $1 WHERE {whereAttribute} = $2"
            );
            cmd.Parameters.AddWithValue(newValue);
            cmd.Parameters.AddWithValue(searchValue);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Returns a value from a vable
        /// </summary>
        /// <typeparam name="T">The return value</typeparam>
        /// <typeparam name="T1">The where value</typeparam>
        /// <param name="table">The table to search</param>
        /// <param name="selectAttribute">The value to search for</param>
        /// <param name="whereAttribute">The where attribute, unique like a ID</param>
        /// <param name="whereValue">The value of the where attribute, that it needs to find</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public T GetValue<T, T1>(string table, string selectAttribute, string whereAttribute, T1 whereValue)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand(
                $"SELECT ({selectAttribute}) FROM {table} WHERE {whereAttribute} = $1;"
            );

            cmd.Parameters.AddWithValue(whereValue);
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    object value = reader[0];
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                else
                {
                    throw new InvalidOperationException("No rows returned.");
                }
            }
        }

    }
}
