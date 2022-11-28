using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;


namespace quizserver
{
    public partial class Form1 : Form
    {
        public static readonly object locked = new object();


        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //List<Socket> clientSockets = new List<Socket>();

        List<String> questions = new List<String>();
        List<int> answers = new List<int>();

        bool terminating = false;
        bool listening = false;
        bool canbeplayer = true;
        int noquestion;
        bool gameFinished = false;

        static Barrier barrier = new Barrier(2, x => Console.WriteLine("Both threads have come to the end.\n"));


        struct client
        {
            public Socket client_socket;
            public String client_name;
        }


        struct player
        {
            public String name;
            public List<double> answers;
            public int score;

        }

        struct serverAnswerItem
        {
            public player player1;
            public player player2;

        }

        List<client> clientList = new List<client>(); //clients with socket and name
        List<player> playerList = new List<player>(); //users with name, answer and score

        List<serverAnswerItem> serverAnswerList = new List<serverAnswerItem>(); //the list to keep the answers of users

        // List<serverAnswerItem> serverAnswerList = new List<serverAnswerItem>(); 

        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            this.FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            InitializeComponent();
        }



        private void button_listen_Click(object sender, EventArgs e)
        {
            //TO DO: TRY CATCH EKLENSIN BURAYA DIREKT IF YERINE

            noquestion = Int32.Parse(textbox_noquestion.Text); //number of questions


            //*****QUESTION AND ANSWER READING FROM FILE

            var readlines = File.ReadLines("questions.txt");
            int counter = 0;
            foreach (var line in readlines)
            {
                if (counter % 2 == 0) //questions from file
                {
                    questions.Add(line);

                }
                else //answers from file
                {
                    int answertemp = Int32.Parse(line);
                    answers.Add(answertemp);
                }
                counter++;
            }

            //*****


            int serverPort;

            if (Int32.TryParse(textbox_port.Text, out serverPort))
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, serverPort);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(2); //listen two clients for now

                listening = true;
                button_listen.Enabled = false;


                Thread acceptThread = new Thread(Accept);
                acceptThread.Start();

                control_panel.AppendText("Started listening on port: " + serverPort + "\n");

            }
            else
            {
                control_panel.AppendText("Please check port number \n");
            }

        }

        private void Accept()
        {
            while (listening)
            {
                try
                {
                    //create client with its socket
                    client newClient;
                    newClient.client_socket = serverSocket.Accept();

                    //take its name
                    Byte[] nameBuffer = new Byte[64];
                    newClient.client_socket.Receive(nameBuffer);

                    //the name sent by client
                    string incomingName = Encoding.Default.GetString(nameBuffer);
                    incomingName = incomingName.Substring(0, incomingName.IndexOf('\0'));


                    //check here if the name is already in clients
                    foreach (client comer in clientList)
                    {
                        if (comer.client_name.Equals(incomingName))
                        {
                            canbeplayer = false;
                            break;
                        }
                    }

                    //the client added to list, port and name is known
                    newClient.client_name = incomingName;
                    clientList.Add(newClient);

                    //if not a existing client, create new player
                    if (canbeplayer == true)
                    {
                        control_panel.AppendText("Player " + newClient.client_name + " connected.\n");
                        player newPlayer;
                        newPlayer.name = newClient.client_name;
                        newPlayer.score = 0;
                        newPlayer.answers = new List<double>();

                        playerList.Add(newPlayer);

                        //successfully start thread with client
                        Thread receiveThread = new Thread(() => Receive(newClient));
                        receiveThread.Start();



                    }
                    else
                    {
                        control_panel.AppendText("Player " + newClient.client_name + " already exists.\n");
                        newClient.client_socket.Close();
                        clientList.Remove(newClient);
                    }



                }
                catch
                {
                    if (terminating)
                    {
                        listening = false;
                    }
                    else
                    {
                        control_panel.AppendText("The socket stopped working.\n");
                    }

                }
            }
        }


        private void Receive(client newClient)
        {

            //client run this receive function right now, we have name and socket

            client newComer = clientList[clientList.Count() - 1];

            bool connected = true;
            int appended = 0, gameRound = 0;
            //send message to inform game starts
            if (playerList.Count == 2)
            {
                Byte[] gameStart = new Byte[64];
                gameStart = Encoding.Default.GetBytes("Server: Welcome to the game. We are starting...\n");
                control_panel.AppendText("Server: Welcome to the game. We are starting...\n");

            }

            while (connected)
            {
                try
                {

                    //if 2 player is joined
                    //ask questions
                    //check if everyone answered
                    //check answers
                    //announce winner
                    //list the score table


                    if (playerList.Count == 2)
                    {
                        lock (locked)
                        {
                            Byte[] gameStart = new Byte[64];
                            gameStart = Encoding.Default.GetBytes("Server: Welcome to the game. We are starting...\n");
                            newClient.client_socket.Send(gameStart);
                        }

                        //while # of questions are finished
                        for (int i = 0; i < noquestion; i++)
                        {
                            int answeredNum = 0;
                            Byte[] qBuffer = new Byte[64];
                            qBuffer = Encoding.Default.GetBytes(questions[i % (questions.Count())] + "\n");
                            newClient.client_socket.Send(qBuffer);
                            lock (locked)
                            {
                                if (appended == gameRound)
                                {
                                    control_panel.AppendText("Server: " + questions[i % (questions.Count())] + "\n");
                                    Interlocked.Increment(ref appended);
                                }
                            }
                            Byte[] aBuffer = new Byte[64];
                            newClient.client_socket.Receive(aBuffer);
                            String incomingAnswer = Encoding.Default.GetString(aBuffer);
                            incomingAnswer = incomingAnswer.Substring(0, incomingAnswer.IndexOf('\0'));
                            double playerAnswer = Convert.ToDouble(incomingAnswer);

                            foreach (player plyr in playerList)
                            {
                                if (newClient.client_name == plyr.name)
                                {
                                    plyr.answers.Add(playerAnswer);
                                    answeredNum++;
                                }
                            }
                            barrier.SignalAndWait();

                            lock (locked)
                            {
                                double player1guess = Math.Abs(answers[i] - playerList[0].answers[i]);
                                double player2guess = Math.Abs(answers[i] - playerList[1].answers[i]);

                                if (player1guess < player2guess)
                                {
                                    int newScore = playerList[0].score + 1;
                                    var temp = playerList[0];
                                    temp.score = newScore;
                                    playerList[0] = temp;
                                    control_panel.AppendText("Server: Player named " + playerList[0].name + " earned the point for Question " + (i + 1) + "!\n");

                                    // Sending the winner information to the player
                                    Byte[] winnerBuffer = new Byte[64];
                                    winnerBuffer = Encoding.Default.GetBytes("Player named " + playerList[0].name + " earned the point for Question " + (i + 1) + "!\n");
                                    newClient.client_socket.Send(winnerBuffer);

                                }
                                else
                                {
                                    int newScore = playerList[1].score + 1;
                                    var temp = playerList[1];
                                    temp.score = newScore;
                                    playerList[1] = temp;
                                    control_panel.AppendText("Server: Player named " + playerList[1].name + " earned the point for Question " + (i + 1) + "!\n");

                                    // Sending the winner information to the player
                                    Byte[] winnerBuffer = new Byte[64];
                                    winnerBuffer = Encoding.Default.GetBytes("Player named " + playerList[1].name + " earned the point for Question " + (i + 1) + "!\n");
                                    newClient.client_socket.Send(winnerBuffer);
                                }

                                // Score table to see the results in general
                                scoreTable();
                            }
                            Interlocked.Decrement(ref appended);
                            barrier.SignalAndWait();

                        }

                    }

                }
                catch
                {
                    if (!terminating)
                    {
                        control_panel.AppendText("A client has disconnected\n");
                        //inform the other player about disconnection
                        //make it winner
                    }
                    newClient.client_socket.Close();
                    clientList.Remove(newClient);
                    connected = false;
                }
            }
        }

        private void scoreTable()
        {
            control_panel.AppendText("-------------------------\n");
            control_panel.AppendText("SCORE TABLE:\n");

            if (playerList[0].score > playerList[1].score)
            {
                control_panel.AppendText("1. " + playerList[0].name + ": " + playerList[0].score + " points\n");
                control_panel.AppendText("2. " + playerList[1].name + ": " + playerList[1].score + " points\n");
            }
            else if (playerList[1].score > playerList[0].score)
            {
                control_panel.AppendText("1. " + playerList[1].name + ": " + playerList[1].score + " points\n");
                control_panel.AppendText("2. " + playerList[0].name + ": " + playerList[0].score + " points\n");
            }
            control_panel.AppendText("-------------------------\n");
        }

        private void checkWinner()
        {

        }

        private void listDescending()
        {

        }

        //if exit button is clicked
        private void Form1_FormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            listening = false;
            terminating = true;
            Environment.Exit(0);
        }
    }
}