using System;
using System.Threading.Tasks;
using ChessLogic;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatClient
{
    class Program
    {
        static HubConnection ChatHubConnection;

        static void Main(string[] args)
        {
            ConnectToChatHub();
            ChessGame game = new ChessGame();
            drawGame(game);

            sendMessage("i am loaded");
            Console.ReadKey();
        }

        static private void drawGame(ChessGame game)
        {
            Console.WriteLine("////////////////////////////////////////");
            Console.WriteLine("New game loaded.");
            Console.WriteLine($"Turn #{game.Turn}. Turn owner: {game.TurnOwner}. Rule of 50: {game.RuleOf50}");
            Console.WriteLine($"CastlingAvailable:\n" +
                $"White King: {game.WhiteKingCastlingAvailable}\n" +
                $"White Queen: {game.WhiteQueenCastlingAvailable}\n" +
                $"Black King: {game.BlackKingCastlingAvailable}\n" +
                $"Black Queen: {game.BlackQueenCastlingAvailable}");

            Console.WriteLine("----------");

            for (int y = 7; y >= 0; y--)  
            {
                Console.Write("|");
                for (int x = 0; x < 8; x++)
                {
                    Figure figure = game.getFigureAt(x, y);
                    if (figure == null)
                    {
                        Console.Write("*");
                    }
                    else
                    {
                        Console.ForegroundColor = figure.Color == Color.White ? ConsoleColor.White : ConsoleColor.DarkRed;
                        Console.Write((Char)figure.Type);
                        Console.ResetColor();
                    }
                }
                Console.Write("|\n");
            }

            Console.WriteLine("----------");

            Console.WriteLine("////////////////////////////////////////");
        }

        static private void ConnectToChatHub()
        {
            ChatHubConnection = new HubConnectionBuilder()
                .WithUrl("http://localhost:41729/notification")
                .Build();

            ChatHubConnection.On<string>("Send", message => Console.WriteLine($"Message from server: {message}"));
            ChatHubConnection.StartAsync();
        }

        static private void sendMessage(string message)
        {
            ChatHubConnection.SendAsync("SendMessage", message);
        }
    }
}
