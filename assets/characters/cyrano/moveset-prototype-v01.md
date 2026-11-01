# Cyrano — pack de combat HD v0.1

Même catalogue et mêmes identifiants d'animations communes que les autres combattants. Toutes les ressources sont des sources à découper et aligner avant intégration.

## Identité de combat proposée

Le bretteur : la rapière lui donne la plus longue allonge debout (poing jusqu'à 465 unités), avec des coups moyens en dégâts et la vitesse de Jeanne à la marche. Il a 950 points de vie. Son spécial est un projectile : un coup de taille qui lance une plume de lumière.

Les différences de vitesse, de portée et de résistance se règlent dans sa propre configuration, sans condition spécifique dans le moteur.

## Catalogue source

| Animation | Poses ciblées | Disposition demandée |
|---|---:|---|
| idle | 6 | 3 × 2 |
| walk_forward | 8 | 4 × 2 |
| stand_punch_light | 6 | 3 × 2 |
| hit_high | 3 | 3 × 1 |
| block_high | 3 | 3 × 1 |
| stand_kick_light | 6 | 3 × 2 |
| stand_kick_heavy | 8 | 4 × 2 |
| special_panache_cast | 8 | 4 × 2 |
| special_panache_projectile | 4 | 2 × 2 |
| special_panache_impact | 4 | 2 × 2 |
| crouch, block_low, hit_crouch | 3 chacune | 3 × 1 |
| crouch_punch, crouch_kick | 6 chacune | 3 × 2 |
| jump | 6 | 3 × 2 |
| jump_punch, jump_kick | 4 chacune | 4 × 1 |
| ko | 6 | 3 × 2 |
| portrait | 1 | portrait seul |

## Le Panache — `special_panache`

Cyrano salue d'un geste du chapeau, arme sa rapière au-dessus de l'épaule et tranche à l'horizontale : une plume blanche de lumière part vers l'adversaire.

- Personnage, plume et impact restent trois ressources indépendantes ; le personnage n'a ni trace ni effet dessiné.
- La pose 5 est le coup de taille, la pose 6 le maintien : la plume est créée une seule fois à l'entrée de la pose 5, au bout de la lame, à hauteur de poitrine.
- Vitesse constante (12 par tick), sans gravité, sans suivi ni rebond ; une seule plume active à la fois ; 80 dégâts.
- Collision compacte autour du cœur de la plume, sans le halo.
- Commande : quart de cercle avant + poing, ou la touche spéciale.

## Timings à essayer dans le moteur

Hypothèses de départ pour une simulation à 60 mises à jour par seconde, à ajuster en jouant.

| Animation | Durées des poses, en mises à jour | Événement |
|---|---|---|
| Poing rapide (estoc) | 2, 3, 2, 3, 4, 7 | Poses 3 et 4 actives |
| Pied rapide | 3, 3, 2, 3, 5, 8 | Pose 4 active |
| Pied puissant | 4, 4, 5, 3, 5, 5, 5, 9 | Pose 5 active |
| Le Panache | 3, 4, 4, 4, 3, 4, 10, 11 | Émission à l'entrée de la pose 5 |
| Plume | 3, 3, 3, 3 en boucle | Translation gérée par la simulation |
| Impact | 2, 2, 3, 3 | Lecture unique puis suppression |
