using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseQuiz.Scripts
{
    public class PostgresRep : IRepository
    {

        private string connectionString = "Host=localhost;Username=postgres;Password=;DataBase=quizGame";
        private NpgsqlDataSource dateSource;

        private enum Categories
        {
            LoveCraft,
            DataBaser,
            Henrettelsesmetoder,
            Koreansk,
            Superhelte
        }
        #region Start
        public void Init()
        {
            dateSource = NpgsqlDataSource.Create(connectionString);


            GenTabels();
            GenerateCategories();
            GenerateQuestions();
        }


        private void GenTabels()
        {
            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS users (" +
                "username VARCHAR(30) PRIMARY KEY," +
                "total_score INT DEFAULT 0" +
                ");"
            ).ExecuteNonQuery();

            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS categories (" +
                "name VARCHAR(30) PRIMARY KEY," +
                "description VARCHAR(30) NOT NULL" +
                ");"
            ).ExecuteNonQuery();

            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS answers (" +
                "answer_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY," +
                "description VARCHAR(30) NOT NULL " +
                ");"
            ).ExecuteNonQuery();


            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS questions (" +
                "question_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY," +
                "correct_answer_id INT, " +
                "picked BOOL DEFAULT False, " +
                "difficulty INT DEFAULT 1, " +
                "description VARCHAR(30) NOT NULL, " +
                "FOREIGN KEY (correct_answer_id) REFERENCES answers(answer_id)" +
                ");"
            ).ExecuteNonQuery();


            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS cat_has_questions (" +
                "category_name VARCHAR(30)," +
                "question_id INT," +
                "FOREIGN KEY (category_name) REFERENCES categories(name)," +
                "FOREIGN KEY (question_id) REFERENCES questions(question_id)" +
                ");"
            ).ExecuteNonQuery();

            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS question_has_answers (" +
                "question_id INT," +
                "answer_id INT," +
                "FOREIGN KEY (question_id) REFERENCES questions(question_id)," +
                "FOREIGN KEY (answer_id) REFERENCES answers(answer_id)" +
                ");"
            ).ExecuteNonQuery();

            TruncateTable("users");
            TruncateTable("cat_has_questions");
            TruncateTable("question_has_answers");
            TruncateTable("questions");
            TruncateTable("answers"); 
            TruncateTable("categories"); 
        }


        private void TruncateTable(string table)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand($"TRUNCATE TABLE {table} RESTART IDENTITY CASCADE;"); 
            cmd.ExecuteNonQuery();
        }
        private void GenerateCategories()
        {
            AddToCategory(Categories.LoveCraft, "Noget");
            AddToCategory(Categories.DataBaser, "Noget");
            AddToCategory(Categories.Henrettelsesmetoder, "Noget");
            AddToCategory(Categories.Koreansk, "Noget");
            AddToCategory(Categories.Superhelte, "Noget");
        }
        private void AddToCategory(Categories categoryName, string description)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("INSERT INTO categories (name, description) VALUES ($1, $2);");
            cmd.Parameters.AddWithValue(categoryName.ToString());
            cmd.Parameters.AddWithValue(description);
            cmd.ExecuteNonQuery();
        }
        private void GenerateQuestions()
        {
            //
            CreateQuestion(Categories.LoveCraft, "Boi", 5, "YES", new string[] { "not this", "also not this" });
            CreateQuestion(Categories.DataBaser, "EQQ", 5, "Rigtig", new string[] { "not this", "also not this" });
            CreateQuestion(Categories.Koreansk, "FASF", 5, "Rigtig", new string[] { "not this", "also not this" });
            CreateQuestion(Categories.Superhelte, "HGVG", 5, "Rigtig", new string[] { "not this", "also not this" });
            CreateQuestion(Categories.Henrettelsesmetoder, "A gun", 3, "Rigtig", new string[] { "not this", "also not this" });
        }

        private void CreateQuestion(Categories category, string description, int difficulty, string correct_answer_description, string[] wrong_answers_description)
        {
            foreach (var answer in wrong_answers_description)
            {
                GenerateAnswer(answer);
            }

            GenerateAnswer(correct_answer_description);


            //Add to question
            int correct_answer_id = ReturnIdOfAnswersOrQuestions("answers", "answer_id", correct_answer_description);

            AddQuestion(description, difficulty, correct_answer_id);

            int question_id = ReturnIdOfAnswersOrQuestions("questions", "question_id", description);
            
            
            // Add to CategoryHasAnswer
            AddCategoryHasAnswer(category, question_id);


            // Add to the QuestionHasAnswer
            AddQuestionHasAnswers(correct_answer_id, question_id);

            foreach (var wrong_answer in wrong_answers_description)
            {
                int answer_id = ReturnIdOfAnswersOrQuestions("answers", "answer_id", wrong_answer);
                AddQuestionHasAnswers(answer_id, question_id);
            }
        }

        private void AddCategoryHasAnswer(Categories category, int question_id)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("INSERT INTO cat_has_questions (category_name, question_id) VALUES ($1, $2);");
            cmd.Parameters.AddWithValue(category.ToString()); //Dont need to find it, since we already know it has the exact name as our enum
            cmd.Parameters.AddWithValue(question_id);
            cmd.ExecuteNonQuery();
        }

        private void AddQuestionHasAnswers(int answer_id, int question_id)
        {
            NpgsqlCommand addCategory = dateSource.CreateCommand("INSERT INTO question_has_answers (answer_id, question_id) VALUES ($1, $2);");
            addCategory.Parameters.AddWithValue(answer_id);
            addCategory.Parameters.AddWithValue(question_id);
            addCategory.ExecuteNonQuery();
        }

        private void AddQuestion(string description, int difficulty, int correct_answer_id)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("INSERT INTO questions (description, difficulty, correct_answer_id) VALUES ($1, $2, $3);");
            cmd.Parameters.AddWithValue(description);
            cmd.Parameters.AddWithValue(difficulty);
            cmd.Parameters.AddWithValue(correct_answer_id);
            cmd.ExecuteNonQuery();
        }

        private void GenerateAnswer(string description)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("INSERT INTO answers (description) VALUES ($1);");
            cmd.Parameters.AddWithValue(description);
            cmd.ExecuteNonQuery();
        }

        private int ReturnIdOfAnswersOrQuestions(string table, string attribute, string description)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand($"SELECT {attribute} FROM {table} WHERE description = $1");
            cmd.Parameters.AddWithValue(description);

            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }
            throw new Exception("Need to have the description in the table before trying to find it");
        }

        private string ReturnDescriptionOfAnswersOrQuestions(string table, string attribute, int id)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand($"SELECT description FROM {table} WHERE {attribute} = $1;");
            cmd.Parameters.AddWithValue(id);

            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetString(0);
                }
            }

            throw new Exception("Cant find the desciption. Remeber to use a existing table and check for spelling errors.");
        }

        #endregion

        public void AddToUserScore(string username, int scoreToAdd)
        {
            int currentScore = ReturnUserScore(username);
            //NpgsqlCommand cmd = dateSource.CreateCommand("UPDATE questions SET picked = True WHERE  ");
            NpgsqlCommand cmd = dateSource.CreateCommand("UPDATE users SET total_score = $1 WHERE username = $2;");
            cmd.Parameters.AddWithValue(currentScore + scoreToAdd);
            cmd.Parameters.AddWithValue(username);
            cmd.ExecuteNonQuery();

            Console.WriteLine();
        }

        public int ReturnUserScore(string username)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("SELECT * FROM users WHERE username = $1;");
            cmd.Parameters.AddWithValue(username);
            
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    string usernameRead = reader.GetString(0);
                    int total_score = reader.GetInt32(1);

                    if (usernameRead == username)
                    {
                        return total_score;
                    }
                }
            }

            throw new Exception("Cant find the username in the users table");
        }

        public void AddUser(string username)
        {
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
            Console.WriteLine("Categories:");

            using (NpgsqlDataReader reader = cmdAllCategories.ExecuteReader())
            {
                int index = 1;
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

        public string SelectFromCategories(List<string> categories)
        {
            Console.WriteLine("\nVælg en categori ved at skrive deres tilsvarende nummer:");
            string input = Console.ReadLine();

            // Check if the input is a number
            if (int.TryParse(input, out int index) && index > 0 && index <= categories.Count)
            {
                return categories[index - 1];
            }
            else
            {
                Console.WriteLine("Invalid input. Prøv igen.");
                return SelectFromCategories(categories); //Making a loop
            }
        }

        public List<int> GetQuestions(string selectedCategory)
        {
            List<int> questionIds = new List<int>();
            NpgsqlCommand cmd = dateSource.CreateCommand("SELECT (question_id) FROM cat_has_questions WHERE category_name = $1;");
            cmd.Parameters.AddWithValue(selectedCategory);

            Console.WriteLine($"Du har valgt categorien {selectedCategory}:");
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                int index = 1;
                while (reader.Read())
                {
                    int question_id = reader.GetInt32(0);
                    //Get description thats in the cat_has_questions
                    string description = ReturnDescriptionOfAnswersOrQuestions("questions", "question_id", question_id);
                    
                    questionIds.Add(question_id);
                    Console.WriteLine($"    {index}. {description}");
                }
            }

            Console.WriteLine();
            return questionIds;
        }

        public int SelectFromQuestions(List<int> questionIds)
        {
            Console.WriteLine("\nVælg en spøgsmål ved at skrive deres tilsvarende nummer til deres sværhedsgrad:");
            string input = Console.ReadLine();

            // Check if the input is a number
            if (int.TryParse(input, out int index) && index > 0 && index <= questionIds.Count)
            {
                return questionIds[index - 1];
            }
            else
            {
                Console.WriteLine("Invalid input. Prøv igen.");
                return SelectFromQuestions(questionIds); //Making a loop
            }
        }

        public List<int> GetAnswers(int selectedQuestionId)
        {
            List<int> answerIds = new List<int>();
            NpgsqlCommand cmd = dateSource.CreateCommand("SELECT (answer_id) FROM question_has_answers WHERE question_id = $1;");
            cmd.Parameters.AddWithValue(selectedQuestionId);

            string questionDescription = ReturnDescriptionOfAnswersOrQuestions("questions", "question_id", selectedQuestionId);
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
            
            List<int> randomizedAnswerIds = answerIds.OrderBy(x => rnd.Next()).ToList();

            for (int i = 0; i < randomizedAnswerIds.Count; i++)
            {
                string description = ReturnDescriptionOfAnswersOrQuestions("answers", "answer_id", randomizedAnswerIds[i]);
                Console.WriteLine($"    {i + 1}. {description}");
            }

            Console.WriteLine();
            return randomizedAnswerIds;
        }

        public void SelectFromAnswers(List<int> answerIds, int selectedQuestionId, string username)
        {
            Console.WriteLine("\nVælg en svar ved at skrive deres tilsvarende nummer:");
            string input = Console.ReadLine();

            // Check if the input is a number
            if (int.TryParse(input, out int index) && index > 0 && index <= answerIds.Count)
            {
                //Check om svaret er rigtigt og giv derefter difficulty til spillerens score.
                //
                string answerDescription = ReturnDescriptionOfAnswersOrQuestions("answers", "answer_id", answerIds[index - 1]);
                Console.WriteLine($"\nDu har valgt svaret: {answerDescription}");
                
                int correct_answer_id = GetSelectedAnswerId(selectedQuestionId);
                if (correct_answer_id == answerIds[index - 1])
                {
                    int difficulty = GetDiffucltyInQuestion(selectedQuestionId);
                    int score = difficulty * 100;
                    AddToUserScore(username, score);
                    Console.WriteLine($"Det er korrekt, +{score} points til spiller {username}\n");
                }
                else
                {
                    Console.WriteLine($"Svaret er ukorrekt. Det rigtige svar var:");
                    string description = ReturnDescriptionOfAnswersOrQuestions("answers", "answer_id", correct_answer_id);
                    Console.WriteLine($"    - {description}\n");
                }


                //SET QUESTION TO PICKED = TRUE, og vis kun questions some picked = false
            }
            else
            {
                Console.WriteLine("Invalid input. Prøv igen.");
                SelectFromQuestions(answerIds); //Making a loop
            }
        }

        private int GetDiffucltyInQuestion(int selectedQuestionId)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("SELECT (difficulty) FROM questions WHERE question_id = $1");
            cmd.Parameters.AddWithValue(selectedQuestionId);

            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();
                return reader.GetInt32(0);
            }
        }

        private int GetSelectedAnswerId(int selectedQuestionId) 
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("SELECT (correct_answer_id) FROM questions WHERE question_id = $1");
            cmd.Parameters.AddWithValue(selectedQuestionId);
            
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                reader.Read();

                return reader.GetInt32(0);
            }
        }


    }

}
