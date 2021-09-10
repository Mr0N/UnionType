using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;

namespace SourceGeneratorSamples
{


    [Generator]
    public class NewGen : ISourceGenerator
    {
        const int countClass = 6;
        public void Execute(GeneratorExecutionContext context)
        {
            // begin creating the source we'll inject into the users compilation
            var sourceBuilder = new StringBuilder(@"
using System;

namespace UnionType
   {
");
            for (int i = 1; i < countClass; i++)
            {
                sourceBuilder.AppendLine(CreateClass(i));
            }

            sourceBuilder.Append(@"
   }");
            string re = sourceBuilder.ToString();
            // inject the created source into the users compilation
            context.AddSource("UnionType", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
        }
        private string CreateClass(int count)
        {
            var res = Get(count);
            var sourceBuilder = new StringBuilder();
            sourceBuilder.AppendLine(string.Format("public class Union<{0}>{1}", res.allType, "{"));
            sourceBuilder.AppendLine(res.cod);
            sourceBuilder.AppendLine(@"}");
            return sourceBuilder.ToString();
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
        private string CreateOperator(string nameType, string allType, string nameFields)
        {
            const string x = "{";
            const string y = "}";
            return string.Format(@"
        private {0} {1};
        public static implicit operator {0}(Union<{2}> union)
        {3}
            return union.{1};
        {4}
        public static implicit operator Union<{2}>({0} obj)
        {3}
            return new Union<{2}>() {3} {1} = obj {4};
        {4}
            ", nameType, nameFields, allType, x, y);
        }
        private IEnumerable<string> GetNameTypes(int countName)
        {
            return Enumerable.Range(65, 25)
                .Take(countName)
                .Select(a => (char)a)
                .Select(a => a.ToString());
        }
        public (string allType, string cod) Get(int count)
        {
            var res = GetNameTypes(count).ToList();
            string allType = string.Join(",", res);
            StringBuilder builder = new StringBuilder();
            foreach (var item in res)
            {
                builder.AppendLine(CreateOperator(item, allType, $"_{item.ToLower()}"));
            }
            return (allType, builder.ToString());
        }

    }
}