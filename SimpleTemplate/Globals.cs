using System;
using System.Collections.Generic;

namespace SimpleTemplate
{
    public class Globals
    {
        public Dictionary<string, object> Context
        {
            get;
            set;
        }

        public Func<object, string[], object> ResolveDots
        {
            get;
            set;
        }

        public Func<object, bool> IsTrue
        {
            get;
            set;
        }

        public Func<object, IEnumerable<object>> ConvertToEnumerable
        {
            get;
            set;
        }

        public Func<object, string> FormatPrice
        {
            get;
            set;
        }
    }
}