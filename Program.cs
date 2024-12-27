using System;
using System.Data;
using Npgsql;

class QuizApp
{
    private static string connectionString = "Host=localhost;Username=postgres;Password=1323;Database=quiz_app";

    static void Main()
    {
        while (true)
        {
            Console.Clear();
            ShowHomePage();

            Console.WriteLine("\n1. Sign Up");
            Console.WriteLine("2. Sign In");
            Console.WriteLine("3. Exit");

            Console.Write("\nSelect an option: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    SignUp();
                    break;
                case "2":
                    SignIn();
                    break;
                case "3":
                    return;
                default:
                    Console.WriteLine("Invalid choice! Press Enter to try again.");
                    Console.ReadLine();
                    break;
            }
        }
    }

    static void ShowHomePage()
    {
        Console.WriteLine("====================================");
        Console.WriteLine("      QUIZ APP - TERMINAL VERSION   ");
        Console.WriteLine("====================================");
        Console.WriteLine("A fun way to test your knowledge!\n");
    }

    static void SignUp()
    {
        Console.Clear();
        Console.WriteLine("=== Sign Up ===");
        Console.Write("Enter Name: ");
        string name = Console.ReadLine();

        Console.Write("Enter Email: ");
        string email = Console.ReadLine();

        Console.Write("Enter Password: ");
        string password = Console.ReadLine();

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("INSERT INTO users (name, email, password) VALUES (@name, @email, @password)", connection);
            command.Parameters.AddWithValue("name", name);
            command.Parameters.AddWithValue("email", email);
            command.Parameters.AddWithValue("password", password);

            try
            {
                command.ExecuteNonQuery();
                Console.WriteLine("Sign Up Successful! Press Enter to continue.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            Console.ReadLine();
        }
    }

    static void SignIn()
    {
        Console.Clear();
        Console.WriteLine("=== Sign In ===");
        Console.Write("Enter Email: ");
        string email = Console.ReadLine();

        Console.Write("Enter Password: ");
        string password = Console.ReadLine();

        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT id, name FROM users WHERE email = @Email AND password = @Password", connection);
            command.Parameters.AddWithValue("Email", email);
            command.Parameters.AddWithValue("Password", password);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    int userId = reader.GetInt32(0);
                    string userName = reader.GetString(1);
                    Console.WriteLine($"\nWelcome, {userName}!");

                    StartQuiz(userId);
                }
                else
                {
                    Console.WriteLine("Invalid credentials. Press Enter to try again.");
                    Console.ReadLine();
                }
            }
        }
    }

    static void StartQuiz(int userId)
    {
        Console.Clear();
        Console.WriteLine("=== Quiz Time ===\n");

        int score = 0;
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("SELECT id, question, option_a, option_b, option_c, option_d, correct_option FROM questions LIMIT 15", connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Console.WriteLine(reader.GetString(1));
                    Console.WriteLine($"A. {reader.GetString(2)}");
                    Console.WriteLine($"B. {reader.GetString(3)}");
                    Console.WriteLine($"C. {reader.GetString(4)}");
                    Console.WriteLine($"D. {reader.GetString(5)}");

                    Console.Write("\nYour answer: ");
                    string answer = Console.ReadLine().ToUpper();

                    if (answer == reader.GetString(6).ToUpper())
                    {
                        score++;
                    }
                }
            }
        }

        SaveScore(userId, score);
        Console.WriteLine($"\nYou scored {score} out of 15!");
        Console.WriteLine("Press Enter to return to the main menu.");
        Console.ReadLine();
    }

    static void SaveScore(int userId, int score)
    {
        using (var connection = new NpgsqlConnection(connectionString))
        {
            connection.Open();
            var command = new NpgsqlCommand("INSERT INTO scores (user_id, score) VALUES (@userId, @score)", connection);
            command.Parameters.AddWithValue("userId", userId);
            command.Parameters.AddWithValue("score", score);
            command.ExecuteNonQuery();
        }
    }
}
