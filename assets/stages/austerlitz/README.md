# Austerlitz — décor v0.1

Le champ de bataille d'Austerlitz un matin d'hiver de 1805 : le « soleil d'Austerlitz » perce la brume de la vallée, collines enneigées, village et clocher, moulins. Au plan moyen, le bivouac de la Grande Armée (tentes, canons, faisceaux de fusils, tambour, chariots, feux de camp, drapeau tricolore sans inscription). Décor de Napoléon.

Même recette que la [fête foraine](../fete-foraine/README.md) : trois calques pour la parallaxe, pas de premier plan. Sources générées avec le générateur d'images de Codex CLI le 31 octobre 2026, par `tools/codex-image/generate.py` (`codex exec -m gpt-5.6-sol --enable image_generation -s workspace-write`, lancé dans ce dossier). Références données à Codex (`-i`, dans l'ordre) : pour le ciel, la référence maîtresse du personnage (rendu et encrage) et le ciel de la fête foraine (niveau de détail et cadrage) ; pour les deux autres calques, le ciel de ce décor (palette, lumière, horizon) puis la référence du personnage.

## Contenu

- `source/austerlitz_ciel_v01.png` : fond lointain, 1983 × 793, opaque.
- `source/austerlitz_plan_moyen_v01.png` : plan moyen, 1983 × 793, transparent.
- `source/austerlitz_sol_v01.png` : sol, 1983 × 793, opaque.
- `export/` : images du jeu, copiées telles quelles dans `game/Assets/Resources/Stages/austerlitz/`.
- `prompts-v01.json` : prompts exacts, images de référence et consigne d'enregistrement. Pour régénérer un calque, depuis la racine du dépôt : `python3 tools/codex-image/generate.py assets/stages/austerlitz prompts-v01.json <0, 1 ou 2>`.

Les sources sont les sorties brutes du générateur ; Codex avait pour consigne de ne pas les retoucher.

## Export

Aucun agrandissement : `scale` dans `stage.json` agrandit à l'affichage. Dimensions multiples de 4. Depuis ce dossier :

```bash
magick source/austerlitz_ciel_v01.png -crop 1980x792+1+0 +repage -alpha off -strip PNG24:export/ciel.png
magick source/austerlitz_plan_moyen_v01.png -crop 1980x516+1+211 +repage -channel A -fx 'u<0.02?0:u' +channel -modulate 80,72,100 \( +clone -fill '#3a3a62' -colorize 100 \) -compose blend -define compose:args=12 -composite -compose over -strip PNG32:export/plan-moyen.png
magick source/austerlitz_sol_v01.png -crop 1980x396+1+396 +repage -resize 1980x176! -modulate 85,85,100 -alpha off -strip PNG24:export/sol.png
```

- **Ciel** : trois colonnes retirées pour la largeur, opaque.
- **Plan moyen** : marges transparentes retirées, alpha quasi nul ramené à 0, assombri (80 %), désaturé (72 %) et teinté à 12 % (#3a3a62) pour le repousser derrière les combattants. La teinte garde l'alpha de l'image.
- **Sol** : la moitié basse de la source (la plus proche), tassée de moitié en hauteur pour un angle rasant, légèrement assombrie.

## Réglages en jeu (`stage.json`)

| Calque | Image (px) | Parallaxe | Échelle | Taille affichée (unités) | Bas (y) | Ordre |
|---|---|---:|---:|---|---:|---:|
| `ciel.png` | 1980 × 792, opaque | 20 | 125 | 2475 × 990 | 0 | -90 |
| `sol.png` | 1980 × 176, opaque | 100 | 148 | 2930 × 260 | -150 | -45 |
| `plan-moyen.png` | 1980 × 516, transparent | 70 | 134 | 2653 × 691 | 40 | -40 |

Couverture, selon la règle de la fête foraine (1920 + 9,6 × parallaxe + 20 unités en 16:9) : ciel 2475 ≥ 2132, plan moyen 2653 ≥ 2612, sol 2930 ≥ 2900. Le sol est à 148 % (135 % pour la fête foraine) parce que l'image est moins large.

## Contrôle

Aucune personne, aucun texte lisible ni logo dans les trois images.

Captures (`captures/`) :

- `webgl-combat.jpg` : Napoléon contre Saint Louis dans la version navigateur, interface de combat comprise.
- `webgl-mur-gauche.jpg`, `webgl-mur-droit.jpg` : les deux combattants acculés contre chaque mur. Aucun bord de calque ni trou n'est visible.
- `editeur-etincelle-coup.jpg` : pied puissant de Saint Louis qui touche, avec l'étincelle commune, dans l'éditeur (scénario `AgentScripts/VerifyEffects.cs`).

Le drapeau tricolore du bivouac se trouve à peu près au centre de l'arène. Il passe derrière les combattants sans gêner la lecture, parce que le plan moyen est assombri. Les panaches de fumée gardent un léger liseré coloré sur leurs bords transparents ; à la taille du jeu, il ne se voit pas.
