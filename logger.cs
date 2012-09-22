using System;
using System.IO;
using System.Net.Mail;
using System.Configuration;
using System.Resources;
using System.Reflection;
using System.Diagnostics;

namespace TwitterStreamClient
{  
	public class Logger 
	{
		private static string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
		private string _subDirectory = null;
		private string _logDirectory = null;
		private string _logFile = null;
		private DateTime dt;
        public enum LogLevel { OFF, ERROR, WARNING, INFO, ALL };
		public enum FileTypes {Log, ReadLog, MailLog};
		private FileTypes _fileType = FileTypes.Log;


		public FileTypes FileType
		{
			get { return _fileType;}
			set 
			{ 
				_fileType = value;
				switch(value)
				{
					case FileTypes.Log:
						this.SubDirectory = "logs";
						break;
					case FileTypes.ReadLog:
						this.SubDirectory = "readlog";
						break;
					case FileTypes.MailLog:
						this.SubDirectory = "maillog";
						break;
				}
			}
		}

		public string SubDirectory
		{
			get
			{ 
				if(_subDirectory == null)
				{
					 _subDirectory = "\\logs\\";
				}
				return _subDirectory;
			}
			set
			{
				_subDirectory = "\\" + value + "\\";
				_logDirectory = null;
				_logFile = null;
			}
		}

		public string LogFile
		{
			get
			{
				if(_logFile == null)
				{
					_logFile = this.LogDirectory + dt.ToString("yyyyMMdd") + ".log";
				}

				return _logFile; 
			}
		}
		public string LogDirectory
		{
			get 
			{
				if(_logDirectory == null)
				{
					dt = DateTime.Now;
					_logDirectory = _baseDirectory + this.SubDirectory +  dt.ToString("yyyyMMdd") + "\\";
					if (!Directory.Exists(_logDirectory))
					{
						Directory.CreateDirectory(_logDirectory);
					}
				}
				return _logDirectory;
			}
		}
		public Logger()
		{
		}
		public void emailLog()
		{
			MailMessage objMailMessage = new MailMessage();
			objMailMessage.Body = "Please review the attached log file.";
			objMailMessage.From = new MailAddress(ConfigurationManager.AppSettings["EmailFrom"]);
			objMailMessage.To.Add(new MailAddress(ConfigurationManager.AppSettings["EmailTo"]));
			objMailMessage.Subject = "-- Log Message --";
			if (File.Exists(this.LogFile)) 
			{
				objMailMessage.Attachments.Add(new Attachment(this.LogFile));
			}
            System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient();
			smtp.Host = ConfigurationManager.AppSettings["SMTPServer"];	
			smtp.Send( objMailMessage );
		}
		public void backup()
		{

			int intCtr = 1;
			string filePath1 = this.LogFile;
			string filePath2 = this.LogDirectory + dt.ToString("yyyyMMdd") + intCtr.ToString("0000") + ".log";
			while(File.Exists(filePath2) && intCtr < 99999) 
			{
				intCtr++;
				filePath2 = this.LogDirectory + dt.ToString("yyyyMMdd") + intCtr.ToString("0000") + ".log";
			}
			if (File.Exists(filePath1))
			{
				File.Move(filePath1,filePath2);
			}
		}
		public static string MethodName()
		{
			StackTrace stackTrace = new StackTrace();
			StackFrame stackFrame = stackTrace.GetFrame(1);
			MethodBase methodBase = stackFrame.GetMethod();
			return methodBase.ReflectedType.Name + "." + methodBase.Name;
		}
		public void append(String message, LogLevel level) 
		{
			int logLevel = (int) level;
            int configLogLevel = 0;

			string strLogLevel = ConfigurationManager.AppSettings["loglevel"];
            configLogLevel = (int)LogLevel.OFF;
            if (!String.IsNullOrEmpty(strLogLevel))
            {
                configLogLevel = (int) Enum.Parse(typeof(LogLevel), strLogLevel);
            }

            if (configLogLevel >= (int) level) 
			{
				if (!File.Exists(this.LogFile)) 
				{
					FileStream fs = File.Create(this.LogFile);
					fs.Close();
				}
				try 
				{
					DateTime dtLog = DateTime.Now;
					StreamWriter sw = File.AppendText(this.LogFile);
					sw.WriteLine(dtLog.ToString("s") + "\t" + message);
					sw.Flush();
					sw.Close();
				} 
				catch (Exception e) 
				{
					Console.WriteLine(e.Message.ToString());
				}
			}
		}
	}
}
