using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace TwitterStreamClient
{
    [DataContract]
    public class user
    {
        //<user>
        //<id>1401881</id>
        // <name>Doug Williams</name>
        // <screen_name>dougw</screen_name>
        // <location>San Francisco, CA</location>
        // <description>Twitter API Support. Internet, greed, users, dougw and opportunities are my passions.</description>
        // <profile_image_url>http://s3.amazonaws.com/twitter_production/profile_images/59648642/avatar_normal.png</profile_image_url>
        // <url>http://www.igudo.com</url>
        // <lang>en</lang>
        // <protected>false</protected>
        // <followers_count>1027</followers_count>
        // <profile_background_color>9ae4e8</profile_background_color>
        // <profile_text_color>000000</profile_text_color>
        // <profile_link_color>0000ff</profile_link_color>
        // <profile_sidebar_fill_color>e0ff92</profile_sidebar_fill_color>
        // <profile_sidebar_border_color>87bc44</profile_sidebar_border_color>
        // <friends_count>293</friends_count>
        // <created_at>Sun Mar 18 06:42:26 +0000 2007</created_at>
        // <favourites_count>0</favourites_count>
        // <utc_offset>-18000</utc_offset>
        // <time_zone>Eastern Time (US & Canada)</time_zone>
        // <profile_background_image_url>http://s3.amazonaws.com/twitter_production/profile_background_images/2752608/twitter_bg_grass.jpg</profile_background_image_url>
        // <profile_background_tile>false</profile_background_tile>
        // <statuses_count>3390</statuses_count>
        // <notifications>false</notifications>
        // <following>false</following>
        // <verified>true</verified>
        // <contributors_enabled>false</verified>
        //</user> 
        [DataMember]
        public string id;
        [DataMember]
        public string name;
        [DataMember]
        public string screen_name;
        [DataMember]
        public string location;
        [DataMember]
        public string description;
        [DataMember]
        public string profile_image_url;
        [DataMember]
        public string url;
        [DataMember]
        public string lang;
        [DataMember]
        public string @protected;
        [DataMember]
        public string followers_count;
        [DataMember]
        public string profile_background_color;
        [DataMember]
        public string profile_text_color;
        [DataMember]
        public string profile_link_color;
        [DataMember]
        public string profile_sidebar_fill_color;
        [DataMember]
        public string profile_sidebar_border_color;
        [DataMember]
        public string friends_count;
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
        public string favourites_count;
        [DataMember]
        public string utc_offset;
        [DataMember]
        public string time_zone;
        [DataMember]
        public string profile_background_image_url;
        [DataMember]
        public string profile_background_tile;
        [DataMember]
        public string statuses_count;
        [DataMember]
        public string notifications;
        [DataMember]
        public string following;
        [DataMember]
        public string verified;
        [DataMember]
        public string contributors_enabled;
    }
}
