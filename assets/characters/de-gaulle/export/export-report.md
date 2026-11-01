# Export technique — de-gaulle

Généré par `npm run assets:export -- tools/export-character/configs/de-gaulle.json`. Ne pas modifier à la main : relancer l'outil.

## Résultat

- 97 poses. Cellules personnage 1088 × 768, pivot (544, 704) au sol entre les pieds ; cellules d'effet 1024 × 768, pivot au centre de l'effet (512, 384).
- Hauteur de garde ramenée à 560 px (= unités monde du jeu).
- Images individuelles : `assets/characters/de-gaulle/export/frames/de_gaulle_<animation>_<NN>.png` (NN = numéro de pose, à partir de 01).
- Atlas du jeu : `atlas-0.png` 4088 × 4016, `atlas-1.png` 3912 × 1300 dans `game/Assets/Resources/Fighters/de-gaulle`.
- Aperçus : `assets/characters/de-gaulle/export/previews` (planche sur fonds sombre et clair avec pivot et sol, superposition des poses).

## Traitements appliqués

- Séparation des poses par composantes connexes (alpha ≥ 128) dans chaque rangée ; rangées coupées sur la ligne la plus vide ; fragments proches (pointe d’épée, mèches) rattachés à leur pose.
- Nettoyage : pixels d’alpha < 8 supprimés, couleur cachée sous les pixels transparents remise à zéro, poussières isolées écartées. Les bords anticrénelés sont conservés.
- Alignement : bas de la silhouette sur le sol ; horizontalement sur le pied arrière (garde, coups, réactions) ou le buste (marche), décalé pour que le pivot tombe entre les pieds de la pose de référence.
- Mise à l’échelle Lanczos (alpha prémultiplié) ; une échelle par planche, calculée sur sa pose de garde.

## Limites connues

- L’alignement automatique ne conserve pas les déplacements intentionnels dessinés dans les planches (ex. avancée du corps pendant la marche) : la translation vient du moteur.
- Les échelles par planche reposent sur la hauteur de la pose de garde : des écarts de quelques pour cent subsistent entre planches.

## Avertissements

- Cellule agrandie uniformément à 1088 × 768 (pivot 544, 704) : une pose dépassait 1024 × 768.

## Détail par animation

### idle

- Source : `de_gaulle_idle_hd_v01.png` (1536 × 1024, sha256 `ff5e50fc9b0f1999…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.1313 ; repère `rearFoot`

### walk_forward

- Source : `de_gaulle_walk_forward_hd_v01.png` (1536 × 1024, sha256 `a4475093ae5c85b7…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.1313 ; repère `torso`

### stand_punch_light

- Source : `de_gaulle_stand_punch_light_hd_v01.png` (1536 × 1024, sha256 `0f12c94c54e68605…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.1313 ; repère `rearFoot`

### stand_kick_light

- Source : `de_gaulle_stand_kick_light_hd_v01.png` (1536 × 1024, sha256 `b6ade3a69f7d1d66…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.1313 ; repère `rearFoot`

### stand_kick_heavy

- Source : `de_gaulle_stand_kick_heavy_hd_v01.png` (1536 × 1024, sha256 `ab1f5bb4d80cdcda…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.1667 ; repère `rearFoot`

### special_appel_cast

- Source : `de_gaulle_special_appel_cast_hd_v01.png` (1536 × 1024, sha256 `d7cc995d1f131e6d…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.3208 ; repère `rearFoot`

### hit_high

- Source : `de_gaulle_hit_high_hd_v01.png` (2172 × 724, sha256 `ab9ff3e7c277f1f5…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.8104 ; repère `rearFoot`

### block_high

- Source : `de_gaulle_block_high_hd_v01.png` (2172 × 724, sha256 `1d49827b977c3012…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.8296 ; repère `rearFoot`

### crouch

- Source : `de_gaulle_crouch_hd_v01.png` (2172 × 724, sha256 `63087d73f5ceda7f…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.8069 ; repère `rearFoot`

### block_low

- Source : `de_gaulle_block_low_hd_v01.png` (2172 × 724, sha256 `bbd4cb0f80d3006d…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6695 ; repère `rearFoot`

### crouch_punch

- Source : `de_gaulle_crouch_punch_hd_v01.png` (1536 × 1024, sha256 `e3e84ad6d37f27d4…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.8905 ; repère `rearFoot`

### crouch_kick

- Source : `de_gaulle_crouch_kick_hd_v01.png` (1536 × 1024, sha256 `ecc5ab5a062613f3…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9080 ; repère `rearFoot`

### hit_crouch

- Source : `de_gaulle_hit_crouch_hd_v01.png` (2172 × 724, sha256 `8441687f073d21da…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.5942 ; repère `rearFoot`

### jump

- Source : `de_gaulle_jump_hd_v01.png` (1536 × 1024, sha256 `b3ed781c4604670a…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.2431 ; repère `torso`

### jump_punch

- Source : `de_gaulle_jump_punch_hd_v01.png` (2172 × 724, sha256 `7fdf4dbf594fd1e2…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.7066 ; repère `torso`

### jump_kick

- Source : `de_gaulle_jump_kick_hd_v01.png` (2172 × 724, sha256 `3deb49f1631e0ac5…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.8064 ; repère `torso`

### ko

- Source : `de_gaulle_ko_hd_v01.png` (1536 × 1024, sha256 `bbf8d9b1e5d25fae…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.1417 ; repère `rearFoot`

### appel_projectile

- Source : `de_gaulle_special_appel_projectile_hd_v01.png` (1254 × 1254, sha256 `3e09a85098bb5e18…`)
- Découpage : {"type":"grid","rows":2,"columns":2} → 4 poses ; échelle 0.4500 ; repère `head`

### appel_impact

- Source : `de_gaulle_special_appel_impact_hd_v01.png` (1254 × 1254, sha256 `a135d6d585f93b3e…`)
- Découpage : {"type":"grid","rows":2,"columns":2} → 4 poses ; échelle 0.4500 ; repère `centroid`
  - `idle_01` : pose source à (79, 12), 379 × 496 px → 428 × 560 px pleins ; décalage dans la cellule (330, 144)
  - `idle_02` : pose source à (586, 11), 377 × 497 px → 425 × 559 px pleins ; décalage dans la cellule (330, 143)
  - `idle_03` : pose source à (1094, 7), 377 × 501 px → 425 × 564 px pleins ; décalage dans la cellule (329, 138)
  - `idle_04` : pose source à (79, 520), 378 × 488 px → 428 × 549 px pleins ; décalage dans la cellule (330, 153)
  - `idle_05` : pose source à (585, 531), 378 × 477 px → 426 × 537 px pleins ; décalage dans la cellule (329, 165)
  - `idle_06` : pose source à (1091, 521), 379 × 487 px → 426 × 548 px pleins ; décalage dans la cellule (329, 154)
  - `walk_forward_01` : pose source à (28, 10), 411 × 496 px → 463 × 558 px pleins ; décalage dans la cellule (312, 144)
  - `walk_forward_02` : pose source à (468, 25), 292 × 480 px → 328 × 540 px pleins ; décalage dans la cellule (365, 163)
  - `walk_forward_03` : pose source à (854, 14), 233 × 496 px → 261 × 560 px pleins ; décalage dans la cellule (420, 144)
  - `walk_forward_04` : pose source à (1157, 10), 374 × 493 px → 422 × 555 px pleins ; décalage dans la cellule (315, 147)
  - `walk_forward_05` : pose source à (24, 516), 416 × 486 px → 469 × 546 px pleins ; décalage dans la cellule (310, 156)
  - `walk_forward_06` : pose source à (459, 530), 299 × 470 px → 335 × 529 px pleins ; décalage dans la cellule (358, 173)
  - `walk_forward_07` : pose source à (825, 513), 276 × 489 px → 309 × 547 px pleins ; décalage dans la cellule (371, 152)
  - `walk_forward_08` : pose source à (1143, 516), 379 × 483 px → 427 × 543 px pleins ; décalage dans la cellule (318, 159)
  - `stand_punch_light_01` : pose source à (55, 13), 387 × 496 px → 436 × 560 px pleins ; décalage dans la cellule (325, 144)
  - `stand_punch_light_02` : pose source à (565, 15), 408 × 495 px → 459 × 558 px pleins ; décalage dans la cellule (324, 145)
  - `stand_punch_light_03` : pose source à (1075, 15), 430 × 495 px → 483 × 559 px pleins ; décalage dans la cellule (325, 145)
  - `stand_punch_light_04` : pose source à (38, 516), 477 × 494 px → 538 × 556 px pleins ; décalage dans la cellule (324, 145)
  - `stand_punch_light_05` : pose source à (569, 521), 404 × 487 px → 454 × 550 px pleins ; décalage dans la cellule (325, 153)
  - `stand_punch_light_06` : pose source à (1081, 520), 396 × 490 px → 445 × 553 px pleins ; décalage dans la cellule (325, 150)
  - `stand_kick_light_01` : pose source à (52, 14), 401 × 495 px → 451 × 557 px pleins ; décalage dans la cellule (317, 145)
  - `stand_kick_light_02` : pose source à (628, 14), 264 × 494 px → 296 × 558 px pleins ; décalage dans la cellule (286, 146)
  - `stand_kick_light_03` : pose source à (1029, 15), 440 × 493 px → 497 × 558 px pleins ; décalage dans la cellule (210, 147)
  - `stand_kick_light_04` : pose source à (77, 515), 482 × 495 px → 543 × 556 px pleins ; décalage dans la cellule (312, 145)
  - `stand_kick_light_05` : pose source à (647, 516), 271 × 493 px → 304 × 556 px pleins ; décalage dans la cellule (278, 146)
  - `stand_kick_light_06` : pose source à (1061, 515), 386 × 495 px → 434 × 556 px pleins ; décalage dans la cellule (317, 145)
  - `stand_kick_heavy_01` : pose source à (29, 23), 357 × 482 px → 417 × 559 px pleins ; décalage dans la cellule (336, 143)
  - `stand_kick_heavy_02` : pose source à (460, 25), 234 × 485 px → 271 × 565 px pleins ; décalage dans la cellule (286, 139)
  - `stand_kick_heavy_03` : pose source à (826, 19), 236 × 491 px → 275 × 571 px pleins ; décalage dans la cellule (288, 132)
  - `stand_kick_heavy_04` : pose source à (1132, 33), 393 × 478 px → 456 × 555 px pleins ; décalage dans la cellule (285, 147)
  - `stand_kick_heavy_05` : pose source à (24, 537), 521 × 469 px → 607 × 544 px pleins ; décalage dans la cellule (212, 158)
  - `stand_kick_heavy_06` : pose source à (575, 532), 244 × 474 px → 283 × 551 px pleins ; décalage dans la cellule (282, 152)
  - `stand_kick_heavy_07` : pose source à (898, 536), 250 × 469 px → 289 × 545 px pleins ; décalage dans la cellule (263, 158)
  - `stand_kick_heavy_08` : pose source à (1200, 529), 336 × 471 px → 390 × 548 px pleins ; décalage dans la cellule (335, 155)
  - `special_appel_cast_01` : pose source à (42, 64), 334 × 426 px → 438 × 560 px pleins ; décalage dans la cellule (324, 142)
  - `special_appel_cast_02` : pose source à (434, 15), 275 × 477 px → 360 × 629 px pleins ; décalage dans la cellule (324, 75)
  - `special_appel_cast_03` : pose source à (763, 32), 357 × 460 px → 469 × 605 px pleins ; décalage dans la cellule (272, 97)
  - `special_appel_cast_04` : pose source à (1113, 37), 404 × 455 px → 532 × 598 px pleins ; décalage dans la cellule (236, 104)
  - `special_appel_cast_05` : pose source à (27, 496), 362 × 503 px → 475 × 661 px pleins ; décalage dans la cellule (293, 41)
  - `special_appel_cast_06` : pose source à (433, 502), 342 × 497 px → 452 × 655 px pleins ; décalage dans la cellule (317, 49)
  - `special_appel_cast_07` : pose source à (787, 559), 370 × 442 px → 487 × 581 px pleins ; décalage dans la cellule (306, 121)
  - `special_appel_cast_08` : pose source à (1170, 569), 326 × 431 px → 430 × 567 px pleins ; décalage dans la cellule (325, 136)
  - `hit_high_01` : pose source à (70, 15), 585 × 699 px → 472 × 565 px pleins ; décalage dans la cellule (313, 139)
  - `hit_high_02` : pose source à (820, 9), 569 × 705 px → 459 × 570 px pleins ; décalage dans la cellule (313, 134)
  - `hit_high_03` : pose source à (1551, 21), 569 × 693 px → 460 × 561 px pleins ; décalage dans la cellule (313, 143)
  - `block_high_01` : pose source à (105, 18), 554 × 677 px → 458 × 560 px pleins ; décalage dans la cellule (312, 143)
  - `block_high_02` : pose source à (824, 29), 532 × 666 px → 440 × 552 px pleins ; décalage dans la cellule (312, 152)
  - `block_high_03` : pose source à (1482, 18), 559 × 677 px → 463 × 561 px pleins ; décalage dans la cellule (312, 143)
  - `crouch_01` : pose source à (106, 16), 547 × 696 px → 439 × 560 px pleins ; décalage dans la cellule (324, 143)
  - `crouch_02` : pose source à (844, 103), 513 × 609 px → 412 × 489 px pleins ; décalage dans la cellule (323, 214)
  - `crouch_03` : pose source à (1620, 253), 443 × 459 px → 357 × 369 px pleins ; décalage dans la cellule (324, 334)
  - `block_low_01` : pose source à (51, 118), 704 × 553 px → 470 × 370 px pleins ; décalage dans la cellule (309, 334)
  - `block_low_02` : pose source à (793, 148), 661 × 523 px → 441 × 350 px pleins ; décalage dans la cellule (308, 354)
  - `block_low_03` : pose source à (1505, 116), 616 × 556 px → 411 × 370 px pleins ; décalage dans la cellule (309, 333)
  - `crouch_punch_01` : pose source à (64, 83), 441 × 419 px → 391 × 370 px pleins ; décalage dans la cellule (348, 333)
  - `crouch_punch_02` : pose source à (534, 97), 464 × 406 px → 411 × 360 px pleins ; décalage dans la cellule (325, 343)
  - `crouch_punch_03` : pose source à (1039, 100), 459 × 403 px → 407 × 356 px pleins ; décalage dans la cellule (347, 347)
  - `crouch_punch_04` : pose source à (57, 566), 509 × 399 px → 451 × 353 px pleins ; décalage dans la cellule (348, 350)
  - `crouch_punch_05` : pose source à (629, 563), 442 × 401 px → 392 × 355 px pleins ; décalage dans la cellule (335, 348)
  - `crouch_punch_06` : pose source à (1122, 560), 394 × 403 px → 350 × 358 px pleins ; décalage dans la cellule (348, 345)
  - `crouch_kick_01` : pose source à (80, 115), 317 × 408 px → 285 × 369 px pleins ; décalage dans la cellule (401, 334)
  - `crouch_kick_02` : pose source à (506, 169), 449 × 345 px → 406 × 311 px pleins ; décalage dans la cellule (326, 392)
  - `crouch_kick_03` : pose source à (985, 186), 535 × 334 px → 485 × 301 px pleins ; décalage dans la cellule (347, 402)
  - `crouch_kick_04` : pose source à (27, 609), 631 × 350 px → 572 × 316 px pleins ; décalage dans la cellule (402, 387)
  - `crouch_kick_05` : pose source à (674, 596), 353 × 364 px → 319 × 328 px pleins ; décalage dans la cellule (401, 374)
  - `crouch_kick_06` : pose source à (1145, 593), 318 × 368 px → 288 × 333 px pleins ; décalage dans la cellule (402, 370)
  - `hit_crouch_01` : pose source à (21, 13), 704 × 690 px → 418 × 410 px pleins ; décalage dans la cellule (360, 294)
  - `hit_crouch_02` : pose source à (765, 46), 720 × 658 px → 426 × 390 px pleins ; décalage dans la cellule (359, 314)
  - `hit_crouch_03` : pose source à (1522, 79), 622 × 624 px → 369 × 371 px pleins ; décalage dans la cellule (359, 334)
  - `jump_01` : pose source à (131, 87), 317 × 411 px → 392 × 508 px pleins ; décalage dans la cellule (354, 194)
  - `jump_02` : pose source à (606, 13), 313 × 495 px → 386 × 613 px pleins ; décalage dans la cellule (371, 90)
  - `jump_03` : pose source à (1162, 19), 247 × 396 px → 304 × 489 px pleins ; décalage dans la cellule (409, 213)
  - `jump_04` : pose source à (169, 531), 262 × 351 px → 323 × 434 px pleins ; décalage dans la cellule (405, 268)
  - `jump_05` : pose source à (660, 531), 238 × 471 px → 293 × 583 px pleins ; décalage dans la cellule (420, 119)
  - `jump_06` : pose source à (1145, 597), 318 × 396 px → 394 × 490 px pleins ; décalage dans la cellule (381, 213)
  - `jump_punch_01` : pose source à (80, 54), 418 × 616 px → 294 × 433 px pleins ; décalage dans la cellule (488, 270)
  - `jump_punch_02` : pose source à (606, 44), 441 × 616 px → 310 × 434 px pleins ; décalage dans la cellule (475, 269)
  - `jump_punch_03` : pose source à (1111, 119), 599 × 489 px → 421 × 344 px pleins ; décalage dans la cellule (483, 359)
  - `jump_punch_04` : pose source à (1742, 52), 367 × 623 px → 258 × 439 px pleins ; décalage dans la cellule (518, 265)
  - `jump_kick_01` : pose source à (12, 25), 387 × 540 px → 312 × 433 px pleins ; décalage dans la cellule (474, 270)
  - `jump_kick_02` : pose source à (519, 27), 465 × 571 px → 374 × 458 px pleins ; décalage dans la cellule (441, 245)
  - `jump_kick_03` : pose source à (1029, 69), 755 × 488 px → 607 × 392 px pleins ; décalage dans la cellule (450, 311)
  - `jump_kick_04` : pose source à (1733, 42), 419 × 556 px → 336 × 446 px pleins ; décalage dans la cellule (465, 257)
  - `ko_01` : pose source à (80, 66), 448 × 500 px → 508 × 569 px pleins ; décalage dans la cellule (282, 133)
  - `ko_02` : pose source à (615, 127), 358 × 438 px → 406 × 498 px pleins ; décalage dans la cellule (294, 205)
  - `ko_03` : pose source à (1016, 176), 499 × 375 px → 567 × 425 px pleins ; décalage dans la cellule (109, 277)
  - `ko_04` : pose source à (25, 666), 500 × 280 px → 568 × 317 px pleins ; décalage dans la cellule (35, 385)
  - `ko_05` : pose source à (550, 754), 469 × 181 px → 531 × 204 px pleins ; décalage dans la cellule (8, 498)
  - `ko_06` : pose source à (1056, 781), 475 × 140 px → 538 × 157 px pleins ; décalage dans la cellule (1, 545)
  - `appel_projectile_01` : pose source à (0, 0), 627 × 627 px → 199 × 164 px pleins ; décalage dans la cellule (321, 219)
  - `appel_projectile_02` : pose source à (627, 0), 627 × 627 px → 208 × 162 px pleins ; décalage dans la cellule (347, 214)
  - `appel_projectile_03` : pose source à (0, 627), 627 × 627 px → 199 × 167 px pleins ; décalage dans la cellule (334, 242)
  - `appel_projectile_04` : pose source à (627, 627), 627 × 627 px → 182 × 161 px pleins ; décalage dans la cellule (335, 242)
  - `appel_impact_01` : pose source à (0, 0), 627 × 627 px → 126 × 136 px pleins ; décalage dans la cellule (366, 228)
  - `appel_impact_02` : pose source à (627, 0), 627 × 627 px → 266 × 254 px pleins ; décalage dans la cellule (371, 231)
  - `appel_impact_03` : pose source à (0, 627), 627 × 627 px → 263 × 244 px pleins ; décalage dans la cellule (369, 249)
  - `appel_impact_04` : pose source à (627, 627), 627 × 627 px → 205 × 194 px pleins ; décalage dans la cellule (360, 261)
