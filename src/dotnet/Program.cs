﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace MyApp
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            string baconTask = await RunServer();
            Console.WriteLine("baconTask: " + baconTask);
        }

        private static async Task<string> RunServer()
        {
            var hostName = Dns.GetHostName();
            IPHostEntry localhost = await Dns.GetHostEntryAsync(hostName);
            IPAddress ipAddress = localhost.AddressList[0];

            IPEndPoint ipEndPoint = new(ipAddress, 11_000);

            using Socket listener = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(ipEndPoint);
            listener.Listen(8080);

            var handler = await listener.AcceptAsync();
            while (true)
            {
                var buffer = new byte[1_024];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|EOM|>";
                if (response.IndexOf(eom) > -1 /* is end of message */)
                {
                    Console.WriteLine(
                        $"Socket server received message: \"{response.Replace(eom, "")}\"");

                    var ackMessage = "<|ACK|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(echoBytes, 0);
                    Console.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");

                    break;
                }
            }

            return DateTime.Now.ToString();
        }
    }
}
