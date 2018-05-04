using System.Collections.Generic;
using Xunit;

namespace SimpleTemplate.Tests
{
    public class TemplateTest
    {
        [Fact]
        public void RenderWithContext()
        {
            string text = @"<h1>{{ name }}</h1>";
            var context = new Dictionary<string, object>()
            {
                { "name", "Tom" },
            };
            Template template = new Template(text, context);

            string result = template.Render();

            Assert.Equal("<h1>Tom</h1>", result);
        }

        [Fact]
        public void RenderWithComment()
        {
            string text = @"{# This is comment #}<h1>{{ name }}</h1>";
            var context = new Dictionary<string, object>()
            {
                { "name", "Tom" },
            };
            Template template = new Template(text, context);

            string result = template.Render();

            Assert.Equal("<h1>Tom</h1>", result);
        }

        [Fact]
        public void RenderWithIf()
        {
            var user = new User { IsChild = true };
            var context = new Dictionary<string, object>()
            {
                { "user", user },
            };
            string text = @"{% if user.IsChild %}<h1>Hello</h1>{% endif %}";
            Template template = new Template(text, context);

            string result = template.Render();

            Assert.Equal("<h1>Hello</h1>", result);
        }

        [Fact]
        public void RenderWithFor()
        {
            var context = new Dictionary<string, object>()
            {
                { "numbers", new[] { 1, 2, 3 } },
            };
            string text = @"<ol>{% for number in numbers %}<li>{{ number }}</li>{% endfor %}</ol>";
            Template template = new Template(text, context);

            string result = template.Render();

            Assert.Equal("<ol><li>1</li><li>2</li><li>3</li></ol>", result);
        }

        [Fact]
        public void RenderWithMethod()
        {
            var user = new User { Name = "Tom" };
            var context = new Dictionary<string, object>()
            {
                { "user", user },
            };
            string text = @"<h1>Hello, {{ user.Name.ToUpper }}</h1>";
            Template template = new Template(text, context);

            string result = template.Render();

            Assert.Equal("<h1>Hello, TOM</h1>", result);
        }

        [Fact]
        public void RenderWithPipe()
        {
            var context = new Dictionary<string, object>()
            {
                { "price", 100 },
            };
            string text = @"<h1>Price: {{ price|FormatPrice }}</h1>";
            Template template = new Template(text, context);

            string result = template.Render();

            Assert.Equal("<h1>Price: $100</h1>", result);
        }

        public class User
        {
            public string Name
            {
                get;
                set;
            }

            public bool IsChild
            {
                get;
                set;
            }
        }
    }
}
