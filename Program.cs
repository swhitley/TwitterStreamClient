using System;
#region License
/*
 Copyright (c) 2010 Whitley Media

 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:

 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.
 
 Acknowledgements - Gary Short (For the MSMQ code) - http://community.devexpress.com/blogs/garyshort/archive/2010/03/16/how-to-drink-at-the-twitter-firehose-and-not-drown.aspx
                    Joe Vivona (Track additions)
 
 */
#endregion


namespace TwitterStreamClient
{
    class Program
    {
        static void Main(string[] args)
        {
            TwitterStream stream = new TwitterStream();
            Logger logger = new Logger();
            string arg1 = "";

            try
            {
                if (args.Length > 0)
                {
                    arg1 = args[0];
                }

                switch (arg1)
                {
                    case "/?":
                        Console.WriteLine("Twitter Stream Client.\n");
                        Console.WriteLine("twitterstreamclient [/p]\n");
                        Console.WriteLine("[]\t\t\tProcess the Twitter Stream to a Message Queue.");
                        Console.WriteLine("/p\t\t\tProcess the Message Queue.");
                        Console.WriteLine("/encrypt password\tEncrypt a Twitter password for the config file.\n");
                        Console.WriteLine("\nPress any key to continue . . .");
                        Console.ReadKey();
                        break;
                    case "/encrypt":
                        if (args.Length > 1)
                        {
                            Console.WriteLine(Common.Encrypt(args[1]));
                        }
                        break;
                    case "/p":
                        stream.QueueRead();
                        break;
                    default:
                        stream.Stream2Queue();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.append(ex.Message, Logger.LogLevel.ERROR);
            }
        }
    }
}
