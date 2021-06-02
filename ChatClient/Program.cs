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
            drawGameInfo(game);
            drawGameBoard(game);

            while (true)
            {
                string input = Console.ReadLine();
                if(input.Length < 5)
                {
                    continue;
                }
                else
                {
                    string command = input.Substring(0, 5);
                    if (command == "!exit")
                    {
                        break;
                    }
                    else if (command == "!move")
                    {
                        string move = input.Substring(6, input.Length - 6);
                        if (game.MakeMove(move))
                        {
                            sendMessage($"player make move: {move}");
                            drawGameInfo(game);
                            drawGameBoard(game);
                        }
                        else
                        {
                            Console.WriteLine("Cant make this move.");
                        }

                    }
                    else if (command == "!send")
                    {
                        string message = input.Substring(6, input.Length - 6);
                        sendMessage(message);
                    }
                }
            }
        }

        static private void drawGameInfo(ChessGame game)
        {
            Console.WriteLine("////////////////////////////////////////\n" +
                "New game loaded.\n" +
                $"Turn #{game.Turn}. Turn owner: {game.TurnOwner}. Rule of 50: {game.RuleOf50}\n" +
                $"CastlingAvailable:\n" +
                $"White King: {game.WhiteKingCastlingAvailable}\n" +
                $"White Queen: {game.WhiteQueenCastlingAvailable}\n" +
                $"Black King: {game.BlackKingCastlingAvailable}\n" +
                $"Black Queen: {game.BlackQueenCastlingAvailable}");
            Console.WriteLine("////////////////////////////////////////");
        }

        static private void drawGameBoard(ChessGame game)
        {
            Console.WriteLine("*---------------*");

            for (int y = 7; y >= 0; y--)
            {
                Console.Write("|");
                for (int x = 0; x < 8; x++)
                {
                    Figure figure = game.getFigureAt(x, y);
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

            Console.WriteLine("*---------------*");
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
