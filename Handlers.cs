using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Examples.Polling
{

    public class Handlers
    {
        private static readonly HashSet<long> greetedChats = new HashSet<long>();
        private static readonly Dictionary<long, BotState> userStates = new Dictionary<long, BotState>();
        private static readonly Dictionary<long, bool> awaitingCommand = new Dictionary<long, bool>();
        private static readonly List<string> teamNames = new List<string>();
        public enum BotState
        {
            Greeting,
            TeamNameInput,
            WaitingStart,
            StartGameInput
        }
        public static List<string> GetTeamNames()
        {
            return teamNames;
        }
        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => HandleMessageUpdate(botClient, update),
                //UpdateType.EditedMessage => HandleEditedMessageUpdate(botClient, update),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(botClient, update.CallbackQuery, update.EditedMessage!),
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }
        private static async Task HandleMessageUpdate(ITelegramBotClient botClient, Update update)
        {
            await BotOnMessageReceived(botClient, update.Message!);
            await HandleUserResponse(botClient, update.Message!); // ���������, ��������� �� ����� �� ������������ ����� ���������
        }
        public static async Task HandleUserResponse(ITelegramBotClient botClient, Message message)
        {
            if (userStates.TryGetValue(message.Chat.Id, out var state))
            {
                switch (state)
                {
                    case BotState.TeamNameInput:
                        await HandleUserTeam(botClient, message);
                        break;
                    case BotState.StartGameInput:
                        await HandleStartGame(botClient, message);
                        break;
                    default:
                        // Invalid state, do nothing
                        break;
                }
            }
            //// ���������, ���� ��������� ����� �� ������� ����
            //if (awaitingCommand.TryGetValue(message.Chat.Id, out var isWaiting) && isWaiting)
            //{
            //    // ����� ����� ������������ ����� ������������
            //    string userResponse = message.Text;
            //    await botClient.SendTextMessageAsync(message.Chat.Id, $"�� �����: {userResponse}");

            //    // ���������� ��������� �������� ������
            //    awaitingCommand[message.Chat.Id] = false;
            //}
        }

       


        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {

            if (message.From.Username == "katerinka_kartinka")
            {
                if(message.Text=="������123")
                {
                    userStates[message.Chat.Id] = BotState.StartGameInput;
                }
                if (!greetedChats.Contains(message.Chat.Id))
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "Hi Kate");
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "�����! ��� ��� ���������� �������� � ������������ �����. ��� ����������� ������ ���� \n\n Hello! This bot is designed for entertainment purposes only. Select a language to continue",
                        replyMarkup: new InlineKeyboardMarkup(new[]
                        {
                        new []
                        {
                            InlineKeyboardButton.WithCallbackData("���������", "1"),
                            InlineKeyboardButton.WithCallbackData("English", "2")
                        }
                        }));

                    greetedChats.Add(message.Chat.Id);
                   
                }
            }
        }

        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery, Message message)
        {
            var chatId = callbackQuery.Message.Chat.Id;
            var data = callbackQuery.Data;

            if (data == "1")
            {
                await botClient.SendTextMessageAsync(chatId, "�� ������ ��������� ����");
                await botClient.SendTextMessageAsync(chatId, "������ ����� ���� �������:");
                userStates[chatId] = BotState.TeamNameInput;
            }
            else if (data == "2")
            {
                await botClient.SendTextMessageAsync(chatId, "You have chosen English language");
               
            }

            await botClient.EditMessageReplyMarkupAsync(chatId, callbackQuery.Message.MessageId, replyMarkup: null);

        }
        // ��������� ������ ������������
 

        public static async Task HandleUserTeam(ITelegramBotClient botClient, Message message)
        {
            var chatId = message.Chat.Id;
            string userResponse = message.Text;
            await botClient.SendTextMessageAsync(message.Chat.Id, $"�� �����: {userResponse}");
            teamNames.Add(userResponse);
            await botClient.SendTextMessageAsync(message.Chat.Id, "���������� �� ������ ���...");
        }
        public static async Task HandleStartGame(ITelegramBotClient botClient, Message message)
        {
           
        }
    }
}