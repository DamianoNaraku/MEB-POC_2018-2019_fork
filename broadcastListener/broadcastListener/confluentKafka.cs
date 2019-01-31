using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace broadcastListener
{
    class confluentKafka
    {
        static Producer producer;
        public static void staticinit() {
            Dictionary<string, object> config = new Dictionary<string, object>()
            {
                { "bootstrap.servers", Program.args.KafkaNodes },
                //{ "group.id", "ept-oi-log" },
                { "enable.auto.commit", true },
                { "session.timeout.ms", 15000 },
                { "client.id", "1" },
            };
            producer = new Producer(config);
            producer.OnError += Producer_OnError;
        }
        
        public static void test()//was async
        {
            
            string msgkey="", msgval = "Confluent";
            //for(int i =0; i<100; i++) { msgval += "y"; }
            msgkey = "Key_" + msgval;
            byte[] bmsgKey = msgkey.stringToByteArr(), bmsgVal = msgval.stringToByteArr();
                try
                {
                    Task<Confluent.Kafka.Message> task = producer.ProduceAsync(Program.args.KafkaTopic, bmsgKey, bmsgVal);
                for(int i = 0; i < 1; i++) {
                    Thread.Sleep(2000);
                    MessageBox.Show("Confluent TaskStatus, after "+(i+1)*2+" sec:"+task.Status+";");
                }

                Thread.Sleep(2000);
                MessageBox.Show("Confluent TaskStatus (final):" + task.Status + ";");
                }
                catch (KafkaException e)
                {
                    MessageBox.Show("Confluent failed to deliver message:"+e.Message+", errNo:"+e.Error.Code+"; exception:"+e);
                }

            // Awaiting the asynchronous produce request below prevents flow of execution
            // from proceeding until the acknowledgement from the broker is received.
            //var deliveryReport = producer.ProduceAsync(topicName, new Confluent.Kafka.Message<string, string> { Key = "", Value = "" });
        }

        private static void Producer_OnError(object sender, Error e)
        {
            MessageBox.Show("confluent producer Error:" + e.ToString()+" kafkanodes:"
                +Program.args.KafkaNodes);
        }
    }
}
