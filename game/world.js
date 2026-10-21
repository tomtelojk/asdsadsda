export const TILE=32, WIDTH=1120, HEIGHT=704;
export const buildings={
 farm:[{x:2,y:2,w:7,h:5,name:'Sua casa',type:'home',roof:'#b97958',wall:'#e0c592'}],
 village:[{x:4,y:3,w:7,h:5,name:'Armazém da Lia',type:'shop',roof:'#bb785a',wall:'#e6cb99'},{x:23,y:3,w:7,h:5,name:'Oficina do Bento',type:'workshop',roof:'#728b9a',wall:'#c8b795'},{x:14,y:2,w:6,h:5,name:'Casa das flores',type:'flowers',roof:'#a88a9b',wall:'#ead7b2'}]
};
export const trees={farm:[[1,3],[11,2],[14,2],[17,2],[20,2],[23,2],[27,2],[31,3],[33,5],[2,11],[10,10],[2,18],[6,19],[10,18],[32,19],[22,20]],village:[[1,4],[2,2],[12,2],[22,2],[32,3],[33,7],[2,12],[4,18],[8,20],[19,20],[33,20],[21,7]]};
export const resources=[{id:'wood-a',scene:'farm',x:10,y:15,type:'wood'},{id:'wood-b',scene:'farm',x:7,y:13,type:'wood'},{id:'wood-c',scene:'farm',x:16,y:18,type:'wood'},{id:'wood-d',scene:'village',x:21,y:18,type:'wood'},{id:'stone-a',scene:'farm',x:15,y:15,type:'stone'},{id:'stone-b',scene:'farm',x:30,y:12,type:'stone'}];
export function pond(scene){return scene==='farm'?{x:26,y:16,w:6,h:4}:{x:25,y:16,w:7,h:4};}
export function plotPosition(index){return {x:17*32+index%8*32,y:8*32+Math.floor(index/8)*32};}
export function plotAt(x,y){const c=Math.floor(x/32)-17,r=Math.floor(y/32)-8;return c>=0&&c<8&&r>=0&&r<6?r*8+c:-1;}
export function blocked(scene,x,y){if(x<16||x>1104||y<32||y>680)return true;const tx=Math.floor(x/32),ty=Math.floor(y/32);for(const b of buildings[scene])if(tx>=b.x&&tx<b.x+b.w&&ty>=b.y+2&&ty<b.y+b.h)return true;if(scene==='village'&&tx>=15&&tx<=18&&ty>=14&&ty<=15)return true;const p=pond(scene);if(tx>=p.x&&tx<p.x+p.w&&ty>=p.y&&ty<p.y+p.h)return true;for(const [a,b]of trees[scene])if(Math.abs(x-(a*32+16))<14&&Math.abs(y-(b*32+20))<13)return true;return false;}
// A* on the same collision grid used by keyboard movement.
export function findPath(scene,start,target){const sx=Math.floor(start.x/32),sy=Math.floor(start.y/32),gx=Math.floor(target.x/32),gy=Math.floor(target.y/32);if(blocked(scene,gx*32+16,gy*32+16))return [];
 const key=(x,y)=>x+','+y,open=[{x:sx,y:sy,g:0,f:0}],best=new Map([[key(sx,sy),0]]),parents=new Map();let iterations=0;
 while(open.length&&iterations++<1200){open.sort((a,b)=>a.f-b.f);const n=open.shift(),k=key(n.x,n.y);if(n.x===gx&&n.y===gy){const path=[];let cur=k;while(cur!==key(sx,sy)){const [x,y]=cur.split(',').map(Number);path.unshift({x:x*32+16,y:y*32+16});cur=parents.get(cur);if(!cur)return [];}return path;}
 for(const [dx,dy]of [[1,0],[-1,0],[0,1],[0,-1]]){const x=n.x+dx,y=n.y+dy,nk=key(x,y),g=n.g+1;if(blocked(scene,x*32+16,y*32+16)||g>=(best.get(nk)??Infinity))continue;best.set(nk,g);parents.set(nk,k);open.push({x,y,g,f:g+Math.abs(x-gx)+Math.abs(y-gy)});}}
 return [];
}
