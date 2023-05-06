
using SPIAPI;
using SupportBot.Service;
using System.Net.Sockets;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SupportBot
{
    class Program
    {
        private static readonly string supportApiBaseUrl = "https://localhost:7165/api/";

        private static ITelegramBotClient botClient;

        private static SupportService supportService;

        private static Service.User? user;
        
        private static Ticket? ticket;

        static async Task Main(string[] args)
        {
            HttpClient client = new HttpClient();

            supportService = new SupportService(client, supportApiBaseUrl);
            _ = await supportService.LoginAsync("Telegram", "Telegram");

            botClient = new TelegramBotClient("");


            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");

            var cts = new CancellationTokenSource();
            botClient.StartReceiving(HandleUpdateAsync, HandleErrorAsync, cancellationToken: cts.Token);

            Console.ReadLine();

            cts.Cancel();
        }

        static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message.Type == MessageType.Text)
            {

                string chatId = update.Message.Chat.Id.ToString();
                string username = update.Message.Chat.Username;

                if (ticket != null && user != null && ticket.AssignedToUserId != null)
                {
                    await supportService.SendMessageToTicketAsync(ticket.Id, user.Id, update.Message.Text);
                }

                if (update.Message.Text == "/start")
                {
                    await supportService.CreateTelegramUserAsync(username, chatId);

                    user = await supportService.GetUserByTelegramIdAsync(chatId);
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Вызвать сотрудника", "call_support"),
                        }
                    });
                    await bot.SendTextMessageAsync(chatId,
                        "Добро пожаловать. Вы попали в сервис технической поддержки. Нажмите кнопку, чтобы вызвать сотрудника поддержки для решения ваших вопросов.",
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken);
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                string chatId = update.CallbackQuery.Message.Chat.Id.ToString();

                if (update.CallbackQuery.Data == "call_support")    
                {
                    
                    if (user != null && user.TicketId == null)
                    {
                        await supportService.CreateTicket(user.Id, "Просто заголовок", "Просто описание");

                        Console.WriteLine(user.TicketId.Value);
                        ticket = await supportService.GetTicketByIdAsync(user.TicketId.Value);


                        await bot.SendTextMessageAsync(chatId,
                            "Один из наших сотрудников свяжется с вами в ближайшее время.",
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await bot.SendTextMessageAsync(chatId, "Вы не можете иметь больше 1-ого тикета. Дождитесь сотрудника, чтобы он закрыл вам тикет.",
                            cancellationToken: cancellationToken);
                    }
                }
            }
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }
    }
}