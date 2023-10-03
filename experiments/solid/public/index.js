// https://esm.sh/stable/solid-js@1.7.11/esnext/solid-js.mjs
var h = { context: void 0, registry: void 0 };
var Te = (e, t) => e === t;
var se = Symbol("solid-proxy");
var Pe = Symbol("solid-track");
var bt = Symbol("solid-dev-component");
var ie = { equals: Te };
var L = null;
var Fe = De;
var M = 1;
var X = 2;
var je = { owned: null, cleanups: null, context: null, owner: null };
var d = null;
var u = null;
var K = null;
var z = null;
var g = null;
var y = null;
var S = null;
var oe = 0;
var [tt, ke] = I(false);
function B(e, t) {
  let r = g, n = d, i = e.length === 0, s = t === void 0 ? n : t, l = i ? je : { owned: null, cleanups: null, context: s ? s.context : null, owner: s }, o = i ? e : () => e(() => k(() => H(l)));
  d = l, g = null;
  try {
    return V(o, true);
  } finally {
    g = r, d = n;
  }
}
function I(e, t) {
  t = t ? Object.assign({}, ie, t) : ie;
  let r = { value: e, observers: null, observerSlots: null, comparator: t.equals || void 0 }, n = (i) => (typeof i == "function" && (u && u.running && u.sources.has(r) ? i = i(r.tValue) : i = i(r.value)), Re(r, i));
  return [qe.bind(r), n];
}
function nt(e, t, r) {
  let n = _(e, t, false, M);
  K && u && u.running ? y.push(n) : W(n);
}
function A(e, t, r) {
  r = r ? Object.assign({}, ie, r) : ie;
  let n = _(e, t, true, 0);
  return n.observers = null, n.observerSlots = null, n.comparator = r.equals || void 0, K && u && u.running ? (n.tState = M, y.push(n)) : W(n), qe.bind(n);
}
function k(e) {
  if (g === null)
    return e();
  let t = g;
  g = null;
  try {
    return e();
  } finally {
    g = t;
  }
}
function N(e) {
  return d === null || (d.cleanups === null ? d.cleanups = [e] : d.cleanups.push(e)), e;
}
function Ve(e) {
  if (u && u.running)
    return e(), u.done;
  let t = g, r = d;
  return Promise.resolve().then(() => {
    g = t, d = r;
    let n;
    return (K || U) && (n = u || (u = { sources: /* @__PURE__ */ new Set(), effects: [], promises: /* @__PURE__ */ new Set(), disposed: /* @__PURE__ */ new Set(), queue: /* @__PURE__ */ new Set(), running: true }), n.done || (n.done = new Promise((i) => n.resolve = i)), n.running = true), V(e, false), g = d = null, n ? n.done : void 0;
  });
}
function $e(e, t) {
  let r = Symbol("context");
  return { id: r, Provider: ft(r), defaultValue: e };
}
function Le(e) {
  let t = A(e), r = A(() => me(t()));
  return r.toArray = () => {
    let n = r();
    return Array.isArray(n) ? n : n != null ? [n] : [];
  }, r;
}
var U;
function qe() {
  let e = u && u.running;
  if (this.sources && (e ? this.tState : this.state))
    if ((e ? this.tState : this.state) === M)
      W(this);
    else {
      let t = y;
      y = null, V(() => le(this), false), y = t;
    }
  if (g) {
    let t = this.observers ? this.observers.length : 0;
    g.sources ? (g.sources.push(this), g.sourceSlots.push(t)) : (g.sources = [this], g.sourceSlots = [t]), this.observers ? (this.observers.push(g), this.observerSlots.push(g.sources.length - 1)) : (this.observers = [g], this.observerSlots = [g.sources.length - 1]);
  }
  return e && u.sources.has(this) ? this.tValue : this.value;
}
function Re(e, t, r) {
  let n = u && u.running && u.sources.has(e) ? e.tValue : e.value;
  if (!e.comparator || !e.comparator(n, t)) {
    if (u) {
      let i = u.running;
      (i || !r && u.sources.has(e)) && (u.sources.add(e), e.tValue = t), i || (e.value = t);
    } else
      e.value = t;
    e.observers && e.observers.length && V(() => {
      for (let i = 0; i < e.observers.length; i += 1) {
        let s = e.observers[i], l = u && u.running;
        l && u.disposed.has(s) || ((l ? !s.tState : !s.state) && (s.pure ? y.push(s) : S.push(s), s.observers && Ne(s)), l ? s.tState = M : s.state = M);
      }
      if (y.length > 1e6)
        throw y = [], new Error();
    }, false);
  }
  return t;
}
function W(e) {
  if (!e.fn)
    return;
  H(e);
  let t = d, r = g, n = oe;
  g = d = e, Ce(e, u && u.running && u.sources.has(e) ? e.tValue : e.value, n), u && !u.running && u.sources.has(e) && queueMicrotask(() => {
    V(() => {
      u && (u.running = true), g = d = e, Ce(e, e.tValue, n), g = d = null;
    }, false);
  }), g = r, d = t;
}
function Ce(e, t, r) {
  let n;
  try {
    n = e.fn(t);
  } catch (i) {
    return e.pure && (u && u.running ? (e.tState = M, e.tOwned && e.tOwned.forEach(H), e.tOwned = void 0) : (e.state = M, e.owned && e.owned.forEach(H), e.owned = null)), e.updatedAt = r + 1, ee(i);
  }
  (!e.updatedAt || e.updatedAt <= r) && (e.updatedAt != null && "observers" in e ? Re(e, n, true) : u && u.running && e.pure ? (u.sources.add(e), e.tValue = n) : e.value = n, e.updatedAt = r);
}
function _(e, t, r, n = M, i) {
  let s = { fn: e, state: n, updatedAt: null, owned: null, sources: null, sourceSlots: null, cleanups: null, value: t, owner: d, context: d ? d.context : null, pure: r };
  if (u && u.running && (s.state = 0, s.tState = n), d === null || d !== je && (u && u.running && d.pure ? d.tOwned ? d.tOwned.push(s) : d.tOwned = [s] : d.owned ? d.owned.push(s) : d.owned = [s]), z) {
    let [l, o] = I(void 0, { equals: false }), f = z(s.fn, o);
    N(() => f.dispose());
    let a = () => Ve(o).then(() => c.dispose()), c = z(s.fn, a);
    s.fn = (p) => (l(), u && u.running ? c.track(p) : f.track(p));
  }
  return s;
}
function J(e) {
  let t = u && u.running;
  if ((t ? e.tState : e.state) === 0)
    return;
  if ((t ? e.tState : e.state) === X)
    return le(e);
  if (e.suspense && k(e.suspense.inFallback))
    return e.suspense.effects.push(e);
  let r = [e];
  for (; (e = e.owner) && (!e.updatedAt || e.updatedAt < oe); ) {
    if (t && u.disposed.has(e))
      return;
    (t ? e.tState : e.state) && r.push(e);
  }
  for (let n = r.length - 1; n >= 0; n--) {
    if (e = r[n], t) {
      let i = e, s = r[n + 1];
      for (; (i = i.owner) && i !== s; )
        if (u.disposed.has(i))
          return;
    }
    if ((t ? e.tState : e.state) === M)
      W(e);
    else if ((t ? e.tState : e.state) === X) {
      let i = y;
      y = null, V(() => le(e, r[0]), false), y = i;
    }
  }
}
function V(e, t) {
  if (y)
    return e();
  let r = false;
  t || (y = []), S ? r = true : S = [], oe++;
  try {
    let n = e();
    return ut(r), n;
  } catch (n) {
    r || (S = null), y = null, ee(n);
  }
}
function ut(e) {
  if (y && (K && u && u.running ? ot(y) : De(y), y = null), e)
    return;
  let t;
  if (u) {
    if (!u.promises.size && !u.queue.size) {
      let n = u.sources, i = u.disposed;
      S.push.apply(S, u.effects), t = u.resolve;
      for (let s of S)
        "tState" in s && (s.state = s.tState), delete s.tState;
      u = null, V(() => {
        for (let s of i)
          H(s);
        for (let s of n) {
          if (s.value = s.tValue, s.owned)
            for (let l = 0, o = s.owned.length; l < o; l++)
              H(s.owned[l]);
          s.tOwned && (s.owned = s.tOwned), delete s.tValue, delete s.tOwned, s.tState = 0;
        }
        ke(false);
      }, false);
    } else if (u.running) {
      u.running = false, u.effects.push.apply(u.effects, S), S = null, ke(true);
      return;
    }
  }
  let r = S;
  S = null, r.length && V(() => Fe(r), false), t && t();
}
function De(e) {
  for (let t = 0; t < e.length; t++)
    J(e[t]);
}
function ot(e) {
  for (let t = 0; t < e.length; t++) {
    let r = e[t], n = u.queue;
    n.has(r) || (n.add(r), K(() => {
      n.delete(r), V(() => {
        u.running = true, J(r);
      }, false), u && (u.running = false);
    }));
  }
}
function le(e, t) {
  let r = u && u.running;
  r ? e.tState = 0 : e.state = 0;
  for (let n = 0; n < e.sources.length; n += 1) {
    let i = e.sources[n];
    if (i.sources) {
      let s = r ? i.tState : i.state;
      s === M ? i !== t && (!i.updatedAt || i.updatedAt < oe) && J(i) : s === X && le(i, t);
    }
  }
}
function Ne(e) {
  let t = u && u.running;
  for (let r = 0; r < e.observers.length; r += 1) {
    let n = e.observers[r];
    (t ? !n.tState : !n.state) && (t ? n.tState = X : n.state = X, n.pure ? y.push(n) : S.push(n), n.observers && Ne(n));
  }
}
function H(e) {
  let t;
  if (e.sources)
    for (; e.sources.length; ) {
      let r = e.sources.pop(), n = e.sourceSlots.pop(), i = r.observers;
      if (i && i.length) {
        let s = i.pop(), l = r.observerSlots.pop();
        n < i.length && (s.sourceSlots[l] = n, i[n] = s, r.observerSlots[n] = l);
      }
    }
  if (u && u.running && e.pure) {
    if (e.tOwned) {
      for (t = e.tOwned.length - 1; t >= 0; t--)
        H(e.tOwned[t]);
      delete e.tOwned;
    }
    Ue(e, true);
  } else if (e.owned) {
    for (t = e.owned.length - 1; t >= 0; t--)
      H(e.owned[t]);
    e.owned = null;
  }
  if (e.cleanups) {
    for (t = e.cleanups.length - 1; t >= 0; t--)
      e.cleanups[t]();
    e.cleanups = null;
  }
  u && u.running ? e.tState = 0 : e.state = 0;
}
function Ue(e, t) {
  if (t || (e.tState = 0, u.disposed.add(e)), e.owned)
    for (let r = 0; r < e.owned.length; r++)
      Ue(e.owned[r]);
}
function We(e) {
  return e instanceof Error ? e : new Error(typeof e == "string" ? e : "Unknown error", { cause: e });
}
function Ee(e, t, r) {
  try {
    for (let n of t)
      n(e);
  } catch (n) {
    ee(n, r && r.owner || null);
  }
}
function ee(e, t = d) {
  let r = L && t && t.context && t.context[L], n = We(e);
  if (!r)
    throw n;
  S ? S.push({ fn() {
    Ee(n, r, t);
  }, state: M }) : Ee(n, r, t);
}
function me(e) {
  if (typeof e == "function" && !e.length)
    return me(e());
  if (Array.isArray(e)) {
    let t = [];
    for (let r = 0; r < e.length; r++) {
      let n = me(e[r]);
      Array.isArray(n) ? t.push.apply(t, n) : t.push(n);
    }
    return t;
  }
  return e;
}
function ft(e, t) {
  return function(n) {
    let i;
    return nt(() => i = k(() => (d.context = { ...d.context, [e]: n.value }, Le(() => n.children))), void 0), i;
  };
}
var xe = Symbol("fallback");
var ve = $e();

// https://esm.sh/stable/solid-js@1.7.11/esnext/web.js
var _2 = ["allowfullscreen", "async", "autofocus", "autoplay", "checked", "controls", "default", "disabled", "formnovalidate", "hidden", "indeterminate", "ismap", "loop", "multiple", "muted", "nomodule", "novalidate", "open", "playsinline", "readonly", "required", "reversed", "seamless", "selected"];
var F = /* @__PURE__ */ new Set(["className", "value", "readOnly", "formNoValidate", "isMap", "noModule", "playsInline", ..._2]);
var U2 = Object.assign(/* @__PURE__ */ Object.create(null), { className: "class", htmlFor: "for" });
var K2 = Object.assign(/* @__PURE__ */ Object.create(null), { class: "className", formnovalidate: { $: "formNoValidate", BUTTON: 1, INPUT: 1 }, ismap: { $: "isMap", IMG: 1 }, nomodule: { $: "noModule", SCRIPT: 1 }, playsinline: { $: "playsInline", VIDEO: 1 }, readonly: { $: "readOnly", INPUT: 1, TEXTAREA: 1 } });
function J2(n, t, e) {
  let i = e.length, s = t.length, r = i, l = 0, o = 0, f = t[s - 1].nextSibling, a = null;
  for (; l < s || o < r; ) {
    if (t[l] === e[o]) {
      l++, o++;
      continue;
    }
    for (; t[s - 1] === e[r - 1]; )
      s--, r--;
    if (s === l) {
      let u2 = r < i ? o ? e[o - 1].nextSibling : e[r - o] : f;
      for (; o < r; )
        n.insertBefore(e[o++], u2);
    } else if (r === o)
      for (; l < s; )
        (!a || !a.has(t[l])) && t[l].remove(), l++;
    else if (t[l] === e[r - 1] && e[o] === t[s - 1]) {
      let u2 = t[--s].nextSibling;
      n.insertBefore(e[o++], t[l++].nextSibling), n.insertBefore(e[--r], u2), t[s] = e[r];
    } else {
      if (!a) {
        a = /* @__PURE__ */ new Map();
        let d2 = o;
        for (; d2 < r; )
          a.set(e[d2], d2++);
      }
      let u2 = a.get(t[l]);
      if (u2 != null)
        if (o < u2 && u2 < r) {
          let d2 = l, m = 1, p;
          for (; ++d2 < s && d2 < r && !((p = a.get(t[d2])) == null || p !== u2 + m); )
            m++;
          if (m > u2 - o) {
            let j = t[l];
            for (; o < u2; )
              n.insertBefore(e[o++], j);
          } else
            n.replaceChild(e[o++], t[l++]);
        } else
          l++;
      else
        t[l++].remove();
    }
  }
}
function Q(n, t, e, i = {}) {
  let s;
  return B((r) => {
    s = r, t === document ? n() : S2(t, n(), t.firstChild ? null : void 0, e);
  }, i.owner), () => {
    s(), t.textContent = "";
  };
}
function S2(n, t, e, i) {
  if (e !== void 0 && !i && (i = []), typeof t != "function")
    return y2(n, t, i, e);
  nt((s) => y2(n, t(), s, e), i);
}
function y2(n, t, e, i, s) {
  if (h.context) {
    !e && (e = [...n.childNodes]);
    let o = [];
    for (let f = 0; f < e.length; f++) {
      let a = e[f];
      a.nodeType === 8 && a.data.slice(0, 2) === "!$" ? a.remove() : o.push(a);
    }
    e = o;
  }
  for (; typeof e == "function"; )
    e = e();
  if (t === e)
    return e;
  let r = typeof t, l = i !== void 0;
  if (n = l && e[0] && e[0].parentNode || n, r === "string" || r === "number") {
    if (h.context)
      return e;
    if (r === "number" && (t = t.toString()), l) {
      let o = e[0];
      o && o.nodeType === 3 ? o.data = t : o = document.createTextNode(t), e = h2(n, e, i, o);
    } else
      e !== "" && typeof e == "string" ? e = n.firstChild.data = t : e = n.textContent = t;
  } else if (t == null || r === "boolean") {
    if (h.context)
      return e;
    e = h2(n, e, i);
  } else {
    if (r === "function")
      return nt(() => {
        let o = t();
        for (; typeof o == "function"; )
          o = o();
        e = y2(n, o, e, i);
      }), () => e;
    if (Array.isArray(t)) {
      let o = [], f = e && Array.isArray(e);
      if (A2(o, t, e, s))
        return nt(() => e = y2(n, o, e, i, true)), () => e;
      if (h.context) {
        if (!o.length)
          return e;
        for (let a = 0; a < o.length; a++)
          if (o[a].parentNode)
            return e = o;
      }
      if (o.length === 0) {
        if (e = h2(n, e, i), l)
          return e;
      } else
        f ? e.length === 0 ? M2(n, o, i) : J2(n, e, o) : (e && h2(n), M2(n, o));
      e = o;
    } else if (t.nodeType) {
      if (h.context && t.parentNode)
        return e = l ? [t] : t;
      if (Array.isArray(e)) {
        if (l)
          return e = h2(n, e, i, t);
        h2(n, e, null, t);
      } else
        e == null || e === "" || !n.firstChild ? n.appendChild(t) : n.replaceChild(t, n.firstChild);
      e = t;
    } else
      console.warn("Unrecognized value. Skipped inserting", t);
  }
  return e;
}
function A2(n, t, e, i) {
  let s = false;
  for (let r = 0, l = t.length; r < l; r++) {
    let o = t[r], f = e && e[r], a;
    if (!(o == null || o === true || o === false))
      if ((a = typeof o) == "object" && o.nodeType)
        n.push(o);
      else if (Array.isArray(o))
        s = A2(n, o, f) || s;
      else if (a === "function")
        if (i) {
          for (; typeof o == "function"; )
            o = o();
          s = A2(n, Array.isArray(o) ? o : [o], Array.isArray(f) ? f : [f]) || s;
        } else
          n.push(o), s = true;
      else {
        let u2 = String(o);
        f && f.nodeType === 3 && f.data === u2 ? n.push(f) : n.push(document.createTextNode(u2));
      }
  }
  return s;
}
function M2(n, t, e = null) {
  for (let i = 0, s = t.length; i < s; i++)
    n.insertBefore(t[i], e);
}
function h2(n, t, e, i) {
  if (e === void 0)
    return n.textContent = "";
  let s = i || document.createTextNode("");
  if (t.length) {
    let r = false;
    for (let l = t.length - 1; l >= 0; l--) {
      let o = t[l];
      if (s !== o) {
        let f = o.parentNode === n;
        !r && !l ? f ? n.replaceChild(s, o) : n.insertBefore(s, e) : f && o.remove();
      } else
        r = true;
    }
  } else
    n.insertBefore(s, e);
  return [s];
}

// src/App.tsx
var App = () => {
  const [count, setCount] = I(0), timer = setInterval(() => {
    setCount(count() + 360);
    document.getElementById("solid-logo")?.style.setProperty("--solid-logo-rotation", `${count()}deg`);
    console.log(`Set Logo rotation to ${count()}deg.`);
  }, 1e3);
  N(() => clearInterval(timer));
  return /* @__PURE__ */ React.createElement(React.Fragment, null, /* @__PURE__ */ React.createElement("div", { id: "root" }, /* @__PURE__ */ React.createElement("img", { src: "logo.svg", id: "solid-logo" }), /* @__PURE__ */ React.createElement("h1", null, "Get started by editing", /* @__PURE__ */ React.createElement("code", null, "src/App.tsx"), " and reloading! Learn ", /* @__PURE__ */ React.createElement("a", { href: "https://www.solidjs.com/guides/getting-started", target: "_blank" }, "Solid.js"), "!")));
};

// src/index.tsx
console.log("Starting to render Solid.js app...");
Q(() => /* @__PURE__ */ React.createElement(App, null), document.body);
