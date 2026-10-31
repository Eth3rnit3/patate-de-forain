# Ajouter un combattant

Un combattant qui utilise les mécaniques existantes s'ajoute **sans modifier le code** : il suffit d'un dossier de données dans `game/Assets/Resources/Fighters/<id>/`. Le jeu découvre ce dossier tout seul, le valide et le rend sélectionnable avec **F3** / **F4**. Le guide suit l'exemple de Napoléon ; les noms de fichiers et les valeurs ci-dessous sont des exemples à adapter.

Pour un exemple complet et réel, consulter le paquet de Saint Louis : `game/Assets/Resources/Fighters/saint-louis/` (quatre coups, un projectile, toutes les animations) et sa configuration d'export `tools/export-character/configs/saint-louis.json`.

## 1. Les visuels sources

Suivre le [standard personnage](character-standard-v01.md) : un dossier `assets/characters/napoleon/source/` contenant une planche par animation, personnage tourné vers la droite, sur fond transparent.

L'outil d'export accepte des planches de tailles et de grilles différentes. Il lui faut seulement :
- des **rangées de hauteur égale** ;
- des poses qui **ne se touchent pas** à l'intérieur d'une rangée (un membre peut déborder au-dessus de la pose voisine, tant qu'il ne la touche pas) ;
  si deux poses se touchent malgré tout (pans de manteau, semelles), une **couture** les sépare (voir `seams` plus bas) ;
- pour les effets (projectile, impact), une **grille régulière**.

Animations minimales :

| Rôle | Exemple |
|---|---|
| Garde, en boucle | `idle` |
| Marche avant (rejouée à l'envers pour reculer) | `walk_forward` |
| Réaction au coup (sert aussi de pose de KO tant qu'il n'y en a pas) | `hit_high` |
| Blocage | `block_high` |
| Une animation par coup | `stand_punch_light`… |
| Projectile et impact, si le combattant a un projectile | |
| Portrait | |

## 2. L'export technique

Copier `tools/export-character/configs/saint-louis.json` vers `napoleon.json` et adapter :

```json
{
  "character": "napoleon",
  "filePrefix": "napoleon",
  "sourceDir": "assets/characters/napoleon/source",
  "framesDir": "assets/characters/napoleon/export/frames",
  "previewDir": "assets/characters/napoleon/export/previews",
  "reportFile": "assets/characters/napoleon/export/export-report.md",
  "packageDir": "game/Assets/Resources/Fighters/napoleon",
  "cell": { "width": 1024, "height": 768, "pivotX": 512, "pivotY": 704 },
  "standingHeight": 480,
  "portrait": { "file": "napoleon_portrait_hd_v01.png", "size": 512 },
  "animations": [
    { "name": "idle", "file": "napoleon_idle_hd_v01.png", "layout": { "type": "poses", "rows": 2 }, "frames": 6, "referenceFrame": 1, "anchor": "rearFoot" },
    { "name": "walk_forward", "file": "napoleon_walk_forward_hd_v01.png", "layout": { "type": "poses", "rows": 2 }, "frames": 8, "scaleLike": "idle", "referenceFrame": 1, "anchor": "torso" },
    { "name": "cannon_ball", "file": "napoleon_cannon_ball_hd_v01.png", "layout": { "type": "grid", "rows": 1, "columns": 4 }, "frames": 4, "kind": "effect", "scale": 0.45, "anchor": "head" }
  ]
}
```

Les options d'une animation :

| Champ | Rôle |
|---|---|
| `frames` | Nombre de poses attendu. L'outil s'arrête avec un message clair s'il en trouve un autre nombre |
| `standingHeight`, `referenceFrame` | La pose de garde indiquée est ramenée à cette hauteur, en pixels, qui deviennent des unités monde. Elle fixe la taille du personnage à l'écran |
| `scaleLike` | Réutilise l'échelle d'une planche dessinée à la même taille |
| `anchor` | Repère d'alignement : `rearFoot` (garde, coups, réactions), `torso` (marche), `head` (projectile), `centroid` (impact) |
| `nudge` | Correction manuelle en pixels de cellule, par pose : `{ "3": [-12, 0] }` |
| `layout.seams` | Sépare deux poses qui se touchent dans une rangée : `[{ "row": 1, "x": 1050 }]`. L'outil trace, autour de la colonne `x` de la planche source (± `radius`, 60 par défaut), le chemin de haut en bas qui traverse le moins de pixels opaques ; les poses ne le franchissent pas. Si une pose est dessinée **devant** sa voisine (pointe de botte sur le talon), imposer le passage avec `"through": [[x, y], …]` pour que la partie cachée reste à la pose du dessous. Exemple réel : `hit_high` de Napoléon |

Lancer l'export :

```bash
npm run assets:export -- tools/export-character/configs/napoleon.json
```

Contrôler ensuite les aperçus dans `assets/characters/napoleon/export/previews/` :
- `*_planche.png` : franges sur fond clair et sombre, pieds sur la ligne de sol ;
- `*_superposition.png` : dérive des appuis d'une pose à l'autre.

L'outil écrit `atlas-0.png`, `atlas.json` et `portrait.png` dans le dossier du paquet. Ces fichiers sont générés : ne pas les modifier à la main.

## 3. Le paquet

Trois fichiers à écrire dans `game/Assets/Resources/Fighters/napoleon/`. Le dossier porte l'identifiant du combattant.

### `fighter.json` : identité

```json
{
  "format": "patate-fighter",
  "formatVersion": 1,
  "id": "napoleon",
  "name": "Napoléon",
  "version": "0.1.0",
  "description": "Empereur, combat au sabre au fourreau.",
  "credits": ["Équipe Patate de Forain"]
}
```

Augmenter `version` à chaque changement de gameplay publié. L'empreinte SHA-256 du gameplay, calculée par le jeu, identifie de toute façon la version exacte.

### `gameplay.json` : ce qui décide du combat

**Unités et conventions**
- Distances en unités monde : 1 unité = 1 pixel de cellule exportée.
- Durées en ticks : 60 par seconde.
- Rectangle = `{ "x", "y", "w", "h" }` :
  - relatif au **pivot**, au sol entre les pieds ;
  - personnage **regardant à droite** ;
  - **y vers le haut**, 0 au sol ;
  - `(x, y)` désigne le coin bas-gauche.
- Pour mesurer une portée, le plus simple est de lire les cellules exportées : pivot en (512, 704) dans une image 1024 × 768, donc `x = colonne − 512` et `y = 704 − ligne`.

```json
{
  "stats": { "health": 1000, "walkForwardSpeed": 7, "walkBackSpeed": 5 },
  "body": {
    "pushbox": { "x": -70, "y": 0, "w": 140, "h": 410 },
    "hurtboxes": [
      { "x": -50, "y": 380, "w": 100, "h": 100 },
      { "x": -90, "y": 190, "w": 180, "h": 190 },
      { "x": -140, "y": 0, "w": 280, "h": 190 }
    ],
    "crouch": { "pushbox": { "x": -70, "y": 0, "w": 140, "h": 280 },
                "hurtboxes": [{ "x": -100, "y": 0, "w": 200, "h": 290 }] },
    "air": { "pushbox": { "x": -60, "y": 110, "w": 120, "h": 280 },
             "hurtboxes": [{ "x": -90, "y": 70, "w": 180, "h": 280 }] }
  },
  "moves": [
    {
      "id": "cannon_shot",
      "commands": [{ "button": "special" }, { "motion": [2, 3, 6], "button": "lightPunch", "window": 15 }],
      "priority": 10,
      "segments": [
        { "phase": "startup", "duration": 16 },
        { "phase": "recovery", "duration": 24,
          "onEnter": [{ "type": "spawnProjectile", "projectile": "cannon_ball", "offset": { "x": 300, "y": 360 } }] }
      ]
    },
    {
      "id": "stand_punch_light",
      "commands": [{ "button": "lightPunch" }],
      "segments": [
        { "phase": "startup", "duration": 5 },
        { "phase": "active", "duration": 3, "hitboxes": [{ "x": 150, "y": 360, "w": 140, "h": 50 }] },
        { "phase": "recovery", "duration": 10 }
      ],
      "hit": { "damage": 40, "hitstun": 14, "blockstun": 10, "hitstop": 6,
               "pushback": { "hit": 50, "block": 40 }, "level": "mid", "blockable": true }
    },
    {
      "id": "jump_kick",
      "stance": "air",
      "commands": [{ "button": "lightKick" }],
      "segments": [
        { "phase": "startup", "duration": 6 },
        { "phase": "active", "duration": 8, "hitboxes": [{ "x": 130, "y": 120, "w": 220, "h": 130 }] },
        { "phase": "recovery", "duration": 6 }
      ],
      "hit": { "damage": 75, "hitstun": 16, "blockstun": 12, "hitstop": 7,
               "pushback": { "hit": 40, "block": 30 }, "level": "high", "blockable": true }
    }
  ],
  "projectiles": [
    { "id": "cannon_ball", "speed": 10, "lifetime": 180, "hitbox": { "x": -40, "y": -40, "w": 80, "h": 80 }, "maxActive": 1,
      "hit": { "damage": 90, "hitstun": 18, "blockstun": 14, "hitstop": 8,
               "pushback": { "hit": 70, "block": 60 }, "level": "mid", "blockable": true } }
  ]
}
```

| Champ | Sens |
|---|---|
| `body.crouch` / `body.air` | Corps accroupi et en l'air (facultatifs : sinon celui de la garde). En l'air, `y` part du pivot, donc des pieds |
| `stats.jump` | Facultatif : `velocity` (40), `gravity` (2), `forwardSpeed` (9), `backSpeed` (8), `preJumpTicks` (4), `landingTicks` (4). Hauteur ≈ v²/2g |
| `stance` | `stand` (par défaut), `crouch` (bas tenu) ou `air` (pendant un saut, un coup par saut) |
| `hit.level` | `high` : gardé debout seulement, passe au-dessus d'un adversaire accroupi s'il vise haut ; `mid` : gardé debout ou accroupi ; `low` : gardé accroupi seulement |
| `commands[].button` | `lightPunch`, `mediumPunch`, `heavyPunch`, `lightKick`, `mediumKick`, `heavyKick`, `special`. Le clavier actuel donne `lightPunch`, `lightKick`, `heavyKick` et `special` |
| `commands[].direction` | Direction tenue, en notation pavé numérique (6 = avant, 4 = arrière, 2 = bas) |
| `commands[].motion` | Séquence de directions, ex. `[2, 3, 6]`, dans `window` ticks |
| `priority` | Plus grand = essayé en premier (spéciaux avant normaux) |
| `segments[].phase` | `startup`, `active` (seule phase où une hitbox compte) ou `recovery` |
| `segments[].hurtboxes` | Remplace les hurtboxes du corps pendant le segment (ex. jambe tendue, vulnérable) |
| `segments[].advance` | Avancée par tick |
| `segments[].onEnter` | Actions déclenchées une fois à l'entrée du segment. Aujourd'hui : `spawnProjectile` |
| `hit.hitstun` / `blockstun` | Ticks pendant lesquels la cible ne peut pas agir |
| `hit.hitstop` | Gel d'impact |
| `hit.pushback` | Recul de la cible, touchée ou en garde |
| `hit.chipDamage` | Dégâts à travers la garde (0 par défaut) |
| `projectiles[].hitbox` | Collision réelle, relative au centre du projectile, **sans le halo** |
| `projectiles[].maxActive` | Projectiles de ce type à l'écran par lanceur |

### `presentation.json` : ce qui s'affiche

```json
{
  "atlas": "atlas.json",
  "portrait": "portrait.png",
  "animations": {
    "idle": { "frames": [{ "sprite": "idle_01", "duration": 8 }, { "sprite": "idle_02", "duration": 8 }] },
    "stand_punch_light": { "frames": [
      { "sprite": "stand_punch_light_01", "duration": 3 }, { "sprite": "stand_punch_light_02", "duration": 2 },
      { "sprite": "stand_punch_light_03", "duration": 3 }, { "sprite": "stand_punch_light_04", "duration": 10 } ] }
  },
  "states": {
    "idle": { "animation": "idle", "playback": "loop" },
    "walkForward": { "animation": "walk_forward", "playback": "loop" },
    "walkBack": { "animation": "walk_forward", "playback": "reverseLoop" },
    "guard": { "animation": "block_high", "playback": "hold", "frame": 0 },
    "hitstun": { "animation": "hit_high", "playback": "stretch" },
    "blockstun": { "animation": "block_high", "playback": "stretch" },
    "ko": { "animation": "ko", "playback": "once" },
    "crouch": { "animation": "crouch", "playback": "once" },
    "crouchGuard": { "animation": "block_low", "playback": "hold", "frame": 0 },
    "jump": { "animation": "jump_air", "playback": "once" }
  },
  "moves": { "stand_punch_light": { "animation": "stand_punch_light" }, "cannon_shot": { "animation": "cannon_shot_cast" } },
  "projectiles": { "cannon_ball": { "animation": "cannon_ball", "impact": "cannon_ball_impact" } },
  "sounds": {}
}
```

**Règle importante** : l'animation d'un coup dure **exactement** autant de ticks que ses segments. Ici 3 + 2 + 3 + 10 = 18 = 5 + 3 + 10. Placer la pose de frappe sur les ticks actifs. La validation refuse une animation qui ne correspond pas et indique les deux durées.

Les modes de lecture (`playback`) :

| Mode | Usage |
|---|---|
| `once` | Coups, obligatoire |
| `loop` | Boucle |
| `reverseLoop` | Boucle lue à l'envers |
| `stretch` | Étirée sur la durée de l'étourdissement ou du blocage |
| `hold` | Image fixe, choisie par `frame` à partir de 0 |

Les sept premiers états sont obligatoires. Les états de posture sont facultatifs ; un état absent se rabat sur un voisin :

| État | Quand | À défaut |
|---|---|---|
| `crouch` | accroupi | `idle` |
| `crouchGuard` | garde accroupie, en attente | `crouch` |
| `crouchHitstun` / `crouchBlockstun` | touché ou gardé accroupi | `hitstun` / `blockstun` |
| `preJump` / `land` | préparation du saut, réception | `crouch` |
| `jump` | en l'air | `idle` |
| `airHitstun` | touché en l'air, jusqu'au sol | `hitstun` |

Les sons sont facultatifs. Ils s'ajoutent sans code (voir l'[architecture](architecture.md)).

## 4. Valider et jouer

1. Dans Unity : *Patate de Forain › Valider les combattants*. Chaque erreur indique le fichier, le chemin JSON et la correction attendue. Par exemple :
   `Erreur — presentation.json › moves.stand_kick_heavy : l'animation « stand_kick_heavy » dure 43 ticks mais le coup en dure 40 dans gameplay.json`.
2. Lancer les tests EditMode. `LePaquetDeSaintLouisEstValide` charge **tous** les paquets du dossier et échoue si l'un d'eux est refusé.
3. **Play**, puis **F3** ou **F4** jusqu'au nouveau combattant, et **F1** pour vérifier que les boîtes collent aux poses.

Un paquet invalide n'empêche pas le jeu de démarrer : il est écarté du catalogue et ses erreurs s'affichent dans la console.

## 5. Un comportement vraiment nouveau

Si le combattant a besoin d'une mécanique qui n'existe pas (contre, téléportation, prise…), elle s'ajoute au **moteur**, une fois, pour tous les combattants :

1. Une classe `SegmentAction` dans `Patate.Combat`, et son traitement dans `SegmentActions`.
2. Sa lecture et ses bornes dans `FighterPackageReader.ReadAction`.
3. Des tests sur le combattant synthétique (`Tests/EditMode/Support/SyntheticFighter.cs`).

Le paquet l'utilise ensuite par son nom (`"type": "…"`). Ne jamais écrire de condition sur l'identifiant d'un personnage.
