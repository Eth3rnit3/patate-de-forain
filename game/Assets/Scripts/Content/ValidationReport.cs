using System.Collections.Generic;
using System.Linq;

namespace Patate.Content
{
    public enum Severity { Error, Warning }

    /// <summary>Problème trouvé dans un paquet : fichier, chemin JSON et message lisible.</summary>
    public sealed class ValidationIssue
    {
        public readonly Severity Severity;
        public readonly string File;
        public readonly string Path;
        public readonly string Message;

        public ValidationIssue(Severity severity, string file, string path, string message)
        {
            Severity = severity; File = file; Path = path; Message = message;
        }

        public override string ToString() =>
            $"{(Severity == Severity.Error ? "Erreur" : "Attention")} — {File}{(string.IsNullOrEmpty(Path) ? "" : " › " + Path)} : {Message}";
    }

    public sealed class ValidationReport
    {
        readonly List<ValidationIssue> issues = new List<ValidationIssue>();

        public IReadOnlyList<ValidationIssue> Issues => issues;
        public bool HasErrors => issues.Any(i => i.Severity == Severity.Error);

        public void Error(string file, string path, string message) => issues.Add(new ValidationIssue(Severity.Error, file, path, message));
        public void Warning(string file, string path, string message) => issues.Add(new ValidationIssue(Severity.Warning, file, path, message));

        public override string ToString() => string.Join("\n", issues.Select(i => i.ToString()));
    }
}
