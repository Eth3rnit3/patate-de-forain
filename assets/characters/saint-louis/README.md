# Saint Louis — pack de validation HD v0.1

Sources graphiques créées avec l'outil intégré ImageGen. Direction validée : illustration 2D HD, contours fluides et ombres en aplats. Louis IX réinterprété en combattant arcade, à mains nues et épée au fourreau.

## Contenu attendu

- `source/saint_louis_reference_hd_v01.png` : référence face, garde et dos.
- `source/saint_louis_idle_hd_v01.png` : garde, cible de 6 poses.
- `source/saint_louis_walk_forward_hd_v01.png` : marche, cible de 8 poses.
- `source/saint_louis_stand_punch_light_hd_v01.png` : jab, cible de 6 poses.
- `source/saint_louis_hit_high_hd_v01.png` : réaction haute, cible de 3 poses.
- `source/saint_louis_block_high_hd_v01.png` : blocage haut, cible de 3 poses.
- `source/saint_louis_portrait_hd_v01.png` : portrait de sélection.
- `prompts-hd.json` : prompts exacts du pack pour pouvoir itérer.
- `hd-style-test.md` : prompt de la référence HD.

## Extension : pieds et spécial

- `source/saint_louis_stand_kick_light_hd_v01.png` : pied frontal rapide, 6 poses ciblées.
- `source/saint_louis_stand_kick_heavy_hd_v01.png` : pied latéral haut puissant, 8 poses ciblées.
- `source/saint_louis_special_royal_lily_cast_hd_v01.png` : lancement du Poing de Saint Louis, 8 poses ciblées.
- `source/saint_louis_special_royal_lily_projectile_hd_v01.png` : projectile fleur de lys, 4 poses ciblées.
- `source/saint_louis_special_royal_lily_impact_hd_v02.png` : impact doré, 4 poses en grille 2 × 2 ; version à utiliser. La v01 reste un essai conservé.
- `prompts-hd-extension-v01.json` : prompts exacts de cette extension.
- `prompt-impact-hd-v02.md` : prompt de révision de l'impact.
- [Spécification des coups et réglages initiaux](moveset-prototype-v01.md).

## Statut

Ce dossier contient des **sources de travail**. Les planches ne sont pas encore des animations validées dans un moteur : le nombre de poses, leur alignement, le cycle, les détails du costume et la transparence doivent être vérifiés. Contrôler particulièrement la continuité des mains, fleurs de lys, couronne et épée. Les variations de poses ne garantissent pas à elles seules une boucle fluide.

Toute planche opaque doit être détourée avant intégration, même si la transparence a été demandée au générateur. Les premiers fichiers sans suffixe `_hd_` sont des essais pixel art conservés pour historique.

Il n'y a pas encore de sprites découpés, d'atlas, de durées par image, de collisions, de sons ou de données de combat. Conserver ces sources intactes et exporter les sprites nettoyés dans un dossier distinct.

Le standard proposé et la suite du catalogue figurent dans `../../../docs/character-standard-v01.md`.
