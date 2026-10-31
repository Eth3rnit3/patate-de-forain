# Interface et effets — v0.1

Ces éléments habillent l'interface de combat et les menus, en plus du fond de menu, du logo et des mots d'annonce déjà livrés dans `game/Assets/Resources/UI/`. On y trouve les cadres de la jauge, du portrait et du chronomètre, l'emblème « VS », les fonds des écrans de sélection et de résultat, et les étincelles des coups au corps.

Les sources ont été générées avec le générateur d'images de Codex CLI le 31 octobre 2026, avec `tools/codex-image/generate.py` lancé dans ce dossier. Les images de référence données à Codex (`-i`, dans l'ordre) sont :
- le logo du jeu, pour le lettrage et la palette ;
- le fond de menu, pour le rendu ;
- pour les deux fonds d'écran, en plus, la référence maîtresse de Saint Louis, pour l'encrage.

Les prompts exacts sont dans `prompts-v01.json`. Pour régénérer une entrée, depuis la racine du dépôt :

```bash
python3 tools/codex-image/generate.py assets/ui prompts-v01.json <n>
```

Les sources sont les sorties brutes du générateur. Codex avait pour consigne de ne pas les retoucher.

## Contenu et usage en jeu

| Source | Export (jeu) | Où | Remarque |
|---|---|---|---|
| `jauge_cadre_v01.png` 2172 × 724 | `UI/hud/jauge-cadre.png` 1088 × 200 | Jauge de vie, miroir pour J2 | Fenêtre transparente : x 0,111 → 0,874, y 0,3225 → 0,6825 de l'export |
| `portrait_cadre_v01.png` 1254 × 1254 | `UI/hud/portrait-cadre.png` 512 × 512 | Portrait du HUD | Fenêtre : x 0,1874 → 0,8126, y 0,2257 → 0,7759 |
| `chrono_medaillon_v01.png` 1254 × 1254 | `UI/hud/chrono.png` 512 × 512 | Chronomètre | Disque bleu marine opaque : centre (0,4992 ; 0,563), rayon 0,2584 |
| `vs_v01.png` 1254 × 1254 | `UI/vs.png` 816 × 628 | Sélection, quand tout le monde est prêt | |
| `selection_fond_v01.png` 1672 × 941 | `UI/selection-fond.jpg` 1672 × 940 | Fond de la sélection | Baraque de lutteurs foraine, scène vide |
| `resultat_fond_v01.png` 1672 × 941 | `UI/resultat-fond.jpg` 1672 × 940 | Fond du résultat | Podium, projecteur et feux d'artifice |
| `etincelle_coup_v01.png` 1254 × 1254 | `Effects/etincelle-coup/1…4.png` 450 × 320 | Coup au corps qui touche | Planche 2 × 2 découpée image par image |
| `etincelle_garde_v01.png` 1254 × 1254 | `Effects/etincelle-garde/1…4.png` 450 × 320 | Coup au corps gardé | idem |

Les fractions de fenêtre sont mesurées sur les images, en suivant la transparence au centre de chaque ligne et de chaque colonne. Elles sont reprises dans `Hud.cs`. Un élément absent de `Resources` est remplacé par le dessin d'origine : cadres unis, texte « COMBAT ! » à la place du « VS », fond de menu commun.

## Export

Depuis ce dossier :

```bash
# Cadres : alpha sous 8 % ramené à 0 (bruit du générateur), fenêtre vidée (quelques points semi-opaques sur ses bords), réduction.
magick source/jauge_cadre_v01.png -crop 2172x400+0+148 +repage -channel A -fx 'u<0.08?0:u' +channel -region 1650x137+245+133 -alpha transparent +region -resize 1088x200! -strip PNG32:export/hud/jauge-cadre.png
magick source/portrait_cadre_v01.png -channel A -fx 'u<0.08?0:u' +channel -region 776x682+239+287 -alpha transparent +region -resize 512x512 -strip PNG32:export/hud/portrait-cadre.png
magick source/chrono_medaillon_v01.png -channel A -fx 'u<0.08?0:u' +channel -resize 512x512 -strip PNG32:export/hud/chrono.png
magick source/vs_v01.png -crop 1224x944+27+161 +repage -channel A -fx 'u<0.08?0:u' +channel -resize 816x628! -strip PNG32:export/vs.png
magick source/selection_fond_v01.png -crop 1672x940+0+0 +repage -strip -quality 88 export/selection-fond.jpg
magick source/resultat_fond_v01.png -crop 1672x940+0+0 +repage -strip -quality 88 export/resultat-fond.jpg
```

**Étincelles.** Chaque image est découpée dans sa case de la planche, puis posée sur une toile de 900 × 640 où le point d'impact tombe en (300, 320). La toile est ensuite réduite de moitié. Le point d'impact se trouve donc au tiers gauche et à mi-hauteur de chaque image : c'est le pivot de `SharedEffects`. L'étincelle est dessinée vers la droite et retournée selon le sens du coup.

Cases et points d'impact, en pixels de la source :

| Planche | Image | Case (x0, y0, x1, y1) | Point d'impact |
|---|---|---|---|
| `etincelle_coup` | 1 | (0, 0, 525, 645) | (300, 340) |
| | 2 | (525, 0, 1254, 645) | (700, 330) |
| | 3 | (0, 645, 660, 1254) | (190, 935) |
| | 4 | (660, 645, 1254, 1254) | (820, 940) |
| `etincelle_garde` | 1 | (0, 0, 600, 640) | (330, 340) |
| | 2 | (600, 0, 1254, 640) | (900, 340) |
| | 3 | (0, 640, 600, 1254) | (330, 940) |
| | 4 | (600, 640, 1254, 1254) | (920, 940) |

```bash
magick source/etincelle_coup_v01.png -crop 729x645+525+0 +repage -channel A -fx 'u<0.08?0:u' +channel -background none -gravity NorthWest -extent 900x640-125+10 -resize 50% +repage -strip PNG32:export/effets/etincelle-coup/2.png
```

Cette commande donne l'image 2. Pour les autres, on adapte la case et le décalage : décalage = (impact − coin de la case) − (300, 320).

## Contrôle

- Transparence : les quatre coins sont à alpha 0, ainsi que le centre de la fenêtre des cadres.
- Aucun texte parasite. Le « VS » ne contient que ces deux lettres.
- Les fleurs de lys des cadres sont un clin d'œil assumé à la palette royale de l'interface ; elles n'apparaissent pas dans les décors du combat.
- Rendu en jeu : voir les captures du README principal.
