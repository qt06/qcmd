using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qthk
{
    public class Suggestion
    {
        public string FileName;
        public string SectionName;
        public List<string> SuggestionList;

        public Suggestion(string filename, string sectionname)
        {
            this.FileName = filename;
            this.SectionName = sectionname;
            this.SuggestionList = new List<string>();
            if (!System.IO.File.Exists(this.FileName))
            {
                if(!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(this.FileName)))
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.FileName));
                }
                System.IO.File.WriteAllText(this.FileName, "; config\r\n", Encoding.Unicode);
            }
            if (string.IsNullOrEmpty(this.SectionName))
            {
                string t = System.Windows.Forms.Application.ExecutablePath;
                t = t.Substring(t.LastIndexOf("\\"));
                t = t.Substring(0, t.IndexOf("."));
                this.SectionName = t;
            }
            string str = ini.Read(this.SectionName, "Suggestion", "", this.FileName);
            if (str.Length > 0)
            {
                string[] ls = str.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string tmp in ls)
                {
                    this.SuggestionList.Add(tmp);
                }
            }
        }

        public void Add(string add)
        {
            if (this.SuggestionList.Contains(add))
            {
                this.SuggestionList.Remove(add);
            }
            this.SuggestionList.Insert(0, add);
            this.Save();

        }

        public void Remove(string add)
        {
            if (this.SuggestionList.Contains(add))
            {
                this.SuggestionList.Remove(add);
                this.Save();
            }
        }

        public void Save()
        {
            StringBuilder sb = new StringBuilder(256);
            foreach (string str in this.SuggestionList)
            {
                sb.Append("|" + str);
            }
            ini.Write(this.SectionName, "Suggestion", sb.ToString(), this.FileName);
        }

        public void Clear()
        {
            this.SuggestionList.Clear();
            this.Save();
        }
    }
}
