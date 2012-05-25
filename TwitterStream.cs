using System;
using System.Text;
using System.IO;
using System.Net;
using System.Configuration;
using System.Threading;
using System.Diagnostics;
using System.Web;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Messaging;
using System.Security.Cryptography;

namespace TwitterStreamClient
{
    public class TwitterStream
    {
        public void Stream2Queue()
        {
            string username = ConfigurationManager.AppSettings["twitter_username"];
            string password = ConfigurationManager.AppSettings["twitter_password"];
            if (ConfigurationManager.AppSettings["twitter_password_encrypted"] == "true")
            {
                password = Common.Decrypt(password);
            }
            //Twitter Streaming API
            string stream_url = ConfigurationManager.AppSettings["stream_url"];

            HttpWebRequest webRequest = null;
            HttpWebResponse webResponse = null;
            StreamReader responseStream = null;
            MessageQueue q = null;
            string useQueue = ConfigurationManager.AppSettings["use_queue"];
            string postparameters = (ConfigurationManager.AppSettings["track_keywords"].Length == 0 ? string.Empty : "&track=" + ConfigurationManager.AppSettings["track_keywords"]) +
                (ConfigurationManager.AppSettings["follow_userid"].Length == 0 ? string.Empty : "&follow=" + ConfigurationManager.AppSettings["follow_userid"]) +
                (ConfigurationManager.AppSettings["location_coord"].Length == 0 ? string.Empty : "&locations=" + ConfigurationManager.AppSettings["location_coord"]);


            int wait = 250;
            string jsonText = "";

            Logger logger = new Logger();


            try
            {
                //Message Queue
                if (useQueue == "true")
                {
                    if (MessageQueue.Exists(@".\private$\Twitter"))
                    {
                        q = new MessageQueue(@".\private$\Twitter");
                    }
                    else
                    {
                        q = MessageQueue.Create(@".\private$\Twitter");
                    }
                }

                while (true)
                {

                    try
                    {
                        //Connect
                        webRequest = (HttpWebRequest)WebRequest.Create(stream_url);
                        webRequest.Credentials = new NetworkCredential(username, password);
                        webRequest.Timeout = -1;

                        Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                        if (postparameters.Length > 0)
                        {
                            webRequest.Method = "POST";
                            webRequest.ContentType = "application/x-www-form-urlencoded";

                            byte[] _twitterTrack = encode.GetBytes(postparameters);

                            webRequest.ContentLength = _twitterTrack.Length;
                            Stream _twitterPost = webRequest.GetRequestStream();
                            _twitterPost.Write(_twitterTrack, 0, _twitterTrack.Length);
                            _twitterPost.Close();
                        }

                        webResponse = (HttpWebResponse)webRequest.GetResponse();
                        responseStream = new StreamReader(webResponse.GetResponseStream(), encode);

                        //Read the stream.
                        while (true)
                        {
                            jsonText = responseStream.ReadLine();
                            //Post each message to the queue.
                            if (useQueue == "true")
                            {
                                Message message = new Message(jsonText);
                                q.Send(message);
                            }

                            //Success
                            wait = 250;

                            //Write Status
                            Console.Write(jsonText);

                        }
                        //Abort is needed or responseStream.Close() will hang.
                        webRequest.Abort();
                        responseStream.Close();
                        responseStream = null;
                        webResponse.Close();
                        webResponse = null;

                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine(ex.Message);
                        logger.append(ex.Message, Logger.LogLevel.ERROR);
                        if (ex.Status == WebExceptionStatus.ProtocolError)
                        {
                            //-- From Twitter Docs -- 
                            //When a HTTP error (> 200) is returned, back off exponentially. 
                            //Perhaps start with a 10 second wait, double on each subsequent failure, 
                            //and finally cap the wait at 240 seconds. 
                            //Exponential Backoff
                            if (wait < 10000)
                            {
                                wait = 10000;
                            }
                            else
                            {
                                if (wait < 240000)
                                {
                                    wait = wait * 2;
                                }
                            }
                        }
                        else
                        {
                            //-- From Twitter Docs -- 
                            //When a network error (TCP/IP level) is encountered, back off linearly. 
                            //Perhaps start at 250 milliseconds and cap at 16 seconds.
                            //Linear Backoff
                            if (wait < 16000)
                            {
                                wait += 250;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        logger.append(ex.Message, Logger.LogLevel.ERROR);
                    }
                    finally
                    {
                        if (webRequest != null)
                        {
                            webRequest.Abort();
                        }
                        if (responseStream != null)
                        {
                            responseStream.Close();
                            responseStream = null;
                        }

                        if (webResponse != null)
                        {
                            webResponse.Close();
                            webResponse = null;
                        }
                        Console.WriteLine("Waiting: " + wait);
                        Thread.Sleep(wait);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.append(ex.Message, Logger.LogLevel.ERROR);
                Console.WriteLine("Waiting: " + wait);
                Thread.Sleep(wait);
            }
        }
        
        
        public void QueueRead()
        {
            MessageQueue q;
            string multiThread = ConfigurationManager.AppSettings["multithread"];
            Logger logger = new Logger();

            try
            {
                if (MessageQueue.Exists(@".\private$\Twitter"))
                {
                    q = new MessageQueue(@".\private$\Twitter");
                }
                else
                {
                    Console.WriteLine("Queue does not exists.");
                    return;
                }

                while (true)
                {
                    Message message;
                    try
                    {
                        message = q.Receive();
                        message.Formatter =
                            new XmlMessageFormatter(new String[] { "System.String" });
                        if (multiThread == "true")
                        {
                            ThreadPool.QueueUserWorkItem(MessageProcess, message);
                        }
                        else
                        {
                            MessageProcess(message);
                        }
                    }
                    catch { continue; }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.append(ex.Message, Logger.LogLevel.ERROR);
            }
        }
        
        
        public void MessageProcess(object objMessage)
        {
            status status = new status();
            Logger logger = new Logger();
            DataContractJsonSerializer json = new DataContractJsonSerializer(status.GetType());

            try
            {
                Message message = objMessage as Message;

                byte[] byteArray = Encoding.UTF8.GetBytes(message.Body.ToString());
                MemoryStream stream = new MemoryStream(byteArray);
                
                //TODO:  Check for multiple objects.
                status = json.ReadObject(stream) as status;

                Console.WriteLine(message.Body.ToString());

                //TODO: Store the status object
                DataStore.Add(status);




            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.append(ex.Message, Logger.LogLevel.ERROR);
            }
        }
    }
}