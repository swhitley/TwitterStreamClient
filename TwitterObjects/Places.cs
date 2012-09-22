using System;
using System.Runtime.Serialization;
	
namespace TwitterStreamClient.TwitterObjects
{
	[DataContract]
	public class Places
	{
		[DataMember]
		public PlacesAttributes attributes;
		
		[DataMember]
		public BoundingBox bounding_box;
		
		[DataMember]
		public string country;
		
		[DataMember]
		public string country_code;
		
		[DataMember]
		public string full_name;
		
		[DataMember]
		public string id;
		
		[DataMember]
		public string name;
		
		[DataMember]
		public string place_type;
		
		[DataMember]
		public string url;
	}
	
	[DataContract]
	public class BoundingBox
	{
		[DataMember]
		public float[][][][] coordinates;
		
		[DataMember]
		public string type;
	}
	
	[DataContract]
	public class PlacesAttributes
	{
		[DataMember]
		public string street_address;
		
		[DataMember]
		public string locality;
		
		[DataMember]
		public string region;
		
		[DataMember]
		public string iso3;
		
		[DataMember]
		public string postal_code;
		
		[DataMember]
		public string phone;
		
		[DataMember]
		public string twitter;
		
		[DataMember]
		public string url;
		
		/*
		 * TODO: Include this member
		[DataMember]
		public string app:id;
		*/
	}
}

