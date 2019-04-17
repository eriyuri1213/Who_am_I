using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        const int PORT_NO = 5000;
        const int TAM = 2048;
        static Socket servidor = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static List<Socket> clientes = new List<Socket>();
        static byte[] buffer = new byte[TAM];

        static void Main(string[] args)
        {
            Configurar();
            Console.ReadLine();
            Terminar();
        }

        private static void Configurar()
        {
            servidor.Bind(new IPEndPoint(IPAddress.Any, PORT_NO));
            Console.WriteLine("Listening...");
            servidor.Listen(0);
            servidor.BeginAccept(Aceitar, null);
            Console.WriteLine("Configuração terminada");
        }

        private static void Aceitar(IAsyncResult ar)
        {
            Socket novo;
            try
            {
                novo = servidor.EndAccept(ar);
            }
            catch (ObjectDisposedException) 
            {
                return;
            }
            clientes.Add(novo);
            novo.BeginReceive(buffer, 0, TAM, SocketFlags.None, Receber, novo);
            Console.WriteLine("Novo cliente conectado");
            servidor.BeginAccept(Aceitar, null);
        }

        private static void Receber(IAsyncResult ar) 
        {
            Socket socket = (Socket)ar.AsyncState;
            int receber;

            try
            {
                receber = socket.EndReceive(ar);
            }
            catch (SocketException)
            {
                Console.WriteLine("Cliente desconectado");
                socket.Close();
                clientes.Remove(socket);
                return;
            }
            byte[] pacote = new byte[receber];
            Array.Copy(buffer, pacote, receber);
            string texto = Encoding.ASCII.GetString(pacote);
            Console.WriteLine("Texto recebido: " + texto);

        }

        private static void Terminar()
        {
            foreach (Socket cliente in clientes)
            {
                cliente.Shutdown(SocketShutdown.Both);
                cliente.Close();
            }
            servidor.Close();
        }
    }
}