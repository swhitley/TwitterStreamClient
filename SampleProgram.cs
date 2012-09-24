using System;

namespace TwitterStreamClient
{
	public class SampleProgram
	{
		static string user = "user";
		static string pass = "pass";
		static bool isEncrypted = false;
		static int numberOfTweets = 10;
		
		static void Main (string[] args)
		{
			// create a string with your credentials
			// if you provide an encrypted password, set the flag
			TwitterStream stream = new TwitterStream (user, pass, isEncrypted);
			
			/*
			// Optional: Add a filter. If filtering,
			// the Twitter API requires to specify at least 1 parameter
			TwitterStreamFilter filter = new TwitterStreamFilter ();
			string[] keywords = { "twitter", "pizza"};
			filter.setTrackingKeywords (keywords);
			// to remove the filter, set it to null
			stream.SetStreamFilter (filter);
			*/
			
			stream.StartAsyncStream ();
			
			while (stream.IsStreamRunning() && (numberOfTweets != 0)) {
				Status status = stream.GetBlockingNextStatus ();
				
				// make sure to handle deleted statuses which appear in
				// public stream
				if (status.text != null) {
					Console.Write ("@" + status.user.screen_name + ": ");
					Console.WriteLine (status.text);
					
					numberOfTweets--;
				}
			}
			
			// make sure to clean up
			stream.StopAsyncStream ();
			
		}
	}
}

