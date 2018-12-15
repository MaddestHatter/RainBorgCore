using Microsoft.Data.Sqlite;

namespace RainBorg
{
    public partial class Wallet
    {

	/*    
	// Gets a user's balance from the database
        public static decimal GetBalance(ulong UID)
        {
            // Create Sql command
            SqliteCommand Command = new SqliteCommand("SELECT balance FROM users WHERE uid = @uid", Database);
            Command.Parameters.AddWithValue("uid", UID);

            // Execute command
            using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read())
                    return Reader.GetDecimal(0);

            // Could not find uid
            return -666;
        }
	*/
        
	// Gets a user's balance from the database
        public static decimal GetBalance()
        {
           //string PaymentId =  RainBorg.botPaymentId;
	    
           using (SqliteConnection Connection = new SqliteConnection("Data Source=" + RainBorg.databaseFileTipBot))
           {
              Connection.Open();

	   	 // Create Sql command
           	 SqliteCommand Command = new SqliteCommand("SELECT balance FROM users WHERE paymentid = @paymentid", Connection);
            	 //Command.Parameters.AddWithValue("paymentid", PaymentId.ToUpper());
		Command.Parameters.AddWithValue("paymentid", RainBorg.botPaymentId.ToUpper());
            	// Execute command
            	using (SqliteDataReader Reader = Command.ExecuteReader())
                if (Reader.Read()) 
		{
		    	return Reader.GetDecimal(0);
		}
		 // Could not find pid
            	else return -666;
           }
         
    	 }	

    }
} 
