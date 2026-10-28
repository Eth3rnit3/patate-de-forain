# Contrôle du décor v0.1

## Contrôle visuel des sources

| Source | Constat |
|---|---|
| Ciel | Conforme du premier coup : opaque, sans texte, encrage et aplats proches des personnages. Sol de terre au bas de l'image, caché par les baraques en jeu. |
| Baraques | Conforme : transparente au-dessus des stands et dans les quatre coins, stands accolés d'un bord à l'autre sur une même ligne de sol. Les panneaux du manège portent des fleurs de lys bleu-vert : clin d'œil à Saint Louis, atténué par l'étalonnage. |
| Sol | Conforme une fois la retouche de Codex écartée (voir le README) : pavés et terre battue, densité régulière, pas d'objet saillant. |

Aucune personne, aucun texte lisible ni logo dans les trois images.

## Réglages en jeu (`stage.json`)

| Calque | Image (px) | Parallaxe | Échelle | Taille affichée (unités) | Bas (y) | Ordre |
|---|---|---:|---:|---|---:|---:|
| `ciel.png` | 1820 × 864, opaque | 20 | 125 | 2275 × 1080 | 0 | -90 |
| `sol.png` | 2172 × 176, opaque | 100 | 135 | 2932 × 238 | -140 | -45 |
| `baraques.png` | 1972 × 628, transparent | 70 | 134 | 2642 × 841 | 40 | -40 |

Fond `#2c2976` (haut du ciel), sol `#281a1f` (bas du sol peint). La bande unie du moteur (ordre -50) reste sous le sol peint.

Couverture : la caméra va de x = -480 à 480 en 16:9. Un calque de parallaxe `p` doit couvrir 1920 + 9,6 × `p` unités, plus 20 pour le tremblement ; en tenant compte de l'élargissement du champ et du recentrage de la caméra, les trois calques couvrent l'écran jusqu'à un format de 1,87:1 (limite des baraques ; 1,94:1 pour le ciel), donc en 16:9 et 16:10, pas en 21:9. Les baraques dessinent une ligne de toits irrégulière : la grande roue et le ciel restent visibles au-dessus des auvents.

## Lisibilité des combattants

Vérifiée sur captures de la version navigateur (`captures/`), Saint Louis contre Saint Louis :

- au centre, au départ de la manche (`webgl-centre.jpg`) ;
- les deux combattants poussés contre le mur droit puis gauche (`webgl-mur-droit.jpg`, `webgl-mur-gauche.jpg`) : aucun bord de calque ni trou visible.

Une première composition avec les baraques brutes montrait un plan moyen aussi vif que les combattants (mêmes rouges saturés, ampoules à hauteur de tête, panneaux du manège bleu et or derrière Saint Louis). Après étalonnage (assombri, désaturé, teinté de violet), les silhouettes encrées bleu et or ressortent nettement ; les couronnes restent lisibles devant les auvents. Le ciel garde ses lumières vives au-dessus des combattants.

Les captures des murs ont été prises avec le ciel à l'échelle 117 ; il est passé à 125 ensuite pour garder une marge en 16:9 (à 117, il ne couvrait que jusqu'à 1,80:1). La capture du centre est celle de la version finale.

## Poids

Compression normale pour les décors (DXT1 opaque, DXT5 transparent), sans agrandissement à l'export. Version navigateur : **14,4 Mo** au total, dont données 6,4 Mo (environ 5 Mo sans décor) et code 7,8 Mo ; soit environ +1,4 Mo pour les trois calques.

## Reste à faire

- Musique du décor.
- Essai sans l'extension BC7/BPTC et sous Safari ou Firefox (les décors, en DXT, n'en dépendent pas ; l'atlas des combattants si).
- Écrans plus larges que 1,87:1 : agrandir ou élargir les baraques et le ciel.
