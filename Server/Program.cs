using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat
{
    class Servidor
    {
        private static TcpListener serverSocket = default(TcpListener);
        private static Socket clientSocket = default(Socket);
        private static readonly int maxClientsCount = 4;
        private static readonly handleClient[] clients = new handleClient[maxClientsCount];

        static void Main(string[] args)
        {

            Console.Title = "Servidor";

            serverSocket = new TcpListener(IPAddress.Any, 7777);
            clientSocket = default(Socket);
            serverSocket.Start();

            while (true)
            {
                Console.WriteLine("Esperando conexões...");
                clientSocket = serverSocket.AcceptSocket();
                Console.WriteLine("Conectado!");
                int i = 0;
                for (i = 0; i < maxClientsCount; i++)
                {
                    if (clients[i] == null)
                    {
                        (clients[i] = new handleClient()).startClient(clientSocket, clients);
                        break;
                    }
                }

                if (i == maxClientsCount)
                {
                    StreamWriter ots = new StreamWriter(new NetworkStream(clientSocket));
                    ots.AutoFlush = true;
                    ots.WriteLine("*** Servidor Cheio ***");
                    ots.Close();
                    clientSocket.Close();
                }
            }
        }
    }

    public class handleClient
    {
        private Socket clientSocket;
        private handleClient[] clients;
        private int maxClientsCount;
        private String clientName;
        private StreamReader ins;
        private StreamWriter ots;
        private String palavra;
        private int jogadorDaVez;
        
        public void startClient(Socket inClientSocket, handleClient[] clients)
        {
            this.clientSocket = inClientSocket;
            this.clients = clients;
            this.maxClientsCount = clients.Length;

            ots = new StreamWriter(new NetworkStream(clientSocket));
            ots.AutoFlush = true;

            if (inClientSocket.Equals(clients[0].clientSocket))
            {
                ots.WriteLine("*** Voce e o mestre ***");
                clients[0].jogadorDaVez = 1;
            }

            Thread ctThread = new Thread(Jogo);
            ctThread.Start();
        }

        private void Jogo()
        {
            int maxClientsCount = this.maxClientsCount;
            handleClient[] clients = this.clients;

            try
            {
                ins = new StreamReader(new NetworkStream(clientSocket));
                ots = new StreamWriter(new NetworkStream(clientSocket));
                ots.AutoFlush = true;
                String name;

                ots.WriteLine("*** Informe seu nome ***");
                name = ins.ReadLine().Trim();

                Console.WriteLine("Novo usuario: " + name);
                ots.WriteLine("*** Ola " + name + " ***\n*** Para sair digite /quit ***");

                lock (this)
                {
                    for (int i = 0; i < maxClientsCount; i++)
                    {
                        if (clients[i] != null && clients[i] == this)
                        {
                            clientName = name;
                            break;
                        }
                    }

                    for (int i = 0; i < maxClientsCount; i++)
                    {
                        if (clients[i] != null && clients[i] != this)
                        {
                            clients[i].ots.WriteLine("*** Novo usuario entrou: " + name + " ***");
                        }
                    }
                }

                if (clientSocket.Equals(clients[0].clientSocket))
                {
                    ots.WriteLine("***Informe a palavra do jogo: ***");
                    palavra = ins.ReadLine();
                    Console.WriteLine("***Palavra do jogo: " + palavra + "***");
                }

                while (true)
                {
                    //Console.WriteLine(jogadorDaVez);
                    //Console.WriteLine(clients.Length);
                    if (clientSocket.Equals(clients[clients[0].jogadorDaVez].clientSocket) || clientSocket.Equals(clients[0].clientSocket))
                    {
                        if (clientSocket.Equals(clients[clients[0].jogadorDaVez].clientSocket))
                        {
                            ots.WriteLine("***Jogador da vez !***\n***Faca a pergunta: ***");
                            String pergunta = ins.ReadLine();

                            if (pergunta.StartsWith("/quit"))
                            {
                                break;
                            }

                            else
                            {
                                lock (this)
                                {
                                    for (int i = 0; i < maxClientsCount; i++)
                                    {
                                        if (clients[i] != null && clients[i] != null)
                                            clients[i].ots.WriteLine("*** O usuario " + name + " perguntou: " + pergunta+ "***");

                                    }

                                    clients[0].ots.WriteLine("***Digite a resposta: ***");
                                    String resposta = clients[0].ins.ReadLine();

                                    for (int i = 0; i < maxClientsCount; i++)
                                    {
                                        if (clients[i] != null && clients[i] != null)
                                            clients[i].ots.WriteLine("*** Mestre respondeu: " + resposta+ "***");
                                    }

                                    ots.WriteLine("***Faca o chute:***");
                                    String chute = this.ins.ReadLine();

                                    if (chute.ToLower() == clients[0].palavra.ToLower())
                                    {
                                        Console.WriteLine("***Palavra adivinhada***");
                                        for (int i = 0; i < maxClientsCount; i++)
                                        {
                                            if (clients[i] != null && clients[i] != null)
                                                clients[i].ots.WriteLine("*** O usuario " + name + " acertou a palavra " + palavra + " e o VENCEDOR ***");

                                        }

                                        using (var writer = new StreamWriter("/home/erica/Downloads/guessWho-master (1)/guessWho-master/Server/pontos.txt", true))
                                        {
                                            DateTime dt = DateTime.Now;
                                            writer.WriteLine("Jogo " + dt.ToString("yyyy/MM/dd HH:mm"));
                                            writer.WriteLine("Jogadores: ");

                                            writer.WriteLine("Mestre: " + clients[0].clientName);

                                            for (int i = 1; i < maxClientsCount; i++)
                                            {
                                                if (clients[i] != null)
                                                    writer.WriteLine("Jogador " + i + ": " + clients[i].clientName);
                                            }

                                            writer.WriteLine("Jogador " + clients[clients[0].jogadorDaVez].clientName + " ganhou!\n\n");

                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < maxClientsCount; i++)
                                        {
                                            if (clients[i] != null && clients[i] != null)
                                                clients[i].ots.WriteLine("*** O usuario " + name + " errou a palavra");
                                        }
                                    }
                                }
                            }
                        }

                        if (clientSocket.Equals(clients[clients[0].jogadorDaVez].clientSocket))
                        {
                            clients[0].jogadorDaVez++;
                            if (clients[0].jogadorDaVez >= clients.Length)
                                clients[0].jogadorDaVez = 1;
                        }
                    }

                    else
                    {
                        String line = ins.ReadLine();
                        ots.WriteLine("*** Aguarde sua vez! ***");
                    }
                }

                Console.WriteLine("Usuario " + name + " se desconectou");
                lock (this)
                {
                    for (int i = 0; i < maxClientsCount; i++)
                    {
                        if (clients[i] != null && clients[i] != null)
                        {
                            clients[i].ots.WriteLine("*** O usuario " + name + " saiu ***");
                        }
                    }
                }
                ots.WriteLine("*** Ate logo " + name + " ***");

                lock (this)
                {
                    for (int i = 0; i < maxClientsCount; i++)
                    {
                        if (clients[i] == this)
                        {
                            clients[i] = null;
                        }
                    }
                }
                ins.Close();
                ots.Close();
                clientSocket.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        
        }
    }
}
