# Relais en ligne

Le combat en ligne de Patate de Forain n'a pas de serveur de jeu : chaque joueur simule toute la manche, et seules les commandes circulent (voir [docs/architecture.md](../../docs/architecture.md), section « Jeu en ligne »). Ce relais ne fait que réunir deux joueurs par **code de salon** et transmettre à l'un, tels quels, les messages de l'autre. Il ne connaît ni les règles, ni les combattants, ni l'état du combat.

Deux versions, mêmes règles (`src/rooms.js`) :

| | Fichier | Usage |
|---|---|---|
| Local | `src/local.js` (Node + `ws`) | Développement, essais, tests |
| Déployé | `src/worker.js` (Cloudflare Worker + Durable Objects) | Jeu publié sur GitHub Pages ou PC |

## Protocole

Connexion WebSocket à `/salon/<CODE>` (4 à 12 lettres ou chiffres, majuscules ou non). Ajouter `?rejoindre` refuse un salon vide au lieu de l'ouvrir.

| Du relais vers le joueur | Sens |
|---|---|
| `#bienvenue <place> <présents>` | Entrée acceptée : place 0 (ouvre le salon, joueur 1) ou 1 |
| `#arrivee` | L'adversaire vient d'entrer |
| `#depart` | L'adversaire est parti |

Tout autre message texte d'un joueur est transmis à l'autre. Un message qui commence par `#` est ignoré (il imiterait le relais). Refus par fermeture de la connexion : 4000 code invalide, 4001 salon plein, 4004 salon introuvable, 4009 message trop long (plus de 4 096 caractères), 4029 plus de 240 messages par seconde.

Les messages du jeu (salut, choix du combattant, commandes par tick, empreintes d'état, revanche) sont décrits dans `game/Assets/Scripts/Application/OnlineLink.cs`.

## Lancer le relais local

```bash
cd server/relay
npm install
npm start
```

Il écoute sur `ws://localhost:8787` (variable `PORT` pour changer). C'est le relais par défaut de l'éditeur Unity et d'une version WebGL servie depuis `localhost`.

### Deux joueurs sur une même machine

1. Construire la version navigateur (voir [docs/etat-du-prototype.md](../../docs/etat-du-prototype.md)) et la servir : `python3 -m http.server 8000 --directory game/WebBuild`.
2. Ouvrir deux **fenêtres** (pas deux onglets d'une même fenêtre : un onglet caché est mis en pause par le navigateur, et l'adversaire attend) :
   - `http://localhost:8000/?salon=ESSAI&creer`
   - `http://localhost:8000/?salon=ESSAI`
3. Choisir un combattant de chaque côté, puis **Prêt**.

Paramètres de la page (ou de la ligne de commande sur PC : `-salon ESSAI -creer -robot -relais ws://…`) :

| Paramètre | Effet |
|---|---|
| `salon=CODE` | Ouvre directement le salon en ligne et y entre |
| `creer` | Ouvre le salon au lieu de le rejoindre |
| `relais=ws://…` | Relais à utiliser (sinon : dernier saisi, puis déployé, puis local) |
| `robot` | Le joueur local joue seul et se déclare prêt : vérification sans personne au clavier |

Avec `robot` des deux côtés, chaque fenêtre écrit en fin de manche dans la console du navigateur : `En ligne, salon ESSAI, manche 1 : finie au tick …, empreinte …, aucune désynchronisation.` Les deux empreintes doivent être identiques.

Pour essayer avec de la latence (et voir le rollback travailler) : `RETARD_MS=60 npm start` retarde de 60 ms chaque message transmis.

## Tests

```bash
npm test                                   # règles du salon et scénario complet contre un relais local éphémère
npm run verifier -- ws://localhost:8787    # même scénario contre un relais en marche, local ou déployé
npm run dev:cloudflare                     # version Cloudflare exécutée localement (workerd), sans compte, sur le port 8787
npm run essai:onglets -- http://localhost:8000/   # deux onglets de Chrome jouent deux manches en robots, revanche comprise
```

`essai:onglets` pilote Google Chrome sans interface (variable `CHROME` pour un autre emplacement) contre le build WebGL servi et le relais lancé. Il réussit si les deux onglets finissent chaque manche sur la même empreinte, sans désynchronisation. Résultat au 28 octobre 2026, relais local à 60 ms : deux manches finies par KO (ticks 1481 et 1376), environ 200 rollbacks par joueur et par manche, 49 et 45 empreintes comparées, mêmes empreintes finales.

## Déployer sur Cloudflare (offre gratuite)

Rien n'est déployé pour l'instant. Il faut un compte Cloudflare (gratuit, sans carte bancaire pour l'offre Workers Free).

1. `cd server/relay && npm install`
2. `npx wrangler login` : ouvre le navigateur pour autoriser Wrangler sur le compte.
3. Dans `wrangler.toml`, restreindre `ORIGINES` aux pages du jeu, par exemple `"https://<utilisateur>.github.io,http://localhost:8000"`. Vide, toute page peut utiliser le relais.
4. `npm run deploy` : affiche l'adresse, du genre `https://patate-relais.<sous-domaine>.workers.dev`.
5. Vérifier : `npm run verifier -- wss://patate-relais.<sous-domaine>.workers.dev`
6. Inscrire `wss://patate-relais.<sous-domaine>.workers.dev` dans `OnlineSettings.DeployedRelay` (`game/Assets/Scripts/Unity/OnlineRelay.cs`) et reconstruire le jeu. En attendant, `?relais=wss://…` dans l'adresse de la page suffit.

Le Worker oriente chaque connexion vers le Durable Object de son salon (un par code, à stockage SQLite, seul type disponible dans l'offre gratuite). L'API d'hibernation évite de payer la durée d'un salon où personne ne parle.

### Ce que permet l'offre gratuite

Limites quotidiennes de Cloudflare pour les Durable Objects (vérifiées le 28 octobre 2026 sur [la page des tarifs](https://developers.cloudflare.com/durable-objects/platform/pricing/)) : 100 000 requêtes et 13 000 Go·s de durée par jour, remises à zéro à minuit UTC. Un message WebSocket reçu compte pour 1/20 de requête ; les messages envoyés par le relais ne comptent pas.

Un joueur envoie environ 33 messages par seconde (commandes par lots de deux ticks, empreintes, mesure de latence). Une manche complète (99 s) coûte donc de l'ordre de 350 requêtes et 13 Go·s : **environ 250 manches par jour**, limite atteinte d'abord par les requêtes. Au-delà, les connexions échouent jusqu'au lendemain ; l'offre payante (5 $ par mois) lève ces limites.

## Alternative sans serveur

Action Legends n'a pas de relais : les navigateurs se trouvent par des relais Nostr publics et se parlent en pair-à-pair WebRTC (trystero). Rien à déployer ni à surveiller, et la latence est plus faible quand la connexion directe réussit. En revanche, la connexion échoue derrière certains réseaux (NAT symétrique, réseaux d'entreprise) faute de serveur TURN, elle dépend de relais publics tiers, et le PC n'a pas de WebRTC sans bibliothèque supplémentaire. Le protocole du jeu ne dépend que de `IRelaySocket` : changer de transport ne toucherait pas au combat en ligne.
