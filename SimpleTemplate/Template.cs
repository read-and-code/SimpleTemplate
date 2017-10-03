using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            var script = CSharpScript.RunAsync(code, scriptOptions, new Globals { Context = this.Context, ResolveDots = this.ResolveDots, IsTrue = this.IsTrue, ConvertToEnumerable = this.ConvertToEnumerable });

            return script.Result.ReturnValue.ToString();
        }

        private void Initialize(string text)
        {
            this.CodeBuilder.AddLine("var result = new List<string>();");

            var operationStack = new Stack<string>();
            var buffered = new List<string>();
            var variablesSection = this.CodeBuilder.AddSection();
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
                    this.FlushOutput(buffered);

                    var words = token.Substring(2, token.Length - 4).Trim().Split();

                    if (words[0] == "if")
                    {
                        if (words.Length != 2)
                        {
                            this.SyntaxError(string.Format("Don't understand if, token: {0}", token));
                        }

                        operationStack.Push("if");

                        this.CodeBuilder.AddLine(string.Format("if (IsTrue({0})) {{", this.EvaluateExpression(words[1])));
                        this.CodeBuilder.Indent();
                    }
                    else if (words[0] == "for")
                    {
                        if (words.Length != 4 || words[2] != "in")
                        {
                            this.SyntaxError(string.Format("Don't understand for, token: {0}", token));
                        }

                        operationStack.Push("for");

                        this.AddVariable(words[1], this.LoopVariables);

                        this.CodeBuilder.AddLine(string.Format("foreach (var {0} in ConvertToEnumerable({1})) {{", words[1], this.EvaluateExpression(words[3])));
                        this.CodeBuilder.Indent();
                    }
                    else if (words[0].StartsWith("end"))
                    {
                        if (words.Length != 1)
                        {
                            this.SyntaxError(string.Format("Don't understand end, token: {0}", token));
                        }

                        var endWhat = words[0].Substring(3);

                        if (operationStack.Count == 0)
                        {
                            this.SyntaxError(string.Format("Too many ends, token: {0}", token));
                        }

                        var startWhat = operationStack.Pop();

                        if (startWhat != endWhat)
                        {
                            this.SyntaxError(string.Format("Mismatched end tag, token: {0}", token));
                        }

                        this.CodeBuilder.AddLine("}");
                        this.CodeBuilder.Dedent();
                    }
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

            foreach (string variableName in new HashSet<string>(this.AllVariables.Except(this.LoopVariables)))
            {
                variablesSection.AddLine(string.Format("var {0} = Context[{1}];", variableName, this.ConvertToStringLiteral(variableName)));
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
            if (expression.Contains("."))
            {
                var dots = expression.Split('.');
                var code = this.EvaluateExpression(dots[0]);
                var arguments = dots.ToList().GetRange(1, dots.Length - 1).Select(x => this.ConvertToStringLiteral(x)).ToArray();

                return string.Format("ResolveDots({0}, {1})", code, string.Format("new [] {{ {0} }}", string.Join(", ", arguments)));
            }
            else
            {
                this.AddVariable(expression, this.AllVariables);

                return expression;
            }
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

        private object ResolveDots(object value, string[] arguments)
        {
            foreach (string argument in arguments)
            {
                var members = value.GetType().GetMember(argument);

                // Suppose there is only one member
                var member = members[0];

                switch (member.MemberType)
                {
                    case MemberTypes.Property:
                        value = ((PropertyInfo)member).GetValue(value);

                        break;
                    case MemberTypes.Field:
                        value = ((FieldInfo)member).GetValue(value);

                        break;
                    case MemberTypes.Method:
                        value = ((MethodInfo)member).Invoke(value, null);

                        break;
                    default:
                        throw new TemplateRuntimeException(string.Format("Unsupported member type {0}", member.MemberType));
                }
            }

            return value;
        }

        private bool IsTrue(object value)
        {
            var type = value.GetType();

            switch (type.Name)
            {
                case "Boolean":
                    return (bool)value;
                default:
                    throw new TemplateRuntimeException(string.Format("Unsupported type to test truth, type: {0}", type.Name));
            }
        }

        private IEnumerable<object> ConvertToEnumerable(object value)
        {
            return ((Array)value).Cast<object>();
        }
    }
}