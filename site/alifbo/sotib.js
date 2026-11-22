// Yangi Alifbo — sotib olish sahifasi. API: dellcom.uz/api/alifbo/* (nginx → pos-central, Server_v4/alifbo_shop.js)
const API = '/api/alifbo';
const $ = id => document.getElementById(id);
const qs = new URLSearchParams(location.search);
const HW = /^[a-f0-9]{32}$/.test(qs.get('hw') || '') ? qs.get('hw') : '';
const PC = (qs.get('pc') || '').slice(0, 64), VER = (qs.get('v') || '').slice(0, 20);
const money = n => String(Math.round(n)).replace(/\B(?=(\d{3})+(?!\d))/g, ' ') + " so'm";
const NET = "Internet bilan muammo. Iltimos, qayta urinib ko'ring.";
let poll = null;

async function call(path, body) {
  const r = await fetch(API + path, body ? { method: 'POST', headers: { 'content-type': 'application/json' }, body: JSON.stringify(body) } : {});
  const j = await r.json().catch(() => ({}));
  if (!r.ok) throw Object.assign(new Error(j.error || NET), { status: r.status });
  return j;
}

function show(o) {
  $('buyForm').hidden = true; $('orderBox').hidden = false;
  $('oNo').textContent = '№' + o.no;
  $('oAmount').textContent = money(o.amount);
  $('oNew').hidden = o.status !== 'new'; $('oPaid').hidden = o.status !== 'paid';
  $('oCancel').hidden = !(o.status === 'cancelled' || o.status === 'refunded');
  if (o.status === 'new') {
    $('oPayme').hidden = !o.payUrl; $('oManual').hidden = !!o.payUrl;
    if (o.payUrl) $('payBtn').href = o.payUrl;
    $('oReq').textContent = o.manualText || '';
    $('paidBtn').hidden = !!o.paidClaimed; $('paidNote').hidden = !o.paidClaimed;
  }
  if (o.status === 'paid') {
    $('oKey').textContent = o.key;
    $('oPaidPc').hidden = !o.boundToPc;
    if (poll) { clearInterval(poll); poll = null; }
  }
}

async function refresh(no, t) {
  try { const o = await call(`/order/${no}?t=${encodeURIComponent(t)}`); show(o); return o; }
  catch (e) { $('oMsg').textContent = e.status === 404 ? 'Buyurtma topilmadi.' : e.message; $('oMsg').className = 'form-msg err'; return null; }
}

function watch(no, t) {
  if (poll) clearInterval(poll);
  poll = setInterval(() => { if (!document.hidden) refresh(no, t); }, 5000);
}

async function start() {
  try { const i = await call('/info'); $('price').textContent = money(i.price); } catch {}
  if (HW) { $('pcNote').hidden = false; $('pcNote').textContent = `Kompyuter: ${PC || HW.slice(0, 8)} — to'lovdan keyin dastur shu kompyuterda o'zi faollashadi.`; }
  $('paidBtn').addEventListener('click', async () => {
    const p = new URLSearchParams(location.search);
    $('paidBtn').disabled = true;
    try { show(await call(`/order/${p.get('o')}/paid`, { t: p.get('t') })); }
    catch (e) { $('oMsg').textContent = e.message; $('oMsg').className = 'form-msg err'; }
    finally { $('paidBtn').disabled = false; }
  });
  const no = qs.get('o'), t = qs.get('t');
  if (/^\d{7}$/.test(no || '') && t) {
    const o = await refresh(no, t);
    if (o && o.status === 'new') watch(no, t);
    return;
  }
  const form = $('buyForm'), msg = form.querySelector('.form-msg'), btn = form.querySelector('button[type=submit]');
  form.addEventListener('submit', async e => {
    e.preventDefault();
    const d = Object.fromEntries(new FormData(form));
    if (!form.agree.checked) { msg.textContent = "Ommaviy oferta shartlariga rozilik belgisini qo'ying."; msg.className = 'form-msg err'; return; }
    btn.disabled = true; msg.textContent = 'Yuborilmoqda…'; msg.className = 'form-msg';
    try {
      const o = await call('/order', { agree: true, name: d.name || '', phone: d.phone || '', ...(HW ? { hwid: HW, pc: PC, version: VER } : {}) });
      history.replaceState(null, '', `?o=${o.no}&t=${o.token}`);
      try { localStorage.setItem('ya_order', JSON.stringify({ no: o.no, t: o.token })); } catch {}
      show(o);
      if (o.payUrl) location.href = o.payUrl; else watch(o.no, o.token);
    } catch (err) { msg.textContent = err.message; msg.className = 'form-msg err'; }
    finally { btn.disabled = false; }
  });
}
start();
