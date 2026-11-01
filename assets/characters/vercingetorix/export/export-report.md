# Export technique — vercingetorix

Généré par `npm run assets:export -- tools/export-character/configs/vercingetorix.json`. Ne pas modifier à la main : relancer l'outil.

## Résultat

- 111 poses. Cellules personnage 1088 × 768, pivot (544, 704) au sol entre les pieds ; cellules d'effet 1024 × 768, pivot au centre de l'effet (512, 384).
- Hauteur de garde ramenée à 520 px (= unités monde du jeu).
- Images individuelles : `assets/characters/vercingetorix/export/frames/vercingetorix_<animation>_<NN>.png` (NN = numéro de pose, à partir de 01).
- Atlas du jeu : `atlas-0.png` 4032 × 3736, `atlas-1.png` 3956 × 2848 dans `game/Assets/Resources/Fighters/vercingetorix`.
- Aperçus : `assets/characters/vercingetorix/export/previews` (planche sur fonds sombre et clair avec pivot et sol, superposition des poses).

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

- Source : `vercingetorix_idle_hd_v01.png` (1536 × 1024, sha256 `10e542eaa7ea8f9a…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0277 ; repère `rearFoot`

### walk_forward

- Source : `vercingetorix_walk_forward_hd_v01.png` (1536 × 1024, sha256 `34446ebc6e188701…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.0277 ; repère `torso`

### stand_punch_light

- Source : `vercingetorix_stand_punch_light_hd_v01.png` (1536 × 1024, sha256 `cc683ff9b639750c…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0277 ; repère `rearFoot`

### stand_kick_light

- Source : `vercingetorix_stand_kick_light_hd_v01.png` (1536 × 1024, sha256 `7b9f4e9a10b5360f…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0277 ; repère `rearFoot`

### stand_kick_heavy

- Source : `vercingetorix_stand_kick_heavy_hd_v01.png` (1774 × 887, sha256 `4b8bf64ffc389f9e…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.2530 ; repère `rearFoot`

### special_alesia_cast

- Source : `vercingetorix_special_alesia_cast_hd_v01.png` (1774 × 887, sha256 `d7e0d5daecb46cbc…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.2293 ; repère `rearFoot`

### fureur_crochet

- Source : `vercingetorix_fureur_crochet_hd_v02.png` (1536 × 1024, sha256 `25873686de7d4ac9…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0901 ; repère `rearFoot`

### fureur_massue

- Source : `vercingetorix_fureur_massue_hd_v02.png` (1774 × 887, sha256 `c3c7833c73a26d85…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.2530 ; repère `rearFoot`

### special_bouclier_cast

- Source : `vercingetorix_special_bouclier_cast_hd_v02.png` (1536 × 1024, sha256 `5ca2f5e38be9509b…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.2322 ; repère `rearFoot`

### hit_high

- Source : `vercingetorix_hit_high_hd_v01.png` (2172 × 724, sha256 `e382382fa25a6bf4…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.7670 ; repère `rearFoot`

### block_high

- Source : `vercingetorix_block_high_hd_v01.png` (2172 × 724, sha256 `d43553e54897bcab…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.7625 ; repère `rearFoot`

### crouch

- Source : `vercingetorix_crouch_hd_v01.png` (2172 × 724, sha256 `4efd70fbd036c633…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.7397 ; repère `rearFoot`

### block_low

- Source : `vercingetorix_block_low_hd_v01.png` (2172 × 724, sha256 `177fd6644e9afba9…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.7081 ; repère `rearFoot`

### crouch_punch

- Source : `vercingetorix_crouch_punch_hd_v01.png` (1536 × 1024, sha256 `c66a445c824c2005…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.8737 ; repère `rearFoot`

### crouch_kick

- Source : `vercingetorix_crouch_kick_hd_v01.png` (1536 × 1024, sha256 `796eadb115bcb256…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.9547 ; repère `rearFoot`

### hit_crouch

- Source : `vercingetorix_hit_crouch_hd_v01.png` (2172 × 724, sha256 `f0db5112404c2153…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6683 ; repère `rearFoot`

### jump

- Source : `vercingetorix_jump_hd_v01.png` (1536 × 1024, sha256 `b4e8f7128ae21d1c…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.1887 ; repère `torso`

### jump_punch

- Source : `vercingetorix_jump_punch_hd_v01.png` (2172 × 724, sha256 `4da398126fe0d715…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.7904 ; repère `torso`

### jump_kick

- Source : `vercingetorix_jump_kick_hd_v01.png` (2172 × 724, sha256 `f35a2f2170e495ea…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 1.0588 ; repère `torso`

### ko

- Source : `vercingetorix_ko_hd_v01.png` (1536 × 1024, sha256 `ae1b1d0307d88e28…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0166 ; repère `rearFoot`
  - `idle_01` : pose source à (66, 15), 418 × 507 px → 429 × 520 px pleins ; décalage dans la cellule (328, 183)
  - `idle_02` : pose source à (569, 13), 422 × 509 px → 432 × 522 px pleins ; décalage dans la cellule (329, 181)
  - `idle_03` : pose source à (1076, 10), 419 × 512 px → 429 × 525 px pleins ; décalage dans la cellule (329, 178)
  - `idle_04` : pose source à (66, 529), 419 × 484 px → 429 × 496 px pleins ; décalage dans la cellule (329, 207)
  - `idle_05` : pose source à (568, 530), 422 × 483 px → 432 × 494 px pleins ; décalage dans la cellule (329, 208)
  - `idle_06` : pose source à (1077, 529), 417 × 484 px → 428 × 495 px pleins ; décalage dans la cellule (328, 207)
  - `walk_forward_01` : pose source à (27, 16), 372 × 494 px → 381 × 505 px pleins ; décalage dans la cellule (351, 197)
  - `walk_forward_02` : pose source à (427, 41), 343 × 467 px → 350 × 478 px pleins ; décalage dans la cellule (354, 225)
  - `walk_forward_03` : pose source à (849, 15), 255 × 495 px → 261 × 507 px pleins ; décalage dans la cellule (436, 196)
  - `walk_forward_04` : pose source à (1169, 13), 326 × 496 px → 334 × 507 px pleins ; décalage dans la cellule (376, 195)
  - `walk_forward_05` : pose source à (27, 522), 373 × 482 px → 383 × 494 px pleins ; décalage dans la cellule (351, 210)
  - `walk_forward_06` : pose source à (429, 544), 368 × 455 px → 375 × 466 px pleins ; décalage dans la cellule (343, 237)
  - `walk_forward_07` : pose source à (858, 520), 261 × 484 px → 266 × 494 px pleins ; décalage dans la cellule (430, 208)
  - `walk_forward_08` : pose source à (1167, 520), 334 × 479 px → 342 × 490 px pleins ; décalage dans la cellule (369, 213)
  - `stand_punch_light_01` : pose source à (45, 13), 430 × 505 px → 440 × 517 px pleins ; décalage dans la cellule (323, 185)
  - `stand_punch_light_02` : pose source à (551, 21), 432 × 497 px → 442 × 507 px pleins ; décalage dans la cellule (323, 193)
  - `stand_punch_light_03` : pose source à (1028, 18), 485 × 500 px → 495 × 512 px pleins ; décalage dans la cellule (323, 190)
  - `stand_punch_light_04` : pose source à (44, 521), 550 × 489 px → 562 × 499 px pleins ; décalage dans la cellule (323, 202)
  - `stand_punch_light_05` : pose source à (568, 525), 431 × 485 px → 441 × 494 px pleins ; décalage dans la cellule (323, 207)
  - `stand_punch_light_06` : pose source à (1063, 521), 436 × 489 px → 447 × 500 px pleins ; décalage dans la cellule (323, 202)
  - `stand_kick_light_01` : pose source à (41, 11), 432 × 499 px → 442 × 510 px pleins ; décalage dans la cellule (322, 193)
  - `stand_kick_light_02` : pose source à (620, 11), 311 × 497 px → 317 × 510 px pleins ; décalage dans la cellule (321, 194)
  - `stand_kick_light_03` : pose source à (1094, 11), 417 × 498 px → 428 × 510 px pleins ; décalage dans la cellule (322, 193)
  - `stand_kick_light_04` : pose source à (46, 522), 547 × 486 px → 562 × 497 px pleins ; décalage dans la cellule (262, 206)
  - `stand_kick_light_05` : pose source à (650, 513), 318 × 495 px → 324 × 506 px pleins ; décalage dans la cellule (303, 196)
  - `stand_kick_light_06` : pose source à (1066, 516), 428 × 492 px → 440 × 503 px pleins ; décalage dans la cellule (323, 199)
  - `stand_kick_heavy_01` : pose source à (30, 10), 353 × 417 px → 440 × 520 px pleins ; décalage dans la cellule (323, 182)
  - `stand_kick_heavy_02` : pose source à (506, 10), 321 × 417 px → 401 × 520 px pleins ; décalage dans la cellule (324, 182)
  - `stand_kick_heavy_03` : pose source à (920, 9), 286 × 429 px → 356 × 536 px pleins ; décalage dans la cellule (238, 167)
  - `stand_kick_heavy_04` : pose source à (1320, 10), 424 × 425 px → 529 × 531 px pleins ; décalage dans la cellule (184, 172)
  - `stand_kick_heavy_05` : pose source à (30, 448), 525 × 424 px → 653 × 529 px pleins ; décalage dans la cellule (174, 173)
  - `stand_kick_heavy_06` : pose source à (594, 449), 283 × 423 px → 349 × 528 px pleins ; décalage dans la cellule (250, 174)
  - `stand_kick_heavy_07` : pose source à (1015, 447), 282 × 425 px → 350 × 531 px pleins ; décalage dans la cellule (302, 171)
  - `stand_kick_heavy_08` : pose source à (1388, 446), 356 × 425 px → 443 × 529 px pleins ; décalage dans la cellule (323, 172)
  - `special_alesia_cast_01` : pose source à (36, 15), 373 × 425 px → 457 × 519 px pleins ; décalage dans la cellule (315, 183)
  - `special_alesia_cast_02` : pose source à (457, 80), 383 × 357 px → 468 × 435 px pleins ; décalage dans la cellule (315, 266)
  - `special_alesia_cast_03` : pose source à (855, 64), 431 × 371 px → 527 × 453 px pleins ; décalage dans la cellule (315, 249)
  - `special_alesia_cast_04` : pose source à (1301, 60), 472 × 375 px → 577 × 456 px pleins ; décalage dans la cellule (311, 244)
  - `special_alesia_cast_05` : pose source à (12, 488), 463 × 373 px → 566 × 456 px pleins ; décalage dans la cellule (312, 245)
  - `special_alesia_cast_06` : pose source à (475, 479), 446 × 388 px → 545 × 473 px pleins ; décalage dans la cellule (368, 227)
  - `special_alesia_cast_07` : pose source à (931, 454), 404 × 418 px → 497 × 511 px pleins ; décalage dans la cellule (316, 191)
  - `special_alesia_cast_08` : pose source à (1375, 454), 375 × 419 px → 458 × 512 px pleins ; décalage dans la cellule (315, 190)
  - `fureur_crochet_01` : pose source à (22, 14), 486 × 497 px → 527 × 540 px pleins ; décalage dans la cellule (298, 162)
  - `fureur_crochet_02` : pose source à (522, 21), 496 × 491 px → 541 × 531 px pleins ; décalage dans la cellule (299, 169)
  - `fureur_crochet_03` : pose source à (1058, 16), 471 × 497 px → 511 × 540 px pleins ; décalage dans la cellule (298, 162)
  - `fureur_crochet_04` : pose source à (15, 530), 575 × 471 px → 624 × 510 px pleins ; décalage dans la cellule (298, 192)
  - `fureur_crochet_05` : pose source à (543, 523), 521 × 478 px → 566 × 518 px pleins ; décalage dans la cellule (298, 184)
  - `fureur_crochet_06` : pose source à (1070, 525), 451 × 480 px → 490 × 519 px pleins ; décalage dans la cellule (298, 182)
  - `fureur_massue_01` : pose source à (55, 46), 371 × 416 px → 464 × 519 px pleins ; décalage dans la cellule (312, 183)
  - `fureur_massue_02` : pose source à (497, 72), 380 × 390 px → 474 × 487 px pleins ; décalage dans la cellule (311, 215)
  - `fureur_massue_03` : pose source à (958, 11), 362 × 451 px → 452 × 563 px pleins ; décalage dans la cellule (311, 139)
  - `fureur_massue_04` : pose source à (1370, 94), 365 × 368 px → 454 × 459 px pleins ; décalage dans la cellule (311, 243)
  - `fureur_massue_05` : pose source à (27, 500), 462 × 368 px → 576 × 456 px pleins ; décalage dans la cellule (311, 244)
  - `fureur_massue_06` : pose source à (489, 535), 419 × 333 px → 522 × 413 px pleins ; décalage dans la cellule (311, 288)
  - `fureur_massue_07` : pose source à (935, 474), 358 × 398 px → 448 × 496 px pleins ; décalage dans la cellule (312, 206)
  - `fureur_massue_08` : pose source à (1376, 477), 359 × 393 px → 448 × 489 px pleins ; décalage dans la cellule (311, 212)
  - `special_bouclier_cast_01` : pose source à (32, 87), 357 × 425 px → 437 × 520 px pleins ; décalage dans la cellule (324, 181)
  - `special_bouclier_cast_02` : pose source à (431, 181), 345 × 331 px → 422 × 402 px pleins ; décalage dans la cellule (304, 296)
  - `special_bouclier_cast_03` : pose source à (795, 63), 340 × 437 px → 418 × 534 px pleins ; décalage dans la cellule (323, 127)
  - `special_bouclier_cast_04` : pose source à (1223, 18), 260 × 397 px → 315 × 486 px pleins ; décalage dans la cellule (376, 96)
  - `special_bouclier_cast_05` : pose source à (24, 549), 352 × 393 px → 432 × 483 px pleins ; décalage dans la cellule (320, 131)
  - `special_bouclier_cast_06` : pose source à (394, 655), 409 × 328 px → 502 × 399 px pleins ; décalage dans la cellule (324, 301)
  - `special_bouclier_cast_07` : pose source à (811, 620), 345 × 374 px → 422 × 456 px pleins ; décalage dans la cellule (324, 244)
  - `special_bouclier_cast_08` : pose source à (1182, 578), 339 × 417 px → 415 × 510 px pleins ; décalage dans la cellule (324, 191)
  - `hit_high_01` : pose source à (108, 21), 602 × 680 px → 461 × 519 px pleins ; décalage dans la cellule (311, 183)
  - `hit_high_02` : pose source à (790, 22), 642 × 679 px → 490 × 518 px pleins ; décalage dans la cellule (295, 184)
  - `hit_high_03` : pose source à (1512, 20), 605 × 681 px → 464 × 519 px pleins ; décalage dans la cellule (312, 183)
  - `block_high_01` : pose source à (104, 12), 593 × 686 px → 451 × 521 px pleins ; décalage dans la cellule (320, 182)
  - `block_high_02` : pose source à (820, 16), 601 × 682 px → 457 × 518 px pleins ; décalage dans la cellule (320, 185)
  - `block_high_03` : pose source à (1528, 11), 588 × 685 px → 447 × 519 px pleins ; décalage dans la cellule (320, 183)
  - `crouch_01` : pose source à (31, 8), 666 × 705 px → 492 × 519 px pleins ; décalage dans la cellule (297, 183)
  - `crouch_02` : pose source à (808, 68), 660 × 641 px → 487 × 472 px pleins ; décalage dans la cellule (297, 230)
  - `crouch_03` : pose source à (1563, 192), 587 × 517 px → 432 × 380 px pleins ; décalage dans la cellule (297, 323)
  - `block_low_01` : pose source à (16, 134), 706 × 540 px → 499 × 381 px pleins ; décalage dans la cellule (294, 322)
  - `block_low_02` : pose source à (769, 133), 685 × 544 px → 484 × 383 px pleins ; décalage dans la cellule (294, 320)
  - `block_low_03` : pose source à (1490, 123), 671 × 554 px → 474 × 389 px pleins ; décalage dans la cellule (294, 313)
  - `crouch_punch_01` : pose source à (61, 61), 422 × 438 px → 368 × 382 px pleins ; décalage dans la cellule (359, 321)
  - `crouch_punch_02` : pose source à (547, 69), 420 × 430 px → 365 × 374 px pleins ; décalage dans la cellule (359, 329)
  - `crouch_punch_03` : pose source à (1020, 81), 452 × 418 px → 394 × 363 px pleins ; décalage dans la cellule (351, 340)
  - `crouch_punch_04` : pose source à (51, 549), 526 × 419 px → 458 × 363 px pleins ; décalage dans la cellule (359, 339)
  - `crouch_punch_05` : pose source à (581, 548), 451 × 417 px → 393 × 362 px pleins ; décalage dans la cellule (359, 341)
  - `crouch_punch_06` : pose source à (1081, 545), 422 × 424 px → 368 × 368 px pleins ; décalage dans la cellule (359, 335)
  - `crouch_kick_01` : pose source à (50, 93), 395 × 401 px → 374 × 381 px pleins ; décalage dans la cellule (356, 322)
  - `crouch_kick_02` : pose source à (548, 124), 390 × 373 px → 370 × 354 px pleins ; décalage dans la cellule (305, 349)
  - `crouch_kick_03` : pose source à (1017, 126), 489 × 374 px → 465 × 356 px pleins ; décalage dans la cellule (356, 347)
  - `crouch_kick_04` : pose source à (35, 591), 619 × 368 px → 589 × 349 px pleins ; décalage dans la cellule (356, 354)
  - `crouch_kick_05` : pose source à (670, 592), 360 × 367 px → 342 × 349 px pleins ; décalage dans la cellule (356, 355)
  - `crouch_kick_06` : pose source à (1103, 569), 399 × 392 px → 380 × 371 px pleins ; décalage dans la cellule (357, 331)
  - `hit_crouch_01` : pose source à (41, 35), 651 × 655 px → 435 × 435 px pleins ; décalage dans la cellule (334, 267)
  - `hit_crouch_02` : pose source à (753, 73), 714 × 615 px → 474 × 409 px pleins ; décalage dans la cellule (321, 294)
  - `hit_crouch_03` : pose source à (1489, 117), 644 × 572 px → 430 × 380 px pleins ; décalage dans la cellule (334, 323)
  - `jump_01` : pose source à (56, 95), 382 × 412 px → 452 × 486 px pleins ; décalage dans la cellule (330, 215)
  - `jump_02` : pose source à (579, 32), 397 × 477 px → 469 × 564 px pleins ; décalage dans la cellule (300, 138)
  - `jump_03` : pose source à (1149, 14), 326 × 422 px → 387 × 498 px pleins ; décalage dans la cellule (384, 203)
  - `jump_04` : pose source à (108, 528), 359 × 377 px → 427 × 445 px pleins ; décalage dans la cellule (370, 257)
  - `jump_05` : pose source à (633, 531), 331 × 479 px → 392 × 565 px pleins ; décalage dans la cellule (394, 136)
  - `jump_06` : pose source à (1110, 611), 369 × 399 px → 436 × 471 px pleins ; décalage dans la cellule (343, 231)
  - `jump_punch_01` : pose source à (52, 84), 464 × 567 px → 366 × 446 px pleins ; décalage dans la cellule (386, 256)
  - `jump_punch_02` : pose source à (595, 18), 448 × 648 px → 352 × 510 px pleins ; décalage dans la cellule (403, 193)
  - `jump_punch_03` : pose source à (1113, 141), 476 × 525 px → 374 × 412 px pleins ; décalage dans la cellule (375, 290)
  - `jump_punch_04` : pose source à (1672, 85), 465 × 569 px → 367 × 448 px pleins ; décalage dans la cellule (384, 255)
  - `jump_kick_01` : pose source à (37, 115), 447 × 423 px → 472 × 446 px pleins ; décalage dans la cellule (364, 257)
  - `jump_kick_02` : pose source à (568, 104), 430 × 498 px → 454 × 526 px pleins ; décalage dans la cellule (365, 178)
  - `jump_kick_03` : pose source à (1083, 102), 629 × 434 px → 664 × 457 px pleins ; décalage dans la cellule (391, 245)
  - `jump_kick_04` : pose source à (1698, 113), 427 × 460 px → 449 × 483 px pleins ; décalage dans la cellule (388, 218)
  - `ko_01` : pose source à (25, 36), 496 × 511 px → 504 × 517 px pleins ; décalage dans la cellule (267, 185)
  - `ko_02` : pose source à (523, 77), 405 × 470 px → 411 × 476 px pleins ; décalage dans la cellule (225, 226)
  - `ko_03` : pose source à (953, 126), 577 × 421 px → 585 × 427 px pleins ; décalage dans la cellule (138, 276)
  - `ko_04` : pose source à (13, 642), 497 × 327 px → 502 × 331 px pleins ; décalage dans la cellule (145, 373)
  - `ko_05` : pose source à (511, 769), 501 × 169 px → 506 × 171 px pleins ; décalage dans la cellule (69, 533)
  - `ko_06` : pose source à (1016, 775), 512 × 151 px → 517 × 151 px pleins ; décalage dans la cellule (132, 551)
