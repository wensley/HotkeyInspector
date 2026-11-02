import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { TOPICS, SOURCES, UPDATED } from "./commands-data.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const root = resolve(here, "..");
const outDir = join(root, "docs", "hotkeys", "commands");
mkdirSync(outDir, { recursive: true });

const esc = (v) => String(v == null ? "" : v)
  .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
  .replace(/"/g, "&quot;").replace(/'/g, "&#39;");

const attr = esc;
const countRows = (topic) => topic.sections.reduce((n, s) => n + s.rows.length, 0);
const countDanger = (topic) => topic.sections.reduce((n, s) => n + s.rows.filter((r) => r.danger).length, 0);
const cleanPath = (file) => file.replace(/\.html$/, "");
const canonical = (topic) => topic ? `https://zuhejian.com/commands/${cleanPath(topic.file)}` : "https://zuhejian.com/commands/";

const filters = {
  general: ["Windows", "PowerShell", "macOS", "Linux"],
  excel: ["Windows", "macOS", "Web"],
  windows: ["PowerShell", "CMD", "WSL"],
  linux: ["Debian", "Ubuntu", "RHEL", "Fedora", "Arch", "Alpine", "SUSE"],
  docker: ["Compose", "Dockerfile"],
  macos: []
};

function head({ title, description, url, id, kind }) {
  return `<!DOCTYPE html>
<html lang="zh-CN">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self' data:; object-src 'none'; base-uri 'self'; form-action 'self'; frame-src 'none'; upgrade-insecure-requests">
  <title>${esc(title)} · 快捷键大全</title>
  <meta name="description" content="${attr(description)}">
  <meta name="robots" content="index, follow">
  <meta name="theme-color" media="(prefers-color-scheme: light)" content="#f8f8f8">
  <meta name="theme-color" media="(prefers-color-scheme: dark)" content="#0d0d0d">
  <link rel="canonical" href="${url}">
  <link rel="icon" type="image/svg+xml" href="../favicon.svg">
  <meta property="og:type" content="website">
  <meta property="og:site_name" content="快捷键大全">
  <meta property="og:locale" content="zh_CN">
  <meta property="og:url" content="${url}">
  <meta property="og:title" content="${attr(title)} · 快捷键大全">
  <meta property="og:description" content="${attr(description)}">
  <meta property="og:image" content="https://zuhejian.com/og-cover.png">
  <meta name="twitter:card" content="summary_large_image">
  <meta name="twitter:title" content="${attr(title)}">
  <meta name="twitter:description" content="${attr(description)}">
  <meta name="twitter:image" content="https://zuhejian.com/og-cover.png">
  <script src="../theme-init.js?v=1"></script>
  <link rel="stylesheet" href="../styles.css?v=9">
  <link rel="stylesheet" href="commands.css?v=7">
  <script src="commands.js?v=6" defer></script>
  <script src="../version.js?v=2" defer></script>
</head>
<body class="cmd-page" data-page="${id}" data-kind="${kind}">
  <a class="skip-link" href="#main">跳到正文</a>
  <header class="cmd-topbar">
    <div class="cmd-topbar-inner">
      <a class="cmd-brand" href="../"><span class="cmd-brand-mark" aria-hidden="true">⌘</span><span>快捷键大全</span></a>
      <nav class="cmd-nav" aria-label="主导航">
        <a href="../">快捷键</a>
        <a href="./" aria-current="page">命令大全</a>
        <a href="../tools/">开发者工具</a>
      </nav>
      <div class="cmd-tools">
        <button class="cmd-icon" id="cmd-top" type="button" aria-label="回到顶部" title="回到顶部">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M12 19V5M5 12l7-7 7 7" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg>
        </button>
        <button class="cmd-icon" id="theme-btn" type="button" aria-label="切换深色 / 浅色主题" title="切换深色 / 浅色主题">
          <svg class="cmd-theme-sun" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="4.2" fill="none" stroke="currentColor" stroke-width="1.8"/><path d="M12 2.5v2.2M12 19.3v2.2M2.5 12h2.2M19.3 12h2.2M5.1 5.1l1.6 1.6M17.3 17.3l1.6 1.6M18.9 5.1l-1.6 1.6M6.7 17.3l-1.6 1.6" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/></svg>
          <svg class="cmd-theme-moon" viewBox="0 0 24 24" aria-hidden="true"><path d="M20.5 14.3A8.5 8.5 0 1 1 9.7 3.5a6.6 6.6 0 0 0 10.8 10.8z" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linejoin="round"/></svg>
        </button>
        <a class="cmd-icon" href="https://github.com/wensley/HotkeyInspector" target="_blank" rel="noopener noreferrer" aria-label="GitHub" title="GitHub">
          <svg viewBox="0 0 24 24" aria-hidden="true"><path fill="currentColor" d="M12 .5C5.73.5.5 5.73.5 12c0 5.08 3.29 9.39 7.86 10.91.58.11.79-.25.79-.56 0-.27-.01-1.16-.02-2.11-3.2.7-3.88-1.37-3.88-1.37-.52-1.33-1.28-1.68-1.28-1.68-1.05-.72.08-.71.08-.71 1.16.08 1.77 1.19 1.77 1.19 1.03 1.77 2.7 1.26 3.36.96.1-.75.4-1.26.73-1.55-2.55-.29-5.23-1.28-5.23-5.7 0-1.26.45-2.29 1.19-3.1-.12-.29-.52-1.46.11-3.05 0 0 .97-.31 3.18 1.18a11.1 11.1 0 0 1 5.79 0c2.2-1.49 3.17-1.18 3.17-1.18.63 1.59.23 2.76.11 3.05.74.81 1.19 1.84 1.19 3.1 0 4.43-2.69 5.41-5.25 5.69.41.36.78 1.06.78 2.14 0 1.55-.01 2.8-.01 3.18 0 .31.21.68.8.56A11.51 11.51 0 0 0 23.5 12C23.5 5.73 18.27.5 12 .5z"/></svg>
        </a>
      </div>
    </div>
  </header>`;
}

function foot() {
  return `  <footer class="cmd-footer">
    <nav class="cmd-footer-links" aria-label="页脚">
      <a href="../">快捷键大全</a>
      <a href="./">命令大全</a>
      <a href="../tools/">开发者工具</a>
      <a href="../sitemap.html">站点地图</a>
      <a href="../privacy.html">隐私说明</a>
    </nav>
    <p>来源以 Microsoft Learn、GNU 手册、各发行版官方文档为主。命令行为可能随版本变化，请先在测试环境验证。</p>
    <p class="cmd-deploy">© zuhejian.com<span data-cf-version-wrap hidden> · 版本 <code data-cf-version></code></span></p>
  </footer>
</body>
</html>`;
}

function sourceCell(row) {
  const src = SOURCES[row.src];
  return `<a href="${attr(src.url)}" target="_blank" rel="noopener noreferrer">${esc(src.name)}</a>`;
}

function platformTags(row) {
  const list = row.p || ["通用"];
  if (list.length >= 7) {
    return `<span class="cmd-tag" title="${attr(list.join(" / "))}">全发行版</span>`;
  }
  if (list.length === 6 && list.indexOf("systemd") === -1 && list.indexOf("Alpine") === -1) {
    return `<span class="cmd-tag" title="${attr(list.join(" / "))}">systemd 系</span>`;
  }
  return list.map((p) => `<span class="cmd-tag">${esc(p)}</span>`).join("");
}

function rowHtml(row, topic) {
  const danger = Boolean(row.danger);
  const copy = row.syntax || row.cmd;
  return `<tr class="cmd-row${danger ? " is-danger" : ""}" data-platforms="${attr((row.p || []).join("|"))}">
  <td data-label="命令 / 语法">
    <div class="cmd-term"><code>${esc(row.cmd)}</code><button class="cmd-copy" type="button" data-copy="${attr(copy)}" aria-label="复制 ${attr(row.cmd)}">复制</button></div>
    ${row.syntax && row.syntax !== row.cmd ? `<div class="cmd-syntax"><code>${esc(row.syntax)}</code></div>` : ""}
    ${danger ? `<div class="cmd-warn"><strong>危险</strong>${esc(row.warn || "执行前确认目标并备份。")}</div>` : ""}
  </td>
  <td data-label="说明">${esc(row.desc)}</td>
  <td data-label="示例">${row.example ? `<code>${esc(row.example)}</code>` : "—"}</td>
  <td data-label="平台"><span class="cmd-tags">${platformTags(row)}</span></td>
  <td data-label="来源">${sourceCell(row)}</td>
</tr>`;
}

function topicPage(topic) {
  const total = countRows(topic);
  const danger = countDanger(topic);
  const filterList = filters[topic.id] || [];
  const options = filterList.map((f) => `<option value="${attr(f)}">${esc(f)}</option>`).join("");
  const sections = topic.sections.map((s) => `<section class="cmd-section" data-section id="${attr(s.id)}">
    <div class="cmd-section-head"><h2>${esc(s.title)}</h2><span class="cmd-section-count">${s.rows.length} 条</span></div>
    ${s.intro ? `<p class="cmd-section-intro">${esc(s.intro)}</p>` : ""}
    <div class="cmd-table-wrap">
      <table class="cmd-table">
        <caption class="cmd-caption">${esc(s.title)}命令表</caption>
        <thead><tr><th scope="col">命令 / 语法</th><th scope="col">说明</th><th scope="col">示例</th><th scope="col">平台</th><th scope="col">来源</th></tr></thead>
        <tbody>${s.rows.map((r) => rowHtml(r, topic)).join("\n")}</tbody>
      </table>
    </div>
  </section>`).join("\n");

  const toc = topic.sections.map((s) => `<a href="#${attr(s.id)}" data-toc="${attr(s.id)}"><span>${esc(s.title)}</span><span class="cmd-side-count">${s.rows.length}</span></a>`).join("");
  return `${head({
    title: topic.title,
    description: `${topic.lead} 共 ${total} 条，标注官方来源。`,
    url: canonical(topic),
    id: topic.id,
    kind: "topic"
  })}
  <main class="cmd-main" id="main">
    <nav class="cmd-breadcrumb" aria-label="面包屑"><a href="../">快捷键大全</a><span>/</span><a href="./">命令大全</a><span>/</span><span aria-current="page">${esc(topic.nav)}</span></nav>
    <header class="cmd-hero">
      <p class="cmd-eyebrow">${esc(topic.eyebrow)}</p>
      <h1>${esc(topic.title)}</h1>
      <p class="cmd-lead">${esc(topic.lead)}</p>
      <p class="cmd-meta"><span>${total} 条</span><span>${topic.sections.length} 个分区</span><span>更新 ${UPDATED}</span>${danger ? `<span class="cmd-meta-danger">${danger} 条危险操作</span>` : ""}</p>
    </header>
    <div class="cmd-callout${danger ? " is-danger" : ""}">
      <strong>${danger ? "先看安全提示" : "使用说明"}</strong>
      <p>${esc(topic.note || topic.intro)}</p>
    </div>
    <div class="cmd-search-card">
      <div class="cmd-search-row" role="search">
        <div class="cmd-search-box">
          <svg class="cmd-search-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M21 21l-4.35-4.35M11 18a7 7 0 1 1 0-14 7 7 0 0 1 0 14z" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>
          <input id="cmd-search" type="search" autocomplete="off" spellcheck="false" placeholder="搜索命令、说明或示例">
          <button class="cmd-search-clear" id="cmd-search-clear" type="button" aria-label="清空搜索" hidden>×</button>
        </div>
        <div class="cmd-actions">
          <button type="button" class="cmd-btn" data-action="print">打印 / PDF</button>
          <button type="button" class="cmd-btn" data-action="md">导出 Markdown</button>
          <button type="button" class="cmd-btn" data-action="csv">导出 CSV</button>
        </div>
      </div>
      ${filterList.length ? `<div class="cmd-platform-chips" role="group" aria-label="按平台筛选"><button class="cmd-chip is-active" type="button" data-platform="" aria-pressed="true">全部</button>${filterList.map((f) => `<button class="cmd-chip" type="button" data-platform="${attr(f)}" aria-pressed="false">${esc(f)}</button>`).join("")}</div>` : ""}
    </div>
    <div class="cmd-layout" id="cmd-layout">
      <aside class="cmd-side" id="cmd-side" aria-label="页内目录">
        <button class="cmd-side-toggle" id="cmd-side-toggle" type="button" aria-expanded="true" aria-controls="cmd-side-nav">
          <span class="cmd-side-label">目录</span>
          <svg class="cmd-side-caret" viewBox="0 0 24 24" aria-hidden="true"><path d="m6 9 6 6 6-6" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/></svg>
        </button>
        <nav class="cmd-side-nav" id="cmd-side-nav" aria-label="页内目录">${toc}</nav>
      </aside>
      <div class="cmd-content">
        <p class="cmd-count" id="cmd-count" aria-live="polite">显示 ${total} / ${total} 条</p>
        <div id="cmd-sections">${sections}</div>
        <p class="cmd-empty" id="cmd-empty" hidden>没有匹配的命令，试试更短的关键词或切换平台。</p>
        <p class="cmd-source-note">来源：Microsoft Learn、man7、GNU 手册及各发行版官方文档；更新 ${UPDATED}。</p>
      </div>
    </div>
  </main>
${foot()}`;
}

function hubPage() {
  const total = TOPICS.reduce((n, t) => n + countRows(t), 0);
  const cards = TOPICS.map((t) => `<a class="cmd-card" href="./${cleanPath(t.file)}">
    <span class="cmd-card-eyebrow">${esc(t.eyebrow)}</span>
    <h2>${esc(t.title)}</h2>
    <p>${esc(t.lead)}</p>
    <span class="cmd-card-meta">${countRows(t)} 条 · ${t.sections.length} 个分区${countDanger(t) ? ` · <b>${countDanger(t)} 条危险操作</b>` : ""}</span>
    <span class="cmd-card-go">查看 →</span>
  </a>`).join("\n");
  return `${head({
    title: "命令大全",
    description: `电脑常用命令、Excel 函数、Windows 运维、WSL 与容器、Linux、Docker、macOS 命令和手势，共 ${total} 条，标注官方来源。`,
    url: canonical(null),
    id: "hub",
    kind: "hub"
  })}
  <main class="cmd-main" id="main">
    <header class="cmd-hero cmd-hero-hub">
      <p class="cmd-eyebrow">官方来源 · 可打印 · 可导出</p>
      <h1>命令大全</h1>
      <p class="cmd-lead">把散落在官方文档里的高频命令整理成可检索、可打印、可导出的单页专题。共 ${total} 条，全部标注官方来源。</p>
      <p class="cmd-meta"><span>${TOPICS.length} 个专题</span><span>${total} 条</span><span>更新 ${UPDATED}</span></p>
    </header>
    <div class="cmd-callout">
      <strong>内容原则</strong>
      <p>以 Microsoft Learn、GNU 手册、各发行版官方文档为主；不整段抓取第三方速查表。危险命令单独警示，并建议先在虚拟机或测试环境验证。</p>
    </div>
    <div class="cmd-cards">${cards}</div>
    <section class="cmd-sources">
      <h2>使用方式</h2>
      <ul class="cmd-plain-list">
        <li>每个专题都是独立静态页，支持浏览器搜索、平台筛选和一键复制。</li>
        <li>“打印 / PDF”会隐藏导航与按钮，按 A4 输出表格；“导出 Markdown / CSV”用于二次整理。</li>
        <li>Excel 函数以 Microsoft 365 为准；旧版本可能不支持动态数组函数。</li>
        <li>Linux 包管理和防火墙命令按发行版筛选；不要混用 apt、dnf、pacman、zypper、apk。</li>
        <li>Windows 专题包含 WSL 发行版管理与 WSL 内调用容器运行时的命令，与 Docker 专题互为补充。</li>
        <li>macOS 专题把终端命令与触控板、妙控鼠标手势分开列出；手势以出厂默认值为准，可在触控板设置中改配。</li>
      </ul>
      <p class="cmd-sources-note">发现命令过时或来源失效，请在 <a href="https://github.com/wensley/HotkeyInspector/issues" target="_blank" rel="noopener noreferrer">GitHub Issues</a> 反馈。</p>
    </section>
  </main>
${foot()}`;
}

for (const topic of TOPICS) {
  for (const section of topic.sections) {
    if (!section.rows.length) throw new Error(`empty section ${topic.id}/${section.id}`);
    for (const row of section.rows) {
      if (!SOURCES[row.src]) throw new Error(`missing source ${row.src} (${topic.id}/${row.cmd})`);
      if (!row.p || !row.p.length) throw new Error(`missing platforms ${topic.id}/${row.cmd}`);
      if (row.danger && !row.warn) throw new Error(`danger without warning ${topic.id}/${row.cmd}`);
    }
  }
}

writeFileSync(join(outDir, "index.html"), hubPage(), "utf8");
for (const topic of TOPICS) {
  writeFileSync(join(outDir, topic.file), topicPage(topic), "utf8");
}
console.log(`commands: wrote ${TOPICS.length + 1} pages, ${TOPICS.reduce((n, t) => n + countRows(t), 0)} entries`);
