using System;
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
    }
}
