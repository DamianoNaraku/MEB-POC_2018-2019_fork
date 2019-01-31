using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.IO;

namespace broadcastListener{

    public class StartupArgJson{
        public int broadcastPort_Tool, broadcastPort_Slaves;
        public bool enableGUI;
        public bool networkGuaranteeDatagramOrder;// in broadcast è impossibile che un programma ricevi A,B e un altro ricevi B,A; quindi dovrebbe essere sempre true.
        public List<Slave> replicatorsList;
        public int myPartitionNumber, partitionNumbers_Total;
        public int toolReceiverThreads, slaveReceiverThreads;
        // of your network
        public string broadcastAddress;
        // those field are for debug, each one of them can be independently null
        public string logFile, errFile, criticalErrFile, toolMsgFile, slaveMsgFile;
        // if greater than 0 allow notifying multiple message publishing confirmation to the slaves with a single intra-message.
        // the value 0 means the feature is disabledd and there will be a publishing confirm for each message (doubling network load)
        // the value 1 makes little sense but is not forbidden.
        // values greater than 1 are the maximum message publishing confirmed with a single intra-message.
        // if the queue rech 0 messages, a batch-confirmation message will be sent regardless, just before the publisher will block itself, to avoid latency in the notification.
        //$todo: priorità del publisher dinamica: bassa quando ha la coda vuota o quasi, alta quando ha la coda > x%.
        public int slaveNotifyMode_Batch;
        /// <summary>
        /// true if this running instance has been executed after his peers.
        /// it will announce itself to all peers to dinamically add itself into the partition as a slave.
        /// </summary>
        public bool dinamicallyStarted;
        /// <summary>
        /// specify when to log tool messages: when received or when published to kafka.
        /// to avoid logging at all disable both toolMsgFile and enableGUI
        /// </summary>
        public bool logToolMsgOnReceive, exclusiveBind;

        // comma-separated with format "Address:Port" or "URI:Port"
        public string KafkaNodes;
        public string KafkaTopic;
        //enables test mode and display performance statistics.
        public bool benchmark;

        //todo: non devono esserci campi non public, pare che non vengano serializzati
        //public int replicationDegree;


        [JsonConstructor] public StartupArgJson() { }
        public override string ToString() { return this.serialize(); }
        public string serialize() { return JsonConvert.SerializeObject(this); }
        public static StartupArgJson deserialize(string s) {
            StartupArgJson json = null;
            object o;
            //JsonSerializer jss = new JsonSerializer();
            //jss.MissingMemberHandling = MissingMemberHandling.Error;
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.MissingMemberHandling = MissingMemberHandling.Error;
            //settings.ReferenceLoopHandling = ReferenceLoopHandling.Serialize; che succederebbe?
            try { json = (StartupArgJson)(o = JsonConvert.DeserializeObject<StartupArgJson>(s, settings)); }
            catch (Exception e) { MessageBox.Show("Input JSON format should be like this: " + new StartupArgJson().ToString() + Environment.NewLine + "Found instead:" + s + Environment.NewLine + "Exception:" + e.ToString()); }
            return json;
        }

        internal bool Validate() {
            //if (this.benchmark && !this.enableGUI && this.logFile == null) { MessageBox.Show(""); }
            if (myPartitionNumber >= partitionNumbers_Total) { MessageBox.Show("PartitionNumbers_Total must be the number of partitions made. Since partition numbers starts from zero it cannot be equal or less to a partition number."); return false; };
            if (partitionNumbers_Total <= 0) { MessageBox.Show("PartitionNumbers_Total must be the number of partitions made. Therefore it cannot be less than one."); return false; };
            //if (replicationDegree <= 0) { MessageBox.Show("replicationDegree must be the number of backup-slaves running in case of failure for fault tolerance. It cannot be a negative number."); return false; };
            //it's logical, not physical! it is the maximum number of thread executable simultaneously.
            if (toolReceiverThreads < 0) toolReceiverThreads = (int)Math.Ceiling(Environment.ProcessorCount / 2.0);
            if (slaveReceiverThreads < 0) slaveReceiverThreads = (int)Math.Floor(Environment.ProcessorCount / 2.0);
            System.Net.IPAddress tmp;
            if (!System.Net.IPAddress.TryParse(this.broadcastAddress, out tmp)) Program.pex("broadcastAddress must be a valid IP string");
            if (this.partitionNumbers_Total > 1 && this.broadcastPort_Slaves == this.broadcastPort_Tool) Program.pex("port of tool broadcasting and intra-communication broadcasting can be equal only if there is a single partition (no partition)");
            if (slaveNotifyMode_Batch < 0) { Program.pex("slaveNotifyMode_Batch must be at least zero to be valid, and equal to 0 or greater than 1 to make sense. Read the documentation."); return false; }

            if (toolMsgFile != null) try { Path.GetFullPath(this.toolMsgFile); } catch (Exception e) { Program.pex("ToolMsgFile must either be null or a valid path", e); }
            if (slaveMsgFile != null) try { Path.GetFullPath(this.slaveMsgFile); } catch (Exception e) { Program.pex("SlaveMsgFile must either be null or a valid path", e); }
            if (logFile != null) try { Path.GetFullPath(this.logFile); } catch (Exception e) { Program.pex("logFile must either be null or a valid path", e); }
            if (errFile != null) try { Path.GetFullPath(this.errFile); } catch (Exception e) { Program.pex("errFile must either be null or a valid path", e); }
            if (criticalErrFile != null) try { Path.GetFullPath(this.criticalErrFile); } catch (Exception e) { Program.pex("criticalErrFile must either be null or a valid path", e); }
            if (toolMsgFile != null) try { Path.GetFullPath(this.toolMsgFile); } catch (Exception e) { Program.pex("ToolMsgFile must either be null or a valid path", e); }
            if (exclusiveBind && (toolReceiverThreads > 1 || slaveReceiverThreads > 1)) Program.pex("You can't request exclusive socket bind with multiple receiver threads");

            foreach (Slave s in this.replicatorsList){
                //check done in slave.coInitializer();

            }
            return true;
        }
        public static string[] fakeinput() {
            string desktop = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
            bool exclBind = false;
            string kafkaHost = "localhost";//"192.168.1.8";//"localhost"
            StartupArgJson json = new StartupArgJson() {
                myPartitionNumber = 0,
                partitionNumbers_Total = 1,
                broadcastPort_Tool = 20001,
                toolReceiverThreads = exclBind ? 1 : (int)Math.Ceiling(Environment.ProcessorCount / 2.0),//it's logical, not physical! it is the maximum number of thread executable simultaneously.
                slaveReceiverThreads = exclBind ? 1 : (int)Math.Floor(Environment.ProcessorCount / 2.0),
                enableGUI = true,
                broadcastAddress = "192.168.1.255",
                criticalErrFile = desktop + @"\Listener_CriticalErrors.txt",
                errFile = desktop + @"\Listener_Errors.txt",
                logFile = desktop + @"\Listener_EventLog.txt",
                toolMsgFile = desktop + @"\Listener_ToolLog.txt",
                slaveMsgFile = desktop + @"\Listener_SlaveLog.txt",
                logToolMsgOnReceive = false,
                exclusiveBind = exclBind,
                KafkaNodes = "http://"+kafkaHost+":9093, http://"+kafkaHost+":9094, http://"+kafkaHost+":9094",
                KafkaTopic = "toolsEvents",
                benchmark = true,
            };
            json.slaveNotifyMode_Batch = 100;
            json.dinamicallyStarted = false;
            json.broadcastPort_Slaves = 20002 + json.myPartitionNumber;
            

            ulong guid = Program.GetMACAddress();
            int replicationDegree = 3;
            json.replicatorsList = new List<Slave>(replicationDegree);

            for (int i = 0; i < replicationDegree; i++){
                Slave replica = new Slave();
                replica.ip_string = "192.168.1." + (100 + i);
                replica.id = guid + (ulong)i;
                replica.isSelf = i == 0;
                json.replicatorsList.Add(replica);
            }
            string s = json.ToString();
            return new string[] { json.ToString() };
        }
    }
}