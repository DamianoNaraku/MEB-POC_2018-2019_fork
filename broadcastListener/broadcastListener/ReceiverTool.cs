using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace broadcastListener {

        /// <summary>
        /// Classe addetta all'ascolto dei messaggi inviati dai tool.
        /// </summary>
    class ReceiverTool : Receiver {
        public static List<ReceiverTool> all;
        //public static IPEndPoint IPEP_ToolBroadcast_Receive;
        public static MessageQueueI messageQueue;
        public const double averageMessageSize = 970.0;
        private static List<UdpClient> udpClients;
        private static List<Thread> threads;

        //su ogni rete (possono essere multiple o unica) possono esserci N listener che con un alg distrib si assegnano numeri progressivi unici
        //per ogni segnale ricevuto viene calcolato un hash [su (int32)(sender.roboid)%maxnum] che stabilisce quale listener dovrà processare il messaggio
        //se una dash non riceve un segnale da un listener manda una richiesta di resend al listener inadempiente. dopo 3 timeout  la dash invia in broadcast un segnale di ListenerDown con il numero del listener, tutti i listener lo ricevono e ricalcolano il loro numero e il maxnum di conseguenza modificando la funzione hash e assorbendo i robot scoperti, il modello attuale però fa mandare dati errati per una intera finestra temporale perchè un robot può essere spostato da un listener funzionante ad un altro listener funzionante e ogni listener ha solo una parte dei segnali di quel robot, quindi è tragico come un riavvio completo...
        //invece potrei tenere tutti i listnernum uguali e ri-hashare solo per quelli con listener down, ma questo implica altri problemi, tipo sull'aggiunta di un listener si ripete lo stesso problema.

        public static List<ReceiverTool> staticInit() {
            ReceiverTool.all = new List<ReceiverTool>();
            ReceiverTool.udpClients = new List<UdpClient>();
            ReceiverTool.threads = new List<Thread>();
            ReceiverTool.messageQueue = new MessageQueue();
            return ReceiverTool.all; }
        /// <summary>
        /// Inizializza un Thread addetto a ricevere i segnali dai tool.
        /// </summary>
        public ReceiverTool(IPEndPoint endpoint = null, int maxThread = -1){
            if (endpoint == null) endpoint = new IPEndPoint(Receiver.receiveAddress, args.broadcastPort_Tool);
            this.endpoint = endpoint;
            myindex = ReceiverTool.all.Count;
            myThread = new Thread(new ParameterizedThreadStart(receiveBroadcast));
            ReceiverTool.all.Add(this);
            ReceiverTool.threads.Add(myThread);
            // Il buffer di sistema è molto limitato e i messaggi ricevuti sono di ~1kb.
            // per evitare il rischio di perdere messaggi a causa del riempimento del buffer, il programma non deve solo avere un alto throughput, 
            // ma anche una bassa latenza di scheduling, altrimenti anche con una potenza di throughput infinita correrebbe il rischio di perderne.
            // i messaggi di intra-comunicazione sono molto più piccoli e rari e ci metteranno molto di più a riempire il buffer
            // inoltre la perdita di un messaggio di intra-comunicazione non è vitale e nella maggior parte dei casi non produce effetti collaterali gravi.
            myThread.Priority = ThreadPriority.Highest;
            if (Program.args.exclusiveBind == true) {
                try { ReceiverTool.udpClients.Add(udpClient = new UdpClient(this.endpoint)); }
                catch (Exception e) { Program.pe("Error while binding broadcast adress:port, this could be caused by multiple instance running with argument exclusiveBind = true; reason:", e); }
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
            }
            else {
                try{ReceiverTool.udpClients.Add(udpClient = new UdpClient());}
                catch (Exception e) { Program.pe("Error while binding broadcast adress:port, this could be caused by multiple instance running with argument exclusiveBind = true; reason:", e); }
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
                udpClient.ExclusiveAddressUse = false;
                udpClient.Client.Bind(this.endpoint); }
        }
        public static void RemoveOne() {
            lock (ReceiverTool.all) { //todo: ci sono accessi non con lock su ReceiverTool.all o sulle sue componenti?
                ReceiverTool s;
                if (ReceiverTool.all.Count == 0) return;
                s = ReceiverTool.all[0];
                if (s.myThread != null) s.myThread.Abort();//todo: controlla se quando efrmi il thread lo rimetti semrpe a null. non voglio che il thread sia non-null ma non in esecuzione.
                s.udpClient.Close();

                ReceiverTool.all.RemoveAt(0);
                for (int i = 1; i < ReceiverTool.all.Count; i++) { ReceiverTool.all[i].myindex = i; }
            }
        }
            /// <summary>
            /// Funzione utilizzata per misurare e confrontare le prestazioni dei diversi storage nella ricezione di messaggi.
            /// Utile anche nella release per dimensionare il numero di robot assegnabili a questo Listener in base alle sue prestazioni
            /// che sono dipendenti dalla macchina e non possono essere note a priori.
            /// La ricezione termina dopo aver accumulato abbastanza dati per una misurazione affidabile della performance media.
            /// </summary>
            public static void receiveudp_Benchmark()
            {/* todo:
                int ricevuti = 0;
                while (ricevuti == 0 || ricevuti != udpClient.Available && udpClient.Available < 65000)
                {
                    ricevuti = udpClient.Available;
                    Program.p("accumulo: " + ricevuti + " = "+averageMessageSize+" * " + ricevuti / averageMessageSize);
                    Thread.Sleep(2000);
                }

                int cicli = 0;
                Stopwatch sw = new Stopwatch();
                double receivetime, receivemedia = -1, processtime, processmedia = -1, insertdtime = -1, insertdmedia = -1, insertrtime = -1, insertrmedia = -1;
                double receivemin = 999999, receivemax = 0, processmin = 999999, processmax = 0, insertdmin = 999999, insertdmax = 0, insertrmin = 999999, insertrmax = 0;
                int ignoraprimi = 10;
                sw.Start();
                RobotMessage msg;
                if (ricevuti / 13 - 10 - 1 < 0) { MessageBox.Show("dati insufficienti per fare un test adeguato: " + (ricevuti / 13.0 - 1 - 10)); return; }
                while (cicli <= (ricevuti / 13 - 10 - 1))
                {
                    if (ignoraprimi-- == 0) { receivemedia = processmedia = insertdmedia = insertrmedia = -1; cicli = 0; }
                    sw.Restart();
                    byte[] data = udpClient.Receive(ref Listener.IPEP_RobotNet);
                    receivetime = sw.Elapsed.TotalMilliseconds;

                    sw.Restart();
                    if (null == (msg = RobotMessage.ProcessaMessaggio(data))) { MessageBox.Show("Discarded: " + System.Text.Encoding.UTF8.GetString(data) + ";"); return; }
                    processtime = sw.Elapsed.TotalMilliseconds;

                    sw.Restart();
                    Listener.storage.AddSignal(false, msg);
                    insertrtime = sw.Elapsed.TotalMilliseconds;

                    if (Listener.storageconfronto != null)
                    {
                        sw.Restart();
                        Listener.storageconfronto.AddSignal(false, msg);
                        insertdtime = sw.Elapsed.TotalMilliseconds;
                    }

                    receivemin = Math.Min(receivemin, receivetime); receivemax = Math.Max(receivemax, receivetime);
                    processmin = Math.Min(processmin, processtime); processmax = Math.Max(processmax, processtime);
                    insertrmin = Math.Min(insertrmin, insertrtime); insertrmax = Math.Max(insertrmax, insertrtime);
                    insertdmin = Math.Min(insertdmin, insertdtime); insertdmax = Math.Max(insertdmax, insertdtime);

                    receivemedia = (receivemedia * cicli + receivetime) / (cicli + 1);
                    processmedia = (processmedia * cicli + processtime) / (cicli + 1);
                    insertrmedia = (insertrmedia * cicli + insertrtime) / (cicli + 1);
                    insertdmedia = (insertdmedia * cicli + insertdtime) / (cicli + 1);

                    cicli++;
                    if (cicli % 100 == 0) { Listener.p("Rimanenti: " + udpClient.Available / 13); }
                }

                MessageBox.Show("Ricevuti:" + cicli + Environment.NewLine + " Tempi:{Lowest, Mid, Highest};" + Environment.NewLine +
                    " Receive:{" + receivemin + ", " + receivemedia + ", " + receivemax + "}" + Environment.NewLine +
                    " Process:{" + processmin + ", " + processmedia + ", " + processmax + "}" + Environment.NewLine +
                    " InsertR:{" + insertrmin + ", " + insertrmedia + ", " + insertrmax + "}" + Environment.NewLine +
                    " InsertD:{" + insertdmin + ", " + insertdmedia + ", " + insertdmax + "}");*/
            }

        /// <summary>
        /// Istruzioni eseguite dai Thread in <seealso cref="ReceiverTool.threads"/>
        /// per accodare i messaggi ricevuti dai tool nella coda <seealso cref="myMessage.queue"/>.
        /// </summary>



        /// <summary>
        /// Avvia la ricezione di tutti i ricevitori.
        /// </summary>
        public static void Start() { foreach (ReceiverTool rec in ReceiverTool.all) rec.start(); return; }

        /// <summary>
        /// Interrompe la ricezione di tutti i ricevitori.
        /// </summary>
        public static void Stop(object stopReason) { foreach (ReceiverTool rec in ReceiverTool.all) rec.stop(stopReason); return; }
        /// <summary>
        /// Avvia la ricezione di questo ricevitore.
        /// </summary>
        override public void start() {this.myThread.Start(); }
        /// <summary>
        /// Interrompe la ricezione di questo ricevitore.
        /// </summary>
        override public void stop(object stopReason) {
            ReceiverTool.threads.Remove(this.myThread);
            ReceiverTool.udpClients.Remove(this.udpClient);
            this.udpClient.Close();
            this.myThread.Abort(stopReason); }
    }
}
