using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace broadcastListener
{
    /*class receiverbackup
    {using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace a
{
    class Receiver
    {
        public static StartupArgJson args { get { return Program.args; } }
        public static UdpClient receiver;
        public static Dictionary<string, myMessage> msgBuffer;
        public static SortedDictionary<DateTime, myMessage> msgBuffer_Sorted;
        public static IPAddress self;
        public static Thread broadcastReceiver;
        //public static StartupArgJson parameters;
        public static Semaphore canPublish;
        public static Thread toolReceiverThread;
        public static int hash(byte[] msg) {
            int skip = 50, lengthMax = 100;
            int length = Math.Min(msg.Length, lengthMax);
            //nb: message length is always about 970 byte
            byte b = (byte)(msg.Length % 255);
            while (length-- > skip) { b ^= msg[length]; }
            return b % args.partitionNumbers_Total;
        }
        public static void receiveFromBroadcast()
        {
            receiver = new UdpClient();
            receiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            receiver.Client.Bind(new IPEndPoint(IPAddress.Any, Program.broadcastReceivePort));
            IPEndPoint broadcast = new IPEndPoint(IPAddress.Broadcast, Program.broadcastReceivePort);
            byte[] buffer;// = new byte[receiver.Client.ReceiveBufferSize];
            //Master.canSendMessage = new Semaphore(1,1);
            Master.canPublish = new Semaphore(1, int.MaxValue);
            bool debugBinary = false;
            while (true)
            {
                //blocking receive. by design.
                if (receiver.Available > 10) throw new Exception("Debug: overloading");
                buffer = receiver.Receive(ref broadcast);
                if (hash(buffer) != args.myPartitionNumber) continue;
                // todo: menziona che puoi barare sui partition number per creare slave connessi a più master (intended feature)
                // es: 16 vere partizioni, ad una replica dici che ce ne sono 8 e che lui è il 5°, lui diventerà replica del 10° e dell'11°.
                // todo: il master da pingare deve diventare un array e deve cambiare argomenti e ri-partizionarsi, sennò maneggia anche i messaggi del master ancora vivo, creando duplicati.
                //MessageBox.Show("Received!");
                //throw new Exception();
                //todo: che encoding??
                string raw;
                if(debugBinary){
                    raw = "| ";
                    foreach (byte b in buffer)
                    {
                        raw += b + " ";
                    }
                    raw += "|";
                }
                else raw = Encoding.UTF8.GetString(buffer);
                GUI.textBoxReceived_Append(raw);
                
                myMessage message = new myMessage(raw);
                message.consume(raw);
                Master.canPublish.Release();
                if (Master.iAmTheMaster) {
                    Master.canSendMessage.Release();
                    Master.emptyMessageBuffer();
                    //send from buffer attivando un altro thread
                }
            }
        }

        public void StartListeningAsynchronous()
        {
            Receiver.receiver.BeginReceive(ReceiveAsinchronous, new object());

        }
        private void ReceiveAsinchronous(IAsyncResult ar)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Program.broadcastReceivePort);
            byte[] bytes = receiver.EndReceive(ar, ref ip);
            string message = Encoding.ASCII.GetString(bytes);
            StartListeningAsynchronous(); // sure about that??
        }
    }
}

    }*/
}
