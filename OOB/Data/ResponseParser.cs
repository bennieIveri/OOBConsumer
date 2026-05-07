
//Kobus Walters, 04/18/2026 11:08 AM https://redmine.iveri.com/issues/21111 
using Newtonsoft.Json.Linq;
using System.Xml.Linq;

namespace OOB.Data
{
    public static class ResponseParser
    {
        public static Dictionary<string, string> ParseJsonResponse(string json)
        {
            JObject root = JObject.Parse(json);
            JToken transaction = root["Transaction"];
            var collection = new Dictionary<string, string>();
            // Start recursion
            FlattenJson(transaction, "", collection);
            return collection;
        }
        private static void FlattenJson(JToken token, string path, Dictionary<string, string> result)
        {
            if (token.Type == JTokenType.Object)
            {
                foreach (var prop in token.Children<JProperty>())
                {
                    string currentPath = string.IsNullOrEmpty(path) ? prop.Name : $"{path}.{prop.Name}";
                    FlattenJson(prop.Value, currentPath, result);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                // Optional: Handle arrays if they appear in your JSON
                int index = 0;
                foreach (var item in token.Children())
                {
                    FlattenJson(item, $"{path}[{index}]", result);
                    index++;
                }
            }
            else
            {
                // It's a leaf value (string, number, etc.)
                result[path] = token.ToString();
            }
        }
        public static Dictionary<string, string> ParseXmlResponse(string xml)
        {
            XElement root = XElement.Parse(xml);
            XElement transactionNode = root.Element("Transaction");
            var collection = new Dictionary<string, string>();
            // Start recursion
            FlattenXml(transactionNode, "", collection);
            return collection;
        }
        private static void FlattenXml(XElement element, string path, Dictionary<string, string> result)
        {
            foreach (var child in element.Attributes())
            {
                result[$"{path}{(string.IsNullOrEmpty(path) ? "" : ".")}{child.Name.LocalName}"] = child.Value.Trim();
            }
            if (element.HasElements)
            {
                // It has children: recurse into each child
                foreach (var child in element.Elements())
                {
                    FlattenXml(child, $"{path}{(string.IsNullOrEmpty(path) ? "" : ".")}{child.Name.LocalName}", result);
                }
            }
            else if (!element.HasAttributes)
            {
                // It's a leaf node: add the path and the trimmed value
                result[path] = element.Value.Trim();
            }
        }
    }
}
