using System;
using System.Runtime.Serialization;

namespace TwitterStreamClient.TwitterObjects
{
	[DataContract]
	public class Media
	{
		[DataMember]
		public string display_url;
		[DataMember]
		public string expanded_url;
		[DataMember]
		public string id_str;
		[DataMember]
		public int[] indices;
		[DataMember]
		public string media_url;
		[DataMember]
		public string media_url_https;
		[DataMember]
		public Sizes sizes;
		[DataMember]
		public string source_status_id_str;
		[DataMember]
		public string type;
		[DataMember]
		public string url;
	}
}

