using Telegram.Bot;
using Telegram.Bot.Polling;
using ST = System.Timers;
using Npgsql;

class Program
{
    public static string tableName = "tble";
    public static string connectionWay = "Server=lishesisklag.beget.app;Port=5432;User Id=postgres;Password=YpVf4%sw;Database=postgres;";
    public static bool active;
    public static long ChtId = 6264514373;

    static ITelegramBotClient bot = new TelegramBotClient("6947341237:AAGIWhiJGRvCnebwxlvjPToE_1-ybiUOfsQ");
    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

        ChtId = update.Message.Chat.Id;
        if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
        {
            var message = update.Message;

            if (message.Text.ToLower() == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat, "Запуск...");
                return;
            }
            if (message.Text.ToLower() == "/count")
            {
                await botClient.SendTextMessageAsync(message.Chat, "Количество сообщений: " + SendCount());
                return;
            }
            if (message.Text.ToLower() == "/chatid")
            {

                await botClient.SendTextMessageAsync(message.Chat, $"ChatId = {ChtId}");
                return;
            }

            WriteNewLetter(message.Text.ToString());
        }
    }
    public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    }

    static void Main()
    {
        Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
        
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        if (ChtId != 0)
        {
            ST.Timer timer = new ST.Timer(10000);
            timer.Elapsed += Activate;
            timer.Start();
        }
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }, // receive all update types
        };
        bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken
        );

        //if (active)
        //{
        //    await bot.SendTextMessageAsync(ChtId, $"Колво Сообщений {SendCount}");
        //    active = false;
        //}
        

        Console.ReadLine();

        
    }
    static async void Activate(object sender, ST.ElapsedEventArgs e)
    {
        try
        {
            await bot.SendTextMessageAsync(ChtId, "Количество сообщений: " + SendCount());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message} + {ChtId}");
        }
    }
    public static string DataBaseHour()
    {
        string count = "Если тут написано это значит что то сломалось";
        var con = new NpgsqlConnection(connectionString: connectionWay);
        con.Open();

        if (TableExists(con, $"{tableName}"))
        {
            Console.WriteLine($"Таблица {tableName} существует.");
            count = GetRecordCount(con, tableName).ToString();
        }
        con.Close();
        return count;
    }
    public static string SendCount()
    {
        string letterCount = "***";
        try
        {
            var con = new NpgsqlConnection(connectionString: connectionWay);
            con.Open();

            Console.WriteLine(TableExists(con, $"{tableName}"));

            if (TableExists(con, $"{tableName}"))
            {
                letterCount = GetRecordCount(con, tableName).ToString();
            }

            con.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(" ");
            return letterCount;
        }
        return letterCount;
    }
    public static void WriteNewLetter(string let)
    {
        var con = new NpgsqlConnection(connectionString: connectionWay);
        con.Open();

        if (TableExists(con, $"{tableName}"))
        {
            Console.WriteLine($"Таблица {tableName} существует.");
        }
        else
        {
            Console.WriteLine($"Таблица {tableName} не существует и будет создана.");
            CreateEmptyTable(con, tableName);
        }
        InsertRecord(con, tableName, let);
        con.Close();
    }
    static bool TableExists(NpgsqlConnection connection, string tableName)
    {
        NpgsqlCommand command = new NpgsqlCommand();
        
        command.Connection = connection;
        command.CommandText = "SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = @tableName)";
        command.Parameters.AddWithValue("tableName", tableName);
        return (bool)command.ExecuteScalar();
    }
    static void InsertRecord(NpgsqlConnection connection, string tableName, string column1Value)
    {
        NpgsqlCommand command = new NpgsqlCommand();
        
        command.Connection = connection;
        command.CommandText = $"INSERT INTO {tableName} (column1) VALUES (@column1)";
        command.Parameters.AddWithValue("column1", column1Value);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            Console.WriteLine($"Запись {column1Value} успешно добавлена.");
        }
        else
        {
            Console.WriteLine("Произошла ошибка при добавлении записи.");
        }
    }
    static void CreateEmptyTable(NpgsqlConnection connection, string tableName)
    {
        NpgsqlCommand command = new NpgsqlCommand();
        command.Connection = connection;
        command.CommandText = $"CREATE TABLE {tableName} (column1 text);";
        try
        {
            command.ExecuteNonQuery();
            Console.WriteLine($"Таблица {tableName} успешно создана.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при создании таблицы: {ex.Message}");
        }
        
    }
    static int GetRecordCount(NpgsqlConnection connection, string tableName)
    {
        NpgsqlCommand command = new NpgsqlCommand();
        
        command.Connection = connection;
        command.CommandText = $"SELECT COUNT(*) FROM {tableName}";

        try
        {
            object result = command.ExecuteScalar();

            if (result != null && int.TryParse(result.ToString(), out int count))
            {
                return count;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении количества записей: {ex.Message}");
        }

        return 0;
    }
}

