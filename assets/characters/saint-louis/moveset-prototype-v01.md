# Saint Louis — coups de pied et spécial

Extension du pack HD : deux attaques de pied debout et un spécial à projectile. Ce document décrit le comportement souhaité ; il ne constitue pas une implémentation dans un moteur.

## Coup de pied rapide — `stand_kick_light`

Coup de pied frontal au corps. Rôle : toucher rapidement à moyenne portée et interrompre une approche. Six poses sources : garde, genou levé, extension partielle, frappe, repli, retour en garde.

## Coup de pied puissant — `stand_kick_heavy`

Coup de pied latéral haut, plus ample et plus lent. Rôle : tenir l'adversaire à distance, avec une récupération qui permet de le punir s'il rate. Huit poses sources : garde, transfert du poids, armé du genou, extension partielle, frappe, repli, descente du pied, garde.

## Le Poing de Saint Louis — `special_royal_lily`

Saint Louis arme une frappe, puis projette une fleur de lys d'énergie dorée à hauteur de poitrine. Le personnage garde son épée au fourreau.

- Animation du personnage : `special_royal_lily_cast`, huit poses.
- Projectile : `special_royal_lily_projectile`, boucle de quatre poses ; déplacement et collision gérés par le moteur.
- Impact : `special_royal_lily_impact`, quatre poses jouées une fois.
- Source recommandée pour l'impact : `saint_louis_special_royal_lily_impact_hd_v02.png`, lecture en grille 2 × 2 (haut gauche, haut droite, bas gauche, bas droite).
- Apparition du projectile au niveau du poing lors de la cinquième pose de lancement.
- Prévoir un point d'émission par pose, en coordonnées relatives au pivot ; ne pas déduire sa position du bord de l'image.
- Retourner le projectile et déplacer son point d'émission si le personnage regarde à gauche.
- Proposition de commande : quart de cercle avant + poing. Pour le premier prototype, une touche spéciale dédiée facilitera les essais.
- Un seul projectile de Saint Louis à la fois. Le détruire au premier contact, à la limite de l'arène ou à l'expiration de sa durée de vie.
- Déclencher l'effet d'impact au contact. Les effets de blocage pourront ensuite être différenciés.

## Premiers réglages à essayer

Valeurs de conception provisoires pour une simulation à 60 mises à jour par seconde, indépendantes du rafraîchissement écran. Elles n'ont pas été testées en jeu. Une pose graphique peut durer plusieurs mises à jour.

| Action | Durées des poses, en mises à jour | Comportement |
|---|---|---|
| Pied rapide | 3, 3, 2, 3, 5, 8 | Pose 4 active ; un seul impact par attaque |
| Pied puissant | 4, 4, 5, 3, 5, 5, 5, 9 | Pose 5 active ; un seul impact par attaque |
| Spécial | 3, 4, 4, 4, 3, 3, 9, 11 | Créer le projectile une seule fois à l'entrée de la pose 5 |
| Projectile | 3, 3, 3, 3, en boucle | Avance à vitesse constante jusqu'au contact ou à sa destruction |
| Impact | 2, 2, 3, 3 | Lecture unique puis suppression |

Commencer sans dégâts à travers la garde, sans invincibilité et sans annulation des attaques. Ajuster ensuite portée, vitesse, dégâts, recul et étourdissement en jouant. Les hitboxes doivent suivre uniquement les poses actives ; la collision du projectile reste indépendante de son halo graphique.

## État des assets

Planches sources générées avec ImageGen à partir de la référence HD validée. Le nombre et l'ordre des poses sont à contrôler sur les sorties, puis les images doivent être retouchées, découpées et alignées. Les durées ci-dessus décrivent les poses souhaitées, à adapter si une sortie nécessite une correction. Sons, programmation, collisions et essais en jeu restent à réaliser.
