using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace DataBaseQuiz.Scripts
{
    public class Question
    {
        public int question_id;
        public int difficulty;
        public string description;
        public Question(int question_id, int difficulty, string description)
        {
            this.question_id = question_id;
            this.difficulty = difficulty;
            this.description = description;
        }
    }
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
                "description VARCHAR(150) NOT NULL" +
                ");"
            ).ExecuteNonQuery();

            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS answers (" +
                "answer_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY," +
                "description VARCHAR(150) NOT NULL " +
                ");"
            ).ExecuteNonQuery();


            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS questions (" +
                "question_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY," +
                "correct_answer_id INT, " +
                "picked BOOL DEFAULT False, " +
                "difficulty INT DEFAULT 1, " +
                "description VARCHAR(150) NOT NULL, " +
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
        private void AddToCategory(Categories categoryName, string description)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand("INSERT INTO categories (name, description) VALUES ($1, $2);");
            cmd.Parameters.AddWithValue(categoryName.ToString());
            cmd.Parameters.AddWithValue(description);
            cmd.ExecuteNonQuery();
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


        #region GenerateCategoriesAndQuestions
        private void GenerateCategories()
        {
            AddToCategory(Categories.LoveCraft, "Noget");
            AddToCategory(Categories.DataBaser, "Noget");
            AddToCategory(Categories.Henrettelsesmetoder, "Noget");
            AddToCategory(Categories.Koreansk, "Koreansk med udgang i formel samtaler");
            AddToCategory(Categories.Superhelte, "Noget");
        }
        private void GenerateQuestions()
        {
            GenerateQuestionsLoveCraft();
            GenerateQuestionsDataBaser();
            GenerateQuestionsKoreansk();
            GenerateQuestionsSuperhelte();
            GenerateQuestionsHenrettelsesmetoder();
        }

        private void GenerateQuestionsLoveCraft()
        {
            CreateQuestion(Categories.LoveCraft, "Boi", 5, "YES", new string[] { "not this", "also not this" });

        }

        private void GenerateQuestionsDataBaser()
        {
            CreateQuestion(Categories.DataBaser, "Hvad står SQL for?", 1, "Structured Query Language", new string[] { "Structured Query Linguistics", "Standard Query Library", "Sequential Query Logic" });
            CreateQuestion(Categories.DataBaser, "EQQ", 5, "Rigtig", new string[] { "not this", "also not this" });

        }
        private void GenerateQuestionsKoreansk()
        {
            //CreateQuestion(Categories.Koreansk, "Spørgsmål", 5, "Rigtig svar", new string[] { "forkert svar1", "forkert svar2", "forkert svar3" });
            CreateQuestion(Categories.Koreansk, "Hvad er det koreanske alfabet kendt som?", 1, "Hangul", new string[] { "Hiragana", "Katakana", "Kanji" });
            CreateQuestion(Categories.Koreansk, "Hvordan siger man \"hej\" på koreansk?", 2, "안녕하세요 (Annyeonghaseyo)", new string[] { "こんにちは (Konnichiwa)", "你好 (Nǐ hǎo)", "Hola" });
            CreateQuestion(Categories.Koreansk, "Hvilken af følgende er korrekt for at sige \"Jeg elsker dig\" på koreansk?", 3, "사랑해요 (Saranghaeyo)", new string[] { "사랑해요 (Saranghae)", "사랑해요 (Saranghaey)", "사랑해요 (Sarangha)" });
            CreateQuestion(Categories.Koreansk, "Hvordan ville du skrive \"musik\" på koreansk?", 4, "음악 (Eumak)", new string[] { "음막 (Eummak)", "음삭 (Eusak)", "음박 (Eumbak)" });
            CreateQuestion(Categories.Koreansk, "Hvilken af følgende er korrekt for at sige \"Jeg vil gerne have en kop kaffe\" på koreansk?", 5, "커피 한 잔 주세요 (Keopi han jan juseyo)", new string[] { "커피 한 잔 주세 (Keopi han jan juse)", "커피 한 잔 주새요 (Keopi han jan jusaeyo)", "커피 한 잔 주쎄요 (Keopi han jan jusseyo)" });
        }
        private void GenerateQuestionsSuperhelte()
        {
            CreateQuestion(Categories.Superhelte, "HGVG", 5, "Rigtig", new string[] { "not this", "also not this" });

        }
        private void GenerateQuestionsHenrettelsesmetoder()
        {
            CreateQuestion(Categories.Henrettelsesmetoder, "A gun", 3, "Rigtig", new string[] { "not this", "also not this" });

        }
        #endregion


   
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

        public List<int> GetQuestions(string selectedCategory)
        {
            List<Question> questions = new List<Question>();
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
                    //From 1 to 5 in difficulty, only one with 
                    //Get description thats in the cat_has_questions
                    string description = ReturnDescriptionOfAnswersOrQuestions("questions", "question_id", question_id);
                    
                    questions.Add(new Question(question_id, difficuly, description));
                }
            }

            questions = questions.OrderBy(x => x.difficulty).ToList();

            for (int i = 0; i < questions.Count; i++)
            {
                Console.WriteLine($"    {i + 1}. {questions[i].difficulty * 100} points: {questions[i].description}");
            }

            List<int> questionIds = questions.Select(x => x.question_id).ToList();

            Console.WriteLine();
            return questionIds;
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

        public void CheckCorrectAnswer(int selectedAsnwerId, int selectedQuestionId, string username)
        {
            UpdateValue("questions", "picked", true, "question_id", selectedQuestionId); //Sets the question to have been picked / chosen, so it wont show again

            //Check om svaret er rigtigt og giv derefter difficulty til spillerens score.
            string answerDescription = ReturnDescriptionOfAnswersOrQuestions("answers", "answer_id", selectedAsnwerId);
            Console.WriteLine($"\nDu har valgt svaret: {answerDescription}");

            int correct_answer_id = GetValue<int, int>("questions", "correct_answer_id", "question_id", selectedQuestionId);

            if (correct_answer_id == selectedAsnwerId)
            {
                int difficulty = GetValue<int, int>("questions", "difficulty", "question_id", selectedQuestionId); 
                int score = difficulty * 100;
                UpdateValue("users", "total_score", score, "username", username);
                Console.WriteLine($"Det er korrekt, +{score} points til spiller {username}\n");
            }
            else
            {
                Console.WriteLine($"Svaret er ukorrekt. Det rigtige svar var:");
                string description = ReturnDescriptionOfAnswersOrQuestions("answers", "answer_id", correct_answer_id);
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
        /// Updates a value from a table
        /// </summary>
        /// <typeparam name="T1">For the newValue</typeparam>
        /// <typeparam name="T2">For the searchValue</typeparam>
        /// <param name="table">The table that should be checked</param>
        /// <param name="attribute"></param>
        /// <param name="newValue"></param>
        /// <param name="whereAttribute"></param>
        /// <param name="searchValue"></param>
        public void UpdateValue<T1, T2>(string table, string attribute, T1 newValue, string whereAttribute, T2 searchValue)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand($"UPDATE {table} SET {attribute} = $1 WHERE {whereAttribute} = $2");
            cmd.Parameters.AddWithValue(newValue);
            cmd.Parameters.AddWithValue(searchValue);
            cmd.ExecuteNonQuery();
        }

        private T GetValue<T, T1>(string table, string selectAttribute, string whereAttribute, T1 whereValue)
        {
            NpgsqlCommand cmd = dateSource.CreateCommand($"SELECT ({selectAttribute}) FROM {table} WHERE {whereAttribute} = $1;");
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
