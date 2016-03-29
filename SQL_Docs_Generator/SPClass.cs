using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SQL_Docs_Generator
{
    public class SPClass
    {
        public string name { get; set; }
        public string schema { get; set; }
        public string author { get; set; }
        public string creationdate { get; set; }
        public string desc { get; set; }
        public string section { get; set; }
        public List<string> parameters {get; set;}
        public string returned { get; set; }
    }
}