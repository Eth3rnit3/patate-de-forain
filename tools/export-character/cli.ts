/**
 * Export technique d'un personnage : planches sources → poses nettoyées et
 * alignées → cellules standard (pour l'équipe artistique) → atlas + métadonnées
 * (pour le jeu Unity). Les sources ne sont jamais modifiées.
 *
 *   npm run assets:export -- tools/export-character/configs/saint-louis.json
 */
import { createHash } from 'node:crypto';
import { mkdirSync, readFileSync, readdirSync, rmSync, writeFileSync } from 'node:fs';
import { dirname, join, relative, resolve } from 'node:path';
import { measure, type AnchorMode } from './anchor';
import { packAtlas, type PackedSprite } from './atlas';
import { writePreviews } from './preview';
import { alphaBounds, blit, createRaster, crop, loadRaster, resize, savePng, type Raster } from './raster';
import { extractPoses, removeNoise, SOLID_ALPHA, type Layout } from './segment';

interface AnimationConfig {
  name: string;
  file: string;
  layout: Layout;
  frames: number;
  kind?: 'character' | 'effect';
  /** Pose (numérotée à partir de 1) en garde, dont la hauteur est ramenée à `standingHeight`. */
  referenceFrame?: number;
  /** Réutilise l'échelle d'une autre animation dessinée à la même taille. */
  scaleLike?: string;
  /**
   * Planche sans pose debout (accroupi, en l'air, au sol) : la pose de référence
   * prend la hauteur exportée d'une pose comparable d'une animation déjà exportée.
   */
  matchHeight?: { animation: string; frame: number };
  /** Échelle imposée (effets). */
  scale?: number;
  anchor: AnchorMode;
  /** Corrections manuelles, en pixels de cellule : { "3": [dx, dy] }. */
  nudge?: Record<string, [number, number]>;
}

interface ExportConfig {
  character: string;
  filePrefix: string;
  sourceDir: string;
  framesDir: string;
  previewDir: string;
  reportFile: string;
  packageDir: string;
  cell: { width: number; height: number; pivotX: number; pivotY: number };
  standingHeight: number;
  portrait?: { file: string; size: number };
  animations: AnimationConfig[];
}

interface PreparedFrame {
  animation: AnimationConfig;
  index: number;
  raster: Raster;
  /** Pivot dans `raster` (pixels, après mise à l'échelle). */
  pivotX: number;
  pivotY: number;
  sourceOrigin: { x: number; y: number };
  sourceSize: { w: number; h: number };
}

const root = resolve(dirname(new URL(import.meta.url).pathname), '../..');
const configPath = process.argv[2] ?? "";
if (!configPath) {
  console.error('Usage : npm run assets:export -- <config.json>');
  process.exit(1);
}
const config = JSON.parse(readFileSync(resolve(root, configPath), 'utf8')) as ExportConfig;
const at = (path: string) => resolve(root, path);
const sha256 = (path: string) => createHash('sha256').update(readFileSync(path)).digest('hex');
const pad = (n: number) => String(n).padStart(2, '0');

async function main(): Promise<void> {
  const report: string[] = [];
  const warnings: string[] = [];
  const scales = new Map<string, number>();
  /** Hauteur pleine de chaque pose après mise à l'échelle, par animation. */
  const heights = new Map<string, number[]>();
  const prepared: PreparedFrame[] = [];

  // 1. Extraction, nettoyage, échelle et repère de chaque pose.
  for (const animation of config.animations) {
    const sourceFile = at(join(config.sourceDir, animation.file));
    const sheet = await loadRaster(sourceFile);
    const poses = extractPoses(sheet, animation.layout, animation.frames, animation.file);
    const isEffect = animation.kind === 'effect';
    const scale = isEffect ? (animation.scale ?? 1) : resolveScale(animation, poses.map((p) => p.raster), scales, heights);
    scales.set(animation.name, scale);
    heights.set(animation.name, poses.map((p) => (alphaBounds(p.raster, SOLID_ALPHA)?.h ?? 0) * scale));

    let sheetOffset = 0;
    if (!isEffect) {
      const reference = measure(poses[(animation.referenceFrame ?? 1) - 1]!.raster, animation.anchor);
      sheetOffset = reference.feetMidpoint - reference.feature;
    }
    report.push(`\n### ${animation.name}\n`);
    report.push(`- Source : \`${animation.file}\` (${sheet.width} × ${sheet.height}, sha256 \`${sha256(sourceFile).slice(0, 16)}…\`)`);
    report.push(`- Découpage : ${JSON.stringify(animation.layout)} → ${poses.length} poses ; échelle ${scale.toFixed(4)} ; repère \`${animation.anchor}\``);

    for (const [index, pose] of poses.entries()) {
      const landmarks = measure(pose.raster, animation.anchor);
      const scaled = await resize(pose.raster, scale);
      const sx = scaled.width / pose.raster.width;
      const sy = scaled.height / pose.raster.height;
      const [nudgeX, nudgeY] = animation.nudge?.[String(index + 1)] ?? [0, 0];
      prepared.push({
        animation,
        index,
        raster: scaled,
        pivotX: (landmarks.feature + sheetOffset) * sx - nudgeX,
        pivotY: (isEffect ? landmarks.featureY : landmarks.bottom) * sy - nudgeY,
        sourceOrigin: pose.origin,
        sourceSize: { w: pose.raster.width, h: pose.raster.height },
      });
    }
  }

  // 2. Cellule commune ; agrandie uniformément si une pose dépasse (règle du standard).
  const characterFrames = prepared.filter((f) => f.animation.kind !== 'effect');
  const cell = fitCell(config.cell, characterFrames);
  if (cell.width !== config.cell.width || cell.height !== config.cell.height) {
    warnings.push(`Cellule agrandie uniformément à ${cell.width} × ${cell.height} (pivot ${cell.pivotX}, ${cell.pivotY}) : une pose dépassait ${config.cell.width} × ${config.cell.height}.`);
  }
  const effectCell = fitCell({ ...config.cell, pivotY: Math.round(config.cell.height / 2) }, prepared.filter((f) => f.animation.kind === 'effect'));

  // 3. Cellules standard pour l'équipe artistique, et sprites rognés pour l'atlas.
  const framesDir = at(config.framesDir);
  rmSync(framesDir, { recursive: true, force: true });
  mkdirSync(framesDir, { recursive: true });
  const sprites: PackedSprite[] = [];
  const cellsByAnimation = new Map<string, Raster[]>();
  for (const frame of prepared) {
    const target = frame.animation.kind === 'effect' ? effectCell : cell;
    const image = createRaster(target.width, target.height);
    const dx = Math.round(target.pivotX - frame.pivotX);
    const dy = Math.round(target.pivotY - frame.pivotY);
    blit(image, frame.raster, dx, dy);
    const name = `${frame.animation.name}_${pad(frame.index + 1)}`;
    await savePng(image, join(framesDir, `${config.filePrefix}_${name}.png`));
    cellsByAnimation.set(frame.animation.name, [...(cellsByAnimation.get(frame.animation.name) ?? []), image]);

    const trim = alphaBounds(image, 1);
    if (!trim) throw new Error(`${name} : image vide après alignement`);
    sprites.push({ name, raster: crop(image, trim), pivotX: target.pivotX - trim.x, pivotY: target.pivotY - trim.y });
    const solid = alphaBounds(frame.raster, SOLID_ALPHA);
    report.push(
      `  - \`${name}\` : pose source à (${frame.sourceOrigin.x}, ${frame.sourceOrigin.y}), ${frame.sourceSize.w} × ${frame.sourceSize.h} px` +
        ` → ${solid?.w ?? 0} × ${solid?.h ?? 0} px pleins ; décalage dans la cellule (${dx}, ${dy})`,
    );
  }

  // 4. Atlas et métadonnées pour le jeu.
  const packageDir = at(config.packageDir);
  mkdirSync(packageDir, { recursive: true });
  for (const file of readdirSync(packageDir)) {
    if (/^atlas-\d+\.png$|^atlas\.json$|^portrait\.png$/.test(file)) rmSync(join(packageDir, file));
  }
  const atlas = packAtlas(sprites);
  const pages = [];
  for (const [index, page] of atlas.pages.entries()) {
    const file = `atlas-${index}.png`;
    await savePng(page, join(packageDir, file));
    pages.push({ file, width: page.width, height: page.height, sha256: sha256(join(packageDir, file)) });
  }
  const atlasJson = {
    formatVersion: 1,
    generator: `tools/export-character (${relative(root, resolve(root, configPath))})`,
    pages,
    sprites: Object.fromEntries(atlas.placements.map((p) => [p.name, { page: p.page, x: p.x, y: p.y, w: p.w, h: p.h, pivotX: p.pivotX, pivotY: p.pivotY }])),
  };
  writeFileSync(join(packageDir, 'atlas.json'), JSON.stringify(atlasJson, null, 2) + '\n');

  if (config.portrait) await exportPortrait(config.portrait, join(packageDir, 'portrait.png'));

  // 5. Aperçus de contrôle (fonds sombre et clair, pivot et sol) et rapport.
  const previewDir = at(config.previewDir);
  rmSync(previewDir, { recursive: true, force: true });
  mkdirSync(previewDir, { recursive: true });
  for (const [name, cells] of cellsByAnimation) {
    const isEffect = config.animations.find((a) => a.name === name)?.kind === 'effect';
    await writePreviews(previewDir, name, cells, isEffect ? effectCell : cell);
  }

  writeReport(cell, effectCell, atlasJson.pages, report, warnings, prepared.length);
  console.log(`${prepared.length} poses exportées, ${pages.length} page(s) d'atlas → ${relative(root, packageDir)}`);
  for (const warning of warnings) console.warn(`Attention : ${warning}`);
}

function resolveScale(animation: AnimationConfig, poses: Raster[], scales: Map<string, number>, heights: Map<string, number[]>): number {
  if (animation.matchHeight) {
    const { animation: other, frame } = animation.matchHeight;
    const target = heights.get(other)?.[frame - 1];
    if (target === undefined) throw new Error(`${animation.name} : « matchHeight » ${other} pose ${frame} doit être exportée avant`);
    const own = poses[(animation.referenceFrame ?? 1) - 1];
    const solid = own && alphaBounds(own, SOLID_ALPHA);
    if (!solid) throw new Error(`${animation.name} : pose de référence introuvable`);
    return target / solid.h;
  }
  if (animation.scaleLike) {
    const scale = scales.get(animation.scaleLike);
    if (scale === undefined) throw new Error(`${animation.name} : « scaleLike » ${animation.scaleLike} doit être exporté avant`);
    return scale;
  }
  const reference = poses[(animation.referenceFrame ?? 1) - 1];
  const solid = reference && alphaBounds(reference, SOLID_ALPHA);
  if (!solid) throw new Error(`${animation.name} : pose de référence introuvable`);
  return config.standingHeight / solid.h;
}

function fitCell(base: ExportConfig['cell'], frames: PreparedFrame[]): ExportConfig['cell'] {
  let left = base.pivotX;
  let right = base.width - base.pivotX;
  let up = base.pivotY;
  let down = base.height - base.pivotY;
  for (const f of frames) {
    left = Math.max(left, Math.ceil(f.pivotX));
    right = Math.max(right, Math.ceil(f.raster.width - f.pivotX));
    up = Math.max(up, Math.ceil(f.pivotY));
    down = Math.max(down, Math.ceil(f.raster.height - f.pivotY));
  }
  const half = Math.ceil(Math.max(left, right) / 32) * 32;
  const top = Math.ceil(up / 32) * 32;
  return { width: Math.max(base.width, half * 2), height: Math.max(base.height, top + Math.ceil(down / 32) * 32), pivotX: Math.max(base.pivotX, half), pivotY: Math.max(base.pivotY, top) };
}

async function exportPortrait(portrait: NonNullable<ExportConfig['portrait']>, target: string): Promise<void> {
  const raster = await loadRaster(at(join(config.sourceDir, portrait.file)));
  removeNoise(raster);
  const bounds = alphaBounds(raster, 1);
  if (!bounds) throw new Error('portrait vide');
  // Le contrôle visuel a relevé une couronne trop proche du bord : on laisse une marge.
  const margin = Math.round(Math.max(bounds.w, bounds.h) * 0.06);
  const side = Math.max(bounds.w, bounds.h) + margin * 2;
  const square = createRaster(side, side);
  blit(square, crop(raster, bounds), Math.round((side - bounds.w) / 2), Math.round((side - bounds.h) / 2));
  await savePng(await resize(square, portrait.size / side), target);
}

function writeReport(
  cell: ExportConfig['cell'],
  effectCell: ExportConfig['cell'],
  pages: Array<{ file: string; width: number; height: number; sha256: string }>,
  details: string[],
  warnings: string[],
  count: number,
): void {
  const lines = [
    `# Export technique — ${config.character}`,
    '',
    `Généré par \`npm run assets:export -- ${configPath}\`. Ne pas modifier à la main : relancer l'outil.`,
    '',
    '## Résultat',
    '',
    `- ${count} poses. Cellules personnage ${cell.width} × ${cell.height}, pivot (${cell.pivotX}, ${cell.pivotY}) au sol entre les pieds ; cellules d'effet ${effectCell.width} × ${effectCell.height}, pivot au centre de l'effet (${effectCell.pivotX}, ${effectCell.pivotY}).`,
    `- Hauteur de garde ramenée à ${config.standingHeight} px (= unités monde du jeu).`,
    `- Images individuelles : \`${config.framesDir}/${config.filePrefix}_<animation>_<NN>.png\` (NN = numéro de pose, à partir de 01).`,
    `- Atlas du jeu : ${pages.map((p) => `\`${p.file}\` ${p.width} × ${p.height}`).join(', ')} dans \`${config.packageDir}\`.`,
    `- Aperçus : \`${config.previewDir}\` (planche sur fonds sombre et clair avec pivot et sol, superposition des poses).`,
    '',
    '## Traitements appliqués',
    '',
    '- Séparation des poses par composantes connexes (alpha ≥ 128) dans chaque rangée ; rangées coupées sur la ligne la plus vide ; fragments proches (pointe d’épée, mèches) rattachés à leur pose.',
    '- Nettoyage : pixels d’alpha < 8 supprimés, couleur cachée sous les pixels transparents remise à zéro, poussières isolées écartées. Les bords anticrénelés sont conservés.',
    '- Alignement : bas de la silhouette sur le sol ; horizontalement sur le pied arrière (garde, coups, réactions) ou le buste (marche), décalé pour que le pivot tombe entre les pieds de la pose de référence.',
    '- Mise à l’échelle Lanczos (alpha prémultiplié) ; une échelle par planche, calculée sur sa pose de garde.',
    '',
    '## Limites connues',
    '',
    '- L’alignement automatique ne conserve pas les déplacements intentionnels dessinés dans les planches (ex. avancée du corps pendant la marche) : la translation vient du moteur.',
    '- Les échelles par planche reposent sur la hauteur de la pose de garde : des écarts de quelques pour cent subsistent entre planches.',
    '',
    ...(warnings.length ? ['## Avertissements', '', ...warnings.map((w) => `- ${w}`), ''] : []),
    '## Détail par animation',
    ...details,
    '',
  ];
  mkdirSync(dirname(at(config.reportFile)), { recursive: true });
  writeFileSync(at(config.reportFile), lines.join('\n'));
}

await main();
