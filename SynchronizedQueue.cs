using System;
using System.Collections.Generic;
using System.Threading;
using TwitterStreamClient;
using TwitterStreamClient.TwitterObjects;
#region License
/*
 Copyright (c) 2010 Danny Perez

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
 
 */
#endregion

namespace TwitterStreamClient
{
	public class SynchronizedQueue<T>
	{
		string tag = "SMQ: ";
		
		readonly object queueLock = new object();
		
		Queue<T> queue;
		
		// if flag = true, new data in queue
		// if flag = false, no new data
		bool readerFlag = false;
		
		Logger logger = new Logger();
		
		int timeout = 250;
		
		public SynchronizedQueue ()
		{
			queue = new Queue<T> ();	
			
		}
		
		public bool IsItemAvailable ()
		{
			int count;
			// grab the data from the queue
			lock (queueLock) {
				// if producer is stuffing the queue
				if (!readerFlag) {
					try {
						// wait for it to finish
						Monitor.Wait (queueLock);
					} catch (SynchronizationLockException e) {
						Console.WriteLine (tag + e.Message);
						Console.WriteLine (e.StackTrace);
						logger.append (e.Message, Logger.LogLevel.ERROR);
					} catch (ThreadInterruptedException e) {
						Console.WriteLine (tag + e.Message);
						Console.WriteLine (e.StackTrace);
						logger.append (e.Message, Logger.LogLevel.ERROR);
					}
				}
					
				count = queue.Count;
					
				readerFlag = false;
				Monitor.Pulse (queueLock);
			}
			
			
			
			return (count > 0);
		}
		
		public T RemoveItem ()
		{
			T msg = default(T);
			// grab the data from the queue
			lock (queueLock) {
				// if producer is stuffing the queue
				if (!readerFlag) {
					try {
						// wait for it to finish
						Monitor.Wait (queueLock);
					} catch (SynchronizationLockException e) {
						Console.WriteLine (tag + e.Message);
						Console.WriteLine (e.StackTrace);
						logger.append (e.Message, Logger.LogLevel.ERROR);
					} catch (ThreadInterruptedException e) {
						Console.WriteLine (tag + e.Message);
						Console.WriteLine (e.StackTrace);
						logger.append (e.Message, Logger.LogLevel.ERROR);
					}
				}
					
				// get the message
				if (queue.Count > 0) {
						
					msg = queue.Dequeue ();
					
				}
					
				readerFlag = false;
				Monitor.Pulse (queueLock);
			}
			
			return msg;
		}
		
		public void AddItem (T item)
		{
					
			// enter critical region
			lock (queueLock) {
				// if client is currently removing, wait
				if (readerFlag) {
					try {
						Monitor.Wait (queueLock);
					} catch (SynchronizationLockException e) {
						Console.WriteLine (tag + e.Message);
						Console.WriteLine (e.StackTrace);
						logger.append (e.Message, Logger.LogLevel.ERROR);
					} catch (ThreadInterruptedException e) {
						Console.WriteLine (tag + e.Message);
						Console.WriteLine (e.StackTrace);
						logger.append (e.Message, Logger.LogLevel.ERROR);
					}
				}
				
				// else stuff data in the queue
				readerFlag = true;
				queue.Enqueue (item);
				
				// notify other thread that we're done
				Monitor.Pulse (queueLock);
				
				// exit critical region
			}
			
		}
	}
}

