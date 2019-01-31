using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace broadcastListener
{
    class SlaveReceiver : Receiver{
        public static List<SlaveReceiver> all;//todo: nessuno viene mai inserito in SlaveReceiver.all, nè in toolreceiver.all
        public static MessageQueueI SlaveMessageQueue;
        public const double averageMessageSize = 970.0;
        private static List<UdpClient> udpClients;
        private static List<Thread> threads;
        //private UdpClient udpSender { };
        public static List<SlaveReceiver> staticInit()
        {
            //IPEP_SlaveBroadcast_Receive = new IPEndPoint(IPAddress.Broadcast, Program.args.broadcastPort_Slaves);
            //IPEP_SlaveBroadcast_Send = new IPEndPoint(IPAddress.Broadcast, Program.args.broadcastPort_Slaves);
            SlaveReceiver.all = new List<SlaveReceiver>();
            SlaveReceiver.udpClients = new List<UdpClient>();
            SlaveReceiver.threads = new List<Thread>();
            SlaveReceiver.SlaveMessageQueue = new MessageQueue();
            return SlaveReceiver.all;
        }
        /// <summary>
        /// Inizializza un Thread addetto a ricevere i segnali dalle repliche o dal master.
        /// </summary>
        public SlaveReceiver(IPEndPoint endpoint = null, int maxThread = -1) {
            if (endpoint == null) endpoint = new IPEndPoint(Receiver.receiveAddress, args.broadcastPort_Slaves);
            if (maxThread == -1) maxThread = args.slaveReceiverThreads;
            this.endpoint = endpoint;
            myindex = SlaveReceiver.all.Count;
            myThread = new Thread(new ParameterizedThreadStart(receiveBroadcast));
            SlaveReceiver.all.Add(this);
            SlaveReceiver.threads.Add(myThread);
            if (Program.args.exclusiveBind == true) {
                SlaveReceiver.udpClients.Add(udpClient = new UdpClient(this.endpoint));
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, false);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, true);
            }
            else {
                SlaveReceiver.udpClients.Add(udpClient = new UdpClient());
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
                udpClient.ExclusiveAddressUse = false;
                udpClient.Client.Bind(this.endpoint); }
            
        }



        public static void RemoveOne(){//todo: esportalo pure su ToolReceiver

            lock (SlaveReceiver.all) { //todo: ci sono accessi non con lock su slavereceiver.all o sulle sue componenti?
                SlaveReceiver s;
                if (SlaveReceiver.all.Count == 0) return;
                s = SlaveReceiver.all[0];
                if (s.myThread != null) s.myThread.Abort();//todo: controlla se quando efrmi il thread lo rimetti semrpe a null. non voglio che il thread sia non-null ma non in esecuzione.
                s.udpClient.Close();
                //s.udpSender.Close();//todo: serve a qqualcosa? viene usato?

                SlaveReceiver.all.RemoveAt(0);
                for (int i = 1; i < SlaveReceiver.all.Count; i++) { SlaveReceiver.all[i].myindex = i; }
            }
        }
        /// <summary>
        /// Avvia la ricezione di tutti i ricevitori.
        /// </summary>
        public static void Start() { foreach (SlaveReceiver rec in SlaveReceiver.all) rec.start(); return; }

        /// <summary>
        /// Interrompe la ricezione di tutti i ricevitori.
        /// </summary>
        public static void Stop(object stopReason) { foreach (SlaveReceiver rec in SlaveReceiver.all) rec.stop(stopReason); return; }
        /// <summary>
        /// Avvia la ricezione di questo ricevitore.
        /// </summary>
        override public void start() {this.myThread.Start(); }
        /// <summary>
        /// Interrompe la ricezione di questo ricevitore.
        /// </summary>
        override public void stop(object stopReason) {
            SlaveReceiver.threads.Remove(this.myThread);
            SlaveReceiver.udpClients.Remove(this.udpClient);
            this.udpClient.Close();
            this.myThread.Abort(stopReason); }
    }
}
