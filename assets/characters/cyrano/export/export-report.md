# Export technique — cyrano

Généré par `npm run assets:export -- tools/export-character/configs/cyrano.json`. Ne pas modifier à la main : relancer l'outil.

## Résultat

- 97 poses. Cellules personnage 1024 × 768, pivot (512, 704) au sol entre les pieds ; cellules d'effet 1024 × 768, pivot au centre de l'effet (512, 384).
- Hauteur de garde ramenée à 500 px (= unités monde du jeu).
- Images individuelles : `assets/characters/cyrano/export/frames/cyrano_<animation>_<NN>.png` (NN = numéro de pose, à partir de 01).
- Atlas du jeu : `atlas-0.png` 4088 × 3928, `atlas-1.png` 3956 × 1240 dans `game/Assets/Resources/Fighters/cyrano`.
- Aperçus : `assets/characters/cyrano/export/previews` (planche sur fonds sombre et clair avec pivot et sol, superposition des poses).

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

- Source : `cyrano_idle_hd_v01.png` (1536 × 1024, sha256 `8fb2a5412881c44b…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0288 ; repère `rearFoot`

### walk_forward

- Source : `cyrano_walk_forward_hd_v01.png` (1774 × 887, sha256 `33533a1b63a460a1…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.0288 ; repère `torso`

### stand_punch_light

- Source : `cyrano_stand_punch_light_hd_v01.png` (1536 × 1024, sha256 `0e81d92c74e6239d…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0288 ; repère `rearFoot`

### stand_kick_light

- Source : `cyrano_stand_kick_light_hd_v01.png` (1536 × 1024, sha256 `ece0ca80ea98c850…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0288 ; repère `rearFoot`

### stand_kick_heavy

- Source : `cyrano_stand_kick_heavy_hd_v01.png` (1774 × 887, sha256 `d3ea76093d9c1ff2…`)
- Découpage : {"type":"poses","rows":2,"seams":[{"row":1,"x":1300},{"row":2,"x":515}]} → 8 poses ; échelle 1.2048 ; repère `rearFoot`

### special_panache_cast

- Source : `cyrano_special_panache_cast_hd_v01.png` (1774 × 887, sha256 `f16c622652ec4b87…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.1848 ; repère `rearFoot`

### hit_high

- Source : `cyrano_hit_high_hd_v01.png` (2172 × 724, sha256 `a67c8280f5716f7a…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.7342 ; repère `rearFoot`

### block_high

- Source : `cyrano_block_high_hd_v01.png` (2172 × 724, sha256 `ad1a81c916e82245…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.7937 ; repère `rearFoot`

### crouch

- Source : `cyrano_crouch_hd_v01.png` (2172 × 724, sha256 `0cef51afa73acf55…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.7396 ; repère `rearFoot`

### block_low

- Source : `cyrano_block_low_hd_v01.png` (2172 × 724, sha256 `26ec8fa1201839a9…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6406 ; repère `rearFoot`

### crouch_punch

- Source : `cyrano_crouch_punch_hd_v01.png` (1536 × 1024, sha256 `4853e165dea245b0…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.7632 ; repère `rearFoot`

### crouch_kick

- Source : `cyrano_crouch_kick_hd_v01.png` (1536 × 1024, sha256 `052809f960899822…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9412 ; repère `rearFoot`

### hit_crouch

- Source : `cyrano_hit_crouch_hd_v01.png` (2172 × 724, sha256 `cf5c5f952016aa9a…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6701 ; repère `rearFoot`

### jump

- Source : `cyrano_jump_hd_v01.png` (1536 × 1024, sha256 `6b33f4860814cc22…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0918 ; repère `torso`

### jump_punch

- Source : `cyrano_jump_punch_hd_v01.png` (2172 × 724, sha256 `bffad9826a661b2e…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.6839 ; repère `torso`

### jump_kick

- Source : `cyrano_jump_kick_hd_v01.png` (2172 × 724, sha256 `6465ed13652c248d…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.6826 ; repère `torso`

### ko

- Source : `cyrano_ko_hd_v01.png` (1536 × 1024, sha256 `a3d1b31ffe277c28…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0426 ; repère `rearFoot`

### panache_projectile

- Source : `cyrano_special_panache_projectile_hd_v01.png` (1254 × 1254, sha256 `cad207e665578591…`)
- Découpage : {"type":"grid","rows":2,"columns":2} → 4 poses ; échelle 0.4500 ; repère `head`

### panache_impact

- Source : `cyrano_special_panache_impact_hd_v01.png` (1254 × 1254, sha256 `ac60c68c67df3fe6…`)
- Découpage : {"type":"grid","rows":2,"columns":2} → 4 poses ; échelle 0.4500 ; repère `centroid`
  - `idle_01` : pose source à (39, 19), 471 × 489 px → 478 × 501 px pleins ; décalage dans la cellule (280, 202)
  - `idle_02` : pose source à (546, 22), 477 × 486 px → 483 × 497 px pleins ; décalage dans la cellule (276, 205)
  - `idle_03` : pose source à (1068, 17), 468 × 491 px → 476 × 503 px pleins ; décalage dans la cellule (283, 200)
  - `idle_04` : pose source à (33, 531), 478 × 481 px → 486 × 492 px pleins ; décalage dans la cellule (274, 210)
  - `idle_05` : pose source à (550, 535), 474 × 477 px → 483 × 489 px pleins ; décalage dans la cellule (278, 214)
  - `idle_06` : pose source à (1060, 532), 474 × 480 px → 480 × 490 px pleins ; décalage dans la cellule (277, 211)
  - `walk_forward_01` : pose source à (33, 8), 443 × 421 px → 448 × 430 px pleins ; décalage dans la cellule (337, 272)
  - `walk_forward_02` : pose source à (497, 43), 412 × 384 px → 417 × 391 px pleins ; décalage dans la cellule (367, 310)
  - `walk_forward_03` : pose source à (932, 8), 397 × 417 px → 400 × 426 px pleins ; décalage dans la cellule (379, 276)
  - `walk_forward_04` : pose source à (1372, 0), 395 × 430 px → 398 × 436 px pleins ; décalage dans la cellule (369, 263)
  - `walk_forward_05` : pose source à (33, 453), 460 × 412 px → 466 × 421 px pleins ; décalage dans la cellule (316, 281)
  - `walk_forward_06` : pose source à (496, 490), 421 × 376 px → 426 × 384 px pleins ; décalage dans la cellule (352, 318)
  - `walk_forward_07` : pose source à (951, 452), 385 × 415 px → 387 × 424 px pleins ; décalage dans la cellule (378, 279)
  - `walk_forward_08` : pose source à (1374, 448), 400 × 414 px → 409 × 423 px pleins ; décalage dans la cellule (343, 279)
  - `stand_punch_light_01` : pose source à (38, 46), 532 × 469 px → 543 × 483 px pleins ; décalage dans la cellule (294, 221)
  - `stand_punch_light_02` : pose source à (603, 55), 322 × 459 px → 330 × 469 px pleins ; décalage dans la cellule (309, 233)
  - `stand_punch_light_03` : pose source à (1003, 87), 530 × 428 px → 541 × 438 px pleins ; décalage dans la cellule (296, 264)
  - `stand_punch_light_04` : pose source à (4, 574), 643 × 405 px → 657 × 416 px pleins ; décalage dans la cellule (314, 287)
  - `stand_punch_light_05` : pose source à (590, 560), 443 × 435 px → 454 × 447 px pleins ; décalage dans la cellule (315, 257)
  - `stand_punch_light_06` : pose source à (1053, 555), 473 × 435 px → 482 × 446 px pleins ; décalage dans la cellule (297, 257)
  - `stand_kick_light_01` : pose source à (32, 26), 490 × 482 px → 496 × 493 px pleins ; décalage dans la cellule (280, 209)
  - `stand_kick_light_02` : pose source à (573, 20), 461 × 482 px → 467 × 492 px pleins ; décalage dans la cellule (199, 209)
  - `stand_kick_light_03` : pose source à (1059, 25), 465 × 478 px → 473 × 489 px pleins ; décalage dans la cellule (185, 213)
  - `stand_kick_light_04` : pose source à (16, 528), 507 × 478 px → 519 × 489 px pleins ; décalage dans la cellule (257, 213)
  - `stand_kick_light_05` : pose source à (569, 525), 479 × 481 px → 487 × 493 px pleins ; décalage dans la cellule (223, 210)
  - `stand_kick_light_06` : pose source à (1066, 532), 457 × 481 px → 465 × 491 px pleins ; décalage dans la cellule (282, 210)
  - `stand_kick_heavy_01` : pose source à (13, 17), 431 × 416 px → 510 × 500 px pleins ; décalage dans la cellule (262, 204)
  - `stand_kick_heavy_02` : pose source à (458, 28), 428 × 411 px → 506 × 492 px pleins ; décalage dans la cellule (212, 210)
  - `stand_kick_heavy_03` : pose source à (891, 8), 403 × 431 px → 481 × 516 px pleins ; décalage dans la cellule (164, 186)
  - `stand_kick_heavy_04` : pose source à (1286, 18), 473 × 421 px → 560 × 504 px pleins ; décalage dans la cellule (88, 198)
  - `stand_kick_heavy_05` : pose source à (5, 466), 507 × 401 px → 605 × 480 px pleins ; décalage dans la cellule (120, 222)
  - `stand_kick_heavy_06` : pose source à (510, 453), 379 × 415 px → 447 × 497 px pleins ; décalage dans la cellule (130, 205)
  - `stand_kick_heavy_07` : pose source à (891, 458), 440 × 410 px → 520 × 491 px pleins ; décalage dans la cellule (181, 211)
  - `stand_kick_heavy_08` : pose source à (1327, 452), 431 × 415 px → 509 × 496 px pleins ; décalage dans la cellule (263, 206)
  - `special_panache_cast_01` : pose source à (41, 14), 473 × 423 px → 552 × 500 px pleins ; décalage dans la cellule (278, 204)
  - `special_panache_cast_02` : pose source à (543, 14), 295 × 421 px → 348 × 498 px pleins ; décalage dans la cellule (288, 206)
  - `special_panache_cast_03` : pose source à (924, 9), 381 × 426 px → 447 × 502 px pleins ; décalage dans la cellule (259, 200)
  - `special_panache_cast_04` : pose source à (1326, 33), 442 × 399 px → 518 × 473 px pleins ; décalage dans la cellule (271, 232)
  - `special_panache_cast_05` : pose source à (9, 463), 538 × 395 px → 629 × 465 px pleins ; décalage dans la cellule (301, 237)
  - `special_panache_cast_06` : pose source à (446, 465), 533 × 394 px → 624 × 465 px pleins ; décalage dans la cellule (275, 237)
  - `special_panache_cast_07` : pose source à (931, 455), 430 × 404 px → 506 × 477 px pleins ; décalage dans la cellule (260, 225)
  - `special_panache_cast_08` : pose source à (1357, 455), 414 × 408 px → 481 × 481 px pleins ; décalage dans la cellule (281, 222)
  - `hit_high_01` : pose source à (23, 24), 647 × 681 px → 473 × 498 px pleins ; décalage dans la cellule (198, 205)
  - `hit_high_02` : pose source à (739, 20), 723 × 684 px → 530 × 502 px pleins ; décalage dans la cellule (208, 203)
  - `hit_high_03` : pose source à (1540, 19), 578 × 683 px → 423 × 499 px pleins ; décalage dans la cellule (300, 203)
  - `block_high_01` : pose source à (85, 22), 645 × 672 px → 508 × 531 px pleins ; décalage dans la cellule (308, 171)
  - `block_high_02` : pose source à (866, 4), 556 × 689 px → 439 × 545 px pleins ; décalage dans la cellule (301, 157)
  - `block_high_03` : pose source à (1542, 59), 623 × 634 px → 489 × 500 px pleins ; décalage dans la cellule (298, 202)
  - `crouch_01` : pose source à (49, 20), 698 × 679 px → 510 × 500 px pleins ; décalage dans la cellule (275, 203)
  - `crouch_02` : pose source à (767, 112), 716 × 588 px → 522 × 434 px pleins ; décalage dans la cellule (256, 270)
  - `crouch_03` : pose source à (1459, 244), 701 × 457 px → 513 × 335 px pleins ; décalage dans la cellule (233, 367)
  - `block_low_01` : pose source à (17, 118), 784 × 527 px → 497 × 335 px pleins ; décalage dans la cellule (263, 368)
  - `block_low_02` : pose source à (849, 120), 583 × 515 px → 368 × 328 px pleins ; décalage dans la cellule (246, 375)
  - `block_low_03` : pose source à (1497, 115), 642 × 530 px → 406 × 339 px pleins ; décalage dans la cellule (263, 364)
  - `crouch_punch_01` : pose source à (16, 52), 548 × 441 px → 413 × 335 px pleins ; décalage dans la cellule (332, 368)
  - `crouch_punch_02` : pose source à (558, 59), 442 × 432 px → 337 × 328 px pleins ; décalage dans la cellule (335, 375)
  - `crouch_punch_03` : pose source à (1010, 71), 526 × 422 px → 400 × 320 px pleins ; décalage dans la cellule (318, 383)
  - `crouch_punch_04` : pose source à (13, 575), 632 × 406 px → 477 × 308 px pleins ; décalage dans la cellule (328, 395)
  - `crouch_punch_05` : pose source à (571, 563), 442 × 421 px → 335 × 319 px pleins ; décalage dans la cellule (334, 384)
  - `crouch_punch_06` : pose source à (1012, 573), 514 × 414 px → 388 × 314 px pleins ; décalage dans la cellule (317, 389)
  - `crouch_kick_01` : pose source à (33, 125), 462 × 358 px → 432 × 335 px pleins ; décalage dans la cellule (390, 368)
  - `crouch_kick_02` : pose source à (529, 137), 428 × 352 px → 401 × 328 px pleins ; décalage dans la cellule (240, 374)
  - `crouch_kick_03` : pose source à (1011, 179), 514 × 308 px → 481 × 287 px pleins ; décalage dans la cellule (394, 415)
  - `crouch_kick_04` : pose source à (15, 616), 542 × 322 px → 507 × 300 px pleins ; décalage dans la cellule (394, 403)
  - `crouch_kick_05` : pose source à (556, 592), 480 × 341 px → 448 × 320 px pleins ; décalage dans la cellule (392, 384)
  - `crouch_kick_06` : pose source à (1050, 611), 465 × 328 px → 435 × 306 px pleins ; décalage dans la cellule (377, 396)
  - `hit_crouch_01` : pose source à (40, 130), 670 × 526 px → 446 × 351 px pleins ; décalage dans la cellule (262, 353)
  - `hit_crouch_02` : pose source à (728, 119), 700 × 540 px → 466 × 360 px pleins ; décalage dans la cellule (261, 343)
  - `hit_crouch_03` : pose source à (1482, 155), 575 × 502 px → 383 × 334 px pleins ; décalage dans la cellule (294, 369)
  - `jump_01` : pose source à (49, 112), 504 × 382 px → 543 × 415 px pleins ; décalage dans la cellule (249, 288)
  - `jump_02` : pose source à (539, 11), 477 × 488 px → 516 × 529 px pleins ; décalage dans la cellule (254, 172)
  - `jump_03` : pose source à (1053, 0), 473 × 376 px → 514 × 408 px pleins ; décalage dans la cellule (245, 294)
  - `jump_04` : pose source à (39, 522), 495 × 344 px → 536 × 374 px pleins ; décalage dans la cellule (247, 329)
  - `jump_05` : pose source à (551, 514), 499 × 483 px → 539 × 524 px pleins ; décalage dans la cellule (246, 178)
  - `jump_06` : pose source à (1056, 609), 470 × 399 px → 510 × 433 px pleins ; décalage dans la cellule (246, 269)
  - `jump_punch_01` : pose source à (43, 41), 441 × 548 px → 297 × 373 px pleins ; décalage dans la cellule (461, 330)
  - `jump_punch_02` : pose source à (582, 108), 475 × 483 px → 321 × 328 px pleins ; décalage dans la cellule (439, 375)
  - `jump_punch_03` : pose source à (1073, 113), 769 × 485 px → 522 × 330 px pleins ; décalage dans la cellule (392, 373)
  - `jump_punch_04` : pose source à (1775, 112), 388 × 479 px → 264 × 326 px pleins ; décalage dans la cellule (451, 377)
  - `jump_kick_01` : pose source à (32, 74), 541 × 549 px → 366 × 374 px pleins ; décalage dans la cellule (367, 330)
  - `jump_kick_02` : pose source à (554, 89), 515 × 520 px → 348 × 353 px pleins ; décalage dans la cellule (391, 350)
  - `jump_kick_03` : pose source à (1077, 104), 562 × 433 px → 381 × 294 px pleins ; décalage dans la cellule (389, 409)
  - `jump_kick_04` : pose source à (1665, 83), 496 × 553 px → 335 × 376 px pleins ; décalage dans la cellule (406, 328)
  - `ko_01` : pose source à (44, 82), 460 × 484 px → 477 × 502 px pleins ; décalage dans la cellule (217, 200)
  - `ko_02` : pose source à (540, 161), 370 × 414 px → 384 × 431 px pleins ; décalage dans la cellule (218, 273)
  - `ko_03` : pose source à (1026, 67), 492 × 521 px → 509 × 537 px pleins ; décalage dans la cellule (170, 161)
  - `ko_04` : pose source à (16, 588), 497 × 368 px → 515 × 380 px pleins ; décalage dans la cellule (279, 321)
  - `ko_05` : pose source à (522, 794), 488 × 171 px → 507 × 176 px pleins ; décalage dans la cellule (298, 527)
  - `ko_06` : pose source à (1023, 794), 502 × 171 px → 521 × 176 px pleins ; décalage dans la cellule (298, 528)
  - `panache_projectile_01` : pose source à (0, 0), 627 × 627 px → 255 × 120 px pleins ; décalage dans la cellule (338, 228)
  - `panache_projectile_02` : pose source à (627, 0), 627 × 627 px → 249 × 120 px pleins ; décalage dans la cellule (339, 223)
  - `panache_projectile_03` : pose source à (0, 627), 627 × 627 px → 247 × 116 px pleins ; décalage dans la cellule (338, 247)
  - `panache_projectile_04` : pose source à (627, 627), 627 × 627 px → 249 × 122 px pleins ; décalage dans la cellule (334, 242)
  - `panache_impact_01` : pose source à (0, 0), 627 × 627 px → 120 × 130 px pleins ; décalage dans la cellule (367, 228)
  - `panache_impact_02` : pose source à (627, 0), 627 × 627 px → 245 × 233 px pleins ; décalage dans la cellule (372, 228)
  - `panache_impact_03` : pose source à (0, 627), 627 × 627 px → 228 × 218 px pleins ; décalage dans la cellule (362, 246)
  - `panache_impact_04` : pose source à (627, 627), 627 × 627 px → 169 × 179 px pleins ; décalage dans la cellule (371, 231)
