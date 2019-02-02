using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;

namespace broadcastListener
{

    public partial class GUI : Form{
        public static GUI thiss;
        public GUI(){ thiss = this; InitializeComponent();
            lock (Program.ps) this.textboxStatus.Text = Program.ps;
            lock (Program.pes) this.richTextBoxErrors.Text = Program.pes;
        }

        public static void exit() {
            bool b = Program.args.benchmark;
            //Program.args.benchmark = false;//do not trigger threadAbort messages unless they are unexpected aborts.
            try { SlaveReceiver.Stop(1); } catch (Exception) { }
            try { ReceiverTool.Stop(1); } catch (Exception) { }
            try { Master.MasterCrashChecker.Abort(1); } catch (Exception) { }
            try { Master.publisher.Abort(1); } catch (Exception) { }
            try { SlaveMsgConsumer.Stop(); } catch (Exception) { }
            if (Program.args.benchmark = b) { showStat(); }
            Application.Exit();
            return;}

        private void GUI_FormClosing(object sender, FormClosingEventArgs e) { exit(); }

        private void ButtonStat_Click(object sender, EventArgs e){ showStat(); }
        public static void showStat() {
            if (Program.args.benchmark) { MessageBox.Show("Total sent:" + myKafka.totalSuccessSent + "; pending in queue:" + ReceiverTool.messageQueue.Count); }
            else MessageBox.Show("The argument \"benchmark\" must be set to true to enable this feature.");
        }
    }
}
