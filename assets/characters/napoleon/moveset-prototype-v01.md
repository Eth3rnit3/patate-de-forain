# Napoléon — pack de combat HD v0.1

Direction visuelle validée. Même catalogue de base et mêmes identifiants d'animations communes que Saint Louis. Toutes les ressources sont des sources à retoucher, découper et aligner avant intégration.

## Identité de combat proposée

Combattant compact, garde ferme, gestes directs et appuis stables. Poing rapide, pied frontal au corps, pied latéral puissant, puis spécial à distance. Le sabre reste au fourreau ; le bicorne doit rester sur la tête pendant les actions.

Les différences de vitesse, portée et résistance sont des pistes de réglage, pas des valeurs déduites de l'apparence. Les caractéristiques de Napoléon doivent être définies dans sa propre configuration, sans condition spécifique dispersée dans le moteur.

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
| special_imperial_volley_cast | 8 | 4 × 2 |
| special_imperial_volley_projectile | 4 | 2 × 2 |
| special_imperial_volley_impact | 4 | 2 × 2 |
| portrait | 1 | portrait seul |

Les dispositions sont des consignes de génération et doivent être vérifiées visuellement ; elles ne sont pas des métadonnées d'atlas validées. Les tailles des planches peuvent varier.

## Salve impériale

Napoléon plante ses appuis et pointe vers l'adversaire : un boulet spectral apparaît au niveau du geste et traverse l'arène à hauteur de poitrine.

- Personnage, projectile et impact restent trois ressources indépendantes.
- Le boulet est en métal sombre avec un liseré bleu/cyan et une courte traînée spectrale vers l'arrière.
- La pose 5 du lancement représente le geste d'émission, la pose 6 le maintien ; créer le projectile une seule fois à l'entrée de la pose 5.
- Point d'émission relatif au pivot du personnage, à définir sur les poses nettoyées.
- Commencer avec un projectile à vitesse constante, sans gravité, sans suivi de cible et sans rebond.
- Un seul projectile actif par instance de Napoléon ; le détruire au contact, hors de l'arène ou à la fin de sa durée de vie.
- Collision compacte autour du boulet, indépendante du halo et de la traînée.
- Déclencher l'impact au contact ; un effet distinct pour le blocage pourra être ajouté ensuite.
- Commande proposée : quart de cercle avant + poing ; une touche dédiée suffit au prototype.

## Timings à essayer dans le moteur

Hypothèses non testées, pour une simulation à 60 mises à jour par seconde. Le rendu écran ne doit pas contrôler ces durées.

| Animation | Durées des poses, en mises à jour | Événement |
|---|---|---|
| Poing rapide | 3, 2, 2, 3, 4, 6 | Pose 4 active |
| Pied rapide | 3, 3, 2, 3, 5, 8 | Pose 4 active |
| Pied puissant | 4, 4, 5, 3, 5, 5, 5, 9 | Pose 5 active |
| Salve impériale | 3, 4, 4, 4, 3, 4, 10, 11 | Émission à l'entrée de la pose 5 |
| Boulet | 3, 3, 3, 3 en boucle | Translation gérée par la simulation |
| Impact | 2, 2, 3, 3 | Lecture unique puis suppression |

Un seul dégât par attaque sur une même cible. Pas d'invincibilité, de suivi automatique ou de dégâts à travers la garde pour les premiers essais. Les dégâts, vitesses, portées, recul et étourdissement restent à ajuster en jouant.

## Contrôle avant intégration

Stabiliser appuis et échelle ; harmoniser visage, cocarde, boutons, décoration, garde du sabre et pans du manteau. Nettoyer les franges éventuelles sur fonds clairs et sombres. Vérifier l'ordre des poses, choisir des pivots communs, définir les zones de collision et tester les transitions en jeu.

Les ressources ne comprennent pas encore de sons, de sprites découpés, de collisions ou de programmation de combat.
