using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConectaCuatro
{
    public partial class Form1 : Form
    {
        //Para que el juego funcione
            //El arreglo de los paneles visuales
        static private Panel[,] paneles;
            //El arreglo doble donde se guardan las jugadas
        static int[,] tabla = new int[6, 7];
            //Quien esta jugando aqui
        static int currentPlayer = 1;
            //Se acabó el juego?
        static bool gameOver = false;
            //La ultima columna seleccionada que se pasa entre metodos
        static int columnaSeleccion;
            //La columna del otro jugador
        static int columnaOponente;

        //Para la red
        //Bloquea que se pueda jugar
        static bool juegoActivo=true;
        //Permite que se mande la columna al otro jugador
        static bool enviaMensaje=false;

        //Para la coneccion de red
        //Hay que cambiar esto dependiendo de la maquina y la red donde se usa si no:127.0.0.1
        string serverIP = "10.103.17.47";
        int serverPort = 12345;
        UdpClient udpClient = new UdpClient();
        public Form1()
        {
            InitializeComponent();
            iniciaArregloPaneles();
            IniciaTabla();
            etiqueta1.Text = "Jugador: " + currentPlayer;
            Thread conexion = new Thread(conectate);

            conexion.Start();
        }

        public void conectate()
        {
            try
            {

                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
                byte[] columnSelectionByte = Encoding.ASCII.GetBytes(columnaSeleccion.ToString());
                udpClient.Send(columnSelectionByte, columnSelectionByte.Length, serverEndPoint);
                if (currentPlayer == 2)
                {
                    cambiaJugador();
                    juegoActivo = false;
                    byte[] receivedData = udpClient.Receive(ref serverEndPoint);
                    //Todo se bloque hasta obtener respuesta
                    string opponentSelection = Encoding.ASCII.GetString(receivedData);
                    columnaOponente = int.Parse(opponentSelection);
                    //MessageBox.Show("SI LLEGA1");
                    movimientoRevisa(columnaOponente);
                    //MessageBox.Show("SI LLEGA2");
                    juegoActivo=true;
                    
                }


                while (true)
                {
                    if (enviaMensaje)
                    {
                        // Enviar la selección de columna al servidor
                        byte[] columnSelectionBytes = Encoding.ASCII.GetBytes(columnaSeleccion.ToString());
                        udpClient.Send(columnSelectionBytes, columnSelectionBytes.Length, serverEndPoint);
                        enviaMensaje = false;

                        juegoActivo=false;

                        // Esperar la selección de columna del otro jugador
                        byte[] receivedData = udpClient.Receive(ref serverEndPoint);
                        string opponentSelection = Encoding.ASCII.GetString(receivedData);
                        columnaOponente = int.Parse(opponentSelection);

                        movimientoRevisa(columnaOponente);

                        juegoActivo = true;
                        //Console.WriteLine("El oponente seleccionó la columna: " + opponentSelection);
                    }

                    


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }


        private void p00_Paint(object sender, PaintEventArgs e)
        {

        }

        void accionJuego()
        {
            if (gameOver)
            {
                MessageBox.Show("Fin del juego");
                vaciaTablero();
                IniciaTabla();
                gameOver=false;
                currentPlayer = 1;
                //Reiniciar el tablero
            }
            else
            {
                int columna = columnaSeleccion;
                //MessageBox.Show("AAAAA: " + columna);
                if (verificaMov(columna))
                {
                    movimientoRevisa(columna);
                    enviaMensaje = true;
                    //Igual y esto es innecesario ...
                    //recibiMensaje = false;
                    //while (recibiMensaje == false) ;
                }
                else
                {
                    MessageBox.Show("Casilla no valida");
                }
            }
        }

        void movimientoRevisa(int columna)
        {
            //MessageBox.Show("A0");
            int row = movimiento(columna);
            //MessageBox.Show("A1");
            if (revisaGanador(row, columna))
            {
                MessageBox.Show("¡Jugador " + currentPlayer + " ha ganado!");
                gameOver = true;
            }
            else if (verificaLleno())
            {
                MessageBox.Show("Empate");
                gameOver = true;
            }
            else
            {
                //MessageBox.Show("A2");
                cambiaJugador();
                //MessageBox.Show("A3");
            }
        }

        static void IniciaTabla()
        {
            for(int row = 0; row < 6; row++)
            {
                for(int col = 0; col < 7; col++)
                {
                    tabla[row, col] = 0;
                }
            }
        }

        static bool verificaMov(int column)
        {
            return column >= 0 && column < 7 && tabla[0, column] == 0;
        }

        int movimiento(int column)
        {
            for (int row = 5; row >= 0; row--)
            {
                if (tabla[row, column] == 0)
                {
                    tabla[row, column] = currentPlayer;
                    if(currentPlayer==1)
                        paneles[row, column].BackColor = Color.Red;
                    else
                        paneles[row, column].BackColor = Color.Yellow;
                    return row;
                }
            }
            return -1; // No debería llegar aquí
        }

        public void cambiaJugador()
        {
            if (currentPlayer == 1)
                currentPlayer = 2;
            else
                currentPlayer = 1;
            
            //etiqueta1.Text = "Jugador: " + currentPlayer;
        }

        static bool revisaGanador(int row, int col)
        {
            int compara = tabla[row, col];
            bool res;

            // Comprobar horizontal
            for (int c = 0; c < 4; c++)
            {
                if (col - c >= 0 && col - c + 3 < 7)
                {
                    res = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (tabla[row, col - c + i] != compara)
                        {
                            res = false;
                            break;
                        }
                    }
                    if (res) return true;
                }
            }

            // Comprobar vertical
            for (int r = 0; r < 4; r++)
            {
                if (row - r >= 0 && row - r + 3 < 6)
                {
                    res = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (tabla[row - r + i, col] != compara)
                        {
                            res = false;
                            break;
                        }
                    }
                    if (res) return true;
                }
            }

            for (int d = -3; d <= 3; d++)
            {

                if (row + d >= 0 && row + d + 3 < 6 && col - d >= 0 && col - d + 3 < 7)
                {
                    bool win = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (tabla[row + d + i, col - d - i] != compara) // Restar i al segundo índice
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win) return true;
                }

                // Comprobar diagonal descendente (\)
                if (row - d >= 0 && row - d + 3 < 6 && col - d >= 0 && col - d + 3 < 7)
                {
                    bool win = true;
                    for (int i = 0; i < 4; i++)
                    {
                        if (tabla[row - d + i, col - d + i] != compara)
                        {
                            win = false;
                            break;
                        }
                    }
                    if (win) return true;
                }
            }
            return false;
        }

        static bool verificaLleno()
        {
            for (int col = 0; col < 7; col++)
            {
                if (tabla[0, col] == 0)
                    return false;
            }
            return true;
        }

        void iniciaArregloPaneles()
        {
            paneles = new Panel[6, 7];
            paneles[0, 0] = c00;
            paneles[0, 1] = c01;
            paneles[0, 2] = c02;
            paneles[0, 3] = c03;
            paneles[0, 4] = c04;
            paneles[0, 5] = c05;
            paneles[0, 6] = c06;
            paneles[1, 0] = c10;
            paneles[1, 1] = c11;
            paneles[1, 2] = c12;
            paneles[1, 3] = c13;
            paneles[1, 4] = c14;
            paneles[1, 5] = c15;
            paneles[1, 6] = c16;
            paneles[2, 0] = c20;
            paneles[2, 1] = c21;
            paneles[2, 2] = c22;
            paneles[2, 3] = c23;
            paneles[2, 4] = c24;
            paneles[2, 5] = c25;
            paneles[2, 6] = c26;
            paneles[3, 0] = c30;
            paneles[3, 1] = c31;
            paneles[3, 2] = c32;
            paneles[3, 3] = c33;
            paneles[3, 4] = c34;
            paneles[3, 5] = c35;
            paneles[3, 6] = c36;
            paneles[4, 0] = c40;
            paneles[4, 1] = c41;
            paneles[4, 2] = c42;
            paneles[4, 3] = c43;
            paneles[4, 4] = c44;
            paneles[4, 5] = c45;
            paneles[4, 6] = c46;
            paneles[5, 0] = c50;
            paneles[5, 1] = c51;
            paneles[5, 2] = c52;
            paneles[5, 3] = c53;
            paneles[5, 4] = c54;
            paneles[5, 5] = c55;
            paneles[5, 6] = c56;

        }

        private void vaciaTablero()
        {
            for(int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    paneles[j,i].BackColor = SystemColors.ActiveCaption;
                }
            }
        }

        private void p0_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 0;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }

        }

        private void c51_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 1;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c52_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 2;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c53_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 3;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c54_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 4;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c55_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 5;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c56_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 6;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c40_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 0;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c41_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 1;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c42_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 2;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c43_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 3;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c44_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 4;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c45_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 5;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c46_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 6;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c30_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 0;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c31_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 1;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c32_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 2;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c33_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 3;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c34_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 4;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c35_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 5;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c36_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 6;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c20_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 0;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c21_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 1;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c22_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 2;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c23_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 3;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c24_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 4;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c25_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 5;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c26_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 6;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c10_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 0;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c11_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 1;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c12_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 2;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c13_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 3;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c14_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 4;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c15_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 5;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c16_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 6;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c00_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 0;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c01_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 1;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c02_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 2;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c03_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 3;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c04_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 4;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c05_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 5;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }

        private void c06_MouseClick(object sender, MouseEventArgs e)
        {
            if (juegoActivo)
            {
                columnaSeleccion = 6;
                accionJuego();
            }
            else
            {
                MessageBox.Show("Acción no valida. Espere");
            }
        }
    }
}
