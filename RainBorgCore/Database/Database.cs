namespace RainBorg
{
    partial class Database
    {
        internal static void Load()
        {
            // Load operators
            Operators.Load();

            // Load stats
            Stats.Load();

            // Load blacklist
            Blacklist.Load();

            // Load optedout
            OptedOut.Load();
	    
	    //Load Wallet
	    //Wallet.Load();
        
	    //Load Stats
	    Stats.Load();	
	
	}
    }
}
