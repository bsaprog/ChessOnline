﻿using System;
using System.Numerics;
using System.Threading.Tasks;
using ChessLogic;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatClient
{
    class Program
    {
        static HubConnection ChatHubConnection;
        static ChessGame game;

        static void Main(string[] args)
        {
            ConnectToChatHub();
            StartNewGame();

            while (true)
            {
                string input = Console.ReadLine();

                if(input.Length == 0)
                {
                    continue;
                }

                bool isCommand = input[0] == '!';
                if(!isCommand)
                {
                    continue;
                }

                int firstSpaceIndex = input.IndexOf(' ');
                string command = input.Substring(1, firstSpaceIndex != -1 ? firstSpaceIndex -1 : input.Length - 1).ToLower();
                string commandArgs = firstSpaceIndex != -1 ? input.Substring(firstSpaceIndex + 1, input.Length - firstSpaceIndex - 1) : "";

                if (command == "exit")
                {
                    break;
                }
                else if (command == "move")
                {
                    if (game.MakeMove(commandArgs))
                    {
                        DrawGameInfo();
                        DrawGameBoard();
                    }
                    else
                    {
                        Console.WriteLine("Cant make this move.");
                    }
                }
                else if (command == "message")
                {
                    SendMessage(commandArgs);
                }
                else if (command == "restart")
                {
                    StartNewGame();
                }
                else if (command == "load")
                {
                    LoadGame(commandArgs);
                }
                else if(command == "loadtemplate")
                {
                    string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
                    if (commandArgs == "1")
                    {
                        fen = "kn2r1R1/pb6/8/3Q4/8/8/8/2K5 w - - 0 1";
                    }
                    LoadGame(fen);
                }
            }
        }

        static private void ConnectToChatHub()
        {
            ChatHubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:41729/notification")
                .Build();

            ChatHubConnection.On<string>("Send", message => Console.WriteLine($"Message from server: {message}"));
            ChatHubConnection.StartAsync();
        }

        static private void StartNewGame()
        {
            game = new ChessGame();
            DrawGameInfo();
            DrawGameBoard();
        }

        static private void LoadGame(string fen = "")
        {
            game = new ChessGame(fen);
            DrawGameInfo();
            DrawGameBoard();
        }

        static private void DrawGameInfo()
        {
            Console.WriteLine(game.GetStateString());
        }

        static private void DrawGameBoard()
        {
            Console.WriteLine("  +---------------+");

            for (int y = 7; y >= 0; y--)
            {
                Console.Write($" {y + 1}|");
                for (int x = 0; x < 8; x++)
                {
                    Figure figure = game.GetFigureAt(new Vector2(x, y));
                    if (figure == null)
                    {
                        Console.Write(" ");
                    }
                    else
                    {
                        Console.ForegroundColor = figure.Color == Color.White ? ConsoleColor.White : ConsoleColor.DarkRed;
                        Console.Write((Char)figure.Type);
                        Console.ResetColor();
                    }
                    Console.Write("|");
                }
                Console.Write("\n");
            }

            Console.WriteLine("  +---------------+");
            Console.WriteLine("   a b c d e f g h");
        }

        static private void SendMessage(string message)
        {
            ChatHubConnection.SendAsync("SendMessage", message);
        }

    }
}
