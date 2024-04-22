using Npgsql;
using System;

namespace DataBaseQuiz.Scripts
{
    public class PostgresRepInitData
    {
        private NpgsqlDataSource dateSource;

        /// <summary>
        /// Gets used like strings to make the categories or add to questions.
        /// </summary>
        private enum Categories
        {
            LoveCraft,
            DataBaser,
            Henrettelsesmetoder,
            Koreansk,
            Superhelte
        }

        public PostgresRepInitData(NpgsqlDataSource dateSource)
        {
            this.dateSource = dateSource;

            GenTabels();
            GenerateCategories();
            GenerateQuestions();
        }

        #region Generate Tables

        private void GenTabels()
        {
            // Creates users table
            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS users (" +
                "username VARCHAR(30) PRIMARY KEY," +
                "total_score INT DEFAULT 0" +
                ");"
            ).ExecuteNonQuery();

            // Creates categories table
            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS categories (" +
                "name VARCHAR(30) PRIMARY KEY," +
                "description VARCHAR(150) NOT NULL" +
                ");"
            ).ExecuteNonQuery();

            // Creates answers table
            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS answers (" +
                "answer_id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY," +
                "description VARCHAR(150) NOT NULL " +
                ");"
            ).ExecuteNonQuery();

            // Creates questions table
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

            // Creates cat_has_questions table to hold references to categories and its questions
            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS cat_has_questions (" +
                "category_name VARCHAR(30)," +
                "question_id INT," +
                "FOREIGN KEY (category_name) REFERENCES categories(name)," +
                "FOREIGN KEY (question_id) REFERENCES questions(question_id)" +
                ");"
            ).ExecuteNonQuery();

            // Creates question_has_answers table to hold references to questions and its answers
            dateSource.CreateCommand(
                "CREATE TABLE IF NOT EXISTS question_has_answers (" +
                "question_id INT," +
                "answer_id INT," +
                "FOREIGN KEY (question_id) REFERENCES questions(question_id)," +
                "FOREIGN KEY (answer_id) REFERENCES answers(answer_id)" +
                ");"
            ).ExecuteNonQuery();

            // We truncate each table, because we then dont need to worry if the items exits already
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

        #endregion Generate Tables

        #region Generate Categories And Questions

        private void GenerateCategories()
        {
            AddToCategory(Categories.LoveCraft, "Omhandler H.P. Lovecrafts værker og karakterer.");
            AddToCategory(Categories.DataBaser, "Fokuserer på databaser, SQL, og databasedesign.");
            AddToCategory(Categories.Henrettelsesmetoder, "Dækker forskellige henrettelsesmetoder gennem historien.");
            AddToCategory(Categories.Koreansk, "Centreret om det koreanske sprog, med fokus på høflig samtaler.");
            AddToCategory(Categories.Superhelte, "Omfatter forskellige superhelte og deres universer.");
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
            CreateQuestion(Categories.LoveCraft, "Hvilke af H.P. Lovecrafts værker er det mest kendte?", 1, "The Call of Cthulhu.", new string[] { "The Rats in the Walls.", "The Case of Charles Dexter Ward.", "The Colour Out of Space." });
            CreateQuestion(Categories.LoveCraft, "Hvad er navnet på Lovecrafts længste værk og hvor langt er det?", 2, "The Case of Charles Dexter Ward ca. 180 sider.", new string[] { "At the Mountains of Madness på 158 sider.", "The Shadow over Innsmouth på 106 sider.", "The Shadow Out of Time på ca. 70 sider." });
            CreateQuestion(Categories.LoveCraft, "Hvilken \"Antagonist\" er inspireret af \nYog-Sothoth, Shub-Niggurath og Hastur(Him who is not to be named)?", 3, "Yogg-Saron.", new string[] { "Sauron.", "Voldemort.", "Tzeentch." });
            CreateQuestion(Categories.LoveCraft, "Hvilken \"Outer God\" bliver holdt i en magisk søvn for at verdenen kan eksistere?", 4, "Azathoth, \"The Blind Idiot God\".", new string[] { "Shub-Niggurath, \"The Black Goat of the Woods with a Thousand Young\".", "Nyarlathotep, \"Crawling Chaos\" og \"God of a Thousand Forms\".", "Yog-Sothoth, \"The Key and the Gate\"." });
            CreateQuestion(Categories.LoveCraft, "I bogen \"The Call of Cthulhu\" synger cultisterne \n\"Ph'nglui mglw'nafh Cthulhu R'lyeh wgah'nagl fhtagn\" i deres messer, hvad betyder dette?", 5, "\"In his house at R'lyeh, dead Cthulhu waits dreaming\".", new string[] { "Intet, det er blot volapyk fordi de er blevet vanvittige.", "\"The lord of the city R'lyeh, Cthulhu will rise\".", "\"The master Cthulhu will awaken from R'lyeh, the city of dreams\"." });
        }

        private void GenerateQuestionsDataBaser()
        {
            CreateQuestion(Categories.DataBaser, "Hvad står SQL for?", 1, "Structured Query Language", new string[] { "Structured Query Linguistics", "Standard Query Library", "Sequential Query Logic" });
            CreateQuestion(Categories.DataBaser, "Hvilke af disse kan ikke tilføjes for at øge sikkerheden for kodeord?", 2, "Rainbow table", new string[] { "Salt", "Pepper", "Iterations" });
            CreateQuestion(Categories.DataBaser, "Hvad er Normaliseringsreglerne i databasedesign?", 3, "De indeholder kontrolspørgsmål som kan bruges til at minimere redundans og undgå unødvendig kompleksitet", new string[] { "De bestemmer, hvordan man sikrer, at data er krypteret under overførsel", "De fastlægger, hvordan man sikrer, at data kun kan tilgås af autoriserede brugere", "De bestemmer, hvordan man sikrer, at data backup udføres regelmæssigt for at undgå tab af information" });
            CreateQuestion(Categories.DataBaser, "Hvad er forskellen mellem en primær nøgle og en unik nøgle i en database?", 4, "En primær nøgle kan ikke være NULL og der kan kun være én", new string[] { "En primær nøgle kan indeholde NULL og der kan kun være én", "En primær nøgle kan indeholde NULL og der kan være flere", "En primær nøgle kan ikke indeholde NULL og der kan være flere" });
            CreateQuestion(Categories.DataBaser, "Hvad står ACID for?", 5, "Atomicity Consistency Isolation Durabilty", new string[] { "Automatic Committed Isolated Data", "Abstraction Consistency Isolated Data", "Atomic Continuous Isolated Durability" });
        }

        private void GenerateQuestionsKoreansk()
        {
            CreateQuestion(Categories.Koreansk, "Hvad er det koreanske alfabet kendt som?", 1, "Hangul", new string[] { "Hiragana", "Katakana", "Kanji" });
            CreateQuestion(Categories.Koreansk, "Hvordan siger man \"hej\" på koreansk?", 2, "안녕하세요 (Annyeonghaseyo)", new string[] { "こんにちは (Konnichiwa)", "你好 (Nǐ hǎo)", "Hola" });
            CreateQuestion(Categories.Koreansk, "Hvilken af følgende er korrekt for at sige \"Jeg elsker dig\" på koreansk (høflig)?", 3, "사랑해요 (Saranghaeyo)", new string[] { "사랑해 (Saranghae)", "사랑헤요 (Sarangheyo)", "사랑혜요 (Saranghyeyo)" });
            CreateQuestion(Categories.Koreansk, "Hvordan ville du skrive \"musik\" på koreansk?", 4, "음악 (Eumak)", new string[] { "음막 (Eummak)", "음삭 (Eusak)", "음박 (Eumbak)" });
            CreateQuestion(Categories.Koreansk, "Hvilken af følgende er korrekt for at sige \"Jeg vil gerne have en kop kaffe\" på koreansk?", 5, "커피 한 잔 주세요 (Keopi han jan juseyo)", new string[] { "커피 한 잔 주세 (Keopi han jan juse)", "커피 한 잔 주새요 (Keopi han jan jusaeyo)", "커피 한 잔 주쎄요 (Keopi han jan jusseyo)" });
        }

        private void GenerateQuestionsSuperhelte()
        {
            CreateQuestion(Categories.Superhelte, "Hvilken superhelt kommer fra krypton?", 1, "Superman", new string[] { "Captain Marvel", "Greenlantern", "Captain America" });
            CreateQuestion(Categories.Superhelte, "Hvilken Marvel film er årsagen til at marvel startede “MCU”?", 2, "Blade", new string[] { "Fantastic 4", "Howard the duck", "The Punisher" });
            CreateQuestion(Categories.Superhelte, "Hvilken skuespiller var lige ved at spille Iron Man før Robert Downey Junior blev casted?", 3, "Tom Cruise", new string[] { "Nicolas cage", "Patrick swayzee", "Sylvester stallone" });
            CreateQuestion(Categories.Superhelte, "Hvad hed Batmans første Robin?", 4, "Dick Grayson", new string[] { "Damian Wayne", "Tim Drake", "Jason Todd" });
            CreateQuestion(Categories.Superhelte, "Hvem spiller Butcher i amzons tv-serien the boys?", 5, "Carl Urban", new string[] { "Mel Gibson", "Al Pacino", "Scarllet johansson" });
        }

        private void GenerateQuestionsHenrettelsesmetoder()
        {
            CreateQuestion(Categories.Henrettelsesmetoder, "Hvilken Henrettelsesmetode er den mest anvendte i dag?", 1, "Dødelig indsprøjtning", new string[] { "Halshugning", "Elektrochok", "Stening" });
            CreateQuestion(Categories.Henrettelsesmetoder, "Hvornår blev guillotinen opfundet?", 2, "1792", new string[] { "1685", "1702", "1849" });
            CreateQuestion(Categories.Henrettelsesmetoder, "Hvad refererer den ældste indiske henrettelsesmetode, Gunga rao til?", 3, "lemlæstelse med døden til følge af elefant.", new string[] { "Stening", "forgiftet af kobraslanger", "Død ved sømmåtte" });
            CreateQuestion(Categories.Henrettelsesmetoder, "I antikken anvendte grækerne og romerne dette instrument til bl.a. at dræbe kristne martyrer.", 4, "Bronzetyren", new string[] { "Guillotine", "Den elektriske stol", "Et stort Trækors" });
            CreateQuestion(Categories.Henrettelsesmetoder, "Hvilken henrettelsesmetode blev anvendt til tilfangetagne fjender i vikingetiden?", 5, "Blodørn", new string[] { "Hængning", "Fjenderne blev kastet i et hul med slanger", "Fjenderne blev gasset i et gaskammer" });
        }

        #endregion Generate Categories And Questions
    }
}