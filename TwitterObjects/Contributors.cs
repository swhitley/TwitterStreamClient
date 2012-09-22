using System.Runtime.Serialization;

namespace TwitterStreamClient.TwitterObjects
{
	[DataContract]
	public class Contributor
	{
		[DataMember]
		public string id;
		[DataMember]
		public string id_str;
		[DataMember]
		public string screen_name;
	}
}