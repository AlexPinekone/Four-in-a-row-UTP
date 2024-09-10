using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerConecta4
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //variables necesarias
            byte[] recibeCJ1, recibeCJ2, datosJ1, datosJ2, j1RespuestaB, j2RespuestaB;
            bool servidorEjecutandose=true;
            // Puerto en el que espera
            int port = 12345; 
            UdpClient udpServer = new UdpClient(port);
            //Crea a los dos jugadores con sus "EndPoints" para la coneccion 
            IPEndPoint j1Dispositivo = new IPEndPoint(IPAddress.Any, 0);
            IPEndPoint j2Dispositivo = new IPEndPoint(IPAddress.Any, 0);

            Console.WriteLine("Puerto del servidor: " + port);
            
            //Espera los mensajes de los dos jugadores para identificarlos y "asignarlos"

            recibeCJ1 = udpServer.Receive(ref j1Dispositivo);
            string player1Selectio = Encoding.ASCII.GetString(recibeCJ1);
            Console.WriteLine("Jugador 1 esta: " + player1Selectio);

            recibeCJ2 = udpServer.Receive(ref j2Dispositivo);
            string player2Selectio = Encoding.ASCII.GetString(recibeCJ2);
            Console.WriteLine("Jugador 2 esta: " + player2Selectio);


            while (servidorEjecutandose)
            {
                // Recibe mensaje del primer jugador
                datosJ1 = udpServer.Receive(ref j1Dispositivo);
                string player1Selection = Encoding.ASCII.GetString(datosJ1);
                Console.WriteLine("Jugador 1 seleccionó la columna: " + player1Selection);

                // Enviar del jugador 1 al 2
                j1RespuestaB = Encoding.ASCII.GetBytes(player1Selection);
                udpServer.Send(j1RespuestaB, j1RespuestaB.Length, j2Dispositivo);

                // Recibe mensaje del segundo jugador
                datosJ2 = udpServer.Receive(ref j2Dispositivo);
                string player2Selection = Encoding.ASCII.GetString(datosJ2);
                Console.WriteLine("Jugador 2 seleccionó la columna: " + player2Selection);

                // Enviar del jugador 2 al 1
                j2RespuestaB = Encoding.ASCII.GetBytes(player2Selection);
                udpServer.Send(j2RespuestaB, j2RespuestaB.Length, j1Dispositivo);
            }
        }
    }
}
