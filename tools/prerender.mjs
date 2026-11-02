import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const here = dirname(fileURLToPath(import.meta.url));
const root = resolve(here, "..");
const kbDir = join(root, "docs", "hotkeys");
const files = [
  "data.js",
  "data/os.js",
  "data/office.js",
  "data/communication.js",
  "data/browser.js",
  "data/design.js",
  "data/video.js",
  "data/dev.js",
  "data/tools.js",
  "data/media.js",
  "data/popular.js"
];

const sandbox = {};
for (const rel of files) {
  new Function("window", readFileSync(join(kbDir, rel), "utf8"))(sandbox);
}
const data = sandbox.HOTKEY_DATA;
if (!data || !Array.isArray(data.categories)) throw new Error("HOTKEY_DATA missing");

const es = (v) => String(v == null ? "" : v)
  .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
  .replace(/"/g, "&quot;").replace(/'/g, "&#39;");
const appId = (cat, app) => `app-${cat.id}-${app.id}`;
const grpId = (cat, app, i) => `grp-${cat.id}-${app.id}-${i}`;
const zh = data.i18n.zh;
const appCount = data.categories.reduce((n, c) => n + c.apps.length, 0);
const itemCount = data.categories.reduce((n, c) =>
  n + c.apps.reduce((m, a) => m + a.groups.reduce((k, g) => k + g.items.length, 0), 0), 0);

const lines = [
  '      <div class="prerender">',
  `        <h1>${es(zh.title)}</h1>`,
  `        <p>${es(zh.subtitle)}</p>`,
  `        <p>${data.categories.length} 个分类 · ${appCount} 款应用 · ${itemCount} 条快捷键 · 更新 ${es(data.meta.updated)}</p>`
];
for (const cat of data.categories) {
  lines.push(`        <section><h2>${es(cat.name.zh)}</h2><p>${es(cat.desc.zh)}</p>`);
  lines.push("          <ul>");
  for (const app of cat.apps) {
    lines.push(`            <li><a href="#${appId(cat, app)}">${es(app.name.zh)}</a>（${app.groups.reduce((n, g) => n + g.items.length, 0)} 条）`);
    if (app.groups.length) {
      lines.push("              <ul>");
      app.groups.forEach((g, i) => {
        lines.push(`                <li><a href="#${grpId(cat, app, i)}">${es(g.name.zh)}</a></li>`);
      });
      lines.push("              </ul>");
    }
    lines.push("            </li>");
  }
  lines.push("          </ul>");
  lines.push("        </section>");
}
lines.push("      </div>");

const snippet = lines.join("\n");
const indexPath = join(kbDir, "index.html");
const html = readFileSync(indexPath, "utf8");
const start = "      <!-- prerender:start -->";
const end = "      <!-- prerender:end -->";
const a = html.indexOf(start);
const b = html.indexOf(end);
if (a === -1 || b === -1 || b < a) throw new Error("prerender markers not found");
const next = html.slice(0, a + start.length) + "\n" + snippet + "\n" + html.slice(b);

if (process.argv.includes("--check")) {
  if (next !== html) {
    console.error("prerender: index.html is stale; run node tools/prerender.mjs");
    process.exit(1);
  }
  console.log(`prerender: ok (${data.categories.length} categories, ${appCount} apps, ${itemCount} items)`);
} else {
  writeFileSync(indexPath, next, "utf8");
  console.log(`prerender: wrote index.html (${data.categories.length} categories, ${appCount} apps, ${itemCount} items)`);
}
