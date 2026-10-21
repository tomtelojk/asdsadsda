import {CROPS,NPCS,weather} from './state.js';
import {buildings,trees,resources,pond,plotPosition} from './world.js';
export function createRenderer(canvas){
 const c=canvas.getContext('2d');c.imageSmoothingEnabled=false;
 const r=(x,y,w,h,color)=>{c.fillStyle=color;c.fillRect(Math.round(x),Math.round(y),w,h);};
 function text(s,x,y,size=11,color='#fbf3d7',align='center'){c.font=`${size}px monospace`;c.textAlign=align;c.fillStyle='#29463855';c.fillText(s,x+1,y+1);c.fillStyle=color;c.fillText(s,x,y);}
 function tree(x,y,small=false){const sc=small?.8:1;c.save();c.translate(x,y);c.scale(sc,sc);r(-28,6,64,12,'#294c3430');r(-7,-29,15,46,'#856247');r(-3,-23,5,36,'#a38056');r(-35,-65,70,38,'#49744a');r(-43,-52,86,24,'#49744a');r(-29,-85,58,50,'#5e8650');r(-34,-67,64,24,'#5e8650');r(-19,-93,35,16,'#759557');r(-23,-79,23,9,'#81a25e');r(-29,-53,15,6,'#749754');r(17,-48,17,9,'#3e6947');c.restore();}
 function house(b){const x=b.x*32,y=b.y*32,w=b.w*32,h=b.h*32;r(x-9,y+h-5,w+24,17,'#34503930');r(x,y+45,w,h-45,b.wall);for(let k=55;k<h;k+=15)r(x+5,y+k,w-10,2,'#a58a6433');r(x+6,y+49,6,h-49,'#c0a67a');r(x+w-12,y+49,6,h-49,'#c0a67a');r(x-9,y+42,w+18,10,'#745641');for(let i=0;i<8;i++){r(x-9+i*9,y+39-i*7,w+18-i*18,8,i%2?b.roof:shade(b.roof,12));}r(x+w-45,y-7,15,42,'#a78465');r(x+w-49,y-10,23,6,'#c9ad8a');for(const xx of [x+24,x+w-55]){r(xx,y+70,30,34,'#8a7555');r(xx+4,y+74,22,25,'#8db0ad');r(xx+14,y+74,3,25,'#f5deb2');r(xx+4,y+85,22,3,'#f5deb2');r(xx-3,y+104,36,6,'#b49467');r(xx,y+103,6,4,'#68854d');r(xx+18,y+101,5,6,'#d78c8e');}const dx=x+w/2-17;r(dx,y+h-48,34,48,'#785d43');r(dx+4,y+h-44,26,44,'#a17e55');r(dx+24,y+h-24,3,3,'#efd995');r(dx-9,y+h,52,7,'#b79e76');r(dx-14,y+h+7,62,5,'#d0b88c');r(x+w/2-65,y+50,130,18,'#5b634a');text(b.name,x+w/2,y+63,10);if(b.type==='shop'){for(let k=0;k<8;k++)r(x+8+k*(w-16)/8,y+47,(w-16)/8,11,k%2?'#eee0b1':'#849a72');}if(b.type==='flowers'){for(let k=0;k<5;k++){r(x+14+k*28,y+h-13,16,13,'#a47956');r(x+17+k*28,y+h-24,9,12,'#6f954e');r(x+14+k*28,y+h-26,15,5,'#d59ca6');}}}
 function shade(hex,n){const v=parseInt(hex.slice(1),16);return '#'+[v>>16,(v>>8)&255,v&255].map(x=>Math.min(255,x+n).toString(16).padStart(2,'0')).join('');}
 function character(x,y,color,hair,name,walk,farmer=false){const bob=walk?Math.sin(walk*12)*2:0; r(x-12,y+10,25,7,'#294c3438');r(x-7,y+4,6,12+(walk?Math.sin(walk*12)*2:0),'#4d6260');r(x+3,y+4,6,12-(walk?Math.sin(walk*12)*2:0),'#4d6260');r(x-10,y-12+bob,22,21,color);r(x-14,y-9+bob,5,15,'#e8bb92');r(x+12,y-9+bob,5,15,'#e8bb92');r(x-8,y-28+bob,18,18,'#eac29a');r(x-9,y-29+bob,20,7,hair);r(x-9,y-25+bob,4,12,hair);r(x+5,y-20+bob,2,3,'#3d4c40');if(farmer){r(x-15,y-31+bob,32,6,'#e1bf77');r(x-10,y-40+bob,22,10,'#ebce8b');r(x-10,y-32+bob,22,3,'#af8d56');r(x-5,y-10+bob,13,18,'#77989b');}if(name){r(x-27,y-54,54,16,'#334d3de0');text(name,x,y-42,10);}}
 function crop(x,y,p,t){if(p.crop===null)return;const cr=CROPS[p.crop],ripe=p.growth>=cr.days;const h=5+Math.min(p.growth,cr.days)*4;r(x-1,y-h,3,h,'#50763e');r(x-10,y-h+2,9,4,'#789a50');r(x+2,y-h-1,9,4,'#5b8a45');r(x-7,y-h-2,4,4,'#8cad5e');if(ripe){r(x-5,y-5,11,10,cr.color);r(x-2,y+5,5,3,cr.color);if(p.crop===2){r(x-10,y-8,7,8,cr.color);r(x+5,y-7,7,9,cr.color);r(x+7,y-5,2,2,'#ffe5b1');}if(Math.sin(t*3+x)>.5){r(x+12,y-22,2,7,'#fff1b4');r(x+10,y-20,6,2,'#fff1b4');}}}
 function water(scene,t){const p=pond(scene),x=p.x*32,y=p.y*32,w=p.w*32,h=p.h*32;r(x-8,y+8,w+16,h-16,'#6e986a');r(x+10,y-7,w-20,h+14,'#6e986a');r(x,y+5,w,h-10,'#93bbae');r(x+8,y,w-16,h,'#93bbae');r(x+9,y+9,w-18,h-18,'#70a2a2');for(let i=0;i<18;i++){let xx=x+14+(i*47)%(w-35)+Math.sin(t+i)*3,yy=y+15+(i*29)%(h-30);r(xx,yy,12,2,'#b9d1bc');}r(x+32,y+28,18,8,'#548c61');r(x+36,y+23,14,7,'#74a36c');r(x+42,y+24,4,4,'#efd3ad');for(let i=0;i<5;i++){r(x-8+i*17,y+h-10,3,16,'#658c55');r(x-10+i*17,y+h-13,5,7,'#a68a51');}if(scene==='village'){r(x+35,y-18,88,25,'#af9266');for(let j=0;j<6;j++)r(x+36+j*15,y-18,2,25,'#87704e');r(x+38,y-24,5,33,'#8e724c');r(x+116,y-24,5,33,'#8e724c');}}
 return function render(s,{time,hover=-1,npcPositions=[],moving=false,target=null,fishing=null}){
 const scene=s.scene;r(0,0,1120,704,scene==='farm'?'#9db77b':'#a5bb83');
 for(let i=0;i<520;i++){const x=(i*157+19)%1120,y=(i*83+37)%704;r(x,y,3,i%5===0?5:2,i%3?'#91ad70':'#b2c58a');if(i%19===0){r(x-2,y-1,6,3,i%2?'#e6dfb1':'#d6a7a1');}}
 // paths
 r(0,345,1120,68,'#c8b488');r(0,351,1120,55,'#dac69a');if(scene==='farm'){r(397,0,66,704,'#c8b488');r(403,0,54,704,'#dac69a');r(159,221,52,150,'#dac69a');}else{r(512,190,78,514,'#c8b488');r(518,190,66,514,'#dac69a');r(206,245,53,120,'#dac69a');r(833,245,53,120,'#dac69a');r(760,375,54,168,'#dac69a');r(497,403,115,150,'#dac69a');}
 for(let i=0;i<64;i++)r((i*127)%1120,355+(i*13)%40,6,2,'#cbb689');
 // perimeter hedge
 for(let i=0;i<35;i++){r(i*32,0,31,22,'#668855');r(i*32+4,0,23,14,'#7a985e');if(i<12||i>15){r(i*32,689,31,15,'#6c8d57');}}
 water(scene,time);
 buildings[scene].forEach(house);
 if(scene==='farm'){
  r(529,234,283,234,'#b29b6d');r(535,240,271,222,'#8f7653');
  for(let i=0;i<48;i++){const p=s.plots[i],{x,y}=plotPosition(i);r(x,y,30,30,p.tilled?(p.water?'#78684d':'#a78456'):'#a6b77b');if(p.tilled){for(let k=0;k<3;k++)r(x+3,y+6+k*8,24,2,p.water?'#6a5c44':'#937349');if(p.water){r(x+3,y+3,2,2,'#a0b4a7');r(x+23,y+21,2,2,'#9bafa4');}}else{r(x+7,y+11,3,5,'#8a9f65');}crop(x+15,y+20,p,time);if(hover===i){c.strokeStyle='#fff1b2';c.lineWidth=2;c.strokeRect(x,y,30,30);}}
  for(let i=0;i<11;i++){const x=514+i*29;for(const y of [215,476]){r(x,y+8,29,4,'#c4a777');r(x,y+19,29,4,'#b49a6e');r(x,y+2,5,29,'#d8bc85');r(x-1,y,7,5,'#ead09a');}}
  r(318,280,5,31,'#8d7454');r(307,269,28,18,'#b67d5b');r(310,272,21,11,'#ca9970');r(333,264,3,14,'#e4c478');r(336,264,9,5,'#d3a65d');
  r(1017,315,5,33,'#95764c');r(988,309,62,23,'#d9bf87');text('VILA →',1018,325,10,'#6e6245');
 } else {
  // central fountain with flowers and stone square
  r(455,417,196,126,'#b4b19b');for(let i=0;i<9;i++)r(460+(i%3)*63,421+Math.floor(i/3)*39,58,34,'#c6c3a9');r(502,450,105,56,'#929c91');r(509,444,91,54,'#cbd1b7');r(516,451,77,37,'#8bb6b0');r(545,426,18,47,'#b8c3b1');r(538,421,32,9,'#d9dcc4');r(548,415,12,8,'#dfdfc9');r(550,407+Math.sin(time*3)*2,7,12,'#bddbd0');for(const x of [439,650]){r(x,449,13,48,'#ab835e');r(x-2,443,17,7,'#c59b6b');r(x-1,432,15,11,'#789757');r(x,428,7,7,'#e7c49a');}
  // benches and noticeboard
  for(const [x,y]of [[350,440],[673,440],[350,540]]){r(x,y,53,9,'#ab8961');r(x,y+14,53,7,'#c6a474');r(x+4,y+20,5,10,'#816b4c');r(x+44,y+20,5,10,'#816b4c');}
  r(338,293,5,42,'#94744d');r(390,293,5,42,'#94744d');r(329,271,74,47,'#9b7952');r(334,276,64,36,'#d5bd87');r(341,282,15,19,'#f3e8c3');r(363,281,25,24,'#e9ddb4');text('FAZENDA',76,385,10,'#7b6b4b');text('←',22,384,19,'#7b6b4b');
 }
 for(const res of resources.filter(a=>a.scene===scene&&!s.gathered.includes(a.id))){const x=res.x*32+16,y=res.y*32+16;if(res.type==='wood'){r(x-14,y-6,28,14,'#8d714c');r(x-16,y-7,10,16,'#c4a36e');r(x-13,y-4,4,10,'#967849');r(x+1,y-5,3,12,'#a58759');}else{r(x-13,y-3,26,13,'#929c8d');r(x-9,y-9,17,11,'#b2baa5');r(x-7,y-7,8,3,'#ccd0b7');}}
 const drawables=trees[scene].map(([a,b])=>({y:b*32+20,draw:()=>tree(a*32+16,b*32+20,b%3===0)}));
 for(const n of npcPositions)drawables.push({y:n.y,draw:()=>character(n.x,n.y,n.color,n.hair,n.name,Math.sin(time+n.x)*.1)});
 drawables.push({y:s.player.y,draw:()=>character(s.player.x,s.player.y,'#bc9a67','#765b42',null,moving?time:0,true)});drawables.sort((a,b)=>a.y-b.y).forEach(o=>o.draw());
 if(target){c.strokeStyle='#fff4be88';c.lineWidth=2;c.beginPath();c.ellipse(target.x,target.y+10,9,4,0,0,Math.PI*2);c.stroke();}
 // fireflies, birds and rain
 for(let i=0;i<3;i++){const x=190+i*280+Math.sin(time*.3+i)*60,y=85+Math.cos(time*.5+i)*25;r(x,y,6,2,'#f5ecc7');r(x-4,y-3+Math.sin(time*8)*2,5,2,'#f5ecc7');r(x+6,y-3+Math.sin(time*8)*2,5,2,'#f5ecc7');}
 const evening=Math.max(0,(s.minute-1020)/400);if(evening>0)r(0,0,1120,704,`rgba(32,42,64,${Math.min(.42,evening)})`);
 if(weather(s)==='rain'){r(0,0,1120,704,'#5e7c8c28');for(let i=0;i<100;i++){const x=(i*149+time*70)%1120,y=(i*83+time*370)%704;r(x,y,1,9,'#d7e6df77');}}
 if(fishing){const x=s.player.x+38,y=s.player.y-5;c.strokeStyle='#d9c69c';c.lineWidth=2;c.beginPath();c.moveTo(s.player.x+14,s.player.y-14);c.lineTo(x,y-30);c.lineTo(x+35,y+35);c.stroke();r(x+32,y+32,6,5,fishing.ready?'#f2ca79':'#e8c4aa');if(fishing.ready){r(s.player.x-46,s.player.y-79,92,25,'#f2d391');text('AGORA! [ESPAÇO]',s.player.x,s.player.y-62,10,'#5a6041');}}
 };
}
