# Patate de Forain

Jeu de combat 2D aux sensations arcade, avec nos propres personnages et notre direction artistique. Construit avec Unity, sur le modèle d'Action Legends.

**État : prototype jouable.** Deux Saint Louis s'affrontent en combat miroir, à deux joueurs sur un clavier. Voir [l'état du prototype](docs/etat-du-prototype.md) pour le détail de ce qui fonctionne, de ce qui a été vérifié et de ce qui manque.

## Lancer le jeu

1. Unity **6000.4.7f1**. Dans Unity Hub : *Add project from disk* → choisir le dossier **`game/`**.
   Ne pas ouvrir la racine du dépôt : sur macOS, `assets/` (les sources de l'équipe artistique) et `Assets/` désignent le même dossier, et Unity y écrirait ses fichiers.
2. Ouvrir la scène `Assets/Scenes/Combat.unity`.
3. **Play**. La fenêtre Game doit avoir le focus clavier ; le jeu se met en pause quand elle le perd.

## Version navigateur (WebGL)

Le module *WebGL Build Support* de Unity 6000.4.7f1 doit être installé (Unity Hub › Installs › Add modules).

- **Dans l'éditeur** : *File › Build Profiles* › **Web** › *Switch Platform*, puis menu *Patate de Forain › Construire la version navigateur*.
- **En ligne de commande**, sans ouvrir l'éditeur sur ce projet (Unity lancé en mode batch), depuis `game/` :

```bash
unity build --target WebGL --execute-method Patate.EditorTools.WebBuilder.Build .
```

La version est écrite dans `game/WebBuild/` (ignoré par git ; la variable `PATATE_WEB_DIR` change ce dossier). Fichiers compressés en Gzip avec décompression de repli et sans threads : n'importe quel hébergeur statique convient, GitHub Pages compris (`.nojekyll` et `build-info.json` sont ajoutés). Pour l'essayer en local, depuis la racine du dépôt :

```bash
python3 -m http.server 8765 --directory game/WebBuild
```

puis ouvrir <http://localhost:8765> et **cliquer dans le jeu** pour lui donner le clavier. Ouvrir `index.html` directement depuis le disque ne fonctionne pas.

Poids téléchargé : environ **13 Mo** (code 7,6 Mo, données 5 Mo, dont l'atlas de Saint Louis : 4048 × 2680 en BC7, 10,3 Mo avant Gzip).

## Commandes

Les touches sont des **positions physiques** (Input System). Les noms ci-dessous sont ceux d'un clavier **AZERTY** ; l'aide en jeu (**F2**) affiche les noms de votre disposition réelle.

| Action | Joueur 1 | Joueur 2 |
|---|---|---|
| Avancer / reculer | **Q** / **D** | **←** / **→** |
| Bas (pour le quart de cercle) | **S** | **↓** |
| Garder | tenir la direction opposée à l'adversaire | idem |
| Poing | **F** | **K** ou pavé **1** |
| Pied rapide | **G** | **L** ou pavé **2** |
| Pied puissant | **H** | **M** ou pavé **3** |
| Spécial (Poing de Saint Louis) | **R**, ou bas, bas-avant, avant + poing | **O** ou pavé **0**, ou ↓ ↘ → + poing |

| Touche | Effet |
|---|---|
| **F1** | Débogage : boîtes de collision, états, entrées, empreinte de l'état (mémorisé) |
| **F2** | Aide des commandes |
| **F3** / **F4** | Combattant suivant pour J1 / J2 (relance la manche) |
| **F5**, ou **Entrée** en fin de manche | Nouvelle manche |
| **Échap** | Pause |

À deux sur un même clavier, certaines combinaisons de nombreuses touches simultanées peuvent ne pas être détectées (limite matérielle du clavier). Le pavé numérique du joueur 2 aide à l'éviter quand il existe.

## Vérifier

- **Tests (69)** : dans Unity, *Window › General › Test Runner › EditMode › Run All*. Ou en ligne de commande, éditeur ouvert, depuis `game/` : `unity command test run --mode editor`.
- **Paquets de combattants** : menu *Patate de Forain › Valider les combattants*.
- **Scénario en jeu scripté** : `AgentScripts/VerifyMatch.cs` (mode d'emploi en tête de fichier).

## Jouer en ligne

Deux joueurs, chacun sur sa machine (navigateur ou PC), dans un salon désigné par un code. Il faut un relais : en local, `cd server/relay && npm install && npm start`. L'un crée le salon, l'autre entre son code. Un lien `?salon=CODE` (ou `-salon CODE` sur PC) mène directement au salon : c'est le seul accès tant que le menu n'a pas d'entrée « En ligne ». Lancement, paramètres, essais et déploiement sur Cloudflare : [server/relay/README.md](server/relay/README.md).

## Intégrer des visuels

Depuis la racine du dépôt (Node 22) :

```bash
npm install
npm run assets:export -- tools/export-character/configs/saint-louis.json
```

L'outil lit les planches sources sans les modifier. Il produit des images individuelles et des aperçus de contrôle dans `assets/characters/<personnage>/export/`, puis l'atlas du jeu dans `game/Assets/Resources/Fighters/<personnage>/`. Le rapport `export-report.md` décrit chaque traitement.

## Documentation

- [Architecture](docs/architecture.md) : les couches, la simulation, les points d'extension et le jeu en ligne.
- [Relais en ligne](server/relay/README.md) : lancer le relais, essayer à deux, le déployer.
- [Ajouter un combattant](docs/ajouter-un-combattant.md) : guide complet, avec l'exemple de Napoléon.
- [État du prototype](docs/etat-du-prototype.md) : ce qui marche, ce qui est vérifié, les limites et les besoins artistiques.
- [Standard personnage](docs/character-standard-v01.md) : direction artistique et format des exports.
