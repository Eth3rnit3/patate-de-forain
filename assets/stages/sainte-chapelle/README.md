# Sainte-Chapelle — décor v0.1

L'île de la Cité au Moyen Âge, de nuit : la Sainte-Chapelle et ses vitraux allumés, Notre-Dame au loin, la Seine et un pont de pierre. Au plan moyen, une foire médiévale dans la cour royale (étals, forge, tonneaux, bannières unies, torches) devant un mur à arcades. Décor de Saint Louis.

Même recette que la [fête foraine](../fete-foraine/README.md) : trois calques pour la parallaxe, pas de premier plan. Sources générées avec le générateur d'images de Codex CLI le 31 octobre 2026, par `tools/codex-image/generate.py` (`codex exec -m gpt-5.6-sol --enable image_generation -s workspace-write`, lancé dans ce dossier). Références données à Codex (`-i`, dans l'ordre) : pour le ciel, la référence maîtresse du personnage (rendu et encrage) et le ciel de la fête foraine (niveau de détail et cadrage) ; pour les deux autres calques, le ciel de ce décor (palette, lumière, horizon) puis la référence du personnage.

## Contenu

- `source/sainte_chapelle_ciel_v01.png` : fond lointain, 1983 × 793, opaque.
- `source/sainte_chapelle_plan_moyen_v01.png` : plan moyen, 1983 × 793, transparent.
- `source/sainte_chapelle_sol_v01.png` : sol, 1983 × 793, opaque.
- `export/` : images du jeu, copiées telles quelles dans `game/Assets/Resources/Stages/sainte-chapelle/`.
- `prompts-v01.json` : prompts exacts, images de référence et consigne d'enregistrement. Pour régénérer un calque, depuis la racine du dépôt : `python3 tools/codex-image/generate.py assets/stages/sainte-chapelle prompts-v01.json <0, 1 ou 2>`.

Les sources sont les sorties brutes du générateur ; Codex avait pour consigne de ne pas les retoucher.

## Export

Aucun agrandissement : `scale` dans `stage.json` agrandit à l'affichage. Dimensions multiples de 4. Depuis ce dossier :

```bash
magick source/sainte_chapelle_ciel_v01.png -crop 1980x792+1+0 +repage -alpha off -strip PNG24:export/ciel.png
magick source/sainte_chapelle_plan_moyen_v01.png -crop 1980x584+1+130 +repage -channel A -fx 'u<0.02?0:u' +channel -modulate 80,72,100 \( +clone -fill '#2a2a5c' -colorize 100 \) -compose blend -define compose:args=12 -composite -compose over -strip PNG32:export/plan-moyen.png
magick source/sainte_chapelle_sol_v01.png -crop 1980x396+1+396 +repage -resize 1980x176! -modulate 85,85,100 -alpha off -strip PNG24:export/sol.png
```

- **Ciel** : trois colonnes retirées pour la largeur, opaque.
- **Plan moyen** : marges transparentes retirées, alpha quasi nul ramené à 0, assombri (80 %), désaturé (72 %) et teinté à 12 % (#2a2a5c) pour le repousser derrière les combattants. La teinte garde l'alpha de l'image.
- **Sol** : la moitié basse de la source (la plus proche), tassée de moitié en hauteur pour un angle rasant, légèrement assombrie.

## Réglages en jeu (`stage.json`)

| Calque | Image (px) | Parallaxe | Échelle | Taille affichée (unités) | Bas (y) | Ordre |
|---|---|---:|---:|---|---:|---:|
| `ciel.png` | 1980 × 792, opaque | 20 | 125 | 2475 × 990 | 0 | -90 |
| `sol.png` | 1980 × 176, opaque | 100 | 148 | 2930 × 260 | -150 | -45 |
| `plan-moyen.png` | 1980 × 584, transparent | 70 | 134 | 2653 × 782 | 40 | -40 |

Couverture, selon la règle de la fête foraine (1920 + 9,6 × parallaxe + 20 unités en 16:9) : ciel 2475 ≥ 2132, plan moyen 2653 ≥ 2612, sol 2930 ≥ 2900. Le sol est à 148 % (135 % pour la fête foraine) parce que l'image est moins large.

## Contrôle

Aucune personne, aucun texte lisible ni logo dans les trois images.

Captures (`captures/`) :

- `editeur-etincelle-garde.jpg` : pied puissant de Saint Louis gardé par Napoléon, avec l'étincelle de garde, dans l'éditeur (scénario `AgentScripts/VerifyEffects.cs`).

La couverture n'a pas été contrôlée aux murs dans ce décor. La géométrie (largeurs et échelles) y est la même qu'à Austerlitz, où elle l'a été. Les vitraux de la chapelle et la lune restent au-dessus des têtes. Les bannières du plan moyen sont unies. Celles du ciel portent des fleurs de lys, mais aux deux extrémités de l'image, hors du champ habituel de la caméra.
