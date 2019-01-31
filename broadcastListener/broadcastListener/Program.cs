using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;
using System.Text;
using System.IO;

namespace broadcastListener
{
    static class Program {
        public enum ExecutionMode { debug, production };
        public static StartupArgJson args;
        public static ExecutionMode mode = ExecutionMode.debug;

        public const int broadcastReceivePort = 20 * 1000 + 1;//unregistered until 20560
        public const int replication = 4;
        //private static IPEndPoint broadcastEP;


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread] private static void Main(string[] args_Raw) {
            if (Program.mode == Program.ExecutionMode.debug) { args_Raw = StartupArgJson.fakeinput(); }
            if (args_Raw.Length != 1) args_Raw = new string[] { "TriggerError Message: there must be only a single argument." };
            Program.args = StartupArgJson.deserialize(args_Raw[0]);
            if (Program.args == null || !Program.args.Validate()) return;
            Clipboard.SetText(args_Raw[0]);
            HotRestart();

        }
        public static void HotRestart() {
            IPAddress tmp = IPAddress.Parse(Program.args.broadcastAddress);
            Slave.broadcastToSlaveEP = new IPEndPoint(tmp, Program.args.broadcastPort_Slaves);
            Master.canPublish = new Semaphore(0, int.MaxValue);//todo: check
            //myKafka.kafkatest();
            //myKafka.send(new myMessage[]{new myMessage(MessageType.confirmMessageSuccess_Single, "ProducerA first test")});
            //return;

            ReceiverTool.staticInit();
            SlaveReceiver.staticInit();
            Slave.staticInit();
            myKafka.staticInit();
            SlaveMsgConsumer.staticInit();
            /*var Client = new UdpClient();
            var RequestData = Encoding.ASCII.GetBytes("SomeRequestData");
            var ServerEp = new IPEndPoint(IPAddress.Any, 0);

            Client.EnableBroadcast = true;
            Client.Send(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888));
            */
            //Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //sock.Poll(1,SelectMode.SelectWrite);
            //tmp = IPAddress.Broadcast;
            //Slave.senderToSlave.Client.Bind(Program.broadcastEP);
            foreach (Slave a in args.replicatorsList) { a.coInitializer(); }
            if (Slave.self == null) pex("Argument error: Exactly one slave must have \"isSelf = true\". It is needed to know what slave id this instance has.");
            new Master();
            while (ReceiverTool.all.Count < args.toolReceiverThreads) { new ReceiverTool(); }
            while (SlaveReceiver.all.Count < args.slaveReceiverThreads) { new SlaveReceiver(); }
            //only happening in case of hot restart for argument change: the receiver threads will never stop collecting.
            //NB: in caso di riduzione dei thread potrebbe verificarsi la perdita di un messaggio se il thread viene interrotto tra la ricezione e l'accodamento. fare un fermo sincronizzato però implicherebbe delle operazioni durante la normale esecuzione che rallenterebbero il throughput.
            while (ReceiverTool.all.Count > args.toolReceiverThreads) { ReceiverTool.RemoveOne(); }
            while (SlaveReceiver.all.Count > args.slaveReceiverThreads) { SlaveReceiver.RemoveOne(); }
            for (int i = 0; i < ReceiverTool.all.Count; i++) { ReceiverTool.all[i].myThread.Name = "Tool Broadcast Broadcast Receiver " + (i + 1) + "/" + ReceiverTool.all.Count; }
            for (int i = 0; i < SlaveReceiver.all.Count; i++) { SlaveReceiver.all[i].myThread.Name = "IntraCommunication Receiver " + (i + 1) + "/" + SlaveReceiver.all.Count; }
            new SlaveMsgConsumer().start();
            SlaveReceiver.Start();
            ReceiverTool.Start();
            //MessageBox.Show("Done");
            if (args.enableGUI && GUI.thiss == null) {
                Thread.CurrentThread.Name = "MainGuiThread";
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new GUI());
            }
        }

        public static IPAddress stringToIP(string ip) {
            string[] tmp = ip.Split('.');
            return new IPAddress(new byte[] { byte.Parse(tmp[0]), byte.Parse(tmp[1]), byte.Parse(tmp[2]), byte.Parse(tmp[3]) });
        }
        public static ulong GetMACAddress() {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces()) {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed && tempMac != null && tempMac.Length >= MIN_MAC_ADDR_LENGTH) {
                    byte[] mac = nic.GetPhysicalAddress().GetAddressBytes();
                    if (mac.Length != 6) { Program.pe("Mac adress too long? is it wrong? length:"+mac.Length); }
                    ulong ret = 0;
                    for (int i = 0; i < 8; i++) { ret |= (i < mac.Length ? ((ulong)mac[i] << (i) * 8) : 0); }
                    /*byte[] mac64 = new byte[8];
                    ulong ret = 0;
                    for(int i=0; i<mac64.Length; i++) { if (i < mac.Length) mac64[i] = mac[i]; else mac64[i] = 0; }
                    ret = BitConverter.ToUInt64(mac64, 0);
                    */
                    return ret;
                }
            }
            Program.pex("Failed to get mac adress, it was used to make unique identificators. please use a custom-defined identificator for this machine and manually ensure it is unique.");
            return 1;
        }

        delegate void delegateVoidStringException(string s, Exception e);
        public static FileStream criticalErrorFilef = null, ErrorFilef = null, LogFilef = null, ToolMsgFilef = null, SlaveMsgFilef = null;
        public static StreamWriter criticalErrorFile = null, ErrorFile = null, LogFile = null, ToolMsgFile = null, SlaveMsgFile = null;
        public static volatile int peLine = 0, pLine = 0, ptLine = 0, psLine = 0;
        public static volatile string ps = "", pes = "", pts = "", pss = "";
        public static string pexs{get{ return pes; }set { pes = value; } }
        public static int pexLine { get { return peLine; } set { peLine = value; } }
        /// <summary>
        /// log for critical unexpected events and exceptions. Something wrong and unrecoverable happened and the system cannot work anymore.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        public static void pex(string s, Exception e = null) {
            try {if (GUI.thiss != null && GUI.thiss.InvokeRequired) { GUI.thiss.Invoke(new delegateVoidStringException(pex), new object[] { s, e }); return; } } catch (Exception) { }
            s = (pexLine) + "*} " + s + (e == null ? "" : Environment.NewLine + pexLine + ") " + e.ToString()) + Environment.NewLine;
            pexLine++;
            lock (pexs) pexs += s;
            lock (Program.args.criticalErrFile) if (Program.args.criticalErrFile != null) {
                if (criticalErrorFile == null) {
                    criticalErrorFilef = File.Open(Program.args.criticalErrFile, FileMode.Append, FileAccess.Write);
                    criticalErrorFile = new StreamWriter(criticalErrorFilef);
                }
                //string[] tmp = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                //File.AppendAllLines(Program.args.criticalErrFile, tmp); }
                lock (criticalErrorFile) criticalErrorFile.Write(s);
            }
            try
            {
                if (GUI.thiss != null && GUI.thiss != null && !GUI.thiss.Disposing && !GUI.thiss.IsDisposed
                && GUI.thiss.richTextBoxErrors != null && !GUI.thiss.richTextBoxErrors.Disposing && !GUI.thiss.richTextBoxErrors.IsDisposed)
                {
                    lock (pexs) GUI.thiss.richTextBoxErrors.Text = pexs;
                }
                MessageBox.Show(s);
            }
            
            catch (Exception) { }
            
            GUI.GUI_FormClosing(null, null);
        }
        /// <summary>
        /// log for unexpected events and exceptions. The error happened is likely recoverable and the system should still be able to work.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        internal static void pe(string s, Exception e = null) {
            try { if (GUI.thiss != null && GUI.thiss.InvokeRequired) { GUI.thiss.Invoke(new delegateVoidStringException(pe), new object[] { s, e }); return; } } catch (Exception) { }
            s = (peLine) + ") " + s + (e == null ? "" : Environment.NewLine + peLine + ") " + e.ToString()) + Environment.NewLine;
            peLine++;
            lock (pes) pes += s;
            lock (Program.args.errFile) if (Program.args.errFile != null) {
                if (ErrorFile == null) {
                    ErrorFilef = File.Open(Program.args.errFile, FileMode.Append, FileAccess.Write);
                    ErrorFile = new StreamWriter(ErrorFilef);
                //string[] tmp = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                //File.AppendAllLines(Program.args.logFile, tmp);
                lock (ErrorFile) ErrorFile.Write(s);
                }
            }
            try
            {
                if (GUI.thiss != null && GUI.thiss != null && !GUI.thiss.Disposing && !GUI.thiss.IsDisposed
                && GUI.thiss.richTextBoxErrors != null && !GUI.thiss.richTextBoxErrors.Disposing && !GUI.thiss.richTextBoxErrors.IsDisposed)
                {
                    lock (pes) GUI.thiss.richTextBoxErrors.Text = pes;
                }
            }
            catch (Exception) { }
        }
        /// <summary>
        /// log for expected events and exceptions.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        internal static void p(string s, Exception e = null) {
            if (GUI.thiss != null && GUI.thiss.InvokeRequired && !GUI.thiss.IsDisposed && !GUI.thiss.Disposing) {
                try { GUI.thiss.Invoke(new delegateVoidStringException(p), new object[] { s, e }); return; } catch (Exception) { } }
            try
            {
                s = (pLine) + ") " + s + (e == null ? "" : Environment.NewLine + pLine + ") " + (e == null ? "" : e.ToString())) + Environment.NewLine;
                pLine++;
                lock (ps) ps += s;
                lock (Program.args.logFile) if (Program.args.logFile != null)
                    {
                        if (LogFile == null)
                        {
                            LogFilef = File.Open(Program.args.logFile, FileMode.Append, FileAccess.Write);
                            LogFile = new StreamWriter(LogFilef);
                        }
                        string[] tmp = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        lock (LogFile) LogFile.Write(s);
                        //File.AppendAllLines(Program.args.logFile, tmp);
                    }
                try
                {
                    if (GUI.thiss != null && !GUI.thiss.Disposing && !GUI.thiss.IsDisposed
                    && GUI.thiss.textboxStatus != null && !GUI.thiss.textboxStatus.Disposing && !GUI.thiss.textboxStatus.IsDisposed)
                    {
                        lock (ps) GUI.thiss.textboxStatus.Text = ps;
                    }
                }
                catch (Exception) { }
            }
            catch (Exception ex) { MessageBox.Show(""+ex.ToString()); }
        }        
        
        /// <summary>
        /// log for expected events and exceptions.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        internal static void LogToolMsg(string s, Exception e = null) {
            if (GUI.thiss != null && GUI.thiss.InvokeRequired && !GUI.thiss.IsDisposed && !GUI.thiss.Disposing) {
                try { GUI.thiss.Invoke(new delegateVoidStringException(LogToolMsg), new object[] { s, null }); return; } catch (Exception) { } }
            try{
                s = (ptLine) + ") " + s + (e == null ? "" : Environment.NewLine + ptLine + ") " + (e == null ? "" : e.ToString())) + Environment.NewLine;
                ptLine++;
                lock (pts) pts += s;
                lock (Program.args.toolMsgFile) if (Program.args.toolMsgFile != null){
                        if (ToolMsgFile == null)
                        {
                            ToolMsgFilef = File.Open(Program.args.toolMsgFile, FileMode.Append, FileAccess.Write);
                            ToolMsgFile = new StreamWriter(ToolMsgFilef);
                        }
                        string[] tmp = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        lock (ToolMsgFile) ToolMsgFile.Write(s);
                        //File.AppendAllLines(Program.args.logFile, tmp);
                    }
                try
                {
                    if (GUI.thiss != null && !GUI.thiss.Disposing && !GUI.thiss.IsDisposed
                    && GUI.thiss.richTextBoxmsg != null && !GUI.thiss.richTextBoxmsg.Disposing && !GUI.thiss.richTextBoxmsg.IsDisposed)
                    {
                        lock (pts) GUI.thiss.richTextBoxmsg.Text = pts;
                    }
                }
                catch (Exception) { }
            }
            catch (Exception ex) { MessageBox.Show(""+ex.ToString()); }
        }        
        /// <summary>
        /// log for expected events and exceptions.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        internal static void logSlave(string s, Exception e = null) {
            if (GUI.thiss != null && GUI.thiss.InvokeRequired && !GUI.thiss.IsDisposed && !GUI.thiss.Disposing) {
                try { GUI.thiss.Invoke(new delegateVoidStringException(logSlave), new object[] { s, null }); return; } catch (Exception) { } }
            try{
                s = (psLine) + ") " + s + (e == null ? "" : Environment.NewLine + psLine + ") " + (e == null ? "" : e.ToString())) + Environment.NewLine;
                psLine++;
                lock (pss) pss += s;
                lock (Program.args.slaveMsgFile) if (Program.args.slaveMsgFile != null){
                        if (Program.SlaveMsgFile == null)
                        {
                            SlaveMsgFilef = File.Open(Program.args.slaveMsgFile, FileMode.Append, FileAccess.Write);
                            SlaveMsgFile = new StreamWriter(SlaveMsgFilef);
                        }
                        string[] tmp = s.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        lock (SlaveMsgFile) SlaveMsgFile.Write(s);
                        //File.AppendAllLines(Program.args.logFile, tmp);
                    }
                try
                {
                    if (GUI.thiss != null && !GUI.thiss.Disposing && !GUI.thiss.IsDisposed
                    && GUI.thiss.richTextBoxSlave != null && !GUI.thiss.richTextBoxSlave.Disposing && !GUI.thiss.richTextBoxSlave.IsDisposed)
                    {
                        lock (pss) GUI.thiss.richTextBoxSlave.Text = pss;
                    }
                }
                catch (Exception) { }
            }
            catch (Exception ex) { MessageBox.Show(""+ex.ToString()); }
        }
        public static IPAddress GetMyIPAddress()
        {
            IPHostEntry Host = default(IPHostEntry);
            string Hostname = System.Environment.MachineName;
            Host = Dns.GetHostEntry(Hostname);
            foreach (IPAddress IP in Host.AddressList)
            {
                if (IP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return IP;
                    //IPAddress = Convert.ToString(IP);
                }
            }
            return null;
        }
        static volatile int ordinal = 0;
        public static byte[] fakereceive() {
            string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"+Environment.NewLine+
"<InhibitEvent xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"+Environment.NewLine+
"    <Inserted>"+Environment.NewLine+
"        <equip_OID>0x111111111</equip_OID>n<recipe_OID>0x9V543792L9678465</recipe_OID>"+Environment.NewLine+
"        <step_OID>0x2ST329581T758465</step_OID>"+Environment.NewLine+
"        <hold_type>45-ProcessEquipHold</hold_type>"+Environment.NewLine+
"        <hold_flag>N</hold_flag>"+Environment.NewLine+
"        <event_datetime>2019-01-31T17:33:50.081</event_datetime>"+Environment.NewLine+
"    </Inserted>"+Environment.NewLine+
"    <Deleted>"+Environment.NewLine+
"        <equip_OID>0x111111111</equip_OID>n<recipe_OID>0x9V543792L9678465</recipe_OID>"+Environment.NewLine+
"        <step_OID>0x2ST329581T758465</step_OID>"+Environment.NewLine+
"        <hold_type>45-ProcessEquipHold</hold_type>"+Environment.NewLine+
"        <hold_flag>Y</hold_flag>"+Environment.NewLine+
"        <event_datetime>2019-01-31T17:33:50.081</event_datetime>"+Environment.NewLine+
"    </Deleted>"+Environment.NewLine+
"</InhibitEvent>";
            //xml = ((++ordinal) + " - fakemessage").stringToByteArr();
            return xml.stringToByteArr();
        }
    }

}
