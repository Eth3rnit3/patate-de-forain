# État du prototype — 1ᵉʳ novembre 2026

## Ce qui fonctionne en jeu

Six combattants jouables, **Saint Louis**, **Napoléon**, **De Gaulle**, **Vercingétorix**, **Jeanne d'Arc** et **Cyrano**, dans Unity 6000.4.7f1 (scène `Assets/Scenes/Combat.unity`), au clavier ou à la manette, avec le parcours complet : titre, choix du mode (versus local, solo contre l'ordinateur, en ligne), sélection des personnages et du décor, combat, résultat.

- **Déplacements** : marche avant et arrière, orientation automatique vers l'adversaire, collision corporelle (pousser l'adversaire en avançant), limites de l'arène, écart maximal entre combattants.
- **Saut et accroupissement** :
  - haut (Z, ↑, stick haut) : saut vertical ; haut-avant et haut-arrière : sauts en avant et en arrière. Courte préparation (4 ticks), trajectoire fixe une fois en l'air (420 unités de haut, 41 ticks en l'air), réception de 4 ticks. On peut sauter par-dessus l'adversaire, qu'on frappe alors dans le dos ;
  - bas (S, ↓, stick bas) : accroupi, sans avancer ni reculer.
- **Coups** :
  - poing ;
  - pied rapide ;
  - pied puissant ;
  - accroupi : poing (coup moyen) et balayette au pied rapide ou puissant (coup bas) ;
  - en l'air : poing sauté et pied sauté (coups hauts), un seul par saut ;
  - Poing de Saint Louis : touche dédiée ou quart de cercle avant + poing. Il projette une fleur de lys émise au poing à l'entrée de la 5ᵉ pose, à raison d'un seul projectile à la fois, qui produit un impact doré au contact ;
  - Salve impériale de Napoléon : mêmes commandes. Napoléon pointe l'adversaire et un boulet spectral part du doigt à l'entrée de la 5ᵉ pose, à hauteur de poitrine, un seul à la fois, avec un impact bleu au contact ;
  - Appel du 18 Juin de De Gaulle : mêmes commandes. De Gaulle lève les bras en V et lance son appel : une onde sonore bleu-blanc-rouge part à l'entrée de la 5ᵉ pose, à hauteur de tête et d'épaules (elle passe au-dessus de Saint Louis et de Napoléon accroupis, pas de De Gaulle accroupi, plus grand), une seule à la fois, avec un impact tricolore au contact ;
  - Charge d'Alésia de Vercingétorix : mêmes commandes. Il fonce épaule en avant et traverse environ 250 unités pendant ses poses actives (la simulation le fait avancer, les poses sont dessinées sur place) ; un seul coup, 120 dégâts ;
  - Étendard d'Orléans de Jeanne d'Arc : mêmes commandes. Elle s'élance étendard pointé comme une lance, environ 290 unités, et sa fente porte jusqu'à 460 unités ; un seul coup, 90 dégâts ;
  - Le Panache de Cyrano : mêmes commandes. Un coup de taille horizontal lance une plume de lumière à l'entrée de la 5ᵉ pose, à hauteur de poitrine, une seule à la fois, avec un impact au contact.
- **Réactions** :
  - dégâts, étourdissement, gel d'impact, recul (reporté sur l'attaquant si la cible est coincée), puis retour en garde ;
  - garde en tenant arrière, avec garde sur place face à une attaque ou un projectile proche. Debout, elle arrête les coups hauts et moyens ; accroupi (bas-arrière), les coups bas et moyens. Pas de garde en l'air ;
  - touché en l'air, le combattant retombe et reste étourdi jusqu'à la réception ;
  - KO : chute au sol en six poses, habits déchirés et taches de sang (sans gore) ;
  - deux projectiles adverses s'annulent ; deux coups simultanés s'échangent.
- **Enchaînements** (règles `patate-core` v3), pour les six combattants :
  - un coup qui touche **ou** est gardé s'interrompt dans un coup plus lourd : poing → pied rapide → pied puissant (le poing peut aussi passer directement au pied puissant), et poing accroupi → balayette ;
  - un coup normal qui **touche** (pas gardé) s'annule dans le spécial, même le pied puissant et la balayette ;
  - l'appui compte dès le contact, y compris pendant le gel d'impact ; la suite démarre à la fin du gel ;
  - au contact, chaque enchaînement déclaré est un vrai combo : la suite touche avant la fin de l'étourdissement. De loin, le recul peut faire manquer la suite ;
  - les annulations se déclarent coup par coup dans `gameplay.json` (`cancels`, `cancelsOnHit`, `cancelOnly`), sans code propre à un personnage.
- **Manche** : introduction, chronomètre de 99 secondes, KO ou temps écoulé, annonce du vainqueur, puis **Entrée** (ou **Start**) vers l'écran de résultat ; **F5** recommence la manche.
- **Menus** : écran titre avec le logo, choix du mode (Versus local, Solo contre l'ordinateur avec trois niveaux, En ligne), choix de l'appareil de chaque joueur (« Qui joue ? » : clavier gauche, clavier droit, manette 1, 2…), sélection des personnages (joueurs déjà placés ; un appareil libre peut encore rejoindre en appuyant, portraits, choix du décor quand il y en a plusieurs), résultat (revanche, changer de personnage, retour au menu). Navigation au clavier et à la manette.
- **Décors** : fête foraine, Sainte-Chapelle et Austerlitz, chacun en trois calques peints (ciel, plan moyen, sol) avec parallaxe, plus l'arène d'entraînement ; vignette de chaque décor à la sélection.
- **Habillage** : fonds peints de la sélection et du résultat, emblème « VS » quand les deux joueurs sont prêts, étiquettes lisibles sur fond bleu marine, police d'affichage Bangers (OFL) pour les titres, noms, boutons et le chronomètre, étincelles peintes sur les coups au corps (touche et garde, taille selon les dégâts).
- **Interface de combat** : portraits et jauges de vie dans des cadres dorés peints, traînée des dégâts, médaillon du chronomètre, noms et appareil de chaque joueur, annonces de manche peintes, pause, aide des commandes (**F2**), changement de combattant (**F3**/**F4**).
- **Navigateur** : gabarit WebGL au logo du jeu pendant le chargement, jeu ajusté à la fenêtre.
- **Débogage (F1)** :
  - boîtes de la simulation : pushbox, hurtboxes, hitboxes actives et consommées, projectiles ;
  - pivot, état, tick et gel de chaque combattant ;
  - entrées, empreinte de l'état, références exactes des définitions ;
  - signaux sonores non livrés.

## Ce qui a été vérifié, et comment

- **154 tests EditMode, tous verts** : 154/154 au dernier lancement (1ᵉʳ novembre 2026, dans l'éditeur ouvert avec `unity command test run --mode EditMode`) (le test du relais, `[Explicit]`, lancé à part avec le relais local) ; en mode batch : `unity test --mode EditMode .`. Ils couvrent :
  - **chronologie** : les phases tombent aux ticks exacts, l'appui sur un bouton déclenche le coup mais pas son maintien, quart de cercle selon l'orientation ;
  - **dégâts** : un seul impact par coup, étourdissement plus gel, recul exact, échange de coups simultanés ;
  - **garde** : tenir arrière, garde de proximité, pas de garde en pleine attaque, attaque non blocable ;
  - **collisions** : jamais de chevauchement, poussée, limites, coin ;
  - **projectiles** : émission unique (y compris avec un gel forcé au tick d'émission), une instance par lanceur, comptée par lanceur en miroir, contact, garde, annulation, expiration ;
  - **déterminisme** : mêmes entrées, même empreinte ; sauvegarde, restauration puis reprise identiques à l'octet près ; l'état d'entrée n'est jamais modifié ;
  - **indépendance des deux combattants** ;
  - **saut et accroupissement** : hauteur et durée du saut, distance d'un saut avant, pas de marche en l'air ni accroupi, coup accroupi au bouton bas tenu, coup haut qui passe au-dessus d'un adversaire accroupi, coup bas gardé seulement accroupi, coup sauté gardé debout mais pas accroupi, un coup par saut puis réception, pas de garde en l'air, sauvegarde et restauration en plein saut ;
  - **manche** : KO, temps écoulé, entrées ignorées pendant l'introduction et après le KO ;
  - **validation des paquets** : les vrais paquets des six combattants sont acceptés ; 18 paquets volontairement cassés sont refusés (dont trois annulations invalides : coup inconnu, coup sauté, coup lui-même), chacun avec un message localisé ;
  - **présentation** : la pose de frappe est affichée pendant les ticks actifs, le projectile apparaît à la 5ᵉ pose, les réactions sont étirées sur leur durée (pour les deux combattants) ;
  - **Napoléon contre Saint Louis** : la Salve impériale touche Saint Louis immobile (90 dégâts) ; Napoléon le met KO avec ses trois coups normaux, qui portent tous ;
  - **De Gaulle contre Napoléon** : l'Appel du 18 Juin touche Napoléon debout (85 dégâts) et part à l'entrée de la 5ᵉ pose ; la pose de frappe s'affiche pendant les ticks actifs ; De Gaulle met Napoléon KO avec ses trois coups normaux ;
  - **Vercingétorix, Jeanne d'Arc et Cyrano contre Napoléon** : la pose de frappe s'affiche pendant les ticks actifs (charges comprises, poses 4 à 6) ; les charges partent de la distance de départ, avancent et touchent Napoléon (120 et 90 dégâts) ; le Panache part à l'entrée de la 5ᵉ pose et touche Napoléon debout (80 dégâts) ; chacun met Napoléon KO avec ses trois coups normaux ;
  - **enchaînements** : pour chaque annulation déclarée des six combattants, au contact de Napoléon, la suite touche pendant l'étourdissement du premier coup ; sans annulation déclarée, la suite attend la fin du coup ;
  - **politiques de statistiques** : modes libre et équilibré ;
  - **session** : combat miroir réel joué jusqu'au KO sans rendu, replay par le journal d'entrées, nouvelle manche, horloge à pas fixe à 144 Hz ;
  - **clavier** : correspondances des deux joueurs, vérifiées sur un clavier virtuel ;
  - **manette** : directions (stick, croix), boutons de face, manette débranchée, « appuyer pour rejoindre », sur une manette virtuelle de l'Input System ;
  - **adversaire de l'ordinateur** : il lance ses coups accroupis bas tenu, garde accroupi face à un coup bas ; il gagne par KO contre un adversaire immobile à chaque niveau, avec Saint Louis et avec le combattant de synthèse ; il garde parfois des projectiles ; le niveau difficile garde et frappe davantage que le facile ; même graine, même partie, y compris après « recommencer ».
  - **choix des appareils** : un appareil par joueur, appareils précédents repris s'ils sont branchés, échange quand on prend celui de l'autre, Solo et en ligne à un seul joueur, l'ordinateur gardé, manette débranchée qui bloque la suite ;
  - **sélection des personnages** : rejoindre, déplacer et verrouiller, revenir en arrière, étape du décor, Solo (le joueur choisit pour l'ordinateur), joueurs gardés en changeant de personnage, réglages produits ;
  - **menus à la manette** : stick (un appui, puis attente avant répétition, seuil, direction dominante) et liste des appareils, sur une manette virtuelle. Les boutons et la croix (appuis « cette image ») ne se testent pas en EditMode sans le cadre de test de l'Input System : non vérifiés.
- **Les tests détectent bien les régressions** : en cassant volontairement « un seul impact par coup » et la garde, 12 tests échouent.
- **Le domaine ne dépend pas d'Unity** : `noEngineReferences` sur Combat, Content et Application.
- **En jeu, mode Play** : un scénario scripté (`AgentScripts/VerifyMatch.cs`) a été joué jusqu'au bout, avec des captures d'écran contrôlées à chaque étape :
  - pied puissant qui touche au tick 146 (110 dégâts, gel de 12 ticks) ;
  - fleur de lys émise puis bloquée au tick 225 (0 dégât, impact affiché) ;
  - KO au tick 1136 et fin de manche au tick 1286.
- **Postures, en mode Play** (`AgentScripts/VerifyStances.cs`, Saint Louis contre Saint Louis puis Napoléon contre Saint Louis, boîtes de débogage affichées) : accroupi des deux côtés, poing accroupi qui touche un adversaire accroupi (35 dégâts), balayette gardée accroupi, saut vertical, pied sauté et poing sauté qui touchent la tête d'un adversaire debout, KO jusqu'à la pose allongée. Les hitboxes suivent le poing et la botte dessinés. Le KO de Saint Louis flottait au-dessus du sol : corrigé à l'export. Le ressenti du saut (hauteur, durée) n'a pas été jugé par un joueur.
- **De Gaulle, en mode Play** (1ᵉʳ novembre 2026, `VerifyStances` puis `VerifyMatch` avec De Gaulle contre Napoléon, boîtes de débogage affichées) : pied sauté et poing sauté qui touchent (75 et 60 dégâts), balayette gardée accroupi, pied puissant dont la hitbox suit la botte dessinée, Appel du 18 Juin émis puis gardé par Napoléon avec l'impact tricolore affiché, KO ; portrait et nom dans le HUD. Le poing accroupi n'a pas porté dans ce scénario : De Gaulle, qui marche moins vite, n'était pas à portée.
- **Vercingétorix, Jeanne d'Arc et Cyrano, en mode Play** (1ᵉʳ novembre 2026, contre Napoléon, boîtes de débogage affichées) : la Charge d'Alésia part de la distance de départ et l'épaule touche Napoléon (120 dégâts) ; l'Étendard d'Orléans touche à la fente (90 dégâts) ; la plume du Panache sort au bout de la rapière et touche (80 dégâts) ; poing, pied rapide et pied puissant portent ; avec `VerifyStances`, poing accroupi de Jeanne et de Cyrano qui touche (35), balayette gardée accroupi par Napoléon, pieds sautés de Vercingétorix et de Jeanne qui touchent (80 et 70), KO de Napoléon. Le poing accroupi de Vercingétorix n'a pas porté (il marche moins vite et n'était pas à portée) ; le pied sauté de Cyrano, court, a manqué Napoléon sur le saut vertical. Portraits et noms dans le HUD.
- **Export** : 97 poses découpées par combattant (56 avant les postures), 2 pages d'atlas chacun. Les deux exports ont été relus sur leurs aperçus (fond clair et sombre, superpositions) ; les sources sont intactes.
  - Napoléon : bicorne, pans de la redingote et fourreau stables d'une pose à l'autre, pieds sur la ligne de sol, pivot commun, aucun recentrage automatique (le pied d'appui reste planté pendant les coups de pied, le buste part en arrière comme dessiné). Garde ramenée à 480 unités contre 500 pour Saint Louis : il paraît plus trapu, conformément à son identité.
  - Dans la réaction de Napoléon, les poses 2 et 3 se touchent (pans de manteau, et pointe de botte dessinée devant le talon voisin). L'outil les sépare désormais par une **couture** (`layout.seams`) ; réexporter Saint Louis avec l'outil modifié redonne des fichiers identiques à l'octet près.

- **Boucle temps réel, en mode Play normal sans pilotage** : 252 ticks simulés en environ 4,2 secondes, soit environ 60 par seconde ; l'horloge à pas fixe et la boucle `Update` fonctionnent.

- **Export WebGL** : construit en mode batch (`WebBuilder`, 82 s), servi en statique et ouvert dans un navigateur Chromium : les deux Saint Louis se chargent (lecture des paquets JSON avec Newtonsoft intacte après IL2CPP, sans `link.xml`), marchent et se frappent sur des touches maintenues (événements clavier simulés dans la page, pas une frappe humaine), les jauges baissent, aucune erreur dans la console. Environ 13 Mo à télécharger à l'époque ; 14,4 Mo depuis le décor de la fête foraine (vérifié de même, décor affiché derrière le combat, aucune erreur). Seuls avertissements : `getInternalformatParameter: invalid internalformat`, émis par le moteur au démarrage, sans effet visible. L'atlas en BC7 dépend de l'extension `EXT_texture_compression_bptc`, présente ici ; non essayé sans.
- **Ensemble fusionné (Napoléon, décor, manettes et IA, menus, en ligne), en WebGL** : environ 19 Mo à télécharger. Parcours vérifié au clavier simulé : titre au logo → modes → Solo → sélection (Napoléon contre Saint Louis tenu par l'ordinateur, décor) → combat sur la fête foraine, où l'ordinateur attaque, garde et lance la fleur de lys → KO → résultat → retour au menu → salon en ligne. En ligne : deux onglets Chrome sans interface (`server/relay`, `npm run essai:onglets`) jouent deux manches avec revanche via le relais local, même empreinte finale des deux côtés, rollbacks sans désynchronisation. Aucune erreur dans la console.
- **Défauts vus à la fusion, corrigés** : en fin de manche en ligne, une seule invite (celle du mode en ligne) et l'annonce de victoire, absente jusque-là car la simulation s'arrête à la fin de la manche ; le salon en ligne a le style des menus et se pilote au clavier, à la manette ou à la souris ; l'annonce de victoire nomme la source du vainqueur (« J2 · Ordinateur », « J1 · En ligne »). Vérifié en WebGL : deux manches en ligne sans désynchronisation, et un salon rejoint en tapant son code dans le menu (Chrome sans interface, vraies frappes).

- **Intégration continue et GitHub Pages** (1er novembre 2026) : le workflow `web.yml` passe en entier sur `main` (outils et relais, tests EditMode, version navigateur, publication). Le jeu publié sur <https://eth3rnit3.github.io/patate-de-forain/> répond, `build-info.json` donne la compilation de la CI, et le parcours titre → modes → Versus → « Qui joue ? » → sélection → combat sur la fête foraine fonctionne au clavier (vraies frappes dans le navigateur intégré), sans erreur dans la console.

- **Napoléon en WebGL** : build batch de la branche, servi en statique, ouvert dans Chromium. **F3** fait passer le joueur 1 à Napoléon, puis **F3**/**F4** inversent les côtés. Événements clavier simulés dans la page. Vérifié avec les boîtes (**F1**) :
  - la Salve impériale part du doigt, à hauteur de poitrine, et touche Saint Louis ;
  - le poing et le pied puissant portent, et les hurtboxes suivent le buste penché ;
  - Napoléon, placé à droite, joue sa réaction puis bloque en tenant arrière ;
  - aucune erreur dans la console.

  Défaut relevé, antérieur à Napoléon : le panneau de débogage demande la police Menlo, absente en WebGL. Son texte reste vide et la console reçoit un avertissement à chaque image.

- **Manettes et IA en WebGL** (même navigateur, page en arrière-plan, boucle d'affichage remplacée par une boucle à 60 Hz pour rester en temps réel) :
  - clavier contre l'ordinateur (niveau normal) : l'IA avance, frappe, lance la fleur de lys et gagne la manche contre une suite de touches simulées ; l'aide **F2** affiche « Ordinateur — Niveau : Normal » ;
  - manette simulée dans la page (faux `navigator.getGamepads()`, disposition standard) : Unity la reconnaît comme `WebGLGamepad`, chaque bouton donne la bonne commande (X poing, A pied rapide, B pied puissant, Y spécial, stick et croix pour les directions), `Connected()` et `JustPressed()` la signalent, l'aide affiche « X / Carré »… Ce n'est **pas une vraie manette** : aucune manette physique Xbox ou PlayStation n'a été essayée, ni dans le navigateur ni dans l'éditeur.

- **Interface, en WebGL au clavier** (événements clavier simulés dans la page, Chromium) : logo et progression pendant le chargement ; titre → modes (Solo refusé tant que l'ordinateur n'est pas branché, En ligne → salon provisoire → titre) → sélection (deux joueurs rejoignent avec F et Entrée, verrouillent, annulent avec G, aucun joueur placé par l'appui qui ouvre l'écran) → combat (annonces, jauges, étiquettes « J1 · Clavier gauche ») → K.O. → victoire → résultat → changer de personnage (joueurs gardés) et retour au menu. Page ajustée à une fenêtre 16:9 et à une fenêtre plus haute que large. Revanche non rejouée en WebGL (même appel que l'ancien écran provisoire). Manettes : navigation non essayée sur une vraie manette (seul le stick l'est, sur une manette virtuelle).

- **Habillage (31 octobre), en WebGL au clavier simulé** (environ 23 Mo à télécharger) : vignettes et passage d'un décor à l'autre à la sélection (quatre décors), emblème « VS », HUD encadré, annonce « Combat ! », bandeau de victoire, écran de résultat, étiquettes lisibles. À Austerlitz, les deux combattants poussés contre chaque mur : aucun bord de décor visible. Étincelles vérifiées dans l'éditeur par un combat scripté (`AgentScripts/VerifyEffects.cs`) : touche à Austerlitz, garde à la Sainte-Chapelle. 114 tests verts, 1 ignoré (relais). Non vérifié : les murs de la Sainte-Chapelle (même géométrie qu'Austerlitz).
- **Choix des appareils, en WebGL avec une manette simulée** (une manette Xbox standard injectée dans `navigator.getGamepads`, reconnue par Unity : « Gamepad added ») : titre et mode validés au bouton A, « Qui joue ? » où « Manette 1 » apparaît, J1 passe sur la manette (croix gauche), J2 garde le clavier droit ; sélection avec les deux joueurs déjà placés, Napoléon choisi à la manette ; en combat, J1 avance et saute à la croix. Aucune erreur dans la console. Non vérifié : une vraie manette, deux manettes en même temps.
- **Postures, en WebGL au clavier simulé** (environ 27,5 Mo à télécharger, 19,4 Mo de données) : sélection, puis en combat J1 accroupi (↓ tenu) pendant que J2 saute en avant (Z + Q) ; saut sous le HUD, aucune erreur dans la console. Non vérifié : manette réelle, Safari et Firefox.
- **Police Bangers, en WebGL au clavier simulé** : titre, modes, sélection (accents de « Napoléon », « Prêt ! »), VS, HUD, victoire, résultat et salon en ligne lus à l'écran ; aucune erreur dans la console ; 114 tests verts.
- **Relais en ligne déployé** (Cloudflare Workers) : les essais de conformité passent contre le relais déployé, et deux onglets Chrome sans interface jouent deux manches à travers lui (ticks 1189 et 1523, même empreinte des deux côtés, 129 à 206 rollbacks, aucune désynchronisation).

**Pas vérifié par moi** : une vraie partie au clavier, à deux humains, avec la sensation de jeu. C'est le prochain essai à faire. Il n'y a pas eu d'export autonome (PC, Mac), ni d'essai WebGL sous Safari ou Firefox.

- **Jeu en ligne** : relais local (`server/relay`), build WebGL servi en statique, deux onglets d'un même Chrome sans interface, chacun en robot (`?robot`), relais réglé à 60 ms de latence par message. Deux manches complètes, revanche comprise, finies par KO au même tick des deux côtés, avec la même empreinte d'état ; environ 200 rollbacks par joueur et par manche, 45 à 49 empreintes comparées en cours de manche, aucune désynchronisation (`npm run essai:onglets`). Le relais Cloudflare est **déployé** (`wss://patate-relais.eth3rnit3.workers.dev`, 31 octobre 2026) : il passe le scénario de salon (`npm run verifier`), et le même essai à deux onglets à travers lui (`RELAIS=wss://… npm run essai:onglets`, aucun relais local lancé) a joué deux manches, revanche comprise, finies au même tick des deux côtés avec la même empreinte (ticks 1189 et 1523), 129 à 206 rollbacks par joueur et par manche, aucune désynchronisation. Les tests EditMode du protocole (10) font jouer deux machines simulées avec latence et gigue. Deux onglets d'une même fenêtre, l'un caché, jouent aussi, mais au rythme de l'onglet caché que le navigateur ralentit à 1 ou 2 images par seconde.

Le transport de l'éditeur et du PC (`ClientWebSocket`) a échangé avec le relais local dans un test explicite (`OnlineRelayTests`, à lancer par son nom, relais démarré).

**Pas vérifié en ligne** : une partie entre deux humains, sur deux machines distantes ; un combat complet depuis l'éditeur ou un export PC.

## Hypothèses à régler en jouant

- **Timings** :
  - pied rapide, pied puissant et spécial : ceux de `moveset-prototype-v01.md` ;
  - poing : aucune donnée documentée, j'ai pris 2-3-2-3-4-6 ticks, actif sur les poses 3 et 4.
- **Napoléon** : timings de `assets/characters/napoleon/moveset-prototype-v01.md`, poing actif sur les poses 3 et 4 comme Saint Louis. Mêmes dégâts que Saint Louis ; marche plus lente (7 et 5), boulet un peu plus lent (11 par tick). Ses coups de pied ont moins d'allonge que son poing, parce qu'il recule le buste en frappant.
- **De Gaulle** : garde à 560 unités (le plus grand), timings de `assets/characters/de-gaulle/moveset-prototype-v01.md`, un peu plus lents au démarrage (poing 6 ticks, pied rapide 9, pied puissant 18, appel 16). Mêmes dégâts que les autres, onde à 85 dégâts et 10 par tick. Marche la plus lente (6 et 4). Son allonge se voit surtout en bas et en l'air : balayette jusqu'à 430 unités, pied sauté jusqu'à 515.
- **Vercingétorix** : le colosse, garde à 520 unités, 1150 points de vie, marche à 5 et 4. Démarrages lents (poing 9 ticks, pied rapide 9, pied puissant 19, charge 14) et coups plus lourds (50, 70, 130). Pas de projectile ; sa charge avance de 14 par tick sur 18 ticks actifs.
- **Jeanne d'Arc** : la plus petite (470 unités) et la plus rapide : poing 4 ticks, pied rapide 7, pied puissant 14, charge 11 ; 950 points de vie ; coups plus légers (35, 55, 100). Sa charge avance de 18 par tick sur 16 ticks actifs.
- **Cyrano** : garde à 500 unités, 950 points de vie, marche à 8 et 6. La rapière lui donne la plus longue allonge debout (poing jusqu'à 465 unités) ; Panache en 15 ticks, plume à 12 par tick et 80 dégâts. Son pied sauté est court (260 unités).
- **Enchaînements** (1ᵉʳ novembre 2026) : pour que chaque suite fasse combo, l'étourdissement des coups légers a été allongé (poing 17 à 22 ticks selon le combattant, pied rapide 17 à 22, poing accroupi 14 à 19, balayette 16 à 19) et les spéciaux à projectile démarrent plus vite (lys 15 ticks au lieu de 18, salve 15 au lieu de 20, appel 16 au lieu de 21, panache 15 au lieu de 19). Contrepartie : un poing qui touche laisse désormais 8 à 10 ticks d'avance, ce qui peut suffire à placer un second poing une fois le premier terminé (non vérifié) ; le recul limiterait ces répétitions à quelques coups. À surveiller en jouant.
- **Valeurs provisoires** : dégâts, étourdissement, gel, recul, vitesses de marche (8 et 6), vitesse du projectile (12 par tick), boîtes de collision. Les boîtes sont mesurées sur les poses exportées, mais restent grossières (trois hurtboxes par pose).
- **Taille à l'écran** : garde à 500 unités, écran visible de 1080 unités de haut (le standard évoquait environ 600 px, à confirmer).

## Limites actuelles

- **Une seule manche** : pas de match en deux manches gagnantes.
- **Pas de retour au menu pendant un combat** : la pause (**Échap**) n'a pas encore de menu ; il faut finir la manche.
- **Pas d'animation dédiée** :
  - recul : marche avant lue à l'envers ;
  - garde de proximité : 1ʳᵉ pose de blocage tenue.
- **Absents** : chopes, étourdissement cumulé (dizzy), compteur de combo, dégâts dégressifs dans un combo.
- **Un seul recul** : pas de recul « K.O. » ; la chute du KO se joue sur place.
- **Saut** : une seule hauteur et une seule vitesse pour les deux combattants (`stats.jump` permet de les régler par personnage). Pas de super saut, de saut court ni de coup spécial en l'air. Pas de réception dédiée après un coup reçu en l'air : la pose de touché haut est tenue jusqu'au sol.
- **Alignement de l'export** : il cale le pied arrière (ou le buste pour la marche) mais ne conserve pas les déplacements dessinés dans les planches. Les échelles par planche gardent quelques pour cent d'écart.
- **Animations de garde et de marche jouées telles quelles** : six poses de garde quasi identiques, cycle de marche non retravaillé (voir les contrôles de l'équipe artistique).
- **Aucun son** : les signaux sont branchés, mais sans fichier.
- **Clavier** : un seul échantillon par image d'affichage, donc un appui plus court qu'une image peut être manqué. Certains claviers ne détectent pas toutes les combinaisons de touches simultanées (ghosting). Même échantillonnage par image pour la manette.
- **Adversaire de l'ordinateur** : il s'accroupit pour ses coups bas et sa garde basse, mais ne saute jamais et n'utilise pas les coups sautés ; il n'a donc pas de réponse à un adversaire qui ne fait que lancer des projectiles de loin (il garde sans avancer). Il n'utilise pas les enchaînements.
- **Pause** : le jeu se met en pause quand la fenêtre perd le focus.
- **Jeu en ligne** : pas de reconnexion après une coupure ; un onglet caché met l'adversaire en attente ; relais déployé réservé aux pages du jeu (GitHub Pages et localhost) depuis le 1er novembre 2026, mais un client hors navigateur peut toujours s'y connecter, puisqu'il n'envoie pas d'en-tête `Origin` ; offre gratuite de Cloudflare, environ 250 manches par jour.
- **Pas encore faits** : import de paquets et signature (voir l'[architecture](architecture.md)).

## Besoins artistiques à nous transmettre

Classés par effet sur le jeu.

1. **Saint Louis** :
   - pose de victoire ;
   - marche arrière (8 poses) ;
   - impact de blocage (réaction du personnage ; l'étincelle de garde est faite).
2. **Stabilisation** de la boucle de garde et du cycle de marche : appuis fixes, détails constants (fleurs de lys, couronne, épée).
3. **Décors**. Trois sont en jeu (fête foraine par défaut, Sainte-Chapelle, Austerlitz) ; leur musique manque (champ `music` de `stage.json`). Pour les suivants, suivre [le dossier de la fête foraine](../assets/stages/fete-foraine/README.md) ou celui de la [Sainte-Chapelle](../assets/stages/sainte-chapelle/README.md), générée avec `tools/codex-image/generate.py`. Repères (`Resources/Stages/<id>/`) :
   - 1 unité = 1 px à l'échelle 100 ; le champ `scale` d'un calque l'agrandit à l'affichage ;
   - zone visible de 1920 × 1080 ;
   - sol à 130 px du bas ;
   - arène de 2400 px plus 240 px de débord de chaque côté, soit un plan du sol d'**au moins 2880 px de large** (2900 avec le tremblement), et environ 1920 + 9,6 × parallaxe pour un plan éloigné ;
   - PNG, dimensions multiples de 4 ; plusieurs calques : fond, plan moyen, sol.
4. **Sons**. Signaux attendus :
   - début de manche ;
   - lancement de coup ;
   - impact léger et lourd ;
   - blocage ;
   - émission et impact du projectile ;
   - KO ;
   - temps écoulé ;
   - musique du décor.

   OGG ou WAV. Un son propre à un coup se déclare par exemple sous `moveStart:stand_kick_heavy`.
5. **Interface** : la police Bangers (OFL, Google Fonts) remplace la police par défaut pour le gras ; une police dessinée pour le jeu, plus proche du pinceau du logo, reste possible (`Ui.Display`). Les fonds, cadres du HUD, « VS », mots d'annonce et étincelles ont été générés avec Codex CLI ([assets/ui](../assets/ui/README.md)) et peuvent être remplacés dans `game/Assets/Resources/UI/` et `Effects/` sans toucher au code.
6. **Pour aller vers six boutons** : poing moyen et fort, pied moyen, avec leurs versions accroupies et sautées.
7. **Napoléon** : les mêmes besoins que Saint Louis (victoire, marche arrière, impact de blocage), plus :
   - réaction haute : écarter les poses 2 et 3. La pointe de botte de la pose 2 masque le talon de la pose 3, auquel il manque donc un petit morceau ;
   - garde : six poses presque identiques, à stabiliser comme celles de Saint Louis.
8. **De Gaulle** : planches générées avec Codex CLI, non retouchées (voir [son dossier](../assets/characters/de-gaulle/README.md)) :
   - réaction accroupie : les poses 1 et 2 sont à demi accroupies seulement ;
   - KO : la chute est recalée à l'export (`nudge`) pour qu'il ne saute pas de 400 unités en arrière entre deux poses ;
   - mêmes besoins que les autres (victoire, marche arrière, impact de blocage).
9. **Vercingétorix, Jeanne d'Arc et Cyrano** : planches générées avec Codex CLI, non retouchées (voir les dossiers de [Vercingétorix](../assets/characters/vercingetorix/README.md), de [Jeanne](../assets/characters/jeanne/README.md) et de [Cyrano](../assets/characters/cyrano/README.md)) :
   - charges et KO recalés à l'export (`nudge`) : les poses dessinées très en retrait faisaient reculer le personnage entre deux poses ;
   - Cyrano : poses du pied puissant trop serrées sur la planche, séparées par des coutures ;
   - pas encore d'IA qui sache se servir des charges autrement que comme un coup au contact ;
   - mêmes besoins que les autres (victoire, marche arrière, impact de blocage).
10. **Combattant suivant** : suivre [Ajouter un combattant](ajouter-un-combattant.md). Laisser de l'espace entre les poses évite les coutures.
