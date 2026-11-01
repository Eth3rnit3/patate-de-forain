// Règles d'un salon, communes au relais local (Node) et au relais déployé
// (Cloudflare). Le relais ne comprend rien au jeu : il admet au plus deux
// joueurs par code et transmet à l'un ce que l'autre envoie, tel quel.
//
// Messages du relais vers un joueur (texte, commençant par « # ») :
//   #bienvenue <place> <présents>   à l'entrée : place 0 ou 1, nombre de joueurs dans le salon
//   #arrivee                        l'adversaire vient d'entrer
//   #depart                         l'adversaire est parti
// Un refus ferme la connexion avec l'un des codes ci-dessous.

export const MAX_MESSAGE = 4096;
// Au-delà, la connexion est fermée : le jeu envoie une à deux dizaines de messages par seconde.
export const MAX_MESSAGES_PER_SECOND = 240;
export const CODE_PATTERN = /^[A-Z0-9]{4,12}$/;

export const Refusal = {
  invalidCode: [4000, 'code de salon invalide'],
  full: [4001, 'salon plein'],
  unknown: [4004, 'salon introuvable'],
  tooLong: [4009, 'message trop long'],
  flood: [4029, 'trop de messages'],
};

/** Code de salon d'un chemin « /salon/<code> », ou null. */
export function roomCode(pathname) {
  const match = /^\/salon\/([^/]+)\/?$/.exec(pathname);
  if (!match) return null;
  const code = decodeURIComponent(match[1]).toUpperCase();
  return CODE_PATTERN.test(code) ? code : null;
}

/**
 * Place attribuée à un joueur qui entre, ou refus.
 * @param {number[]} taken places déjà occupées
 * @param {boolean} mustExist « rejoindre » : refuse un salon vide plutôt que de l'ouvrir
 */
export function admit(taken, mustExist) {
  if (mustExist && taken.length === 0) return {refusal: Refusal.unknown};
  const free = [0, 1].find(slot => !taken.includes(slot));
  if (free === undefined) return {refusal: Refusal.full};
  return {slot: free};
}

/** Refus éventuel d'un message d'un joueur. Les messages « # » sont réservés au relais. */
export function check(text) {
  if (typeof text !== 'string') return Refusal.tooLong;
  if (text.length > MAX_MESSAGE) return Refusal.tooLong;
  return null;
}

/** Message à transmettre, ou null s'il usurpe la forme d'un message du relais. */
export function forwardable(text) {
  return text.length > 0 && text[0] !== '#' ? text : null;
}

export const welcome = (slot, present) => `#bienvenue ${slot} ${present}`;
export const arrival = '#arrivee';
export const departure = '#depart';

/** Compte les messages par seconde d'une connexion. */
export class FloodGate {
  constructor(now = () => Date.now()) {
    this.now = now;
    this.window = 0;
    this.count = 0;
  }

  /** Vrai si le message est accepté. */
  pass() {
    const second = Math.floor(this.now() / 1000);
    if (second !== this.window) {
      this.window = second;
      this.count = 0;
    }
    return ++this.count <= MAX_MESSAGES_PER_SECOND;
  }
}

/**
 * Origine autorisée ? Liste vide : toutes. Sans en-tête Origin (version PC,
 * éditeur) : accepté, seul un navigateur l'envoie, et la liste ne sert qu'à
 * empêcher d'autres sites d'utiliser le relais.
 */
export function originAllowed(origin, allowed) {
  if (!allowed || allowed.length === 0 || !origin) return true;
  return allowed.includes(origin);
}

export function parseOrigins(text) {
  return (text || '').split(',').map(s => s.trim()).filter(Boolean);
}
