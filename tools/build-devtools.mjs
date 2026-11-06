import { mkdirSync, writeFileSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { CATEGORIES, TOOLS, HTTP_STATUS, UPDATED } from "./devtools-data.mjs";

const here = dirname(fileURLToPath(import.meta.url));
const root = resolve(here, "..");
const outDir = join(root, "docs", "hotkeys", "tools");
mkdirSync(outDir, { recursive: true });

const esc = (v) => String(v == null ? "" : v)
  .replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;")
  .replace(/"/g, "&quot;").replace(/'/g, "&#39;");
const attr = esc;
const canonical = (tool) => tool ? `https://zuhejian.com/tools/${tool.id}` : "https://zuhejian.com/tools/";
const catName = (id) => (CATEGORIES.find((c) => c.id === id) || { name: id }).name;

const LIB_FILES = {
  yaml: "vendor/yaml.js",
  sql: "vendor/sql.js",
  toml: "vendor/toml.js",
  qrcode: "vendor/qrcode.js",
  gifuct: "vendor/gifuct.js",
  mediabunny: "vendor/mediabunny.js",
  marked: "vendor/marked.js",
  purify: "vendor/purify.js",
  bcryptjs: "vendor/bcryptjs.js",
  jsonpath: "vendor/jsonpath.js",
  terser: "vendor/terser.js",
  gifenc: "vendor/gifenc.js",
  heic2any: "vendor/heic2any.js",
  beautify: "vendor/beautify-bundle.js",
  jsonrepair: "vendor/jsonrepair.js",
  fflate: "vendor/fflate.js",
  brotli: "vendor/brotli/brotli.js",
  zstd: "vendor/zstd/zstd.js"
};

/* ESM 模块库需要用 type="module" 加载 */
const ESM_LIBS = new Set(["brotli", "zstd"]);

/* 第三方依赖的出处与许可证，用于在工具页如实标注 */
const LIB_META = {
  yaml: ["yaml", "ISC"],
  sql: ["sql-formatter", "MIT"],
  toml: ["iarna-toml", "ISC"],
  qrcode: ["qrcode", "MIT"],
  gifuct: ["gifuct-js", "MIT"],
  mediabunny: ["mediabunny", "MPL-2.0"],
  marked: ["marked", "MIT"],
  purify: ["DOMPurify", "MPL-2.0"],
  bcryptjs: ["bcryptjs", "BSD-3-Clause"],
  jsonpath: ["jsonpath-plus", "MIT"],
  terser: ["terser", "BSD-2-Clause"],
  gifenc: ["gifenc", "MIT"],
  heic2any: ["heic2any", "MIT"],
  beautify: ["js-beautify", "MIT"],
  jsonrepair: ["jsonrepair", "ISC"],
  fflate: ["fflate", "MIT"],
  brotli: ["brotli-wasm", "Apache-2.0"],
  zstd: ["zstd-wasm", "MIT"]
};

const COMMAND_LINKS = {
  color: [{ href: "../commands/general", label: "命令大全 · 电脑常用" }],
  encode: [{ href: "../commands/general#http", label: "命令大全 · HTTP 与证书" }],
  format: [{ href: "../commands/general#verify", label: "命令大全 · 校验与传输" }],
  text: [{ href: "../commands/linux#scripting", label: "命令大全 · Shell 脚本进阶" }],
  css: [{ href: "../commands/general", label: "命令大全 · 电脑常用" }],
  security: [
    { href: "../commands/general#verify", label: "命令大全 · 校验与传输" },
    { href: "../commands/windows#accounts", label: "命令大全 · Windows 账户与加密" }
  ],
  debug: [{ href: "../commands/linux#logs", label: "命令大全 · 日志与故障排查" }],
  generate: [{ href: "../commands/general#files", label: "命令大全 · 文件与压缩" }],
  ai: [{ href: "../commands/general", label: "命令大全 · 电脑常用" }],
  number: [{ href: "../commands/general#system", label: "命令大全 · 系统信息" }],
  image: [{ href: "../commands/general#files", label: "命令大全 · 文件与压缩" }],
  video: [{ href: "../commands/linux#verify", label: "命令大全 · 校验与归档" }],
  network: [
    { href: "../commands/linux#network-adv", label: "命令大全 · Linux 网络进阶" },
    { href: "../commands/general#http", label: "命令大全 · HTTP 与证书" }
  ],
  reference: [{ href: "../commands/general", label: "命令大全 · 电脑常用" }]
};

function head({ title, description, url, page, tool }) {
  const connect = tool && tool.id === "file-drop"
    ? "connect-src 'self' blob: https://filebox.zhcn.xyz"
    : "connect-src 'self' blob:";
  // meta 预览类工具要显示任意站点的分享图
  const imgSrc = tool && tool.id.indexOf("meta-tag-") === 0
    ? "img-src 'self' data: blob: https:;"
    : "img-src 'self' data: blob:;";
  return `<!DOCTYPE html>
<html lang="zh-CN">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; worker-src 'self' blob:; style-src 'self' 'unsafe-inline'; ${imgSrc} font-src 'self' data:; media-src 'self' blob:; ${connect}; object-src 'none'; base-uri 'self'; form-action 'self'; frame-src 'none'; upgrade-insecure-requests">
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
  <link rel="stylesheet" href="../commands/commands.css?v=7">
  <link rel="stylesheet" href="tools.css?v=55">
${(tool ? [...new Set([...(tool.libs || []), ...((tool.ui && tool.ui.libs) || [])])] : []).map((lib) => ESM_LIBS.has(lib) ? `  <script src="${LIB_FILES[lib]}?v=3" type="module"></script>` : `  <script src="${LIB_FILES[lib]}?v=3" defer></script>`).join("\n")}
  <script src="tools.js?v=98" defer></script>
  <script src="../version.js?v=2" defer></script>
</head>
<body class="cmd-page tool-page" data-page="${attr(page)}"${tool ? ` data-tool="${attr(tool.id)}"` : ""}>
  <a class="skip-link" href="#main">跳到正文</a>
  <header class="cmd-topbar">
    <div class="cmd-topbar-inner">
      <a class="cmd-brand" href="../"><span class="cmd-brand-mark" aria-hidden="true">⌘</span><span>快捷键大全</span></a>
      <nav class="cmd-nav" aria-label="主导航">
        <a href="../">快捷键</a>
        <a href="../commands/">命令大全</a>
        <a href="./" aria-current="page">开发者工具</a>
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
  </header>
`;
}

function foot() {
  return `  <footer class="cmd-footer">
    <nav class="cmd-footer-links" aria-label="页脚">
      <a href="../">快捷键大全</a>
      <a href="../commands/">命令大全</a>
      <a href="./">开发者工具</a>
      <a href="../sitemap.html">站点地图</a>
      <a href="../privacy.html">隐私说明</a>
    </nav>
    <p>所有工具默认在浏览器本地运行。图片、视频和文件不会上传；只有 WebRTC 检测会连接你指定的 STUN 服务器。</p>
    <p class="cmd-deploy">© zuhejian.com<span data-cf-version-wrap hidden> · 版本 <code data-cf-version></code></span></p>
  </footer>
</body>
</html>`;
}

const field = (label, inner) => `<div class="tool-field"><span>${esc(label)}</span>${inner}</div>`;

/* 一行分段选择器（不经过 select，直接可点即生效） */
const seg = (key, options, value) =>
  `<div class="tool-seg" data-seg="${attr(key)}"><input type="hidden" data-opt="${attr(key)}" value="${attr(value)}">${options
    .map((o) => `<button class="tw-seg-btn" type="button" data-seg-val="${attr(o)}" aria-pressed="${o === value}">${esc(o)}</button>`)
    .join("")}</div>`;

// 预览器默认填充本站真实 head，打开就有内容可看
const SITE_HEAD = `<title>快捷键大全：Windows/Office/Photoshop/微信等 60+ 款应用快捷键</title>
<meta name="description" content="60+ 款常用软件的快捷键大全，中英双语对照，支持输入任意组合键反查功能与冲突提示，并识别浏览器与系统保留键。">
<meta name="viewport" content="width=device-width, initial-scale=1">
<link rel="canonical" href="https://zuhejian.com/">
<link rel="icon" type="image/svg+xml" href="https://zuhejian.com/favicon.svg">
<meta name="robots" content="index, follow">
<meta property="og:type" content="website">
<meta property="og:site_name" content="快捷键大全">
<meta property="og:url" content="https://zuhejian.com/">
<meta property="og:title" content="快捷键大全：Windows/Office/Photoshop/微信等 60+ 款应用快捷键">
<meta property="og:description" content="60+ 款常用软件的快捷键大全，中英双语对照，支持输入任意组合键反查功能与冲突提示，并识别浏览器与系统保留键。">
<meta property="og:image" content="https://zuhejian.com/og-cover.png">
<meta name="twitter:card" content="summary_large_image">
<meta name="twitter:title" content="快捷键大全：Windows/Office/Photoshop/微信等 60+ 款应用快捷键">
<meta name="twitter:description" content="60+ 款常用软件的快捷键大全，中英双语对照，支持输入任意组合键反查功能与冲突提示，并识别浏览器与系统保留键。">
<meta name="twitter:image" content="https://zuhejian.com/og-cover.png">`;

function optHtml(opt) {
  const key = `data-opt="${attr(opt.key)}"`;
  if (opt.k === "checkbox") {
    return `<label class="tool-check"><input type="checkbox" ${key}${opt.checked ? " checked" : ""}> ${esc(opt.label)}</label>`;
  }
  if (opt.k === "select") {
    return field(opt.label, `<select ${key}>${opt.options.map((o) => `<option${o === opt.value ? " selected" : ""}>${esc(o)}</option>`).join("")}</select>`);
  }
  if (opt.k === "number") {
    return field(opt.label, `<input type="number" ${key} value="${attr(opt.value)}" min="${attr(opt.min)}" max="${attr(opt.max)}" step="${attr(opt.step || 1)}">`);
  }
  if (opt.k === "color") {
    return field(opt.label, `<input type="color" ${key} value="${attr(opt.value)}">`);
  }
  if (opt.k === "file") {
    return field(opt.label, `<input type="file" ${key} accept="${attr(opt.accept || "")}">`);
  }
  if (opt.k === "textarea") {
    return field(opt.label, `<textarea ${key} rows="${opt.rows || 5}" spellcheck="false">${esc(opt.value || "")}</textarea>`);
  }
  return field(opt.label, `<input type="${attr(opt.type || "text")}" ${key} value="${attr(opt.value || "")}" placeholder="${attr(opt.ph || "")}">`);
}

// 本站真实信息：示例数据、默认值统一用它，避免出现 example.com 式的占位
const SITE = {
  name: "快捷键大全",
  home: "https://zuhejian.com/",
  tools: "https://zuhejian.com/tools/",
  title: "开发者工具 · 快捷键大全",
  commandTitle: "命令大全 · 快捷键大全",
  desc: "105 个开发与调试时最常打开的小工具：JSON、Base64、颜色、时间戳、正则、哈希、图片与视频，全部在浏览器本地运行。",
  commandDesc: "778 条 Windows、Linux、Docker、macOS 与 Excel 命令，按场景分类，可搜索、可复制。",
  image: "https://zuhejian.com/og-cover.png"
};

// 示例输入：统一用本站内容，避免每页都塞一段无意义的假数据
const SAMPLES = {
  "json-formatter": '{\n  "site": "zuhejian.com",\n  "name": "快捷键大全",\n  "sections": ["快捷键", "命令大全", "开发者工具"],\n  "tools": 105\n}',
  "json-validator": '{"site":"zuhejian.com","tools":105,"local":true,"upload":false}',
  "json-to-csv": '[{"name":"JSON 格式化","cat":"格式化","local":true},{"name":"Base64 编解码","cat":"编码","local":true}]',
  "csv-to-json": "name,cat,count\n快捷键,知识库,3056\n命令大全,命令,778\n开发者工具,工具,105",
  base64: "快捷键大全 · zuhejian.com",
  "url-encoder": "https://zuhejian.com/tools/?q=开发者工具&lang=zh",
  "html-entities": '<a href="https://zuhejian.com/tools/">开发者工具</a> & 快捷键',
  "json-escape": '{"site":"zuhejian.com","desc":"快捷键 \\"大全\\"","path":"C:\\\\tools"}',
  "xml-formatter": '<site><name>快捷键大全</name><tools count="105">开发者工具</tools><commands>778</commands></site>',
  "json-yaml": '{"site":"zuhejian.com","name":"快捷键大全","tools":105,"local":true}',
  "yaml-validator": "site: zuhejian.com\nname: 快捷键大全\ntools: 105\nlocal: true",
  "toml-to-json": '[site]\nname = "快捷键大全"\nurl = "https://zuhejian.com/"\n\n[tools]\ncount = 105',
  "sql-formatter": "select name,shortcut,platform from shortcuts where app='Excel' and platform='Windows' order by name limit 20",
  "markdown-html": "# 快捷键大全\n\n- 3056 条快捷键\n- 778 条命令\n- 105 个开发者工具\n\n**全部本地运行**，`Ctrl + /` 可以识别任意组合键。",
  "html-formatter": '<article class="card"><h2>快捷键大全</h2><p>3056 条快捷键</p><a href="/tools/">开发者工具</a></article>',
  "css-beautifier": ".tool-card{display:flex;gap:12px;padding:18px;border-radius:14px}.tool-card:hover{border-color:#111}",
  "javascript-beautifier": "function searchTools(query){return tools.filter(t=>t.name.includes(query)).map(t=>t.id)}",
  "json-to-typescript": '{"site":"zuhejian.com","tools":105,"categories":["格式化","编码","颜色"]}',
  "json-to-go": '{"site":"zuhejian.com","tools":105,"categories":["格式化","编码","颜色"]}',
  "html-minifier": '<article class="card">\n  <h2> 快捷键大全 </h2>\n\n  <!-- 顶部说明 -->\n  <p>  3056 条快捷键，全部本地运行。  </p>\n</article>',
  "css-minifier": '.tool-card {\n  display: flex; /* 横向排列 */\n  gap: 12px;\n  padding: 18px 20px;\n}\n\n.tool-card:hover {\n  border-color: #111;\n}',
  "javascript-minifier": '// 按关键词筛出命中的工具 id\nfunction searchTools(query) {\n  const hits = tools.filter((t) => t.name.includes(query));\n  return hits.map((t) => t.id);\n}',
  "xml-to-json": '<site name="快捷键大全"><tools>105</tools><local>true</local><version>2.3.0</version></site>',
  "case-converter": "hotkey knowledge base zuhejian",
  "word-counter": "快捷键大全收录 3056 条快捷键、778 条命令和 105 个开发者工具。\n所有工具在浏览器本地运行，输入不会上传。",
  "remove-duplicate-lines": "Windows\nExcel\nWindows\nPhotoshop\nExcel\nChrome",
  "sort-text": "Photoshop\nExcel\nChrome\nWindows\nVim",
  "timestamp-converter": "1761782400",
  "hash-generator": "zuhejian.com",
  "curl-converter":
    'curl -X POST https://zuhejian.com/tools/json -H "Content-Type: application/json" -d \'{"id":"json-formatter"}\'',
  "user-agent-parser":
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36",
  "url-parser": "https://zuhejian.com/tools/json-formatter?x=1&y=2#top",
  "cookie-parser": "theme=dark; lang=zh-CN; consent=accept",
  "cron-parser": "*/15 9-18 * * 1-5",
  "subnet-calculator": "192.168.1.0/24",
  "cidr-calculator": "10.0.0.0/8",
  "binary-translator": "zuhejian",
  "binary-to-decimal": "101101",
  "decimal-to-binary": "45",
  "text-to-binary": "Hi",
  "binary-to-text": "01001000 01101001",
  "hex-to-decimal": "2f",
  "decimal-to-hex": "255",
  "octal-to-decimal": "755",
  "hex-to-binary": "ff"
};

const SAMPLE_IDS = Object.keys(SAMPLES);

/* 文本类工具的主按钮文案：按工具名挑一个动词，避免一律写成「转换」 */
function actionLabel(tool) {
  const n = tool.name;
  if (/去重|重复/.test(n)) return "去重";
  if (/排序/.test(n)) return "排序";
  if (/清理/.test(n)) return "清理";
  if (/美化/.test(n)) return "美化";
  if (/格式化/.test(n)) return "格式化";
  if (/压缩/.test(n)) return "压缩";
  if (/校验|验证/.test(n)) return "校验";
  if (/测试/.test(n)) return "测试";
  if (/解读/.test(n)) return "解读";
  if (/统计|计数/.test(n)) return "统计";
  if (/解析/.test(n)) return "解析";
  if (/构建/.test(n)) return "生成";
  if (/哈希|HMAC|bcrypt/i.test(n)) return "计算";
  if (/解密/.test(n)) return "解密";
  if (/加密/.test(n)) return "加密";
  if (/生成/.test(n)) return "生成";
  if (/转换|转|换/.test(n)) return "转换";
  return "运行";
}

/* JSONPath 速查表：点一下把片段插进表达式 */
const JSONPATH_HELP = [
  ["$", "根元素"],
  ["[*]", "所有元素"],
  ["..", "递归下降"],
  ["[0:2]", "数组切片"],
  ["[?(@.x)]", "过滤表达式"]
];

function jsonpathHelp() {
  return `<div class="jp-cheats" role="group" aria-label="JSONPath 速查表">
      ${JSONPATH_HELP.map(([code, desc]) => `<button class="jp-chip" type="button" data-jp="${attr(code)}"><code>${esc(code)}</code><span>${esc(desc)}</span></button>`).join("")}
    </div>`;
}

/* 结果视图：文本 / 树形 / 表格。放在 JSON 所在的那一侧（viewsAt: in | out | both） */
function viewsHere(tool, at) {
  const where = tool.ui.viewsAt || "out";
  return (where === at || where === "both") && (tool.ui.views || ["text"]).length > 1;
}

function viewsSeg(tool) {
  const views = tool.ui.views || ["text"];
  const btns = views.map((v, i) =>
    `<button class="tw-seg-btn" type="button" data-view="${attr(v)}" aria-pressed="${i === 0}">${{ text: "文本", tree: "树形", table: "表格" }[v]}</button>`
  ).join("");
  return `<div class="tw-views tw-seg" role="group" aria-label="视图切换">${btns}</div>`;
}

function panesHtml(tool) {
  const views = tool.ui.views || ["text"];
  const panes = [];
  if (views.includes("tree")) {
    panes.push(`<div class="tw-tree" data-pane="tree" role="region" aria-label="树形视图" hidden>
      <div class="tv-filter">
        <input type="search" data-filter placeholder="筛选键或值…" aria-label="筛选树节点">
        <button class="tw-btn tw-btn-ghost" type="button" data-filter-clear>移除筛选</button>
      </div>
      <div class="tv-body"></div>
    </div>`);
  }
  if (views.includes("table")) panes.push(`<div class="tw-table" data-pane="table" role="region" aria-label="表格视图" hidden></div>`);
  return panes.join("\n      ");
}

function textPanel(tool) {
  const ui = tool.ui;
  const opts = (ui.opts || []).length ? `<div class="tool-opts">${ui.opts.map(optHtml).join("")}</div>` : "";
  const acts = (ui.acts || [["run", null]]).map(([act, label], i) =>
    `<button class="tw-btn${i === 0 ? " tw-btn-primary" : ""}" type="button" data-act="${attr(act)}">${esc(label || actionLabel(tool))}</button>`
  ).join("");
  const sample = SAMPLES[tool.id];
  const inViews = viewsHere(tool, "in");
  // 不接收文本输入的工具（生成器、计算器）不放编辑器，避免空着的大文本框
  const editor = ui.ph ? `<div${inViews ? ' class="tw-ed tw-views-box" data-views="in"' : ' class="tw-ed"'}>
      ${inViews ? `<div class="tw-out-label">${esc(ui.in || "输入")}${viewsSeg(tool)}</div>` : ""}
      <div${inViews ? ' class="tw-ed-rel"' : ' style="display:contents"'}>
        <textarea data-input rows="11" spellcheck="false" aria-label="${attr(ui.in || "输入")}" placeholder="${attr(ui.ph)}"></textarea>
        <div class="tw-empty" data-empty>
          <p>${esc(ui.ph)}</p>
          ${sample ? '<button class="tw-sample" type="button" data-act="sample">载入示例</button>' : ""}
        </div>
      </div>
      ${inViews ? panesHtml(tool) : ""}
      ${sample ? `<template data-sample>${esc(sample)}</template>` : ""}
    </div>` : "";
  const outViews = viewsHere(tool, "out");
  return `<section class="tool-work tw-card" data-panel="${attr(tool.id)}" data-handler="${attr(tool.id)}">
    ${opts}
    ${tool.id === "jsonpath-tester" ? jsonpathHelp() : ""}
    ${editor}
    <div${outViews ? ' class="tw-out tw-views-box" data-views="out"' : ' class="tw-out"'}>
      <span class="tw-out-label">${esc(ui.out || "结果")}${outViews ? viewsSeg(tool) : ""}</span>
      <textarea data-output rows="4" readonly spellcheck="false" aria-label="${esc(ui.out || "结果")}"></textarea>
      ${outViews ? panesHtml(tool) : ""}
    </div>
    <div class="tool-extra" data-extra hidden></div>
    <footer class="tw-foot">
      <div class="tw-actions">${acts}${editor ? '<button class="tw-btn tw-btn-ghost" type="button" data-act="clear">清空</button>' : ""}</div>
      <div class="tw-meta">
        <span class="tw-status" data-status role="status" aria-live="polite"></span>
        <span class="tw-kbd" aria-hidden="true"><kbd>Ctrl</kbd><kbd>↵</kbd></span>
        <span class="tw-local">本地处理</span>
      </div>
    </footer>
  </section>`;
}

function asciiTable() {
  const rows = [];
  for (let i = 32; i <= 126; i += 1) {
    const ch = i === 32 ? "空格" : String.fromCharCode(i);
    const esc4 = i === 60 ? "&lt;" : i === 62 ? "&gt;" : i === 38 ? "&amp;" : esc(ch);
    rows.push(`<tr class="cmd-row"><td data-label="十进制"><code>${i}</code></td><td data-label="十六进制"><code>${i.toString(16).toUpperCase().padStart(2, "0")}</code></td><td data-label="八进制"><code>${i.toString(8).padStart(3, "0")}</code></td><td data-label="二进制"><code>${i.toString(2).padStart(8, "0")}</code></td><td data-label="字符"><code>${esc4}</code></td></tr>`);
  }
  return `<section class="tool-work" data-panel="ascii-table" data-handler="ascii-table">
    <div class="cmd-controls" role="search"><label class="cmd-field cmd-search"><span>搜索</span><input id="ascii-search" type="search" placeholder="输入字符或数字，如 65、A、0x41"></label></div>
    <p class="tool-status" data-status role="status" aria-live="polite">可打印 ASCII 32-126，共 95 个字符</p>
    <div class="cmd-table-wrap"><table class="cmd-table"><caption class="cmd-caption">ASCII 对照表</caption><thead><tr><th>十进制</th><th>十六进制</th><th>八进制</th><th>二进制</th><th>字符</th></tr></thead><tbody id="ascii-body">${rows.join("\n")}</tbody></table></div>
    <p class="cmd-empty" id="ascii-empty" hidden>没有匹配项。</p>
  </section>`;
}

function httpTable() {
  const rows = HTTP_STATUS.map((s) => `<tr class="cmd-row" data-http-row><td data-label="状态码"><div class="cmd-term"><code>${s.code}</code></div></td><td data-label="名称">${esc(s.name)}</td><td data-label="中文">${esc(s.zh)}</td><td data-label="说明">${esc(s.desc)}</td></tr>`).join("\n");
  return `<section class="tool-work" data-panel="http-status-codes" data-handler="http-status-codes">
    <div class="cmd-controls" role="search"><label class="cmd-field cmd-search"><span>搜索</span><input id="http-search" type="search" placeholder="输入 404、重定向、超时等关键词"></label></div>
    <p class="tool-status" data-status role="status" aria-live="polite">显示 ${HTTP_STATUS.length} / ${HTTP_STATUS.length} 个状态码</p>
    <div class="cmd-table-wrap"><table class="cmd-table"><caption class="cmd-caption">HTTP 状态码速查表</caption><thead><tr><th>状态码</th><th>名称</th><th>中文</th><th>说明</th></tr></thead><tbody id="http-body">${rows}</tbody></table></div>
    <p class="cmd-empty" id="http-empty" hidden>没有匹配的状态码。</p>
  </section>`;
}

function noticePanel(tool) {
  return `<section class="tool-work" data-panel="${attr(tool.id)}" data-handler="${attr(tool.id)}">
    <div class="cmd-callout is-danger"><strong>不提供上传</strong><p>${esc(tool.ui.text)}</p></div>
  </section>`;
}

const rangeRow = (key, label, min, max, value, step = 1) => `<label class="tool-field tool-range"><span>${esc(label)} <b data-val="${key}">${value}</b></span><input type="range" data-opt="${key}" min="${min}" max="${max}" step="${step}" value="${value}"></label>`;

const PANELS = {
  ua: () => `<section class="tool-work tw-card" data-panel="user-agent-parser" data-handler="user-agent-parser">
    <div class="tw-ed">
      <div style="display:contents">
        <textarea data-input rows="3" spellcheck="false" aria-label="User-Agent" placeholder="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"></textarea>
        <div class="tw-empty" data-empty>
          <p>粘贴一条 User-Agent 字符串</p>
          <button class="tw-sample" type="button" data-act="sample">载入示例</button>
        </div>
      </div>
      <template data-sample>Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.1 Safari/605.1.15</template>
    </div>
    <div class="ua-cards" data-ua-cards></div>
    <div class="qv-head">
      <span>解析结果</span>
      <div class="tw-seg" role="group" aria-label="解析结果视图">
        <button class="tw-seg-btn" type="button" data-qv="text" aria-pressed="true">文本</button>
        <button class="tw-seg-btn" type="button" data-qv="tree" aria-pressed="false">树形</button>
        <button class="tw-seg-btn" type="button" data-qv="table" aria-pressed="false">表格</button>
        <button class="tw-seg-btn" type="button" data-qv="json" aria-pressed="false">JSON</button>
      </div>
      <button class="tw-btn tw-btn-ghost" type="button" data-act="copy">复制</button>
    </div>
    <div class="qv-pane" data-qv-pane="text"><pre class="qv-pre" data-qv-text></pre></div>
    <div class="qv-pane" data-qv-pane="tree" hidden><div data-qv-tree></div></div>
    <div class="qv-pane" data-qv-pane="table" hidden><div class="tv-tablewrap" data-qv-table></div></div>
    <div class="qv-pane" data-qv-pane="json" hidden><pre class="qv-pre" data-qv-json></pre></div>
    <div class="tool-extra" data-extra hidden></div>
    <footer class="tw-foot">
      <div class="tw-actions">
        <button class="tw-btn tw-btn-primary" type="button" data-act="run">解析</button>
        <button class="tw-btn tw-btn-ghost" type="button" data-act="mine">使用浏览器当前的 UA</button>
        <button class="tw-btn tw-btn-ghost" type="button" data-act="clear">清空</button>
      </div>
      <div class="tw-meta">
        <span class="tw-status" data-status role="status" aria-live="polite">UA 是自报的，可被伪造</span>
        <span class="tw-kbd" aria-hidden="true"><kbd>Ctrl</kbd><kbd>↵</kbd></span>
        <span class="tw-local">本地处理</span>
      </div>
    </footer>
  </section>`,
  cookie: () => `<section class="tool-work tw-card" data-panel="cookie-parser" data-handler="cookie-parser">
    <div class="tool-opts">
      <div class="tool-field"><span>解析对象</span>
        <div class="tool-seg ck-seg" role="group" aria-label="解析对象">
          <button class="tw-seg-btn" type="button" data-act="mode" data-ck-val="cookie" aria-pressed="true">Cookie 头</button>
          <button class="tw-seg-btn" type="button" data-act="mode" data-ck-val="set" aria-pressed="false">Set-Cookie</button>
        </div>
        <input type="hidden" data-opt="mode" value="cookie">
      </div>
      <label class="tool-check"><input type="checkbox" data-opt="decode" checked> URL 解码值</label>
    </div>
    <div class="tw-ed">
      <div style="display:contents">
        <textarea data-input rows="4" spellcheck="false" aria-label="Cookie 或 Set-Cookie 头" placeholder="hi_theme=dark; hi_lang=zh-CN"></textarea>
        <div class="tw-empty" data-empty>
          <p>粘贴 Cookie 或 Set-Cookie 头</p>
          <button class="tw-sample" type="button" data-act="sampleCookie">Cookie 示例</button>
          <button class="tw-sample" type="button" data-act="sampleSet">Set-Cookie 示例</button>
        </div>
      </div>
    </div>
    <dl class="kv-list" data-parts></dl>
    <div class="ck-warn" data-ck-warn></div>
    <div class="qv-head">
      <span data-ck-title>Cookie</span>
      <div class="tw-seg" role="group" aria-label="解析结果视图">
        <button class="tw-seg-btn" type="button" data-qv="text" aria-pressed="true">文本</button>
        <button class="tw-seg-btn" type="button" data-qv="tree" aria-pressed="false">树形</button>
        <button class="tw-seg-btn" type="button" data-qv="table" aria-pressed="false">表格</button>
        <button class="tw-seg-btn" type="button" data-qv="json" aria-pressed="false">JSON</button>
      </div>
      <button class="tw-btn tw-btn-ghost" type="button" data-act="copy">复制</button>
    </div>
    <div class="qv-pane" data-qv-pane="text"><pre class="qv-pre" data-qv-text></pre></div>
    <div class="qv-pane" data-qv-pane="tree" hidden><div data-qv-tree></div></div>
    <div class="qv-pane" data-qv-pane="table" hidden><div class="tv-tablewrap" data-qv-table></div></div>
    <div class="qv-pane" data-qv-pane="json" hidden><pre class="qv-pre" data-qv-json></pre></div>
    <div class="tool-extra" data-extra hidden></div>
    <footer class="tw-foot">
      <div class="tw-actions">
        <button class="tw-btn tw-btn-primary" type="button" data-act="run">解析</button>
        <button class="tw-btn tw-btn-ghost" type="button" data-act="load">加载我的 cookie</button>
        <button class="tw-btn tw-btn-ghost" type="button" data-act="clear">清空</button>
      </div>
      <div class="tw-meta">
        <span class="tw-status" data-status role="status" aria-live="polite">按 RFC 6265 解析；document.cookie 看不到 HttpOnly cookie</span>
        <span class="tw-kbd" aria-hidden="true"><kbd>Ctrl</kbd><kbd>↵</kbd></span>
        <span class="tw-local">本地处理</span>
      </div>
    </footer>
  </section>`,
  cron: () => `<section class="tool-work tw-card" data-panel="cron-parser" data-handler="cron-parser">
    <div class="tool-opts">
      <div class="tool-field"><span>时区</span><select data-opt="tz"><option value="local">本地</option><option value="utc">UTC</option></select></div>
      <div class="tool-field"><span>预览次数</span><select data-opt="count"><option>5</option><option>3</option><option>10</option></select></div>
    </div>
    <div class="tw-ed">
      <div style="display:contents">
        <textarea data-input rows="3" spellcheck="false" aria-label="Cron 表达式" placeholder="0 3 * * *"></textarea>
        <div class="tw-empty" data-empty>
          <p>分 · 时 · 日 · 月 · 周</p>
          <button class="tw-sample" type="button" data-act="sample">载入示例</button>
        </div>
      </div>
      <template data-sample>0 3 * * *</template>
    </div>
    <dl class="kv-list" data-cron-summary></dl>
    <div class="qv-head"><span>字段明细</span></div>
    <div class="tv-tablewrap" data-cron-fields></div>
    <div class="qv-head"><span>接下来 <span data-cron-count>5</span> 次运行</span><span data-cron-tz>本地时区</span></div>
    <div class="tv-tablewrap" data-cron-runs></div>
    <div class="qv-head"><span>常用预设</span></div>
    <div class="jp-cheats" data-cron-presets>
      <button class="jp-chip" type="button" data-act="preset" data-cron-preset="* * * * *"><code>* * * * *</code><span>每分钟</span></button>
      <button class="jp-chip" type="button" data-act="preset" data-cron-preset="*/15 * * * *"><code>*/15 * * * *</code><span>每 15 分钟</span></button>
      <button class="jp-chip" type="button" data-act="preset" data-cron-preset="0 * * * *"><code>0 * * * *</code><span>每小时</span></button>
      <button class="jp-chip" type="button" data-act="preset" data-cron-preset="0 0 * * *"><code>0 0 * * *</code><span>每天午夜</span></button>
      <button class="jp-chip" type="button" data-act="preset" data-cron-preset="0 0 * * 0"><code>0 0 * * 0</code><span>每周日 00:00</span></button>
      <button class="jp-chip" type="button" data-act="preset" data-cron-preset="0 0 1 * *"><code>0 0 1 * *</code><span>每月 1 日</span></button>
      <button class="jp-chip" type="button" data-act="preset" data-cron-preset="0 0 1 1 *"><code>0 0 1 1 *</code><span>每年 1 月 1 日</span></button>
    </div>
    <footer class="tw-foot">
      <div class="tw-actions">
        <button class="tw-btn tw-btn-primary" type="button" data-act="run">解析</button>
        <button class="tw-btn tw-btn-ghost" type="button" data-act="clear">清空</button>
      </div>
      <div class="tw-meta">
        <span class="tw-status" data-status role="status" aria-live="polite">支持 5 段与 6 段（含秒）、@daily 等别名、JAN / MON 名称</span>
        <span class="tw-kbd" aria-hidden="true"><kbd>Ctrl</kbd><kbd>↵</kbd></span>
        <span class="tw-local">本地处理</span>
      </div>
    </footer>
  </section>`,
  url: () => `<section class="tool-work tw-card" data-panel="url-parser" data-handler="url-parser">
    <div class="tool-opts">
      <label class="tool-check"><input type="checkbox" data-opt="plus"> + 作为空格</label>
      <label class="tool-check"><input type="checkbox" data-opt="dupe" checked> 重复键保存为数组</label>
    </div>
    <div class="tw-ed">
      <div style="display:contents">
        <textarea data-input rows="4" spellcheck="false" aria-label="URL" placeholder="https://zuhejian.com/tools/url-parser?q=cmd&amp;page=2"></textarea>
        <div class="tw-empty" data-empty>
          <p>https://zuhejian.com/tools/url-parser?q=cmd&amp;page=2</p>
          <button class="tw-sample" type="button" data-act="sample">载入示例</button>
        </div>
      </div>
      <template data-sample>https://zuhejian.com/tools/url-parser?q=cmd&amp;page=2&amp;tag=a&amp;tag=b</template>
    </div>
    <dl class="kv-list" data-parts></dl>
    <div class="qv-head">
      <span>查询参数</span>
      <div class="tw-seg" role="group" aria-label="查询参数视图">
        <button class="tw-seg-btn" type="button" data-qv="text" aria-pressed="true">文本</button>
        <button class="tw-seg-btn" type="button" data-qv="tree" aria-pressed="false">树形</button>
        <button class="tw-seg-btn" type="button" data-qv="table" aria-pressed="false">表格</button>
        <button class="tw-seg-btn" type="button" data-qv="json" aria-pressed="false">JSON</button>
      </div>
      <button class="tw-btn tw-btn-ghost" type="button" data-act="copy">复制</button>
    </div>
    <div class="qv-pane" data-qv-pane="text"><pre class="qv-pre" data-qv-text></pre></div>
    <div class="qv-pane" data-qv-pane="tree" hidden><div data-qv-tree></div></div>
    <div class="qv-pane" data-qv-pane="table" hidden><div class="tv-tablewrap" data-qv-table></div></div>
    <div class="qv-pane" data-qv-pane="json" hidden><pre class="qv-pre" data-qv-json></pre></div>
    <div class="tool-extra" data-extra hidden></div>
    <footer class="tw-foot">
      <div class="tw-actions">
        <button class="tw-btn tw-btn-primary" type="button" data-act="run">解析</button>
        <button class="tw-btn tw-btn-ghost" type="button" data-act="clear">清空</button>
      </div>
      <div class="tw-meta">
        <span class="tw-status" data-status role="status" aria-live="polite">需要完整协议，例如 https://zuhejian.com/tools</span>
        <span class="tw-kbd" aria-hidden="true"><kbd>Ctrl</kbd><kbd>↵</kbd></span>
        <span class="tw-local">本地处理</span>
      </div>
    </footer>
  </section>`,
  colorPicker: () => `<section class="tool-work" data-panel="color-picker" data-handler="color-picker">
    <div class="tool-grid tool-grid-2">
      ${field("取色", `<input type="color" data-opt="hex" value="#0072f5">`)}
      ${field("HEX", `<input data-opt="hexText" value="#0072f5" spellcheck="false">`)}
      ${field("RGB", `<input data-opt="rgb" value="rgb(0, 114, 245)" spellcheck="false">`)}
      ${field("HSL", `<input data-opt="hsl" value="hsl(212, 100%, 48%)" spellcheck="false">`)}
    </div>
    <div class="cp-swatch" data-swatch aria-label="颜色预览"></div>
    <dl class="kv-list">
      <div class="kv-row"><dt>OKLCH</dt><dd data-oklch>—</dd></div>
      <div class="kv-row"><dt>对比度</dt><dd data-contrast>—</dd></div>
    </dl>
    <p class="tool-status" data-status role="status" aria-live="polite">修改任意输入即可联动。</p>
  </section>`,
  contrast: () => `<section class="tool-work" data-panel="color-contrast-checker" data-handler="color-contrast-checker">
    <div class="tool-grid tool-grid-2">
      ${field("前景色", `<input type="color" data-opt="fg" value="#0072f5">`)}
      ${field("背景色", `<input type="color" data-opt="bg" value="#ffffff">`)}
    </div>
    <div class="tool-swatch" data-contrast-preview>
      <p class="cc-preview-normal">常规文本（AA 需 4.5）——快捷键大全 HotkeyInspector</p>
      <p class="cc-preview-large">大号文本（AA 需 3.0）——标题</p>
    </div>
    <dl class="kv-list">
      <div class="kv-row"><dt>对比度</dt><dd data-ratio>—</dd></div>
      <div class="kv-row"><dt>AA 常规</dt><dd data-aa>—</dd></div>
      <div class="kv-row"><dt>AA 大号</dt><dd data-aa-large>—</dd></div>
      <div class="kv-row"><dt>AAA 常规</dt><dd data-aaa>—</dd></div>
      <div class="kv-row"><dt>AAA 大号</dt><dd data-aaa-large>—</dd></div>
    </dl>
    <p data-verdict aria-live="polite"></p>
    <p class="tool-status" data-status role="status" aria-live="polite">按 WCAG 2.x 相对亮度计算。</p>
  </section>`,
  invisibleCleaner: () => `<section class="tool-work" data-panel="invisible-character-cleaner" data-handler="invisible-character-cleaner">
    <div class="ic-cats" role="group" aria-label="要清理的类别">
      ${[["zw", "零宽", true], ["bidi", "双向控制字符", true], ["tag", "标签字符", true], ["ctrl", "控制字符", true], ["space", "空格和 NBSP", false], ["homo", "同形字符", false]].map(function (c) {
        return '<label class="ic-cat"><input type="checkbox" data-cat="' + c[0] + '"' + (c[2] ? " checked" : "") + '> <span>' + c[1] + '</span><b data-count="' + c[0] + '" hidden></b></label>';
      }).join("")}
    </div>
    <div class="tool-field"><span>您的文本</span><textarea data-input rows="5" spellcheck="false" placeholder="粘贴可能含有零宽字符、方向控制符或异常空白的文本"></textarea></div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">清理</button><button class="cmd-btn" data-act="clear">清除</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">「零宽 / 双向 / 标签 / 控制」类别会删除字符；「空格和 NBSP / 同形字符」只做规范化替换，默认关闭以保护合法文本。</p>
    <div data-highlight-wrap hidden>
      <div class="tool-field"><span>高亮显示的输入</span><pre class="tool-pre ic-highlight" data-highlight></pre></div>
    </div>
    <div data-table-wrap hidden>
      <div class="tool-field"><span>可疑字符</span><div class="tv-tablewrap"><table class="tv-table ic-table"><thead><tr><th>码位</th><th>名称</th><th>类别</th><th>位置</th></tr></thead><tbody data-tbody></tbody></table></div></div>
    </div>
    <div class="tool-field" data-clean-wrap hidden><span>清理后文本</span><textarea data-output rows="5" readonly spellcheck="false"></textarea></div>
    <div class="tool-actions" data-clean-actions hidden><button class="cmd-btn" data-act="copy">复制清理结果</button></div>
  </section>`,
  imageBase64: () => `<section class="tool-work" data-panel="image-to-base64" data-handler="image-to-base64">
    ${field("选择图片", `<input type="file" data-opt="file" accept="image/*">`)}
    <div class="tool-field"><span>Base64 或数据 URI</span><textarea data-opt="b64" rows="4" spellcheck="false" placeholder="粘贴 data:image/png;base64,… 或纯 Base64 串"></textarea></div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">图片 → Base64</button><button class="cmd-btn" data-act="toImage">Base64 → 图片</button><button class="cmd-btn" data-act="copy">复制</button><button class="cmd-btn" data-act="download">下载图片</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">图片不会上传，data URI 可直接用于 CSS 或 HTML；数据 URI 约有 33% 的体积开销。</p>
    <div class="tool-field"><span>data URI</span><textarea data-output rows="5" readonly spellcheck="false"></textarea></div>
    <dl class="kv-list" data-details hidden></dl>
    <div class="tool-preview" data-preview></div>
  </section>`,
  binaryTranslator: () => `<section class="tool-work" data-panel="binary-translator" data-handler="binary-translator">
    <div class="tool-field"><span>文本</span><textarea data-opt="text" rows="5" spellcheck="false">Hi</textarea></div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="toBinary">文本 → 二进制</button><button class="cmd-btn" data-act="toText">二进制 → 文本</button><button class="cmd-btn" data-act="copy">复制</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">默认按 UTF-8 编码，空格分隔每个字节。</p>
    <div class="tool-field"><span>二进制</span><textarea data-output rows="6" spellcheck="false"></textarea></div>
    <div class="tool-extra" data-extra hidden></div>
  </section>`,
  caseConverter: () => `<section class="tool-work" data-panel="case-converter" data-handler="case-converter">
    ${field("输入文本", `<textarea data-input rows="4" spellcheck="false" placeholder="hotkey inspector tool"></textarea>`)}
    <p class="tool-status" data-status role="status" aria-live="polite">一次给出 11 种命名风格，点「复制」按需取用。</p>
    <div data-out></div>
  </section>`,
  binaryCalc: () => `<section class="tool-work" data-panel="binary-calculator" data-handler="binary-calculator">
    <div class="tool-grid tool-grid-2">
      ${field("操作数 A", `<input data-opt="a" value="1010" spellcheck="false">`)}
      ${field("操作数 B", `<input data-opt="b" value="110" spellcheck="false">`)}
      ${field("输入进制", `<select data-opt="base"><option value="2">二进制</option><option value="10">十进制</option><option value="16">十六进制</option></select>`)}
      ${field("运算", `<select data-opt="op"><option>+</option><option>-</option><option>&amp;</option><option>|</option><option>^</option><option>&lt;&lt;</option><option>&gt;&gt;</option></select>`)}
      ${field("位宽", `<select data-opt="width"><option value="8">8</option><option value="16">16</option><option value="32" selected>32</option><option value="64">64</option></select>`)}
    </div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">计算</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">结果按所选位宽截断，并按二进制补码解读为有符号数。</p>
    <dl class="tool-dl" data-out></dl>
  </section>`,
  markdown: () => `<section class="tool-work" data-panel="markdown-html" data-handler="markdown-html">
    <div class="tool-field"><span>Markdown 或 HTML</span><textarea data-input rows="7" spellcheck="false"># 快捷键大全

- 3056 条快捷键
- 778 条命令
- 105 个开发者工具

**全部本地运行**，\`Ctrl + /\` 可以识别任意组合键，详见 https://zuhejian.com/。</textarea></div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="md2html">Markdown → HTML</button><button class="cmd-btn" data-act="html2md">HTML → Markdown</button><button class="cmd-btn" data-act="copy">复制</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">HTML 会先经 DOMPurify 清洗，再渲染预览。</p>
    <div class="tool-field"><span>HTML</span><textarea data-output rows="5" readonly spellcheck="false"></textarea></div>
    <div class="tool-preview tool-preview-rich" data-preview></div>
  </section>`,
  har: () => `<section class="tool-work" data-panel="har-file-viewer" data-handler="har-file-viewer">
    ${field("选择 .har 文件", `<input type="file" data-opt="file" accept=".har,application/json">`)}
    <div class="tool-actions">
      <button class="cmd-btn cmd-btn-primary" data-act="run">解析</button>
      <button class="cmd-btn" data-act="clearFile">清空</button>
    </div>
    <div class="har-filter" data-filter role="group" aria-label="按类型过滤">
      <button type="button" data-filter-val="all" aria-pressed="true">全部</button>
      <button type="button" data-filter-val="xhr" aria-pressed="false">XHR</button>
      <button type="button" data-filter-val="js" aria-pressed="false">JS</button>
      <button type="button" data-filter-val="css" aria-pressed="false">CSS</button>
      <button type="button" data-filter-val="image" aria-pressed="false">图片</button>
      <button type="button" data-filter-val="media" aria-pressed="false">媒体</button>
      <button type="button" data-filter-val="font" aria-pressed="false">字体</button>
      <button type="button" data-filter-val="document" aria-pressed="false">文档</button>
      <button type="button" data-filter-val="error" aria-pressed="false">错误</button>
    </div>
    <dl class="har-summary" data-summary></dl>
    <div class="cmd-table-wrap">
      <table class="cmd-table har-table">
        <thead><tr><th>状态</th><th>方法</th><th>URL</th><th data-sort="time" class="har-sort">耗时</th><th data-sort="size" class="har-sort">大小</th></tr></thead>
        <tbody data-rows></tbody>
      </table>
    </div>
    <p class="tool-status" data-status role="status" aria-live="polite">文件只在本地解析，不会上传。</p>
  </section>`,
  compression: () => `<section class="tool-work" data-panel="compression-comparison" data-handler="compression-comparison">
    ${field("选择文件", `<input type="file" data-opt="file">`)}
    <div class="tool-field"><span>或输入文本</span><textarea data-input rows="6" spellcheck="false"></textarea></div>
    <div class="tool-grid tool-grid-2">
      ${field("压缩级别", `<select data-opt="mode"><option value="fast" selected>最快</option><option value="min">最小</option><option value="custom">自定义</option></select>`)}
      ${field("自定义级别", `<input type="number" data-opt="level" value="6" min="1" max="19" disabled>`)}
    </div>
    <p class="tool-status" data-status role="status" aria-live="polite">级别越高，压缩后体积越小，但速度越慢。范围：gzip 0–9，Brotli 1–11，Zstd 1–19。</p>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">压缩</button></div>
    <div data-out></div>
  </section>`,
  diff: () => `<section class="tool-work" data-panel="diff-checker" data-handler="diff-checker">
    <div class="tool-grid tool-grid-2">
      ${field("原文本", `<textarea data-opt="left" rows="6" spellcheck="false"></textarea>`)}
      ${field("新文本", `<textarea data-opt="right" rows="6" spellcheck="false"></textarea>`)}
    </div>
    <div class="tool-opts">
      <label class="tool-check"><input type="checkbox" data-opt="nocase"> 忽略大小写</label>
      <label class="tool-check"><input type="checkbox" data-opt="ws"> 忽略行尾空白</label>
    </div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">比较</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">逐行比较，绿色为新增，红色为删除。</p>
    <pre class="tool-pre" data-out></pre>
  </section>`,
  gradient: () => `<section class="tool-work" data-panel="css-gradient-generator" data-handler="css-gradient-generator">
    <div class="tool-grid tool-grid-3">
      ${field("渐变类型", `<select data-opt="type"><option value="linear">线性 linear</option><option value="radial">径向 radial</option><option value="conic">锥形 conic</option></select>`)}
      ${field("角度 / 方向", `<input type="number" data-opt="angle" value="135" min="0" max="360" step="1">`)}
      ${field("预设", `<select data-opt="preset"><option value="">自定义</option><option value="brand">品牌蓝紫</option><option value="sunset">日落</option><option value="forest">森林</option><option value="mono">灰阶</option><option value="mesh">三色网点</option></select>`)}
    </div>
    <div class="cg-stops" data-stops></div>
    <div class="tool-actions"><button class="cmd-btn" data-act="addStop">添加色标</button><button class="cmd-btn cmd-btn-primary" data-act="run">生成</button><button class="cmd-btn" data-act="copy">复制</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">色标按位置从 0% 到 100% 排列，全部在本地计算。</p>
    <div class="tool-swatch tool-swatch-lg" data-preview></div>
    <div class="tool-field"><span>CSS</span><textarea data-output rows="3" readonly spellcheck="false"></textarea></div>
  </section>`,
  shadow: () => `<section class="tool-work" data-panel="box-shadow-generator" data-handler="box-shadow-generator">
    <div class="tool-row">
      <div class="tool-field is-sm"><span>选择预设</span><select data-opt="preset"><option value="">选择预设</option><option value="soft">柔和卡片</option><option value="lift">抬升</option><option value="float">悬浮</option><option value="inner">内辉光</option><option value="hard">硬边偏移</option></select></div>
    </div>
    <div class="bs-layers" data-layers>
      <div class="bs-layer" data-layer data-bl="[{&quot;x&quot;:0,&quot;y&quot;:4,&quot;blur&quot;:12,&quot;spread&quot;:-2,&quot;color&quot;:&quot;#0f172a&quot;,&quot;alpha&quot;:0.08,&quot;inset&quot;:false}]">
        <div class="bs-layer-head"><b class="bs-layer-title">图层 1</b><button class="tw-icon" type="button" data-act="removeLayer" title="删除本层" aria-label="删除本层">&#10005;</button></div>
        <div class="bs-grid">
          <label class="bs-num"><span>X 偏移</span><input type="number" data-lopt="x" value="0" step="1"></label>
          <label class="bs-num"><span>Y 偏移</span><input type="number" data-lopt="y" value="4" step="1"></label>
          <label class="bs-num"><span>模糊</span><input type="number" data-lopt="blur" value="12" step="1" min="0"></label>
          <label class="bs-num"><span>扩展</span><input type="number" data-lopt="spread" value="-2" step="1"></label>
          <label class="bs-num"><span>颜色</span><input type="color" data-lopt="color" value="#0f172a"></label>
          <label class="bs-num"><span>不透明度</span><input type="number" data-lopt="alpha" value="0.08" min="0" max="1" step="0.01"></label>
        </div>
        <label class="tool-check"><input type="checkbox" data-lopt="inset"> 内阴影</label>
      </div>
    </div>
    <div class="tool-actions"><button class="cmd-btn" data-act="addLayer">添加图层</button><button class="cmd-btn cmd-btn-primary" data-act="run">生成</button><button class="cmd-btn" data-act="copy">复制</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">多层阴影按顺序叠加，全部在本地计算。</p>
    <div class="tool-shadow-stage"><div class="tool-shadow-box" data-preview></div></div>
    <div class="tool-field"><span>CSS</span><textarea data-output rows="3" readonly spellcheck="false"></textarea></div>
  </section>`,
  chmod: () => `<section class="tool-work" data-panel="chmod-calculator" data-handler="chmod-calculator">
    <div class="tool-grid tool-grid-2">
      ${field("数字权限", `<input data-opt="numeric" value="755" spellcheck="false">`)}
      ${field("符号权限", `<input data-opt="symbolic" value="rwxr-xr-x" spellcheck="false">`)}
    </div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">换算</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">支持 3 位和 4 位（含 setuid/setgid/sticky）。</p>
    <dl class="tool-dl" data-out></dl>
  </section>`,
  aes: () => `<section class="tool-work" data-panel="aes-encryption" data-handler="aes-encryption">
    <div class="tool-grid tool-grid-2">
      ${field("密码算法", `<select data-opt="cipher"><option value="AES-GCM">AES-GCM</option><option value="AES-CBC">AES-CBC</option></select>`)}
      ${field("密钥长度", `<select data-opt="bits"><option value="256">256</option><option value="128">128</option><option value="192">192</option></select>`)}
    </div>
    <div class="tool-field"><span>密码短语</span><input type="password" data-opt="password" value="hotkey-inspector" placeholder="至少 8 位，用于 PBKDF2 派生"></div>
    <div class="tool-field"><span>明文 / 密文（Base64）</span><textarea data-input rows="6" spellcheck="false"></textarea></div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="encrypt">加密</button><button class="cmd-btn" data-act="decrypt">解密</button><button class="cmd-btn" data-act="copy">复制</button><button class="cmd-btn" data-act="clear">清除</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">密钥用 PBKDF2（12 万次迭代，SHA-256）从密码短语派生；密文里带盐和 IV，可在本机解回。</p>
    <div class="tool-field"><span>结果（Base64）</span><textarea data-output rows="5" readonly spellcheck="false"></textarea></div>
  </section>`,
  jwt: () => `<section class="tool-work" data-panel="jwt-decoder" data-handler="jwt-decoder">
    <div class="tool-field"><span>JWT</span><textarea data-input rows="4" spellcheck="false" placeholder="eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjMifQ.sig"></textarea></div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">解码</button><button class="cmd-btn" data-act="clear">清空</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">只解码 Base64URL，不校验签名。</p>
    <div class="tool-grid tool-grid-2">
      ${field("Header", `<pre class="tool-pre" data-header>—</pre>`)}
      ${field("Payload", `<pre class="tool-pre" data-payload>—</pre>`)}
    </div>
  </section>`,
  schema: () => `<section class="tool-work" data-panel="json-schema-builder" data-handler="json-schema-builder">
    <div class="tool-grid tool-grid-2">
      ${field("Schema 名称", `<input data-opt="name" value="tool_arguments" spellcheck="false">`)}
      ${field("导出格式", `<select data-opt="format"><option>JSON Schema</option><option>OpenAI response_format</option><option>OpenAI tool</option><option>Anthropic</option><option>Gemini</option><option>Zod</option><option>Pydantic</option></select>`)}
    </div>
    <div class="sf-list" data-fields></div>
    <div class="sf-tools">
      <button class="tw-btn" type="button" data-act="addField">＋ 添加字段</button>
      <button class="tw-btn" type="button" data-act="infer">从 JSON 推断</button>
    </div>
    <details class="sf-infer" open>
      <summary>从 JSON 推断字段</summary>
      <textarea data-opt="sample" rows="4" spellcheck="false">${esc('{"site":"zuhejian.com","tools":105,"local":true}')}</textarea>
    </details>
    <div class="tw-out">
      <span class="tw-out-label">导出</span>
      <textarea data-output rows="10" readonly spellcheck="false" aria-label="导出结果"></textarea>
    </div>
    <p class="tool-status" data-status role="status" aria-live="polite"></p>
  </section>`,
  jsonl: () => `<section class="tool-work" data-panel="jsonl-validator" data-handler="jsonl-validator">
    <div class="tool-actions">
      <button class="tw-btn tw-btn-primary" type="button" data-act="run">校验</button>
      <button class="tw-btn" type="button" data-act="train">下载 train.jsonl</button>
      <button class="tw-btn" type="button" data-act="val">下载 val.jsonl</button>
    </div>
    <div class="tool-grid tool-grid-2">
      ${field("数据集格式", seg("format", ["OpenAI", "Anthropic", "仅 JSON"], "OpenAI"))}
      ${field("模型（估算 token）", seg("model", ["GPT-4o", "Claude", "Gemini"], "GPT-4o"))}
    </div>
    ${field("JSONL 文件", `<input type="file" data-opt="file" accept=".jsonl,.json,text/plain">`)}
    <div class="tool-field"><span>或粘贴 JSONL</span><textarea data-input rows="8" spellcheck="false">{"messages":[{"role":"system","content":"你是快捷键大全的助手。"},{"role":"user","content":"Ctrl+Shift+T 是什么？"},{"role":"assistant","content":"重新打开最近关闭的标签页。"}]}
{"messages":[{"role":"user","content":"Excel 里怎么冻结首行？"},{"role":"assistant","content":"视图 → 冻结窗格 → 冻结首行。"}]}
{"messages":[{"role":"user","content":"Excel 里怎么冻结首行？"},{"role":"assistant","content":"视图 → 冻结窗格 → 冻结首行。"}]}</textarea></div>
    <div class="jl-stats" data-stats></div>
    <div class="jl-errors" data-errors hidden></div>
    <div class="jl-split">
      ${field("验证集比例（%）", `<input type="number" data-opt="valPct" value="10" min="1" max="50" step="1">`)}
    </div>
    <p class="tool-status" data-status role="status" aria-live="polite">逐行解析，报告行号与原因；文件只在本地读取。</p>
  </section>`,
  metaGenerator: () => `<section class="tool-work" data-panel="meta-tag-generator" data-handler="meta-tag-generator">
    <div class="tool-grid tool-grid-2">
      ${field("页面标题", `<input data-opt="title" value="${attr(SITE.title)}" spellcheck="false">`)}
      ${field("网站名称", `<input data-opt="siteName" value="${attr(SITE.name)}" spellcheck="false">`)}
      <div class="tool-field tool-span-2"><span>Meta 描述</span><textarea data-opt="description" rows="3" spellcheck="false">${esc(SITE.desc)}</textarea></div>
      ${field("Canonical URL", `<input data-opt="url" value="${attr(SITE.tools)}" spellcheck="false">`)}
      ${field("社交图片 URL", `<input data-opt="image" value="${attr(SITE.image)}" spellcheck="false">`)}
      ${field("Twitter 账号", `<input data-opt="twitter" value="@zuhejian" spellcheck="false">`)}
      ${field("Robots", seg("robots", ["index, follow", "noindex, nofollow"], "index, follow"))}
      ${field("Open Graph 类型", seg("ogType", ["website", "article", "product"], "website"))}
      ${field("Twitter card", seg("card", ["summary_large_image", "summary"], "summary_large_image"))}
    </div>
    <div class="meta-prev" data-preview>
      <div>
        <p class="meta-prev-label">Google 搜索结果</p>
        <div class="meta-serp">
          <p class="meta-serp-site" data-serp-site>zuhejian.com › tools</p>
          <p class="meta-serp-title" data-serp-title>${esc(SITE.title)}</p>
          <p class="meta-serp-desc" data-serp-desc>${esc(SITE.desc)}</p>
        </div>
      </div>
      <div>
        <p class="meta-prev-label">社交卡片</p>
        <div class="meta-card">
          <div class="meta-card-media" data-card-media><img alt="" data-card-img><span class="meta-card-empty" data-card-empty>无法加载图片</span></div>
          <div class="meta-card-body">
            <p class="meta-card-site" data-card-site>zuhejian.com</p>
            <p class="meta-card-title" data-card-title>${esc(SITE.title)}</p>
            <p class="meta-card-desc" data-card-desc>${esc(SITE.desc)}</p>
          </div>
        </div>
      </div>
    </div>
    <div class="tw-out">
      <span class="tw-out-label">生成的 meta 标签</span>
      <textarea data-output rows="8" readonly spellcheck="false" aria-label="生成的 meta 标签"></textarea>
    </div>
    <p class="tool-status" data-status role="status" aria-live="polite"></p>
  </section>`,
  metaPreview: () => `<section class="tool-work" data-panel="meta-tag-previewer" data-handler="meta-tag-previewer">
    <div class="tw-ed">
      <textarea data-input rows="8" spellcheck="false" placeholder="粘贴 &lt;head&gt; 或整页 HTML">${esc(SITE_HEAD)}</textarea>
      <div class="tw-empty" data-empty>
        <p>粘贴 &lt;head&gt; 或整页 HTML</p>
        <button class="tw-sample" type="button" data-act="sample">试用本站示例</button>
      </div>
      <template data-sample>${esc(SITE_HEAD)}</template>
    </div>
    <div class="meta-prev meta-prev-stack" data-preview></div>
    <div class="meta-check" data-checklist></div>
    <p class="tool-status" data-status role="status" aria-live="polite"></p>
  </section>`,
  timestamp: () => `<section class="tool-work" data-panel="timestamp-converter" data-handler="timestamp-converter" data-live="1">
    <div class="tool-row">
      ${field("Unix 时间戳（秒或毫秒）", `<input data-opt="ts" value="1793145600" inputmode="numeric" spellcheck="false">`)}
      ${field("日期时间（本地时区）", `<input type="datetime-local" data-opt="dt" step="1">`)}
      <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">时间戳转日期</button><button class="cmd-btn" data-act="fromdate">日期转时间戳</button><button class="cmd-btn" data-act="now">现在</button></div>
    </div>
    <dl class="kv-list">
      <div class="kv-row"><dt>本地时间</dt><dd data-local>—</dd></div>
      <div class="kv-row"><dt>UTC</dt><dd data-utc>—</dd></div>
      <div class="kv-row"><dt>ISO 8601</dt><dd data-iso>—</dd></div>
      <div class="kv-row"><dt>相对现在</dt><dd data-rel>—</dd></div>
    </dl>
    <p class="tool-status" data-status role="status" aria-live="polite">10 位按秒、13 位按毫秒；两个输入框改哪个都会同步另一个。</p>
  </section>`,
  regex: () => `<section class="tool-work" data-panel="regex-tester" data-handler="regex-tester">
    <div class="tool-row">
      <div class="tool-field is-wide"><span>正则表达式</span><input data-opt="pattern" value="https?://[\\w.-]+\\.\\w{2,}" spellcheck="false"></div>
      <div class="tool-field is-sm"><span>Flags</span><input data-opt="flags" value="gi" maxlength="6" spellcheck="false"></div>
    </div>
    <div class="tool-field"><span>测试文本</span><textarea data-input rows="6" spellcheck="false">快捷键大全：https://zuhejian.com/
开发者工具：https://zuhejian.com/tools/
欢迎反馈：hello@zuhejian.com</textarea></div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">测试</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">支持 g、i、m、s、u、y。</p>
    <pre class="tool-pre" data-out></pre>
  </section>`,
  subnet: () => `<section class="tool-work" data-panel="subnet-calculator" data-handler="subnet-calculator">
    <div class="tool-row">
      ${field("IPv4 地址", `<input data-opt="ip" value="192.168.1.10" spellcheck="false">`)}
      ${field("前缀长度", `<input type="number" data-opt="prefix" min="0" max="32" value="24">`)}
      <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">计算</button></div>
    </div>
    <p class="tool-status" data-status role="status" aria-live="polite">支持 IPv4 /0 到 /32。</p>
    <dl class="tool-dl" data-out></dl>
  </section>`,
  ice: () => `<section class="tool-work" data-panel="webrtc" data-handler="webrtc">
    <div class="tool-grid tool-grid-2">
      ${field("STUN 服务器", `<input data-opt="stun" value="stun:stun.l.google.com:19302" spellcheck="false">`)}
      ${field("检测", `<select data-opt="mode"><option value="ice">ICE 候选收集</option><option value="leak">IP 泄露检查</option></select>`)}
    </div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">开始检测</button><button class="cmd-btn" data-act="stop">停止</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">会连接你填写的 STUN 服务器；不填写则只用本地候选。</p>
    <pre class="tool-pre" data-out></pre>
  </section>`,
  qr: () => `<section class="tool-work" data-panel="qr-code-generator" data-handler="qr-code-generator">
    <div class="tool-field"><span>文本或 URL</span><textarea data-input rows="4" spellcheck="false">https://zuhejian.com/</textarea></div>
    <div class="tool-row">
      ${field("尺寸", `<input type="number" data-opt="size" min="128" max="1024" value="320">`)}
      ${field("容错级别", `<select data-opt="level"><option>M</option><option>L</option><option>Q</option><option>H</option></select>`)}
      <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">生成</button><button class="cmd-btn" data-act="download">下载 PNG</button></div>
    </div>
    <p class="tool-status" data-status role="status" aria-live="polite">二维码在本地用 Canvas 绘制。</p>
    <div class="tool-preview tool-preview-center" data-preview></div>
  </section>`,
  favicon: () => `<section class="tool-work" data-panel="favicon-generator" data-handler="favicon-generator">
    ${field("选择方形图片（建议 512×512 以上）", `<input type="file" data-opt="file" accept="image/*">`)}
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">生成</button><button class="cmd-btn" data-act="download">下载 ICO</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">生成 16/32/48/64/128/256 多尺寸 ICO，全程本地完成。</p>
    <div class="tool-preview tool-preview-grid" data-preview></div>
  </section>`,
  imageConvert: (tool) => `<section class="tool-work" data-panel="${attr(tool.id)}" data-handler="${attr(tool.id)}">
    ${field("选择图片（可多选）", `<input type="file" data-opt="file" accept="image/*,.heic,.heif" multiple>`)}
    <div class="tool-opts">${(tool.ui.opts || []).map(optHtml).join("")}</div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">转换</button><button class="cmd-btn" data-act="download">下载全部</button></div>
    <p class="tool-status" data-status role="status" aria-live="polite">图片在本地用 Canvas 处理，不会上传。</p>
    <div class="tool-preview tool-preview-grid" data-preview></div>
  </section>`,
  videoConvert: (tool) => `<section class="tool-work" data-panel="${attr(tool.id)}" data-handler="${attr(tool.id)}">
    ${field("选择视频", `<input type="file" data-opt="file" accept="video/*,.mkv,.mov,.webm,.mp4,.gif">`)}
    <div class="tool-opts">${(tool.ui.opts || []).map(optHtml).join("")}</div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">开始转换</button><button class="cmd-btn" data-act="download" disabled>下载结果</button></div>
    <progress class="tool-progress" data-progress value="0" max="100" hidden></progress>
    <p class="tool-status" data-status role="status" aria-live="polite">需要支持 WebCodecs 的浏览器（Chrome / Edge 较新版本）。大文件耗时较长，请保持页面在前台。</p>
    <div class="tool-preview" data-preview></div>
  </section>`,
  videoGif: (tool) => `<section class="tool-work" data-panel="${attr(tool.id)}" data-handler="${attr(tool.id)}">
    ${field("选择视频", `<input type="file" data-opt="file" accept="video/*">`)}
    <div class="tool-opts">${(tool.ui.opts || []).map(optHtml).join("")}</div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">生成 GIF</button><button class="cmd-btn" data-act="download" disabled>下载 GIF</button></div>
    <progress class="tool-progress" data-progress value="0" max="100" hidden></progress>
    <p class="tool-status" data-status role="status" aria-live="polite">按设定帧率截取前 5 秒，生成过程在本地完成。</p>
    <div class="tool-preview" data-preview></div>
  </section>`,
  videoTrim: (tool) => `<section class="tool-work" data-panel="${attr(tool.id)}" data-handler="${attr(tool.id)}">
    ${field("选择视频", `<input type="file" data-opt="file" accept="video/*">`)}
    <div class="tool-opts">${(tool.ui.opts || []).map(optHtml).join("")}</div>
    <div class="tool-actions"><button class="cmd-btn cmd-btn-primary" data-act="run">剪切</button><button class="cmd-btn" data-act="download" disabled>下载结果</button></div>
    <progress class="tool-progress" data-progress value="0" max="100" hidden></progress>
    <p class="tool-status" data-status role="status" aria-live="polite">优先无损重封装；编码不兼容时自动转为 H.264。</p>
    <div class="tool-preview" data-preview></div>
  </section>`,
  notice: (tool) => noticePanel(tool),
  fileDrop: () => `<section class="tool-work" data-panel="file-drop" data-handler="file-drop">
    <div class="cmd-callout">
      <strong>文件会发送到 FileBox</strong>
      <p>上传接口是 <code>filebox.zhcn.xyz</code>（FileCodeBox），文件会离开浏览器。单文件上限 50 MB，有效期最长 7 天，请勿上传敏感内容。</p>
    </div>
    ${field("选择文件（最大 50 MB）", `<input type="file" data-opt="file">`)}
    <div class="tool-grid tool-grid-2">
      ${field("有效期单位", `<select data-opt="expireStyle"><option value="day">天</option><option value="hour">小时</option><option value="minute">分钟</option><option value="forever">永久</option><option value="count">下载次数</option></select>`)}
      ${field("有效期数值", `<input type="number" data-opt="expireValue" value="1" min="1" max="100">`)}
    </div>
    <div class="tool-actions">
      <button class="cmd-btn cmd-btn-primary" type="button" data-act="run">上传到 FileBox</button>
      <button class="cmd-btn" type="button" data-act="copy">复制链接</button>
      <a class="cmd-btn" href="https://filebox.zhcn.xyz/" target="_blank" rel="noopener noreferrer">打开 FileBox</a>
    </div>
    <progress class="tool-progress" data-progress value="0" max="100" hidden></progress>
    <p class="tool-status" data-status role="status" aria-live="polite">上传后会返回提取码和分享链接，并生成二维码。</p>
    <dl class="tool-dl" data-result hidden></dl>
    <div class="tool-preview tool-preview-center" data-preview></div>
  </section>`,
  ascii: asciiTable,
  http: httpTable
};

function panelFor(tool) {
  const ui = tool.ui;
  if (ui.k === "table") return ui.panel === "ascii" ? asciiTable() : httpTable();
  if (ui.k === "notice") return noticePanel(tool);
  if (ui.k === "custom") {
    const fn = PANELS[ui.panel];
    if (!fn) throw new Error(`missing panel ${ui.panel} for ${tool.id}`);
    return fn(tool);
  }
  return textPanel(tool);
}

const GUIDE = {
  color: "十六进制与 RGB 是同一色彩空间的不同写法；OKLCH 更接近人眼感知，适合做调色和明暗阶梯。对比度按 WCAG 相对亮度计算，正文需 ≥ 4.5。",
  encode: "Base64 不是加密，只是编码；URL 组件编码与整段 URL 编码要区分，混用是签名失败最常见的原因。",
  format: "格式化不会修复非法语法。JSON 的尾逗号、注释和单引号都会报错；SQL、HTML、CSS 的大小写和空白处理请以目标解析器为准。",
  text: "处理用户文本前先明确是否区分大小写、是否忽略首尾空格。差异比较按行进行，不处理词级高亮。",
  css: "CSS 生成器输出的是标准属性，复制后请在实际浏览器中确认，尤其是渐变角度和阴影颜色空间。",
  security: "哈希是单向摘要，不能解密；HMAC 需要密钥；AES 使用密码派生密钥，密文包含随机盐和 IV。不要把生产密钥粘贴到任何在线页面，包括本页。",
  debug: "Cron 按服务器本地时区解释，不包含秒字段。UA 和 Cookie 的格式由客户端决定，解析结果只用于调试。",
  generate: "生成器输出的是模板或静态资源，下载前请检查内容是否符合项目规范。",
  ai: "Token 数是估算值，真实计费以模型提供商的 tokenizer 为准；成本计算不包含缓存、批处理折扣和网络费用。",
  number: "进制转换按无符号整数处理；负数请使用补码表示。按位运算受 32 位限制。",
  image: "图片处理依赖浏览器 Canvas 和 WebCodecs，格式支持因浏览器而异；导出前请检查尺寸和透明通道。",
  video: "视频转码依赖 WebCodecs，Chrome / Edge 支持较好，Safari 和 Firefox 可能不支持。大文件会占用较多内存。",
  network: "WebRTC 检测会连接 STUN 服务器以收集公网候选；其余工具不发起网络请求。",
  reference: "参考表按标准整理，具体行为以你的系统版本和服务器配置为准。"
};

const ICON_COPY =
  '<svg viewBox="0 0 24 24" aria-hidden="true"><rect x="9" y="9" width="11" height="11" rx="2.5" fill="none" stroke="currentColor" stroke-width="1.7"/><path d="M15 6.5V5.5A2.5 2.5 0 0 0 12.5 3h-7A2.5 2.5 0 0 0 3 5.5v7A2.5 2.5 0 0 0 5.5 15h1" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round"/></svg>';
const ICON_CLEAR =
  '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M6 6l12 12M18 6L6 18" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/></svg>';

function cardLabel(tool) {
  const ascii = (tool.tags || []).find((t) => /^[\x20-\x7E]+$/.test(t));
  if (ascii) return ascii.toUpperCase();
  return String(tool.id).split("-")[0].slice(0, 9).toUpperCase();
}

function cardHead(tool) {
  return `<div class="tw-head">
      <span class="tw-id">${esc(cardLabel(tool))}</span>
      <span class="tw-chip">${esc(catName(tool.cat))}</span>
      <div class="tw-head-tools">
        <button class="tw-icon" type="button" data-act="copy" title="复制结果" aria-label="复制结果">${ICON_COPY}</button>
        <button class="tw-icon" type="button" data-act="clear" title="清空输入" aria-label="清空输入">${ICON_CLEAR}</button>
      </div>
    </div>`;
}

/* 找出某个 <div ...> 的闭合位置（允许嵌套） */
function findDivEnd(src, startIdx) {
  const re = /<div\b|<\/div>/g;
  re.lastIndex = startIdx;
  let depth = 0;
  let m;
  while ((m = re.exec(src))) {
    if (m[0] === "</div>") {
      depth -= 1;
      if (depth === 0) return re.lastIndex;
    } else {
      depth += 1;
    }
  }
  return -1;
}

/* 互为逆操作的按钮对：点了反方向就把结果搬回输入，省一次复制粘贴 */
const SWAP_PAIRS = [
  ["encode", "decode"],
  ["escape", "unescape"],
  ["encrypt", "decrypt"],
  ["toJson", "toXml"],
  ["toYaml", "toJson"],
  ["toJson", "toToml"],
  ["md2html", "html2md"],
  ["toBinary", "toText"],
  ["run", "format"],
  ["run", "beautify"]
];

/* 选项不超过 4 个的下拉改成一行分段按钮；只剩一个选项的直接去掉下拉 */
function enhanceSelects(panel) {
  return panel.replace(
    /<div class="tool-field"><span>([^<]*)<\/span><select data-opt="([^"]+)">([\s\S]*?)<\/select><\/div>/g,
    (whole, label, key, inner) => {
      const options = [...inner.matchAll(/<option([^>]*)>([^<]*)<\/option>/g)].map(([, attrs, text]) => {
        const value = /value="([^"]*)"/.exec(attrs);
        return { value: value ? value[1] : text, label: text, selected: /\bselected\b/.test(attrs) };
      });
      if (!options.length) return whole;
      const current = options.find((o) => o.selected) || options[0];
      if (options.length === 1) {
        return `<input type="hidden" data-opt="${attr(key)}" value="${attr(current.value)}">`;
      }
      if (options.length > 4) return whole;
      const buttons = options
        .map((o) => `<button class="tw-seg-btn" type="button" data-seg-val="${attr(o.value)}" aria-pressed="${o.value === current.value}">${esc(o.label)}</button>`)
        .join("");
      return `<div class="tool-field"><span>${label}</span><div class="tool-seg" data-seg="${attr(key)}"><input type="hidden" data-opt="${attr(key)}" value="${attr(current.value)}">${buttons}</div></div>`;
    }
  );
}

/* 统一卡片骨架：卡片头 → 操作条 → 内容 → 状态条 */
function enhancePanel(panel) {
  if (!/<footer class="tw-foot">/.test(panel)) {
    panel = panel.replace(
      /<\/section>\s*$/,
      () => `<footer class="tw-foot"><div class="tw-meta"><span class="tw-local">本地处理</span><span class="tw-local">无需登录 · 刷新即清空</span></div></footer>\n        </section>`
    );
  }

  // 少量窄参数收进操作条，避免只剩一个数字框时被拉满整行；
  // 参数多或含分段按钮时留在原位，否则会把操作条撑成好几行
  let opts = "";
  const optsMatch = panel.match(/<div class="tool-opts">/);
  if (optsMatch) {
    const start = optsMatch.index;
    const end = findDivEnd(panel, start);
    if (end > start) {
      const block = panel.slice(start, end);
      // 滑块也带 tool-field / tool-range，需要一起计数
      const count = (block.match(/class="tool-field\b/g) || []).length;
      if (count <= 3) {
        opts = block;
        panel = panel.slice(0, start) + panel.slice(end);
      }
    }
  }

  // 把操作行从原来的位置摘出来
  const actionsMatch = panel.match(/<div class="(?:tool|tw)-actions">/);
  let actions = "";
  if (actionsMatch) {
    const start = actionsMatch.index;
    const end = findDivEnd(panel, start);
    if (end > start) {
      actions = panel.slice(start, end);
      panel = panel.slice(0, start) + panel.slice(end);
    }
  }

  if (actions) {
    const buttons = actions.match(/<button\b[\s\S]*?<\/button>/g) || [];
    const AUX = ["clear", "copy", "sample", "download"];
    const actOf = (b) => (b.match(/data-act="([^"]+)"/) || [])[1] || "";
    const main = buttons.filter((b) => !AUX.includes(actOf(b)));
    const aux = buttons.filter((b) => AUX.includes(actOf(b)));
    const acts = main.map(actOf);
    const pair = acts.length === 2 && SWAP_PAIRS.some(([a, b]) => acts.includes(a) && acts.includes(b));

    if (acts.length === 2) {
      // 两个互斥动作做成一行分段按钮，点哪边就是哪边
      const segButtons = main
        .map((b, i) => b.replace(/class="[^"]*"/, 'class="tw-seg-btn"').replace(/<button /, `<button aria-pressed="${i === 0}" `))
        .join("");
      actions = `<div class="tw-seg${pair ? " tw-seg-swap" : ""}">${segButtons}</div>${aux.join("")}`;
    } else {
      actions = `<div class="tw-actions">${main.join("")}${aux.join("")}</div>`;
    }

    const headEnd = findDivEnd(panel, panel.indexOf('<div class="tw-head"'));
    if (headEnd > 0) {
      panel = panel.slice(0, headEnd) + `\n    <div class="tw-bar">${actions}${opts}</div>` + panel.slice(headEnd);
    }
  }

  // 底栏只留状态
  panel = panel.replace(/<footer class="tw-foot">\s*<\/footer>/, `<footer class="tw-foot"><div class="tw-meta"><span class="tw-local">本地处理</span><span class="tw-local">无需登录 · 刷新即清空</span></div></footer>`);

  return enhanceSelects(panel);
}


/* 用到的第三方库要在页面上写清楚：库名、许可证、只在浏览器本地跑 */
function libNoteHtml(tool) {
  const libs = [...new Set([...(tool.libs || []), ...((tool.ui && tool.ui.libs) || [])])].filter((l) => LIB_META[l]);
  if (!libs.length) return "";
  const items = libs.map((l) => `${LIB_META[l][0]}（${LIB_META[l][1]}）`).join("、");
  return `<section class="tool-note"><h2>第三方依赖</h2><p>本工具使用开源库 ${items}，已随站点打包，全部在你的浏览器里运行，不会把输入发送到这些库的任何服务。</p></section>`;
}

function faqItems(tool) {
  const remote =
    tool.id === "file-drop"
      ? "会。文件会上传到 FileBox，其余工具不会发起上传；提取码和链接只保存在当前页面。"
      : tool.id === "webrtc-ice-test" || tool.id === "webrtc-leak-test"
        ? "不会上传。检测会连接你选定的 STUN 服务器以收集候选地址，页面本身不保存任何结果。"
        : "不会。所有计算都在浏览器里完成，输入不离开本机，刷新页面即清空。";
  return [
    ["输入的数据会被上传吗？", remote],
    [
      "支持哪些浏览器？",
      tool.cat === "video"
        ? "视频类工具依赖 WebCodecs，建议使用较新的 Chrome 或 Edge；Safari 和 Firefox 可能无法转码。"
        : "支持 Chrome、Edge、Firefox 和 Safari 的近期版本。哈希、加密和二维码使用 Web Crypto 与 Canvas，需要 HTTPS 或 localhost。"
    ],
    [
      "结果和预期不一致怎么办？",
      "先在下方「使用提示」确认边界条件，再到 GitHub Issues 反馈并附上输入样本，方便复现。"
    ]
  ];
}

function toolPage(tool) {
  const siblings = TOOLS.filter((t) => t.cat === tool.cat && t.id !== tool.id);
  const commands = COMMAND_LINKS[tool.cat] || [];
  return `${head({
    title: tool.name,
    description: `${tool.desc} 在浏览器本地运行，不上传输入内容。`,
    url: canonical(tool),
    page: "tool",
    tool
  })}
  <main class="cmd-main tool-main" id="main">
    <nav class="cmd-breadcrumb" aria-label="面包屑"><a href="../">快捷键大全</a><span>/</span><a href="./">开发者工具</a><span>/</span><span aria-current="page">${esc(tool.name)}</span></nav>
    <div class="tool-layout">
      <aside class="tool-aside" aria-label="工具导航">
        <div class="tool-aside-block">
          <h2>分类</h2>
          <div class="tool-aside-links">
            <a href="./">← 全部 ${TOOLS.length} 个工具</a>
            <span class="tool-aside-current">${esc(catName(tool.cat))}</span>
          </div>
        </div>
        <div class="tool-aside-block">
          <h2>相关命令</h2>
          <div class="tool-aside-links">${commands.map((c) => `<a href="${attr(c.href)}">${esc(c.label)}</a>`).join("")}</div>
        </div>
        <div class="tool-aside-block">
          <h2>同分类</h2>
          <div class="tool-aside-links">${siblings.map((t) => `<a href="./${t.id}">${esc(t.name)}</a>`).join("")}</div>
        </div>
      </aside>
      <div class="tool-content">
        <header class="cmd-hero tool-hero">
          <p class="cmd-eyebrow">${esc(catName(tool.cat))}</p>
          <h1>${esc(tool.name)}</h1>
          <p class="cmd-lead">${esc(tool.desc)}</p>
          <p class="cmd-meta"><span>本地运行</span><span>无需登录</span><span>更新 ${UPDATED}</span></p>
        </header>
        ${tool.id === "file-drop"
          ? '<p class="tool-privacy">文件会发送到 FileBox；提取码和链接只保存在当前页面。</p>'
          : '<p class="tool-privacy">本地运行，输入不会上传，刷新即清空。</p>'}
        ${(function () {
          let panel = panelFor(tool)
            .replace('class="tool-work"', 'class="tool-work tw-card"')
            .replace(/(<section class="tool-work tw-card"[^>]*>)/, `$1\n          ${cardHead(tool)}`);
          return enhancePanel(panel);
        })()}
        ${libNoteHtml(tool)}
        <section class="tool-note"><h2>使用提示</h2><p>${esc(GUIDE[tool.cat] || tool.desc)}</p></section>
      </div>
    </div>
  </main>
${foot()}`;
}

function hubPage() {
  const usable = TOOLS.filter((t) => t.ui.k !== "notice").length;
  const cards = CATEGORIES.map((cat, index) => {
    const tools = TOOLS.filter((t) => t.cat === cat.id);
    if (!tools.length) return "";
    return `<details class="tool-group" data-cat="${attr(cat.id)}"${index < 4 ? " open" : ""}>
      <summary class="tool-group-head"><h2>${esc(cat.name)}</h2><span class="cmd-section-count">${tools.length} 个工具</span></summary>
      <p class="cmd-section-intro">${esc(cat.desc)}</p>
      <div class="cmd-cards tool-cards">
        ${tools.map((t) => `<a class="cmd-card tool-card" href="./${t.id}" data-keywords="${attr(t.keywords)}" data-tool-name="${attr(t.name)}">
          <h3>${esc(t.name)}</h3>
          <p>${esc(t.desc)}</p>
          <span class="tool-card-tags">${t.tags.map((tag) => `<span class="cmd-tag">${esc(tag)}</span>`).join("")}</span>
        </a>`).join("\n        ")}
      </div>
    </details>`;
  }).join("\n    ");

  return `${head({
    title: "开发者工具",
    description: `${TOOLS.length} 个开发者工具：JSON 格式化、Base64、URL 编解码、颜色、时间戳、哈希、JWT、正则、Cron、子网、图片和视频转换，全部在浏览器本地运行。`,
    url: canonical(null),
    page: "hub"
  })}
  <main class="cmd-main" id="main">
    <header class="cmd-hero cmd-hero-hub">
      <p class="cmd-eyebrow">本地运行 · 无需登录 · 不上传数据</p>
      <h1>开发者工具</h1>
      <p class="cmd-lead">${TOOLS.length} 个开发与调试时最常打开的小工具，覆盖颜色、编码、格式化、文本、CSS、安全、调试、生成器、AI、数字、图像、视频和网络。全部在浏览器本地计算。</p>
      <p class="cmd-meta"><span>${TOOLS.length} 个工具</span><span>${CATEGORIES.length} 个分类</span><span>${usable} 个直接可用</span><span>更新 ${UPDATED}</span></p>
    </header>
    <div class="cmd-callout">
      <strong>为什么是本地工具</strong>
      <p>调试 JWT、哈希和编码时经常涉及令牌和密钥片段。除 WebRTC 检测连接 STUN、文件分享上传到 FileBox 外，这些工具不发起网络请求，输入不会离开浏览器；但请不要在生产环境粘贴真实密钥。</p>
    </div>
    <div class="tool-search-card">
      <div class="tool-search" role="search">
        <svg class="tool-search-icon" viewBox="0 0 24 24" aria-hidden="true"><path d="M21 21l-4.35-4.35M11 18a7 7 0 1 1 0-14 7 7 0 0 1 0 14z" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round"/></svg>
        <input id="tool-search" type="search" autocomplete="off" spellcheck="false" placeholder="共 ${TOOLS.length} 个工具，试试「json」「base64」「时间戳」…" aria-label="搜索工具">
        <button class="tool-search-clear" id="tool-search-clear" type="button" aria-label="清空搜索" hidden>×</button>
      </div>
    </div>
    <div class="tool-filters" id="tool-filters" role="group" aria-label="按分类筛选">
      <button class="tool-chip is-active" type="button" data-cat-filter="" aria-pressed="true">全部 <span>${TOOLS.length}</span></button>
      ${CATEGORIES.map((c) => `<button class="tool-chip" type="button" data-cat-filter="${attr(c.id)}" aria-pressed="false">${esc(c.name)} <span>${TOOLS.filter((t) => t.cat === c.id).length}</span></button>`).join("")}
    </div>
    <p class="cmd-count" id="tool-count" aria-live="polite">显示 ${TOOLS.length} / ${TOOLS.length} 个工具</p>
    <div id="tool-groups">${cards}</div>
    <p class="cmd-empty" id="tool-empty" hidden>没有匹配的工具，试试 json、编码或时间戳。</p>
    <section class="cmd-sources">
      <h2>使用说明</h2>
      <ul class="cmd-plain-list">
        <li>所有工具都是静态页面，输入不离开浏览器；刷新即清空，不需要登录。</li>
        <li>哈希、随机数、AES 和 HMAC 使用 Web Crypto，需要 HTTPS 或 localhost。</li>
        <li>视频转码依赖 WebCodecs，建议使用较新的 Chrome 或 Edge；Safari / Firefox 可能不可用。</li>
        <li>JWT 工具只解码、不校验签名；判断令牌是否有效必须由服务端完成。</li>
        <li>文件分享会把文件上传到 FileBox（单文件 50 MB，最长 7 天），上传后返回提取码、链接和二维码。</li>
      </ul>
      <p class="cmd-sources-note">需要补充工具或发现计算错误，请在 <a href="https://github.com/wensley/HotkeyInspector/issues" target="_blank" rel="noopener noreferrer">GitHub Issues</a> 反馈。</p>
    </section>
  </main>
${foot()}`;
}

for (const tool of TOOLS) {
  if (!CATEGORIES.some((c) => c.id === tool.cat)) throw new Error(`bad category ${tool.id}/${tool.cat}`);
  if (tool.ui.k === "custom" && !PANELS[tool.ui.panel]) throw new Error(`missing panel ${tool.ui.panel}`);
  for (const lib of [...(tool.libs || []), ...((tool.ui && tool.ui.libs) || [])]) {
    if (!LIB_FILES[lib]) throw new Error(`unknown lib ${lib} (${tool.id})`);
  }
}

writeFileSync(join(outDir, "index.html"), hubPage(), "utf8");
for (const tool of TOOLS) {
  writeFileSync(join(outDir, `${tool.id}.html`), toolPage(tool), "utf8");
}
console.log(`devtools: wrote ${TOOLS.length + 1} pages, ${TOOLS.length} tools (${TOOLS.filter((t) => t.ui.k !== "notice").length} usable)`);
