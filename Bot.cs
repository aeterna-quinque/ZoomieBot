﻿using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;

namespace GimmeTheZoomBot
{
    public class Bot
    {
        public ITelegramBotClient _botClient;

        public Bot(string token)
        {
            _botClient = new TelegramBotClient(token);
            _botClient.OnMessage += StartCommand;
            _botClient.OnMessage += GetGmailCommand;
            _botClient.OnCallbackQuery += AuthCommand;
        }

        private async void GetGmailCommand(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text)
            {
                if (e.Message.Text == @"/get")
                {
                    GmailServiceWorker.GetGmailMessage();
                    
                    await _botClient.SendTextMessageAsync(e.Message.Chat.Id, "Done!");
                }
            }
        }

        private async void AuthCommand(object sender, CallbackQueryEventArgs e)
        {
            var message = e.CallbackQuery;
            if (message.Data == "Auth")
            {
                GmailServiceWorker.Init(message.Message.Chat.Id);
                GmailServiceWorker.GetService();

                string messageToUser = $"Вы успешно прошли авторизацию, как {GmailServiceWorker.GetGmailName(message.Message.Chat.Id)}";

                await _botClient.SendTextMessageAsync(message.Message.Chat.Id, messageToUser);
            }
            else if (message.Data == "ReAuth")
            {
                GmailServiceWorker.ReInit(message.Message.Chat.Id);
                GmailServiceWorker.GetService();
            }
        }

        public void Start()
        {
            _botClient.StartReceiving();
        }

        public async void StartCommand(object sender, MessageEventArgs e)
        {
            //e.Message.Chat.Type == ChatType.Group
            if (e.Message.Type == MessageType.Text)
            {
                if (e.Message.Text == @"/start")
                {
                    InlineKeyboardMarkup inlineKeyboard;
                    string message;

                    //if (!GmailServiceWorker.IsAuth(e.Message.Chat.Id))
                    {
                        inlineKeyboard = new InlineKeyboardMarkup(
                            new[] { InlineKeyboardButton.WithCallbackData("Авторизируйся здесь!", "Auth") }
                        );

                        message = "Привет! Чтобы получать уведомления о парах, необходимо подключить почту!";
                    }
                    //else
                    //{
                    //    inlineKeyboard = new InlineKeyboardMarkup(
                    //        new[] { InlineKeyboardButton.WithCallbackData("Авторизовать другую почту!", "ReAuth") }
                    //    );

                    //    message = $"Ты уже авторизован как {GmailServiceWorker.GetGmailName(e.Message.Chat.Id)}";
                    //}

                    await _botClient.SendTextMessageAsync(e.Message.Chat.Id, message, replyMarkup: inlineKeyboard);
                }
            }
        }
    }
}
