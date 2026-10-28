# Napoléon HD — contrôle des sources

## Contrôle visuel

| Source | Poses visibles | Points à reprendre avant intégration |
|---|---:|---|
| Garde | 6 | Stabiliser hauteur, boutons, médaille, position des bottes et plis |
| Marche | 8 | Différences de jambes lisibles ; vérifier l'alternance exacte des appuis et le raccord de boucle |
| Poing rapide | 6 | Frappe et retour lisibles ; harmoniser les poses d'extension et les appuis |
| Réaction haute | 3 | Recul lisible ; marge étroite entre les bottes des poses 2 et 3, contrôler leur séparation au découpage |
| Blocage haut | 3 | Avant-bras protecteurs lisibles ; échelle et hauteur à recaler entre les poses |
| Pied rapide | 6 | Armé, frappe et repli lisibles ; harmoniser position du sabre et mouvement des pans |
| Pied puissant | 8 | Extension haute lisible ; pose étendue débordant de la grille théorique, définir des rectangles de découpage adaptés |
| Salve impériale | 8 | Geste de commandement lisible ; pose 5 d'émission et pose 6 de maintien à recaler |
| Portrait | 1 | Bicorne entier visible ; adapter le cadrage à l'interface |
| Boulet spectral | 4 | Grille 2 × 2, même sens de déplacement ; aligner le centre du boulet et nettoyer la traînée |
| Impact spectral | 4 | Grille 2 × 2, expansion puis dispersion ; réduire les résidus colorés et vérifier les marges de l'effet maximal |

Les images sont des sources graphiques, pas des animations validées. Le découpage et l'alignement doivent conserver une échelle corporelle et un pivot au sol communs. La redingote et le sabre ne doivent pas entraîner de recentrage automatique des poses.

## Intégration

Conserver les originaux dans `source/`. Créer les exports nettoyés, atlas et métadonnées séparément. Les grilles archivées dans les prompts sont des intentions de génération ; contrôler leur conformité sur chaque planche. Tester les contours sur fond clair et sombre et nettoyer toute frange colorée.

Le moteur doit gérer les timings, collisions, projectiles, orientation et sons. Aucun essai en jeu n'a encore été réalisé sur ces sources.

## Contrôle technique final

Les onze sorties du pack sont présentes, lisibles comme PNG, avec canal alpha et au moins un pixel entièrement transparent sur leur première ligne. Elles couvrent les 56 poses de combat et d'effets demandées ainsi qu'un portrait. Leurs dimensions réelles sont consignées dans `inventory-hd-v01.json` ; la référence grise reste volontairement opaque.

Le contrôle de transparence confirme l'existence de zones vides, pas la qualité de chaque contour. Les boucles et timings restent à vérifier dans le moteur après retouche et découpage.
