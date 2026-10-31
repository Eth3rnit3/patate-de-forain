# Export technique — saint-louis

Généré par `npm run assets:export -- tools/export-character/configs/saint-louis.json`. Ne pas modifier à la main : relancer l'outil.

## Résultat

- 97 poses. Cellules personnage 1024 × 768, pivot (512, 704) au sol entre les pieds ; cellules d'effet 1024 × 768, pivot au centre de l'effet (512, 384).
- Hauteur de garde ramenée à 500 px (= unités monde du jeu).
- Images individuelles : `assets/characters/saint-louis/export/frames/saint_louis_<animation>_<NN>.png` (NN = numéro de pose, à partir de 01).
- Atlas du jeu : `atlas-0.png` 4056 × 3856, `atlas-1.png` 3872 × 476 dans `game/Assets/Resources/Fighters/saint-louis`.
- Aperçus : `assets/characters/saint-louis/export/previews` (planche sur fonds sombre et clair avec pivot et sol, superposition des poses).

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

- Source : `saint_louis_idle_hd_v01.png` (1536 × 1024, sha256 `a7245f798493a8ba…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0121 ; repère `rearFoot`

### walk_forward

- Source : `saint_louis_walk_forward_hd_v01.png` (1536 × 1024, sha256 `ac1350b1c6a058ba…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.0121 ; repère `torso`

### stand_punch_light

- Source : `saint_louis_stand_punch_light_hd_v01.png` (1536 × 1024, sha256 `894a596edceb6906…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0121 ; repère `rearFoot`

### stand_kick_light

- Source : `saint_louis_stand_kick_light_hd_v01.png` (1536 × 1024, sha256 `c5f0e3c8541a3748…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0121 ; repère `rearFoot`

### stand_kick_heavy

- Source : `saint_louis_stand_kick_heavy_hd_v01.png` (1774 × 887, sha256 `932fcd0d511293a8…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.1364 ; repère `rearFoot`

### special_royal_lily_cast

- Source : `saint_louis_special_royal_lily_cast_hd_v01.png` (1774 × 887, sha256 `8bd71bf3c3bdb7bd…`)
- Découpage : {"type":"poses","rows":2} → 8 poses ; échelle 1.1876 ; repère `rearFoot`

### hit_high

- Source : `saint_louis_hit_high_hd_v01.png` (1536 × 1024, sha256 `c4fb28d733cfc659…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.6173 ; repère `rearFoot`

### block_high

- Source : `saint_louis_block_high_hd_v01.png` (1536 × 1024, sha256 `568a58299f204ada…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.5995 ; repère `rearFoot`

### crouch

- Source : `saint_louis_crouch_hd_v01.png` (2172 × 724, sha256 `587b27ed4ed89789…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.7194 ; repère `rearFoot`

### block_low

- Source : `saint_louis_block_low_hd_v01.png` (2172 × 724, sha256 `b90c95ac6f3c0e28…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.5789 ; repère `rearFoot`

### crouch_punch

- Source : `saint_louis_crouch_punch_hd_v01.png` (1536 × 1024, sha256 `2eda0b0b8926abf7…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.8188 ; repère `rearFoot`

### crouch_kick

- Source : `saint_louis_crouch_kick_hd_v01.png` (1536 × 1024, sha256 `3fdd5c51b51aa0b3…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 0.8188 ; repère `rearFoot`

### hit_crouch

- Source : `saint_louis_hit_crouch_hd_v01.png` (1981 × 793, sha256 `1aafbabe60cdf2f5…`)
- Découpage : {"type":"poses","rows":1} → 3 poses ; échelle 0.5600 ; repère `rearFoot`

### jump

- Source : `saint_louis_jump_hd_v01.png` (1536 × 1024, sha256 `dbf373ea468095dc…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.1308 ; repère `torso`

### jump_punch

- Source : `saint_louis_jump_punch_hd_v01.png` (2172 × 724, sha256 `8ad7897ad8b3c91b…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.6409 ; repère `torso`

### jump_kick

- Source : `saint_louis_jump_kick_hd_v01.png` (2172 × 724, sha256 `972b0c5052c1131e…`)
- Découpage : {"type":"poses","rows":1} → 4 poses ; échelle 0.7301 ; repère `torso`

### ko

- Source : `saint_louis_ko_hd_v01.png` (1536 × 1024, sha256 `c373f9ae6c3daa71…`)
- Découpage : {"type":"poses","rows":2} → 6 poses ; échelle 1.0094 ; repère `rearFoot`

### royal_lily_projectile

- Source : `saint_louis_special_royal_lily_projectile_hd_v01.png` (2172 × 724, sha256 `826798e79ee70d06…`)
- Découpage : {"type":"grid","rows":1,"columns":4} → 4 poses ; échelle 0.4500 ; repère `head`

### royal_lily_impact

- Source : `saint_louis_special_royal_lily_impact_hd_v02.png` (1303 × 1207, sha256 `a326ff8827fa1c24…`)
- Découpage : {"type":"grid","rows":2,"columns":2} → 4 poses ; échelle 0.4500 ; repère `centroid`
  - `idle_01` : pose source à (83, 15), 382 × 496 px → 385 × 499 px pleins ; décalage dans la cellule (318, 203)
  - `idle_02` : pose source à (576, 15), 386 × 496 px → 391 × 501 px pleins ; décalage dans la cellule (319, 203)
  - `idle_03` : pose source à (1075, 10), 382 × 501 px → 384 × 507 px pleins ; décalage dans la cellule (318, 198)
  - `idle_04` : pose source à (84, 519), 382 × 495 px → 386 × 501 px pleins ; décalage dans la cellule (318, 204)
  - `idle_05` : pose source à (574, 523), 389 × 491 px → 392 × 496 px pleins ; décalage dans la cellule (318, 208)
  - `idle_06` : pose source à (1073, 517), 383 × 497 px → 386 × 502 px pleins ; décalage dans la cellule (318, 202)
  - `walk_forward_01` : pose source à (24, 20), 393 × 487 px → 396 × 490 px pleins ; décalage dans la cellule (312, 213)
  - `walk_forward_02` : pose source à (436, 22), 303 × 486 px → 304 × 490 px pleins ; décalage dans la cellule (349, 213)
  - `walk_forward_03` : pose source à (806, 22), 291 × 485 px → 293 × 489 px pleins ; décalage dans la cellule (343, 215)
  - `walk_forward_04` : pose source à (1190, 23), 291 × 483 px → 293 × 487 px pleins ; décalage dans la cellule (336, 217)
  - `walk_forward_05` : pose source à (36, 516), 390 × 480 px → 392 × 483 px pleins ; décalage dans la cellule (323, 219)
  - `walk_forward_06` : pose source à (442, 520), 304 × 474 px → 306 × 477 px pleins ; décalage dans la cellule (345, 225)
  - `walk_forward_07` : pose source à (822, 518), 280 × 482 px → 281 × 486 px pleins ; décalage dans la cellule (354, 217)
  - `walk_forward_08` : pose source à (1186, 518), 300 × 480 px → 302 × 484 px pleins ; décalage dans la cellule (338, 219)
  - `stand_punch_light_01` : pose source à (45, 12), 383 × 500 px → 387 × 505 px pleins ; décalage dans la cellule (319, 198)
  - `stand_punch_light_02` : pose source à (532, 16), 414 × 496 px → 417 × 501 px pleins ; décalage dans la cellule (318, 202)
  - `stand_punch_light_03` : pose source à (1024, 14), 457 × 498 px → 461 × 504 px pleins ; décalage dans la cellule (317, 200)
  - `stand_punch_light_04` : pose source à (20, 521), 499 × 490 px → 503 × 494 px pleins ; décalage dans la cellule (318, 209)
  - `stand_punch_light_05` : pose source à (537, 521), 424 × 490 px → 428 × 494 px pleins ; décalage dans la cellule (318, 209)
  - `stand_punch_light_06` : pose source à (1068, 520), 391 × 490 px → 394 × 494 px pleins ; décalage dans la cellule (318, 209)
  - `stand_kick_light_01` : pose source à (76, 18), 369 × 488 px → 371 × 493 px pleins ; décalage dans la cellule (326, 211)
  - `stand_kick_light_02` : pose source à (623, 16), 282 × 492 px → 282 × 494 px pleins ; décalage dans la cellule (300, 209)
  - `stand_kick_light_03` : pose source à (1097, 18), 408 × 489 px → 412 × 493 px pleins ; décalage dans la cellule (307, 211)
  - `stand_kick_light_04` : pose source à (73, 520), 472 × 486 px → 476 × 490 px pleins ; décalage dans la cellule (286, 213)
  - `stand_kick_light_05` : pose source à (667, 520), 249 × 486 px → 249 × 490 px pleins ; décalage dans la cellule (313, 213)
  - `stand_kick_light_06` : pose source à (1104, 522), 346 × 484 px → 349 × 488 px pleins ; décalage dans la cellule (325, 215)
  - `stand_kick_heavy_01` : pose source à (72, 8), 296 × 440 px → 334 × 499 px pleins ; décalage dans la cellule (344, 204)
  - `stand_kick_heavy_02` : pose source à (484, 8), 309 × 439 px → 348 × 498 px pleins ; décalage dans la cellule (344, 206)
  - `stand_kick_heavy_03` : pose source à (951, 9), 241 × 439 px → 271 × 498 px pleins ; décalage dans la cellule (308, 205)
  - `stand_kick_heavy_04` : pose source à (1321, 12), 419 × 436 px → 473 × 493 px pleins ; décalage dans la cellule (265, 209)
  - `stand_kick_heavy_05` : pose source à (50, 457), 479 × 415 px → 540 × 470 px pleins ; décalage dans la cellule (286, 233)
  - `stand_kick_heavy_06` : pose source à (618, 453), 219 × 416 px → 248 × 471 px pleins ; décalage dans la cellule (324, 232)
  - `stand_kick_heavy_07` : pose source à (974, 452), 352 × 418 px → 398 × 473 px pleins ; décalage dans la cellule (340, 230)
  - `stand_kick_heavy_08` : pose source à (1407, 448), 285 × 424 px → 322 × 482 px pleins ; décalage dans la cellule (344, 223)
  - `special_royal_lily_cast_01` : pose source à (41, 12), 344 × 423 px → 409 × 500 px pleins ; décalage dans la cellule (308, 203)
  - `special_royal_lily_cast_02` : pose source à (467, 45), 358 × 391 px → 422 × 462 px pleins ; décalage dans la cellule (307, 241)
  - `special_royal_lily_cast_03` : pose source à (906, 28), 380 × 408 px → 448 × 484 px pleins ; décalage dans la cellule (307, 220)
  - `special_royal_lily_cast_04` : pose source à (1365, 22), 375 × 414 px → 442 × 489 px pleins ; décalage dans la cellule (307, 213)
  - `special_royal_lily_cast_05` : pose source à (24, 455), 453 × 410 px → 536 × 485 px pleins ; décalage dans la cellule (307, 218)
  - `special_royal_lily_cast_06` : pose source à (460, 456), 446 × 410 px → 529 × 485 px pleins ; décalage dans la cellule (308, 218)
  - `special_royal_lily_cast_07` : pose source à (922, 455), 380 × 412 px → 449 × 486 px pleins ; décalage dans la cellule (307, 216)
  - `special_royal_lily_cast_08` : pose source à (1370, 445), 346 × 423 px → 409 × 499 px pleins ; décalage dans la cellule (307, 204)
  - `hit_high_01` : pose source à (28, 81), 509 × 823 px → 313 × 506 px pleins ; décalage dans la cellule (363, 197)
  - `hit_high_02` : pose source à (499, 86), 545 × 818 px → 334 × 504 px pleins ; décalage dans la cellule (330, 200)
  - `hit_high_03` : pose source à (1046, 91), 487 × 812 px → 299 × 500 px pleins ; décalage dans la cellule (362, 204)
  - `block_high_01` : pose source à (15, 64), 522 × 843 px → 312 × 503 px pleins ; décalage dans la cellule (368, 200)
  - `block_high_02` : pose source à (542, 115), 502 × 791 px → 300 × 472 px pleins ; décalage dans la cellule (367, 231)
  - `block_high_03` : pose source à (1054, 71), 482 × 836 px → 289 × 499 px pleins ; décalage dans la cellule (368, 204)
  - `crouch_01` : pose source à (179, 12), 556 × 696 px → 398 × 500 px pleins ; décalage dans la cellule (312, 203)
  - `crouch_02` : pose source à (825, 115), 545 × 593 px → 391 × 425 px pleins ; décalage dans la cellule (312, 278)
  - `crouch_03` : pose source à (1510, 254), 483 × 454 px → 345 × 326 px pleins ; décalage dans la cellule (312, 377)
  - `block_low_01` : pose source à (134, 112), 545 × 565 px → 314 × 326 px pleins ; décalage dans la cellule (344, 378)
  - `block_low_02` : pose source à (814, 110), 543 × 567 px → 313 × 328 px pleins ; décalage dans la cellule (345, 376)
  - `block_low_03` : pose source à (1493, 118), 548 × 559 px → 317 × 323 px pleins ; décalage dans la cellule (344, 380)
  - `crouch_punch_01` : pose source à (40, 96), 398 × 400 px → 326 × 326 px pleins ; décalage dans la cellule (349, 377)
  - `crouch_punch_02` : pose source à (570, 115), 404 × 381 px → 330 × 310 px pleins ; décalage dans la cellule (349, 393)
  - `crouch_punch_03` : pose source à (1069, 108), 450 × 388 px → 366 × 316 px pleins ; décalage dans la cellule (348, 387)
  - `crouch_punch_04` : pose source à (37, 589), 515 × 382 px → 420 × 311 px pleins ; décalage dans la cellule (347, 393)
  - `crouch_punch_05` : pose source à (591, 591), 420 × 381 px → 344 × 309 px pleins ; décalage dans la cellule (349, 394)
  - `crouch_punch_06` : pose source à (1104, 586), 409 × 384 px → 333 × 312 px pleins ; décalage dans la cellule (348, 391)
  - `crouch_kick_01` : pose source à (54, 83), 376 × 401 px → 306 × 326 px pleins ; décalage dans la cellule (351, 377)
  - `crouch_kick_02` : pose source à (553, 149), 384 × 335 px → 311 × 272 px pleins ; décalage dans la cellule (362, 431)
  - `crouch_kick_03` : pose source à (1049, 160), 484 × 323 px → 394 × 263 px pleins ; décalage dans la cellule (349, 441)
  - `crouch_kick_04` : pose source à (17, 612), 567 × 340 px → 462 × 276 px pleins ; décalage dans la cellule (347, 427)
  - `crouch_kick_05` : pose source à (600, 603), 377 × 349 px → 307 × 284 px pleins ; décalage dans la cellule (346, 419)
  - `crouch_kick_06` : pose source à (1113, 578), 376 × 380 px → 307 × 309 px pleins ; décalage dans la cellule (351, 394)
  - `hit_crouch_01` : pose source à (41, 114), 702 × 627 px → 392 × 350 px pleins ; décalage dans la cellule (318, 354)
  - `hit_crouch_02` : pose source à (745, 126), 620 × 615 px → 345 × 342 px pleins ; décalage dans la cellule (336, 361)
  - `hit_crouch_03` : pose source à (1412, 157), 546 × 584 px → 304 × 325 px pleins ; décalage dans la cellule (359, 378)
  - `jump_01` : pose source à (107, 175), 330 × 382 px → 370 × 430 px pleins ; décalage dans la cellule (359, 273)
  - `jump_02` : pose source à (622, 4), 300 × 573 px → 336 × 646 px pleins ; décalage dans la cellule (344, 56)
  - `jump_03` : pose source à (1115, 74), 264 × 381 px → 298 × 430 px pleins ; décalage dans la cellule (384, 274)
  - `jump_04` : pose source à (144, 599), 244 × 299 px → 276 × 335 px pleins ; décalage dans la cellule (400, 367)
  - `jump_05` : pose source à (594, 583), 378 × 437 px → 424 × 491 px pleins ; décalage dans la cellule (347, 211)
  - `jump_06` : pose source à (1105, 632), 356 × 378 px → 401 × 424 px pleins ; décalage dans la cellule (311, 278)
  - `jump_punch_01` : pose source à (13, 50), 441 × 528 px → 282 × 335 px pleins ; décalage dans la cellule (388, 367)
  - `jump_punch_02` : pose source à (537, 80), 473 × 516 px → 301 × 330 px pleins ; décalage dans la cellule (382, 374)
  - `jump_punch_03` : pose source à (1028, 111), 699 × 479 px → 448 × 304 px pleins ; décalage dans la cellule (329, 398)
  - `jump_punch_04` : pose source à (1738, 63), 420 × 537 px → 268 × 342 px pleins ; décalage dans la cellule (417, 361)
  - `jump_kick_01` : pose source à (19, 58), 431 × 462 px → 314 × 335 px pleins ; décalage dans la cellule (398, 368)
  - `jump_kick_02` : pose source à (520, 59), 394 × 527 px → 286 × 385 px pleins ; décalage dans la cellule (429, 320)
  - `jump_kick_03` : pose source à (1011, 113), 702 × 498 px → 513 × 362 px pleins ; décalage dans la cellule (410, 341)
  - `jump_kick_04` : pose source à (1750, 58), 392 × 501 px → 284 × 364 px pleins ; décalage dans la cellule (421, 339)
  - `ko_01` : pose source à (61, 46), 428 × 499 px → 431 × 504 px pleins ; décalage dans la cellule (291, 200)
  - `ko_02` : pose source à (539, 121), 404 × 424 px → 406 × 426 px pleins ; décalage dans la cellule (219, 276)
  - `ko_03` : pose source à (1011, 153), 507 × 351 px → 511 × 351 px pleins ; décalage dans la cellule (130, 365)
  - `ko_04` : pose source à (22, 603), 546 × 363 px → 548 × 365 px pleins ; décalage dans la cellule (276, 352)
  - `ko_05` : pose source à (540, 808), 464 × 167 px → 464 × 167 px pleins ; décalage dans la cellule (186, 556)
  - `ko_06` : pose source à (1023, 807), 495 × 166 px → 498 × 166 px pleins ; décalage dans la cellule (140, 557)
  - `royal_lily_projectile_01` : pose source à (0, 0), 543 × 724 px → 229 × 147 px pleins ; décalage dans la cellule (334, 226)
  - `royal_lily_projectile_02` : pose source à (543, 0), 543 × 724 px → 210 × 147 px pleins ; décalage dans la cellule (345, 228)
  - `royal_lily_projectile_03` : pose source à (1086, 0), 543 × 724 px → 228 × 146 px pleins ; décalage dans la cellule (335, 227)
  - `royal_lily_projectile_04` : pose source à (1629, 0), 543 × 724 px → 212 × 146 px pleins ; décalage dans la cellule (342, 227)
  - `royal_lily_impact_01` : pose source à (0, 0), 651 × 603 px → 155 × 158 px pleins ; décalage dans la cellule (362, 241)
  - `royal_lily_impact_02` : pose source à (651, 0), 651 × 603 px → 242 × 247 px pleins ; décalage dans la cellule (373, 240)
  - `royal_lily_impact_03` : pose source à (0, 603), 651 × 603 px → 238 × 219 px pleins ; décalage dans la cellule (355, 254)
  - `royal_lily_impact_04` : pose source à (651, 603), 651 × 603 px → 225 × 199 px pleins ; décalage dans la cellule (364, 254)
