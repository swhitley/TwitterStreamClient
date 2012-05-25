using System;
using System.Runtime.Serialization;
using System.Globalization;

namespace TwitterStreamClient
{
//<status>
//<created_at>Tue Apr 07 22:52:51 +0000 2009</created_at>
//<id>1472669360</id>
//<text>At least I can get your humor through tweets. RT @abdur: I don't mean this in a bad way, but genetically speaking your a cul-de-sac.</text>
//<source><a href="http://www.tweetdeck.com/">TweetDeck</a></source>
//<truncated>false</truncated>
//<in_reply_to_status_id></in_reply_to_status_id>
//<in_reply_to_user_id></in_reply_to_user_id>
//<favorited>false</favorited>
//<in_reply_to_screen_name></in_reply_to_screen_name>
//<geo/>
//<contributors/>
//</status>
    
    [DataContract]
    public class status
    {
        public DateTimeOffset created_at_dt;
        [DataMember]
        public string created_at
        {
            get { return created_at_dt.ToString("ddd MMM dd HH:mm:ss zzz yyyy"); }
            set
            {
                created_at_dt = DateTimeOffset.ParseExact(value, "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture);
            }
        }
        [DataMember]
        public string id;
        [DataMember]
        public string text;
        [DataMember]
        public string source;
        [DataMember]
        public string truncated;
        [DataMember]
        public string in_reply_to_status_id;
        [DataMember]
        public string in_reply_to_user_id;
        [DataMember]
        public string favorited;
        [DataMember]
        public string in_reply_to_screen_name;
        [DataMember]
        public user user;
        [DataMember]
        public geo geo;
        [DataMember]
        public string contributors;
    }

    [DataContract]
    public class geo
    {
        [DataMember]
        public string type;
        [DataMember]
        public string[] coordinates;
    }
}
