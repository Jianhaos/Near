using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Near
{
    class near_post
    {
        public String Id { set; get; }

        public DateTime __createdAt { set; get; }

        [JsonProperty(PropertyName = "uid")]
        public String uID { get; set; }

        [JsonProperty(PropertyName = "uname")]
        public String uName { get; set; }

        [JsonProperty(PropertyName = "content")]
        public String Content { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "imageUri")]
        public string ImageUri { get; set; }

        //[JsonProperty(PropertyName = "postDate")]
        //public DateTime PostDate { set; get; }

    }

    class near_image
    {
        public String Id { set; get; }

        [JsonProperty(PropertyName = "uid")]
        public String uID { get; set; }

        /*[JsonProperty(PropertyName = "uname")]
        public String uName { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        //public double Longitude { get; set; }*/

        [JsonProperty(PropertyName = "containerName")]
        public string ContainerName { get; set; }

        [JsonProperty(PropertyName = "resourceName")]
        public string ResourceName { get; set; }

        [JsonProperty(PropertyName = "sasQueryString")]
        public string SasQueryString { get; set; }

        [JsonProperty(PropertyName = "imageUri")]
        public string ImageUri { get; set; }
    }

    class near_comment
    {
        public String Id { set; get; }

        public DateTime __createdAt { set; get; }

        [JsonProperty(PropertyName = "subjectid")]
        public String SubjectID { get; set; }

        [JsonProperty(PropertyName = "sender")]
        public String Sender { get; set; }

        [JsonProperty(PropertyName = "uname")]
        public String uName { get; set; }

        [JsonProperty(PropertyName = "content")]
        public String Content { get; set; }

        //[JsonProperty(PropertyName = "postDate")]
        //public DateTime PostDate { set; get; }

    }

    public class Channel
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "uri")]
        public string Uri { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string User { get; set; }

        [JsonProperty(PropertyName = "isPush")]
        public bool IsPush { get; set; }
    }

    public class near_chat
    {
        public String Id { set; get; }

        public DateTime __createdAt { set; get; }

        [JsonProperty(PropertyName = "sender")]
        public string Sender { get; set; }

        [JsonProperty(PropertyName = "uname")]
        public string uName { get; set; }

        [JsonProperty(PropertyName = "receiver")]
        public string Receiver { get; set; }

        [JsonProperty(PropertyName = "content")]
        public string Content { get; set; }

        [JsonProperty(PropertyName = "channel")]
        public string Channel { get; set; }

        public override string ToString()
        {
            return string.Format(" {0} Says: {1}", Sender, Content);
        }
    }
}
