import { join } from 'node:path';
import sharp, { type OverlayOptions } from 'sharp';
import type { Raster } from './raster';

interface Cell {
  width: number;
  height: number;
  pivotX: number;
  pivotY: number;
}

const SCALE = 0.4;

/**
 * Aperçus de contrôle d'une animation, pour l'équipe artistique :
 * - `<nom>_planche.png` : poses côte à côte sur fond sombre puis clair, avec la
 *   ligne de sol et l'axe du pivot (franges et alignement visibles) ;
 * - `<nom>_superposition.png` : toutes les poses superposées en transparence
 *   (dérive des pieds, sauts d'échelle).
 */
export async function writePreviews(dir: string, name: string, cells: readonly Raster[], cell: Cell): Promise<void> {
  const w = Math.round(cell.width * SCALE);
  const h = Math.round(cell.height * SCALE);
  const scaled = await Promise.all(cells.map((raster) => toPng(raster, w, h)));
  const guides = (count: number, background: string, stroke: string) => {
    const lines = Array.from({ length: count }, (_, i) => {
      const x = i * w + cell.pivotX * SCALE;
      const ground = cell.pivotY * SCALE;
      return `<line x1="${x}" y1="0" x2="${x}" y2="${h}" stroke="${stroke}" stroke-dasharray="4 4"/>` +
        `<line x1="${i * w}" y1="${ground}" x2="${(i + 1) * w}" y2="${ground}" stroke="${stroke}"/>` +
        `<rect x="${i * w}" y="0" width="${w}" height="${h}" fill="none" stroke="${stroke}" stroke-opacity="0.3"/>`;
    }).join('');
    return Buffer.from(`<svg xmlns="http://www.w3.org/2000/svg" width="${count * w}" height="${h}"><rect width="100%" height="100%" fill="${background}"/>${lines}</svg>`);
  };

  const rows = [
    { background: '#1d1d24', stroke: '#3fd0ff' },
    { background: '#ececec', stroke: '#d0306a' },
  ];
  const sheet = sharp({ create: { width: cells.length * w, height: h * rows.length, channels: 4, background: '#000000' } });
  const layers: OverlayOptions[] = [];
  rows.forEach((row, r) => {
    layers.push({ input: guides(cells.length, row.background, row.stroke), left: 0, top: r * h });
    scaled.forEach((input, i) => layers.push({ input, left: i * w, top: r * h }));
  });
  await sheet.composite(layers).png().toFile(join(dir, `${name}_planche.png`));

  const ghosts = await Promise.all(cells.map((raster) => toPng(raster, w, h, Math.max(0.2, 1.6 / cells.length))));
  await sharp({ create: { width: w, height: h, channels: 4, background: '#7a7a80' } })
    .composite([{ input: guides(1, '#7a7a80', '#ffffff'), left: 0, top: 0 }, ...ghosts.map((input) => ({ input, left: 0, top: 0 }))])
    .png()
    .toFile(join(dir, `${name}_superposition.png`));
}

async function toPng(raster: Raster, width: number, height: number, opacity = 1): Promise<Buffer> {
  const data = Buffer.from(raster.data);
  if (opacity < 1) for (let i = 3; i < data.length; i += 4) data[i] = Math.round((data[i] ?? 0) * opacity);
  return sharp(data, { raw: { width: raster.width, height: raster.height, channels: 4 } }).resize(width, height).png().toBuffer();
}
