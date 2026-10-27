import { createRaster, crop, type Raster, type Rect } from './raster';

/**
 * Découpage d'une planche source en poses.
 *
 * - `poses` : planches de personnage, générées en rangées de hauteur égale.
 *   Dans chaque rangée, les poses sont isolées par composantes connexes en 2D :
 *   cela tolère une jambe ou un poing qui déborde au-dessus de la pose voisine.
 * - `grid` : grille régulière, pour les effets dont les particules détachées
 *   rendraient la détection de composantes instable.
 */
export type Layout =
  | { readonly type: 'poses'; readonly rows: number }
  | { readonly type: 'grid'; readonly rows: number; readonly columns: number };

export interface ExtractedPose {
  /** Pixels de la pose seule, recadrés. */
  readonly raster: Raster;
  /** Position du coin haut-gauche de `raster` dans la planche source. */
  readonly origin: { readonly x: number; readonly y: number };
}

/** Alpha à partir duquel un pixel est « plein » (silhouette). */
export const SOLID_ALPHA = 128;
/** Une composante est une pose si son aire dépasse cette fraction de la plus grande. */
const SEED_RATIO = 0.15;
/** Fragment plein (pointe d'épée, mèche) rattaché à la pose la plus proche jusqu'à cette distance. */
const ATTACH_DISTANCE = 40;
/** Pixels semi-transparents conservés jusqu'à cette distance d'un pixel plein de la pose. */
const SOFT_EDGE_RADIUS = 6;
/** En dessous, l'alpha est du bruit de génération : supprimé. */
export const NOISE_ALPHA = 8;

export function extractPoses(sheet: Raster, layout: Layout, expected: number, label: string): ExtractedPose[] {
  const poses =
    layout.type === 'grid' ? gridCells(sheet, layout.rows, layout.columns) : posesByComponents(sheet, layout.rows);
  if (poses.length !== expected) {
    throw new Error(`${label} : ${poses.length} poses trouvées, ${expected} attendues (disposition ${JSON.stringify(layout)}).`);
  }
  return poses;
}

function gridCells(sheet: Raster, rows: number, columns: number): ExtractedPose[] {
  const w = Math.floor(sheet.width / columns);
  const h = Math.floor(sheet.height / rows);
  const cells: ExtractedPose[] = [];
  for (let row = 0; row < rows; row += 1) {
    for (let column = 0; column < columns; column += 1) {
      const origin = { x: column * w, y: row * h };
      const raster = crop(sheet, { ...origin, w, h });
      removeNoise(raster);
      cells.push({ raster, origin });
    }
  }
  return cells;
}

interface Component {
  area: number;
  bounds: Rect;
  pose: number;
}

function posesByComponents(sheet: Raster, rows: number): ExtractedPose[] {
  const boundaries = [0];
  for (let row = 1; row < rows; row += 1) boundaries.push(emptiestRowNear(sheet, Math.round((sheet.height * row) / rows)));
  boundaries.push(sheet.height);
  const poses: ExtractedPose[] = [];
  for (let row = 0; row < rows; row += 1) {
    const top = boundaries[row] ?? 0;
    const height = (boundaries[row + 1] ?? sheet.height) - top;
    const band = crop(sheet, { x: 0, y: top, w: sheet.width, h: height });
    poses.push(...splitBand(band).map((pose) => ({ raster: pose.raster, origin: { x: pose.origin.x, y: pose.origin.y + top } })));
  }
  return poses;
}

/**
 * Les rangées générées ne sont pas toujours séparées exactement à mi-hauteur :
 * on coupe sur la ligne la moins remplie autour de la position nominale, pour
 * ne pas trancher des semelles ou des couronnes.
 */
function emptiestRowNear(sheet: Raster, nominal: number): number {
  const searchRadius = Math.round(sheet.height * 0.08);
  let best = nominal;
  let bestCount = Number.POSITIVE_INFINITY;
  for (let offset = 0; offset <= searchRadius; offset += 1) {
    for (const y of offset === 0 ? [nominal] : [nominal - offset, nominal + offset]) {
      if (y <= 0 || y >= sheet.height) continue;
      let count = 0;
      for (let x = 0; x < sheet.width; x += 1) if ((sheet.data[(y * sheet.width + x) * 4 + 3] ?? 0) >= SOLID_ALPHA) count += 1;
      if (count < bestCount) {
        best = y;
        bestCount = count;
      }
    }
  }
  return best;
}

function splitBand(band: Raster): ExtractedPose[] {
  const { width, height } = band;
  const labels = new Int32Array(width * height).fill(-1);
  const components: Component[] = [];
  const stack: number[] = [];

  // 1. Composantes connexes (8-voisinage) des pixels pleins.
  for (let start = 0; start < width * height; start += 1) {
    if (labels[start] !== -1 || (band.data[start * 4 + 3] ?? 0) < SOLID_ALPHA) continue;
    const id = components.length;
    const component: Component = { area: 0, bounds: { x: width, y: height, w: 0, h: 0 }, pose: -1 };
    let maxX = -1;
    let maxY = -1;
    labels[start] = id;
    stack.push(start);
    while (stack.length > 0) {
      const index = stack.pop() as number;
      const x = index % width;
      const y = (index - x) / width;
      component.area += 1;
      component.bounds.x = Math.min(component.bounds.x, x);
      component.bounds.y = Math.min(component.bounds.y, y);
      maxX = Math.max(maxX, x);
      maxY = Math.max(maxY, y);
      for (let dy = -1; dy <= 1; dy += 1) {
        for (let dx = -1; dx <= 1; dx += 1) {
          const nx = x + dx;
          const ny = y + dy;
          if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
          const neighbour = ny * width + nx;
          if (labels[neighbour] !== -1 || (band.data[neighbour * 4 + 3] ?? 0) < SOLID_ALPHA) continue;
          labels[neighbour] = id;
          stack.push(neighbour);
        }
      }
    }
    component.bounds.w = maxX - component.bounds.x + 1;
    component.bounds.h = maxY - component.bounds.y + 1;
    components.push(component);
  }
  if (components.length === 0) return [];

  // 2. Les grandes composantes sont les poses, ordonnées de gauche à droite.
  const largest = Math.max(...components.map((component) => component.area));
  const seeds = components
    .filter((component) => component.area >= largest * SEED_RATIO)
    .sort((a, b) => a.bounds.x + a.bounds.w / 2 - (b.bounds.x + b.bounds.w / 2));
  seeds.forEach((seed, index) => (seed.pose = index));

  // 3. Les fragments proches rejoignent leur pose ; les autres sont des poussières.
  for (const component of components) {
    if (component.pose >= 0) continue;
    let best = -1;
    let bestDistance = ATTACH_DISTANCE + 1;
    for (const seed of seeds) {
      const distance = rectDistance(component.bounds, seed.bounds);
      if (distance < bestDistance) {
        best = seed.pose;
        bestDistance = distance;
      }
    }
    component.pose = best;
  }

  // 4. Carte des poses pour les pixels pleins, puis propagation limitée aux bords doux.
  const owner = new Int32Array(width * height).fill(-1);
  let frontier: number[] = [];
  for (let index = 0; index < width * height; index += 1) {
    const id = labels[index] ?? -1;
    if (id < 0) continue;
    const pose = components[id]?.pose ?? -1;
    if (pose < 0) continue;
    owner[index] = pose;
    frontier.push(index);
  }
  for (let radius = 1; radius <= SOFT_EDGE_RADIUS && frontier.length > 0; radius += 1) {
    const next: number[] = [];
    for (const index of frontier) {
      const x = index % width;
      const y = (index - x) / width;
      for (let dy = -1; dy <= 1; dy += 1) {
        for (let dx = -1; dx <= 1; dx += 1) {
          const nx = x + dx;
          const ny = y + dy;
          if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
          const neighbour = ny * width + nx;
          const alpha = band.data[neighbour * 4 + 3] ?? 0;
          if (owner[neighbour] !== -1 || alpha < NOISE_ALPHA || alpha >= SOLID_ALPHA) continue;
          owner[neighbour] = owner[index] ?? -1;
          next.push(neighbour);
        }
      }
    }
    frontier = next;
  }

  // 5. Une image par pose, ne contenant que ses pixels.
  return seeds.map((seed) => {
    let minX = width;
    let minY = height;
    let maxX = -1;
    let maxY = -1;
    for (let index = 0; index < width * height; index += 1) {
      if (owner[index] !== seed.pose) continue;
      const x = index % width;
      const y = (index - x) / width;
      minX = Math.min(minX, x);
      minY = Math.min(minY, y);
      maxX = Math.max(maxX, x);
      maxY = Math.max(maxY, y);
    }
    const raster = createRaster(maxX - minX + 1, maxY - minY + 1);
    for (let y = minY; y <= maxY; y += 1) {
      for (let x = minX; x <= maxX; x += 1) {
        const index = y * width + x;
        if (owner[index] !== seed.pose) continue;
        raster.data.set(band.data.subarray(index * 4, index * 4 + 4), ((y - minY) * raster.width + (x - minX)) * 4);
      }
    }
    return { raster, origin: { x: minX, y: minY } };
  });
}

function rectDistance(a: Rect, b: Rect): number {
  const dx = Math.max(0, a.x - (b.x + b.w), b.x - (a.x + a.w));
  const dy = Math.max(0, a.y - (b.y + b.h), b.y - (a.y + a.h));
  return Math.hypot(dx, dy);
}

/** Supprime le bruit d'alpha très faible et la couleur cachée sous les pixels transparents. */
export function removeNoise(raster: Raster, threshold = NOISE_ALPHA): void {
  for (let index = 0; index < raster.data.length; index += 4) {
    if ((raster.data[index + 3] ?? 0) < threshold) raster.data.fill(0, index, index + 4);
  }
}
