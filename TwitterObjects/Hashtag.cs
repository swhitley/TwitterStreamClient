using System;
using System.Runtime.Serialization;

namespace TwitterStreamClient.TwitterObjects
{
	[DataContract]
	public class Hashtag
	{
		[DataMember]
		public int[] indices;
		
		[DataMember]
		public string text;
	}
}

