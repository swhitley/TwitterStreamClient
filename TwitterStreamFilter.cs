using System;
using System.Configuration;
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
	public class TwitterStreamFilter
	{
		
		string track = "";
		string follow = "";
		string locations = "";
		
		public TwitterStreamFilter ()
		{
		}
		
		public void setConfigTrackingKeywords ()
		{
			track += "&track=";
			string keyword = (ConfigurationManager.AppSettings ["track_keywords"].Length == 0 ? string.Empty : "&track=" + ConfigurationManager.AppSettings ["track_keywords"]);
			if (keyword.Length != 0) {
				track += keyword;
			}
				
			keyword = (ConfigurationManager.AppSettings ["follow_userid"].Length == 0 ? string.Empty : "&follow=" + ConfigurationManager.AppSettings ["follow_userid"]);
			if (keyword.Length != 0) {
				track += keyword;
			}
			
			keyword = (ConfigurationManager.AppSettings ["location_coord"].Length == 0 ? string.Empty : "&locations=" + ConfigurationManager.AppSettings ["location_coord"]);
			if (keyword.Length != 0) {
				track += keyword;
			}
		}
		
		public void setTrackingKeywords (string[] keywords)
		{
			if (keywords != null) {
				track += "&track=";
				
				for (int i = 0; i < keywords.Length; i++) {
					track += keywords [i];
					if (i != keywords.Length - 1) {
						track += ",";
					}
						
				}
			}
		}
		
		public void setFollowIds (int[] userIds)
		{
			follow += "&follow=";
			
			for (int i = 0; i < userIds.Length; i++) {
				follow += userIds [i].ToString ();
				if (i != (userIds.Length - 1)) {
					follow += ",";
				}
			
			}
			
		}
		
		/* boundingBox should only have 4 elements */
		public void setLocations (float[] boundingBox)
		{
			if (boundingBox.Length == 4) {
				Console.WriteLine("Incorrect number of coordinates in bounding box");
			}
			
			locations += "&locations=";
			locations += boundingBox [0].ToString () + ",";
			locations += boundingBox [1].ToString () + ",";
			locations += boundingBox [2].ToString () + ",";
			locations += boundingBox [3].ToString ();
				
		}
		
		public override string ToString ()
		{
			string filterText = "";
			if (track.Length > 0) {
				filterText += track;
			}
			
			if (follow.Length > 0) {
				filterText += follow;
			}
			
			if (locations.Length > 0) {
				filterText += locations;
			}
			
			return filterText;
		}
		
		
	}
	
	
}

