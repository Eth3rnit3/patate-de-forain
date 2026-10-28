# Fête foraine — décor v0.1

Premier décor du jeu, et décor par défaut des combats (`MatchSetup.Stage`). Une fête foraine française au crépuscule, dans la direction des personnages : illustration 2D HD, encrage fluide, ombres en aplats. Trois calques pour la parallaxe ; pas de premier plan, pour ne rien poser devant les combattants ni sous la barre de vie.

Sources générées avec le générateur d'images de Codex CLI (`codex exec --skip-git-repo-check -m gpt-5.6-sol --enable image_generation -s workspace-write`), avec en référence la référence maîtresse de Saint Louis (rendu et encrage), le logo (énergie des couleurs) puis le fond lointain (palette, lumière et horizon des deux autres calques).

## Contenu

- `source/fete_foraine_ciel_v01.png` : fond lointain, 1821 × 864, opaque. Ciel du soir violet et abricot, grande roue illuminée, chapiteau rayé, montagnes russes en bois, clocher d'un village.
- `source/fete_foraine_baraques_v01.png` : plan moyen, 1974 × 797, transparent. Stand de tir, pêche aux canards, confiserie, manège de chevaux de bois, tour de force (« tape-tête »), tonneaux, bottes de paille, réverbères.
- `source/fete_foraine_sol_v01.png` : sol de l'allée, 2172 × 724, opaque. Terre battue et vieux pavés, sciure, confettis.
- `export/` : images nettoyées, copiées telles quelles dans `game/Assets/Resources/Stages/fete-foraine/`.
- `prompts-v01.json` : prompts exacts, images de référence données à Codex (`-i`, dans l'ordre) et consigne d'enregistrement.
- `captures/` : captures de la version navigateur qui servent au contrôle de lisibilité.
- [Contrôle et réglages](validation-v01.md).

Les sources sont les sorties brutes du générateur. Pour le sol et les baraques, Codex avait retouché le fichier après génération (sol mis en miroir et écrasé à 2912 × 220, alpha des baraques effacé par un seuil raté) : ce sont les images d'origine, reprises dans `~/.codex/generated_images/`, qui sont conservées ici. Demander à Codex de ne pas retoucher l'image s'il faut en refaire une.

## Export

Aucun agrandissement : les calques gardent la résolution du générateur et le champ `scale` de `stage.json` les agrandit à l'affichage (de 125 à 135 %), ce qui évite d'alourdir la version navigateur de pixels sans détail. Dimensions multiples de 4 pour la compression par blocs. Commandes (ImageMagick), depuis ce dossier :

```bash
magick source/fete_foraine_ciel_v01.png -crop 1820x864+0+0 +repage -alpha off -strip PNG24:export/ciel.png
magick source/fete_foraine_baraques_v01.png -crop 1972x628+1+128 +repage -channel A -fx 'u<0.02?0:u' +channel -modulate 78,70,100 \( +clone -alpha off -fill '#3a2a5c' -colorize 100 \) -compose blend -define compose:args=12 -composite -compose over -strip PNG32:export/baraques.png
magick source/fete_foraine_sol_v01.png -crop 2172x352+0+372 +repage -resize 2172x176! -modulate 88,85,100 -alpha off -strip PNG24:export/sol.png
```

- **Ciel** : une colonne retirée pour la largeur, opaque (DXT1).
- **Baraques** : marge transparente du haut retirée, alpha quasi nul ramené à 0, puis assombries (78 %), désaturées (70 %) et teintées de violet du soir (12 %) pour les repousser derrière les combattants.
- **Sol** : la moitié basse de la source (pavés proches), tassée de moitié en hauteur pour un angle rasant, légèrement assombrie.

## Statut

Décor jouable et vérifié en WebGL derrière un combat (voir la validation). Pas de musique : `stage.json` accepte un champ `music` (`.ogg` ou `.wav` dans le dossier du décor) dès qu'elle existe.
