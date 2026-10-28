# Contrôle de l'extension HD

## Sources graphiques

| Asset | Poses visibles | Contrôle visuel |
|---|---:|---|
| Pied rapide | 6 | Armé, extension et repli lisibles ; aligner l'appui et harmoniser fourreau et tissu |
| Pied puissant | 8 | Extension ample ; recaler les poses entre cellules, nettoyer les franges colorées et harmoniser les détails |
| Lancement du spécial | 8 | Armé et double pose de libération lisibles ; nettoyer les franges et stabiliser l'échelle |
| Projectile | 4 | Fleur de lys dorée et traînée vers la gauche lisibles ; aligner le cœur du symbole et laisser une marge à la traînée lors de l'export |
| Impact v02 | 4 | Disposition 2 × 2 avec effets séparés ; nettoyer les résidus colorés entre les cellules et autour des particules |

Les planches conservent le style HD validé. Elles restent des sources : aucune animation n'a été testée dans un moteur. Les contours de certaines sorties présentent des franges colorées ; les vérifier sur fond clair et sombre et les nettoyer avant intégration.

Le pied rapide mesure 1536 × 1024 px ; le pied puissant mesure 1774 × 887 px. Tous deux possèdent un canal alpha. Les cellules finales ne doivent pas être calculées en supposant que toutes les planches ont la même taille.

Les timings et événements du spécial figurent dans `moveset-prototype-v01.md`. Les prompts exacts sont conservés dans `prompts-hd-extension-v01.json`.

Les cinq assets initiaux et leurs prompts sont présents. Le lancement mesure 1774 × 887 px ; le projectile et le premier essai d'impact mesurent 2172 × 724 px ; ces fichiers ont un canal alpha. L'impact v01 présente des effets trop proches : préférer la v02 en grille 2 × 2, dont le prompt est archivé séparément. La transparence ne dispense pas du nettoyage des franges colorées.
