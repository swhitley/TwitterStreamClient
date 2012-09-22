using System;
using System.Runtime.Serialization;

namespace TwitterStreamClient.TwitterObjects
{
	[DataContract]
	public class Entities
	{
		[DataMember]
		public Hashtag[] hashtags;
		
		[DataMember]
		public Media[] media;
		
		[DataMember]
		public TwitterUrl urls;
	}
}

