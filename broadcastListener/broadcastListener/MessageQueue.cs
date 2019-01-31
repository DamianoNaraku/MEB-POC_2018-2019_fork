using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace broadcastListener
{
    public class TimeIntPair : IComparable {
        public DateTime t;
        public int i;
        public DateTime Key { get { return t; } }
        public int Value { get { return i; } }
        public TimeIntPair(DateTime t, int i) { this.t = t; this.i = i; }
        public TimeIntPair(int i, DateTime t) { this.t = t; this.i = i; }
        public int CompareTo(object obj) {
            if (!(obj is TimeIntPair)) { Program.pe("Wrong comparison: TimeIntPair with " + obj.GetType()); return -1; }
            TimeIntPair pair = (TimeIntPair)obj;
            return t.CompareTo(pair.t); }
    }
    public interface MessageQueueI {//todo: potrebbe diventare iMessageQueue<T> con T = TimedWithKeyInterface applicata a messageType per prendere timestamp e key con funzioni/proprietà.
        myMessage Add(myMessage m);
        myMessage get(string key, bool remove = false);
        myMessage getOldest(bool remove = false);
        ICollection<myMessage> getOlderThan(string key, bool remove = false);
        ICollection<myMessage> getOlderThan(myMessage t, bool remove = false);
        ICollection<myMessage> getOlderThan(DateTime t, bool remove = false);
        ICollection<myMessage> extractAll(bool remove = true);
        int Count { get; }
    }
    public class MessageQueue : MessageQueueI{
        SortedSet<myMessage> queue;
        Dictionary<string, myMessage> msgBymsgKey;
        // nb: se ho più d'un ricevitore l'ordine nelle code potrebbe essere diverso tra master e slave a seconda dello scheduling.
        // quindi se ci sono thread multipli e crasha il master, in extremis rimando tutto da 1 secondo prima dell'ultimo update del master
        public static TimeSpan sicurezzaPerDelayDeiThread; /*{
            get {
                if (delaythreadHiddenVariable.TotalDays == 64.0) { delaythreadHiddenVariable = Program.args.slaveReceiverThreads > 1 ? new TimeSpan(0, 0, 0, 1, 0) : new TimeSpan(0); }
                return delaythreadHiddenVariable;
            }
        }
        private static TimeSpan delaythreadHiddenVariable = new TimeSpan(64, 0, 0, 0, 0);*/

        public MessageQueue() {
            queue = new SortedSet<myMessage>();
            this.msgBymsgKey = new Dictionary<string, myMessage>();
            sicurezzaPerDelayDeiThread = Program.args.slaveReceiverThreads > 1 ? new TimeSpan(0, 0, 0, 1, 0) : new TimeSpan(0); }
        public int Count { get {
                int i = -1;
                lock (this) { i = this.queue.Count; }
                return i;
            } }
        myMessage MessageQueueI.Add(myMessage m){
            if (m == null) return null;
            if (m.key == null) m.key = m.data;
            lock (this) {
                if (this.msgBymsgKey.ContainsKey(m.key)) return null;//todo: non succederebbe "mai" (quasi) nell'esecuzione normale, ma forse dovrei usare una collezione che supproti i duplicati
                this.msgBymsgKey.Add(m.key, m);
                this.queue.Add(m); }
            if (this.Count == 0) throw new Exception();
            return m; }

        ICollection<myMessage> MessageQueueI.extractAll(bool remove) {
            SortedSet<myMessage> ret;
            lock (this) {
                ret = new SortedSet<myMessage>(this.queue);
                if (remove) {
                    this.queue.Clear();
                    this.msgBymsgKey.Clear(); }
            }//this way should be thread safe
            return ret; }

        myMessage MessageQueueI.get(string key, bool remove){
            myMessage ret = null;
            lock (this) {
                this.msgBymsgKey.TryGetValue(key, out ret);
                if(remove && ret != null) {
                    this.queue.Remove(ret);
                    this.msgBymsgKey.Remove(key); } }
            return ret; }

        ICollection<myMessage> MessageQueueI.getOlderThan(string key, bool remove){
            if (key == null) return new List<myMessage>(0);
            lock (this) {
                myMessage msg = ((MessageQueueI)this).get(key);
                if (msg == null) return new List<myMessage>(0);
                return ((MessageQueueI)this).getOlderThan(msg, remove); } }

        ICollection<myMessage> MessageQueueI.getOlderThan(myMessage msg, bool remove){
            if (msg == null) return new List<myMessage>(0);
            return ((MessageQueueI)this).getOlderThan(msg.arrivalTime - MessageQueue.sicurezzaPerDelayDeiThread, remove); }

        ICollection<myMessage> MessageQueueI.getOlderThan(DateTime t, bool remove) {
            lock (this) {
                myMessage bot = new myMessage(), top = new myMessage();
                bot.arrivalTime = new DateTime(0);
                top.arrivalTime = t;
                ICollection<myMessage> ret = this.queue.GetViewBetween(bot, top);
                if (ret == null) return new List<myMessage>(0);
                //todo:_ siccome alla queue può accederci solo uno per volta, fai una copia cache per il sender.
                if (remove) foreach (myMessage m in ret) ((MessageQueueI)this).get(m.key, true);
                return ret; } }

        public myMessage getOldest(bool remove) {
            myMessage ret;
            lock (this) {
                ret = this.queue.Min;
                if (remove && ret != null) ((MessageQueueI)this).get(ret.key, remove); }
            return ret; }
    }/*
    public class MessageQueueOld{
        List<myMessage> queueSorted;
        SortedDictionary<int, myMessage> queue;
        Dictionary<string, int> indexByMsgKey;
        //C5.TreeDictionary<DateTime, int> indexByTime0;
        C5.TreeBag<TimeIntPair> indexByMsgTime;
        private static int currentKey = 0;

        ////// nuovi

        List<myMessage> queueSortedByTime;
        Dictionary<string, myMessage> byMsgKey;
        public MessageQueueOld(){
            queue = new List<myMessage>();
            indexByMsgKey = new Dictionary<string, int>();//todo: se duplicati?
            //indexByTime0 = new C5.TreeDictionary<DateTime, int>();
            indexByMsgTime = new C5.TreeBag<TimeIntPair>();
            
        }
        // needed: todo:
        // add
        // get and remove by msgkey (or by msg)
        // get and remove older than time
        public myMessage Add(myMessage m) {
            //todo: write in document: assumption made: non possono esistere 2 messaggi identici (stesso equip, recipe, step, holdtype, holdflag, timestamp
            //se capitano io butto i duplicati successivi, mi serve una key univoca.
            lock (this) {
                //indexByTime0.Add(m.timestamp, queue.Count);
                try { indexByMsgKey.Add(m.key, queue.Count); } catch (System.ArgumentException e) { return null; }
                indexByMsgTime.Add(new TimeIntPair(m.timestamp, queue.Count));
                queue.Add(m); }
            return m; }
        public myMessage get(string messageKey) {
            lock (this) { //todo$ cerca se lock usa la var come semaforo vero e proprio garantendo la sicurezza anche dei campi oppure no.
                int index;
                if (!indexByMsgKey.TryGetValue(messageKey, out index)) return null;
                return queue[index]; }
        }
        //todo: sta cosa del timestamp non può funzionare, a meno che io non usi il timestamp contenuto nel messaggio. perchè sennò potrei avere una coda grossa su uno e generare un timestamp molto maggiore del tempo di arrivo del messaggio nel buffer.
        /*public List<myMessage> getPrevious0(DateTime t){
            //var indexByTime0 = new TreeDictionary<DateTime, int>();//useless
            lock (this) {
                int index;
                C5.KeyValuePair<DateTime, int> kv, kvnext;

                if (indexByTime0.TryPredecessor(t, out kv))
                {
                    index = kv.Value;
                    if (indexByTime0.TrySuccessor(kv.Key, out kvnext) && kvnext.Key == t) { index = kv.Value; }
                }
                else {
                    if (indexByTime0.Count > 0 && (kv = indexByTime0.First()).Key == t) { index = 0; }
                    else index = -1;
                }
                if (index == -1) return null;
                return queue.GetRange(0, index);
        }
    }* /
    public List<myMessage> getPrevious(DateTime tt, bool includeEqual=true){
            // getPrevious(B)
            // Tree:     | A | A | B | B | B | B | C |
            // if(includeEqual)                ^      
            // else            ^                      

            TimeIntPair original = new TimeIntPair(tt, int.MinValue);//todo: includi anche int nell'ordinamento
            lock (this) {
                TimeIntPair current , next, prev;
                //get previous startpoint (might be equal)
                if(!indexByMsgTime.TryPredecessor(original, out current)){
                    //todo:
                    throw new Exception ("sei sicuro che non esista un elemento più piccolo? non è che ti da false semplicemente perchè tt non è inserito nella collezione?");
                    if (indexByMsgTime.Count == 0) return null;
                    else current = indexByMsgTime.First(); }

                //check first less than, or last equal to current. (as in the array draw on top)
                if (includeEqual) while (indexByMsgTime.TrySuccessor(current, out next) && next.Key == original.Key) { current = next; }
                else {
                    while (indexByMsgTime.TryPredecessor(current, out prev) && prev.Key == original.Key) { current = prev; }
                    if(indexByMsgTime.TryPredecessor(current, out prev)) { current = prev; }
                }

                int index = current.Value;
                if (index < 0) return null;
                int start = 0;
                int count = index - start + 1;
                return queue.GetRange(start, count);
        }
    }
        internal myMessage PeekOldest(){ return getOldest(false); }
        internal myMessage PopOldest() { return getOldest(true); }
        internal myMessage PopOldest(out int CountAfterPop) { return getOldest(true, out CountAfterPop); }
        internal myMessage getOldest(bool remove){ int a; return getOldest(remove, out a); }
        internal myMessage getOldest(bool remove, out int countAfterOperation){
            myMessage ret;
            bool checkbughere = true;
            lock (this) {
                if (this.queue.Count == 0) { countAfterOperation = 0; return null; }
                //int index = this.indexByTime0.First().Value;
                int index = this.indexByMsgTime.First().Value;
                ret = this.queue[index];//sono desincronizzate perchè blocco la cancellazione a metà, quindi alcune code sono piene e altre no
                if (remove) Remove(ret, index);
                if (checkbughere) { countAfterOperation = 0; return null; }
                countAfterOperation = this.queue.Count;}
            return ret; }
        internal myMessage Remove(myMessage m, int indexx = -1) {
            bool checkbughere = true;
            lock (this){
                if (indexx == -1) this.queue.Remove(m);
                else this.queue.RemoveAt(indexx);
                //this.indexByTime0.Remove(m.timestamp);
                TimeIntPair equalToDeleteTarget = new TimeIntPair(m.timestamp, indexx);
                //if (this.indexByTime.Contains(equalToDeleteTarget)) { throw new Exception(); }
                try { if (!this.indexByMsgTime.Remove(equalToDeleteTarget)) throw new Exception(""); } catch(Exception e) { MessageBox.Show(e.ToString()); }
                if (checkbughere) { return null; }
                if (!this.indexByMsgTime.Remove(equalToDeleteTarget)) throw new Exception("");//todo: bug here
                this.indexByMsgKey.Remove(m.key);
            }
            return m;
        }

        internal myMessage Remove(string key) {
            myMessage m = null;
            lock (this) {
                int index = this.indexByMsgKey[key];
                m = this.queue[index];
                Remove(m, index);
            }
            return m;}

        public void RemoveOlderThan(string msgKey){ lock (this) { RemoveOlderThan(this.get(msgKey)); }; }//todo: il master dice: elimina quelli più vecchi di key (univoca e giusta). lo slave elimina tutti quelli precedenti, ma come determina il precedente se i timestamp sono uguali a quello da eliminare? come determino se ho 3 con stesso timestamp, di cui uno è la key da eliminare fino a quel punto, gli altri 2 vengono prima o dopo?
        public void RemoveOlderThan(int queueKey) {
        }
        // nb: se ho più d'un ricevitore l'ordine nelle code potrebbe essere diverso tra master e slave a seconda dello scheduling.
        // quindi se ci sono thread multipli e crasha il master, in extremis rimando tutto da 1 secondo prima dell'ultimo update del master
        public static TimeSpan sicurezzaPerDelayDeiThread { get {
                if (delaythreadHiddenVariable.TotalDays == 64.0) { delaythreadHiddenVariable = Program.args.slaveReceiverThreads > 1 ? new TimeSpan(0, 0, 0, 1, 0) : new TimeSpan(0); }
                return delaythreadHiddenVariable;
            } }
        private static TimeSpan delaythreadHiddenVariable = new TimeSpan(64, 0, 0, 0, 0);
        protected void RemoveOlderThan(myMessage msg, TimeSpan? sicurezza = null, bool includeEquals = false) { RemoveOlderThan(msg.timestamp - (sicurezza == null ? sicurezzaPerDelayDeiThread : sicurezza.Value), includeEquals); }
        protected void RemoveOlderThan(DateTime time, bool includeEquals = false) {
            List<myMessage> arr = this.getPrevious(time, includeEquals);
            foreach (myMessage msg in arr) this.Remove(msg);
        }
        
    }*/
}
