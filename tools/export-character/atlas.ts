import { blit, createRaster, type Raster } from './raster';

export interface PackedSprite {
  readonly name: string;
  readonly raster: Raster;
  /** Pivot dans le sprite rogné (pixels, origine en haut à gauche). */
  readonly pivotX: number;
  readonly pivotY: number;
}

export interface Placement {
  readonly name: string;
  readonly page: number;
  readonly x: number;
  readonly y: number;
  readonly w: number;
  readonly h: number;
  readonly pivotX: number;
  readonly pivotY: number;
}

/** Taille maximale d'une page, compatible avec les plateformes visées (dont WebGL). */
const MAX_PAGE = 4096;
/** Marge transparente entre sprites : évite les fuites de couleur au filtrage linéaire. */
const PADDING = 4;

/**
 * Rangement par étagères, déterministe (tri par hauteur puis par nom). Les
 * dimensions des pages sont des multiples de 4 pour la compression DXT/BC.
 */
export function packAtlas(sprites: readonly PackedSprite[]): { pages: Raster[]; placements: Placement[] } {
  const ordered = [...sprites].sort((a, b) => b.raster.height - a.raster.height || a.name.localeCompare(b.name));
  const placements: Placement[] = [];
  const pageSizes: Array<{ width: number; height: number }> = [];
  let page = -1;
  let x = 0;
  let y = 0;
  let shelfHeight = 0;
  const newPage = () => {
    page += 1;
    pageSizes.push({ width: 0, height: 0 });
    x = 0;
    y = 0;
    shelfHeight = 0;
  };
  newPage();
  for (const sprite of ordered) {
    const { width: w, height: h } = sprite.raster;
    if (w + PADDING * 2 > MAX_PAGE || h + PADDING * 2 > MAX_PAGE) throw new Error(`${sprite.name} : ${w} × ${h} px dépasse une page d'atlas`);
    if (x + w + PADDING * 2 > MAX_PAGE) {
      x = 0;
      y += shelfHeight;
      shelfHeight = 0;
    }
    if (y + h + PADDING * 2 > MAX_PAGE) newPage();
    const placement = { name: sprite.name, page, x: x + PADDING, y: y + PADDING, w, h, pivotX: sprite.pivotX, pivotY: sprite.pivotY };
    placements.push(placement);
    x += w + PADDING * 2;
    shelfHeight = Math.max(shelfHeight, h + PADDING * 2);
    const size = pageSizes[page]!;
    size.width = Math.max(size.width, x);
    size.height = Math.max(size.height, y + shelfHeight);
  }
  const pages = pageSizes.map((size) => createRaster(roundUp4(size.width), roundUp4(size.height)));
  for (const placement of placements) {
    const sprite = sprites.find((s) => s.name === placement.name)!;
    blit(pages[placement.page]!, sprite.raster, placement.x, placement.y);
  }
  placements.sort((a, b) => a.name.localeCompare(b.name));
  return { pages, placements };
}

const roundUp4 = (value: number) => Math.ceil(value / 4) * 4;
