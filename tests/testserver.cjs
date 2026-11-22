'use strict';
/* Litsenziya sinovi uchun kichik server: dell-platform Server_v4/alifbo.js ni o'zgarishsiz ishlatadi (admin har doim ruxsat).
   Foydalanish: node tests/testserver.cjs <alifbo.js yo'li> <port> <ma'lumot papkasi>. Faqat 127.0.0.1 da. */
const http=require('node:http'),fs=require('node:fs');
const [,,modPath,port='18787',dir]=process.argv;
fs.mkdirSync(dir,{recursive:true});
const send=(res,s,b)=>{res.writeHead(s,{'content-type':'application/json; charset=utf-8'});res.end(JSON.stringify(b))};
const bodyJson=async req=>{const c=[];for await(const x of req)c.push(x);return c.length?JSON.parse(Buffer.concat(c).toString('utf8')):{}};
const A=require(require('node:path').resolve(modPath)).createAlifbo({dir,adminOK:()=>true,adminName:()=>'sinov',send,error:(res,s,m)=>send(res,s,{error:m}),bodyJson});
http.createServer(async(req,res)=>{try{const p=new URL(req.url,'http://x').pathname;if(!(await A.route(req,res,p)))send(res,404,{error:'yo‘q'})}catch(e){send(res,500,{error:e.message})}})
 .listen(Number(port),'127.0.0.1',()=>console.log('tayyor '+port));
