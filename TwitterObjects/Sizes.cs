using System;
using System.Runtime.Serialization;

namespace TwitterStreamClient
{
	[DataContract]
	public class Sizes
	{
		[DataMember]
		public int h;
		[DataMember]
		public int w;
		[DataMember]
		public string resize;
		
		public const string RS_FIT = "fit";
		public const string RS_CROP = "crop";
	}
}

