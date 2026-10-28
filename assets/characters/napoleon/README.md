# Napoléon — pack HD v0.1

Deuxième personnage de Patate de Forain. Direction visuelle validée ; pack de sources graphiques constitué sur le même standard que Saint Louis.

## Direction validée

- Illustration 2D HD dans le même style que Saint Louis : encrage fluide, ombres en aplats et dégradés discrets.
- Silhouette compacte, large et athlétique, distincte du roi médiéval.
- Visage adulte sans barbe, cheveux courts sombres, expression autoritaire.
- Bicorne noir porté en travers avec cocarde tricolore.
- Uniforme vert sombre, col et poignets rouges, gilet et culotte blancs, bottes noires, détails dorés sobres.
- Redingote grise ouverte, pans séparés pour laisser lire les mouvements des jambes.
- Sabre au fourreau ; mains nues pour les premières attaques.

Il s'agit d'une réinterprétation pour le jeu, pas d'une reconstitution historique stricte.

## Source

`source/napoleon_reference_hd_v01.png` : face dans une pose emblématique, garde tournée vers la droite et dos, avec palette.

La référence de Saint Louis est utilisée uniquement pour harmoniser le style. La tête, la silhouette et la tenue de Napoléon doivent rester distinctes.

Génération avec l'outil intégré ImageGen ; prompt exact conservé dans `prompt-reference-hd-v01.md`.

## Animations et spécial

Le catalogue couvre garde, marche, poing, réaction haute, blocage haut, deux coups de pied, lancement de la Salve impériale, boulet spectral, impact et portrait. Les fichiers sont nommés `source/napoleon_<animation>_hd_v01.png`.

Le [moveset](moveset-prototype-v01.md) indique les noms exacts, les poses ciblées et les réglages proposés. Le fichier `prompts-hd-v01.json` archive les prompts, les grilles demandées et les chemins des sorties effectivement enregistrées. Ces grilles ne constituent pas des métadonnées de découpage validées.

Les onze sorties comprennent 56 poses de combat et d'effets, plus un portrait, en complément de la référence. [L'inventaire](inventory-hd-v01.json) donne les dimensions réelles et le contrôle technique des PNG ; [les notes de validation](validation-hd-v01.md) décrivent les reprises nécessaires.

Les planches sont des sources, pas des animations déjà jouables. Les retouches, le découpage, l'alignement, les collisions, les sons et les essais dans le moteur restent à réaliser. Les ressources sont créées avec ImageGen intégré.

Suivre le [standard commun](../../../docs/character-standard-v01.md). Vérifier notamment la stabilité du bicorne, les pans de la redingote et la cohérence du fourreau entre les poses. La taille relative aux autres combattants se règle lors de l'intégration, sans modifier les proportions à chaque image.
