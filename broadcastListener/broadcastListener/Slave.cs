using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace broadcastListener
{
    public class Slave : IComparable {
        public static List<Slave> all;
        public static Slave self = null;
        private static Dictionary<ulong, Slave> ensureUniqueID;
        public static UdpClient senderToSlave;
        public static IPEndPoint broadcastToSlaveEP;

        [JsonIgnore] public bool isMaster { get { return Slave.self == Master.currentMaster; } }
        public string ip_string;
        public ulong id;
        public bool isSelf;
        //public int receivePort, sendPort;
        /*
        public Slave(SlaveArgJson arg){
            this.id = arg.id;
            this.ip = Program.stringToIP(this.ip_string = arg.adress);
            this.coInitializer();
        }*/
        public static List<Slave> staticInit() {
            //todo: in hotRestart chiama esplicitamente tutti gli staticInit e aggiungi clausole per chiudere i client già aperti sovrascrivendoli.
            //cleanup for HotRestart
            if (Slave.senderToSlave != null) { senderToSlave.Close(); }
            if (Slave.ensureUniqueID != null) {
                ensureUniqueID.Clear();
                lock (Slave.all) foreach (Slave s in Slave.all) Slave.Remove(s);
            }
            //real initialization
            Slave.senderToSlave = new UdpClient() { EnableBroadcast = true };
            Slave.senderToSlave.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);//nagle's algorithm should work only in tpc, but i want to be sure that it will not execute, that would be a problem.
            Slave.senderToSlave.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoChecksum, false);
            Slave.senderToSlave.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Slave.senderToSlave.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
            Slave.senderToSlave.ExclusiveAddressUse = false;
            Slave.senderToSlave.EnableBroadcast = true;
            Slave.all = new List<Slave>();
            Slave.ensureUniqueID = new Dictionary<ulong, Slave>();
            return Slave.all;
        }
        [JsonConstructor] public Slave() { }
        public static Slave getFromID(ulong id) {
            lock (Slave.all) {
                foreach (Slave s in Slave.all) if (s.id == id) return s;
            }
            return null;
        }
        public Slave coInitializer() {
            lock (Slave.all) {
                if (Slave.alreadyRegistered(this)) return Slave.getFromID(this.id);
                Slave.all.Add(this);
                if (Slave.ensureUniqueID.ContainsKey(this.id)) Program.pex("Argument ERROR: found 2 slaves with same id ( " + this.id + " ).");
                else Slave.ensureUniqueID.Add(this.id, this);
            }
            if (this.isSelf) {
                if (Slave.self != null) Program.pex("Argument ERROR: only one Slave must be marked as \"self\" ");
                Slave.self = this;
                if (Program.args.dinamicallyStarted) { DinamicallyStart(); }
            }
            return this;
        }


        public static void sendToAll(myMessage m) {
            byte[] data = m.toByteArray();
            senderToSlave.Send(data, data.Length, Slave.broadcastToSlaveEP);
            //Program.pe("Exception send???");
            //throw new Exception("");
        }
        internal static Slave[] sortByMasterPrio() {
            List<Slave> tmp = new List<Slave>(Slave.all);
            tmp.Sort((a, b) => a.CompareTo(b));
            return tmp.ToArray();
        }
        override public int GetHashCode(){ return this.id.GetHashCode(); }
        override public bool Equals(object s) { return s is Slave && this.id == ((Slave)s).id; }
        public int CompareTo(object S) {
            if (!(S is Slave)) { Program.pe("Wrong comparison usage: Slave with " + S.GetType().ToString() + ";"); return -1; }
            Slave s = (Slave)S;
            return this.id > s.id ? 1 : (this.id == s.id ? 0 : -1);
        }
        public static bool alreadyRegistered(Slave s) { return null != getFromID(s.id); }

        public static void Remove(Slave s){
            lock (Slave.all) { Slave.all.Remove(s); Slave.ensureUniqueID.Remove(s.id); }
        }

        /// <summary>
        /// dinamically add itself to slave list of all colleagues in the same partition.
        /// the list of colleagues umst be known before the start and passed as parameter.
        /// </summary>
        public void DinamicallyStart() { new myMessage(MessageType.dinamicallyAddSlave, Slave.self.serialize()).launchToOutput(); }
        
        public override string ToString() { return this.serialize(); }
        public string serialize() { return JsonConvert.SerializeObject(this); }
        public static Slave deserializeOrGet(string str) {
            Slave tmp = null;
            try { tmp = ((Slave)JsonConvert.DeserializeObject(str)).coInitializer(); }
            catch(Exception e) { Program.pe("Failed to deserialize Slave object: " + str, e); }
            if (tmp == null) return null;
            return tmp;
        }

        
    }
}
