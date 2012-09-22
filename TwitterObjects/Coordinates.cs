using System.Runtime.Serialization;

namespace TwitterStreamClient.TwitterObjects
{
	[DataContract]
	public class Coordinates
	{
		[DataMember]
		public float[] coordinates;
		[DataMember]
		public string type;
	}
}

	

