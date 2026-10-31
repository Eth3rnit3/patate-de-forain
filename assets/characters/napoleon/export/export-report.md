# Export technique — napoleon

Généré par `npm run assets:export -- tools/export-character/configs/napoleon.json`. Ne pas modifier à la main : relancer l'outil.

## Résultat

- 97 poses. Cellules personnage 1152 × 768, pivot (576, 704) au sol entre les pieds ; cellules d'effet 1024 × 768, pivot au centre de l'effet (512, 384).
- Hauteur de garde ramenée à 480 px (= unités monde du jeu).
- Images individuelles : `assets/characters/napoleon/export/frames/napoleon_<animation>_<NN>.png` (NN = numéro de pose, à partir de 01).
- Atlas du jeu : `atlas-0.png` 4092 × 3764, `atlas-1.png` 3700 × 496 dans `game/Assets/Resources/Fighters/napoleon`.
- Aperçus : `assets/characters/napoleon/export/previews` (planche sur fonds sombre et clair avec pivot et sol, superposition des poses).

## Traitements appliqués

- Séparation des poses par composantes connexes (alpha ≥ 128) dans chaque rangée ; rangées coupées sur la ligne la plus vide ; fragments proches (pointe d’épée, mèches) rattachés à leur pose.
- Nettoyage : pixels d’alpha < 8 supprimés, couleur cachée sous les pixels transparents remise à zéro, poussières isolées écartées. Les bords anticrénelés sont conservés.
- Alignement : bas de la silhouette sur le sol ; horizontalement sur le pied arrière (garde, coups, réactions) ou le buste (marche), décalé pour que le pivot tombe entre les pieds de la pose de référence.
- Mise à l’échelle Lanczos (alpha prémultiplié) ; une échelle par planche, calculée sur sa pose de garde.

## Limites connues

- L’alignement automatique ne conserve pas les déplacements intentionnels dessinés dans les planches (ex. avancée du corps pendant la marche) : la translation vient du moteur.
- Les échelles par planche reposent sur la hauteur de la pose de garde : des écarts de quelques pour cent subsistent entre planches.

## Avertissements

- Cellule agrandie uniformément à 1152 × 768 (pivot 576, 704) : une pose dépassait 1024 × 768.

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

### crouch

- Source : `napoleon_crouch_hd_v01.png` (2172 × 724, sha256 `6c9d8a741f0e3bb5…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6936 ; repère `rearFoot`

### block_low

- Source : `napoleon_block_low_hd_v01.png` (2172 × 724, sha256 `6ba971ee8ee1e536…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.5596 ; repère `rearFoot`

### crouch_punch

- Source : `napoleon_crouch_punch_hd_v01.png` (1536 × 1024, sha256 `21b7b88662c314c8…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.8497 ; repère `rearFoot`

### crouch_kick

- Source : `napoleon_crouch_kick_hd_v01.png` (1536 × 1024, sha256 `45c4f6fbfc5cf3b6…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.8957 ; repère `rearFoot`

### hit_crouch

- Source : `napoleon_hit_crouch_hd_v01.png` (2172 × 724, sha256 `cf22f99f358418c7…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6696 ; repère `rearFoot`

### jump

- Source : `napoleon_jump_hd_v01.png` (1536 × 1024, sha256 `365706e7d2365822…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.1299 ; repère `torso`

### jump_punch

- Source : `napoleon_jump_punch_hd_v01.png` (2172 × 724, sha256 `786eaa151534bf54…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.7952 ; repère `torso`

### jump_kick

- Source : `napoleon_jump_kick_hd_v01.png` (2172 × 724, sha256 `6a5a0b7233b81754…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.7098 ; repère `torso`

### ko

- Source : `napoleon_ko_hd_v01.png` (1536 × 1024, sha256 `6fbfe38c7d1836f1…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9092 ; repère `rearFoot`

### imperial_volley_projectile

- Source : `napoleon_special_imperial_volley_projectile_hd_v01.png` (1254 × 1254, sha256 `98a42f4dfc111773…`)
- Découpage : {"type":"grid","rows":2,"columns":2} → 4 poses ; échelle 0.4500 ; repère `head`

### imperial_volley_impact

- Source : `napoleon_special_imperial_volley_impact_hd_v01.png` (1254 × 1254, sha256 `5254dab093ac5fe9…`)
- Découpage : {"type":"grid","rows":2,"columns":2} → 4 poses ; échelle 0.4500 ; repère `centroid`
  - `idle_01` : pose source à (106, 16), 351 × 501 px → 336 × 480 px pleins ; décalage dans la cellule (404, 223)
  - `idle_02` : pose source à (587, 17), 357 × 500 px → 342 × 479 px pleins ; décalage dans la cellule (405, 224)
  - `idle_03` : pose source à (1096, 16), 368 × 501 px → 352 × 481 px pleins ; décalage dans la cellule (401, 222)
  - `idle_04` : pose source à (104, 521), 355 × 486 px → 340 × 465 px pleins ; décalage dans la cellule (404, 238)
  - `idle_05` : pose source à (588, 525), 363 × 482 px → 347 × 462 px pleins ; décalage dans la cellule (406, 241)
  - `idle_06` : pose source à (1094, 521), 357 × 486 px → 340 × 465 px pleins ; décalage dans la cellule (404, 238)
  - `walk_forward_01` : pose source à (45, 11), 361 × 497 px → 345 × 476 px pleins ; décalage dans la cellule (389, 227)
  - `walk_forward_02` : pose source à (416, 12), 310 × 497 px → 296 × 477 px pleins ; décalage dans la cellule (391, 227)
  - `walk_forward_03` : pose source à (788, 14), 289 × 495 px → 276 × 475 px pleins ; décalage dans la cellule (411, 229)
  - `walk_forward_04` : pose source à (1139, 13), 346 × 492 px → 331 × 471 px pleins ; décalage dans la cellule (382, 232)
  - `walk_forward_05` : pose source à (29, 515), 375 × 490 px → 359 × 468 px pleins ; décalage dans la cellule (374, 234)
  - `walk_forward_06` : pose source à (421, 516), 342 × 492 px → 327 × 471 px pleins ; décalage dans la cellule (392, 232)
  - `walk_forward_07` : pose source à (799, 516), 288 × 491 px → 275 × 469 px pleins ; décalage dans la cellule (407, 233)
  - `walk_forward_08` : pose source à (1163, 516), 347 × 484 px → 332 × 463 px pleins ; décalage dans la cellule (388, 239)
  - `stand_punch_light_01` : pose source à (65, 15), 357 × 493 px → 341 × 472 px pleins ; décalage dans la cellule (403, 231)
  - `stand_punch_light_02` : pose source à (552, 16), 393 × 493 px → 376 × 472 px pleins ; décalage dans la cellule (398, 231)
  - `stand_punch_light_03` : pose source à (1022, 17), 465 × 492 px → 445 × 471 px pleins ; décalage dans la cellule (391, 232)
  - `stand_punch_light_04` : pose source à (45, 517), 507 × 491 px → 486 × 469 px pleins ; décalage dans la cellule (406, 233)
  - `stand_punch_light_05` : pose source à (565, 520), 401 × 488 px → 384 × 467 px pleins ; décalage dans la cellule (387, 236)
  - `stand_punch_light_06` : pose source à (1074, 517), 374 × 491 px → 358 × 469 px pleins ; décalage dans la cellule (402, 233)
  - `stand_kick_light_01` : pose source à (57, 11), 357 × 492 px → 342 × 470 px pleins ; décalage dans la cellule (398, 232)
  - `stand_kick_light_02` : pose source à (575, 9), 343 × 495 px → 328 × 474 px pleins ; décalage dans la cellule (329, 229)
  - `stand_kick_light_03` : pose source à (1043, 15), 479 × 489 px → 459 × 469 px pleins ; décalage dans la cellule (294, 235)
  - `stand_kick_light_04` : pose source à (37, 515), 525 × 491 px → 503 × 470 px pleins ; décalage dans la cellule (307, 233)
  - `stand_kick_light_05` : pose source à (611, 515), 299 × 493 px → 286 × 472 px pleins ; décalage dans la cellule (289, 231)
  - `stand_kick_light_06` : pose source à (1077, 521), 376 × 489 px → 360 × 469 px pleins ; décalage dans la cellule (384, 234)
  - `stand_kick_heavy_01` : pose source à (16, 25), 324 × 473 px → 329 × 479 px pleins ; décalage dans la cellule (404, 223)
  - `stand_kick_heavy_02` : pose source à (377, 48), 328 × 449 px → 332 × 458 px pleins ; décalage dans la cellule (404, 246)
  - `stand_kick_heavy_03` : pose source à (763, 24), 274 × 476 px → 276 × 484 px pleins ; décalage dans la cellule (283, 220)
  - `stand_kick_heavy_04` : pose source à (1088, 50), 432 × 451 px → 440 × 457 px pleins ; décalage dans la cellule (262, 245)
  - `stand_kick_heavy_05` : pose source à (18, 538), 497 × 453 px → 505 × 461 px pleins ; décalage dans la cellule (292, 243)
  - `stand_kick_heavy_06` : pose source à (529, 532), 315 × 459 px → 320 × 466 px pleins ; décalage dans la cellule (306, 237)
  - `stand_kick_heavy_07` : pose source à (864, 530), 274 × 460 px → 278 × 466 px pleins ; décalage dans la cellule (253, 236)
  - `stand_kick_heavy_08` : pose source à (1181, 530), 316 × 461 px → 322 × 468 px pleins ; décalage dans la cellule (401, 234)
  - `special_imperial_volley_cast_01` : pose source à (40, 29), 362 × 478 px → 364 × 480 px pleins ; décalage dans la cellule (387, 223)
  - `special_imperial_volley_cast_02` : pose source à (412, 16), 295 × 489 px → 296 × 493 px pleins ; décalage dans la cellule (381, 211)
  - `special_imperial_volley_cast_03` : pose source à (738, 22), 347 × 483 px → 349 × 486 px pleins ; décalage dans la cellule (392, 217)
  - `special_imperial_volley_cast_04` : pose source à (1088, 33), 406 × 475 px → 410 × 477 px pleins ; décalage dans la cellule (389, 225)
  - `special_imperial_volley_cast_05` : pose source à (18, 514), 431 × 473 px → 434 × 475 px pleins ; décalage dans la cellule (390, 227)
  - `special_imperial_volley_cast_06` : pose source à (416, 514), 441 × 473 px → 445 × 477 px pleins ; décalage dans la cellule (381, 227)
  - `special_imperial_volley_cast_07` : pose source à (824, 518), 348 × 469 px → 351 × 472 px pleins ; décalage dans la cellule (399, 231)
  - `special_imperial_volley_cast_08` : pose source à (1168, 516), 351 × 471 px → 353 × 473 px pleins ; décalage dans la cellule (381, 229)
  - `hit_high_01` : pose source à (11, 109), 521 × 799 px → 324 × 497 px pleins ; décalage dans la cellule (422, 206)
  - `hit_high_02` : pose source à (526, 130), 541 × 774 px → 338 × 481 px pleins ; décalage dans la cellule (416, 222)
  - `hit_high_03` : pose source à (1041, 134), 488 × 771 px → 305 × 480 px pleins ; décalage dans la cellule (421, 224)
  - `block_high_01` : pose source à (50, 13), 582 × 776 px → 363 × 486 px pleins ; décalage dans la cellule (403, 218)
  - `block_high_02` : pose source à (702, 56), 592 × 733 px → 369 × 458 px pleins ; décalage dans la cellule (404, 245)
  - `block_high_03` : pose source à (1317, 22), 584 × 767 px → 364 × 479 px pleins ; décalage dans la cellule (382, 224)
  - `crouch_01` : pose source à (110, 15), 528 × 693 px → 365 × 481 px pleins ; décalage dans la cellule (384, 224)
  - `crouch_02` : pose source à (841, 88), 553 × 620 px → 384 × 429 px pleins ; décalage dans la cellule (382, 274)
  - `crouch_03` : pose source à (1584, 206), 494 × 502 px → 342 × 347 px pleins ; décalage dans la cellule (374, 356)
  - `block_low_01` : pose source à (59, 60), 617 × 622 px → 344 × 347 px pleins ; décalage dans la cellule (410, 357)
  - `block_low_02` : pose source à (763, 76), 645 × 606 px → 360 × 338 px pleins ; décalage dans la cellule (390, 366)
  - `block_low_03` : pose source à (1507, 60), 617 × 622 px → 344 × 348 px pleins ; décalage dans la cellule (410, 357)
  - `crouch_punch_01` : pose source à (17, 80), 479 × 411 px → 406 × 347 px pleins ; décalage dans la cellule (347, 356)
  - `crouch_punch_02` : pose source à (526, 89), 479 × 402 px → 406 × 340 px pleins ; décalage dans la cellule (347, 363)
  - `crouch_punch_03` : pose source à (1031, 98), 493 × 394 px → 417 × 333 px pleins ; décalage dans la cellule (344, 371)
  - `crouch_punch_04` : pose source à (12, 579), 575 × 404 px → 487 × 341 px pleins ; décalage dans la cellule (345, 362)
  - `crouch_punch_05` : pose source à (557, 579), 483 × 404 px → 408 × 341 px pleins ; décalage dans la cellule (348, 362)
  - `crouch_punch_06` : pose source à (1048, 579), 477 × 404 px → 403 × 340 px pleins ; décalage dans la cellule (343, 362)
  - `crouch_kick_01` : pose source à (47, 112), 378 × 390 px → 338 × 347 px pleins ; décalage dans la cellule (436, 356)
  - `crouch_kick_02` : pose source à (528, 171), 428 × 330 px → 380 × 295 px pleins ; décalage dans la cellule (424, 408)
  - `crouch_kick_03` : pose source à (1021, 172), 498 × 328 px → 445 × 292 px pleins ; décalage dans la cellule (436, 411)
  - `crouch_kick_04` : pose source à (19, 633), 603 × 341 px → 538 × 303 px pleins ; décalage dans la cellule (439, 400)
  - `crouch_kick_05` : pose source à (611, 627), 462 × 343 px → 414 × 305 px pleins ; décalage dans la cellule (432, 398)
  - `crouch_kick_06` : pose source à (1104, 591), 381 × 388 px → 338 × 346 px pleins ; décalage dans la cellule (436, 357)
  - `hit_crouch_01` : pose source à (93, 66), 570 × 610 px → 381 × 406 px pleins ; décalage dans la cellule (413, 297)
  - `hit_crouch_02` : pose source à (819, 95), 565 × 581 px → 377 × 387 px pleins ; décalage dans la cellule (409, 316)
  - `hit_crouch_03` : pose source à (1530, 155), 519 × 521 px → 347 × 348 px pleins ; décalage dans la cellule (388, 356)
  - `jump_01` : pose source à (63, 150), 366 × 404 px → 414 × 453 px pleins ; décalage dans la cellule (346, 249)
  - `jump_02` : pose source à (594, 8), 324 × 568 px → 363 × 640 px pleins ; décalage dans la cellule (361, 62)
  - `jump_03` : pose source à (1074, 77), 342 × 366 px → 383 × 411 px pleins ; décalage dans la cellule (364, 291)
  - `jump_04` : pose source à (98, 589), 305 × 318 px → 342 × 356 px pleins ; décalage dans la cellule (389, 346)
  - `jump_05` : pose source à (594, 576), 370 × 436 px → 416 × 493 px pleins ; décalage dans la cellule (390, 212)
  - `jump_06` : pose source à (1080, 621), 410 × 382 px → 461 × 430 px pleins ; décalage dans la cellule (328, 273)
  - `jump_punch_01` : pose source à (27, 87), 408 × 450 px → 323 × 358 px pleins ; décalage dans la cellule (466, 346)
  - `jump_punch_02` : pose source à (543, 101), 448 × 448 px → 354 × 355 px pleins ; décalage dans la cellule (466, 349)
  - `jump_punch_03` : pose source à (1056, 165), 661 × 440 px → 524 × 348 px pleins ; décalage dans la cellule (446, 355)
  - `jump_punch_04` : pose source à (1741, 109), 366 × 458 px → 290 × 362 px pleins ; décalage dans la cellule (494, 341)
  - `jump_kick_01` : pose source à (26, 40), 455 × 505 px → 322 × 357 px pleins ; décalage dans la cellule (396, 347)
  - `jump_kick_02` : pose source à (564, 43), 433 × 634 px → 306 × 450 px pleins ; décalage dans la cellule (428, 254)
  - `jump_kick_03` : pose source à (1017, 76), 734 × 471 px → 520 × 334 px pleins ; décalage dans la cellule (411, 370)
  - `jump_kick_04` : pose source à (1764, 63), 397 × 516 px → 280 × 366 px pleins ; décalage dans la cellule (433, 339)
  - `ko_01` : pose source à (43, 21), 467 × 532 px → 423 × 482 px pleins ; décalage dans la cellule (332, 221)
  - `ko_02` : pose source à (580, 104), 358 × 451 px → 323 × 409 px pleins ; décalage dans la cellule (282, 294)
  - `ko_03` : pose source à (999, 137), 519 × 409 px → 470 × 370 px pleins ; décalage dans la cellule (0, 333)
  - `ko_04` : pose source à (16, 582), 476 × 391 px → 431 × 353 px pleins ; décalage dans la cellule (77, 350)
  - `ko_05` : pose source à (479, 758), 513 × 206 px → 464 × 184 px pleins ; décalage dans la cellule (266, 518)
  - `ko_06` : pose source à (1010, 800), 510 × 157 px → 461 × 141 px pleins ; décalage dans la cellule (287, 562)
  - `imperial_volley_projectile_01` : pose source à (0, 0), 627 × 627 px → 216 × 120 px pleins ; décalage dans la cellule (312, 225)
  - `imperial_volley_projectile_02` : pose source à (627, 0), 627 × 627 px → 214 × 132 px pleins ; décalage dans la cellule (326, 228)
  - `imperial_volley_projectile_03` : pose source à (0, 627), 627 × 627 px → 225 × 128 px pleins ; décalage dans la cellule (324, 238)
  - `imperial_volley_projectile_04` : pose source à (627, 627), 627 × 627 px → 216 × 117 px pleins ; décalage dans la cellule (318, 236)
  - `imperial_volley_impact_01` : pose source à (0, 0), 627 × 627 px → 116 × 128 px pleins ; décalage dans la cellule (370, 234)
  - `imperial_volley_impact_02` : pose source à (627, 0), 627 × 627 px → 253 × 241 px pleins ; décalage dans la cellule (373, 233)
  - `imperial_volley_impact_03` : pose source à (0, 627), 627 × 627 px → 243 × 220 px pleins ; décalage dans la cellule (365, 251)
  - `imperial_volley_impact_04` : pose source à (627, 627), 627 × 627 px → 202 × 201 px pleins ; décalage dans la cellule (370, 239)
