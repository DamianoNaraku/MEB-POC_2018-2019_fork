using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using KafkaNet;
using KafkaNet.Model;
using System.Threading;

namespace broadcastListener
{
    public enum MessageType { dinamicallyAddSlave, dinamicallyRemoveSlave, confirmMessageSuccess_Single, xml, uninitialized, masterChange, confirmMessageSuccess_Batch, provideSlaveList, argumentChange, xml_NotOFMyPartition };
    public class myMessage : IComparable
    {
        public MessageType type;
        public string data;
        public static char separator = ';', secondSeparator = ',', keySeparator = '_';
        internal DateTime arrivalTime;
        public int arrivalNumber;
        
        //todo: controlla riferimenti alla key e togli proprietà
        public string key { get { return this.keyInternal; } set{ keyInternal = value; } }
        protected string keyInternal;
        public myMessage() { }
        public myMessage(byte[] raw){ init(raw.arrayToString()); }
        public myMessage(string raw) { init(raw); }
        private void init(string raw){
            arrivalTime = DateTime.Now;
            if ((key = tryMakeKey(raw)) == null){
                if (this.type == MessageType.xml_NotOFMyPartition) return;
                int pos = raw.IndexOf(myMessage.separator);
                if (pos == -1) { Program.pe("unrecognized message:" + raw); return; }
                string typestr = raw.Substring(0, pos);
                MessageType[] messageTypes = (MessageType[])Enum.GetValues(typeof(MessageType));
                this.type = MessageType.uninitialized;
                foreach (MessageType mt in messageTypes) {
                    if (typestr == mt.ToString()) { this.type = mt; break; }
                }
                if (this.type == MessageType.uninitialized){ Program.pe("Unknow message type:" + typestr + "; raw:" + raw); }
                data = raw.Substring(pos+1);
            }else {
                this.type = MessageType.xml;
                this.data = raw; }
        }
        public static Exception ex = new Exception();
        public static string err { get { throw ex; } set { throw ex; } }
        
        public static int hash(byte[] msg){ return hashSafe(msg); }

        public static int hashSafe(byte[] msg){
            int skip = 50, lengthMax = 100;
            int length = Math.Min(msg.Length, lengthMax);
            //nb: message length is always about 970 byte
            byte b = (byte)(msg.Length % 255);
            while (length-- > skip) { b ^= msg[length]; }
            return b % Program.args.partitionNumbers_Total; }
        public static int hashUnsafe(byte[] msg){//test if faster
            int skip = 50, lengthMax = 100;
            int length = (Math.Min(msg.Length, lengthMax) - skip*0) / sizeof(ulong);
            ulong val=0;
            unsafe {
                ulong* arr, limit;
                fixed (byte* tmp = msg) {
                    arr = ((ulong*)tmp) + skip;
                    limit = ((ulong*)tmp) + length;
                    //nb: message length is always about 970 byte
                    byte b = (byte)(msg.Length % 255);
                    while (arr < limit) { val^= *arr++; }//todo:System.AccessViolationException: 'Tentativo di lettura o scrittura della memoria protetta. Spesso questa condizione indica che altre parti della memoria sono danneggiate.'
                }
            }
            return (int)(val % (ulong) Program.args.partitionNumbers_Total); }
        public string tryMakeKey(string raw){
            string def = null;
            string equip, recipe, step, holdt, holdf, time;
            const string equipS = "<equip_OID", equipE = "</equip_OID>";
            const string recipeS = "<recipe_OID", recipeE = "</recipe_OID>";
            const string stepS = "<step_OID", stepE = "</step_OID>";
            const string holdtS = "<hold_type", holdtE = "</hold_type>";
            const string holdfS = "<hold_flag", holdfE = "</hold_flag>";
            const string timeS = "<event_datetime", timeE = "</event_datetime>";
            int equipStart, equipEnd, recipeStart, recipeEnd, stepStart, stepEnd, holdtStart, holdtEnd, holdfStart, holdfEnd, timeStart, timeEnd;
            if ((equipStart = raw.IndexOf(equipS, 0)) == -1) { return def; }
            else {
                equipStart += equipS.Length;
                if (raw[equipStart] == '>'){
                    equipStart++;
                    if ((equipEnd = raw.IndexOf(equipE, equipStart)) == -1) { Program.pe("unterminated Equip_OID found in: " + raw); return def; }
                    equip = raw.Substring(equipStart, equipEnd - equipStart); }
                else if (raw[equipStart] == '/') { equip = ""; equipEnd = equipStart; }
                else {Program.pe("MakeKey error"); return raw; } }
            
            if ((recipeStart = raw.IndexOf(recipeS, equipEnd)) == -1) { return def; }
            else {
                recipeStart += recipeS.Length;
                if (raw[recipeStart] == '>'){
                    recipeStart++;
                    if ((recipeEnd = raw.IndexOf(recipeE, recipeStart)) == -1) { Program.pe("unterminated recipe_OID found in: " + raw); return def; }
                    recipe = raw.Substring(recipeStart, recipeEnd - recipeStart); }
                else if (raw[recipeStart] == '/') { recipe = ""; recipeEnd = recipeStart; }
                else {Program.pe("MakeKey error"); return raw; } }
            if (myMessage.hash((equip + recipe).stringToByteArr()) != Program.args.myPartitionNumber) {
                this.type = MessageType.xml_NotOFMyPartition;
                return def; }
            if ((stepStart = raw.IndexOf(stepS, recipeEnd)) == -1) { return def; }
            else {
                stepStart += stepS.Length;
                if (raw[stepStart] == '>'){
                    stepStart++;
                    if ((stepEnd = raw.IndexOf(stepE, stepStart)) == -1) { Program.pe("unterminated step_OID found in: " + raw); return def; }
                    step = raw.Substring(stepStart, stepEnd - stepStart); }
                else if (raw[stepStart] == '/') { step = ""; stepEnd = stepStart; }
                else {Program.pe("MakeKey error"); return raw; } }

            if ((holdtStart = raw.IndexOf(holdtS, stepEnd)) == -1) { return def; }
            else {
                holdtStart += holdtS.Length;
                if (raw[holdtStart] == '>'){
                    holdtStart++;
                    if ((holdtEnd = raw.IndexOf(holdtE, holdtStart)) == -1) { Program.pe("unterminated holdtype found in: " + raw); return def; }
                    holdt = raw.Substring(holdtStart, holdtEnd - holdtStart); }
                else if (raw[holdtStart] == '/') { holdt = ""; holdtEnd = holdtStart; }
                else {Program.pe("MakeKey error"); return raw; } }

            if ((holdfStart = raw.IndexOf(holdfS, holdtEnd)) == -1) { return def; }
            else {
                holdfStart += holdfS.Length;
                if (raw[holdfStart] == '>'){
                    holdfStart++;
                    if ((holdfEnd = raw.IndexOf(holdfE, holdfStart)) == -1) { Program.pe("unterminated holdflag found in: " + raw); return def; }
                    holdf = raw.Substring(holdfStart, holdfEnd - holdfStart); }
                else if (raw[holdfStart] == '/') { holdf = ""; holdfEnd = holdfStart; }
                else {Program.pe("MakeKey error"); return raw; } }

            if ((timeStart = raw.IndexOf(timeS, holdfEnd)) == -1) { return def; }
            else {
                timeStart += timeS.Length;
                if (raw[timeStart] == '>'){
                    //string fragment = raw.Substring(timeStart);
                    timeStart++;
                    if ((timeEnd = raw.IndexOf(timeE, timeStart)) == -1) { Program.pe("unterminated timestamp found in: " + raw); return def; }
                    time = raw.Substring(timeStart, timeEnd - timeStart); }
                else if (raw[timeStart] == '/') { time = ""; timeEnd = timeStart; }
                else {Program.pe("MakeKey error"); return raw; } }

            const string separator = "_";
            return equip + separator + recipe + separator + step + separator + holdt + separator + holdf + separator + time; }
        internal string makeKafkaKey(){
            if (key == null) return null;
            string[] tokens = key.Split(myMessage.keySeparator);
            if (tokens == null || tokens.Length < 6) { Program.pe("MakeKafkaKey error! key:"+key); return key; };//return null;
            const int equip = 0, recipe = 1, step = 2, holdtype = 3, holdflag = 4, timestamp = 5;
            return tokens[equip] + myMessage.keySeparator + tokens[recipe] + myMessage.keySeparator + tokens[holdtype]; }

        public myMessage(MessageType type_forOutputMessages, string data) {
            this.key = null;
            this.data = data;
            this.type = type_forOutputMessages;}
        public void launchToOutput() {
            switch (this.type) {
                case MessageType.provideSlaveList:
                case MessageType.masterChange:
                case MessageType.dinamicallyAddSlave:
                case MessageType.dinamicallyRemoveSlave:
                case MessageType.confirmMessageSuccess_Batch:
                case MessageType.confirmMessageSuccess_Single:
                    Slave.sendToAll(this); break;
                case MessageType.xml:
                case MessageType.uninitialized:
                default: Program.pe("launchToOutput should never be called on this message type: "+this.type); return;
            }
        }

        override public string ToString() { return type.ToString() + myMessage.separator + data; }
        public string ToPrintString() { return "type:"+this.type+"; key:"+this.key+"; data:"+ data+""; }
        internal byte[] toByteArray(){ return Encoding.ASCII.GetBytes(this.ToString()); }

        /// <summary>
        /// processing executed after message got dequeued.
        /// </summary>
        public void consume() {
            Program.logSlave("consuming :"+this.ToPrintString());
            Slave s;
            ulong id;
            int removedCount;
            switch (type) {
                case MessageType.argumentChange:
                    StartupArgJson args;
                    try { args = StartupArgJson.deserialize(this.data); } catch (Exception e) {
                        Program.pe(this.type + " body is not deserializable: " + this.data, e); break; }
                    if (args == null || !args.Validate()) { Program.pe(this.type +" body is deserializable but with invalid content: " + this.data); break; }
                    Program.args = args;
                    Master.MasterCrashChecker.Abort();
                    Master.MasterCrashChecker = null;
                    new Thread(Program.HotRestart).Start();
                    /*
                    string[] arr = this.data.Split(myMessage.separator);
                    foreach (string str in arr) {
                        string[] kv = str.Split(myMessage.secondSeparator);
                        ulong slaveID;
                        if (!ulong.TryParse(kv[0], out slaveID)) { Program.pe("Unexpected slaveID key ("+kv[0]+") found in the body of messagetype."+this.type); continue; }

                    }*/
                    //todo: crea anche un software che generi messaggi di ripartizionamento per gestire dinamicamente tutte le partizioni, un supermaster
                    break;
                case MessageType.masterChange:
                    //if required in future trigger messageType.dinamicallyAddSlave, per ora va tutto bene anche se il nuovo master non era nella lista slaves.
                    string[] split = this.data.Split(myMessage.separator);
                    if (!ulong.TryParse(split[0], out id)) { Program.pe(this.type + " have non-numerical body; expected two numeric id separated by a '" + myMessage.separator + "', found instead: " + this.data); break; }
                    Slave oldMaster = Slave.getFromID(id);
                    if (oldMaster == null) break;
                    if (!ulong.TryParse(split[0], out id)) { Program.pe(this.type + " have non-numerical body; expected two numeric id separated by a '" + myMessage.separator + "', found instead: " + this.data); break; }
                    Slave newMaster = Slave.getFromID(id);
                    if (newMaster == null) break;
                    if (Master.currentMaster == oldMaster) Master.changeMaster(newMaster);//master checker msg received
                    Slave.Remove(oldMaster);
                    break;
                case MessageType.dinamicallyRemoveSlave:
                    if (!ulong.TryParse(this.data, out id)) { Program.pe(this.type + " have non-numerical body; expected numeric id, found: " + this.data); break; }
                    s = Slave.getFromID(id);
                    if (s == null) break;
                    Slave.Remove(s);
                    break;
                case MessageType.dinamicallyAddSlave:
                    Slave.deserializeOrGet(this.data);
                    if (Master.iAmTheMaster) { myMessage m = new myMessage(MessageType.provideSlaveList, "");
                        foreach(Slave s2 in Slave.all){
                            m.data += ";"+s2.serialize();
                        }
                        m.data = m.data.Substring(1);
                        m.launchToOutput();
                    }
                    break;
                case MessageType.provideSlaveList:
                    Volatile.Write(ref Master.lastMasterUpdate, DateTime.Now.Ticks);
                    string[] jsons = this.data.Split(myMessage.separator);
                    lock(Slave.all) foreach (string str in jsons) { Slave.deserializeOrGet(str); } break;
                case MessageType.confirmMessageSuccess_Single:
                    Volatile.Write(ref Master.lastMasterUpdate, DateTime.Now.Ticks);
                    removedCount = ReceiverTool.messageQueue.get(this.data, true) == null ? 0 : 1;
                    Program.logSlave(removedCount + " removed from queue.");
                    break;
                case MessageType.confirmMessageSuccess_Batch:
                    Volatile.Write(ref Master.lastMasterUpdate, DateTime.Now.Ticks);
                    removedCount = ReceiverTool.messageQueue.getOlderThan(this.data, true).Count;
                    Program.logSlave(removedCount + " removed from queue.");
                    break;
                case MessageType.xml: Program.pe("xml messages should be handled in Master thread without consuming."); return;
                default:
                case MessageType.uninitialized: Program.pe("uninitialized message consumed"); return;
            }
        }

        public int CompareTo(object obj){
            if (!(obj is myMessage)) { Program.pe("Invalid comparison: myMessage with " + (obj.GetType())); return -1; }
            return arrivalTime.CompareTo(((myMessage)obj).arrivalTime);
        }
    }
}

