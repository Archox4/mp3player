using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mp3player
{
    public class Song
    {
        public string path { get; set; }
        public string name { get; set; }
        public string author { get; set; }
        public string length { get; set; }
        public TagLib.IPicture image { get; set; }

        public Song(string path, string name, string author, string length, TagLib.IPicture image)
        {
            this.path = path;
            this.name = name;
            this.author = author;
            this.length = length;
            this.image = image;
        }
    }
}
