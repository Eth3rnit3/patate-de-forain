# Jeanne d'Arc — pack de combat HD v0.1

Même catalogue et mêmes identifiants d'animations communes que les autres combattants. Toutes les ressources sont des sources à découper et aligner avant intégration.

## Identité de combat proposée

La plus rapide du roster : poing en 4 ticks, marche la plus vive, mais 950 points de vie et des coups qui font moins mal. Ses coups sautés portent loin (pied sauté jusqu'à 470 unités). Son spécial est une charge éclair à l'étendard, plus rapide et plus longue que celle de Vercingétorix, mais moins puissante.

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
| special_orleans_cast | 8 | 4 × 2 |
| crouch, block_low, hit_crouch | 3 chacune | 3 × 1 |
| crouch_punch, crouch_kick | 6 chacune | 3 × 2 |
| jump | 6 | 3 × 2 |
| jump_punch, jump_kick | 4 chacune | 4 × 1 |
| ko | 6 | 3 × 2 |
| portrait | 1 | portrait seul |

## Étendard d'Orléans — `special_orleans`

Jeanne abaisse son étendard comme une lance, s'élance et transperce la garde adverse d'une fente, fanion au vent.

- Une seule planche, sans effet : l'étendard (hampe et fanion blanc semé de petites fleurs de lys d'or) n'apparaît que dans ce spécial.
- La translation est faite par la simulation (`advance`), les poses sont dessinées sur place.
- Poses 4 à 6 actives, la 5ᵉ (la fente) porte jusqu'à 460 unités devant elle. Elle avance de 18 unités par tick, soit environ 290 unités.
- Un seul coup par charge : 90 dégâts.
- Commande : quart de cercle avant + poing, ou la touche spéciale.

## Timings à essayer dans le moteur

Hypothèses de départ pour une simulation à 60 mises à jour par seconde, à ajuster en jouant.

| Animation | Durées des poses, en mises à jour | Événement |
|---|---|---|
| Poing rapide | 2, 2, 2, 3, 3, 5 | Poses 3 et 4 actives |
| Pied rapide | 2, 3, 2, 3, 4, 7 | Pose 4 active |
| Pied puissant | 3, 4, 4, 3, 5, 5, 4, 8 | Pose 5 active |
| Étendard d'Orléans | 3, 4, 4, 5, 6, 5, 6, 10 | Poses 4 à 6 actives, avance 18 par tick |
