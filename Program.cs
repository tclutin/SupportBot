
using SupportBot.Service;
using SupportBot.Service.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
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

        static async Task Main(string[] args)
        {
            supportService = new SupportService(supportApiBaseUrl);
            _ = await supportService.SendAuthModelAsync("Telegram", "Telegram");

            botClient = new TelegramBotClient("6198831431:AAHOF8x89rppG27arCEQwwWXtYBDG6OPN_4");

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
                var chatId = update.Message.Chat.Id;

                if (update.Message.Text == "/start")
                {
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
                var chatId = update.CallbackQuery.Message.Chat.Id;

                if (update.CallbackQuery.Data == "call_support")
                {
                    var user = await supportService.GetUserByTelegramIdAsync(chatId.ToString());
                    if (user != null && user.TicketId != null)
                    {
                        await bot.SendTextMessageAsync(chatId, "Вы не можете иметь больше 1-ого тикета. Дождитесь сотрудника, чтобы он закрыл вам тикет.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        var message = "Один из наших сотрудников свяжется с вами в ближайшее время.";
                        await bot.SendTextMessageAsync(chatId, message, cancellationToken: cancellationToken);
                    }
                }
            }
        }

        static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(exception.Message);
            return Task.CompletedTask;
        }
    }
}