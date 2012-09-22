using System;
using System.Runtime.Serialization;

namespace TwitterStreamClient.TwitterObjects
{
	[DataContract]
	public class TwitterUrl
	{
		[DataMember]
		public string display_url;
		
		[DataMember]
		public string expanded_url;
		
		[DataMember]
		public int[] indices;
		
		[DataMember]
		public string url;
		
	}
}

