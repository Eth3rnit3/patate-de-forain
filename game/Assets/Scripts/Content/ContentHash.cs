using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Patate.Content
{
    /// <summary>
    /// Empreinte SHA-256 du contenu JSON sous forme canonique (clés triées, sans
    /// espaces) : deux fichiers équivalents ont la même empreinte, quelle que
    /// soit leur mise en forme. Sert à identifier exactement une version jouée.
    /// </summary>
    public static class ContentHash
    {
        public static string Of(params JToken[] documents)
        {
            var text = string.Join("\n", documents.Select(d => Canonical(d).ToString(Formatting.None)));
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
                return string.Concat(bytes.Select(b => b.ToString("x2")));
            }
        }

        static JToken Canonical(JToken token)
        {
            switch (token)
            {
                case JObject obj:
                    return new JObject(obj.Properties().OrderBy(p => p.Name, System.StringComparer.Ordinal).Select(p => new JProperty(p.Name, Canonical(p.Value))));
                case JArray array:
                    return new JArray(array.Select(Canonical));
                default:
                    return token.DeepClone();
            }
        }
    }
}
