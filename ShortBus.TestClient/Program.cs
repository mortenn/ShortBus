using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using ShortBus.Client;

namespace ShortBus.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            client = new ClientPump();
            client.Consumer = ShowMessage;
            client.Start();

            RunChatClientLoop();

            client.Stop();
        }

        static void SendEvent(string name, string payload)
        {
            client.Broadcast(new ShortBus.Contracts.ServiceBusEvent
            {
                EventId = Guid.NewGuid(),
                Sender = Environment.UserName,
                EventName = name,
                Payload = payload
            });
        }

        static void RunChatClientLoop()
        {
            Console.WriteLine("Welcome to ShortBus, hit escape to exit. Type a message and hit enter to send.");
            SendEvent("ClientConnected", string.Empty);

            message = string.Empty;
            while (true)
            {
                ShowPrompt();
                ConsoleKeyInfo keyPress = Console.ReadKey();
                lock (consoleLock)
                {
                    if (keyPress.Key == ConsoleKey.Escape)
                        break;

                    if (keyPress.Key == ConsoleKey.Enter)
                    {
                        SendEvent("ClientChat", message);
                        message = string.Empty;
                        Console.SetCursorPosition(0, Console.CursorTop);
                    }
                    else if (keyPress.Key == ConsoleKey.Backspace && message.Length > 0)
                        message = message.Substring(0, message.Length - 1);

                    else if (keyPress.Modifiers == ConsoleModifiers.Control && keyPress.Key == ConsoleKey.U)
                        message = string.Empty;

                    else
                        message += keyPress.KeyChar;
                }
            }
            SendEvent("ClientDisconnected", string.Empty);
        }

        private static void ShowPrompt()
        {
            lock (consoleLock)
            {
                ClearLine();
                Console.Write("> " + message.Substring(Math.Max(0, message.Length + 4 - Console.WindowWidth)));
            }
        }

        private static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            for (int i = 0; i < Console.WindowWidth - 1; ++i)
                Console.Write(" ");
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        static void ShowMessage(ShortBus.Contracts.ServiceBusEvent e)
        {
            lock (consoleLock)
            {
                ClearLine();
                switch (e.EventName)
                {
                    case "ClientConnected":
                        Console.WriteLine("{0} connected to chat", e.Sender);
                        break;
                    case "ClientDisconnected":
                        Console.WriteLine("{0} left chat", e.Sender);
                        break;
                    case "ClientChat":
                    default:
                        Console.WriteLine("[{0}] <{1}> {2}", e.MessageSent.ToShortTimeString(), e.Sender, e.Payload);
                        break;
                }
                ShowPrompt();
            }
        }

        static DateTime startup = DateTime.Now;
        static string message;
        static object consoleLock = new object();
        static ClientPump client;
    }
}
