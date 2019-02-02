using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KafkaNet;
using KafkaNet.Protocol;
using KafkaNet.Model;
using KafkaNet.Common;
using KafkaNet.Statistics;
using System.Threading;
using System.Windows.Forms;


namespace broadcastListener
{

    public class myKafka {
        public static List<Uri> kafkaNodes;
        private static Producer producer;
        private static string topicName { get { return Program.args.KafkaTopic; } }
        public static Producer staticInit() {
            string[] nodesStr = Program.args.KafkaNodes.Split(',');
            kafkaNodes = new List<Uri>(nodesStr.Length);
            foreach (string node in nodesStr) {
                try { kafkaNodes.Add(new Uri(node)); } catch (Exception e) { Program.pex("Argument Error: Invalid KafkaNodes value: "+node+"; found in: "+Program.args.KafkaNodes, e); }
            }
            var tmp = kafkaNodes.ToArray();
            KafkaOptions options = new KafkaOptions(tmp);
            BrokerRouter router = new BrokerRouter(options);
            producer = new Producer(router);
            return producer;}

        public static void kafkatest(){
            /*string uri = "http://192.168.1.8";
            var options = new KafkaOptions(new Uri(uri+":9092"), new Uri(uri+":9093"));
            var router = new BrokerRouter(options);
            var client = new Producer(router);

            client.SendMessageAsync("TestHarness", new[] { new KafkaNet.Protocol.Message("hello world") }).Wait();

            using (client) { }
            return;*/
            //kafka2019.staticinit(); confluentKafka.staticinit();
            staticInit();
            string s = "PublisherA";
            //for (int i = 0; i < 500; i++) s += "z";
            //myMessage msg = new myMessage(MessageType.confirmMessageSuccess_Batch, s);
            List<myMessage> arr = new List<myMessage>();
            for (int i = 0; i < 2; i++) { arr.Add(new myMessage(MessageType.confirmMessageSuccess_Batch, s)); }
            while (true) {
                //kafka2019.test(); confluentKafka.test();
                myKafka.send(arr);
                var brok = producer.BrokerRouter;
                //MessageBox.Show("tattica A (il primo tipo); inflight:"+producer.InFlightMessageCount+", asyncCount:"+producer.AsyncCount+"; batchdelay:"+producer.BatchDelayTime+"; buffCount:"+producer.BufferCount);
                //throw new Exception();
                break;
            }

        }
        /*consumer:
         
            string topic ="IDGTestTopic";
            Uri uri = new Uri("http://localhost:9092");
            var options = new KafkaOptions(uri);
            var router = new BrokerRouter(options);
            var consumer = new Consumer(new ConsumerOptions(topic, router));
            foreach (var message in consumer.Consume()) { Console.WriteLine(Encoding.UTF8.GetString(message.Value)); }
            
             */
        public static TimeSpan? timeout = new TimeSpan(0, 2, 0, 0);
        public static void send(ICollection<myMessage> mymsgs) {
            List<KafkaNet.Protocol.Message> arr = new List<KafkaNet.Protocol.Message>();
            if (mymsgs.Count == 0) return;
            myMessage last = null;
            //KafkaNet.Protocol.Message tmp = new KafkaNet.Protocol.Message();
            
            foreach (myMessage mymsg in mymsgs) { arr.Add(new KafkaNet.Protocol.Message(mymsg.data, mymsg.makeKafkaKey())); last = mymsg; }
            timeout = new TimeSpan(0, 0, 1);
            Task<List<ProduceResponse>> operationStatus = producer.SendMessageAsync(topicName, arr, 1, timeout, MessageCodec.CodecNone);//todo: can be zipped.
            
            
            if (operationStatus == null) return;
            //operationStatus.Wait();
            //List<ProduceResponse> response = operationStatus.Result;
            //todo: controlla operation status ed esegui la seconda parte solo quando hai ricevuto l'ack, eliminando la wait.
            //operationComplete(mymsgs, last);


            object argArr = new object[] { mymsgs, last, operationStatus };
            Thread t = new Thread(new ParameterizedThreadStart(WaitOperationComplete0));
            t.Name = "Thread Waiting For Kafka Reply";
            //WaitOperationComplete(argArr);
            t.Start(argArr);
            //todo: System.OutOfMemoryException
        }

        private static void WaitOperationComplete0(object arguments){
            try { WaitOperationComplete(arguments); }
            catch (Exception ex) { Program.pe("Eccezione in Kafka.WaitOperationComplete: "+ex.ToString()); MessageBox.Show(ex.ToString()); }
        }
        public static volatile int totalSuccessSent = 0;
        private static void operationComplete(ICollection<myMessage> completed, myMessage last, List<ProduceResponse> reply) {
            foreach (ProduceResponse response in reply) {
                if (response.Error != 0) Program.pe("ErrNo " + response.Error + " for msg n° " + response.Offset + " of partition " + response.PartitionId + " in topic " + response.Topic);
            }
            if (Program.args.slaveNotifyMode_Batch == 0) {
                foreach (myMessage mymsg in completed) { new myMessage(MessageType.confirmMessageSuccess_Single, mymsg.key).launchToOutput(); } }
            else { new myMessage(MessageType.confirmMessageSuccess_Batch, last.key).launchToOutput(); }

            if (Program.args.logToolMsgOnReceive == false){
                int i = 1;
                foreach (myMessage mymsg in completed){
                    Program.LogToolMsg("sentBatch["+(i++)+"/"+completed.Count+"]: "+mymsg.ToPrintString());
                }
            }
            //lock(totalSuccessSent)
            totalSuccessSent += completed.Count;
        }

        public static void WaitOperationComplete(object arguments) {
            object[] arr;
            ICollection<myMessage> completed;
            myMessage last;
            Task<List<ProduceResponse>> task = null;
            if ((arr = arguments as object[]) == null) Program.pex("invalid argument type:" + arguments.GetType());
            int i = 0;
            if ((completed = arr[i] as ICollection<myMessage>) == null) Program.pex("invalid argument[" + i + "](completed) type:" + arr[i].GetType());
            i++;
            if ((last = arr[i] as myMessage) == null) Program.pex("invalid argument[" + i + "](last) type:" + arr[i].GetType());
            i++;
            if ((task = arr[i] as Task<List<ProduceResponse>>) == null) Program.pex("invalid argument[" + i + "](task) type:" + arr[i].GetType());
            i++;
            //todo: wait for task end or timeout
            int sec = 1000, min = 60 * sec, hour = 60 * min;
            int timeout = 2 * min;
            timeout = 30 * sec;
            int checkFrequency = ((int)Math.Ceiling(timeout / 10.0));
            while (timeout > 0) {
                if (task.IsCanceled) { Program.pe("KafkaTask canceled:" + task.Status +";"+ task); return; }
                if (task.IsFaulted) { Program.pe("KafkaTask faulted:" + task.Status +";"+ task); return; }
                if (task.IsCompleted) { operationComplete(completed, last, task.Result); return; }
                timeout -= checkFrequency;
                Thread.Sleep(checkFrequency);
            }
            Program.pe("KafkaTask timed out, status: " + task.Status + ";");//todo: trasforma in pex e cancella l'operation complete anche in caso di timeout
            return; //todo: remove this
            //operationComplete(completed, last, new List<ProduceResponse>(0));
        }
    }
}
