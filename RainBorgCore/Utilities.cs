using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace RainBorg
{
    public partial class RainBorg
    {
        internal static decimal GetBalanceORG()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string dl = client.DownloadString(balanceUrl);
                    JObject j = JObject.Parse(dl);
                    return (decimal)j["balance"];
                }
            }
            catch
            {
                return -1;
            }
        }

 	// New Balance Method via Request	
	internal static decimal GetBalance()
        {
        //  public static JObject RPC(string Host, int Port, string Method, JObject Params = null, string Password = null)

        // Get Balance from Service
	//JObject Result = Request.RPC(daemonHost, daemonPort, "getBalance", new JObject{["address"] = botAddress }, daemonPassword);
	// if (Result.Count < 1 || Result.ContainsKey("error")) return -1;
        // Return current supply dividet by CoinUnits
        //        return (decimal)Result["availableBalance"] / 10000;
	
		decimal balance = Wallet.GetBalance();
		return balance;	
        }



        public static decimal Floor(decimal Input)
        {
            var r = Convert.ToDecimal(Math.Pow(10, decimalPlaces));
            return Math.Floor(Input * r) / r;
        }

        public static string Format(decimal Input)
        {
            Input = Floor(Input);
            string f = "{0:#,##0.#############}";
            return string.Format(f, Input);
        }
        public static string Format(double Input)
        {
            decimal I = Floor((decimal)Input);
            string f = "{0:#,##0.#############}";
            return string.Format(f, I);
        }

        // Log
        public static void Log(string Source, string Message, params object[] Objects)
        {
            // Log to console
            Console.WriteLine("{0} {1}\t{2}", DateTime.Now.ToString("HH:mm:ss"), Source, string.Format(Message, Objects));

            // If log file is specified
            if (!string.IsNullOrEmpty(logFile))
                using (StreamWriter w = File.AppendText(logFile))
                    w.WriteLine(string.Format("{0} {1} {2}\t{3}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(), Source, string.Format(Message, Objects)));
        }
    }
}
