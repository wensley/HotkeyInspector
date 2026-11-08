// 把 docs/hotkeys 下的多文件知识库抽取为单一 hotkeys.json，供 HotkeyInspector.Core 作为嵌入资源使用。
// 用法: node tools/extract-kb.mjs
import { readFileSync, writeFileSync, mkdirSync } from "node:fs";
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
];

const sandbox = {};
for (const rel of files) {
  const code = readFileSync(join(kbDir, rel), "utf8");
  try {
    new Function("window", code)(sandbox);
  } catch (err) {
    console.error(`加载失败: ${rel}`);
    throw err;
  }
}

const data = sandbox.HOTKEY_DATA;
if (!data || !Array.isArray(data.categories)) {
  throw new Error("解析失败：window.HOTKEY_DATA.categories 不存在");
}

let appCount = 0;
let itemCount = 0;
for (const cat of data.categories) {
  for (const app of cat.apps ?? []) {
    appCount++;
    for (const grp of app.groups ?? []) itemCount += (grp.items ?? []).length;
  }
}

const out = {
  meta: data.meta ?? {},
  categories: data.categories,
};

const outDir = join(root, "src", "HotkeyInspector.Core", "Data");
mkdirSync(outDir, { recursive: true });
const outPath = join(outDir, "hotkeys.json");
writeFileSync(outPath, JSON.stringify(out), "utf8");

console.log(`已写出 ${outPath}`);
console.log(`分类 ${data.categories.length} 个 / 应用 ${appCount} 个 / 快捷键 ${itemCount} 条`);

/* ---------------- 人类可读站点地图 sitemap.html ---------------- */

const smPath = join(kbDir, "sitemap.html");
const esc = (s) =>
  String(s ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");

const smUpdated = data.meta?.updated ?? "";

const smCats = data.categories.map((c) => ({
  id: c.id,
  zh: c.name?.zh ?? c.name?.en ?? "",
  en: c.name?.en ?? c.name?.zh ?? "",
  apps: (c.apps ?? []).map((a) => ({
    href: `index.html#app-${c.id}-${a.id}`,
    zh: a.name?.zh ?? a.name?.en ?? a.id,
    en: a.name?.en ?? a.name?.zh ?? a.id,
    vzh: a.vendor?.zh ?? a.vendor?.en ?? "",
    ven: a.vendor?.en ?? a.vendor?.zh ?? "",
  })),
}));

const smCatCount = smCats.length;
const smAppCount = smCats.reduce((n, c) => n + c.apps.length, 0);

const smSections = smCats
  .map((c) => {
    const links = c.apps
      .map((a) => {
        const vendor =
          a.vzh || a.ven
            ? `<span class="sm-vendor" data-zh="${esc(a.vzh)}" data-en="${esc(a.ven)}">${esc(a.vzh)}</span>`
            : "";
        return `        <li><a href="${esc(a.href)}"><span data-zh="${esc(a.zh)}" data-en="${esc(a.en)}">${esc(a.zh)}</span>${vendor}</a></li>`;
      })
      .join("\n");
    return `    <section class="sm-cat">
      <h2 class="sm-cat-title" data-zh="${esc(c.zh)}" data-en="${esc(c.en)}">${esc(c.zh)}</h2>
      <ul class="sm-apps">
${links}
      </ul>
    </section>`;
  })
  .join("\n");

const smLeadZh = `${smCatCount} 个分类 · ${smAppCount} 款应用 · ${itemCount} 条快捷键的完整索引，点击任意应用直达其快捷键列表。`;
const smLeadEn = `Full index of ${smCatCount} categories, ${smAppCount} apps and ${itemCount} shortcuts. Click any app to open its shortcut list.`;
const smLead = `  <p class="sm-lead" data-zh="${esc(smLeadZh)}" data-en="${esc(smLeadEn)}">${esc(smLeadZh)}</p>`;
const smUpdatedLine = `  <span data-zh="更新于 ${esc(smUpdated)}" data-en="Updated ${esc(smUpdated)}">更新于 ${esc(smUpdated)}</span>`;

let smHtml = readFileSync(smPath, "utf8");
const smNl = smHtml.includes("\r\n") ? "\r\n" : "\n";

const fillRegion = (html, name, body) => {
  const begin = `<!-- SM:${name}:BEGIN -->`;
  const end = `<!-- SM:${name}:END -->`;
  const i = html.indexOf(begin);
  const j = html.indexOf(end);
  if (i < 0 || j < 0) return null;
  const indent = /[ \t]*$/.exec(html.slice(0, j))[0];
  return (
    html.slice(0, i + begin.length) +
    smNl +
    body.split("\n").join(smNl) +
    smNl +
    indent +
    html.slice(j)
  );
};

const smRegions = [
  ["LEAD", smLead],
  ["HOTKEYS", smSections],
  ["UPDATED", smUpdatedLine],
];

for (const [name, body] of smRegions) {
  const next = fillRegion(smHtml, name, body);
  if (next === null) {
    throw new Error(`sitemap.html 缺少 SM:${name} 标记，请先恢复该文件`);
  }
  smHtml = next;
}

writeFileSync(smPath, smHtml, "utf8");
console.log(`已写出 sitemap.html（分类 ${smCatCount} / 应用 ${smAppCount}）`);
