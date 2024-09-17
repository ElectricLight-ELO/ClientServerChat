using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

namespace AnonChat
{
    class Program
    {
        public class Message
        {
            public string user1;
            public string user2;
            public List<string> MB = new List<string>();
        }

        public static ConcurrentDictionary<string, Message> message_base = new ConcurrentDictionary<string, Message>();

        public static bool CheckPerep(string n1, string n2)
        {
            return message_base.TryGetValue(Get_ConversationKey(n1, n2), out _);
        }

        public static string Get_ConversationKey(string id1, string id2)
        {
            List<string> _base = new List<string> { id1, id2 };
            _base.Sort();
            return $"{_base[0]}:{_base[1]}";
        }

        public static async Task Obrabotka(Socket handler)
        {
            byte[] bytes = new byte[1024];
            int bytesReceived = await handler.ReceiveAsync(new ArraySegment<byte>(bytes), SocketFlags.None);
            
            string data = Encoding.UTF8.GetString(bytes, 0, bytesReceived);

            if (data.Contains("<message>"))
            {
                Match regex = Regex.Match(data, "<id>(.*?)</id><idTO>(.*?)</idTO><message>(.*?)</message>");
                string id = regex.Groups[1].ToString();
                string id_to = regex.Groups[2].ToString();
                string message = regex.Groups[3].ToString();
                message = RC4.Crypt(Encoding.UTF8.GetBytes(message), RC4.getPass(id, id_to));
                Console.WriteLine($"{id}: {data}");

                string conversationKey = Get_ConversationKey(id, id_to);
                if (CheckPerep(id, id_to))
                {
                    if (message_base.TryGetValue(conversationKey, out Message conversation))
                    {
                        conversation.MB.Add($"{id}: {message}\r\n");

                        if (conversation.MB.Count > 0)
                        {
                            byte[] response = Encoding.UTF8.GetBytes(string.Join("", conversation.MB));
                            
                            await handler.SendAsync(new ArraySegment<byte>(RC4.CryptBytes(response, RC4.getPass(id, id_to))), SocketFlags.None);
                        }
                    }
                }
                else
                {
                    Message newConversation = new Message();
                    newConversation.user1 = id;
                    newConversation.user2 = id_to;
                    newConversation.MB.Add($"{id}: {message}\r\n");
                    message_base[conversationKey] = newConversation;

                    if (newConversation.MB.Count > 0)
                    {
                        byte[] response = Encoding.UTF8.GetBytes(string.Join("", newConversation.MB));
                        await handler.SendAsync(new ArraySegment<byte>(RC4.CryptBytes(response, RC4.getPass(id, id_to))), SocketFlags.None);
                    }
                }
            }

            if (data.Contains("<update>"))
            {
                Match regex = Regex.Match(data, "<id>(.*?)</id><idTO>(.*?)</idTO>");
                string id_to = regex.Groups[2].ToString();
                string id = regex.Groups[1].ToString();

                string conversationKey = Get_ConversationKey(id, id_to);
                if (message_base.TryGetValue(conversationKey, out Message conversation))
                {
                    if (conversation.MB.Count > 0)
                    {
                        byte[] response = Encoding.UTF8.GetBytes(string.Join("", conversation.MB));
                        await handler.SendAsync(new ArraySegment<byte>(RC4.CryptBytes(response, RC4.getPass(id, id_to))), SocketFlags.None);
                    }
                }
            }

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }

        static async Task Main(string[] args)
        {
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int port = 11000;
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            listener.Bind(localEndPoint);

            listener.Listen(10);

            Console.WriteLine($"Waiting for a connection... | Port: {port}");

            while (true)
            {
                Socket handler = await listener.AcceptAsync();
                _ = Obrabotka(handler);
            }
        }
    }
}
