# Jeanne d'Arc — pack HD v0.1

Cinquième personnage de Patate de Forain. Le pack de sources graphiques suit le même standard que Saint Louis, Napoléon et De Gaulle.

## Direction

- Illustration 2D HD dans le style des autres combattants : encrage fluide, ombres en aplats, dégradés discrets.
- Jeanne d'Arc réinterprétée pour le jeu : jeune femme d'environ 19 ans, courageuse et digne, sans sexualisation.
- Mince, légère et athlétique : la plus petite et la plus rapide du roster, garde à 470 unités.
- Cheveux bruns courts coupés au bol, frange droite.
- Armure de plates d'acier argenté, court tabard blanc liseré de bleu avec une petite croix d'or, épée toujours au fourreau à la hanche gauche. Ni casque, ni bouclier, ni fleur de lys sur le corps (pour rester distincte de Saint Louis).
- Elle se bat avec ses gantelets ; l'étendard n'apparaît que dans son spécial.

Il s'agit d'une réinterprétation pour le jeu, pas d'une reconstitution historique.

## Sources

Générées avec Codex CLI le 1er novembre 2026, sans retouche. Les prompts exacts sont dans `prompts-hd-v01.json`.

```bash
python3 tools/codex-image/generate.py assets/characters/jeanne prompts-hd-v01.json <n>
```

Ordre de génération :

1. **Référence maîtresse**, entrée 0 : `source/jeanne_reference_hd_v01.png`, avec une vue de face, une garde tournée vers la droite, une vue de dos et la palette. Les références des autres combattants n'ont servi que pour le style.
2. **Garde**, entrée 1 : `source/jeanne_idle_hd_v01.png`, générée à partir de la référence seule.
3. **Toutes les autres planches**, entrées 2 à 18, générées avec la référence et la garde en images de référence, pour garder la même échelle et le même rendu.

Le catalogue est celui des autres combattants : garde, marche, poing, deux coups de pied, réaction, blocage, postures, coups accroupis et sautés, KO, portrait, plus le spécial. Le spécial compte une seule planche : la charge à l'étendard (`special_orleans_cast`). Il n'y a ni projectile ni effet dessiné. Les noms exacts, les poses et les réglages sont dans le [moveset](moveset-prototype-v01.md).

## Export

Configuration : `tools/export-character/configs/jeanne.json`. Elle donne 89 poses sur une seule page d'atlas, dans des cellules de 1024 × 768 avec le pivot en (512, 704).

Le spécial prend sa 2ᵉ pose comme référence et s'aligne sur la hauteur de la garde (`matchHeight`) : la hampe levée de la 1ʳᵉ pose faisait rapetisser tout le reste. Ses poses 4 et 6 sont recalées de 300 unités vers l'avant, les poses 3 et 4 du KO de 100 et 190.

## Reprises souhaitées

- Spécial : la hampe dépasse beaucoup la tête sur les poses 1 et 8 ; c'est voulu, mais cela oblige à caler l'échelle sur la garde.
- Garde : six poses très proches, comme chez les autres combattants.
