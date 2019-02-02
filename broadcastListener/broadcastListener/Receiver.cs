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
    public abstract class Receiver{
        public static IPAddress receiveAddress = IPAddress.Any; // IPAddress.Parse("192.168.1.255");
        internal UdpClient udpClient;
        internal IPEndPoint endpoint;
        internal Thread myThread;
        internal volatile int myindex = 0;

        private DateTime startTime = DateTime.Now;
        private int discarded = 0, accepted = 0;
        private DateTime messageStart;
        private TimeSpan minMessageTime, maxMessageTime, totMessageTime;

        public static StartupArgJson args { get { return Program.args; } }

        /// <summary>
        /// Avvia la ricezione di questo ricevitore.
        /// </summary>
        public abstract void start();

        /// <summary>
        /// Interrompe la ricezione di questo ricevitore.
        /// </summary>
        public abstract void stop(object stopReason);
        public virtual void receiveBroadcast(object unused){
            receiveBroadcast0(unused);
            //try { receiveBroadcast0(unused); } catch (Exception e) { if (e is ThreadAbortException) return; string s = "exception in ReceiveBroadcast()" + e.ToString(); Program.pe(s); MessageBox.Show(s); }
        }
        public virtual void receiveBroadcast0(object unused) {
            //Receiver r = (Receiver)arg;
            double averageMessageSize = this is ReceiverTool ? ReceiverTool.averageMessageSize : SlaveReceiver.averageMessageSize;
            myMessage msg;
            byte[] data = null;
            try {
                //con 2500 robot nel simulatore sono arrivato a 78 messaggi al secondo processati, con numeri superiori lo reggo svuotando la coda ma solo 15 al secondo, perchè?
                Program.p(Thread.CurrentThread.Name+ " ready");
                bool warningEmitted = false;
                //bool debugBinary = false;
                int rcv = 0;
                //string uselessdebug = Program.pes;
                //string uselessdeb2 = Program.ps;
                int ciclo = 1000;
                //List<myMessage> debugAllProduced = new List<myMessage>();
                this.startTime = DateTime.Now;
                this.accepted = this.discarded = 0;
                this.messageStart = DateTime.MinValue;
                this.minMessageTime = TimeSpan.MaxValue;
                this.maxMessageTime = TimeSpan.MinValue;
                this.totMessageTime = new TimeSpan(0);
                Thread me = Thread.CurrentThread;
                while (true) {
                    //if (ciclo-- == 0) { ciclo = 1000; Thread.Sleep(1000000); throw new Exception("work done"); }
                    //data = Program.fakereceive();
                    if (Program.args.benchmark){
                        if (this.messageStart != DateTime.MinValue){
                            TimeSpan time = DateTime.Now - this.messageStart;
                            this.totMessageTime += time;
                            if (time > this.maxMessageTime) { this.maxMessageTime = time; }
                            if (time < this.minMessageTime) { this.minMessageTime = time; } }
                    }
                    try {
                        //data = Program.fakereceive(); Thread.Sleep(500);
                        data = this.udpClient.Receive(ref this.endpoint);
                    } catch (SocketException ex) {/*likely thread aborted*/; continue; }
                    if (Program.args.benchmark) { this.messageStart = DateTime.Now; }
                    msg = new myMessage(data);
                    //if (!data.arrayToString().Equals("FakeReceive")) throw new Exception("Wrong decoding");
                    if (Program.args.benchmark) {
                        if (msg.type == MessageType.xml_NotOFMyPartition) { discarded++; this.messageStart = DateTime.MinValue; }
                        else { accepted++; } }
                    if (msg.type == MessageType.xml_NotOFMyPartition) { continue; }
                    if (0.90 <= this.udpClient.Available / (ushort.MaxValue )) Program.pe("Questo ricevente è sovraccarico al " + ((int)(10000 * (this.udpClient.Available / (ushort.MaxValue )))) / 100 + "%, la perdita pacchetti è imminente e il programma non può funzionare correttamente in queste condizioni." + Environment.NewLine + "Si prega di ridurre il carico su questo ricevente inserendone altri nella rete e ripartizionando."+Environment.NewLine+"byte pending:" + udpClient.Available + "; pacchetti:" + udpClient.Available / averageMessageSize);
                    if (!warningEmitted && this.udpClient.Available >= (ushort.MaxValue - averageMessageSize * 2) / 10) {
                        warningEmitted = true; Program.p("warning sovraccarico al "+ ((int)(10000 * (this.udpClient.Available / (ushort.MaxValue - averageMessageSize)))) / 100 + "%. byte pending:" + udpClient.Available + "; pacchetti:" + udpClient.Available / averageMessageSize); }
                    
                    if (rcv++ % 20 == 0) {
                        Program.p(Thread.CurrentThread.Name+") receive pending message:" + (this.udpClient.Available / averageMessageSize) 
                            + "; pending byte:" + this.udpClient.Available 
                            + "; load:" + ((int)(10000*(this.udpClient.Available / (ushort.MaxValue ))))/100+"%"); }
                    
                    //debugAllProduced.Add(msg);
                    if (msg.type == MessageType.xml) {
                        ReceiverTool.messageQueue.Add(msg);
                        //if (ReceiverTool.messageQueue.Count == 0) throw new Exception();
                        Master.canPublish.Release(1);
                        if (Program.args.logToolMsgOnReceive == true) { Program.LogToolMsg(me.Name+"Rcv: " + msg); } }
                    else {
                        SlaveReceiver.SlaveMessageQueue.Add(msg);
                        //if (SlaveReceiver.SlaveMessageQueue.Count == 0) throw new Exception();
                        SlaveMsgConsumer.canConsume.Release(1);
                        //Program.logSlave("Rcv: " + msg); 
                    }
                
                    //StartupArgJson debug = Program.args;
                    //EndPoint ee = this.endpoint;
                    if (msg.type == MessageType.xml && (me.Name == null || me.Name[0] == 'I')) { Program.pe("Got xml data on the slave receiver (msg was sent on wrong broadacast port)"); continue; }
                    if (msg.type != MessageType.xml && (me.Name == null || me.Name[0] == 'T')) { Program.pe("Got non-xml data on the tool receiver (msg was sent on wrong broadacast port)"); continue; }
                    //string raw = Encoding.UTF8.GetString(data);
                    //if (debugBinary) { raw = "| "; foreach (byte b in data) { raw += b + " "; } raw += "|"; }
                    /*
                    myMessage message = new myMessage(raw);
                    message.consume(raw);
                    Master.canPublish.Release();
                    if (Master.iAmTheMaster){
                        Master.canSendMessage.Release();
                        Master.emptyMessageBuffer();
                        //send from buffer attivando un altro thread
                    }*/
                }
            }
            catch (ThreadAbortException e) {
                Program.p("Thread receiver ("+this.GetType()+") aborted; Last data handled:"+ (data == null ? "none" : data.arrayToString()), e);
                if (!Program.args.benchmark) return;
                string s = getstatistics();
                MessageBox.Show(s);
                Program.p(s);
                return;
            }
        }
        public string getstatistics() {
                DateTime stopTime = DateTime.Now;
                TimeSpan elapsed = stopTime - startTime;
            string s;
            if (accepted == 0) { s = Thread.CurrentThread.Name + " has received 0 messages, cannot compute statistics."; }
            s =     Thread.CurrentThread.Name + " Performance data: time for fully handling a single message (excluding discarded)." + Environment.NewLine +
                    "Min time:" + this.minMessageTime + Environment.NewLine +
                    "Avg time:" + new TimeSpan((long)(this.totMessageTime.Ticks / (double)accepted)) + " (messageTime/accepted = " + this.totMessageTime + "/" + accepted + ")" + Environment.NewLine +
                    "Max time:" + this.maxMessageTime + Environment.NewLine + Environment.NewLine +
                    "Throughput, including time spent waiting messages:" + ((double)accepted / elapsed.TotalSeconds) + "msg/sec. (accepted/seconds_Elapsed = " + accepted + "/" + elapsed.TotalSeconds + ")" + Environment.NewLine +
                    "";
            return s;
        }
    }
}
