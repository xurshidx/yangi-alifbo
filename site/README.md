# dellcom.uz/alifbo — Yangi Alifbo sahifalari

Statik fayllar: `alifbo/` → VDS `/var/www/dellcom-site/alifbo/` (umumiy `/css/main.css`, `/favicon.svg` saytdan).
API: `/api/alifbo/*` — dellcom.uz nginx orqali pos-central serveriga (dell-platform `deploy/YANGILASH_v2_12_0.md`, 2-band).
MSI: `/var/www/dellcom-site/alifbo/yuklab/YangiAlifbo.msi` (har relizda almashtiriladi).
Sinov: `DCS_REPO=../dell-platform SAYT_CSS=<sayt main.css> node tests/sayt_e2e.cjs`.
Ofertadagi ⟨STIR⟩, ⟨manzil⟩, ⟨telefon⟩ — e'lon qilishdan oldin egasi to'ldiradi.
