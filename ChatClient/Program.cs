using System;
using System.Drawing;
using System.Numerics;
using System.Threading;
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
                else if(command == "template")
                {
                    string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
                    if (commandArgs == "1")
                    {
                        fen = "kn2r1R1/pb6/8/3Q4/8/8/8/2K5 w - - 0 1";
                    }
                    else if(commandArgs == "2")
                    {
                        fen = "2k5/2pr4/8/8/6BQ/8/8/K7 w - - 0 1";
                    }
                    else if (commandArgs == "3")
                    {
                        fen = "8/8/8/p1PppppP/Pkp1PP1P/6K1/8/8 w - - 0 1";
                    }
                    LoadGame(fen);
                }
                else if(command == "randommove")
                {
                    while(game.MakeRandomMove() )
                    {
                        Thread.Sleep(600);
                        DrawGameInfo();
                    }
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
        }

        static private void LoadGame(string fen = "")
        {
            game = new ChessGame(fen);
            DrawGameInfo();
        }

        static private void DrawGameInfo()
        {

            var boardTextTuple = game.GetGameTextTuple();
            foreach (var t in boardTextTuple)
            {
                KnownColor tc = t.Item1;
                ConsoleColor c;

                if (tc == KnownColor.White)
                {
                    c = ConsoleColor.White;
                }
                else if (tc == KnownColor.DarkRed)
                {
                    c = ConsoleColor.DarkRed;
                }
                else
                {
                    c = ConsoleColor.DarkGray;
                }

                Console.ForegroundColor = c;
                Console.Write(t.Item2);
                Console.ResetColor();
            }
        }

        static private void SendMessage(string message)
        {
            ChatHubConnection.SendAsync("SendMessage", message);
        }

    }
}
