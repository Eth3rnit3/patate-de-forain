# Cyrano — pack HD v0.1

Sixième personnage de Patate de Forain. Le pack de sources graphiques suit le même standard que Saint Louis, Napoléon et De Gaulle.

## Direction

- Illustration 2D HD dans le style des autres combattants : encrage fluide, ombres en aplats, dégradés discrets.
- Cyrano de Bergerac, poète et bretteur gascon, réinterprété pour le jeu : spirituel et flamboyant, sans caricature grotesque.
- Sec, nerveux et agile, de taille moyenne, garde à 500 unités.
- Très long nez (son trait fameux, porté fièrement), longs cheveux bruns ondulés, fine moustache relevée et petite barbiche.
- Feutre gris à large bord avec un grand plumet blanc (son panache), pourpoint bordeaux à crevés crème, grand col de dentelle blanche, baudrier de cuir, culotte grise, bottes à revers.
- Il tient toujours une rapière d'acier à garde dorée dans la main droite : c'est le seul combattant armé.

Il s'agit d'une réinterprétation pour le jeu, pas d'une reconstitution historique.

## Sources

Générées avec Codex CLI le 1er novembre 2026, sans retouche. Les prompts exacts sont dans `prompts-hd-v01.json`.

```bash
python3 tools/codex-image/generate.py assets/characters/cyrano prompts-hd-v01.json <n>
```

Ordre de génération :

1. **Référence maîtresse**, entrée 0 : `source/cyrano_reference_hd_v01.png`, avec une vue de face, une garde tournée vers la droite, une vue de dos et la palette. Les références des autres combattants n'ont servi que pour le style.
2. **Garde**, entrée 1 : `source/cyrano_idle_hd_v01.png`, générée à partir de la référence seule.
3. **Toutes les autres planches**, entrées 2 à 20, générées avec la référence et la garde en images de référence, pour garder la même échelle et le même rendu.

Le catalogue est celui des autres combattants : garde, marche, poing, deux coups de pied, réaction, blocage, postures, coups accroupis et sautés, KO, portrait, plus le spécial. Le spécial compte trois planches : le lancement (`special_panache_cast`), la plume (`special_panache_projectile`) et l'impact (`special_panache_impact`). Les noms exacts, les poses et les réglages sont dans le [moveset](moveset-prototype-v01.md).

## Export

Configuration : `tools/export-character/configs/cyrano.json`. Elle donne 97 poses sur 2 pages d'atlas, dans des cellules de 1024 × 768 avec le pivot en (512, 704).

Dans le pied puissant, la rapière d'une pose touche la botte de la suivante : deux coutures (`layout.seams`) séparent les poses. La 3ᵉ pose du KO est recalée de 180 unités vers l'avant.

## Reprises souhaitées

- Pied puissant : poses trop serrées sur la planche, séparées à l'export par des coutures.
- Pied sauté : court (260 unités), il manque souvent l'adversaire sur un saut vertical.
- Garde : six poses très proches, comme chez les autres combattants.
