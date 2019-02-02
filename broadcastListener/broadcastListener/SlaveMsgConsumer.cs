using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace broadcastListener
{
    class SlaveMsgConsumer{
        public static List<SlaveMsgConsumer> all;
        public static Semaphore canConsume;
        public Thread thread;
        public static void staticInit() {
            all = new List<SlaveMsgConsumer>();
            canConsume = new Semaphore(0, int.MaxValue, "SlaveMsgConsumer.canConsume");
        }
        public SlaveMsgConsumer(int num=-1, int totals=-1) {
            coInitializer();
        }
        public void coInitializer(int num=-1, int totals = -1) {
            if (!all.Contains(this)) all.Add(this);
            thread = new Thread(consumeLoop);
            thread.Name = "Intracommunication Message Consumer"; }

        public static void Start() { foreach (SlaveMsgConsumer s in SlaveMsgConsumer.all) { s.start(); } }
        public static void Stop() { foreach (SlaveMsgConsumer s in SlaveMsgConsumer.all) { s.stop(); } }
        public void start(){
            if (thread == null) this.coInitializer();
            thread.Start(); }
        public void stop() {
            if (thread == null) return;
            thread.Abort();
            thread = null; }

        public void consumeLoop() {
            Program.p("SlaveMsgConsumer started");
            try { consumeLoop0(); } catch (Exception e) { if (e is ThreadAbortException) return; string s = "exception in ReceiveBroadcast()" + e.ToString(); Program.pe(s); MessageBox.Show(s); } }
        public void consumeLoop0() {
            while (true) {
                canConsume.WaitOne();
                //todo: fai wait(x) oppure wait until block, perchè il ricevitore lo sblocca 1 volta per ogni messaggio ricevuto, ma lui con 1 sblocco ne processa N, e poi fa N-1 cicli a vuoto.
                //while (SlaveReceiver.messageQueue.Count == 0) canConsume.WaitOne(); 
                ICollection<myMessage> cache = SlaveReceiver.SlaveMessageQueue.extractAll(true);
                
                foreach (myMessage msg in cache) {
                    try {
                        msg.consume();
                    } catch (Exception e) { Program.pe("Failed to consume message: " + msg.ToPrintString()); }
                }
            }
        }
    }
}
