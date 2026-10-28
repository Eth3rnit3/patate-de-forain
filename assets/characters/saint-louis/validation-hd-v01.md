# Contrôle du pack HD v0.1

## Contrôle visuel des sources

| Planche | Poses visibles | Constat |
|---|---:|---|
| Garde | 6 | Silhouette cohérente ; variations de hauteur et de détails à stabiliser |
| Marche | 8 | Positions de jambes distinctes ; appuis et alternance à reprendre pour un cycle fluide |
| Jab | 6 | Extension et retour lisibles ; recaler pieds et poing entre les poses |
| Réaction haute | 3 | Recul et récupération lisibles ; première pose détourne fortement la tête |
| Blocage haut | 3 | Protection des avant-bras lisible ; pieds proches des bords de cellule |
| Portrait | 1 | Identité cohérente ; couronne très proche du bord supérieur, ajouter une marge au placement dans l'interface |

Les cinq planches représentent 26 poses sources ; elles ne sont pas encore des animations jouables. Les changements de fleurs de lys, plis, ceinture, couronne et poignée d'épée doivent être harmonisés.

## Contrôle technique

Les trois premières planches (garde, marche, jab) mesurent 1536 × 1024 px et possèdent un véritable canal alpha, avec des pixels entièrement transparents. Le maximum alpha observé est 254/255 : contrôler l'opacité et les contours dans le moteur lors du nettoyage. La référence maîtresse est volontairement opaque sur fond gris.

Le découpage final, les dimensions des cellules exportées, le pivot, les durées et les données de combat restent à définir et à vérifier dans le moteur choisi. Aucun test en jeu n'a encore été effectué.

Les fichiers de réaction, de blocage et de portrait possèdent également un canal alpha. Les deux planches mesurent 1536 × 1024 px ; le portrait mesure 1254 × 1254 px. Les six sources du pack et leurs six prompts sont présents.
