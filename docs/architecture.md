# Architecture

Moteur : Unity 6000.4.7f1, pipeline de rendu intégré, Input System, comme Action Legends. Unity apporte l'affichage, le son, le clavier et les exports PC ou WebGL. Les règles du combat ne dépendent pas d'Unity : elles sont écrites à part, en C# pur, et testées sans scène ni rendu.

## Les couches

Chaque couche est une assembly (`.asmdef`). Les dépendances vont toujours vers le haut de ce tableau, jamais vers le bas.

| Assembly | Dossier | Rôle | Dépend de |
|---|---|---|---|
| `Patate.Combat` | `game/Assets/Scripts/Combat` | **Domaine** : états, coups, chronologie, collisions, dégâts, garde, projectiles, manche, sérialisation | rien (`noEngineReferences`) |
| `Patate.Content` | `game/Assets/Scripts/Content` | Format des paquets de combattants et de décors, validation, empreintes, choix de l'image à afficher | Combat, Newtonsoft.Json |
| `Patate.Application` | `game/Assets/Scripts/Application` | **Cas d'usage** : sélectionner les combattants et lancer, avancer d'un tick, recommencer, horloge à pas fixe, sources d'entrées, adversaire de l'ordinateur | Combat, Content |
| `Patate.Unity` | `game/Assets/Scripts/Unity` | **Adaptateurs** : chargement depuis `Resources`, sprites, clavier, manettes, rendu, interface, audio, décor, préférences | tout ce qui précède + Unity |
| `Patate.Editor` | `game/Assets/Scripts/Editor` | Réglages d'import des images, menu de validation | éditeur Unity |

Les trois premières assemblies sont compilées avec `noEngineReferences: true`. Une référence à `UnityEngine` y est donc une **erreur de compilation**, pas une simple convention.

## La simulation

`Simulation.Step(état, entrée J1, entrée J2, contexte, événements) → nouvel état`

- **Pas fixe** : 60 ticks par seconde, quel que soit l'écran. `FixedStepClock` convertit le temps réel en ticks (au plus 4 par image, sans rattrapage après une pause).
- **Pure** : l'état reçu n'est pas modifié. Les mêmes entrées donnent le même résultat, ce qu'un test vérifie sur 900 ticks.
- **Entiers uniquement** : positions, vitesses, boîtes et durées. Pas de flottant, pas d'horloge, pas de hasard. L'état ne contient ni dictionnaire ni ensemble : rien dont l'ordre puisse varier.
- **Entrées par tick** : un masque de bits `Buttons` par joueur et par tick, avec des directions absolues que le domaine convertit en avant/arrière. Le clavier, un script, un replay ou un futur transport réseau implémentent tous `IInputProvider`.
- **État sérialisable** : `StateCodec` sérialise et restaure l'état exactement (binaire compact), et en calcule l'empreinte FNV-1a. Un test le vérifie : sauvegarder, restaurer puis continuer donne le même octet près qu'une partie jamais interrompue.
- **Définitions hors de l'état** : chaque combattant est référencé par `DefinitionRef` (identifiant, version, SHA-256 du gameplay), les règles par `RulesId` et `RulesVersion`.

Ordre de résolution d'un tick, emplacement 0 avant 1 :

1. Manche (introduction, chronomètre) et enregistrement des entrées.
2. Projectiles existants : déplacement, durée de vie, sortie d'arène.
3. Combattants : gel d'impact, progression de l'action (les actions d'entrée de segment se déclenchent ici, une seule fois), décision.
4. Déplacements, recul, limites de l'arène, écart maximal, collision corporelle, orientation automatique.
5. Contacts : tous **détectés** à partir des positions, puis **appliqués**. Deux coups actifs au même instant s'échangent.
6. Fin de manche (KO, temps écoulé).

### Ce que le domaine sait faire

- **Actions** : repos, marche avant et arrière, garde de proximité, coup, étourdissement, blocage, KO.
- **Coups** : une suite de segments (préparation, active, récupération), chacun avec sa durée en ticks, ses hitboxes, ses hurtboxes de remplacement, son avancée et ses actions d'entrée. Un coup touche une seule fois, sans annulation.
- **Commandes** : appui sur un bouton, éventuellement avec une direction, ou séquence directionnelle (quart de cercle) dans une fenêtre de ticks. Les coups sont essayés par priorité.
- **Contact** : dégâts, dégâts en garde (0 pour l'instant), étourdissement, blocage, gel d'impact, recul réparti sur 8 ticks et reporté sur l'attaquant si la cible est coincée au bord. Niveau haut, moyen ou bas, et blocable ou non.
- **Garde** : tenir la direction opposée à la source dans un état disponible. Face à une menace proche, le combattant garde sur place au lieu de reculer.
- **Projectiles** : émission unique à l'entrée d'un segment, à un point relatif au pivot. Limite d'instances par lanceur, durée de vie, sortie d'arène, annulation entre projectiles adverses.

## Les entrées : clavier, manette, ordinateur

Le combat ne connaît que des `Buttons` par tick (`IInputProvider`). `FightScreen` crée une source par joueur avec `InputSources.Create(PlayerSource, place, session)`, appelle `Sample()` une fois par image, puis la session lit `InputFor(tick)` à chaque tick simulé.

| Source | Classe | Particularités |
|---|---|---|
| `Keyboard(0)` / `Keyboard(1)` | `KeyboardInput` | Positions physiques de touches, moitiés gauche et droite |
| `Gamepad(id)` | `GamepadInput` | `id` = `InputDevice.deviceId`, stable tant que la manette reste branchée. Stick gauche (seuil 0,5) ou croix ; poing à l'ouest (X, Carré), pied rapide au sud (A, Croix), pied puissant à l'est (B, Rond), spécial au nord (Y, Triangle). Une manette absente donne `None` |
| `Cpu(niveau)` | `CpuInput` → `CpuPlayer` | Rien à lire par image : décide à chaque tick en lisant la session |

En WebGL, l'Input System lit l'API Gamepad du navigateur (disposition standard). Le navigateur ne signale une manette qu'après un premier appui sur l'un de ses boutons.

**Sélection des joueurs** : `InputSources.Connected()` liste les sources branchées (deux moitiés du clavier, chaque manette) ; `InputSources.JustPressed()` donne celles qui viennent d'appuyer sur un bouton d'attaque (ou Start, ou une gâchette haute) pendant l'image, pour « appuyer pour rejoindre » ; `InputSources.Describe` donne leur nom. Ces fonctions sont sans état : l'écran retient qui a rejoint.

### L'adversaire de l'ordinateur

`CpuPlayer` (`Patate.Application`, C# pur) reçoit l'état qui précède chaque tick et produit des `Buttons`, comme un joueur. Il ne connaît aucun personnage : tout est lu dans le gameplay compilé.

- **Coups** : pour chaque coup, la commande la plus simple (appui direct de préférence, sinon le mouvement est exécuté tick par tick), le premier tick actif, et s'il lance un projectile (`SpawnProjectileAction`).
- **Portée** : un coup est « à portée » si l'une de ses hitboxes actives, placée depuis la position actuelle (avancée des segments comprise), recouvre une hurtbox adverse. Ce sont les mêmes fonctions (`Geometry`, `Hitboxes`) que la résolution des contacts.
- **Perception** : l'action adverse est vue avec le retard du niveau (temps de réaction). Chaque menace (coup proche, lancer en préparation, projectile qui approche) donne lieu à **un** tirage : garder ou non. La garde (tenir arrière) n'est prise que face à une menace réelle, pour ne pas reculer sans raison.
- **Choix** : punir une récupération adverse, attaquer à portée (le coup le plus rapide ou un autre), lancer un projectile quand aucun projectile adverse ne toucherait pendant la préparation, sinon avancer, attendre ou reculer.
- **Respect des règles d'entrée** : un bouton n'est appuyé que si le combattant peut agir et n'est pas gelé, puis relâché au tick suivant (les commandes se déclenchent à l'appui).

| Niveau | Réaction | Choix tous les | Garde | Attaque à portée |
|---|---|---|---|---|
| 0 Facile | 24 ticks | 20 ticks | 20 % | 35 %, frappe parfois dans le vide |
| 1 Normal | 14 ticks | 12 ticks | 50 % | 45 % |
| 2 Difficile | 7 ticks | 6 ticks | 85 % | 60 %, coup le plus rapide de préférence |

Le hasard est un xorshift à graine, identique sur toutes les plateformes : même graine et mêmes entrées adverses donnent la même partie, et **recommencer** rejoue la même suite. `InputSources` tire une graine différente à chaque combat ; les tests fixent la leur. Le journal des entrées (`InputLog`) suffit à rejouer une manche contre l'ordinateur.

Limites : sans saut ni accroupissement dans le moteur, l'IA (comme un joueur) n'a pas de réponse à un adversaire qui ne fait que lancer des projectiles à distance : elle garde mais n'avance pas. Elle n'enchaîne pas de combos et ne lit pas les commandes adverses.

## Le rendu observe

`MatchView` lit l'état après chaque tick et en déduit les images. Le choix de l'image (`FramePicker`, dans `Patate.Content`) dépend de l'action et de son tick, jamais de l'inverse. La durée d'un coup vient du gameplay ; la validation refuse une animation de coup dont la durée totale diffère de celle du coup.

Les collisions viennent des données (`gameplay.json`), pas des pixels. L'affichage de débogage (**F1**) dessine exactement les boîtes que la simulation teste, à partir des mêmes fonctions (`Hitboxes`).

Les effets visuels (impacts, tremblement de caméra) et les sons réagissent aux **événements** (`MatchEvent`) et vivent hors de l'état. Un futur rollback n'a donc pas à les rejouer.

## Points d'extension

| Besoin | Où |
|---|---|
| Nouveau combattant aux mécaniques existantes | Un dossier de données, aucun code : [guide](ajouter-un-combattant.md) |
| Mécanique vraiment nouvelle (téléportation, prise…) | Une classe `SegmentAction`, son traitement dans `SegmentActions` (domaine), sa lecture dans `FighterPackageReader`. Disponible ensuite pour tous les combattants |
| Sons | `Resources/Audio/sound-bank.json` (signaux communs) ou `presentation.json › sounds` (signaux propres à un combattant, ex. `moveStart:stand_kick_heavy`) |
| Décors | `Resources/Stages/<id>/stage.json` : couleurs, calques d'image avec parallaxe, musique |
| Autre source d'entrées (réseau, replay…) | Implémenter `IInputProvider` (`ILocalInput` si elle est lue sur cette machine) et la créer dans `InputSources` à partir d'un `PlayerSource` |
| Nouvel écran ou mode (menu, salon en ligne…) | Implémenter `IGameScreen` et ajouter sa fabrique à `GameFlow`. Les menus remplissent `GameFlow.Setup` (`MatchSetup` : mode, combattants, décor, source des commandes de chaque joueur), que `FightScreen` consomme. Pour rester dans le style : `Ui` (repère de 1080 de haut, fond, logo, enseignes, textes détourés, mots peints) et `MenuInput` (commandes des menus au clavier et à la manette) |
| Mode de jeu à statistiques encadrées | `StatPolicy` (voir plus bas) |

Le code ne contient aucune condition sur l'identité d'un personnage. Un test de validation s'assure que les règles fonctionnent avec un combattant fictif (`SyntheticFighter`) qui n'a rien de Saint Louis.

## Préparé pour le jeu en ligne et les personnages importés

**Déjà en place**

- Simulation déterministe à pas fixe, entrées par tick, état sérialisable et restaurable, empreinte d'état. Ce sont les briques d'un rollback.
- Journal des entrées de la manche (`MatchSession.InputLog`) : un test rejoue une manche à l'identique.
- Identification exacte de ce qui est joué : version des règles, et pour chaque combattant version déclarée, empreinte SHA-256 du gameplay et empreinte de la présentation. Celle-ci couvre l'atlas, qui contient lui-même le SHA-256 de chaque image.
- **Frontière de validation** des paquets, conçue pour des données non fiables (`FighterPackageReader`) :
  - aucun code n'est fourni par les paquets, seulement des données et des images ;
  - JSON borné : taille maximale, profondeur maximale, clés en double refusées ;
  - schéma strict : champs inconnus refusés, entiers obligatoires, bornes sur chaque valeur, nombre maximal d'éléments ;
  - chemins limités au paquet : pas de `..`, extensions autorisées seulement, confinement vérifié en plus par la source disque ;
  - vocabulaire d'actions fermé ;
  - compatibilité de format : une version future est refusée avec un message clair.
- **Règles libres ou équilibrées** : `StatPolicy.Free()` admet tout paquet valide. `StatPolicy.Balanced()` borne la vie, la vitesse, les dégâts et la préparation minimale, et peut n'admettre que des empreintes de gameplay approuvées. Importer un personnage ne lui donne donc pas le droit d'imposer ses statistiques en mode classé.

**Pas encore fait**

- Transport réseau, délai d'entrée et boucle de rollback.
- Écran d'import de paquets.
- Signature des paquets.
- Chargement des images d'un paquet importé depuis des octets : `FighterVisuals` lit `Resources`, alors que la lecture des JSON depuis le disque existe déjà (`DirectoryFighterSource`).
- Règles (`MatchRules`) chargées depuis des données : elles sont aujourd'hui des valeurs par défaut en C#.

## Outillage hors moteur

`tools/export-character` (Node + sharp) transforme les planches sources en poses nettoyées et alignées, en cellules standard 1024 × 768 pour l'équipe artistique, puis en atlas et `atlas.json` pour le jeu. Il ne fait que du découpage, du nettoyage, de l'alignement et de la mise à l'échelle : aucune image n'est générée. Ses traitements et limites sont décrits dans chaque `export-report.md`.
