using System;
using System.Text;
using System.Web;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Configuration;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Messaging;
using System.Security.Cryptography;
using TwitterStreamClient.TwitterObjects;

// TODO: some of the JSON messages can't be read correctly
// so they throw an exception when they cant be parsed

namespace TwitterStreamClient
{
    public class TwitterStream
    {
		private Logger logger = new Logger();
		private string username = null;
		private string password = null;
		private string streamUrl = null;
		
		private static bool debugging = false;
		
		private bool isFiltered = false;
		private TwitterStreamFilter filter = null;
		
		private bool useMessageQueue;
		private MessageQueue queue;
		public string messagePath = @"./private$/Twitter";
		
		public const string filteredEndpoint = "https://stream.twitter.com/1.1/statuses/filter.json";
		public const string sampleEndpoint = "https://stream.twitter.com/1.1/statuses/sample.json";
		
		private HttpWebRequest webRequest = null;
		private HttpWebResponse webResponse = null;
		private StreamReader responseStream = null;
		
		private Encoding defaultEncoding = System.Text.Encoding.GetEncoding ("utf-8");
		
		private Thread backgroundThread = null;
		private bool abortFlag = false;
		private SynchronizedQueue<string> jsonQueue;
		
		
		/* Obtain parameters from App.config */
		public TwitterStream ()
		{
			username = ConfigurationManager.AppSettings ["twitter_username"];
			password = ConfigurationManager.AppSettings ["twitter_password"];
			if (ConfigurationManager.AppSettings ["twitter_password_encrypted"] == "true") {
				password = Common.Decrypt (password);
			} 
			
			//Twitter Streaming API
			streamUrl = ConfigurationManager.AppSettings ["stream_url"];
			
		}
		
		/* 
		 * Constructor - Makes stream with no filter by default
		 * and no IPC message queue
		 * 
		 * To use with a message queue for IPC, call SetUseQueue(true)
		 * 
		 */ 
		public TwitterStream (string user, string pass, bool isEncrypted)
		{
			if (isEncrypted) {
				password = Common.Decrypt (pass);
			} else {
				password = pass;
			}			
			
			username = user;
			password = pass;	
			
			jsonQueue = new SynchronizedQueue<string> ();
			
			SetFiltered(false);
			
		}
		
		/* 
		 * SetDebugging(bool debug) 
		 * 
		 * if true, will output json messages to the console
		 */ 
		public void SetDebugging (bool debug)
		{
			debugging = debug;
		}
		
		/*
		 * SetFiltered(bool filter)
		 * 
		 * If set to true, changes the Twitter API endpoint
		 * to the filtered url (filter.json)
		 * 
		 * If set to false, uses the unfiltered url (sample.json)
		 */ 
		private void SetFiltered (bool filter)
		{
			isFiltered = filter;
			
			if (isFiltered) {
				streamUrl = filteredEndpoint;
			} else {
				streamUrl = sampleEndpoint;
			}
		}
		
		/*
		 * SetStreamFilter(TwitterStreamFilter filter)
		 * 
		 * Filter object contains all filters that can be
		 * used with the Twitter API
		 * 
		 */ 
		public void SetStreamFilter (TwitterStreamFilter filter)
		{
			this.filter = filter;
			
			SetFiltered(filter != null);
		}
	
		/*
		 * SetUseQueue(bool enabled)
		 * 
		 * Creates the MessageQueue if one doesn't already exist
		 * with messagePath endpoint
		 * 
		 */ 
		public void SetUseQueue (bool enabled)
		{
			if (useMessageQueue != enabled) {
				if (MessageQueue.Exists (messagePath)) {
					queue = new MessageQueue (messagePath);
				} else {
					queue = MessageQueue.Create (messagePath);
				}
				useMessageQueue = enabled;
			} 

		}
		
		/* StartAsyncStream
		 * 
		 * If useMessageQueue == true, pipes all JSON messages
		 * to MessageQueue
		 * 
		 * If useMessageQueue == false, pipes all JSON messages to
		 * thread-safe queue
		 * 
		 * 
		 */
		public void StartAsyncStream ()
		{
			if (!useMessageQueue) {
				if (backgroundThread != null) {
					if (backgroundThread.IsAlive) {
						Console.WriteLine ("Stream already started." +
					"Close other stream before continuing.");
					}
				}
			}
			
			PrepareRequest ();
			
			backgroundThread = new Thread (new ThreadStart (ProcessStream));
			backgroundThread.Start ();
						
		}
		
		/*
		 * IsStreamRunning
		 * 
		 * returns whether or not background thread is currently running
		 */ 
		public bool IsStreamRunning ()
		{
			if (backgroundThread != null) {
				return (backgroundThread.IsAlive);
			} else {
				return false;
			}
			
			
		}
		
		public void StopAsyncStream ()
		{
			if (backgroundThread != null) {
				if (backgroundThread.IsAlive) {
					abortFlag = true;
				}
			}
				
		}
		
		
		
		private void PrepareRequest ()
		{
			//Connect
			webRequest = (HttpWebRequest)WebRequest.Create (streamUrl);
			webRequest.Credentials = new NetworkCredential (username, password);
			webRequest.Timeout = -1;

			if (filter != null) {
				string postParams = filter.ToString ();
				if (postParams.Length > 0) {
					webRequest.Method = "POST";
					webRequest.ContentType = "application/x-www-form-urlencoded";
					
					if (debugging) {
						Console.WriteLine (postParams);
					}

					byte[] _twitterTrack = defaultEncoding.GetBytes (postParams);

					webRequest.ContentLength = _twitterTrack.Length;
					Stream _twitterPost = webRequest.GetRequestStream ();
					_twitterPost.Write (_twitterTrack, 0, _twitterTrack.Length);
					_twitterPost.Close ();
				}
			}
			
		}
		
		/*
		 * GetBlockingNextStatus
		 * returns next twitter status processed from message
		 * 
		 * NOTE: this method will block until next status
		 * is received
		 * 
		 * Public Stream also returns deleted status, and
		 * these are still processed as status, so most fields
		 * will be null, but the object won't be null.
		 * 
		 * TODO: implement a timeout so we don't block forever
		 */ 
		public Status GetBlockingNextStatus ()
		{
			Status status;
			string message;
			// check if there are any messages
			message = jsonQueue.RemoveItem ();
			
			// process message
			status = MessageProcess (message);
			//Console.WriteLine ("Status received");
			
			return status;

		}
		
		public bool IsStatusAvailable()
		{
			return jsonQueue.IsItemAvailable();
		}
		

		/*
		 * ProcessStream
		 * 
		 * Starts WebRequest and opens a stream for the response
		 * Implements backoff for HTTP errors and Network errors
		 * 
		 * If MessageQueue is used, pipes all JSON messages to MessageQueue
		 * else pipes all JSON to thread-safe queue
		 */ 
        private void ProcessStream ()
		{
			int wait = 250;
			string jsonText = "";
			
			try {
				while (!abortFlag) {
					try {	
						// perform the request
						webResponse = (HttpWebResponse)webRequest.GetResponse ();
						responseStream = new StreamReader (webResponse.GetResponseStream (), defaultEncoding);
						if (debugging) {
							Console.WriteLine ("Twitter stream started");
						}
						

						//Read the stream.
						while (!abortFlag) {
							jsonText = responseStream.ReadLine ();
							
							// post the message to the endpoint
							if (useMessageQueue) {
								Message message = new Message (jsonText);
								queue.Send (message);
							} else {
								jsonQueue.AddItem (jsonText);
							}

							//Success
							wait = 250;

							//Write Status
							if (debugging) {
								Console.WriteLine ("Stream:");
								Console.Write (jsonText);
							}

						}

					} catch (WebException ex) {
						Console.WriteLine (ex.Message);
						Console.WriteLine (ex.StackTrace);
						logger.append (ex.Message, Logger.LogLevel.ERROR);
						
						if (ex.Status == WebExceptionStatus.ProtocolError) {
							//-- From Twitter Docs -- 
							//When a HTTP error (> 200) is returned, back off exponentially. 
							//Perhaps start with a 10 second wait, double on each subsequent failure, 
							//and finally cap the wait at 240 seconds. 
							//Exponential Backoff
							if (wait < 10000) {
								wait = 10000;
							} else {
								if (wait < 240000) {
									wait = wait * 2;
								}
							}
							
						} else {
							//-- From Twitter Docs -- 
							//When a network error (TCP/IP level) is encountered, back off linearly. 
							//Perhaps start at 250 milliseconds and cap at 16 seconds.
							//Linear Backoff
							if (wait < 16000) {
								wait += 250;
							}

						}
					} catch (Exception ex) {
						Console.WriteLine (ex.Message);
						Console.WriteLine (ex.StackTrace);
						logger.append (ex.Message, Logger.LogLevel.ERROR);
						
						if (webRequest != null) {
							webRequest.Abort ();
						}
						if (responseStream != null) {
							responseStream.Close ();
							responseStream = null;
						}

						if (webResponse != null) {
							webResponse.Close ();
							webResponse = null;
						}
						Console.WriteLine ("Waiting: " + wait);
						Thread.Sleep (wait);
					}
				}
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);
				logger.append (ex.Message, Logger.LogLevel.ERROR);
			}
			abortFlag = false;
			if (debugging) {
				Console.WriteLine ("Thread finished");
			}
		}
		
		/*
		 * QueueRead
		 * 
		 * Prints the next JSON message stored in
		 * MessageQueue
		 * 
		 * If MessageQueue is not used, this method won't do anything
		 * 
		 */
        public void QueueRead ()
		{
			MessageQueue q;
			string multiThread = ConfigurationManager.AppSettings ["multithread"];
			Logger logger = new Logger ();
				
			try {
				if (MessageQueue.Exists (messagePath)) {
					q = new MessageQueue (messagePath);
				} else {
					Console.WriteLine ("Queue does not exists.");
					return;
				}

				while (true) {
					Message message;
					try {
						message = q.Receive ();
						message.Formatter =
                            new XmlMessageFormatter (new String[] { "System.String" });
						if (multiThread == "true") {
							ThreadPool.QueueUserWorkItem (MessageProcess, message);
						} else {
							MessageProcess (message);
						}
					} catch {
						continue;
					}
				}
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				logger.append (ex.Message, Logger.LogLevel.ERROR);
			}
		}
        
		private Status MessageProcess (string message)
		{
			Status status = new Status ();
			Logger logger = new Logger ();
			DataContractJsonSerializer json = new DataContractJsonSerializer (status.GetType ());

			try {
				byte[] byteArray = Encoding.UTF8.GetBytes (message);
				MemoryStream stream = new MemoryStream (byteArray);
                
				//TODO:  Check for multiple objects.
				status = json.ReadObject (stream) as Status;
				
				if (debugging) {
					Console.WriteLine ("MessageProcess:");
					Console.WriteLine (message);
				}

			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);
				
				logger.append (ex.Message, Logger.LogLevel.ERROR);
				
			}
			
			return status;
		}
        
        private void MessageProcess (object objMessage)
		{
			
			Status status = new Status ();
			Logger logger = new Logger ();
			DataContractJsonSerializer json = new DataContractJsonSerializer (status.GetType ());

			try {

				Message message = objMessage as Message;

				byte[] byteArray = Encoding.UTF8.GetBytes (message.Body.ToString ());
				MemoryStream stream = new MemoryStream (byteArray);
                
				//TODO:  Check for multiple objects.
				status = json.ReadObject (stream) as Status;
				
				if (debugging) {
					Console.WriteLine ("MessageProcess:");
					Console.WriteLine (message.Body.ToString ());
				}




			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);
				logger.append (ex.Message, Logger.LogLevel.ERROR);
			}
			
		}
    }
}