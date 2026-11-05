export const UPDATED = "2026-10-28";

export const CATEGORIES = [
  { id: "color", name: "颜色", desc: "颜色格式转换、取色与对比度检查。" },
  { id: "encode", name: "编码与转换", desc: "文本、URL、HTML、进制与二进制之间的转换。" },
  { id: "format", name: "格式化与校验", desc: "JSON、XML、SQL、YAML、Markdown 等结构化数据。" },
  { id: "text", name: "文本", desc: "命名规范、字数、差异、去重、排序与隐形字符。" },
  { id: "css", name: "CSS", desc: "单位、渐变、阴影和布局辅助工具。" },
  { id: "security", name: "安全", desc: "哈希、HMAC、AES、JWT、密码与 bcrypt。" },
  { id: "debug", name: "调试", desc: "时间戳、正则、Cron、UA、Cookie、子网与 WebRTC。" },
  { id: "generate", name: "生成器", desc: "二维码、favicon、gitignore、htaccess 与 meta 标签。" },
  { id: "ai", name: "AI / LLM", desc: "Token 估算、成本、上下文窗口与函数调用 Schema。" },
  { id: "number", name: "数字", desc: "二进制、十进制、八进制、十六进制与按位运算。" },
  { id: "image", name: "图像", desc: "图片格式转换、压缩、Base64 与 favicon。" },
  { id: "video", name: "视频", desc: "视频重封装、转码、压缩与 GIF 互转。" },
  { id: "network", name: "网络", desc: "URL、HAR、curl、子网与 WebRTC 检测。" },
  { id: "reference", name: "参考", desc: "ASCII、HTTP 状态码和 chmod 权限速查。" }
];

const T = (id, name, glyph, cat, desc, tags, keywords, ui, libs) => ({
  id, name, glyph, cat, desc,
  tags: tags.split(" "),
  keywords,
  ui,
  libs: libs || [],
  commands: []
});

const text = (label, ph, outLabel, opts, acts, views, viewsAt) => ({
  k: "text", in: label, ph, out: outLabel || "结果", opts: opts || [], acts: acts || [["run", null]],
  views: views || ["text"], viewsAt: viewsAt || "out"
});

export const TOOLS = [
  /* ---------- 颜色 ---------- */
  T("rgb-to-hex", "RGB 转 HEX", "#", "color", "把 rgb() 或逗号分隔的 RGB 值转成十六进制颜色。", "RGB HEX", "rgb hex 颜色 转换 十六进制",
    text("RGB 值", "rgb(0, 114, 245) 或 0, 114, 245", "HEX")),
  T("rgba-to-hex", "RGBA 转 HEX", "#8", "color", "把 rgba() 转成带 alpha 的 8 位 HEX（#RRGGBBAA）。", "RGBA HEX Alpha", "rgba hex 透明度 alpha 8位 颜色",
    text("RGBA 值", "rgba(0, 114, 245, 0.8)", "HEX")),
  T("hex-to-rgb", "HEX 转 RGB", "rgb", "color", "把 3 位或 6 位十六进制颜色转成 rgb()。", "HEX RGB", "hex rgb 颜色 转换",
    text("HEX 值", "#0072f5", "RGB")),
  T("hex-to-rgba", "HEX 转 RGBA", "a", "color", "把 HEX 转成 rgba()，可指定不透明度。", "HEX RGBA Alpha", "hex rgba 透明度 alpha 颜色",
    text("HEX 值", "#0072f5", "RGBA", [{ k: "number", key: "alpha", label: "Alpha（0-1）", min: 0, max: 1, step: 0.05, value: 1 }])),
  T("hex-to-oklch", "HEX 转 OKLCH", "ok", "color", "把 HEX 转成 OKLCH，适合现代 CSS 色彩和感知均匀的调色。", "OKLCH HEX", "oklch 颜色 转换 css 感知",
    text("HEX 值", "#0072f5", "OKLCH")),
  T("color-picker", "颜色选择器", "◉", "color", "取色并同时给出 HEX、RGB、HSL、OKLCH 和对比度。", "取色 HEX RGB HSL", "颜色 选择器 picker hex rgb hsl oklch", { k: "custom", panel: "colorPicker" }),
  T("color-contrast-checker", "颜色对比度检查器", "AA", "color", "按 WCAG 2.x 计算两种颜色的对比度，判断是否达到 AA / AAA。", "WCAG 对比度", "对比度 颜色 wcag aa aaa 可访问性", { k: "custom", panel: "contrast" }),

  /* ---------- 编码 ---------- */
  T("base64", "Base64 编解码", "b64", "encode", "按 UTF-8 编码或解码文本，支持中文和 emoji。", "Base64 UTF-8", "base64 编码 解码 utf8 中文",
    text("输入文本", "快捷键大全 zuhejian.com", "结果", [], [["encode", "编码"], ["decode", "解码"]])),
  T("url-encoder", "URL 编解码", "%20", "encode", "编码或解码 URL 组件与整段 URL，支持表单风格的 + 空格。", "URL 编码", "url 编码 解码 encodeuricomponent 转义 form 表单",
    text("输入 URL 或参数", "https://zuhejian.com/搜索?q=命令 大全", "结果", [
      { k: "select", key: "mode", label: "范围", options: ["组件（encodeURIComponent）", "整段 URL（encodeURI）"], value: "组件（encodeURIComponent）" },
      { k: "checkbox", key: "plus", label: "+ 作为空格" }
    ], [["encode", "编码"], ["decode", "解码"]])),
  T("html-entities", "HTML 实体编解码", "&lt;", "encode", "转义 < > & 等字符（可选命名 / 十进制 / 十六进制实体），或把实体还原为文本。", "HTML 转义", "html 实体 转义 编码 解码 xss 命名实体",
    text("输入文本或实体", '<a href="https://zuhejian.com/tools/">开发者工具</a> & 快捷键', "结果", [
      { k: "select", key: "fmt", label: "输出格式", options: ["命名", "十进制", "Hex"], value: "命名" },
      { k: "checkbox", key: "all", label: "所有非 ASCII 字符" }
    ], [["encode", "编码"], ["decode", "解码"]])),
  T("json-escape", "JSON 转义 / 反转义", "\\n", "encode", "把文本转成可放进 JSON 字符串的转义形式（可带外层引号），或反向还原。", "JSON 转义", "json 转义 escape 字符串 换行 引号",
    text("输入文本", '快捷键 "大全" 与 zuhejian.com', "结果", [
      { k: "checkbox", key: "quote", label: "用引号包裹" }
    ], [["encode", "转义"], ["decode", "反转义"]])),
  T("image-to-base64", "图片转 Base64", "img", "encode", "把图片转成 data URI，或从 data URI 还原下载。", "Base64 图片", "图片 base64 data uri 转换", { k: "custom", panel: "imageBase64" }),
  T("binary-translator", "二进制转换器", "01", "encode", "在文本与二进制之间互转，并附带进制换算。", "二进制 文本", "二进制 转换 文本 ascii utf8", { k: "custom", panel: "binaryTranslator" }),
  T("binary-to-decimal", "二进制转十进制", "2→10", "number", "把二进制数转换为十进制，支持按位宽做二进制补码解读。", "二进制 十进制", "二进制 十进制 转换 补码 位宽",
    text("二进制数", "1100100", "十进制", [
      { k: "select", key: "sign", label: "解释方式", options: ["无符号", "有符号（二进制补码）"], value: "无符号" },
      { k: "select", key: "width", label: "位宽", options: ["8", "16", "32", "64"], value: "8" }
    ], [["run", "转换"]])),
  T("decimal-to-binary", "十进制转二进制", "10→2", "number", "把十进制数转换为二进制，可指定位宽并按半字节分组。", "十进制 二进制", "十进制 二进制 转换 位宽 补码",
    text("十进制数", "100", "二进制", [
      { k: "select", key: "width", label: "位宽", options: ["自动", "8", "16", "32"], value: "自动" },
      { k: "checkbox", key: "nibble", label: "按半字节分组", checked: true }
    ])),
  T("hex-to-decimal", "十六进制转十进制", "16→10", "number", "把十六进制转换为十进制。", "十六进制 十进制", "十六进制 十进制 转换",
    text("十六进制数", "ff", "十进制")),
  T("decimal-to-hex", "十进制转十六进制", "10→16", "number", "把十进制转换为十六进制，可加 0x 前缀、大写、补齐整字节。", "十进制 十六进制", "十进制 十六进制 转换 前缀 大写",
    text("十进制数", "255", "十六进制", [
      { k: "checkbox", key: "prefix", label: "0x 前缀" },
      { k: "checkbox", key: "upper", label: "大写", checked: true },
      { k: "checkbox", key: "pad", label: "补齐为整字节", checked: true }
    ])),
  T("binary-to-text", "二进制转文本", "01→A", "number", "把二进制按 UTF-8 或 ASCII 解码为文本。", "二进制 文本", "二进制 文本 ascii utf8 解码",
    text("二进制", "01001000 01101001", "文本", [
      { k: "select", key: "mode", label: "编码", options: ["UTF-8", "ASCII"], value: "UTF-8" }
    ])),
  T("text-to-binary", "文本转二进制", "A→01", "number", "把文本按 ASCII 或 UTF-8 转为二进制。", "文本 二进制", "文本 二进制 ascii utf8 编码",
    text("文本", "Hi", "二进制", [
      { k: "select", key: "mode", label: "编码", options: ["UTF-8", "ASCII"], value: "UTF-8" },
      { k: "checkbox", key: "space", label: "每字节加空格", checked: true }
    ])),
  T("hex-to-binary", "十六进制转二进制", "16→2", "number", "把十六进制逐位展开为二进制半字节，可加分组空格。", "十六进制 二进制", "十六进制 二进制 转换 分组",
    text("十六进制", "f0a", "二进制", [
      { k: "checkbox", key: "nibble", label: "按半字节分组", checked: true }
    ])),
  T("octal-to-decimal", "八进制转十进制", "8→10", "number", "把八进制数转换为十进制。", "八进制 十进制", "八进制 十进制 转换",
    text("八进制数", "755", "十进制")),
  T("binary-calculator", "二进制计算器", "+", "number", "对两个数做加减和按位与、或、异或、移位。", "二进制 按位", "二进制 计算 按位 and or xor 移位", { k: "custom", panel: "binaryCalc" }),

  /* ---------- 格式化 ---------- */
  T("json-formatter", "JSON 格式化", "{}", "format", "格式化、压缩和校验 JSON，报错给出行列位置。", "JSON 格式化", "json 格式化 美化 压缩 校验",
    text("输入 JSON", '{"site":"zuhejian.com","tools":105}', "结果", [], [["format", "格式化"], ["minify", "压缩"], ["repair", "修复"], ["validate", "仅校验"]], ["text", "tree", "table"], "in"), ["jsonrepair"]),
  T("json-validator", "JSON 校验器", "✓", "format", "校验 JSON 语法，并给出错误位置和修复提示。", "JSON 校验", "json 校验 语法 错误 位置",
    text("输入 JSON", '{"site":"zuhejian.com"}', "校验结果", [], [["validate", "校验"], ["repair", "修复"]], ["text", "tree", "table"], "in"), ["jsonrepair"]),
  T("json-to-csv", "JSON 转 CSV", "csv", "format", "把对象数组转成 CSV，自动收集列名并处理引号。", "JSON CSV", "json csv 转换 表格",
    text("JSON 数组", '[{"name":"JSON 格式化","count":1},{"name":"Base64 编解码","count":2}]', "CSV", [], [["run", null]], ["text", "tree", "table"], "in")),
  T("csv-to-json", "CSV 转 JSON", "[]", "format", "把 CSV 转成对象数组，支持双引号包裹和转义。", "CSV JSON", "csv json 转换 表格",
    text("CSV", "name,count\n快捷键,3056\n命令,778", "JSON", [], [["run", null]], ["text", "tree", "table"], "out")),
  T("json-to-typescript", "JSON 转 TypeScript", "TS", "format", "从 JSON 推断 TypeScript 类型，可选 interface 或 type 写法。", "TypeScript 类型", "json typescript 类型 接口 interface type",
    text("JSON", '{"site":"zuhejian.com","tools":105,"categories":["格式化","编码","颜色"]}', "TypeScript", [
      { k: "input", key: "root", label: "根类型名", value: "Root" },
      { k: "select", key: "shape", label: "输出", options: ["interface", "type"], value: "interface" }
    ], [["run", null]], ["text", "tree", "table"], "in")),
  T("json-to-go", "JSON 转 Go", "Go", "format", "从 JSON 生成 Go 结构体，带 json tag，可选 omitempty 与内联嵌套。", "Go 结构体", "json go golang 结构体 struct omitempty",
    text("JSON", '{"site":"zuhejian.com","tools":105,"categories":["格式化","编码","颜色"]}', "Go", [
      { k: "input", key: "root", label: "根结构体名", value: "Root" },
      { k: "checkbox", key: "omitempty", label: "omitempty 标签" },
      { k: "checkbox", key: "inline", label: "内联嵌套结构体" }
    ], [["run", null]], ["text", "tree", "table"], "in")),
  T("json-schema-builder", "JSON Schema 构建器", "schema", "ai", "搭字段、生成 JSON Schema，并导出 OpenAI、Anthropic、Gemini、Zod、Pydantic。", "JSON Schema", "json schema 推断 结构化 输出 函数调用 openai anthropic gemini zod pydantic",
    { k: "custom", panel: "schema" }),
  T("jsonl-validator", "JSONL 校验器", "jsonl", "format", "逐行校验 JSONL 微调数据，统计重复与 token，并拆分训练/验证集。", "JSONL 微调", "jsonl ndjson 校验 数据集 微调 openai anthropic 训练 验证 拆分 token",
    { k: "custom", panel: "jsonl" }),
  T("xml-formatter", "XML 格式化", "xml", "format", "用浏览器 DOMParser 校验并缩进 XML。", "XML 格式化", "xml 格式化 美化 校验",
    text("XML", "<site><name>快捷键大全</name><tools>105</tools></site>", "结果", [], [["format", "格式化"], ["minify", "压缩"]])),
  T("xml-to-json", "XML ⇄ JSON 转换", "xml↔", "format", "把 XML 转成 JSON 结构（可推断值类型），或把 JSON 还原为简单 XML。", "XML JSON", "xml json 转换 根元素 类型推断",
    text("XML 或 JSON", "<site><tools>105</tools></site>", "结果", [
      { k: "checkbox", key: "infer", label: "自动推断值类型", checked: true },
      { k: "input", key: "rootEl", label: "根元素名", value: "root" }
    ], [["toJson", "XML → JSON"], ["toXml", "JSON → XML"]])),
  T("json-yaml", "JSON ⇄ YAML", "yaml", "format", "在 JSON 与 YAML 之间互转。", "YAML JSON", "json yaml 转换", text("JSON 或 YAML", "site: zuhejian.com", "结果", [], [["toYaml", "JSON → YAML"], ["toJson", "YAML → JSON"]], ["text", "tree", "table"], "in"), ["yaml"]),
  T("yaml-validator", "YAML 校验器", "yaml✓", "format", "校验 YAML 语法并输出 JSON 预览。", "YAML 校验", "yaml 校验 语法 错误",
    text("YAML", "site: zuhejian.com\ntools:\n  - JSON 格式化\n  - Base64 编解码", "结果", [], [["validate", "校验"]]), ["yaml"]),
  T("yaml-to-go", "YAML 转 Go", "yaml→", "format", "从 YAML 生成 Go 结构体。", "YAML Go", "yaml go 结构体 struct",
    text("YAML", "site: zuhejian.com\ntools: 105", "Go", [{ k: "input", key: "root", label: "根类型名", value: "Root" }]), ["yaml"]),
  T("toml-to-json", "TOML 转 JSON", "toml", "format", "在 TOML 与 JSON 之间互转。", "TOML JSON", "toml json 转换", text("TOML 或 JSON", 'name = "快捷键大全"\nurl = "https://zuhejian.com/"', "结果", [], [["toJson", "TOML → JSON"], ["toToml", "JSON → TOML"]]), ["toml"]),
  T("sql-formatter", "SQL 格式化", "SQL", "format", "格式化常见方言 SQL，支持关键字大小写和缩进。", "SQL 格式化", "sql 格式化 美化 缩进", text("SQL", "select name, shortcut from shortcuts where app = 'Excel' limit 20", "结果", [
    { k: "select", key: "language", label: "方言", options: ["sql", "mysql", "postgresql", "sqlite", "tsql"], value: "sql" },
    { k: "select", key: "upper", label: "关键字", options: ["大写", "小写", "保持原样"], value: "大写" }
  ]), ["sql"]),
  T("markdown-html", "Markdown ⇄ HTML", "md", "format", "Markdown 转 HTML 并安全预览，或把 HTML 转回 Markdown。", "Markdown HTML", "markdown md html 转换 预览", { k: "custom", panel: "markdown", libs: ["marked", "purify"] }, ["marked", "purify"]),
  T("html-formatter", "HTML 格式化", "html", "format", "保守缩进 HTML，保留 pre / textarea / script 内容。", "HTML 格式化", "html 格式化 美化 缩进", text("HTML", "<article><h2>快捷键大全</h2><p>3056 条快捷键</p></article>", "结果", [], [["format", "格式化"], ["minify", "压缩"]]), ["beautify"]),
  T("css-beautifier", "CSS 美化", "css", "format", "格式化 CSS，支持缩进宽度。", "CSS 格式化", "css 美化 格式化 缩进", text("CSS", ".tool-card{display:flex;gap:12px;padding:18px}", "结果", [{ k: "number", key: "indent", label: "缩进空格", min: 1, max: 8, value: 2 }]), ["beautify"]),
  T("javascript-beautifier", "JavaScript 美化", "js", "format", "用 js-beautify 格式化 JavaScript。", "JS 格式化", "javascript js 美化 格式化 缩进", text("JavaScript", "function searchTools(q){return tools.filter(t=>t.name.includes(q))}", "结果", [{ k: "number", key: "indent", label: "缩进空格", min: 1, max: 8, value: 2 }]), ["beautify"]),
  T("har-file-viewer", "HAR 文件查看器", "HAR", "network", "读取 HTTP Archive，列出请求、状态码、耗时和体积。", "HAR 网络", "har 文件 查看 http archive 请求 分析", { k: "custom", panel: "har" }),
  T("jsonpath-tester", "JSONPath 测试器", "$..", "format", "用 JSONPath 表达式查询 JSON，列出命中值。", "JSONPath 查询", "jsonpath 查询 json 表达式", text("JSON", '{"site":{"tools":[{"id":"json-formatter"},{"id":"base64"}]}}', "命中结果", [
    { k: "input", key: "expr", label: "JSONPath", value: "$..id" }
  ], [["run", null]], ["text", "tree", "table"], "both"), ["jsonpath"]),
  T("html-minifier", "HTML 压缩", "html↓", "format", "压缩 HTML 空白和注释，也可反向重新格式化。", "HTML 压缩", "html 压缩 minify 体积 格式化",
    text("HTML", "<article>  <h2> 快捷键大全 </h2> </article>", "结果", [], [["run", "压缩"], ["format", "重新格式化"]]), ["beautify"]),
  T("css-minifier", "CSS 压缩", "css↓", "format", "压缩 CSS 注释、空白和多余分号，也可反向美化。", "CSS 压缩", "css 压缩 minify 美化",
    text("CSS", ".tool-card { display: flex; gap: 12px; }", "结果", [], [["run", "压缩"], ["beautify", "美化"]]), ["beautify"]),
  T("javascript-minifier", "JavaScript 压缩", "js↓", "format", "用 Terser 压缩 JavaScript（会改变变量名），也可反向美化。", "JS 压缩", "javascript js 压缩 minify terser 美化",
    text("JavaScript", "function searchTools(q) { return tools.filter((t) => t.name.includes(q)); }", "结果", [], [["run", "压缩"], ["beautify", "美化"]]), ["terser", "beautify"]),
  T("iam-policy-explainer", "AWS IAM 策略解读器", "iam", "security", "把 IAM Policy JSON 展开为“谁在什么资源上能做什么”。", "IAM AWS", "iam policy aws 权限 解读 策略", text("IAM Policy JSON", '{"Statement":[{"Effect":"Allow","Action":"s3:GetObject","Resource":"*"}]}', "解读结果")),
  T("curl-converter", "curl 转代码", "curl", "network", "把 curl 命令转成 fetch、Python requests 或 Go。", "curl fetch", "curl 转换 fetch python go 代码", text("curl 命令", "curl -X POST https://api.example.com -H 'Content-Type: application/json' -d '{\"a\":1}'", "代码", [
    { k: "select", key: "target", label: "目标语言", options: ["fetch", "Python requests", "Go"], value: "fetch" }
  ])),
  T("compression-comparison", "压缩比较", "gz", "format", "比较 gzip / Brotli / Zstd 在不同级别下的压缩效果，全部本地计算。", "gzip 压缩", "压缩 gzip deflate brotli zstd 比较", { k: "custom", panel: "compression" }, ["fflate", "brotli", "zstd"]),

  /* ---------- 文本 ---------- */
  T("case-converter", "命名规范转换", "Aa", "text", "一次给出 camelCase、snake_case、kebab-case 等 11 种命名风格。", "命名 camelCase", "命名 转换 camel snake kebab pascal constant title sentence",
    { k: "custom", panel: "caseConverter" }),
  T("word-counter", "字数统计", "123", "text", "统计字符、单词、行数、段落和阅读时间。", "字数 统计", "字数 统计 字符 单词 行数 阅读时间", text("文本", "粘贴要统计的文本", "统计结果")),
  T("diff-checker", "差异比较", "±", "text", "逐行比较高亮新增、删除和修改。", "Diff 差异", "diff 差异 比较 文本 高亮", { k: "custom", panel: "diff" }),
  T("lorem-ipsum-generator", "Lorem Ipsum 生成器", "lorem", "text", "按段落、句子或单词生成占位文本，可输出纯文本或 HTML。", "占位文本", "lorem ipsum 占位 假文 生成 html", text("设置", "", "结果", [
    { k: "number", key: "count", label: "数量", min: 1, max: 100, value: 3 },
    { k: "select", key: "unit", label: "单位", options: ["段落", "句子", "单词"], value: "段落" },
    { k: "checkbox", key: "classic", label: "以 Lorem ipsum 开头", checked: true },
    { k: "checkbox", key: "html", label: "HTML 段落", checked: false }
  ], [["run", "生成"]])),
  T("remove-duplicate-lines", "删除重复行", "uniq", "text", "去除重复行，可保留首次或末次出现。", "去重 行", "重复行 去重 uniq 文本", text("文本", "每行一条", "结果", [
    { k: "select", key: "keep", label: "保留", options: ["首次出现", "末次出现"], value: "首次出现" },
    { k: "checkbox", key: "trim", label: "忽略首尾空格", checked: true },
    { k: "checkbox", key: "case", label: "忽略大小写", checked: false }
  ])),
  T("sort-text", "文本行排序", "az", "text", "按字母、数字、长度排序，可忽略大小写、去重和反转。", "排序 行", "文本 排序 行 字母 数字 长度 去重", text("文本", "每行一条", "结果", [
    { k: "select", key: "mode", label: "方式", options: ["字母升序", "字母降序", "数字升序", "数字降序", "长度升序", "长度降序"], value: "字母升序" },
    { k: "checkbox", key: "nocase", label: "忽略大小写", checked: true },
    { k: "checkbox", key: "unique", label: "去重", checked: false },
    { k: "checkbox", key: "reverse", label: "反转结果", checked: false }
  ])),
  T("invisible-character-cleaner", "隐形字符清理器", "👻", "text", "按类别检测零宽字符、方向控制符、标签字符和异常空白，高亮可疑字符并一键清理。", "隐形字符 Unicode", "隐形字符 零宽 unicode 清理 提示词注入 bidi", { k: "custom", panel: "invisibleCleaner" }),
  T("ascii-table", "ASCII 表", "A", "reference", "十进制、十六进制、八进制、二进制对照表。", "ASCII 表", "ascii 表 编码 十进制 十六进制", { k: "table", panel: "ascii" }),
  T("meta-tag-previewer", "Meta 标签预览器", "meta", "text", "解析 HTML 的 title、description 和 Open Graph，预览分享卡片。", "Meta OG", "meta 标签 预览 open graph seo 分享", { k: "custom", panel: "metaPreview" }, []),

  /* ---------- CSS ---------- */
  T("px-to-rem", "PX 转 REM", "rem", "css", "按基准字号把 px 换算为 rem，也支持反向换算。", "px rem", "px rem 换算 单位 响应式", text("像素值", "16", "结果", [
    { k: "number", key: "base", label: "基准字号（px）", min: 1, max: 100, value: 16 },
    { k: "select", key: "dir", label: "方向", options: ["px → rem", "rem → px"], value: "px → rem" }
  ])),
  T("css-gradient-generator", "CSS 渐变生成器", "◨", "css", "生成线性、径向和锥形渐变 CSS，并实时预览。", "CSS 渐变", "css 渐变 gradient linear radial conic", { k: "custom", panel: "gradient" }),
  T("box-shadow-generator", "box-shadow 生成器", "▢", "css", "调节偏移、模糊、扩散和颜色，复制 CSS。", "CSS 阴影", "box-shadow 阴影 css 生成", { k: "custom", panel: "shadow" }),
  T("chmod-calculator", "Chmod 计算器", "755", "reference", "在 rwx 与数字权限之间换算。", "chmod 权限", "chmod 权限 755 linux 计算", { k: "custom", panel: "chmod" }),

  /* ---------- 安全 ---------- */
  T("hash-generator", "哈希生成器", "ƒ", "security", "用 Web Crypto 计算 SHA-1/256/384/512 摘要。", "Hash SHA", "哈希 hash sha256 摘要 校验", text("输入文本", "abc", "十六进制摘要", [
    { k: "select", key: "algo", label: "算法", options: ["SHA-256", "SHA-1", "SHA-384", "SHA-512"], value: "SHA-256" },
    { k: "checkbox", key: "upper", label: "大写" }
  ])),
  T("hmac-generator", "HMAC 生成器", "hmac", "security", "用 Web Crypto 计算 HMAC 签名。", "HMAC 签名", "hmac 签名 sha256 密钥", text("消息", "message", "HMAC", [
    { k: "input", key: "key", label: "密钥", type: "password", ph: "secret" },
    { k: "select", key: "algo", label: "算法", options: ["SHA-256", "SHA-1", "SHA-384", "SHA-512"], value: "SHA-256" },
    { k: "select", key: "encoding", label: "输出", options: ["Hex", "Base64"], value: "Hex" }
  ])),
  T("aes-encryption", "AES 加密", "aes", "security", "用密码派生密钥做 AES-GCM / AES-CBC 加密或解密，输出 Base64。", "AES 加密", "aes 加密 解密 gcm cbc pbkdf2 密码", { k: "custom", panel: "aes" }),
  T("jwt-decoder", "JWT 解码器", "jwt", "security", "解码 header 和 payload，不校验签名。", "JWT 解码", "jwt token 解码 header payload", { k: "custom", panel: "jwt" }),
  T("jwt-generator", "JWT 生成器", "jwt+", "security", "用 HS256 签发 JSON Web Token，适合本地调试。", "JWT 签发", "jwt 生成 签发 hs256 token", text("Payload（JSON）", '{"sub":"123"}', "JWT", [
    { k: "input", key: "secret", label: "密钥", type: "password", ph: "secret" }
  ])),
  T("password-generator", "密码生成器", "•••", "security", "本地生成高强度随机密码。", "密码 随机", "密码 生成 随机 强度", text("生成选项", "", "结果", [
    { k: "number", key: "length", label: "长度", min: 8, max: 128, value: 20 },
    { k: "checkbox", key: "upper", label: "大写字母", checked: true },
    { k: "checkbox", key: "digits", label: "数字", checked: true },
    { k: "checkbox", key: "symbols", label: "符号", checked: true },
    { k: "checkbox", key: "plain", label: "排除易混淆字符（Il1O0）", checked: true },
    { k: "checkbox", key: "each", label: "每类至少一个", checked: true },
    { k: "number", key: "count", label: "生成数量", min: 1, max: 20, value: 3 }
  ], [["run", "生成"]])),
  T("token-generator", "令牌生成器", "tk", "security", "生成随机 token，可选 Base64URL、十六进制或 UUID。", "Token 随机", "token 令牌 随机 生成 base64 hex", text("生成选项", "", "结果", [
    { k: "number", key: "bytes", label: "字节数", min: 8, max: 128, value: 32 },
    { k: "select", key: "format", label: "格式", options: ["Base64URL", "HEX", "UUID"], value: "Base64URL" },
    { k: "number", key: "count", label: "数量", min: 1, max: 20, value: 1 }
  ], [["run", "生成"]])),
  T("uuid-generator", "UUID 生成器", "id", "generate", "批量生成 UUID v4 或 v7。", "UUID v4 v7", "uuid guid 生成 批量 v4 v7", text("生成选项", "", "结果", [
    { k: "number", key: "count", label: "数量", min: 1, max: 200, value: 5 },
    { k: "select", key: "version", label: "版本", options: ["v4", "v7"], value: "v4" },
    { k: "checkbox", key: "hyphen", label: "保留连字符", checked: true },
    { k: "checkbox", key: "upper", label: "转为大写", checked: false },
    { k: "checkbox", key: "braces", label: "带引号" }
  ], [["run", "生成"]])),
  T("bcrypt-generator", "Bcrypt 生成器", "bc", "security", "生成 bcrypt 哈希或校验密码与哈希是否匹配。", "Bcrypt 密码", "bcrypt 哈希 校验 密码", text("密码或待校验内容", "password", "结果", [
    { k: "number", key: "rounds", label: "Cost", min: 4, max: 14, value: 10 },
    { k: "input", key: "hash", label: "已有哈希（校验时使用）", ph: "$2b$10$..." }
  ], [["hash", "生成哈希"], ["verify", "校验哈希"]]), ["bcryptjs"]),

  /* ---------- 调试 ---------- */
  T("timestamp-converter", "时间戳转换器", "ts", "debug", "Unix 秒 / 毫秒与本地时间、UTC、ISO 互转。", "Unix 时间戳", "时间戳 unix timestamp 转换 日期", { k: "custom", panel: "timestamp" }),
  T("regex-tester", "RegEx 测试器", ".*", "debug", "实时显示匹配、分组和高亮。", "正则 测试", "正则 regex 测试 匹配 分组", { k: "custom", panel: "regex" }),
  T("cron-parser", "Cron 表达式解析器", "cron", "debug", "解析 5 / 6 段 Cron，给出中文说明、字段明细与未来运行时间，支持本地与 UTC、@daily 等别名和 JAN / MON 名称。", "Cron 定时", "cron 表达式 解析 定时 任务 时区 utc", { k: "custom", panel: "cron" }),
  T("user-agent-parser", "User-Agent 解析器", "UA", "debug", "识别浏览器、内核、操作系统和设备类型。", "UA 解析", "user agent ua 浏览器 操作系统 设备", text("User-Agent", "Mozilla/5.0 ...", "解析结果", [], [["run", "解析"]])),
  T("url-parser", "URL 解析器", "url", "debug", "拆开协议、主机、端口、路径、查询参数与片段；查询参数支持文本、树形、表格、JSON 四种视图，重复键保留为数组，缺少协议方案会直接报错而不自动补全。", "URL 解析", "url 解析 参数 query 拆分 视图 json 表格", { k: "custom", panel: "url" }),
  T("cookie-parser", "Cookie 解析器", "cookie", "debug", "解析 Cookie 字符串或 Set-Cookie 头。", "Cookie 解析", "cookie 解析 set-cookie 属性", text("Cookie 字符串", "a=1; b=2; Path=/", "解析结果")),
  T("http-status-codes", "HTTP 状态码", "404", "reference", "按类别和关键词筛选状态码，附常见原因。", "HTTP 状态码", "http 状态码 200 404 500 502", { k: "table", panel: "http" }),
  T("subnet-calculator", "子网计算器", "/24", "network", "计算 IPv4 子网，支持网络地址、广播、主机数和掩码。", "IPv4 子网", "子网 计算 ipv4 掩码 cidr", { k: "custom", panel: "subnet" }),
  T("cidr-calculator", "CIDR 计算器", "cidr", "network", "把 CIDR 或 IPv4 地址展开为网络地址、广播地址、子网掩码和主机数。", "CIDR", "cidr 计算 网段 范围 子网", text("CIDR 或 IPv4 地址", "10.0.0.0/8", "规范化 CIDR")),
  T("webrtc-ice-test", "WebRTC ICE 测试", "ice", "network", "收集 ICE 候选，检查 STUN 是否可达。", "WebRTC ICE", "webrtc ice stun turn 候选 检测", { k: "custom", panel: "ice" }),
  T("webrtc-leak-test", "WebRTC 泄露测试", "leak", "network", "检查 WebRTC 是否暴露本地或公网 IP。", "WebRTC 泄露", "webrtc 泄露 ip vpn 检测", { k: "custom", panel: "ice" }),

  /* ---------- 生成器 ---------- */
  T("qr-code-generator", "QR Code 生成器", "qr", "generate", "从文本或 URL 生成二维码，可下载 PNG。", "二维码 QR", "二维码 qr code 生成 下载", { k: "custom", panel: "qr" }, ["qrcode"]),
  T("favicon-generator", "favicon 生成器", "ico", "image", "从一张图片生成多尺寸 favicon 并打包下载。", "favicon ICO", "favicon 生成 ico 图标", { k: "custom", panel: "favicon" }),
  T("gitignore-generator", "gitignore 生成器", "git", "generate", "按语言和操作系统组合 .gitignore 模板。", "gitignore 模板", "gitignore 生成 模板 语言", text("选择模板", "", "结果", [
    { k: "select", key: "lang", label: "语言 / 框架", options: ["Node", "Python", "Go", "Java", "Rust", "PHP", "Unity"], value: "Node" },
    { k: "checkbox", key: "os", label: "包含操作系统文件", checked: true },
    { k: "checkbox", key: "ide", label: "包含编辑器文件", checked: true }
  ], [["run", "生成"]])),
  T("htaccess-generator", "htaccess 生成器", "ht", "generate", "生成 HTTPS 跳转、SPA 回退和缓存规则。", "htaccess Apache", "htaccess 生成 重写 跳转 缓存", text("域名", "zuhejian.com", "结果", [
    { k: "checkbox", key: "https", label: "强制 HTTPS", checked: true },
    { k: "checkbox", key: "spa", label: "SPA 回退到 index.html", checked: false },
    { k: "checkbox", key: "cache", label: "静态资源缓存", checked: true }
  ], [["run", "生成"]])),
  T("meta-tag-generator", "Meta 标签生成器", "meta+", "generate", "生成 SEO、Open Graph 和 Twitter 卡片标签。", "Meta 生成", "meta 标签 生成 seo open graph twitter", { k: "custom", panel: "metaGenerator" }),

  /* ---------- AI / LLM ---------- */
  T("llm-token-counter", "LLM Token 计数器", "tok", "ai", "按字符类型估算 GPT/Claude 风格的 token 数。", "Token 估算", "token 计数 llm gpt claude 估算", text("文本", "粘贴要估算的文本", "估算结果", [
    { k: "select", key: "model", label: "模型", options: ["GPT-4o / o 系列", "Claude", "Gemini", "Llama"], value: "GPT-4o / o 系列" }
  ])),
  T("llm-cost-calculator", "LLM 成本计算器", "¥", "ai", "按输入输出 token 和单价估算 API 成本。", "LLM 成本", "llm 成本 价格 token api 费用", text("用量", "", "成本估算", [
    { k: "number", key: "inputTokens", label: "输入 tokens", min: 0, max: 100000000, value: 100000 },
    { k: "number", key: "outputTokens", label: "输出 tokens", min: 0, max: 100000000, value: 20000 },
    { k: "select", key: "model", label: "模型", options: ["GPT-4o", "GPT-4o mini", "Claude Sonnet", "Claude Haiku", "Gemini Pro"], value: "GPT-4o" }
  ], [["run", "计算"]])),
  T("context-window-checker", "上下文窗口检查器", "ctx", "ai", "估算文本 token 数并检查是否超出模型上下文窗口。", "上下文窗口", "上下文 窗口 token 长度 检查", text("文本", "粘贴提示词", "结果", [
    { k: "select", key: "model", label: "模型", options: ["GPT-4o (128k)", "Claude Sonnet (200k)", "Gemini Pro (1M)", "Llama 3 (128k)"], value: "GPT-4o (128k)" }
  ])),

  /* ---------- 图像 ---------- */
  T("heic-to-jpg", "HEIC 转 JPG", "heic", "image", "在浏览器本地把 iPhone HEIC 照片转成 JPG。", "HEIC JPG", "heic jpg 转换 iphone 照片", { k: "custom", panel: "imageConvert", opts: [{ k: "select", key: "format", label: "输出格式", options: ["JPG", "PNG", "WebP"], value: "JPG" }] }, ["heic2any"]),
  T("image-compressor", "图片压缩器", "zip", "image", "用 Canvas 压缩 JPG、PNG、WebP，可调质量。", "图片 压缩", "图片 压缩 jpg png webp 质量", { k: "custom", panel: "imageConvert", opts: [{ k: "select", key: "format", label: "输出格式", options: ["JPG", "WebP", "PNG"], value: "JPG" }, { k: "number", key: "quality", label: "质量（0.1-1）", min: 0.1, max: 1, step: 0.05, value: 0.8 }] }),
  T("svg-to-png", "SVG 转 PNG", "svg", "image", "按倍率把 SVG 栅格化为 PNG。", "SVG PNG", "svg png 转换 栅格化", { k: "custom", panel: "imageConvert", opts: [{ k: "number", key: "scale", label: "倍率", min: 1, max: 8, value: 2 }] }),
  T("png-to-jpg", "PNG 转 JPG", "png", "image", "批量把 PNG 转为 JPG，可设置背景色。", "PNG JPG", "png jpg 转换 批量", { k: "custom", panel: "imageConvert", opts: [{ k: "color", key: "bg", label: "背景色", value: "#ffffff" }] }),
  T("jpg-to-png", "JPG 转 PNG", "jpg", "image", "批量把 JPG 转为 PNG。", "JPG PNG", "jpg png 转换 批量", { k: "custom", panel: "imageConvert", opts: [{ k: "select", key: "format", label: "输出格式", options: ["PNG"], value: "PNG" }] }),
  T("webp-to-png", "WebP 转 PNG", "webp", "image", "批量把 WebP 转为 PNG。", "WebP PNG", "webp png 转换 批量", { k: "custom", panel: "imageConvert", opts: [{ k: "select", key: "format", label: "输出格式", options: ["PNG"], value: "PNG" }] }),

  /* ---------- 视频 ---------- */
  T("mov-to-mp4", "MOV 转 MP4", "mov", "video", "把 QuickTime MOV 重封装为 MP4，编码兼容时无损。", "MOV MP4", "mov mp4 转换 视频", { k: "custom", panel: "videoConvert", opts: [{ k: "select", key: "format", label: "输出", options: ["mp4"], value: "mp4" }] }, ["mediabunny"]),
  T("mkv-to-mp4", "MKV 转 MP4", "mkv", "video", "把 MKV 转换为 MP4，兼容编码自动重封装。", "MKV MP4", "mkv mp4 转换 视频", { k: "custom", panel: "videoConvert", opts: [{ k: "select", key: "format", label: "输出", options: ["mp4"], value: "mp4" }] }, ["mediabunny"]),
  T("webm-to-mp4", "WebM 转 MP4", "webm", "video", "把 WebM（VP8/VP9）转为 H.264 MP4。", "WebM MP4", "webm mp4 转换 视频 vp9", { k: "custom", panel: "videoConvert", opts: [{ k: "select", key: "format", label: "输出", options: ["mp4"], value: "mp4" }] }, ["mediabunny"]),
  T("mp4-to-webm", "MP4 转 WebM", "mp4", "video", "把 MP4 转为 WebM（VP9/Opus），减小网页视频体积。", "MP4 WebM", "mp4 webm 转换 视频 vp9", { k: "custom", panel: "videoConvert", opts: [{ k: "select", key: "format", label: "输出", options: ["webm"], value: "webm" }] }, ["mediabunny"]),
  T("video-to-gif", "视频转 GIF", "gif", "video", "把视频片段转成 GIF，可调帧率和宽度。", "视频 GIF", "视频 gif 转换 动图 帧率", { k: "custom", panel: "videoGif", opts: [{ k: "number", key: "fps", label: "帧率", min: 1, max: 24, value: 10 }, { k: "number", key: "width", label: "宽度", min: 120, max: 960, value: 480 }] }, ["mediabunny", "gifenc"]),
  T("gif-to-mp4", "GIF 转 MP4", "gif→", "video", "把 GIF 转成无声 H.264 MP4。", "GIF MP4", "gif mp4 转换 动图 视频", { k: "custom", panel: "videoConvert", opts: [{ k: "select", key: "format", label: "输出", options: ["mp4"], value: "mp4" }] }, ["mediabunny", "gifuct"]),
  T("video-compressor", "视频压缩", "vzip", "video", "按码率和分辨率压缩视频，全程本地完成。", "视频 压缩", "视频 压缩 码率 分辨率", { k: "custom", panel: "videoConvert", opts: [{ k: "select", key: "quality", label: "质量", options: ["低", "中", "高"], value: "中" }, { k: "select", key: "format", label: "输出", options: ["mp4", "webm"], value: "mp4" }] }, ["mediabunny"]),
  T("video-trimmer", "视频剪切工具", "cut", "video", "截取指定时间段，优先无损重封装。", "视频 剪切", "视频 剪切 截取 时间段", { k: "custom", panel: "videoTrim", opts: [{ k: "number", key: "start", label: "开始（秒）", min: 0, max: 86400, value: 0 }, { k: "number", key: "end", label: "结束（秒）", min: 0, max: 86400, value: 10 }] }, ["mediabunny"]),

  /* ---------- 网络补充 ---------- */
  T("file-drop", "一次性文件分享", "drop", "network", "上传文件到 FileBox，返回提取码、分享链接和二维码。单文件最大 50 MB，最长 7 天。", "文件分享 FileBox", "文件 分享 一次性 上传 链接 filebox", { k: "custom", panel: "fileDrop" }, ["qrcode"])
];

/* 说明：file-drop 保留占位，明确说明不提供，不计入“可用工具”统计。 */

export const TOOL_BY_ID = Object.fromEntries(TOOLS.map((t) => [t.id, t]));

export const HTTP_STATUS = [
  { code: 100, name: "Continue", zh: "继续", desc: "请求头已收到，客户端可继续发送请求体。" },
  { code: 101, name: "Switching Protocols", zh: "切换协议", desc: "服务器同意切换到 Upgrade 指定的协议，如 WebSocket。" },
  { code: 103, name: "Early Hints", zh: "早期提示", desc: "在最终响应前提前返回资源提示，便于预加载。" },
  { code: 200, name: "OK", zh: "成功", desc: "请求成功，响应体包含结果。" },
  { code: 201, name: "Created", zh: "已创建", desc: "资源创建成功，通常在 Location 头返回新地址。" },
  { code: 202, name: "Accepted", zh: "已接受", desc: "请求已接受但尚未处理完成，适合异步任务。" },
  { code: 204, name: "No Content", zh: "无内容", desc: "成功但没有响应体，常用于 DELETE 和预检请求。" },
  { code: 206, name: "Partial Content", zh: "部分内容", desc: "范围请求成功，响应体只包含指定区间。" },
  { code: 301, name: "Moved Permanently", zh: "永久移动", desc: "资源永久迁移，搜索引擎会更新索引。" },
  { code: 302, name: "Found", zh: "临时移动", desc: "临时跳转，方法可能被改为 GET，不适合永久迁移。" },
  { code: 304, name: "Not Modified", zh: "未修改", desc: "缓存仍有效，服务器不返回响应体，节省带宽。" },
  { code: 307, name: "Temporary Redirect", zh: "临时重定向", desc: "临时跳转且保持原请求方法，适合 POST 场景。" },
  { code: 308, name: "Permanent Redirect", zh: "永久重定向", desc: "永久跳转且保持原请求方法。" },
  { code: 400, name: "Bad Request", zh: "请求错误", desc: "请求语法或参数有问题，服务器无法解析。" },
  { code: 401, name: "Unauthorized", zh: "未认证", desc: "缺少或无效的认证凭据，通常需要登录。" },
  { code: 403, name: "Forbidden", zh: "禁止访问", desc: "身份已知但无权限；与 401 的区别是认证不是问题。" },
  { code: 404, name: "Not Found", zh: "未找到", desc: "资源不存在，或服务器选择不暴露其存在。" },
  { code: 405, name: "Method Not Allowed", zh: "方法不允许", desc: "资源存在但不支持该 HTTP 方法。" },
  { code: 408, name: "Request Timeout", zh: "请求超时", desc: "服务器等待请求完成超时，客户端可重试。" },
  { code: 409, name: "Conflict", zh: "冲突", desc: "请求与资源当前状态冲突，如并发写入。" },
  { code: 410, name: "Gone", zh: "已删除", desc: "资源曾存在且已永久删除，比 404 更明确。" },
  { code: 413, name: "Content Too Large", zh: "内容过大", desc: "请求体超过服务器限制，常见于上传接口。" },
  { code: 415, name: "Unsupported Media Type", zh: "媒体类型不支持", desc: "Content-Type 不被接口接受。" },
  { code: 422, name: "Unprocessable Content", zh: "无法处理", desc: "语法正确但语义校验失败。" },
  { code: 429, name: "Too Many Requests", zh: "请求过多", desc: "触发限流，查看 Retry-After 头决定重试时间。" },
  { code: 500, name: "Internal Server Error", zh: "服务器内部错误", desc: "未捕获的异常，需看服务端日志。" },
  { code: 501, name: "Not Implemented", zh: "未实现", desc: "服务器不支持该功能或方法。" },
  { code: 502, name: "Bad Gateway", zh: "网关错误", desc: "反向代理收到上游的无效响应。" },
  { code: 503, name: "Service Unavailable", zh: "服务不可用", desc: "服务过载或维护中，可配合 Retry-After 重试。" },
  { code: 504, name: "Gateway Timeout", zh: "网关超时", desc: "代理等待上游超时，常见于慢查询和长任务。" },
  { code: 505, name: "HTTP Version Not Supported", zh: "HTTP 版本不支持", desc: "服务器不支持请求使用的 HTTP 版本。" },
  { code: 507, name: "Insufficient Storage", zh: "存储不足", desc: "服务器无法存储请求所需内容，检查磁盘和配额。" }
];
