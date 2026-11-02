const VERSION_PATH = "/version";

/* 旧工具 ID → 新 ID（与 docs/hotkeys/_redirects 保持一致） */
const REDIRECTS = {
  "/tools/json": "/tools/json-formatter",
  "/tools/url": "/tools/url-encoder",
  "/tools/html": "/tools/html-entities",
  "/tools/color": "/tools/color-picker",
  "/tools/timestamp": "/tools/timestamp-converter",
  "/tools/uuid": "/tools/uuid-generator",
  "/tools/hash": "/tools/hash-generator",
  "/tools/jwt": "/tools/jwt-decoder",
  "/tools/regex": "/tools/regex-tester",
  "/tools/cidr": "/tools/cidr-calculator",
  "/tools/httpstatus": "/tools/http-status-codes"
};

function redirectTarget(pathname) {
  const clean = pathname.length > 1 ? pathname.replace(/\/+$/, "") : pathname;
  return REDIRECTS[clean] || null;
}

function versionResponse(env) {
  const meta = env.CF_VERSION_METADATA || {};
  const body = {
    id: typeof meta.id === "string" ? meta.id : "",
    tag: typeof meta.tag === "string" ? meta.tag : "",
    timestamp: typeof meta.timestamp === "string" ? meta.timestamp : ""
  };
  return new Response(JSON.stringify(body), {
    headers: {
      "content-type": "application/json; charset=utf-8",
      "cache-control": "no-store"
    }
  });
}

export default {
  async fetch(request, env) {
    const url = new URL(request.url);
    if (url.pathname === VERSION_PATH) {
      if (request.method !== "GET" && request.method !== "HEAD") {
        return new Response("Method Not Allowed", {
          status: 405,
          headers: { allow: "GET, HEAD", "cache-control": "no-store" }
        });
      }
      return versionResponse(env);
    }
    const target = redirectTarget(url.pathname);
    if (target) {
      const location = target + (url.search || "");
      return new Response(null, {
        status: 301,
        headers: { location, "cache-control": "public, max-age=86400" }
      });
    }
    return env.ASSETS.fetch(request);
  }
};
