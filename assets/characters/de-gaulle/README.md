# De Gaulle — pack HD v0.1

Troisième personnage de Patate de Forain. Le pack de sources graphiques suit le même standard que Saint Louis et Napoléon.

## Direction

- Illustration 2D HD dans le style de Saint Louis et de Napoléon : encrage fluide, ombres en aplats, dégradés discrets.
- Charles de Gaulle en 1940, réinterprété pour le jeu, reconnaissable et sans caricature grotesque.
- Très grand, mince, longs bras et longues jambes : le plus grand du roster, avec une garde à 560 unités contre 500 et 480.
- Long visage, grand nez, petite moustache, cheveux courts sombres.
- Képi kaki de général : bandeau sombre, feuilles de chêne brodées d'or, deux étoiles.
- Vareuse kaki à quatre poches et boutons dorés, ceinturon et baudrier de cuir brun, barrette tricolore et croix de Lorraine sur la poitrine, culotte kaki, bottes de cuir brun.
- Ni manteau, ni arme : il se bat à mains nues.

Il s'agit d'une réinterprétation pour le jeu, pas d'une reconstitution historique.

## Sources

Générées avec Codex CLI le 1er novembre 2026, sans retouche. Les prompts exacts sont dans `prompts-hd-v01.json`.

```bash
python3 tools/codex-image/generate.py assets/characters/de-gaulle prompts-hd-v01.json <n>
```

Ordre de génération :

1. **Référence maîtresse**, entrée 0 : `source/de_gaulle_reference_hd_v01.png`, avec une vue de face, une garde tournée vers la droite, une vue de dos et la palette. Les références de Saint Louis et de Napoléon n'ont servi que pour le style.
2. **Garde**, entrée 1 : `source/de_gaulle_idle_hd_v01.png`, générée à partir de la référence seule.
3. **Toutes les autres planches**, entrées 2 à 20, générées avec la référence et la garde en images de référence, pour garder la même échelle et le même rendu.

Le catalogue est celui de Napoléon : garde, marche, poing, deux coups de pied, réaction, blocage, postures, coups accroupis et sautés, KO, portrait, plus le spécial. Le spécial compte trois planches : le lancement (`special_appel_cast`), l'onde (`special_appel_projectile`) et l'impact (`special_appel_impact`). Les noms exacts, les poses et les réglages sont dans le [moveset](moveset-prototype-v01.md).

## Export

Configuration : `tools/export-character/configs/de-gaulle.json`. Elle donne 97 poses sur 2 pages d'atlas, dans des cellules de 1088 × 768 avec le pivot en (544, 704).

Le KO est recalé avec `nudge` sur les poses 3, 4 et 6. Sans ce recalage, la chute sautait d'environ 400 unités en arrière entre les poses 2 et 3.

## Reprises souhaitées

- Réaction accroupie : les poses 1 et 2 ne sont qu'à demi accroupies.
- Garde : six poses très proches, comme chez les autres combattants.
- Spécial : dans le V, le personnage se tourne de face. C'est lisible, mais moins dans l'axe que le reste du jeu.
