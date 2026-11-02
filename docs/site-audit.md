# docs/hotkeys 前端审计报告

审计日期：2026-10-20  
审计范围：`docs/hotkeys/`（index.html、app.js、styles.css、data/*.js、sitemap.html/xml、robots.txt、CHANGELOG、CONTRIBUTING）及 `tools/extract-kb.mjs` 的生成关系。  
审计方式：本地静态服务器 + Chrome DevTools（DOM/性能/键盘/对比度）、Lighthouse（desktop）、Node 数据校验、生产站点抽查（https://zuhejian.com）。  
结论：数据与基础工程质量尚可（XSS 防护完整、i18n 键齐全、计数一致），但存在 **2 个 P0 级问题**：搜索引擎无法抓取页面内容；快捷键识别器对约 12.6% 的数据无法匹配。另有性能、键盘可访问性、收藏碰撞、双语 SEO、移动端溢出等 P1 问题。

> **整改状态（2026-10-20）**：P0-1、P0-2 与 P1-1～P1-6 已修复并验证；P2 已补主题初始化、storage 防护、导出细节、CSP、分析同意、og:image、静态预渲染与渐进渲染。站点已部署为 Cloudflare Worker `hotkeyinspector-site`，`zuhejian.com` 与 `www.zuhejian.com` 路由已切换；源码另存私有仓库 `wensley/HotkeyInspector-site`。逐项记录见文末「九、整改记录」。

## 一、总体结论

| 级别 | 数量 | 问题摘要 |
| --- | --- | --- |
| P0 | 2 | robots.txt 阻断渲染所需 JS；识别器无法匹配标点键、序列键、Page Up/Down 等约 385 条数据 |
| P1 | 6 | 首屏 3056 行全量渲染且每次输入全量重绘；侧栏/TOC 键盘不可达、识别器拦截 Tab/Enter、对比度大面积失败；53 个应用内重复组合键导致收藏/识别语义碰撞；语言切换与 canonical/hreflang 不一致；保留键提示表缺失 16 项；375px 横向溢出 40px |
| P2 | 8 | 主题闪烁；`docs/hotkeys` 未纳入版本库且线上版本落后；localStorage 未全程防护；收藏导出/搜索 URL/hash 等细节；双追踪脚本无同意；缺少 CSP/og:image；文档与数据漂移；`sitemap.html` 细节 |

数据实测：9 个分类、60 个应用、304 个分组、**3056 条**快捷键、**797 个**不同组合、**53 条**应用内重复 `app+key`、**36 条**含空格/序列的键、**349 条**含识别器不会生成的 token（合计约 385 条、12.6% 无法被识别器命中）。

## 二、P0：必须先修

### P0-1 robots.txt 阻止 Google 抓取全部内容（SEO 致命）

`robots.txt:4-6`：

```
Disallow: /app.js
Disallow: /data*.js
Disallow: /data/
```

页面正文全部由 `data/*.js` + `app.js` 在客户端渲染，`index.html:123` 的 `<main id="content" tabindex="-1"></main>` 初始为空；`index.html:187-198` 依次加载 11 个数据脚本与 `app.js`。上述规则把渲染所需的全部脚本都挡在 Googlebot 之外：

- `/data*.js` 的 `*` 会匹配 `/data/os.js`、`/data/office.js` 等；
- `/data/` 与上一条重复；
- `/app.js` 被单独禁止。

生产站点实测（2026-10-20）robots.txt 与本文件一致，首页 `<main>` 同样为空，因此 Google 即使抓取首页，也无法执行 JS 得到 3056 条内容。Lighthouse SEO 100 分不能发现这一问题（它不模拟 robots 对子资源的阻断）。

建议：删除 `Disallow: /app.js`、`/data*.js`、`/data/` 三条（至少允许 JS/CSS），保留 `Sitemap`；部署后用 Search Console「网址检查 → 查看已渲染 HTML」验证正文可见，并请求重新编入索引。更彻底的方案是构建时预渲染静态 HTML（见 P2-5）。

### P0-2 识别器无法匹配约 12.6% 的快捷键

识别器把 `KeyboardEvent.code` 转成 `BracketLeft`、`Slash`、`Comma` 等 token（`app.js:1121-1135`），而数据里大量使用字面符号（`Ctrl+]`、`Ctrl+/`、`Ctrl+,`、`Ctrl+;`、`Ctrl+\`、Ctrl+反引号 等）；`normCombo()`（`app.js:1167-1177`）与 `KEY_SYNONYM`（`app.js:1143-1156`）没有做双向映射，导致 `keyIndex` 的键与识别器生成的键不相等。

实测：按 `Ctrl+[` 时识别器显示「Ctrl+[」但结果为 **0 条**，而搜索 `ctrl+[` 能找到 **12 条**（知识库中确有该组合）。静态比对结果：

- **349 条**含识别器不会生成的 token（标点、`Ctrl+加号`、`Ctrl+1-9` 等）；
- **36 条**为序列/带空格键名（`Ctrl+K Ctrl+B`、`git add .`、`Page Up`、`Page Down`、`Shift Shift`、`Right Shift(按住八秒)` 等）；
- 去重后 **249 个**不同键值受影响，约占全部条目的 12.6%。

此外：

- `Page Down`/`Page Up` 被 `split(/[+\s]+/)` 拆成 `Page+Down`，与识别器的 `PageDown/PageUp` 永远不相等（`app.js:1168`）。
- 序列键会被错误归一化：`Ctrl+K Ctrl+B → Ctrl+K+B`，`Ctrl+M Ctrl+M → Ctrl+M+M`，`Shift Shift → Shift`；`renderKeys()` 还按 `+` 切分，显示成 `Ctrl + K Ctrl + B`（`app.js:229-235`）。
- macOS 的 `Cmd` 被归一化为 `Win`（`KEY_SYNONYM`），识别器在 Mac 上也显示 `Win`，与数据展示的 `Cmd` 不一致。
- `Ctrl+加号` 这类中文键名无法由键盘事件生成。

建议：

1. 建立唯一的规范化词典：标点双向映射（`] ↔ BracketRight`、`/ ↔ Slash`、`,` ↔ Comma、`- ↔ Minus`、`= ↔ Equal`……），并让 `renderKeys` 只做展示映射。
2. 数据模型区分「单步组合」与「序列」：如 `{ k: "Ctrl+K, Ctrl+B", seq: ["Ctrl+K","Ctrl+B"] }`，序列在识别器中明确提示「需分步按下」，不要伪装成单步。
3. 修正 `Page Up/Down`、`Shift Shift`、范围条目（`Ctrl+1-9`）的存储方式；`Page Up/Down` 统一为 token，范围条目改成说明文本或展开成独立条目。
4. 按平台展示修饰键：macOS 显示 `Cmd/Option`，Windows 显示 `Win/Alt`。
5. 为识别器/归一化补单元测试，至少覆盖全部 797 个组合的 `normalize → keyIndex` 往返。

## 三、P1：影响体验、可访问性与数据语义

### P1-1 首屏与搜索性能：全量 DOM + 每次按键全量重绘

实测（桌面、本地 HTTP）：

- DOM：**49,528 个节点**、**6,199 个 button**、**3,056 个 `.kb-row`**；`document.documentElement.outerHTML` 约 **3.17 MB**。
- 初次加载：DCL 685 ms、load 741 ms、FCP 200 ms；JS 传输约 407 KB（未压缩，另含 GA）。
- 搜索输入：空查询 279 ms、`c` 224 ms、`ctrl` 148 ms、清空 263 ms；在 4× CPU 降速下分别约 **2.6 s / 1.6 s / 2.9 s**。
- 一次完整渲染触发 **3,061 次 `localStorage.getItem`**；输入 `c` 后再增加约 2,639 次；50 条收藏下三次搜索累计约 21,000 次。

根因：

- `favHas()` 每渲染一行都调用 `favAll()` → `favEnsure()` → `favData()` → `JSON.parse(localStorage)`（`app.js:103-129`、`app.js:416`）。3056 行就是 3056 次解析。
- 输入事件没有 debounce，直接 `renderSidebar + renderContent + renderToc`（`app.js:844-851`），每次重建整页。
- `visibleSidebarItems()` 在侧栏、目录、计数中被重复计算；`renderToc()` 与 `renderAll()` 各调用一次 `obsLinks()`（`app.js:698`、`app.js:830`）。
- 所有内容一次性 `innerHTML` 注入，没有分段/虚拟化/`content-visibility`。

建议（按收益排序）：

1. 用 `Set` 缓存收藏 id，`favHas` O(1)，渲染开始时只读一次 localStorage。
2. 输入 debounce 120–150 ms；查询无变化时跳过渲染。
3. 结果只渲染匹配项，侧栏/目录用已计算的结果复用，避免三次全量遍历。
4. 给 `.app` 加 `content-visibility: auto; contain-intrinsic-size`；或对超长分类做虚拟列表/分页。
5. 首屏目标：DOM < 10k、输入 p95 < 100 ms（桌面）/ < 300 ms（4× CPU）。

### P1-2 键盘与无障碍：导航不可 Tab、识别器吞掉所有按键、对比度大面积失败

- **728 个 `<a>` 没有 `href`**：侧栏 364 个、TOC 364 个（`app.js:382-393`、`app.js:684-691` 生成时只写 `data-target`）。实测 `sideLink.focus()` / `tocLink.focus()` 均失败，键盘用户无法通过 Tab 进入侧栏和目录，也无法使用「跳到某应用」。应改为 `href="#<id>"`（保留 click 增强）或 `tabindex="0"` + `role="link"`，并维护 `aria-current`。
- **识别器把 Tab/Enter/Space 全部取消**：`app.js:1394-1401` 对所有 keydown 执行 `preventDefault()` + `stopPropagation()`。实测打开识别器后按 Tab，焦点仍停在 `#detector-capture`；对话框内的关闭、锁定、保留键按钮都无法通过键盘到达/激活。应只在焦点位于捕获区且按键不是 Tab/Escape 时拦截；实现真正的 focus trap、关闭后归还焦点，并给背景加 `inert`。
- **对话框语义不完整**：`role="dialog" aria-modal="true"` 在外层 `.detector-card`（`index.html:156`），但捕获区是 `div tabindex="0"` 且无 role/名称（`index.html:160`），CSS 还设了 `outline:none`（`styles.css:1273`），键盘焦点不可见。
- **对比度**：Lighthouse 报 **1,254 个节点**不达标。`--text-3: #999999` 在 `#f8f8f8` 上约 **2.68:1**（12px 计数/目录），子计数 `#a7a7a7` 约 **2.26:1**，暗色 `#6e6e73` 在 `#0d0d0d` 上约 **3.56–3.83:1**；`styles.css:1-90` 定义。建议把 `text-3` 提到约 `#6b6b6b`（亮）/ `#9a9aa0`（暗），并对所有小号文字复测 4.5:1。
- **aria-label 与页面语言不一致**：中文界面仍有英文可访问名称，如 `Shortcut detector`、`Toggle navigation`、`Language`、`Popular apps`、`Documentation navigation`、`On this page`、`Close`、`clear`（`index.html:45,55,71,74,92,118,128,157`）。搜索/主题/收藏等少数按钮会在 `renderChrome()` 里更新，其余不会；应全部走 `I18N`。
- **搜索框没有 label**：`index.html:69` 只有 placeholder；placeholder 不能替代持久标签，输入后即消失。建议加 `<label class="sr-only" for="search">` 或 `aria-label`。
- **语言按钮无状态**：`index.html:74-77` 仅靠 `.active` 类表达当前语言，缺少 `aria-pressed`/`aria-current`；`sitemap.html` 同样问题。
- **其他**：`#to-top` 在不可见时 `aria-hidden="true"` 且 `visibility:hidden`，这一点实现正确；但全站没有 `prefers-reduced-motion`，平滑滚动/抽屉/弹窗动画均未降级；快捷键行是无语义的 `div`，3056 行没有列表/表格语义，屏幕阅读器难以按表浏览。

建议目标：axe/Lighthouse 对比度 0 失败；仅用键盘可完成「侧栏跳转 → 搜索 → 打开识别器 → 关闭」；屏幕阅读器能听到中文名称与当前语言。

### P1-3 应用内重复组合键导致收藏与识别碰撞（53 条）

同一应用内同一 `k` 出现多次，例如：

- `powerpoint|Ctrl+P`：「打印」与「切换画笔工具」；
- `powerpoint|Ctrl+L`：「左对齐」与「切换激光笔」；
- `windows|Backspace/Delete/Home/End`：文本编辑与资源管理器两种上下文；
- `discord|Esc`、`wps|Ctrl+1`、`notion|Ctrl+K` 等。

`favId()` 只用 `app.id + "|" + it.k`（`app.js:101`），`buildIndex()` 也按该键建 `FAV_INDEX`（`app.js:1224`）。实测给 PowerPoint 的「打印」加星后，重新渲染时两条 `Ctrl+P` 都显示已收藏，但 localStorage 只有 1 条；再点另一条会取消同一条收藏。识别器同样会显示重复命中。

建议：id 加入分组/动作上下文（如 `app|group|k|zh` 的稳定哈希），或在数据层合并重复项为「上下文说明」数组；识别器结果去重。

### P1-4 语言切换与 canonical/hreflang 不一致（双语 SEO 基本无效）

- 切换语言只改 `state.lang`、`document.documentElement.lang`、`document.title`（`app.js:779-794`、`app.js:835-841`），**不改 URL**；分享或刷新后回到旧语言。
- 实测：访问 `/?lang=en` → 点「中」→ 刷新，页面又变回英文（`?lang=en` 在 `app.js:1461-1475` 覆盖了 localStorage 选择）。
- `canonical` 始终是 `https://zuhejian.com/`（`index.html:22`），英文 `?lang=en` 的 canonical 指向中文页；`hreflang="en"`（`index.html:25`）与 canonical 冲突，Google 很可能把英文版视为重复页而不索引。
- 切换语言后 `meta description`、`og:title/og:description`、`og:locale` 仍是中文（`index.html:16,29-36`）；`html lang` 已更新，但社交/搜索摘要不更新。
- `hreflang` 只存在于静态 HTML；`sitemap.xml:8-10` 同样指向 `?lang=en`，但服务端对查询参数返回同一份 HTML，没有真正的英文静态页。

建议二选一：

1. **单页 + URL 同步**：语言切换用 `history.replaceState` 写入 `?lang=`，canonical 指向自身（或统一去掉 hreflang，仅保留一个可索引页）；同时更新 description/OG。
2. **静态多语言页**：`/en/` 独立 HTML、独立 canonical/description/OG，并做 `hreflang` 互指。

至少先修「切换语言不更新 URL」和「canonical 与 hreflang 冲突」。

### P1-5 保留键提示表不一致：16 个条目漏警告

识别器的警告集合 `RESERVED`（`app.js:1232-1237`）与展示列表 `RESERVED_LIST`（`app.js:1240-1276`）是两份手写数据。列表里有 16 项不在警告集合中：

`Ctrl+1`、`Ctrl+9`、`Ctrl+D`、`Ctrl+Shift+B`、`Ctrl+P`、`Ctrl+S`、`Win`、`Alt+Tab`、`Win+Tab`、`Alt+F4`、`Ctrl+Alt+Delete`、`Win+L`、`Win+D`、`Win+Shift+S`、`Print`、`Ctrl+Shift+Esc`。

实测 `Ctrl+S` 显示「匹配到 31 项快捷键」且**没有**「浏览器保留」警告；`Ctrl+1` 匹配 15 项也无警告（`Ctrl+T` 则有）。用户会以为这些键可以正常捕获。

建议：由 `RESERVED_LIST` 单一数据源派生 `RESERVED`（并补上 `Ctrl+P/S/D` 等），或在数据层给每条快捷键加 `reserved` 标记；加测试保证两表同步。

### P1-6 375px 移动端横向溢出 40px

在 375×812 移动仿真下：

- `documentElement.scrollWidth - clientWidth = 40px`；
- 顶栏 `.topbar-right` 右边缘到 391px（含 GitHub 按钮），加左右 24px 内边距后最小内容宽约 415px；
- `menu` 34px、品牌 44px、右侧 261px，在 375px 下放不下，产生横向滚动。

`styles.css:1127-1203` 只隐藏了 `detector-kbd`、把搜索框缩到 150px，没有压缩语言开关/图标间距或允许品牌文字省略。建议在 ≤400px 隐藏 GitHub 或把语言开关收进菜单，确保 320–430px 无横向滚动。

## 四、P2：稳定性、发布与细节

### P2-1 主题闪烁（FOUC）

`index.html` 没有在 `<head>` 内提前应用 `hk-theme`，主题在 `app.js:1117` 才执行；暗色系统 + 用户存了亮色（或反之）时会先闪错主题。对比：`sitemap.html` 由 `tools/extract-kb.mjs` 生成时已包含内联主题初始化。建议把同样的 3 行内联脚本放进 `index.html` 的 `<head>`（并考虑 CSP nonce/hash）。

### P2-2 线上版本与本地不一致，且站点源码未纳入仓库

- 线上 `app.js` 为 **1,324 行 / 49,693 B**，本地为 **1,482 行 / 55,739 B**；线上缺少 `favMigrate`、`looksLikeHotkey` 等本地已有逻辑。
- 线上 `data.js` 5,424 B，本地 6,640 B；线上首页也缺少收藏 JSON/MD 导出按钮。
- 线上 `styles.css` 与本地哈希不同。
- 本地 `docs/hotkeys/` 在 git 中是 `??`（未跟踪）；`git ls-tree origin/main docs` 只有 `docs/architecture.md`，仓库根也没有 `CONTRIBUTING.md`。
- 但 `index.html:142` 的「贡献指南」链接指向 `https://github.com/wensley/HotkeyInspector/blob/main/CONTRIBUTING.md`，在公开仓库中必然 404。
- `.github/workflows/ci.yml` 只构建/测试 .NET，不对站点做链接、数据 schema、重复键、Lighthouse 校验。

建议：把站点源码放入独立仓库或本仓库（若确实不公开，至少做私有备份与部署流水线），修正贡献链接（或指向 Issues），在 CI 中校验数据与生成物；确认线上发布源，避免「本地已修、线上仍旧」。

### P2-3 localStorage 未全程防护

`app.js:12-13` 在脚本初始化时直接 `localStorage.getItem`；`app.js:839`、`app.js:1113` 直接 `setItem`。在存储被禁用/配额满的环境会抛异常，导致整个 IIFE 中断、页面空白。`favData`、`popularSaved` 已有 try/catch，但入口和写入没有。建议统一封装 `storageGet/Set/Remove`，失败时降级为内存态。

### P2-4 收藏导出、URL 与搜索细节

- Markdown 导出未转义 `|` 和换行（`app.js:188-191`），动作文本含竖线会破坏表格。
- 搜索/滚动不更新 `history`：侧栏/TOC 点击 `preventDefault + scrollIntoView`（`app.js:973-980`），URL hash 不变，无法分享定位，浏览器后退不生效。建议 `history.pushState(null, "", "#"+id)`。
- 搜索词 `?q=` 只在加载时读取，输入时不写回；建议可用 `replaceState` 同步，便于分享结果。
- `copyText` 的 `execCommand` 分支无论成功失败都 `resolve()`（`app.js:651-670`），会误报「已复制」。
- `downloadBlob` 没有回收失败路径（可忽略）。
- 收藏不支持导入（只有 JSON/MD 导出），迁移旧数组会直接删除旧数据（`app.js:155-165`）；建议提示或提供导入。

### P2-5 内容构建与 SEO 细节

- 页面完全依赖 JS，且没有 `<noscript>` 兜底；结合 P0-1，禁用 JS/爬虫看到的是空 `<main>`。
- `index.html:38` 的 JSON-LD `dateModified` 为 **2026-10-16**，`sitemap.xml:5,14` 也是 10-16，但数据 `meta.updated` 是 **2026-10-15**；应由同一来源生成。
- 缺少 `og:image` / `twitter:image`，社交分享无图；`keywords` 已无 SEO 价值。
- `sitemap.html` 由工具生成时引用 `styles.css`（无 `?v=2`），而首页是 `styles.css?v=2`，缓存策略不一致；`sitemap.html` 的语言按钮也缺少 `aria-pressed`。
- `?lang=en` 的英文页 `og:locale` 仍是 `zh_CN`，见 P1-4。
- `docs/3056-shortcuts-one-site.md` 提到 OBS，但数据中没有 OBS（media 分类只有网易云/QQ音乐/Spotify/PotPlayer/VLC/B站），属于文案与数据漂移。

### P2-6 安全与隐私

- 未发现 XSS：数据/查询输出基本都经过 `escapeHtml`/`escapeAttr`（`app.js:85-93`），`innerHTML` 插值可追踪；这一点做得不错。
- 但 `renderPopular()` 把 localStorage 里的 id 直接拼进 `querySelector`（`app.js:587,615`），损坏的本地数据可抛 `SyntaxError` 使推荐区渲染中断；建议 `CSS.escape` 或从内存映射取节点。
- 编辑模式把 `<button>` 放进 `<a href>`（`app.js:595-608`），属于无效的交互嵌套，读屏/点击行为不稳定。
- 没有 CSP；内联 GA/JSON-LD 需要 hash/nonce 才能上 CSP。GA 与 Cloudflare Insights（线上自动注入）在无同意提示下加载，若面向欧盟用户需评估同意与隐私政策。
- 生产由 Cloudflare 提供 HTTPS；本地审计无法验证 HSTS/压缩/缓存头。线上 `app.js` 返回 `Cache-Control: max-age=14400`，更新后需考虑 `?v=` 失效或 purge。

## 五、数据与构建校验

`tools/extract-kb.mjs` 将站点数据抽取为 `src/HotkeyInspector.Core/Data/hotkeys.json`，并生成 `sitemap.html`；当前两边一致：

| 指标 | 站点 | hotkeys.json |
| --- | --- | --- |
| 分类 | 9 | 9 |
| 应用 | 60 | 60 |
| 分组 | 304 | 304 |
| 条目 | 3056 | 3056 |

但工具只做「读取 + 计数 + 拼接」，没有 schema 校验（缺 `id/name/groups`、重复 `app+key`、空 `zh/en`、序列键格式、非法 token 都不会报错），也不生成/校验 `index.html` 与 `sitemap.xml`。`CONTRIBUTING.md:11-27` 只描述了 `{k,zh,en}`，没有说明 `name:{zh,en}`、`groups`、`vendor`、`note`、`id` 唯一性等实际结构，贡献者按文档新增应用会失败。

建议在 `extract-kb.mjs` 中增加：

1. 必填字段与 id 唯一性校验；
2. 重复 `app+key` 报告（允许但要求显式 `context`）；
3. 序列/标点键的规范化与可识别性检查；
4. `hotkeys.json` 与站点计数、`sitemap.xml` 日期一致性检查；
5. CI 中运行上述检查（`node tools/extract-kb.mjs --check` 模式）。

## 六、逐文件问题索引

| 文件 | 行号 | 问题 |
| --- | --- | --- |
| `robots.txt` | 4-6 | 阻断 app.js 与全部 data 脚本（P0-1） |
| `index.html` | 123 | `<main>` 为空，正文依赖 JS |
| `index.html` | 187-198 | 11 个数据脚本 + app.js 串行阻塞，无 defer/bundle |
| `index.html` | 22-26 | canonical 与 `?lang=en` hreflang 冲突 |
| `index.html` | 16,29-36 | 语言切换不更新 description/OG |
| `index.html` | 38 | `dateModified` 2026-10-16 与数据 10-15 不一致 |
| `index.html` | 45,55,60,65,71,74,92,100,118,128,157 | 英文 aria-label / 缺少语言状态 |
| `index.html` | 69 | 搜索框无 label |
| `index.html` | 150-152 | to-top（实现基本正确） |
| `index.html` | 154-185 | 对话框结构；capture 无角色；见 P1-2 |
| `app.js` | 12-13 | localStorage 入口未防护 |
| `app.js` | 101,1224 | 收藏 id 仅 app+key，重复碰撞 |
| `app.js` | 124-129,416 | 每行一次 JSON.parse（性能根因） |
| `app.js` | 212-217 | `highlight` 已转义，安全 |
| `app.js` | 229-235 | 序列键显示错误 |
| `app.js` | 587,615 | localStorage id 拼 selector，可能抛错 |
| `app.js` | 595-608 | button 嵌套在 a 内 |
| `app.js` | 651-670 | 复制失败仍 resolve |
| `app.js` | 844-851 | 输入无 debounce，全量重绘 |
| `app.js` | 973-980 | 跳转不写 hash |
| `app.js` | 1121-1135 | 标点 token 与数据字面量不一致 |
| `app.js` | 1143-1156 | 缺少标点/Cmd 展示映射 |
| `app.js` | 1167-1177 | 空白拆分破坏 Page Up/Down 与序列 |
| `app.js` | 1232-1276 | 保留键两表不同步 |
| `app.js` | 1394-1401 | 拦截 Tab/Enter/Space |
| `app.js` | 1461-1475 | `?lang` 覆盖本地选择，URL 不更新 |
| `styles.css` | 1-90 | text-3 对比度不足 |
| `styles.css` | 291,401,1273 | `outline:none`（输入框有替代、对话框捕获区无） |
| `styles.css` | 1127-1203 | 375px 顶栏溢出 |
| `styles.css` | 1209-1243 | 对话框动画无 reduced-motion |
| `styles.css` | 1301 | 警告色暗色适配仅用 media，未跟随 data-theme |
| `tools/extract-kb.mjs` | 11-22,50+ | 无 schema/重复/可识别性校验；sitemap.html 无 CSS 版本 |
| `CONTRIBUTING.md` | 11-27 | 数据结构说明不完整；文件未入库 |
| `CHANGELOG.md` | 9-14 | 宣称的可访问性改进与实测不符（可更新） |

## 七、修复顺序建议

1. **立即**：放开 robots 中的 JS/CSS；修复识别器标点/序列映射与保留键表；确认线上发布源。
2. **本迭代**：收藏 Set 缓存 + 输入 debounce + 结果复用（先把 2.9s 降到可接受）；侧栏/TOC 改 `href`；识别器键盘交互与中文 aria。
3. **下迭代**：重复键数据模型（context）与收藏 id；对比度变量；语言 URL/canonical/OG；移动端 360–430px 适配。
4. **持续**：站点入库/备份 + CI 数据校验 + Lighthouse/axe 基线；预渲染或静态多语言方案；引入 CSP 与隐私说明。

验收基线建议：

- 375/360px 无横向滚动；
- 键盘可完成导航、搜索、识别器开关与按钮操作；
- Lighthouse a11y ≥ 95 且 color-contrast 0 失败；
- 识别器对全部单步组合（含标点）可命中，序列有明确提示；
- 搜索输入 p95 < 100 ms（桌面）/ < 300 ms（4× CPU），首屏 DOM < 10k；
- Googlebot 渲染后的 HTML 包含 3056 条正文。

## 八、审计限制

- Lighthouse 仅跑了 desktop navigation，未跑移动端性能分；本地服务器无 Cloudflare 压缩/Beacon，网络数据与线上有差异。
- 未使用真实屏幕阅读器（NVDA/JAWS/VoiceOver）与真机测试；键盘结论来自 Chrome DevTools 实测。
- 审计对象是本地 `docs/hotkeys/`；线上版本更旧（见 P2-2），线上结论仅基于实测的 robots、空 `<main>` 与脚本差异。
- 未审计 Cloudflare 后台配置、DNS、证书、WAF、缓存规则与 GA 账号设置。

## 九、整改记录（2026-10-20）

| 项目 | 状态 | 处理与验证 |
| --- | --- | --- |
| P0-1 robots.txt | 已修并部署 | 删除 `/app.js`、`/data*.js`、`/data/` 的 Disallow，保留 Allow 与 Sitemap。线上 `https://zuhejian.com/robots.txt` 已返回 `Allow: /`，待 Search Console 重新抓取验证渲染 HTML。 |
| P0-2 识别器 | 已修 | 增加键名标准化、可生成 token 集合、序列/组合判定与两步捕获；分类为 single 2854 + sequence 20 + special 182 = 3056，特殊操作单独归类，不再算“未匹配”。实测 `Ctrl+[` 11 条、`PageDown` 9 条、`Ctrl+Shift+.` 5 条、`Ctrl+K→Ctrl+B` 1 条。 |
| P1-1 性能 | 部分修复 | 收藏改为 Set 缓存，localStorage 初始读取由 3061 次降至 5 次，搜索期间增量为 0；输入加 90 ms debounce。新增渐进渲染：默认 600 条，每次“显示更多”增加 600 条约 153 ms；深链接/侧栏跳转会按需展开全部。未做真正窗口化。 |
| P1-2 可访问性 | 已修 | 侧栏/TOC 补 `href` 并同步 hash；动态中文 aria-label；识别器支持 Tab 循环、Enter/Space、Esc 关闭与焦点恢复；对比度变量调整。Lighthouse desktop snapshot：Accessibility/Best Practices/SEO/Agentic 均 100，0 失败。 |
| P1-3 重复组合键 | 已修 | 为重复 `app+key` 生成 `base`、`base#2`…稳定唯一 id；收藏按实例 id 读写。实测 `windows\|Backspace` 三个 id 唯一，点击/收藏互不影响。 |
| P1-4 双语 SEO | 已修 | 语言切换同步 canonical、og:url、og:locale、og:title/description、twitter、`html lang` 与 URL `lang` 参数；保留 `q` 参数。 |
| P1-5 保留键 | 已修 | `RESERVED_LIST` 单一数据源，补 `Ctrl+K/E/B/O/U/G`、`Ctrl+Shift+J/C`，区分浏览器与系统级别；锁定后按级别提示。 |
| P1-6 移动端 | 已修 | 520/430/360 断点压缩顶栏、键位换行、收藏工具常显；修复 `.kb-list` 300px 最小列导致的溢出。375px 与 320px 实测 `scrollWidth == clientWidth`，溢出 0。 |
| P2-1 主题闪烁 | 已修 | `index.html` 样式表前内联脚本读取 `hk-theme` 并预设 `data-theme`；实测刷新后首屏即为深色，无闪白。 |
| P2-2 发布/入库 | 已修复 | 私有备份仓库 `wensley/HotkeyInspector-site` 已推送完整快照；站点已部署为 Cloudflare Worker `hotkeyinspector-site`，`zuhejian.com/*` 与 `www.zuhejian.com/*` 路由已生效，主工作区源码仍保持未跟踪。 |
| P2-3 localStorage | 部分修复 | 新增 `lsGet`/`lsSet`，语言、主题与初始化读取已防护；收藏路径原本已有 try/catch。其余写入点仍建议统一封装。 |
| P2-4 导出/交互细节 | 部分修复 | Markdown 导出转义 `\|` 与换行；`execCommand('copy')` 失败改为 reject；热门应用由 `button` 嵌套 `a` 改为 `span` 容器，chip 整块可点击，跳转前清除搜索/收藏过滤；未渲染目标按数据计算节点 id；远距离跳转改为即时定位，避免长距离平滑滚动像“无响应”；应用节点查询加 `CSS.escape`；序列收藏 `keysNorm` 修正。 |
| P2-5 内容/SEO 细节 | 已修（摘要级） | JSON-LD 与 sitemap 日期统一；页脚链接站内化；移除无数据的 OBS；`sitemap.html` CSS 加版本；`<main>` 增加 `<noscript>`；新增 1200×630 `og-cover.png` 与 OG/Twitter 大图；`tools/prerender.mjs` 预渲染 9 分类/60 应用/304 分组摘要并接入 CI。未预渲染全部 3056 行。 |
| P2-6 安全/隐私 | 已部署，待关 CF Insights | 主站与 sitemap 加 CSP（主站无 `unsafe-inline`，JSON-LD 用 hash）；`theme-init.js` 外置；GA 改为同意后注入；sitemap 删除 GA；新增 `privacy.html`；线上 CSP 已验证。Cloudflare 自动注入的 Insights beacon 被 CSP 阻止，需在后台关闭 Web Analytics 才能消除控制台告警。 |
| P2-7 文档漂移 | 部分修复 | 修正 `docs/3056-shortcuts-one-site.md` 的 OBS 描述；CHANGELOG、CONTRIBUTING 与实际功能仍需后续同步。 |
| P2-8 sitemap 细节 | 已修 | `sitemap.html` 样式表增加 `?v=4`，与主站缓存策略一致；右侧 TOC 已移除，桌面布局改为侧栏 + 内容两列。 |

**回归验证**：`node --check app.js`、`node --check data.js`、`node tools/prerender.mjs --check` 通过；桌面 Lighthouse 四项 100（本地与线上，含同意横幅）；检测器键盘 Tab/Enter/Esc 与焦点恢复通过；语言切换保留 `q`；320/375px 无横向溢出；默认首屏 600 行、加载更多 1200 行、侧栏深链接自动展开；分析同意拒绝不加载 GA、允许后 GA 请求成功；线上 `robots.txt`、`og-cover.png`、`privacy.html`、`sitemap.xml` 均返回 200，首页 600 行；线上仅 Cloudflare Insights beacon 被 CSP 阻止，待后台关闭。`og-cover.png` 为 1200×630。

