# Export technique — napoleon

Généré par `npm run assets:export -- tools/export-character/configs/napoleon.json`. Ne pas modifier à la main : relancer l'outil.

## Résultat

- 56 poses. Cellules personnage 1024 × 768, pivot (512, 704) au sol entre les pieds ; cellules d'effet 1024 × 768, pivot au centre de l'effet (512, 384).
- Hauteur de garde ramenée à 480 px (= unités monde du jeu).
- Images individuelles : `assets/characters/napoleon/export/frames/napoleon_<animation>_<NN>.png` (NN = numéro de pose, à partir de 01).
- Atlas du jeu : `atlas-0.png` 3988 × 2432 dans `game/Assets/Resources/Fighters/napoleon`.
- Aperçus : `assets/characters/napoleon/export/previews` (planche sur fonds sombre et clair avec pivot et sol, superposition des poses).

## Traitements appliqués

- Séparation des poses par composantes connexes (alpha ≥ 128) dans chaque rangée ; rangées coupées sur la ligne la plus vide ; fragments proches (pointe d’épée, mèches) rattachés à leur pose.
- Nettoyage : pixels d’alpha < 8 supprimés, couleur cachée sous les pixels transparents remise à zéro, poussières isolées écartées. Les bords anticrénelés sont conservés.
- Alignement : bas de la silhouette sur le sol ; horizontalement sur le pied arrière (garde, coups, réactions) ou le buste (marche), décalé pour que le pivot tombe entre les pieds de la pose de référence.
- Mise à l’échelle Lanczos (alpha prémultiplié) ; une échelle par planche, calculée sur sa pose de garde.

## Limites connues

- L’alignement automatique ne conserve pas les déplacements intentionnels dessinés dans les planches (ex. avancée du corps pendant la marche) : la translation vient du moteur.
- Les échelles par planche reposent sur la hauteur de la pose de garde : des écarts de quelques pour cent subsistent entre planches.

## Détail par animation

### idle

- Source : `napoleon_idle_hd_v01.png` (1536 × 1024, sha256 `9f429382452b84b1…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9619 ; repère `rearFoot`

### walk_forward

- Source : `napoleon_walk_forward_hd_v01.png` (1536 × 1024, sha256 `55cebd12347591fa…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 0.9619 ; repère `torso`

### stand_punch_light

- Source : `napoleon_stand_punch_light_hd_v01.png` (1536 × 1024, sha256 `40185889cae4b244…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9619 ; repère `rearFoot`

### stand_kick_light

- Source : `napoleon_stand_kick_light_hd_v01.png` (1536 × 1024, sha256 `30646d10cecdf1ac…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9619 ; repère `rearFoot`

### stand_kick_heavy

- Source : `napoleon_stand_kick_heavy_hd_v01.png` (1536 × 1024, sha256 `2989fd2919d35de6…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.0191 ; repère `rearFoot`

### special_imperial_volley_cast

- Source : `napoleon_special_imperial_volley_cast_hd_v01.png` (1536 × 1024, sha256 `1ccfbce99941f5af…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.0105 ; repère `rearFoot`

### hit_high

- Source : `napoleon_hit_high_hd_v01.png` (1536 × 1024, sha256 `c25caa3642a640b8…`)
- Découpage : {"type":"poses","rows":1,"seams":[{"row":1,"x":1050,"through":[[1067,846],[1067,878],[1049,887]]}]} → 3 poses ; échelle 0.6242 ; repère `rearFoot`

### block_high

- Source : `napoleon_block_high_hd_v01.png` (1930 × 815, sha256 `b615ffb5bb67f878…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6275 ; repère `rearFoot`

### imperial_volley_projectile

- Source : `napoleon_special_imperial_volley_projectile_hd_v01.png` (1254 × 1254, sha256 `98a42f4dfc111773…`)
- Découpage : {"type":"grid","rows":2,"columns":2} → 4 poses ; échelle 0.4500 ; repère `head`

### imperial_volley_impact

- Source : `napoleon_special_imperial_volley_impact_hd_v01.png` (1254 × 1254, sha256 `5254dab093ac5fe9…`)
- Découpage : {"type":"grid","rows":2,"columns":2} → 4 poses ; échelle 0.4500 ; repère `centroid`
  - `idle_01` : pose source à (106, 16), 351 × 501 px → 336 × 480 px pleins ; décalage dans la cellule (340, 223)
  - `idle_02` : pose source à (587, 17), 357 × 500 px → 342 × 479 px pleins ; décalage dans la cellule (341, 224)
  - `idle_03` : pose source à (1096, 16), 368 × 501 px → 352 × 481 px pleins ; décalage dans la cellule (337, 222)
  - `idle_04` : pose source à (104, 521), 355 × 486 px → 340 × 465 px pleins ; décalage dans la cellule (340, 238)
  - `idle_05` : pose source à (588, 525), 363 × 482 px → 347 × 462 px pleins ; décalage dans la cellule (342, 241)
  - `idle_06` : pose source à (1094, 521), 357 × 486 px → 340 × 465 px pleins ; décalage dans la cellule (340, 238)
  - `walk_forward_01` : pose source à (45, 11), 361 × 497 px → 345 × 476 px pleins ; décalage dans la cellule (325, 227)
  - `walk_forward_02` : pose source à (416, 12), 310 × 497 px → 296 × 477 px pleins ; décalage dans la cellule (327, 227)
  - `walk_forward_03` : pose source à (788, 14), 289 × 495 px → 276 × 475 px pleins ; décalage dans la cellule (347, 229)
  - `walk_forward_04` : pose source à (1139, 13), 346 × 492 px → 331 × 471 px pleins ; décalage dans la cellule (318, 232)
  - `walk_forward_05` : pose source à (29, 515), 375 × 490 px → 359 × 468 px pleins ; décalage dans la cellule (310, 234)
  - `walk_forward_06` : pose source à (421, 516), 342 × 492 px → 327 × 471 px pleins ; décalage dans la cellule (328, 232)
  - `walk_forward_07` : pose source à (799, 516), 288 × 491 px → 275 × 469 px pleins ; décalage dans la cellule (343, 233)
  - `walk_forward_08` : pose source à (1163, 516), 347 × 484 px → 332 × 463 px pleins ; décalage dans la cellule (324, 239)
  - `stand_punch_light_01` : pose source à (65, 15), 357 × 493 px → 341 × 472 px pleins ; décalage dans la cellule (339, 231)
  - `stand_punch_light_02` : pose source à (552, 16), 393 × 493 px → 376 × 472 px pleins ; décalage dans la cellule (334, 231)
  - `stand_punch_light_03` : pose source à (1022, 17), 465 × 492 px → 445 × 471 px pleins ; décalage dans la cellule (327, 232)
  - `stand_punch_light_04` : pose source à (45, 517), 507 × 491 px → 486 × 469 px pleins ; décalage dans la cellule (342, 233)
  - `stand_punch_light_05` : pose source à (565, 520), 401 × 488 px → 384 × 467 px pleins ; décalage dans la cellule (323, 236)
  - `stand_punch_light_06` : pose source à (1074, 517), 374 × 491 px → 358 × 469 px pleins ; décalage dans la cellule (338, 233)
  - `stand_kick_light_01` : pose source à (57, 11), 357 × 492 px → 342 × 470 px pleins ; décalage dans la cellule (334, 232)
  - `stand_kick_light_02` : pose source à (575, 9), 343 × 495 px → 328 × 474 px pleins ; décalage dans la cellule (265, 229)
  - `stand_kick_light_03` : pose source à (1043, 15), 479 × 489 px → 459 × 469 px pleins ; décalage dans la cellule (230, 235)
  - `stand_kick_light_04` : pose source à (37, 515), 525 × 491 px → 503 × 470 px pleins ; décalage dans la cellule (243, 233)
  - `stand_kick_light_05` : pose source à (611, 515), 299 × 493 px → 286 × 472 px pleins ; décalage dans la cellule (225, 231)
  - `stand_kick_light_06` : pose source à (1077, 521), 376 × 489 px → 360 × 469 px pleins ; décalage dans la cellule (320, 234)
  - `stand_kick_heavy_01` : pose source à (16, 25), 324 × 473 px → 329 × 479 px pleins ; décalage dans la cellule (340, 223)
  - `stand_kick_heavy_02` : pose source à (377, 48), 328 × 449 px → 332 × 458 px pleins ; décalage dans la cellule (340, 246)
  - `stand_kick_heavy_03` : pose source à (763, 24), 274 × 476 px → 276 × 484 px pleins ; décalage dans la cellule (219, 220)
  - `stand_kick_heavy_04` : pose source à (1088, 50), 432 × 451 px → 440 × 457 px pleins ; décalage dans la cellule (198, 245)
  - `stand_kick_heavy_05` : pose source à (18, 538), 497 × 453 px → 505 × 461 px pleins ; décalage dans la cellule (228, 243)
  - `stand_kick_heavy_06` : pose source à (529, 532), 315 × 459 px → 320 × 466 px pleins ; décalage dans la cellule (242, 237)
  - `stand_kick_heavy_07` : pose source à (864, 530), 274 × 460 px → 278 × 466 px pleins ; décalage dans la cellule (189, 236)
  - `stand_kick_heavy_08` : pose source à (1181, 530), 316 × 461 px → 322 × 468 px pleins ; décalage dans la cellule (337, 234)
  - `special_imperial_volley_cast_01` : pose source à (40, 29), 362 × 478 px → 364 × 480 px pleins ; décalage dans la cellule (323, 223)
  - `special_imperial_volley_cast_02` : pose source à (412, 16), 295 × 489 px → 296 × 493 px pleins ; décalage dans la cellule (317, 211)
  - `special_imperial_volley_cast_03` : pose source à (738, 22), 347 × 483 px → 349 × 486 px pleins ; décalage dans la cellule (328, 217)
  - `special_imperial_volley_cast_04` : pose source à (1088, 33), 406 × 475 px → 410 × 477 px pleins ; décalage dans la cellule (325, 225)
  - `special_imperial_volley_cast_05` : pose source à (18, 514), 431 × 473 px → 434 × 475 px pleins ; décalage dans la cellule (326, 227)
  - `special_imperial_volley_cast_06` : pose source à (416, 514), 441 × 473 px → 445 × 477 px pleins ; décalage dans la cellule (317, 227)
  - `special_imperial_volley_cast_07` : pose source à (824, 518), 348 × 469 px → 351 × 472 px pleins ; décalage dans la cellule (335, 231)
  - `special_imperial_volley_cast_08` : pose source à (1168, 516), 351 × 471 px → 353 × 473 px pleins ; décalage dans la cellule (317, 229)
  - `hit_high_01` : pose source à (11, 109), 521 × 799 px → 324 × 497 px pleins ; décalage dans la cellule (358, 206)
  - `hit_high_02` : pose source à (526, 130), 541 × 774 px → 338 × 481 px pleins ; décalage dans la cellule (352, 222)
  - `hit_high_03` : pose source à (1041, 134), 488 × 771 px → 305 × 480 px pleins ; décalage dans la cellule (357, 224)
  - `block_high_01` : pose source à (50, 13), 582 × 776 px → 363 × 486 px pleins ; décalage dans la cellule (339, 218)
  - `block_high_02` : pose source à (702, 56), 592 × 733 px → 369 × 458 px pleins ; décalage dans la cellule (340, 245)
  - `block_high_03` : pose source à (1317, 22), 584 × 767 px → 364 × 479 px pleins ; décalage dans la cellule (318, 224)
  - `imperial_volley_projectile_01` : pose source à (0, 0), 627 × 627 px → 216 × 120 px pleins ; décalage dans la cellule (312, 225)
  - `imperial_volley_projectile_02` : pose source à (627, 0), 627 × 627 px → 214 × 132 px pleins ; décalage dans la cellule (326, 228)
  - `imperial_volley_projectile_03` : pose source à (0, 627), 627 × 627 px → 225 × 128 px pleins ; décalage dans la cellule (324, 238)
  - `imperial_volley_projectile_04` : pose source à (627, 627), 627 × 627 px → 216 × 117 px pleins ; décalage dans la cellule (318, 236)
  - `imperial_volley_impact_01` : pose source à (0, 0), 627 × 627 px → 116 × 128 px pleins ; décalage dans la cellule (370, 234)
  - `imperial_volley_impact_02` : pose source à (627, 0), 627 × 627 px → 253 × 241 px pleins ; décalage dans la cellule (373, 233)
  - `imperial_volley_impact_03` : pose source à (0, 627), 627 × 627 px → 243 × 220 px pleins ; décalage dans la cellule (365, 251)
  - `imperial_volley_impact_04` : pose source à (627, 627), 627 × 627 px → 202 × 201 px pleins ; décalage dans la cellule (370, 239)
