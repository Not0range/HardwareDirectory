using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HardwareDictionary
{
    abstract class Article
    {
        public static List<Article> articles = new List<Article>();

        protected string title;

        public string Title
        {
            get
            {
                return title;
            }
        }

        protected string[] text;

        public string[] Text
        {
            get
            {
                return text;
            }
        }

        public Article(string title, string[] text)
        {
            this.title = title;
            this.text = text;
        }

        public virtual bool Search(string filter)
        {
            if (title.ToLower().Contains(filter.ToLower()))
                return true;
            if (text.Any(s => s.ToLower().Contains(filter.ToLower())))
                return true;
            return false;
        }
    }

    class Site : Article, IPictures
    {
        string url;

        public string Url
        {
            get
            {
                return url;
            }
        }

        Bitmap[] pictures;

        public Bitmap[] Pictures
        {
            get
            {
                return pictures;
            }
        }

        public Site(string title, string[] text, string url) : base(title, text)
        {
            this.url = url;
        }

        public void Init(Bitmap[] pictures)
        {
            this.pictures = pictures;
        }

        public override bool Search(string filter)
        {
            if (base.Search(filter))
                return true;
            if (url.ToLower().Contains(filter.ToLower()))
                return true;
            return false;
        }
    }

    class Manufacturer : Article
    {
        int year;

        public int Year
        {
            get
            {
                return year;
            }
        }

        string country;

        public string Country
        {
            get
            {
                return country;
            }
        }

        public Manufacturer(string title, string[] text, int year, string country) : base(title, text)
        {
            this.year = year;
            this.country = country;
        }

        public override bool Search(string filter)
        {
            if (base.Search(filter))
                return true;
            if (year.ToString().Contains(filter))
                return true;
            if (country.ToLower().Contains(filter.ToLower()))
                return true;
            return false;
        }
    }

    class Hardware : Article, IPictures
    {
        bool need;

        public bool Need
        {
            get
            {
                return need;
            }
        }

        Bitmap[] pictures;

        public Bitmap[] Pictures
        {
            get
            {
                return pictures;
            }
        }

        public Hardware(string title, string[] text, bool need) : base(title, text)
        {
            this.need = need;
        }

        public void Init(Bitmap[] pictures)
        {
            this.pictures = pictures;
        }
    }

    interface IPictures
    {
        Bitmap[] Pictures { get; }

        void Init(Bitmap[] pictures);
    }
}
