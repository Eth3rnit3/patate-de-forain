# Standard personnage — v0.1

Premier personnage : Saint Louis (Louis IX), réinterprétation arcade originale.

## Direction graphique

- Direction validée : illustration 2D HD, contours encrés fluides, ombres en aplats et dégradés discrets. Pas de pixel art, de rendu 3D ou de photoréalisme.
- Référence maîtresse de style : `assets/characters/saint-louis/source/saint_louis_reference_hd_v01.png`. Chaque combattant possède ensuite sa propre référence d'identité.
- Échelle cible à confirmer dans le moteur : environ 600 pixels du sommet du crâne aux semelles pour un combattant de taille moyenne.
- Sources générées : références à nettoyer avant intégration ; vérifier proportions, détails du costume, transparence et alignement entre poses.
- Personnage orienté vers la droite. Les deux orientations peuvent être obtenues par miroir si l'inversion des accessoires est acceptée.
- Source sur fond transparent pour les sprites et le portrait ; fond neutre autorisé pour la référence.

## Export après nettoyage

- PNG RGBA, sans fond ni quadrillage dessiné ; filtrage linéaire adapté au rendu HD. Contrôler les franges alpha sur fonds clairs et sombres.
- Cellule cible provisoire : 1024 × 768 pixels ; pivot commun (512, 704), sur le sol. Agrandir uniformément les cellules si une attaque dépasse.
- Les dimensions ci-dessus concernent les exports nettoyés, pas les planches sources générées.
- Ne pas recentrer chaque pose automatiquement : conserver le pivot et les déplacements intentionnels.
- Images individuelles : `<fighter_id>_<animation>_<index sur 2 chiffres>.png` ; exemples d'identifiants : `saint_louis`, `napoleon`.
- Atlas et métadonnées créés après validation dans le moteur choisi.
- Ne pas cuire les hitboxes, hurtboxes ou ombres de sol dans l'image.

## Pack de validation

| Animation / asset | Cible | Usage |
|---|---:|---|
| Référence | face, garde, dos | identité et proportions |
| `idle` | 6 images | boucle de garde |
| `walk_forward` | 8 images | déplacement vers la droite |
| `stand_punch_light` | 6 images | anticipation, extension, récupération |
| `hit_high` | 3 images | réaction au coup |
| `block_high` | 3 images | blocage debout |
| Portrait | 1 image | sélection |
| `stand_kick_light` | 6 images | pied frontal rapide |
| `stand_kick_heavy` | 8 images | pied latéral haut puissant |
| Spécial : lancement | 8 images | préparation, émission, récupération |
| Spécial : projectile | 4 images | boucle de déplacement séparée |
| Spécial : impact | 4 images | effet de contact séparé |

Postures (prompts `prompts-hd-extension-v02.json` de chaque personnage) :

| Animation / asset | Cible | Usage |
|---|---:|---|
| `crouch` | 3 images | descente et tenue accroupie |
| `block_low` | 3 images | garde accroupie |
| `hit_crouch` | 3 images | réaction accroupie |
| `crouch_punch` | 6 images | poing accroupi |
| `crouch_kick` | 6 images | balayette |
| `jump` | 6 images | préparation, montée, sommet, descente, réception |
| `jump_punch` / `jump_kick` | 4 images chacun | coups sautés |
| `ko` | 6 images | chute, puis allongé ; habits déchirés et taches de sang, sans gore |

Le nombre et l'ordre effectifs des poses doivent être contrôlés sur chaque source. Une planche illustrative n'est pas un atlas prêt à découper.

Les effets des coups spéciaux sont exportés séparément du personnage : point d'émission relatif au pivot, vitesse et collision du projectile définies dans le moteur, déclenchement unique sur une pose précise, impact joué une fois au contact. La taille de la collision ne doit pas inclure le halo lumineux.

## Saint Louis — identité v0.1

Roi adulte, athlétique et sans barbe ; cheveux châtains mi-longs, couronne basse dorée, surcot bleu royal à grandes fleurs de lys dorées, manches de mailles, ceinture et bottes en cuir brun, sous-tunique crème. Pas de cape, pas de bouclier. Épée au fourreau pour ce pack ; combat à mains nues.

## Données à ajouter dans le moteur

Durée de chaque frame, boucle ou transition, pivot, déplacement, collision entre combattants, hurtboxes, hitboxes, démarrage/activité/récupération des attaques, dégâts, étourdissement, recul, blocage autorisé, déclenchement des sons et effets.

Ces valeurs ne sont pas déduites automatiquement des images. Elles seront définies après choix du moteur et test de jouabilité.

## Validation avant production complète

Vérifier silhouette à la taille de jeu, anatomie, continuité de la tenue, position des pieds, palette, boucle de garde, cycle de marche et lisibilité du coup. Nettoyer les contours et l'alpha, puis tester en jeu avant de décliner les 18 attaques normales et les coups spéciaux.

Les premiers essais pixel art sont conservés pour historique et ne constituent plus la référence de production.
