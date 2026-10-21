export const SAVE_KEY = 'vale-do-sol-v2';
export const CROPS = [
  { id: 'carrot', name: 'Cenoura', icon: '🥕', days: 3, price: 15, sell: 14, color: '#ef9c42' },
  { id: 'turnip', name: 'Nabo', icon: '🌱', days: 2, price: 10, sell: 9, color: '#eee1bc' },
  { id: 'berry', name: 'Morango', icon: '🍓', days: 4, price: 25, sell: 23, color: '#dc6c79' }
];
export const NPCS = [
  { id: 'lia', name: 'Lia', role: 'Dona do armazém', color: '#d69a70', hair: '#694b3d', morning: [10, 10], afternoon: [13, 12], lines: ['Você deve ser a nova pessoa da Fazenda Girassol! Eu sou a Lia. A vila estava precisando desse seu jeito de recomeçar.', 'Minha avó dizia: uma semente é uma promessa pequena. Regue as suas todos os dias, combinado?', 'Levo o armazém a sério, mas um café na praça sempre vem primeiro. Como vai sua horta?'] },
  { id: 'bento', name: 'Bento', role: 'Carpinteiro', color: '#819ca1', hair: '#655e50', morning: [24, 10], afternoon: [21, 14], lines: ['Bento. Carpinteiro, colecionador de pregos e péssimo contador de piadas. Se precisar melhorar suas ferramentas, é comigo.', 'O regador de cobre alcança os canteiros vizinhos. Traga 8 madeiras e 60 moedas que eu faço a melhoria.', 'Essa vila já viu muitos invernos. Sempre tem uma porta aberta para quem cuida da terra.'] },
  { id: 'iara', name: 'Iara', role: 'Pescadora', color: '#a28bb0', hair: '#3e4e47', morning: [28, 15], afternoon: [24, 15], lines: ['O lago tem mais histórias do que peixes. Toma, uma vara antiga minha. Perto da água, aperte F e espere a fisgada!', 'Quando aparecer AGORA!, aperte Espaço. Antes ou depois disso, o peixe leva a isca e deixa a história.', 'Dias de chuva são meus favoritos. O mundo fica quietinho e os peixes aparecem para conversar.'] },
  { id: 'theo', name: 'Theo', role: 'Botânico da vila', color: '#89a76b', hair: '#9d714f', morning: [16, 16], afternoon: [8, 16], lines: ['Sou Theo! Estou estudando o solo do vale. Três cenouras da sua primeira colheita seriam perfeitas para minha pesquisa.', 'Não se preocupe se precisar de tempo. Aqui, cada coisa cresce na sua estação.', 'Quando chove, você pode deixar o regador em casa. A natureza também gosta de ajudar.'] }
];
export function freshState() {
  return { version: 2, day: 1, minute: 480, coins: 120, energy: 100, maxEnergy: 100,
    seeds: [10, 5, 0], inventory: [0, 0, 0], wood: 0, stone: 0, fish: 0, totalHarvest: 0,
    selected: 0, scene: 'farm', player: {x: 464, y: 400},
    plots: Array.from({length:48}, (_,i) => ({tilled:i<16, crop:i<3?0:null, growth:i<3?1:0, water:false})),
    friends: Object.fromEntries(NPCS.map(n=>[n.id,0])), met: [], talked: {}, gifted: {}, claimed: [], gathered: [],
    rod: false, wateringLevel: 1, flags: {tilled:false,planted:false,watered:false}, rng: 4871 };
}
const int = (v,min,max) => Number.isInteger(v) && v>=min && v<=max;
export function validState(s) {
  if (!s || s.version!==2 || !int(s.day,1,100000) || !int(s.minute,360,1440) || !int(s.coins,0,1e9) || !int(s.maxEnergy,100,200) || !int(s.energy,0,s.maxEnergy)) return false;
  if (!['farm','village'].includes(s.scene) || !s.player || !Number.isFinite(s.player.x) || !Number.isFinite(s.player.y) || s.player.x<16 || s.player.x>1104 || s.player.y<32 || s.player.y>680) return false;
  for(const key of ['seeds','inventory']) if(!Array.isArray(s[key]) || s[key].length!==3 || !s[key].every(n=>int(n,0,1e7))) return false;
  if(!Array.isArray(s.plots) || s.plots.length!==48 || !s.plots.every(p=>p && typeof p.tilled==='boolean' && (p.crop===null||int(p.crop,0,2)) && int(p.growth,0,100000) && typeof p.water==='boolean')) return false;
  if(!NPCS.every(n=>int(s.friends?.[n.id],0,5))) return false;
  if(!Array.isArray(s.met)||!s.met.every(id=>NPCS.some(n=>n.id===id))||!s.talked||!s.gifted) return false;
  if(!Array.isArray(s.claimed)||!s.claimed.every(id=>['meet','harvest','delivery'].includes(id))||!Array.isArray(s.gathered)||!s.gathered.every(id=>typeof id==='string')) return false;
  if(!['wood','stone','fish','totalHarvest'].every(k=>int(s[k],0,1e7))||!int(s.selected,0,2)||!int(s.wateringLevel,1,2)||typeof s.rod!=='boolean'||!int(s.rng,1,2147483646)) return false;
  return ['tilled','planted','watered'].every(k=>typeof s.flags?.[k]==='boolean');
}
export function loadState(storage) {
  try { const saved=JSON.parse(storage.getItem(SAVE_KEY)); if(validState(saved)) return saved;
    const old=JSON.parse(storage.getItem('vale-do-sol-v1'));
    if(old && int(old.day,1,100000) && int(old.coins,0,1e9) && Array.isArray(old.plots) && old.plots.length===48){
      const s=freshState();s.day=old.day;s.coins=old.coins;s.seeds=old.seeds;s.plots=old.plots;s.selected=old.selected??0;s.energy=old.energy;
      if(validState(s)) return s;
    }
  } catch {} return freshState();
}
export function weather(s) { return s.day%4===0 ? 'rain' : 'sun'; }
export function random(s) { s.rng=s.rng*16807%2147483647; return (s.rng-1)/2147483646; }
export function nextDay(s) {
  let grown=0;
  for(const p of s.plots){if(p.crop!==null && (p.water || weather(s)==='rain')){p.growth=Math.min(CROPS[p.crop].days,p.growth+1);grown++;} p.water=false;}
  s.day++;s.minute=480;s.energy=s.maxEnergy;s.gathered=[];
  if(weather(s)==='rain')s.plots.forEach(p=>{if(p.tilled)p.water=true;});
  return grown;
}
export function farmAction(s,index,tool) {
  const p=s.plots[index]; if(!p)return {ok:false,message:'Escolha um canteiro.'};
  if(s.energy<3)return {ok:false,message:'Você está cansado. Descanse na sua casa.'};
  if(tool===0){if(p.tilled)return {ok:false,message:'Terra pronta! Selecione as sementes (2).'};p.tilled=true;s.flags.tilled=true;}
  else if(tool===1){if(!p.tilled)return {ok:false,message:'Prepare a terra com a enxada (1).'};if(p.crop!==null)return {ok:false,message:'Já existe uma planta neste canteiro.'};if(s.seeds[s.selected]===0)return {ok:false,message:'Sem sementes! Visite o armazém da Lia na vila.'};p.crop=s.selected;p.growth=0;p.water=weather(s)==='rain';s.seeds[s.selected]--;s.flags.planted=true;}
  else if(tool===2){if(!p.tilled)return {ok:false,message:'Prepare a terra antes de regar.'};if(p.water)return {ok:false,message:'Este canteiro já foi regado hoje.'};p.water=true;s.flags.watered=true;if(s.wateringLevel===2){for(const j of [index-8,index+8,index%8>0?index-1:-1,index%8<7?index+1:-1])if(s.plots[j]?.tilled)s.plots[j].water=true;}}
  else if(tool===3){if(p.crop===null)return {ok:false,message:'Não há nada para colher aqui.'};const c=CROPS[p.crop];if(p.growth<c.days)return {ok:false,message:`${c.name}: faltam ${c.days-p.growth} dias de rega.`};s.inventory[p.crop]++;s.totalHarvest++;p.crop=null;p.growth=0;}
  else return {ok:false,message:'Selecione uma ferramenta para cultivar.'};
  s.energy-=3;return {ok:true,message:['Terra preparada. Um novo começo!','Semente plantada. Agora é só regar!','Água fresquinha! Sua planta agradece.','Colheita guardada! Venda para Lia na vila.'][tool]};
}
export function buySeeds(s,i){const c=CROPS[i];if(!c||s.coins<c.price)return false;s.coins-=c.price;s.seeds[i]+=5;return true;}
export function sellAll(s){let value=s.fish*18;s.fish=0;s.inventory.forEach((n,i)=>{value+=n*CROPS[i].sell;s.inventory[i]=0;});s.coins+=value;return value;}
export function talk(s,id){if(!NPCS.some(n=>n.id===id))return false;if(!s.met.includes(id))s.met.push(id);if(s.talked[id]!==s.day){s.talked[id]=s.day;s.friends[id]=Math.min(5,s.friends[id]+1);}if(id==='iara')s.rod=true;return true;}
export function gift(s,id){if(!NPCS.some(n=>n.id===id)||s.gifted[id]===s.day)return false;const i=s.inventory.findIndex(n=>n>0);if(i<0)return false;s.inventory[i]--;s.gifted[id]=s.day;s.friends[id]=Math.min(5,s.friends[id]+1);return true;}
export function upgrade(s){if(s.wateringLevel===2||s.wood<8||s.coins<60)return false;s.wood-=8;s.coins-=60;s.wateringLevel=2;return true;}
export function quests(s){return [
 {id:'meet',name:'Rostos do vale',description:'Conheça os quatro moradores.',progress:s.met.length,target:4,reward:40},
 {id:'harvest',name:'O fruto do trabalho',description:'Colha 5 vegetais na sua fazenda.',progress:s.totalHarvest,target:5,reward:60},
 {id:'delivery',name:'A pesquisa de Theo',description:'Entregue 3 cenouras para Theo.',progress:s.inventory[0],target:3,reward:75}
];}
export function claim(s,id){const q=quests(s).find(q=>q.id===id);if(!q||s.claimed.includes(id)||q.progress<q.target)return false;if(id==='delivery')s.inventory[0]-=3;s.claimed.push(id);s.coins+=q.reward;return true;}
