import { chromium } from 'playwright-core';
import assert from 'node:assert/strict';
import { freshState, SAVE_KEY } from '../game/state.js';
const browser=await chromium.launch({executablePath:process.env.CHROMIUM_PATH||chromium.executablePath(),headless:true,args:process.env.CI?['--no-sandbox']:[]});
let page=await browser.newPage({viewport:{width:1440,height:960}});const errors=[];page.on('pageerror',e=>errors.push(e.message));
const url=process.env.GAME_URL||'http://localhost:5173';
async function restore(s){await page.close();page=await browser.newPage({viewport:{width:1440,height:960}});page.on('pageerror',e=>errors.push(e.message));await page.addInitScript(({s,key})=>localStorage.setItem(key,JSON.stringify(s)),{s,key:SAVE_KEY});await page.goto(url);await page.waitForSelector('#tools .tool');}
async function clickWorld(x,y){const box=await page.locator('canvas').boundingBox();await page.mouse.click(box.x+x/1120*box.width,box.y+y/704*box.height);}
try{
 await page.goto(url);await page.waitForSelector('#tools .tool');
 // Real pointer navigation, conversation, shop purchase and persistence.
 await page.getByRole('button',{name:/Visitar a vila/}).click();await clickWorld(336,325);await page.waitForSelector('#conversation[open]');assert.equal(await page.locator('#npc-name').textContent(),'Lia');await page.locator('[data-npc-action="shop"]').click();await page.locator('[data-buy="0"]').click();await page.locator('[data-close="shop-dialog"]').click();await page.reload();assert.equal(await page.locator('#coins').textContent(),'105');
 // Actual UI farming cycle, including the overnight confirmation.
 let s=freshState();s.player={x:720,y:304};s.plots[5]={tilled:true,crop:0,growth:2,water:false};await restore(s);await page.getByRole('button',{name:'Regador',exact:true}).click();await clickWorld(720,272);await page.waitForFunction(()=>document.querySelector('#toast').textContent.includes('Água fresquinha'));
 await page.locator('#sleep').click();await page.locator('#confirm-rest').click();await page.getByRole('button',{name:'Cesta',exact:true}).click();await clickWorld(720,272);await page.waitForFunction(()=>document.querySelector('#toast').textContent.includes('Colheita guardada'));await page.getByRole('button',{name:'Mochila',exact:true}).click();assert.match(await page.locator('#sidebar-content').textContent(),/1 colhidos/);
 // Iara actually unlocks fishing, the catch is persisted.
 s=freshState();s.scene='village';s.player={x:912,y:464};await restore(s);await page.keyboard.press('e');await page.waitForSelector('#conversation[open]');assert.equal(await page.locator('#npc-name').textContent(),'Iara');await page.locator('[data-npc-action="bye"]').click();await page.keyboard.press('f');await page.waitForSelector('#fishing-hint.ready');await page.keyboard.press('Space');await page.waitForFunction(()=>JSON.parse(localStorage.getItem('vale-do-sol-v2')).fish===1);
 // Corrupt imports do not replace valid progress.
 await page.locator('#save-file').setInputFiles({name:'broken.json',mimeType:'application/json',buffer:Buffer.from('{"version":2}')});await page.waitForFunction(()=>document.querySelector('#toast').textContent.includes('não é um save válido'));
 // The desktop-sized viewport fits and the phone layout does not overflow horizontally.
 assert.ok(await page.evaluate(()=>document.documentElement.scrollHeight<=window.innerHeight+2));
 await page.setViewportSize({width:390,height:844});assert.ok(await page.evaluate(()=>document.documentElement.scrollWidth<=window.innerWidth));
 await page.getByRole('button',{name:/Como jogar/}).click();await page.locator('[data-close="help"]').first().click();assert.deepEqual(errors,[]);
 console.log('PASS browser: NPCs, loja, caminhada, save, rega/descanso/colheita, pesca, importação inválida e layouts.');
}finally{await browser.close();}
