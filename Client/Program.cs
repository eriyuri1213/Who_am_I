using System;
using System.Net.Sockets;
using System.Text;
using System.Net;

namespace Client
{
    class Program
    {
        static readonly Socket Cliente = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        const int PORT_NO = 5000;

        static void Main(string[] args) 
        {
            ConectarServidor();
            Conexao();
        }

        private static void ConectarServidor()
        {
            int conex = 0;
            while (!Cliente.Connected)
            {
                try
                {
                    conex++;
                    Console.WriteLine("Conexão " + conex);
                    Cliente.Connect(IPAddress.Loopback, PORT_NO);
                }
                catch (SocketException)
                {
                    Console.WriteLine("Erro de socket");
                }

                Console.WriteLine("Conectado");
            }
        }

        private static void Conexao() 
        {
            while (true)
            {
                Enviar();
                Resposta();
            }
        }

        private static void Enviar()
        {
            //Dados para enviar para o servidor
            Console.Write("Digite uma mensagem: ");
            string mensagem = Console.ReadLine();
            string textToSend = mensagem;

            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);
            Cliente.Send(bytesToSend, 0, bytesToSend.Length, SocketFlags.None);
        }

        private static void Resposta(){ 
            //Le a resposta
            byte[] buffer = new byte[2048];
            int resposta = Cliente.Receive(buffer, SocketFlags.None);
            if (resposta == 0)
                return;
            var dado = new byte[resposta];
            Array.Copy(buffer, dado, resposta);
            string texto = Encoding.ASCII.GetString(dado);
            Console.WriteLine("Mensagem recebida: "+texto);
        }
    }
}
