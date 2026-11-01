# De Gaulle — pack de combat HD v0.1

Même catalogue et mêmes identifiants d'animations communes que Saint Louis et Napoléon. Toutes les ressources sont des sources à découper et aligner avant intégration.

## Identité de combat proposée

Le plus grand du roster : longs bras, longues jambes, garde haute. Il gagne sur l'allonge et tient l'adversaire à distance, mais il démarre un peu plus lentement et marche moins vite. Debout, son poing et ses pieds portent à peu près aussi loin que ceux de Napoléon, parce qu'il recule le buste en frappant. Son allonge se voit en bas et en l'air : la balayette porte jusqu'à 430 unités et le pied sauté jusqu'à 515. Il a aussi un spécial à distance, plus haut que les autres, qui passe au-dessus des adversaires accroupis plus petits que lui.

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
| special_appel_cast | 8 | 4 × 2 |
| special_appel_projectile | 4 | 2 × 2 |
| special_appel_impact | 4 | 2 × 2 |
| crouch, block_low, hit_crouch | 3 chacune | 3 × 1 |
| crouch_punch, crouch_kick | 6 chacune | 3 × 2 |
| jump | 6 | 3 × 2 |
| jump_punch, jump_kick | 4 chacune | 4 × 1 |
| ko | 6 | 3 × 2 |
| portrait | 1 | portrait seul |

## L'Appel du 18 Juin — `special_appel`

De Gaulle se redresse de toute sa hauteur, lève les deux bras en V et lance son appel : une onde sonore bleu-blanc-rouge part vers l'adversaire et traverse l'arène.

- Personnage, onde et impact restent trois ressources indépendantes ; le personnage n'a ni micro ni effet dessiné.
- L'onde : trois arcs emboîtés, convexes vers l'avant (bleu devant, blanc, rouge derrière), avec quelques étincelles tricolores.
- La pose 5 du lancement est le cri, la pose 6 le maintien : l'onde est créée une seule fois à l'entrée de la pose 5.
- Vitesse constante, sans gravité, sans suivi ni rebond ; une seule onde active à la fois.
- Collision compacte autour des arcs, sans le halo.
- Commande : quart de cercle avant + poing, ou la touche spéciale.

## Timings à essayer dans le moteur

Hypothèses de départ pour une simulation à 60 mises à jour par seconde, à ajuster en jouant.

| Animation | Durées des poses, en mises à jour | Événement |
|---|---|---|
| Poing rapide | 3, 3, 2, 3, 4, 7 | Poses 3 et 4 actives |
| Pied rapide | 3, 3, 3, 3, 5, 8 | Pose 4 active |
| Pied puissant | 4, 5, 5, 4, 5, 5, 5, 10 | Pose 5 active |
| Appel du 18 Juin | 4, 5, 6, 6, 3, 4, 10, 12 | Émission à l'entrée de la pose 5 |
| Onde | 3, 3, 3, 3 en boucle | Translation gérée par la simulation |
| Impact | 2, 2, 3, 3 | Lecture unique puis suppression |
