import { alphaAt, alphaBounds, type Raster } from './raster';
import { SOLID_ALPHA } from './segment';

/**
 * Repères mesurés sur une pose (en pixels de la pose extraite).
 *
 * Les planches générées placent chaque pose de façon arbitraire : il faut les
 * recaler sur un pivot commun. On ne recentre pas la silhouette (ce qui ferait
 * glisser les appuis), on suit un repère physique :
 * - `rearFoot` : le pied arrière, qui reste planté pendant la garde, les coups
 *   et les réactions ;
 * - `torso` : le buste, pour la marche où les deux pieds bougent ;
 * - `head` / `centroid` : pour les effets (tête du projectile, centre de l'impact).
 */
export type AnchorMode = 'rearFoot' | 'torso' | 'head' | 'centroid';

export interface Landmarks {
  /** Bas de la silhouette pleine (semelles). */
  readonly bottom: number;
  readonly top: number;
  /** Repère horizontal du mode choisi. */
  readonly feature: number;
  /** Milieu entre les pieds (bord gauche et droit de la bande des semelles). */
  readonly feetMidpoint: number;
  /** Repère vertical (effets uniquement). */
  readonly featureY: number;
}

export function measure(raster: Raster, mode: AnchorMode): Landmarks {
  const solid = alphaBounds(raster, SOLID_ALPHA);
  if (!solid) throw new Error('pose vide');
  const top = solid.y;
  const bottom = solid.y + solid.h;
  const feet = rowSpan(raster, bottom - Math.max(4, Math.round(solid.h * 0.05)), bottom);
  const feetMidpoint = (feet.left + feet.right) / 2;
  switch (mode) {
    case 'rearFoot':
      // Personnage tourné vers la droite : le pied arrière est le plus à gauche.
      return { bottom, top, feature: feet.left, feetMidpoint, featureY: bottom };
    case 'torso': {
      const band = centroid(raster, top + Math.round(solid.h * 0.12), top + Math.round(solid.h * 0.45));
      return { bottom, top, feature: band.x, feetMidpoint, featureY: band.y };
    }
    case 'centroid': {
      const all = centroid(raster, top, bottom);
      return { bottom, top, feature: all.x, feetMidpoint: all.x, featureY: all.y };
    }
    case 'head': {
      const head = projectileHead(raster);
      return { bottom, top, feature: head.x, feetMidpoint: head.x, featureY: head.y };
    }
  }
}

function rowSpan(raster: Raster, from: number, to: number): { left: number; right: number } {
  let left = raster.width;
  let right = -1;
  for (let y = Math.max(0, from); y < Math.min(raster.height, to); y += 1) {
    for (let x = 0; x < raster.width; x += 1) {
      if (alphaAt(raster, x, y) < SOLID_ALPHA) continue;
      left = Math.min(left, x);
      right = Math.max(right, x);
    }
  }
  return { left, right: right + 1 };
}

function centroid(raster: Raster, from: number, to: number): { x: number; y: number } {
  let sumX = 0;
  let sumY = 0;
  let count = 0;
  for (let y = Math.max(0, from); y < Math.min(raster.height, to); y += 1) {
    for (let x = 0; x < raster.width; x += 1) {
      if (alphaAt(raster, x, y) < SOLID_ALPHA) continue;
      sumX += x;
      sumY += y;
      count += 1;
    }
  }
  return count === 0 ? { x: raster.width / 2, y: raster.height / 2 } : { x: sumX / count, y: sumY / count };
}

/**
 * Tête d'un projectile qui file vers la droite : la partie la plus haute
 * (le symbole) se distingue de la traînée, plus basse. On garde les colonnes
 * dont la hauteur pleine dépasse 70 % du maximum et on prend leur centre.
 */
function projectileHead(raster: Raster): { x: number; y: number } {
  const heights: Array<{ x: number; top: number; bottom: number }> = [];
  for (let x = 0; x < raster.width; x += 1) {
    let top = -1;
    let bottom = -1;
    for (let y = 0; y < raster.height; y += 1) {
      if (alphaAt(raster, x, y) < 250) continue;
      if (top < 0) top = y;
      bottom = y;
    }
    if (top >= 0) heights.push({ x, top, bottom });
  }
  const tallest = Math.max(...heights.map((h) => h.bottom - h.top));
  const head = heights.filter((h) => h.bottom - h.top >= tallest * 0.7);
  const x = head.reduce((sum, h) => sum + h.x, 0) / head.length;
  const y = head.reduce((sum, h) => sum + (h.top + h.bottom) / 2, 0) / head.length;
  return { x, y };
}
