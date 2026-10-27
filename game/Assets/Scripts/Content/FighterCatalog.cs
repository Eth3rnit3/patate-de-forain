using System.Collections.Generic;
using System.Linq;

namespace Patate.Content
{
    /// <summary>Source de paquets : liste les dossiers disponibles et donne accès à leurs fichiers.</summary>
    public interface IFighterSource
    {
        /// <summary>Nom de chaque dossier de paquet (= identifiant attendu du combattant).</summary>
        IEnumerable<string> PackageFolders();
        IPackageFiles Open(string folder);
    }

    /// <summary>
    /// Registre central des combattants. Tous les paquets découverts passent
    /// par la validation ; seuls les paquets valides deviennent sélectionnables,
    /// les autres sont listés avec leurs erreurs.
    /// </summary>
    public sealed class FighterCatalog
    {
        readonly Dictionary<string, FighterPackage> fighters = new Dictionary<string, FighterPackage>();
        readonly Dictionary<string, ValidationReport> rejected = new Dictionary<string, ValidationReport>();

        public IReadOnlyList<FighterPackage> Fighters => fighters.Values.OrderBy(f => f.Id, System.StringComparer.Ordinal).ToList();
        public IReadOnlyDictionary<string, ValidationReport> Rejected => rejected;

        public static FighterCatalog Load(IFighterSource source)
        {
            var catalog = new FighterCatalog();
            foreach (var folder in source.PackageFolders().OrderBy(f => f, System.StringComparer.Ordinal))
            {
                var result = FighterPackageReader.Read(source.Open(folder), folder);
                if (result.Ok) catalog.fighters[result.Package.Id] = result.Package;
                else catalog.rejected[folder] = result.Report;
            }
            return catalog;
        }

        public FighterPackage Get(string id) => fighters.TryGetValue(id, out var package) ? package : null;
    }
}
