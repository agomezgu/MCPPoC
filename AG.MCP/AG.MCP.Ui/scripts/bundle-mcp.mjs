// Post-build step for the MCP-targeted Angular build.
// Re-bundles the chunked ESM output into a single self-contained IIFE so the
// MCP App iframe (which enforces script-src 'self' 'unsafe-inline') can load
// the script inline without any cross-origin chunk requests.

import { build } from 'esbuild';
import { promises as fs } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const browserDir = path.resolve(
  __dirname,
  '..',
  '..',
  'AG.Mcp.Server',
  'wwwroot',
  'clients-ui',
  'browser'
);

const BUNDLE_FILE = 'mcp-bundle.js';

async function findMainEntry(dir) {
  const entries = await fs.readdir(dir);
  const main = entries.find(
    (name) => /^main(?:-[A-Z0-9]+)?\.js$/i.test(name)
  );
  if (!main) {
    throw new Error(
      `Could not locate main-*.js inside ${dir}. Did 'ng build --configuration mcp' run?`
    );
  }
  return path.join(dir, main);
}

async function bundle() {
  const mainPath = await findMainEntry(browserDir);
  const outFile = path.join(browserDir, BUNDLE_FILE);

  await build({
    entryPoints: [mainPath],
    bundle: true,
    format: 'iife',
    splitting: false,
    legalComments: 'none',
    target: 'es2022',
    outfile: outFile,
    allowOverwrite: true,
    logLevel: 'info'
  });

  return outFile;
}

async function cleanupOldArtifacts() {
  const entries = await fs.readdir(browserDir);
  const removable = entries.filter((name) => {
    if (name === BUNDLE_FILE) return false;
    return /^main(?:-[A-Z0-9]+)?\.js(?:\.map)?$/i.test(name)
      || /^chunk-[A-Z0-9]+\.js(?:\.map)?$/i.test(name);
  });

  await Promise.all(
    removable.map((name) =>
      fs.rm(path.join(browserDir, name), { force: true })
    )
  );

  return removable;
}

async function rewriteIndexHtml() {
  const indexPath = path.join(browserDir, 'index.html');
  let html = await fs.readFile(indexPath, 'utf8');

  html = html.replace(
    /<link\s+rel="modulepreload"[^>]*>\s*/gi,
    ''
  );

  html = html.replace(
    /<script\s+src="\/clients-ui\/browser\/main(?:-[A-Z0-9]+)?\.js"[^>]*><\/script>/gi,
    `<script src="/clients-ui/browser/${BUNDLE_FILE}"></script>`
  );

  await fs.writeFile(indexPath, html, 'utf8');
  return indexPath;
}

async function main() {
  const outFile = await bundle();
  const removed = await cleanupOldArtifacts();
  const indexPath = await rewriteIndexHtml();

  console.log('[bundle-mcp] wrote', path.relative(process.cwd(), outFile));
  console.log(
    '[bundle-mcp] removed',
    removed.length,
    'chunk/main artifacts'
  );
  console.log('[bundle-mcp] rewrote', path.relative(process.cwd(), indexPath));
}

main().catch((err) => {
  console.error('[bundle-mcp] failed:', err);
  process.exit(1);
});
