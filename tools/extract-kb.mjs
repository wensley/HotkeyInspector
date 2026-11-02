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

const SITE = "https://zuhejian.com/";
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

const smL = {
  home: { zh: "返回首页", en: "Back to home" },
  title: { zh: "站点地图", en: "Site Map" },
  lead: {
    zh: `${smCatCount} 个分类 · ${smAppCount} 款应用 · ${itemCount} 条快捷键的完整索引，点击任意应用直达其快捷键列表。`,
    en: `Full index of ${smCatCount} categories, ${smAppCount} apps and ${itemCount} shortcuts. Click any app to open its shortcut list.`,
  },
  updated: { zh: "更新于", en: "Updated" },
};

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

const smHtml = `<!DOCTYPE html>
<html lang="zh">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; object-src 'none'; base-uri 'self'; form-action 'self'">
<title>${esc(smL.title.zh)} · 快捷键大全</title>
<meta name="description" content="快捷键大全站点地图：${smCatCount} 个分类、${smAppCount} 款应用的快捷键索引。">
<meta name="robots" content="index,follow">
<link rel="canonical" href="${SITE}sitemap.html">
<link rel="alternate" hreflang="zh-CN" href="${SITE}sitemap.html">
<link rel="alternate" hreflang="en" href="${SITE}sitemap.html?lang=en">
<script src="theme-init.js?v=1"></script>
<link rel="stylesheet" href="styles.css?v=6">
<style>
  body { padding-top: 0; }
  .sm-header {
    position: sticky; top: 0; z-index: 50;
    display: flex; align-items: center; gap: 16px;
    height: var(--header-h); padding: 0 var(--gutter);
    background: color-mix(in srgb, var(--bg) 82%, transparent);
    backdrop-filter: blur(12px);
    border-bottom: 1px solid var(--border);
  }
  .sm-brand { font-weight: 800; font-size: 16px; color: var(--text); text-decoration: none; }
  .sm-spacer { flex: 1; }
  .sm-link { color: var(--text-2); text-decoration: none; font-size: 13px; }
  .sm-link:hover { color: var(--text); }
  .sm-lang { display: flex; border: 1px solid var(--border-2); border-radius: 8px; overflow: hidden; }
  .sm-lang button {
    border: 0; background: transparent; color: var(--text-2);
    padding: 5px 10px; font-size: 12px; cursor: pointer; font-family: inherit;
  }
  .sm-lang button.active { background: var(--accent); color: #fff; }
  .sm-main { max-width: var(--max-w); margin: 0 auto; padding: 40px var(--gutter) 72px; }
  .sm-main h1 { margin: 0 0 8px; font-size: 26px; }
  .sm-lead { margin: 0 0 28px; color: var(--text-2); font-size: 14px; }
  .sm-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(230px, 1fr));
    gap: 20px 28px;
  }
  .sm-cat-title { margin: 0 0 10px; font-size: 14px; color: var(--accent); }
  .sm-apps { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 7px; }
  .sm-apps a { display: flex; align-items: baseline; gap: 7px; font-size: 14px; color: var(--text); text-decoration: none; }
  .sm-apps a:hover { color: var(--accent); text-decoration: underline; }
  .sm-vendor { color: var(--text-3); font-size: 12px; }
</style>
</head>
<body>
<header class="sm-header">
  <a class="sm-brand" href="index.html">快捷键大全</a>
  <span class="sm-spacer"></span>
  <a class="sm-link" href="index.html" data-zh="${esc(smL.home.zh)}" data-en="${esc(smL.home.en)}">${esc(smL.home.zh)}</a>
  <span class="sm-lang" role="group" aria-label="Language">
    <button type="button" data-lang="zh">中</button>
    <button type="button" data-lang="en">EN</button>
  </span>
</header>
<main class="sm-main">
  <h1 data-zh="${esc(smL.title.zh)}" data-en="${esc(smL.title.en)}">${esc(smL.title.zh)}</h1>
  <p class="sm-lead" data-zh="${esc(smL.lead.zh)}" data-en="${esc(smL.lead.en)}">${esc(smL.lead.zh)}</p>
  <div class="sm-grid">
${smSections}
  </div>
</main>
<footer class="footer">
  <span data-zh="${esc(smL.updated.zh + " " + smUpdated)}" data-en="${esc(smL.updated.en + " " + smUpdated)}">${esc(smL.updated.zh)} ${esc(smUpdated)}</span>
  <span class="footer-copy">© zuhejian.com</span>
</footer>
<script>
(function () {
  var zhTitle = ${JSON.stringify(smL.title.zh + " · 快捷键大全")};
  var enTitle = ${JSON.stringify(smL.title.en + " · Hotkey Knowledge Base")};
  function apply(l) {
    document.documentElement.lang = l;
    document.querySelectorAll("[data-zh]").forEach(function (el) {
      var v = el.getAttribute(l === "en" ? "data-en" : "data-zh");
      if (v !== null) el.textContent = v;
    });
    document.title = l === "en" ? enTitle : zhTitle;
    document.querySelectorAll(".sm-lang button").forEach(function (b) {
      b.classList.toggle("active", b.dataset.lang === l);
    });
  }
  var q = new URLSearchParams(location.search).get("lang");
  var saved = localStorage.getItem("hk-lang");
  apply(q === "en" || q === "zh" ? q : saved === "en" ? "en" : "zh");
  document.querySelectorAll(".sm-lang button").forEach(function (b) {
    b.addEventListener("click", function () {
      localStorage.setItem("hk-lang", b.dataset.lang);
      apply(b.dataset.lang);
    });
  });
})();
</script>
</body>
</html>
`;

writeFileSync(join(kbDir, "sitemap.html"), smHtml, "utf8");
console.log(`已写出 sitemap.html（分类 ${smCatCount} / 应用 ${smAppCount}）`);
