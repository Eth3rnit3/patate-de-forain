import sharp from 'sharp';

/** Image RGBA 8 bits non prémultipliée, en mémoire. */
export interface Raster {
  readonly width: number;
  readonly height: number;
  readonly data: Uint8ClampedArray;
}

export interface Rect {
  x: number;
  y: number;
  w: number;
  h: number;
}

export async function loadRaster(file: string): Promise<Raster> {
  const { data, info } = await sharp(file).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  return { width: info.width, height: info.height, data: new Uint8ClampedArray(data.buffer, data.byteOffset, data.length) };
}

export function createRaster(width: number, height: number): Raster {
  return { width, height, data: new Uint8ClampedArray(width * height * 4) };
}

export function alphaAt(raster: Raster, x: number, y: number): number {
  return raster.data[(y * raster.width + x) * 4 + 3] ?? 0;
}

export function crop(raster: Raster, rect: Rect): Raster {
  const out = createRaster(rect.w, rect.h);
  for (let y = 0; y < rect.h; y += 1) {
    const sy = rect.y + y;
    if (sy < 0 || sy >= raster.height) continue;
    for (let x = 0; x < rect.w; x += 1) {
      const sx = rect.x + x;
      if (sx < 0 || sx >= raster.width) continue;
      const from = (sy * raster.width + sx) * 4;
      out.data.set(raster.data.subarray(from, from + 4), (y * rect.w + x) * 4);
    }
  }
  return out;
}

/** Copie `source` dans `target` à (dx, dy), sans mélange (cible supposée vide). */
export function blit(target: Raster, source: Raster, dx: number, dy: number): void {
  for (let y = 0; y < source.height; y += 1) {
    const ty = y + dy;
    if (ty < 0 || ty >= target.height) continue;
    for (let x = 0; x < source.width; x += 1) {
      const tx = x + dx;
      if (tx < 0 || tx >= target.width) continue;
      const from = (y * source.width + x) * 4;
      if ((source.data[from + 3] ?? 0) === 0) continue;
      target.data.set(source.data.subarray(from, from + 4), (ty * target.width + tx) * 4);
    }
  }
}

/** Boîte englobante des pixels d'alpha ≥ seuil, ou undefined si aucun. */
export function alphaBounds(raster: Raster, threshold: number): Rect | undefined {
  let minX = raster.width;
  let minY = raster.height;
  let maxX = -1;
  let maxY = -1;
  for (let y = 0; y < raster.height; y += 1) {
    for (let x = 0; x < raster.width; x += 1) {
      if (alphaAt(raster, x, y) < threshold) continue;
      if (x < minX) minX = x;
      if (x > maxX) maxX = x;
      if (y < minY) minY = y;
      if (y > maxY) maxY = y;
    }
  }
  return maxX < 0 ? undefined : { x: minX, y: minY, w: maxX - minX + 1, h: maxY - minY + 1 };
}

/** Redimensionnement haute qualité ; sharp prémultiplie l'alpha pendant l'opération. */
export async function resize(raster: Raster, scale: number): Promise<Raster> {
  if (scale === 1) return raster;
  const width = Math.max(1, Math.round(raster.width * scale));
  const height = Math.max(1, Math.round(raster.height * scale));
  const { data, info } = await sharp(Buffer.from(raster.data.buffer, raster.data.byteOffset, raster.data.length), {
    raw: { width: raster.width, height: raster.height, channels: 4 },
  })
    .resize(width, height, { kernel: 'lanczos3', fit: 'fill' })
    .raw()
    .toBuffer({ resolveWithObject: true });
  return { width: info.width, height: info.height, data: new Uint8ClampedArray(data.buffer, data.byteOffset, data.length) };
}

export async function savePng(raster: Raster, file: string): Promise<void> {
  await sharp(Buffer.from(raster.data.buffer, raster.data.byteOffset, raster.data.length), {
    raw: { width: raster.width, height: raster.height, channels: 4 },
  })
    .png({ compressionLevel: 9, adaptiveFiltering: true })
    .toFile(file);
}
