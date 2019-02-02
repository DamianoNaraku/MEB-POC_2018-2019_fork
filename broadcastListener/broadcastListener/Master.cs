using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using C5;
namespace broadcastListener
{//todo: riallocare dinamicamente repliche come partizioni se ci sono tante repliche e il carico di lavoro è superiore al previsto.
    class Master { // singleton class.
        public static Master master = null;
        public static bool iAmTheMaster { get { return Slave.self == Master.currentMaster; } }


        public static Semaphore canPublish { get { return canPublish0; } set { canPublish0 = value; } }
        protected static Semaphore canPublish0;

        public const int sec = 1000;
        public const int min = 60 * sec;
        public const int hour = 60 * min;

        public const int crashCheckInterval = 1 * sec;
        public static readonly TimeSpan crashTimeoutInterval = new TimeSpan(30 * sec - crashCheckInterval);//it can fail up to 10 times and still deliver in 5 min
        public static Slave currentMaster = null;
        public static Thread MasterCrashChecker;
        public static Thread publisher;
        //internal static Semaphore canSendMessage;
        public static long lastMasterUpdate;

        public Master(Slave s = null) {
            Master.master = this;
            Slave selfDebug = Slave.self;
            List<Slave> alldebug = Slave.all;
            if (s == null) s = findNextMaster();
            changeMaster(s);//init
            if (iAmTheMaster || MasterCrashChecker != null) return;
            MasterCrashChecker = new Thread(masterCrashCheckLoop);
            MasterCrashChecker.Name = "Master crash checker (Slave only)";
            MasterCrashChecker.Start();
        }

        public static void masterCrashCheckLoop() { try { masterCrashCheckLoop0(); } catch (Exception e) { if (e is ThreadAbortException) return; string s = "exception in MasterCrashCheck()" + e.ToString(); Program.pe(s); MessageBox.Show(s); } }
        public static void masterCrashCheckLoop0() {
            try {
                while (true) {
                    if (masterIsCrashedCheck()) {
                        //what if a dinamically added slave that is not acknowledged by all becomes the master? devo passare direttamente il master scelto per forza.
                        //Slave.sendToAll(new Message(MessageType.masterCrashed, Master.currentMaster.serialize()));
                        Slave newmaster = findNextMaster();
                        new myMessage(MessageType.masterChange, newmaster.serialize()).launchToOutput();
                        Master.changeMaster(newmaster);//master checker loop
                    }
                    Thread.Sleep(Master.crashCheckInterval);
                }
            } catch(ThreadAbortException ex) { Program.p("Stopped thread MasterCrashCheckLoop: ", ex); return; }
        }
        public static bool masterIsCrashedCheck() {
            if (iAmTheMaster)  Program.pe("Master should not execute \"masterIsCrashedCheck()\".");
            myMessage oldest = ReceiverTool.messageQueue.getOldest();
            if(oldest == null) return false; //if my pending list is empty, the master is working great.
            bool c1, c2;
            //  c1 &  c2: master is working but overcharged.
            //  c1 & !c2: master is working fine.
            // !c1 &  c2: master is assumed dead. (or missed to notify a old message while no new messages are incoming).
            // !c1 & !c2: master is assumed alive. (status is unknow, because no message are incoming from tools or master).
            long tmp = Volatile.Read(ref Master.lastMasterUpdate);
            return 
                 ! (c1 = (DateTime.Now.Ticks - tmp < Master.crashTimeoutInterval.Ticks))
                && (c2 = (DateTime.Now - oldest.arrivalTime) > Master.crashTimeoutInterval); }
        public static bool masterIsOvercharged(){
            if (iAmTheMaster) return false; // a better check is done while doing master's operations. this is redundant for master.
            myMessage oldest = ReceiverTool.messageQueue.getOldest();
            if (oldest == null) return false; //if my pending list is empty, the master is working great.
            bool c1, c2;
            //  c1 &  c2: master is working but overcharged.
            //  c1 & !c2: master is working fine.
            // !c1 &  c2: master is assumed dead. (or missed to notify a old message while no new messages are incoming).
            // !c1 & !c2: master is assumed alive. (status is unknow, because no message are incoming from tools or master).
            long tmp = Volatile.Read(ref Master.lastMasterUpdate);
            return (c1 = (DateTime.Now.Ticks - tmp < Master.crashTimeoutInterval.Ticks))
                && (c2 = (DateTime.Now - oldest.arrivalTime) > Master.crashTimeoutInterval); }
        public static void changeMaster(Slave s) {
            if (Master.currentMaster == s) return;
            if (Slave.self == s) { //if i became the master...
                if (MasterCrashChecker != null) { MasterCrashChecker.Abort(); MasterCrashChecker = null; }//todo: sempre setta a null i thread terminati.
                Master.currentMaster = s;
                Master.becomeMaster(); }
            else Master.currentMaster = s;
        }

        public static Slave findNextMaster() {
            Slave[] masterPrioList = Slave.sortByMasterPrio();
            // if it is dinamically executed and there is no known master, 
            // but there are peer colleagues means one of them is the current master even if this slave might have higher master priority than current master.
            // in this case it will not replace the current master, because it's useless while it is working and because this slave have a empty queue.ù
            // so a dinamically started slave can be the master only if it is the only one.
            
            if (Master.currentMaster == null) {
                if (Program.args.dinamicallyStarted) { //todo: uno slave apena partito potrebbe non conoscere il master finchè non riceve la reply messagetype.SlaveList
                    if (masterPrioList[0] == Slave.self && masterPrioList.Length > 1) { return masterPrioList[1]; }
                    else {  return masterPrioList[0]; }
                } else { return masterPrioList[0]; }
            }
            // guaranteed to have at least 2 elements: the previous master (now crashed) and the executing slave program.
            // except during initialitization when there can be 1 without triggering error. (the master = this program)
            Slave nextmaster = masterPrioList[0] == Master.currentMaster ? masterPrioList[0] : masterPrioList[1];
            return nextmaster;
        }

        public static void becomeMaster() {
            publisher = new Thread(publishToKafkaLoop);
            publisher.Name = "Kafka Publisher (Master)";
            //to avoid queue growing, the consumer must have higher priority of the producer (broadcast listener).
            publisher.Priority = ThreadPriority.Highest;
            publisher.Start();
        }



        public static void publishToKafkaLoop() {
            try { publishToKafkaLoop0(); } catch (Exception e) { if (e is ThreadAbortException) return; string s = "exception in MasterPublisher" + e.ToString(); Program.pe(s); MessageBox.Show(s); } 
        }
        internal static void publishToKafkaLoop0() {
            int remaining, received = 0;
            //myMessage msg;
            bool checkbughere = true;
            int volte = 10;

            //List<System.Collections.Generic.ICollection<myMessage>> oldCaches = new List<System.Collections.Generic.ICollection<myMessage>>();
            while (true) {
                //semaphore tell if there are enqueued message without waiting and looping with a timeout
                Master.canPublish.WaitOne();
                System.Collections.Generic.ICollection<myMessage> cache = ReceiverTool.messageQueue.extractAll(true);
                if (cache == null || cache.Count == 0) continue;
                myKafka.send(cache);
                //oldCaches.Add(cache);
                //if (--volte <= 0) throw new Exception();
                /*int i = 0;
                foreach (myMessage msg in cache) {
                    remaining = cache.Count - i++;
                    if (msg == null) continue;
                    //if (checkbughere) continue;
                    publishMessage(msg);
                    if (Program.args.slaveNotifyMode_Batch == 0) new myMessage(MessageType.confirmMessageSuccess_Single, msg.key).launchToOutput();
                    if ((received = received + 1 % Program.args.slaveNotifyMode_Batch) == 0 || remaining == 0) new myMessage(MessageType.confirmMessageSuccess_Batch, msg.key).launchToOutput();
                }*/
            }
        }
    }
}
