using System;

namespace SimpleTemplate
{
    public class TemplateRuntimeException : Exception
    {
        public TemplateRuntimeException(string message)
            : base(message)
        {
        }
    }
}