using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Patate.Content
{
    /// <summary>
    /// Paquets lus depuis un dossier du disque (un sous-dossier par combattant).
    /// Sert aux tests et aux outils, et préfigure l'import de paquets tiers :
    /// les chemins lus restent confinés au dossier du paquet.
    /// </summary>
    public sealed class DirectoryFighterSource : IFighterSource
    {
        readonly string root;

        public DirectoryFighterSource(string root)
        {
            this.root = Path.GetFullPath(root);
        }

        public IEnumerable<string> PackageFolders() =>
            Directory.Exists(root)
                ? Directory.GetDirectories(root).Where(d => File.Exists(Path.Combine(d, FighterPackage.ManifestFile))).Select(Path.GetFileName)
                : Enumerable.Empty<string>();

        public IPackageFiles Open(string folder) => new Files(Path.Combine(root, folder));

        sealed class Files : IPackageFiles
        {
            readonly string folder;

            public Files(string folder)
            {
                this.folder = Path.GetFullPath(folder) + Path.DirectorySeparatorChar;
            }

            public string ReadText(string relativePath)
            {
                var full = Path.GetFullPath(Path.Combine(folder, relativePath));
                // Défense en profondeur : le validateur refuse déjà « .. », on vérifie quand même le confinement.
                if (!full.StartsWith(folder) || !File.Exists(full)) return null;
                return File.ReadAllText(full);
            }
        }
    }
}
