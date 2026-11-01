# Vercingétorix — pack de combat HD v0.1

Même catalogue et mêmes identifiants d'animations communes que les autres combattants. Toutes les ressources sont des sources à découper et aligner avant intégration.

## Identité de combat proposée

Le colosse du roster : lent à démarrer et à marcher, mais plus résistant (1150 points de vie) et plus lourd dans chaque coup. Il ne frappe pas plus loin que les autres debout ; son arme est la charge, qui couvre la moitié de l'écran. Pas de projectile : il doit venir au contact.

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
| special_alesia_cast | 8 | 4 × 2 |
| crouch, block_low, hit_crouch | 3 chacune | 3 × 1 |
| crouch_punch, crouch_kick | 6 chacune | 3 × 2 |
| jump | 6 | 3 × 2 |
| jump_punch, jump_kick | 4 chacune | 4 × 1 |
| ko | 6 | 3 × 2 |
| portrait | 1 | portrait seul |

## Charge d'Alésia — `special_alesia`

Vercingétorix baisse le centre de gravité, rugit et fonce épaule en avant sur l'adversaire, comme un bélier.

- Une seule planche, sans effet : la translation est faite par la simulation (`advance` des segments), les poses sont dessinées sur place.
- Poses 4 à 6 : la course et l'impact d'épaule, actives ; pose 7 : le freinage, pieds écartés.
- Pendant les poses actives il avance de 14 unités par tick, soit environ 250 unités, puis de 3 par tick pendant le freinage.
- Un seul coup par charge : 120 dégâts, gros recul. Gardée, elle le laisse à portée de punition.
- Commande : quart de cercle avant + poing, ou la touche spéciale.

## Timings à essayer dans le moteur

Hypothèses de départ pour une simulation à 60 mises à jour par seconde, à ajuster en jouant.

| Animation | Durées des poses, en mises à jour | Événement |
|---|---|---|
| Poing rapide | 3, 3, 3, 3, 5, 8 | Pose 4 active |
| Pied rapide | 3, 3, 3, 3, 5, 9 | Pose 4 active |
| Pied puissant | 4, 5, 5, 5, 6, 5, 5, 11 | Pose 5 active |
| Charge d'Alésia | 4, 5, 5, 6, 6, 6, 8, 12 | Poses 4 à 6 actives, avance 14 par tick |
