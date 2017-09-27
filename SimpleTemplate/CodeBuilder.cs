using System.Collections.Generic;
using System.Linq;

namespace SimpleTemplate
{
    public class CodeBuilder
    {
        private const int IndentStep = 4;

        public CodeBuilder()
            : this(0)
        {
        }

        public CodeBuilder(int indentLevel)
        {
            this.Codes = new List<object>();
            this.IndentLevel = indentLevel;
        }

        private List<object> Codes
        {
            get;
        }

        private int IndentLevel
        {
            get;
            set;
        }

        public void AddLine(string line)
        {
            this.Codes.AddRange(new List<object> { new string(' ', this.IndentLevel), line, "\n" });
        }

        public CodeBuilder AddSection()
        {
            CodeBuilder section = new CodeBuilder(this.IndentLevel);

            this.Codes.Add(section);

            return section;
        }

        public void Indent()
        {
            this.IndentLevel += IndentStep;
        }

        public void Dedent()
        {
            this.IndentLevel -= IndentStep;
        }

        public override string ToString()
        {
            return string.Join(string.Empty, this.Codes.Select(code => code.ToString()));
        }
    }
}