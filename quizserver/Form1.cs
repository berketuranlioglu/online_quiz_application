using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection;


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
        bool gameStarted = false;

        static Barrier barrier = new Barrier(0, x => Console.WriteLine("Both threads have come to the end.\n"));


        struct client
        {
            public Socket client_socket;
            public String client_name;
        }


        struct player
        {
            public String name;
            public List<double> answers;
            public double score;

        }

        struct serverAnswerItem
        {
            public player player1;
            public player player2;

        }

        List<client> clientList = new List<client>(); //clients with socket and name
        List<player> playerList = new List<player>(); //users with name, answer and score

        List<serverAnswerItem> serverAnswerList = new List<serverAnswerItem>(); //the list to keep the answers of users

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
                serverSocket.Listen(-1); //listen numerous clients

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
                if (gameFinished == true)
                {
                    gameFinished = false;
                }

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

                        // it also increments the barrier
                        barrier.AddParticipant();

                        playerList.Add(newPlayer);

                        //successfully start thread with client
                        Thread receiveThread = new Thread(() => Receive(newClient));
                        receiveThread.Start();
                    }
                    else
                    {
                        Byte[] nameError = new Byte[64];
                        nameError = Encoding.Default.GetBytes("Player " + newClient.client_name + " already exists.\n");
                        newClient.client_socket.Send(nameError);
                        control_panel.AppendText("Player " + newClient.client_name + " already exists.\n");
                        newClient.client_socket.Close();
                        clientList.Remove(newClient);
                        canbeplayer = true;
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


                    // Initial welcome to the players
                    if (gameStarted)
                    {
                        lock (locked)
                        {
                            Byte[] gameStart = new Byte[64];
                            gameStart = Encoding.Default.GetBytes("Welcome to the game. We are starting...\n");
                            newClient.client_socket.Send(gameStart);
                            if (newClient.client_name == playerList[0].name)
                            {
                                control_panel.AppendText("Server: Welcome to the game. We are starting...\n");
                            }
                        }

                        //while # of questions are finished
                        for (int i = 0; i < noquestion; i++)
                        {
                            // Sending the question
                            int answeredNum = 0;
                            Byte[] qBuffer = new Byte[64];
                            qBuffer = Encoding.Default.GetBytes(questions[i % questions.Count()] + "\n");
                            newClient.client_socket.Send(qBuffer);
                            if (newClient.client_name == playerList[0].name)
                            {
                                control_panel.AppendText("Server: " + questions[i % questions.Count()] + "\n");
                            }

                            // Receiving the answers
                            Byte[] aBuffer = new Byte[64];
                            newClient.client_socket.Receive(aBuffer);
                            String incomingAnswer = Encoding.Default.GetString(aBuffer);
                            incomingAnswer = incomingAnswer.Substring(0, incomingAnswer.IndexOf('\0'));
                            double playerAnswer = ConvertToDouble(incomingAnswer);
                            control_panel.AppendText(newClient.client_name + "'s answer is: " + incomingAnswer + "\n");

                            // Adding the answers to lists according to the players
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
                                // collect all the guesses
                                List<Double> guesses = new List<Double>();

                                for (int j = 0; j < playerList.Count(); j++)
                                    guesses.Add(Math.Abs(answers[i % answers.Count()] - playerList[j].answers[i]));

                                String statusString = "";
                                String status = "";

                                // find the closest answer
                                double closestGuess = guesses.Min();
                                List<int> closestPlayers = new List<int>();

                                // check if there are multiple closest players
                                for (int j = 0; j < guesses.Count(); j++)
                                    if (guesses[j] == closestGuess)
                                        closestPlayers.Add(j);

                                // if there is only one winner (i.e., no shared points)
                                if (closestPlayers.Count() == 1)
                                {
                                    // make the closest winner
                                    statusString = playerList[closestPlayers[0]].name + " got the point!";
                                    status = currentStatus(playerList, statusString, answers[i % answers.Count()], i);

                                    if (newClient.client_name == playerList[closestPlayers[0]].name)
                                    {
                                        double newScore = playerList[closestPlayers[0]].score + 1;
                                        var temp = playerList[closestPlayers[0]];
                                        temp.score = newScore;
                                        playerList[closestPlayers[0]] = temp;

                                        control_panel.AppendText(status);
                                    }

                                    // Sending the who-got-correct-answer information to the player
                                    Byte[] winnerBuffer = new Byte[64];
                                    winnerBuffer = Encoding.Default.GetBytes(status);
                                    newClient.client_socket.Send(winnerBuffer);
                                }
                                else // TODO: BOZUK
                                {
                                    statusString = "The point is shared among players!";
                                    status = currentStatus(playerList, statusString, answers[i % answers.Count()], i);

                                    if (newClient.client_name == playerList[0].name)
                                    {
                                        for (int j = 0; j < closestPlayers.Count(); j++)
                                        {
                                            double newScore = playerList[closestPlayers[j]].score + 0.5;
                                            var temp = playerList[closestPlayers[j]];
                                            temp.score = newScore;
                                            playerList[closestPlayers[j]] = temp;
                                        }

                                        control_panel.AppendText(status);
                                    }

                                    // Sending the tie information to the player
                                    Byte[] tieBuffer = new Byte[64];
                                    tieBuffer = Encoding.Default.GetBytes(status);
                                    newClient.client_socket.Send(tieBuffer);
                                }
                            }
                            barrier.SignalAndWait();

                            // Score table to see the results in general
                            string table = scoreTable(playerList);
                            if (newClient.client_name == playerList[0].name)
                            {
                                control_panel.AppendText(table);
                            }

                            // Sending the table to the players
                            Byte[] tableBuffer = new Byte[64];
                            tableBuffer = Encoding.Default.GetBytes("\n" + table);
                            newClient.client_socket.Send(tableBuffer);
                            barrier.SignalAndWait();

                        }

                        // Questions are finished, time to declare the winner
                        string winner = theWinner(playerList);
                        if (newClient.client_name == playerList[0].name)
                        {
                            control_panel.AppendText(winner);
                        }

                        Byte[] victoryBuffer = new Byte[64];
                        victoryBuffer = Encoding.Default.GetBytes(winner);
                        newClient.client_socket.Send(victoryBuffer);
                        barrier.SignalAndWait();

                        // Game is finished, all data must be reset
                        lock (locked)
                        {
                            if (newClient.client_name == playerList[0].name)
                            {
                                control_panel.AppendText("Players' scores and answers " +
                                            "are reset and game is ready for restart.\n");

                                for (int i = 0; i < playerList.Count; i++)
                                {
                                    playerList[i].answers.Clear();
                                    var temp = playerList[i];
                                    temp.score = 0;
                                    playerList[i] = temp;
                                }
                            }
                            gameFinished = true;

                            gameStarted = false;
                            button_start_game.Enabled = true;
                        }
                        barrier.SignalAndWait();
                    }

                }
                catch
                {
                    if (!terminating)
                    {
                        string disconnectMessage = "Player " + newClient.client_name + " has disconnected\n";
                        control_panel.AppendText(disconnectMessage);

                        lock (locked)
                        {
                            // if it will be the first disconnection
                            if (clientList.Count == 2)
                            {
                                // finding the other (not disconnected) client
                                client otherClient = clientList[0].client_name != newClient.client_name
                                    ? clientList[0] : clientList[1];
                                Byte[] victoryBuffer = new Byte[64];
                                disconnectMessage += "YOU ARE THE WINNER!\n";
                                victoryBuffer = Encoding.Default.GetBytes(disconnectMessage);
                                otherClient.client_socket.Send(victoryBuffer);
                                control_panel.AppendText("END OF THE GAME! THE WINNER IS: " + otherClient.client_name + "!\n");

                                for (int i = clientList.Count - 1; i >= 0; i--)
                                {
                                    // removing also from playerList
                                    if (playerList[i].name == newClient.client_name)
                                    {
                                        playerList.Remove(playerList[i]);
                                    }
                                    else
                                    {
                                        playerList[i].answers.Clear();
                                        var temp = playerList[i];
                                        temp.score = 0;
                                        playerList[i] = temp;
                                    }
                                }

                                clientList.Remove(newClient);
                            }
                        }
                    }



                    connected = false;
                    gameFinished = true;

                    gameStarted = false;
                    button_start_game.Enabled = true;
                }
            }
        }

        private void button_start_game_Click(object sender, EventArgs e)
        {
            if (playerList.Count >= 2)
            {
                control_panel.AppendText("ok.\n");
                gameStarted = true;
                button_start_game.Enabled = false;
            }
            else
            {
                control_panel.AppendText("There must be at least two players to start the game.\n");
            }
        }

        private string scoreTable(List<player> plyrList)
        {
            string table = "-------------------------\nSCORE TABLE:\n";

            // sort
            plyrList.Sort((s1, s2) => s1.score.CompareTo(s2.score));

            // print all players
            for (int i = plyrList.Count()-1; i >= 0; i--)
                table += (plyrList.Count()-i) + ". " + plyrList[i].name + ": " + plyrList[i].score + " points\n";

            table += "-------------------------\n";

            return table;
        }

        private string theWinner(List<player> plyrList)
        {
            // sort
            plyrList.Sort((s1, s2) => s1.score.CompareTo(s2.score));

            List<String> winners = new List<String>();

            // if other players' scores are also max
            for (int i = 0; i < plyrList.Count(); i++)
                if (plyrList[i].score == plyrList[plyrList.Count()-1].score)
                    winners.Add(plyrList[i].name);

            if (winners.Count() == 1)
                return ("END OF THE GAME! THE WINNER IS: " + winners[0] + "!\n");

            else
            {
                string w = "";

                for (int i = 0; i < winners.Count(); i++)
                    w += winners[i] + " ";

                return ("THERE IS A TIE, OUR WINNERS ARE: " + w + "!\n");
            }
        }

        private String currentStatus(List<player> plyrList, String status, double correctAnswer, int currentQuestion)
        {
            string currentTable = "\n-------------------------\nSTATUS TABLE:\n";

            foreach (player plyr in plyrList)
                currentTable += "Player " + plyr.name + "'s answer: " + plyr.answers[currentQuestion] + "\n";

            currentTable += "The correct answer: " + correctAnswer + "\n"
                            + "The status for this question: " + status + "\n";
            return currentTable;
        }

        private double ConvertToDouble(string s)
        {
            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            double result = 0;
            try
            {
                if (s != null)
                    if (!s.Contains(","))
                        result = double.Parse(s, CultureInfo.InvariantCulture);
                    else
                        result = Convert.ToDouble(s.Replace(".", systemSeparator.ToString()).Replace(",", systemSeparator.ToString()));
            }
            catch (Exception e)
            {
                try
                {
                    result = Convert.ToDouble(s);
                }
                catch
                {
                    try
                    {
                        result = Convert.ToDouble(s.Replace(",", ";").Replace(".", ",").Replace(";", "."));
                    }
                    catch
                    {
                        throw new Exception("Wrong string-to-double format");
                    }
                }
            }
            return result;
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