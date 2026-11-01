# Export technique — jeanne

Généré par `npm run assets:export -- tools/export-character/configs/jeanne.json`. Ne pas modifier à la main : relancer l'outil.

## Résultat

- 89 poses. Cellules personnage 1024 × 768, pivot (512, 704) au sol entre les pieds ; cellules d'effet 1024 × 768, pivot au centre de l'effet (512, 384).
- Hauteur de garde ramenée à 470 px (= unités monde du jeu).
- Images individuelles : `assets/characters/jeanne/export/frames/jeanne_<animation>_<NN>.png` (NN = numéro de pose, à partir de 01).
- Atlas du jeu : `atlas-0.png` 4076 × 3908 dans `game/Assets/Resources/Fighters/jeanne`.
- Aperçus : `assets/characters/jeanne/export/previews` (planche sur fonds sombre et clair avec pivot et sol, superposition des poses).

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

- Source : `jeanne_idle_hd_v01.png` (1536 × 1024, sha256 `eb411b0289bc1671…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9400 ; repère `rearFoot`

### walk_forward

- Source : `jeanne_walk_forward_hd_v01.png` (1536 × 1024, sha256 `b628f5dfa984246a…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 0.9400 ; repère `torso`

### stand_punch_light

- Source : `jeanne_stand_punch_light_hd_v01.png` (1536 × 1024, sha256 `f21c44bce230bb05…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9400 ; repère `rearFoot`

### stand_kick_light

- Source : `jeanne_stand_kick_light_hd_v01.png` (1536 × 1024, sha256 `abef82cb604548e3…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9400 ; repère `rearFoot`

### stand_kick_heavy

- Source : `jeanne_stand_kick_heavy_hd_v01.png` (1536 × 1024, sha256 `a2ff7ba4f97c24a2…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.0173 ; repère `rearFoot`

### special_orleans_cast

- Source : `jeanne_special_orleans_cast_hd_v01.png` (1536 × 1024, sha256 `981869cf44ce2bc0…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.2113 ; repère `rearFoot`

### hit_high

- Source : `jeanne_hit_high_hd_v01.png` (2172 × 724, sha256 `51ffe1a8b9b0e30f…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6802 ; repère `rearFoot`

### block_high

- Source : `jeanne_block_high_hd_v01.png` (2172 × 724, sha256 `f52e9ca0cee40b69…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6942 ; repère `rearFoot`

### crouch

- Source : `jeanne_crouch_hd_v01.png` (2172 × 724, sha256 `61bad91b704cbb09…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6772 ; repère `rearFoot`

### block_low

- Source : `jeanne_block_low_hd_v01.png` (2172 × 724, sha256 `d0762bddbfb130af…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6422 ; repère `rearFoot`

### crouch_punch

- Source : `jeanne_crouch_punch_hd_v01.png` (1536 × 1024, sha256 `74cb4d859ab9e548…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.7735 ; repère `rearFoot`

### crouch_kick

- Source : `jeanne_crouch_kick_hd_v01.png` (1536 × 1024, sha256 `e6784a0213a46c26…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.8120 ; repère `rearFoot`

### hit_crouch

- Source : `jeanne_hit_crouch_hd_v01.png` (2172 × 724, sha256 `a678cb0fe75a1524…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.5589 ; repère `rearFoot`

### jump

- Source : `jeanne_jump_hd_v01.png` (1536 × 1024, sha256 `6fcd26573881d84a…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.1604 ; repère `torso`

### jump_punch

- Source : `jeanne_jump_punch_hd_v01.png` (2172 × 724, sha256 `0edc010e8581661f…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.6779 ; repère `torso`

### jump_kick

- Source : `jeanne_jump_kick_hd_v01.png` (2172 × 724, sha256 `2a10ad9287a4ee73…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.8540 ; repère `torso`

### ko

- Source : `jeanne_ko_hd_v01.png` (1536 × 1024, sha256 `de8b48af1e39eb49…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9198 ; repère `rearFoot`
  - `idle_01` : pose source à (126, 12), 361 × 501 px → 337 × 470 px pleins ; décalage dans la cellule (343, 233)
  - `idle_02` : pose source à (580, 13), 362 × 500 px → 338 × 469 px pleins ; décalage dans la cellule (342, 234)
  - `idle_03` : pose source à (1032, 13), 361 × 500 px → 337 × 469 px pleins ; décalage dans la cellule (343, 234)
  - `idle_04` : pose source à (126, 524), 360 × 495 px → 337 × 463 px pleins ; décalage dans la cellule (343, 240)
  - `idle_05` : pose source à (581, 524), 361 × 495 px → 337 × 463 px pleins ; décalage dans la cellule (343, 240)
  - `idle_06` : pose source à (1032, 524), 361 × 495 px → 337 × 463 px pleins ; décalage dans la cellule (343, 240)
  - `walk_forward_01` : pose source à (38, 15), 388 × 497 px → 363 × 465 px pleins ; décalage dans la cellule (330, 238)
  - `walk_forward_02` : pose source à (458, 11), 263 × 500 px → 247 × 468 px pleins ; décalage dans la cellule (368, 235)
  - `walk_forward_03` : pose source à (843, 15), 245 × 496 px → 228 × 464 px pleins ; décalage dans la cellule (382, 239)
  - `walk_forward_04` : pose source à (1218, 14), 252 × 498 px → 235 × 466 px pleins ; décalage dans la cellule (381, 237)
  - `walk_forward_05` : pose source à (34, 523), 396 × 486 px → 371 × 455 px pleins ; décalage dans la cellule (315, 248)
  - `walk_forward_06` : pose source à (470, 521), 265 × 490 px → 248 × 459 px pleins ; décalage dans la cellule (367, 244)
  - `walk_forward_07` : pose source à (836, 522), 265 × 489 px → 247 × 458 px pleins ; décalage dans la cellule (368, 245)
  - `walk_forward_08` : pose source à (1216, 524), 258 × 487 px → 242 × 457 px pleins ; décalage dans la cellule (374, 247)
  - `stand_punch_light_01` : pose source à (75, 24), 375 × 479 px → 351 × 448 px pleins ; décalage dans la cellule (336, 255)
  - `stand_punch_light_02` : pose source à (557, 28), 391 × 475 px → 366 × 445 px pleins ; décalage dans la cellule (336, 258)
  - `stand_punch_light_03` : pose source à (1037, 28), 441 × 475 px → 414 × 445 px pleins ; décalage dans la cellule (336, 258)
  - `stand_punch_light_04` : pose source à (52, 527), 478 × 479 px → 446 × 448 px pleins ; décalage dans la cellule (335, 255)
  - `stand_punch_light_05` : pose source à (565, 525), 401 × 481 px → 375 × 450 px pleins ; décalage dans la cellule (336, 253)
  - `stand_punch_light_06` : pose source à (1062, 525), 394 × 481 px → 368 × 450 px pleins ; décalage dans la cellule (336, 253)
  - `stand_kick_light_01` : pose source à (94, 15), 363 × 496 px → 339 × 464 px pleins ; décalage dans la cellule (342, 239)
  - `stand_kick_light_02` : pose source à (623, 15), 259 × 496 px → 243 × 464 px pleins ; décalage dans la cellule (320, 239)
  - `stand_kick_light_03` : pose source à (1097, 15), 393 × 496 px → 367 × 464 px pleins ; décalage dans la cellule (342, 239)
  - `stand_kick_light_04` : pose source à (72, 526), 439 × 490 px → 411 × 460 px pleins ; décalage dans la cellule (306, 244)
  - `stand_kick_light_05` : pose source à (665, 525), 252 × 488 px → 235 × 457 px pleins ; décalage dans la cellule (316, 246)
  - `stand_kick_light_06` : pose source à (1054, 527), 363 × 495 px → 340 × 464 px pleins ; décalage dans la cellule (342, 240)
  - `stand_kick_heavy_01` : pose source à (26, 47), 332 × 464 px → 336 × 471 px pleins ; décalage dans la cellule (342, 233)
  - `stand_kick_heavy_02` : pose source à (398, 46), 290 × 465 px → 293 × 471 px pleins ; décalage dans la cellule (220, 232)
  - `stand_kick_heavy_03` : pose source à (752, 42), 315 × 466 px → 317 × 473 px pleins ; décalage dans la cellule (212, 231)
  - `stand_kick_heavy_04` : pose source à (1112, 49), 409 × 459 px → 415 × 465 px pleins ; décalage dans la cellule (194, 238)
  - `stand_kick_heavy_05` : pose source à (42, 563), 457 × 427 px → 461 × 430 px pleins ; décalage dans la cellule (248, 271)
  - `stand_kick_heavy_06` : pose source à (545, 540), 319 × 449 px → 324 × 455 px pleins ; décalage dans la cellule (224, 248)
  - `stand_kick_heavy_07` : pose source à (899, 540), 277 × 449 px → 281 × 455 px pleins ; décalage dans la cellule (240, 248)
  - `stand_kick_heavy_08` : pose source à (1210, 546), 311 × 443 px → 313 × 450 px pleins ; décalage dans la cellule (343, 254)
  - `special_orleans_cast_01` : pose source à (26, 15), 293 × 518 px → 353 × 625 px pleins ; décalage dans la cellule (328, 78)
  - `special_orleans_cast_02` : pose source à (376, 143), 435 × 390 px → 525 × 470 px pleins ; décalage dans la cellule (328, 233)
  - `special_orleans_cast_03` : pose source à (776, 248), 379 × 287 px → 456 × 346 px pleins ; décalage dans la cellule (316, 356)
  - `special_orleans_cast_04` : pose source à (1154, 175), 376 × 360 px → 453 × 434 px pleins ; décalage dans la cellule (334, 268)
  - `special_orleans_cast_05` : pose source à (8, 692), 531 × 312 px → 641 × 373 px pleins ; décalage dans la cellule (328, 330)
  - `special_orleans_cast_06` : pose source à (498, 663), 392 × 343 px → 472 × 412 px pleins ; décalage dans la cellule (356, 290)
  - `special_orleans_cast_07` : pose source à (868, 535), 368 × 470 px → 444 × 569 px pleins ; décalage dans la cellule (327, 136)
  - `special_orleans_cast_08` : pose source à (1226, 535), 294 × 470 px → 354 × 568 px pleins ; décalage dans la cellule (328, 136)
  - `hit_high_01` : pose source à (133, 11), 566 × 700 px → 383 × 475 px pleins ; décalage dans la cellule (325, 228)
  - `hit_high_02` : pose source à (822, 23), 576 × 688 px → 391 × 467 px pleins ; décalage dans la cellule (325, 236)
  - `hit_high_03` : pose source à (1506, 19), 551 × 692 px → 374 × 470 px pleins ; décalage dans la cellule (325, 233)
  - `block_high_01` : pose source à (151, 16), 527 × 689 px → 364 × 478 px pleins ; décalage dans la cellule (327, 227)
  - `block_high_02` : pose source à (857, 27), 545 × 678 px → 377 × 471 px pleins ; décalage dans la cellule (328, 233)
  - `block_high_03` : pose source à (1540, 25), 532 × 679 px → 367 × 469 px pleins ; décalage dans la cellule (328, 234)
  - `crouch_01` : pose source à (125, 14), 508 × 696 px → 342 × 471 px pleins ; décalage dans la cellule (340, 234)
  - `crouch_02` : pose source à (849, 110), 481 × 600 px → 324 × 404 px pleins ; décalage dans la cellule (340, 299)
  - `crouch_03` : pose source à (1575, 250), 436 × 460 px → 294 × 310 px pleins ; décalage dans la cellule (340, 393)
  - `block_low_01` : pose source à (88, 166), 683 × 484 px → 437 × 310 px pleins ; décalage dans la cellule (282, 393)
  - `block_low_02` : pose source à (799, 194), 655 × 456 px → 420 × 292 px pleins ; décalage dans la cellule (289, 411)
  - `block_low_03` : pose source à (1523, 163), 610 × 487 px → 392 × 312 px pleins ; décalage dans la cellule (300, 392)
  - `crouch_punch_01` : pose source à (39, 96), 398 × 403 px → 307 × 311 px pleins ; décalage dans la cellule (347, 393)
  - `crouch_punch_02` : pose source à (558, 106), 400 × 395 px → 307 × 304 px pleins ; décalage dans la cellule (349, 399)
  - `crouch_punch_03` : pose source à (1027, 97), 473 × 403 px → 364 × 310 px pleins ; décalage dans la cellule (347, 393)
  - `crouch_punch_04` : pose source à (31, 566), 568 × 398 px → 437 × 306 px pleins ; décalage dans la cellule (345, 397)
  - `crouch_punch_05` : pose source à (559, 573), 416 × 393 px → 321 × 302 px pleins ; décalage dans la cellule (344, 401)
  - `crouch_punch_06` : pose source à (1052, 566), 408 × 400 px → 315 × 307 px pleins ; décalage dans la cellule (346, 396)
  - `crouch_kick_01` : pose source à (45, 123), 373 × 384 px → 301 × 310 px pleins ; décalage dans la cellule (348, 393)
  - `crouch_kick_02` : pose source à (558, 139), 398 × 367 px → 321 × 296 px pleins ; décalage dans la cellule (370, 407)
  - `crouch_kick_03` : pose source à (1033, 150), 492 × 356 px → 398 × 286 px pleins ; décalage dans la cellule (373, 417)
  - `crouch_kick_04` : pose source à (25, 631), 552 × 339 px → 448 × 273 px pleins ; décalage dans la cellule (371, 430)
  - `crouch_kick_05` : pose source à (611, 620), 386 × 355 px → 311 × 286 px pleins ; décalage dans la cellule (338, 417)
  - `crouch_kick_06` : pose source à (1120, 609), 355 × 366 px → 286 × 295 px pleins ; décalage dans la cellule (349, 408)
  - `hit_crouch_01` : pose source à (98, 98), 614 × 572 px → 341 × 318 px pleins ; décalage dans la cellule (366, 385)
  - `hit_crouch_02` : pose source à (799, 75), 628 × 594 px → 350 × 330 px pleins ; décalage dans la cellule (366, 373)
  - `hit_crouch_03` : pose source à (1504, 112), 574 × 557 px → 319 × 310 px pleins ; décalage dans la cellule (365, 394)
  - `jump_01` : pose source à (140, 134), 292 × 380 px → 336 × 441 px pleins ; décalage dans la cellule (355, 263)
  - `jump_02` : pose source à (623, 22), 300 × 491 px → 345 × 567 px pleins ; décalage dans la cellule (340, 135)
  - `jump_03` : pose source à (1132, 11), 283 × 397 px → 326 × 459 px pleins ; décalage dans la cellule (380, 244)
  - `jump_04` : pose source à (172, 538), 266 × 356 px → 306 × 410 px pleins ; décalage dans la cellule (399, 292)
  - `jump_05` : pose source à (651, 524), 245 × 486 px → 284 × 561 px pleins ; décalage dans la cellule (405, 141)
  - `jump_06` : pose source à (1137, 665), 298 × 352 px → 344 × 405 px pleins ; décalage dans la cellule (345, 298)
  - `jump_punch_01` : pose source à (42, 27), 434 × 608 px → 292 × 410 px pleins ; décalage dans la cellule (475, 293)
  - `jump_punch_02` : pose source à (570, 30), 433 × 602 px → 292 × 406 px pleins ; décalage dans la cellule (485, 297)
  - `jump_punch_03` : pose source à (1094, 39), 613 × 650 px → 414 × 439 px pleins ; décalage dans la cellule (458, 264)
  - `jump_punch_04` : pose source à (1708, 29), 397 × 640 px → 267 × 433 px pleins ; décalage dans la cellule (488, 271)
  - `jump_kick_01` : pose source à (28, 79), 395 × 483 px → 335 × 411 px pleins ; décalage dans la cellule (405, 293)
  - `jump_kick_02` : pose source à (506, 71), 430 × 556 px → 366 × 474 px pleins ; décalage dans la cellule (392, 230)
  - `jump_kick_03` : pose source à (1014, 75), 692 × 477 px → 589 × 405 px pleins ; décalage dans la cellule (393, 299)
  - `jump_kick_04` : pose source à (1714, 79), 394 × 498 px → 334 × 423 px pleins ; décalage dans la cellule (406, 280)
  - `ko_01` : pose source à (69, 56), 413 × 509 px → 377 × 467 px pleins ; décalage dans la cellule (292, 236)
  - `ko_02` : pose source à (585, 108), 322 × 453 px → 294 × 416 px pleins ; décalage dans la cellule (353, 287)
  - `ko_03` : pose source à (1011, 127), 510 × 429 px → 467 × 393 px pleins ; décalage dans la cellule (177, 310)
  - `ko_04` : pose source à (27, 627), 445 × 272 px → 407 × 248 px pleins ; décalage dans la cellule (197, 455)
  - `ko_05` : pose source à (479, 740), 509 × 170 px → 465 × 153 px pleins ; décalage dans la cellule (189, 549)
  - `ko_06` : pose source à (1003, 770), 524 × 129 px → 481 × 117 px pleins ; décalage dans la cellule (208, 586)
