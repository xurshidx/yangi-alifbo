'use strict';
/* dellcom.uz/alifbo sahifalari — haqiqiy Chrome, dell-platform serveri (DCS_REPO), soxta nginx (statik + /api/alifbo/ proksi).
   Foydalanish: DCS_REPO=/yo'l/dell-platform SAYT_CSS=/yo'l/main.css node tests/sayt_e2e.cjs */
const assert=require('node:assert/strict'),http=require('node:http'),fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const REPO=process.env.DCS_REPO||path.join(__dirname,'../../dell-platform');
process.env.RATE_LIMIT_PER_MIN='900';process.env.INTEGRATION_ENCRYPTION_KEY=crypto.randomBytes(32).toString('hex');
const H=require(path.join(REPO,'Sinovlar/v240_lib.cjs')).create('sayt');
const SITE=path.join(__dirname,'../site');const CSS=process.env.SAYT_CSS||'';
const hw=s=>crypto.createHash('sha256').update(s).digest('hex').slice(0,32);
(async()=>{let ok=false,web;try{
 if(!H.chromium||!H.CHROME){console.log('SKIP');ok=true;return}
 await H.start();const Ad=await H.admin();
 web=http.createServer(async(req,res)=>{const u=new URL(req.url,'http://x');
  if(u.pathname.startsWith('/api/alifbo/')){let b=[];for await(const c of req)b.push(c);const r=await fetch(H.base+req.url,{method:req.method,headers:{'content-type':req.headers['content-type']||'application/json',...(process.env.PROXY_ORIGIN==='1'&&req.headers.origin?{origin:req.headers.origin}:{})} /* nginx kabi: Origin tozalanadi — PROXY_ORIGIN=1 bo'lsa uzatiladi (CORS 403 ni ko'rsatadi) */,body:req.method==='POST'?Buffer.concat(b):undefined});
   res.writeHead(r.status,{'content-type':'application/json'});return res.end(Buffer.from(await r.arrayBuffer()))}
  let f=u.pathname==='/css/main.css'&&CSS?CSS:path.join(SITE,u.pathname.endsWith('/')?u.pathname+'index.html':u.pathname);if(!fs.existsSync(f)&&fs.existsSync(f+'.html'))f+='.html';
  if(!fs.existsSync(f)){res.writeHead(404);return res.end()}
  res.writeHead(200,{'content-type':f.endsWith('.js')?'text/javascript':f.endsWith('.css')?'text/css':'text/html; charset=utf-8'});res.end(fs.readFileSync(f))});
 await new Promise(r=>web.listen(0,'127.0.0.1',r));const W='http://127.0.0.1:'+web.address().port;
 const b=await H.browser();const ctx=await b.newContext({viewport:{width:1366,height:900}});const pg=await ctx.newPage();
 pg.on('pageerror',e=>H.errors.push(e.message));pg.on('console',m=>{if(m.type()==='error'&&!/favicon|fonts|img|status of 4[0-9][0-9]/.test(m.text()))H.errors.push(m.text())});
 /* bosh sahifa */
 await pg.goto(W+'/alifbo/');assert.match(await pg.title(),/Yangi Alifbo/);assert.match(await pg.textContent('h1'),/bir tugmada/);
 assert.ok(await pg.$('a[href="/alifbo/sotib-olish"]'));await pg.screenshot({path:'/tmp/sayt_index.png'});
 H.pass('dellcom.uz/alifbo: bosh sahifa');
 /* yopiq (rekvizit yo'q) */
 const PC=hw('sayt-pc');await pg.goto(W+`/alifbo/sotib-olish.html?hw=${PC}&pc=OFIS-PC&v=3.2.0`);
 await pg.waitForFunction(()=>!document.getElementById('pcNote').hidden);assert.match(await pg.textContent('#pcNote'),/OFIS-PC/);
 await pg.click('#buyForm button[type=submit]');assert.match(await pg.textContent('#buyForm .form-msg'),/rozilik/);
 await pg.check('#buyForm input[name=agree]');await pg.click('#buyForm button[type=submit]');await pg.waitForFunction(()=>/onlayn sotuv yopiq/.test(document.querySelector('#buyForm .form-msg').textContent));
 H.pass('sotib olish: kompyuter nomi ko‘rinadi; ofertasiz yuborilmaydi; rekvizit yo‘q — tushunarli xabar');
 /* qo'lda */
 await H.call(Ad,'/api/admin/alifbo/shop',{manualText:'Karta: 8600 1234 5678 9012\nOluvchi: Raximov X.'});
 await pg.reload();await pg.fill('#buyForm input[name=name]','Aziz');await pg.fill('#buyForm input[name=phone]','+998 90 123 45 67');await pg.check('#buyForm input[name=agree]');
 await pg.click('#buyForm button[type=submit]');await pg.waitForSelector('#orderBox:not([hidden])');
 const no=(await pg.textContent('#oNo')).replace('№','');assert.match(no,/^\d{7}$/);assert.equal(await pg.textContent('#oReq'),'Karta: 8600 1234 5678 9012\nOluvchi: Raximov X.');assert.match(pg.url(),/\?o=\d{7}&t=[a-f0-9]{32}$/);assert.equal(await pg.isVisible('#buyForm'),false,'buyurtmadan keyin forma yashirin');assert.equal(await pg.isVisible('#oPayme'),false);
 await pg.screenshot({path:'/tmp/sayt_qolda.png',fullPage:true});
 await pg.reload();await pg.waitForSelector('#orderBox:not([hidden])');await pg.click('#paidBtn');await pg.waitForSelector('#paidNote:not([hidden])');
 let r=await H.call(Ad,'/api/admin/alifbo');const o=r.data.orders.find(x=>x.no===no);assert.ok(o.claimedAt);assert.equal(o.pc,'OFIS-PC');assert.equal(o.name,'Aziz');
 await H.call(Ad,'/api/admin/alifbo/confirm',{no});await pg.waitForSelector('#oPaid:not([hidden])',{timeout:12000});
 const key=(await pg.textContent('#oKey')).trim();assert.match(key,/^YA-/);assert.equal((await H.call(null,'/api/alifbo/claim',{hwid:PC})).data.key,key);
 await pg.screenshot({path:'/tmp/sayt_tolandi.png',fullPage:true});
 assert.equal(await pg.isVisible('#buyForm'),false,'to‘langanda forma yashirin');assert.equal(await pg.isVisible('#oNew'),false);
 H.pass('qo‘lda: buyurtma → rekvizit; sahifa qayta ochilsa ham «To‘ladim» ishlaydi; tasdiqdan keyin kalit sahifada o‘zi chiqadi, dastur claim bilan oladi');
 /* payme */
 await H.call(Ad,'/api/admin/alifbo/shop',{mode:'payme',merchantId:'5e730e8e0b852a417aa49ceb',testKey:'TestKey_0123456789',test:true});
 await pg.route('https://test.paycom.uz/**',rt=>rt.fulfill({status:200,contentType:'text/html',body:'<h1>PAYME TEST</h1>'}));
 await pg.goto(W+'/alifbo/sotib-olish.html');await pg.check('#buyForm input[name=agree]');await pg.click('#buyForm button[type=submit]');
 await pg.waitForURL(/test\.paycom\.uz/);const dec=Buffer.from(pg.url().replace('https://test.paycom.uz/',''),'base64').toString();assert.match(dec,/^m=5e730e8e0b852a417aa49ceb;ac\.order_id=\d{7};a=1500000;c=https:\/\/dellcom\.uz\/alifbo\/sotib-olish\.html\?o=\d{7}&t=[a-f0-9]{32};l=uz$/);
 H.pass('Payme rejimi: buyurtmadan keyin to‘g‘ridan-to‘g‘ri Payme test sahifasiga');
 await pg.goto(W+'/alifbo/oferta');assert.match(await pg.textContent('h1'),/Ommaviy oferta/);
 console.log('ERRORS',JSON.stringify(H.errors));assert.deepEqual(H.errors,[]);H.pass('oferta sahifasi; JS xatosi yo‘q');ok=true;
}catch(e){console.error('FAIL',e)}finally{if(web)web.close();await H.stop()}if(!ok)process.exitCode=1})();
