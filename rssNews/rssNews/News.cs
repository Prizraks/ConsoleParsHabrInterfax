using System;
using System.Collections.Generic;
using System.Text;

namespace rssNews
{
    class News
    {
        public string Headline { get; set; }
        public DateTime PublicationDate { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public int SourceId { get; set; }
        public Source Source { get; set; }
    }
}
