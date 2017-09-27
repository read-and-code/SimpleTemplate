using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace SimpleTemplate
{
    public class Template
    {
        private static Regex variableNamePattern = new Regex("[_a-zA-Z][_a-zA-Z0-9]*$", RegexOptions.Compiled);

        private static Regex tokenPattern = new Regex("(?s)({{.*?}}|{%.*?%}|{#.*?#})", RegexOptions.Compiled);

        public Template(string text, Dictionary<string, object> context)
        {
            this.Context = context;
            this.CodeBuilder = new CodeBuilder();
            this.AllVariables = new HashSet<string>();
            this.LoopVariables = new HashSet<string>();

            this.Initialize(text);
        }

        private Dictionary<string, object> Context
        {
            get;
        }

        private CodeBuilder CodeBuilder
        {
            get;
        }

        private HashSet<string> AllVariables
        {
            get;
        }

        private HashSet<string> LoopVariables
        {
            get;
        }

        public string Render()
        {
            var code = this.CodeBuilder.ToString();
            var scriptOptions = ScriptOptions.Default.WithImports("System", "System.Collections.Generic");
            var script = CSharpScript.RunAsync(code, scriptOptions);

            return script.Result.ReturnValue.ToString();
        }

        private void Initialize(string text)
        {
            this.CodeBuilder.AddLine("var result = new List<string>();");

            var variablesCode = this.CodeBuilder.AddSection();
            var buffered = new List<string>();
            var tokens = tokenPattern.Split(text);

            foreach (string token in tokens)
            {
                if (token.StartsWith("{#"))
                {
                    continue;
                }
                else if (token.StartsWith("{{"))
                {
                    string expression = this.EvaluateExpression(token.Substring(2, token.Length - 4).Trim());

                    buffered.Add(string.Format("Convert.ToString({0})", expression));
                }
                else if (token.StartsWith("{%"))
                {
                    continue;
                }
                else
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        buffered.Add(this.ConvertToStringLiteral(token));
                    }
                }
            }

            this.FlushOutput(buffered);

            foreach (string variableName in this.AllVariables)
            {
                variablesCode.AddLine(string.Format("var c_{0} = {1};", variableName, this.ConvertToStringLiteral(this.Context[variableName].ToString())));
            }

            this.CodeBuilder.AddLine("return string.Join(string.Empty, result);");
        }

        private void SyntaxError(string message)
        {
            throw new TemplateSyntaxError(message);
        }

        private void AddVariable(string variableName, HashSet<string> variables)
        {
            if (!variableNamePattern.IsMatch(variableName))
            {
                this.SyntaxError(string.Format("{0} is not a valid name", variableName));
            }

            variables.Add(variableName);
        }

        private string EvaluateExpression(string expression)
        {
            this.AddVariable(expression, this.AllVariables);

            return string.Format("c_{0}", expression);
        }

        private void FlushOutput(List<string> buffered)
        {
            if (buffered.Count == 1)
            {
                this.CodeBuilder.AddLine(string.Format("result.Add({0});", buffered[0]));
            }
            else if (buffered.Count > 1)
            {
                this.CodeBuilder.AddLine(string.Format("result.AddRange(new List<string> {{{0}}});", string.Join(", ", buffered)));
            }

            buffered.Clear();
        }

        private string ConvertToStringLiteral(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return "\"" + value.Replace("\"", "\\\"") + "\"";
        }
    }
}