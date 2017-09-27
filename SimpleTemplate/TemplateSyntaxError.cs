using System;

namespace SimpleTemplate
{
    public class TemplateSyntaxError : Exception
    {
        public TemplateSyntaxError(string message)
            : base(message)
        {
        }
    }
}