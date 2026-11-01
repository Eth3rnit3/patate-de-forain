using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Patate.Content
{
    /// <summary>
    /// Lecture défensive d'un document JSON : chaque accès connaît son chemin
    /// et rapporte une erreur lisible au lieu de lever une exception. Les
    /// bornes (min/max, nombre d'éléments) protègent contre les paquets
    /// malformés ou abusifs, en vue de l'import de personnages tiers.
    /// Lecture manuelle, sans réflexion : compatible WebGL/IL2CPP.
    /// </summary>
    public sealed class JsonNode
    {
        public const int MaxDocumentLength = 1_000_000;

        readonly JToken token;
        readonly ValidationReport report;
        public readonly string File;
        public readonly string Path;

        JsonNode(JToken token, ValidationReport report, string file, string path)
        {
            this.token = token; this.report = report; File = file; Path = path;
        }

        /// <summary>Analyse un document ; renvoie null (et une erreur) s'il est illisible.</summary>
        public static JsonNode Parse(string text, string file, ValidationReport report)
        {
            if (text == null)
            {
                report.Error(file, "", "fichier introuvable");
                return null;
            }
            if (text.Length > MaxDocumentLength)
            {
                report.Error(file, "", $"fichier trop volumineux ({text.Length} caractères, maximum {MaxDocumentLength})");
                return null;
            }
            try
            {
                using (var reader = new JsonTextReader(new System.IO.StringReader(text)) { MaxDepth = 32, DateParseHandling = DateParseHandling.None })
                {
                    var token = JToken.ReadFrom(reader, new JsonLoadSettings { DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error });
                    if (token.Type != JTokenType.Object)
                    {
                        report.Error(file, "", "le document doit être un objet JSON");
                        return null;
                    }
                    return new JsonNode(token, report, file, "");
                }
            }
            catch (JsonException exception)
            {
                report.Error(file, "", $"JSON invalide : {exception.Message}");
                return null;
            }
        }

        public JToken Raw => token;

        string Child(string key) => string.IsNullOrEmpty(Path) ? key : $"{Path}.{key}";

        void Error(string path, string message) => report.Error(File, path, message);

        /// <summary>Refuse les champs inconnus : une faute de frappe ne doit pas passer inaperçue.</summary>
        public void AllowOnly(params string[] keys)
        {
            if (!(token is JObject obj)) return;
            foreach (var property in obj.Properties())
                if (!keys.Contains(property.Name)) Error(Child(property.Name), $"champ inconnu (champs permis : {string.Join(", ", keys)})");
        }

        public bool Has(string key) => token is JObject obj && obj.TryGetValue(key, out var value) && value.Type != JTokenType.Null;

        JToken Get(string key, bool required)
        {
            if (token is JObject obj && obj.TryGetValue(key, out var value) && value.Type != JTokenType.Null) return value;
            if (required) Error(Child(key), "champ obligatoire manquant");
            return null;
        }

        public int Int(string key, int min, int max, int? fallback = null)
        {
            var value = Get(key, fallback == null);
            if (value == null) return fallback ?? 0;
            if (value.Type != JTokenType.Integer)
            {
                Error(Child(key), $"doit être un nombre entier (reçu : {Describe(value)})");
                return fallback ?? min;
            }
            long number = value.Value<long>();
            if (number < min || number > max)
            {
                Error(Child(key), $"doit être compris entre {min} et {max} (reçu : {number})");
                return (int)Math.Max(min, Math.Min(max, number));
            }
            return (int)number;
        }

        public bool Bool(string key, bool fallback)
        {
            var value = Get(key, false);
            if (value == null) return fallback;
            if (value.Type == JTokenType.Boolean) return value.Value<bool>();
            Error(Child(key), $"doit valoir true ou false (reçu : {Describe(value)})");
            return fallback;
        }

        public string String(string key, Regex pattern = null, string patternHint = null, int maxLength = 200, bool required = true)
        {
            var value = Get(key, required);
            if (value == null) return null;
            if (value.Type != JTokenType.String)
            {
                Error(Child(key), $"doit être une chaîne (reçu : {Describe(value)})");
                return null;
            }
            var text = value.Value<string>();
            if (text.Length == 0 || text.Length > maxLength) Error(Child(key), $"doit contenir de 1 à {maxLength} caractères");
            else if (pattern != null && !pattern.IsMatch(text)) Error(Child(key), $"« {text} » ne respecte pas le format attendu ({patternHint})");
            return text;
        }

        public T Enum<T>(string key, IReadOnlyDictionary<string, T> values, T fallback, bool required = true)
        {
            var text = String(key, required: required);
            if (text == null) return fallback;
            if (values.TryGetValue(text, out var result)) return result;
            Error(Child(key), $"valeur « {text} » inconnue (valeurs permises : {string.Join(", ", values.Keys)})");
            return fallback;
        }

        public JsonNode Object(string key, bool required = true)
        {
            var value = Get(key, required);
            if (value == null) return null;
            if (value.Type == JTokenType.Object) return new JsonNode(value, report, File, Child(key));
            Error(Child(key), $"doit être un objet (reçu : {Describe(value)})");
            return null;
        }

        public List<JsonNode> Array(string key, int maxCount, bool required = true, int minCount = 0)
        {
            var result = new List<JsonNode>();
            var value = Get(key, required);
            if (value == null) return result;
            if (value.Type != JTokenType.Array)
            {
                Error(Child(key), $"doit être une liste (reçu : {Describe(value)})");
                return result;
            }
            var items = (JArray)value;
            if (items.Count > maxCount) Error(Child(key), $"au plus {maxCount} éléments (reçu : {items.Count})");
            if (items.Count < minCount) Error(Child(key), $"au moins {minCount} élément(s) attendu(s)");
            for (int i = 0; i < Math.Min(items.Count, maxCount); i++) result.Add(new JsonNode(items[i], report, File, $"{Child(key)}[{i}]"));
            return result;
        }

        /// <summary>Entrées d'un objet utilisé comme dictionnaire (clé → valeur).</summary>
        public List<KeyValuePair<string, JsonNode>> Entries(string key, int maxCount, bool required = true)
        {
            var result = new List<KeyValuePair<string, JsonNode>>();
            var node = Object(key, required);
            if (node == null) return result;
            var obj = (JObject)node.token;
            if (obj.Count > maxCount) Error(node.Path, $"au plus {maxCount} entrées (reçu : {obj.Count})");
            foreach (var property in obj.Properties().Take(maxCount))
                result.Add(new KeyValuePair<string, JsonNode>(property.Name, new JsonNode(property.Value, report, File, node.Child(property.Name))));
            return result;
        }

        /// <summary>Lecture de cet élément lui-même comme entier (élément de liste).</summary>
        public int AsInt(int min, int max)
        {
            if (token.Type == JTokenType.Integer && token.Value<long>() >= min && token.Value<long>() <= max) return token.Value<int>();
            Error(Path, $"doit être un entier entre {min} et {max} (reçu : {Describe(token)})");
            return min;
        }

        /// <summary>Lecture de cet élément lui-même comme chaîne (élément de liste).</summary>
        public string AsString(Regex pattern, string patternHint)
        {
            if (token.Type == JTokenType.String && pattern.IsMatch(token.Value<string>())) return token.Value<string>();
            Error(Path, $"doit être une chaîne au format attendu ({patternHint}) (reçu : {Describe(token)})");
            return null;
        }

        public bool IsObject(string message)
        {
            if (token.Type == JTokenType.Object) return true;
            Error(Path, message);
            return false;
        }

        public void Fail(string key, string message) => Error(key == null ? Path : Child(key), message);

        static string Describe(JToken value) => value.Type switch
        {
            JTokenType.String => $"\"{value}\"",
            JTokenType.Object => "un objet",
            JTokenType.Array => "une liste",
            _ => value.ToString(Formatting.None),
        };
    }
}
