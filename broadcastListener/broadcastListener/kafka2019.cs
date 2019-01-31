using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kafka.Public;
namespace broadcastListener
{
    class kafka2019
    {
        static KafkaProducer<string, string> prod;
        static ClusterClient cluster;
        static CustomLogger logger;
        public static void staticinit()
        {
            Configuration conf = new Configuration { Seeds = "localhost:9093" };//"broker.local:9091"
            conf.ClientId = "LibreriaBProducer";
            conf.ClientRequestTimeoutMs = 2000;
            conf.RequestTimeoutMs = 2000;
            conf.RefreshMetadataInterval = new TimeSpan(0, 0, 0, 0, 100);
            conf.CompressionCodec = CompressionCodec.None;
            conf.ErrorStrategy = ErrorStrategy.Retry;
            //conf.MaxBufferedMessages = 100;
            conf.MaxRetry = 10;
            conf.ProduceBatchSize = 1;
            conf.ProduceBufferingTime = new TimeSpan(0, 0, 0, 0, 10);
            conf.RequiredAcks = RequiredAcks.Leader;
            logger = new CustomLogger();
            cluster = new ClusterClient(conf, logger);
            prod = new KafkaProducer<string, string>(Program.args.KafkaTopic, cluster);
            prod.Acknowledged += Prod_Acknowledged;
            prod.MessageDiscarded += Prod_MessageDiscarded;
            prod.MessageExpired += Prod_MessageExpired;
            prod.Throttled += Prod_Throttled;

        }
        public kafka2019() { }
        public static void test() {
            for (int i = 0; i < 3; i++) { 
            cluster.Produce(Program.args.KafkaTopic, "PublisherB1", DateTime.Now);
            //cluster.MessageReceived += kafkaRecord => { /* do something */ };
            //cluster.ConsumeFromLatest("some_topic", somePartition);
            // OR (for consumer group usage)
            //cluster.Subscribe("some group", new[] { "topic", "some_other_topic" }, new ConsumerGroupConfiguration { AutoCommitEveryMs = 5000 });
            //clu

            prod.Produce("PublisherB2", DateTime.Now);
            }
            MessageBox.Show(
                "Libreria B: successfull sent: " + cluster.Statistics.SuccessfulSent + "; discarded: " + cluster.Statistics.Discarded+
                "; posponed: " +cluster.Statistics.MessagePostponed + "; retries: " + cluster.Statistics.MessageRetry + "; raw produced: " 
                + cluster.Statistics.RawProduced + "; rawProdBytes: " + cluster.Statistics.RawProducedBytes);
            MessageBox.Show("LibreriaB logger:" + logger);

        }

        private static void Prod_Throttled(int obj)
        {
            MessageBox.Show("kafkaB message Throttled");
        }

        private static void Prod_MessageExpired(KafkaRecord<string, string> obj)
        {
            MessageBox.Show("kafkaB Message expired");
        }

        private static void Prod_MessageDiscarded(KafkaRecord<string, string> obj)
        {
            MessageBox.Show("kafkaB Message discarded");
        }

        private static void Prod_Acknowledged(int obj)
        {
            MessageBox.Show("kafkaB Message acknowledge success!");
            //throw new NotImplementedException();
        }
    }
    public class CustomLogger : ILogger
    {
        public string logInfo, logError, logDebug, logWarning;
        public IDisposable BeginScopeImpl(object state)
        {
            return null;
        }
        
        public void LogDebug(string message)
        {
            logDebug += Environment.NewLine + message;
        }

        public void LogError(string message)
        {
            logError += Environment.NewLine + message;
        }

        public void LogInformation(string message)
        {
            logInfo +=Environment.NewLine + message;
        }

        public void LogWarning(string message)
        {
            logWarning += Environment.NewLine + message;
        }
        public override string ToString()
        {
            return "Warnings:" + logWarning +
                Environment.NewLine + Environment.NewLine + "Debug:" + logDebug +
                Environment.NewLine + Environment.NewLine + "Errors:" + logError +
                Environment.NewLine + Environment.NewLine + "info:" + logInfo;
        }
    }
}
