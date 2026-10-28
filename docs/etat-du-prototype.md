# État du prototype — 28 octobre 2026

## Ce qui fonctionne en jeu

Deux combattants jouables, **Saint Louis** et **Napoléon**, dans Unity 6000.4.7f1 (scène `Assets/Scenes/Combat.unity`), au clavier ou à la manette, avec le parcours complet : titre, choix du mode (versus local, solo contre l'ordinateur, en ligne), sélection des personnages et du décor, combat, résultat.

- **Déplacements** : marche avant et arrière, orientation automatique vers l'adversaire, collision corporelle (pousser l'adversaire en avançant), limites de l'arène, écart maximal entre combattants.
- **Coups** :
  - poing ;
  - pied rapide ;
  - pied puissant ;
  - Poing de Saint Louis : touche dédiée ou quart de cercle avant + poing. Il projette une fleur de lys émise au poing à l'entrée de la 5ᵉ pose, à raison d'un seul projectile à la fois, qui produit un impact doré au contact ;
  - Salve impériale de Napoléon : mêmes commandes. Napoléon pointe l'adversaire et un boulet spectral part du doigt à l'entrée de la 5ᵉ pose, à hauteur de poitrine, un seul à la fois, avec un impact bleu au contact.
- **Réactions** :
  - dégâts, étourdissement, gel d'impact, recul (reporté sur l'attaquant si la cible est coincée), puis retour en garde ;
  - garde en tenant arrière, avec garde sur place face à une attaque ou un projectile proche ;
  - deux projectiles adverses s'annulent ; deux coups simultanés s'échangent.
- **Manche** : introduction, chronomètre de 99 secondes, KO ou temps écoulé, annonce du vainqueur, puis **Entrée** (ou **Start**) vers l'écran de résultat ; **F5** recommence la manche.
- **Menus** : écran titre avec le logo, choix du mode (Versus local, Solo contre l'ordinateur avec trois niveaux, En ligne), sélection des personnages (appuyer pour rejoindre au clavier ou à la manette, portraits, choix du décor quand il y en a plusieurs), résultat (revanche, changer de personnage, retour au menu). Navigation au clavier et à la manette.
- **Interface de combat** : portraits, jauges de vie avec traînée des dégâts, chronomètre, noms et appareil de chaque joueur, annonces de manche peintes, pause, aide des commandes (**F2**), changement de combattant (**F3**/**F4**).
- **Navigateur** : gabarit WebGL au logo du jeu pendant le chargement, jeu ajusté à la fenêtre.
- **Débogage (F1)** :
  - boîtes de la simulation : pushbox, hurtboxes, hitboxes actives et consommées, projectiles ;
  - pivot, état, tick et gel de chaque combattant ;
  - entrées, empreinte de l'état, références exactes des définitions ;
  - signaux sonores non livrés.

## Ce qui a été vérifié, et comment

- **115 tests EditMode, tous verts** : 115/115 au dernier lancement (le test du relais, `[Explicit]`, lancé à part avec le relais local), en mode batch (`unity test --mode EditMode .`). Ils couvrent :
  - **chronologie** : les phases tombent aux ticks exacts, l'appui sur un bouton déclenche le coup mais pas son maintien, quart de cercle selon l'orientation ;
  - **dégâts** : un seul impact par coup, étourdissement plus gel, recul exact, échange de coups simultanés ;
  - **garde** : tenir arrière, garde de proximité, pas de garde en pleine attaque, attaque non blocable ;
  - **collisions** : jamais de chevauchement, poussée, limites, coin ;
  - **projectiles** : émission unique (y compris avec un gel forcé au tick d'émission), une instance par lanceur, comptée par lanceur en miroir, contact, garde, annulation, expiration ;
  - **déterminisme** : mêmes entrées, même empreinte ; sauvegarde, restauration puis reprise identiques à l'octet près ; l'état d'entrée n'est jamais modifié ;
  - **indépendance des deux combattants** ;
  - **manche** : KO, temps écoulé, entrées ignorées pendant l'introduction et après le KO ;
  - **validation des paquets** : les vrais paquets de Saint Louis et de Napoléon sont acceptés ; 15 paquets volontairement cassés sont refusés, chacun avec un message localisé ;
  - **présentation** : la pose de frappe est affichée pendant les ticks actifs, le projectile apparaît à la 5ᵉ pose, les réactions sont étirées sur leur durée (pour les deux combattants) ;
  - **Napoléon contre Saint Louis** : la Salve impériale touche Saint Louis immobile (90 dégâts) ; Napoléon le met KO avec ses trois coups normaux, qui portent tous ;
  - **politiques de statistiques** : modes libre et équilibré ;
  - **session** : combat miroir réel joué jusqu'au KO sans rendu, replay par le journal d'entrées, nouvelle manche, horloge à pas fixe à 144 Hz ;
  - **clavier** : correspondances des deux joueurs, vérifiées sur un clavier virtuel ;
  - **manette** : directions (stick, croix), boutons de face, manette débranchée, « appuyer pour rejoindre », sur une manette virtuelle de l'Input System ;
  - **adversaire de l'ordinateur** : il gagne par KO contre un adversaire immobile à chaque niveau, avec Saint Louis et avec le combattant de synthèse ; il garde parfois des projectiles ; le niveau difficile garde et frappe davantage que le facile ; même graine, même partie, y compris après « recommencer ».
  - **sélection des personnages** : rejoindre, déplacer et verrouiller, revenir en arrière, étape du décor, Solo (le joueur choisit pour l'ordinateur), joueurs gardés en changeant de personnage, réglages produits ;
  - **menus à la manette** : stick (un appui, puis attente avant répétition, seuil, direction dominante) et liste des appareils, sur une manette virtuelle. Les boutons et la croix (appuis « cette image ») ne se testent pas en EditMode sans le cadre de test de l'Input System : non vérifiés.
- **Les tests détectent bien les régressions** : en cassant volontairement « un seul impact par coup » et la garde, 12 tests échouent.
- **Le domaine ne dépend pas d'Unity** : `noEngineReferences` sur Combat, Content et Application.
- **En jeu, mode Play** : un scénario scripté (`AgentScripts/VerifyMatch.cs`) a été joué jusqu'au bout, avec des captures d'écran contrôlées à chaque étape :
  - pied puissant qui touche au tick 146 (110 dégâts, gel de 12 ticks) ;
  - fleur de lys émise puis bloquée au tick 225 (0 dégât, impact affiché) ;
  - KO au tick 1136 et fin de manche au tick 1286.
- **Export** : 56 poses découpées par combattant, 1 page d'atlas chacun. Les deux exports ont été relus sur leurs aperçus (fond clair et sombre, superpositions) ; les sources sont intactes.
  - Napoléon : bicorne, pans de la redingote et fourreau stables d'une pose à l'autre, pieds sur la ligne de sol, pivot commun, aucun recentrage automatique (le pied d'appui reste planté pendant les coups de pied, le buste part en arrière comme dessiné). Garde ramenée à 480 unités contre 500 pour Saint Louis : il paraît plus trapu, conformément à son identité.
  - Dans la réaction de Napoléon, les poses 2 et 3 se touchent (pans de manteau, et pointe de botte dessinée devant le talon voisin). L'outil les sépare désormais par une **couture** (`layout.seams`) ; réexporter Saint Louis avec l'outil modifié redonne des fichiers identiques à l'octet près.

- **Boucle temps réel, en mode Play normal sans pilotage** : 252 ticks simulés en environ 4,2 secondes, soit environ 60 par seconde ; l'horloge à pas fixe et la boucle `Update` fonctionnent.

- **Export WebGL** : construit en mode batch (`WebBuilder`, 82 s), servi en statique et ouvert dans un navigateur Chromium : les deux Saint Louis se chargent (lecture des paquets JSON avec Newtonsoft intacte après IL2CPP, sans `link.xml`), marchent et se frappent sur des touches maintenues (événements clavier simulés dans la page, pas une frappe humaine), les jauges baissent, aucune erreur dans la console. Environ 13 Mo à télécharger à l'époque ; 14,4 Mo depuis le décor de la fête foraine (vérifié de même, décor affiché derrière le combat, aucune erreur). Seuls avertissements : `getInternalformatParameter: invalid internalformat`, émis par le moteur au démarrage, sans effet visible. L'atlas en BC7 dépend de l'extension `EXT_texture_compression_bptc`, présente ici ; non essayé sans.
- **Ensemble fusionné (Napoléon, décor, manettes et IA, menus, en ligne), en WebGL** : environ 19 Mo à télécharger. Parcours vérifié au clavier simulé : titre au logo → modes → Solo → sélection (Napoléon contre Saint Louis tenu par l'ordinateur, décor) → combat sur la fête foraine, où l'ordinateur attaque, garde et lance la fleur de lys → KO → résultat → retour au menu → salon en ligne. En ligne : deux onglets Chrome sans interface (`server/relay`, `npm run essai:onglets`) jouent deux manches avec revanche via le relais local, même empreinte finale des deux côtés, rollbacks sans désynchronisation. Aucune erreur dans la console.
- **Défauts vus à la fusion, corrigés** : en fin de manche en ligne, une seule invite (celle du mode en ligne) et l'annonce de victoire, absente jusque-là car la simulation s'arrête à la fin de la manche ; le salon en ligne a le style des menus et se pilote au clavier, à la manette ou à la souris ; l'annonce de victoire nomme la source du vainqueur (« J2 · Ordinateur », « J1 · En ligne »). Vérifié en WebGL : deux manches en ligne sans désynchronisation, et un salon rejoint en tapant son code dans le menu (Chrome sans interface, vraies frappes).

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

**Pas vérifié par moi** : une vraie partie au clavier, à deux humains, avec la sensation de jeu. C'est le prochain essai à faire. Il n'y a pas eu d'export autonome (PC, Mac), ni d'essai WebGL sous Safari ou Firefox.

- **Jeu en ligne** : relais local (`server/relay`), build WebGL servi en statique, deux onglets d'un même Chrome sans interface, chacun en robot (`?robot`), relais réglé à 60 ms de latence par message. Deux manches complètes, revanche comprise, finies par KO au même tick des deux côtés, avec la même empreinte d'état ; environ 200 rollbacks par joueur et par manche, 45 à 49 empreintes comparées en cours de manche, aucune désynchronisation (`npm run essai:onglets`). Le relais Cloudflare a passé le même scénario de salon que le relais local, exécuté localement par `wrangler dev` ; il n'est pas déployé. Les tests EditMode du protocole (10) font jouer deux machines simulées avec latence et gigue. Deux onglets d'une même fenêtre, l'un caché, jouent aussi, mais au rythme de l'onglet caché que le navigateur ralentit à 1 ou 2 images par seconde.

Le transport de l'éditeur et du PC (`ClientWebSocket`) a échangé avec le relais local dans un test explicite (`OnlineRelayTests`, à lancer par son nom, relais démarré).

**Pas vérifié en ligne** : une partie entre deux humains, sur deux machines, à travers Internet ; le relais déployé ; un combat complet depuis l'éditeur ou un export PC.

## Hypothèses à régler en jouant

- **Timings** :
  - pied rapide, pied puissant et spécial : ceux de `moveset-prototype-v01.md` ;
  - poing : aucune donnée documentée, j'ai pris 2-3-2-3-4-6 ticks, actif sur les poses 3 et 4.
- **Napoléon** : timings de `assets/characters/napoleon/moveset-prototype-v01.md`, poing actif sur les poses 3 et 4 comme Saint Louis. Mêmes dégâts que Saint Louis ; marche plus lente (7 et 5), boulet un peu plus lent (11 par tick). Ses coups de pied ont moins d'allonge que son poing, parce qu'il recule le buste en frappant.
- **Valeurs provisoires** : dégâts, étourdissement, gel, recul, vitesses de marche (8 et 6), vitesse du projectile (12 par tick), boîtes de collision. Les boîtes sont mesurées sur les poses exportées, mais restent grossières (trois hurtboxes par pose).
- **Taille à l'écran** : garde à 500 unités, écran visible de 1080 unités de haut (le standard évoquait environ 600 px, à confirmer).

## Limites actuelles

- **Une seule manche** : pas de match en deux manches gagnantes.
- **Pas de retour au menu pendant un combat** : la pause (**Échap**) n'a pas encore de menu ; il faut finir la manche.
- **Solo et manettes en attente** : les menus sont prêts, mais l'adversaire ordinateur et la lecture des manettes en combat restent à brancher dans `InputSources` (session en cours). D'ici là, Solo est affiché « Bientôt » et une manette qui veut rejoindre est refusée avec un message.
- **Pas d'animation dédiée** :
  - recul : marche avant lue à l'envers ;
  - KO : 2ᵉ pose de réaction tenue ;
  - garde de proximité : 1ʳᵉ pose de blocage tenue.
- **Absents** : saut, accroupissement, garde basse, coups bas, chopes, annulations, étourdissement cumulé (dizzy), compteur de combo.
- **Un seul recul** : pas de recul « K.O. » ni de chute.
- **Alignement de l'export** : il cale le pied arrière (ou le buste pour la marche) mais ne conserve pas les déplacements dessinés dans les planches. Les échelles par planche gardent quelques pour cent d'écart.
- **Animations de garde et de marche jouées telles quelles** : six poses de garde quasi identiques, cycle de marche non retravaillé (voir les contrôles de l'équipe artistique).
- **Aucun son** : les signaux sont branchés, mais sans fichier.
- **Clavier** : un seul échantillon par image d'affichage, donc un appui plus court qu'une image peut être manqué. Certains claviers ne détectent pas toutes les combinaisons de touches simultanées (ghosting). Même échantillonnage par image pour la manette.
- **Adversaire de l'ordinateur** : sans saut ni accroupissement, il n'a pas de réponse à un adversaire qui ne fait que lancer des projectiles de loin (il garde sans avancer). Pas de combos.
- **Pause** : le jeu se met en pause quand la fenêtre perd le focus.
- **Jeu en ligne** : pas de reconnexion après une coupure ; un onglet caché met l'adversaire en attente ; relais pas encore déployé.
- **Pas encore faits** : import de paquets et signature (voir l'[architecture](architecture.md)).

## Besoins artistiques à nous transmettre

Classés par effet sur le jeu.

1. **Saint Louis** :
   - KO (chute au sol, 4 à 6 poses) ;
   - pose de victoire ;
   - marche arrière (8 poses) ;
   - impact de blocage ;
   - étincelle d'impact pour les coups au corps (aujourd'hui seul le projectile a un effet).
2. **Stabilisation** de la boucle de garde et du cycle de marche : appuis fixes, détails constants (fleurs de lys, couronne, épée).
3. **Décors**. Le premier, la fête foraine, est en jeu et sert par défaut ; sa musique manque (champ `music` de `stage.json`). Pour les suivants, suivre [son dossier](../assets/stages/fete-foraine/README.md). Repères (`Resources/Stages/<id>/`) :
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
5. **Interface** : une police au style du logo (aujourd'hui la police par défaut, détourée) ; le fond de menu et les mots d'annonce ont été générés avec Codex CLI et peuvent être remplacés dans `game/Assets/Resources/UI/` sans toucher au code.
6. **Pour aller vers six boutons** : poing moyen et fort, pied moyen, puis les coups accroupis et sautés si on les ajoute.
7. **Napoléon** : les mêmes besoins que Saint Louis (KO, victoire, marche arrière, impact de blocage), plus :
   - réaction haute : écarter les poses 2 et 3. La pointe de botte de la pose 2 masque le talon de la pose 3, auquel il manque donc un petit morceau ;
   - garde : six poses presque identiques, à stabiliser comme celles de Saint Louis.
8. **Combattant suivant** : suivre [Ajouter un combattant](ajouter-un-combattant.md). Laisser de l'espace entre les poses évite les coutures.
