# Vercingétorix — pack HD v0.1

Quatrième personnage de Patate de Forain. Le pack de sources graphiques suit le même standard que Saint Louis, Napoléon et De Gaulle.

## Direction

- Illustration 2D HD dans le style des autres combattants : encrage fluide, ombres en aplats, dégradés discrets.
- Vercingétorix, le chef arverne d'Alésia, réinterprété pour le jeu, fier et charismatique, sans rien du Gaulois de bande dessinée.
- Colosse : le plus large et le plus lourd du roster, cou épais, épaules et avant-bras massifs, garde à 520 unités.
- Longs cheveux blond cuivré avec deux fines tresses aux tempes, grande moustache tombante, sans barbe.
- Torque d'or au cou, bracelets de bronze, tunique sans manches à carreaux rouges et vert sombre, large ceinture de cuir à boucle de bronze, braies à carreaux bleus et ocre lacées aux mollets, chaussures de cuir.
- Ni casque ailé, ni bouclier, ni épée : il se bat à poings nus.

Il s'agit d'une réinterprétation pour le jeu, pas d'une reconstitution historique.

## Sources

Générées avec Codex CLI le 1er novembre 2026, sans retouche. Les prompts exacts sont dans `prompts-hd-v01.json`.

```bash
python3 tools/codex-image/generate.py assets/characters/vercingetorix prompts-hd-v01.json <n>
```

Ordre de génération :

1. **Référence maîtresse**, entrée 0 : `source/vercingetorix_reference_hd_v01.png`, avec une vue de face, une garde tournée vers la droite, une vue de dos et la palette. Les références des autres combattants n'ont servi que pour le style.
2. **Garde**, entrée 1 : `source/vercingetorix_idle_hd_v01.png`, générée à partir de la référence seule.
3. **Toutes les autres planches**, entrées 2 à 18, générées avec la référence et la garde en images de référence, pour garder la même échelle et le même rendu.

Le catalogue est celui des autres combattants : garde, marche, poing, deux coups de pied, réaction, blocage, postures, coups accroupis et sautés, KO, portrait, plus le spécial. Le spécial compte une seule planche : la charge (`special_alesia_cast`). Il n'y a ni projectile ni effet dessiné. Les noms exacts, les poses et les réglages sont dans le [moveset](moveset-prototype-v01.md).

## Export

Configuration : `tools/export-character/configs/vercingetorix.json`. Elle donne 89 poses sur 2 pages d'atlas, dans des cellules de 1088 × 768 avec le pivot en (544, 704).

Deux poses sont recalées avec `nudge` : la 6ᵉ pose de la charge (+470), sans quoi la seconde foulée reculait d'un demi-corps, et la 3ᵉ pose du KO (+260), pour que la chute ne saute pas en arrière.

## Reprises souhaitées

- Charge : la 6ᵉ pose est dessinée très en retrait des autres ; elle est recalée à l'export, mais une foulée redessinée dans l'axe serait plus propre.
- Garde : six poses très proches, comme chez les autres combattants.
