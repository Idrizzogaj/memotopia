# Turns the Zogaj Ad Studio page into the Memotopia Ad Studio (3 presets, Memotopia art and colours)
import json, sys
src, assets, out = sys.argv[1:4]
L = open(src, encoding='utf8').read().split('\n')
A = json.load(open(assets))
PRE = [
 dict(name='Train your brain', h=['TRAIN YOUR', 'BRAIN', 'FOR FREE!'], top='MEET MEMOTOPIA', bottom='BRAIN TRAINING', era='20 LEVELS IN EVERY GAME', sub='PAIRS · BOXES · FLASH · CHAIN · SAYS', p=[(5, 'YOUR TRAINING', 'TRAIN!'), (0, '5 MEMORY GAMES', 'PLAY!'), (4, '20 LEVELS', 'LEVEL UP!'), (7, 'YOUR PROGRESS', 'STARS + XP')]),
 dict(name='Focus', h=['90 SECONDS', 'OF PURE', 'FOCUS!'], top='NEW IN MEMOTOPIA', bottom='FOCUS TRAINING', era='MEMORY + FOCUS IN ONE APP', sub='BOXES · CHAIN · SAYS · THE DECK', p=[(1, 'FOCUS · BOXES', 'WATCH THE WALLS!'), (2, 'FOCUS · CHAIN', 'GREEN OR RED?'), (3, 'SAYS', 'REPEAT IT!'), (8, 'THE DECK', 'ALL 52 CARDS')]),
 dict(name='Challenge friends', h=['CHALLENGE', 'YOUR', 'FRIENDS!'], top='MEET MEMOTOPIA', bottom='BEAT YOUR FRIENDS', era='TRAIN · CHALLENGE · RANK UP', sub='ARENA · STARS · XP', p=[(6, 'THE ARENA', 'PICK A PLAYER!'), (5, 'CHALLENGE', 'BEAT THEM!'), (4, '20 LEVELS', 'CLIMB!'), (7, 'RANK UP', 'STARS + XP')]),
]
i = next(k for k, l in enumerate(L) if l.startswith('var DEFAULTS='))
L[i] = ('var DEFAULTS=' + json.dumps({'hero': A['hero'], 'icon': A['icon'], 'panels': A['panels']})
        + ';var PRESETS=' + json.dumps(PRE)
        + ';DEFAULTS.panels4=[0,1,2,3].map(function(k){return DEFAULTS.panels[PRESETS[0].p[k][0]]});')
h = '\n'.join(L)

def rep(a, b, n=1):
    global h
    assert h.count(a) >= 1, a[:70]
    h = h.replace(a, b) if n == 0 else h.replace(a, b, n)

rep('<title>Zogaj Ad Studio</title>', '<title>Memotopia Ad Studio</title>')
rep('<h1>Zogaj Ad Studio</h1>', '<h1>Memotopia Ad Studio</h1>')
rep('The example is Captain Zogaj.', 'Three ready ads in Memotopia style: pick one, preview, render.')
rep('family=Barlow+Condensed:wght@800;900&family=IBM+Plex+Sans:wght@400;500;600&family=IBM+Plex+Mono:wght@500',
    'family=Montserrat:wght@600;800;900&family=IBM+Plex+Sans:wght@400;500;600')
rep('--display:"Barlow Condensed",Impact,"Arial Narrow",sans-serif;--body:"IBM Plex Sans",system-ui,sans-serif;--mono:"IBM Plex Mono",ui-monospace,Menlo,Consolas,monospace;',
    '--display:"Montserrat",system-ui,sans-serif;--body:"IBM Plex Sans",system-ui,sans-serif;--mono:"Montserrat",system-ui,sans-serif;')
rep("""var DISPLAY='"Barlow Condensed",Impact,"Arial Narrow",sans-serif',MONO='"IBM Plex Mono",ui-monospace,Menlo,Consolas,monospace';""",
    """var DISPLAY='"Montserrat",system-ui,sans-serif',MONO='"Montserrat",system-ui,sans-serif';""")
rep("(font==='M'?'500 ':'900 ')", "(font==='M'?'600 ':'800 ')")
rep("g.font='900 '+size+'px '+DISPLAY;var w=", "g.font='800 '+size+'px '+DISPLAY;var w=")
for a, b in [('value="BLAST OFF"', 'value="TRAIN YOUR"'), ('value="INTO THE"', 'value="BRAIN"'), ('value="1970s!"', 'value="FOR FREE!"'),
 ('value="MEET CAPTAIN ZOGAJ"', 'value="MEET MEMOTOPIA"'), ('value="SPACE ADVENTURE"', 'value="BRAIN TRAINING"'),
 ('value="SPACE + 4 MORE ERAS"', 'value="20 LEVELS IN EVERY GAME"'), ('value="3 WORLDS · BOSSES · FLIGHTS"', 'value="PAIRS · BOXES · FLASH · CHAIN · SAYS"'),
 ('value="CAPTAIN ZOGAJ’S"', 'value="ZOGAJ"'), ('value="EXTREME CHALLENGE"', 'value="MEMOTOPIA"'),
 ('value="1940s free · 1970s via in-app purchase"', 'value="Free to play · by Idriz Zogaj"'),
 ('id="c0" value="#123a22"', 'id="c0" value="#8a2bff"'), ('id="c1" value="#04120b"', 'id="c1" value="#35108f"'),
 ('id="c2" value="#b8e6a8"', 'id="c2" value="#ffffff"'), ('id="c3" value="#f2cf6a"', 'id="c3" value="#ffc247"'),
 ('https://apps.apple.com/app/id6822024941', 'https://www.memotopia.com'),
 ('https://play.google.com/store/apps/details?id=se.zogaj.extremechallenge', 'https://play.google.com/store/apps/details?id=com.zogajretention.memotopia'),
 ('<option value="heroic">Heroic (A minor to C major)</option><option value="bright">Bright (C major)</option>',
  '<option value="bright">Bright (C major)</option><option value="heroic">Heroic (A minor to C major)</option>'),
 ('Boss badge<input', 'Accent<input')]:
    rep(a, b)
rep('<fieldset><legend>Words</legend>',
    '<fieldset><legend>The three ads</legend><div class="bar" id="presets"></div><p class="note" style="margin-top:10px">Each button loads words and screens for one ad. Change anything afterwards if you like. The Apple link is the website until the iPhone app is public.</p></fieldset>\n   <fieldset><legend>Words</legend>')
rep("var PANEL_DEFAULTS=[{label:'1930s · JUNGLE',cmd:'SWING!',boss:false},{label:'1970s · SPACE',cmd:'BLAST!',boss:false},{label:'1970s · BOSS',cmd:'BOSS FIGHT!',boss:true},{label:'1960s · BOSS',cmd:'BEAT THE BOSS!',boss:true}];",
    "var PANEL_DEFAULTS=PRESETS[0].p.map(function(q){return {label:q[1],cmd:q[2],boss:false}});")
rep("""src="'+DEFAULTS.panels[i]+'\"""", """src="'+DEFAULTS.panels4[i]+'\"""")
rep("g.fillStyle='rgba(0,0,0,.14)';for(var y=0;y<1920;y+=5)g.fillRect(0,y,1080,2);", "")
rep("v.addColorStop(1,'rgba(0,0,0,.5)')", "v.addColorStop(1,'rgba(20,0,60,.35)')")
rep("g.fillStyle=r;g.fillRect(0,0,1080,1920)}",
    "g.fillStyle=r;g.fillRect(0,0,1080,1920);var T=(window.__t||0);g.save();g.globalAlpha=.10;g.fillStyle='#fff';[[140,300,90],[930,520,130],[200,1100,110],[880,1500,80],[540,1750,100]].forEach(function(q,k){g.save();g.translate(q[0],q[1]+Math.sin(T*1.3+k)*18);g.beginPath();for(var j=0;j<6;j++){var a=Math.PI/3*j+Math.PI/6;g.lineTo(Math.cos(a)*q[2],Math.sin(a)*q[2])}g.closePath();g.fill();g.restore()});g.restore()}")
rep("function frame(t,C){", "function frame(t,C){window.__t=t;")
rep("""document.fonts.load('900 100px "Barlow Condensed"'),document.fonts.load('500 30px "IBM Plex Mono"')""",
    """document.fonts.load('800 100px "Montserrat"'),document.fonts.load('600 30px "Montserrat"')""")
rep("IM.hero=r[0];IM.icon=r[1];IM.panels=r.slice(2);draw()", "IM.hero=r[0];IM.icon=r[1];IM.all=r.slice(2);applyPreset(0)")
rep("'ad-15s.mp4'", "(window.__fn||'ad-15s.mp4')", 0)
PJS = r"""
/* ---------- presets ---------- */
function applyPreset(n){var P=PRESETS[n];stopPlay();
 ['h1','h2','h3'].forEach(function(id,k){$(id).value=P.h[k]});$('top').value=P.top;$('bottom').value=P.bottom;$('era').value=P.era;$('subl').value=P.sub;
 for(var k=0;k<4;k++){IM.panels[k]=IM.all[P.p[k][0]];$('th'+k).src=IM.panels[k].src;$('pl'+k).value=P.p[k][1];$('pc'+k).value=P.p[k][2];$('pb'+k).checked=false}
 window.__fn='memotopia-ad-'+(n+1)+'-'+P.name.toLowerCase().replace(/[^a-z]+/g,'-')+'.mp4';
 document.querySelectorAll('#presets button').forEach(function(b,k){b.className=k===n?'primary':''});
 scrub.value=3.5;draw()}
var pb=$('presets');PRESETS.forEach(function(P,n){var b=document.createElement('button');b.type='button';b.textContent=(n+1)+'. '+P.name;b.addEventListener('click',function(){applyPreset(n)});pb.appendChild(b)});
"""
rep("/* ---------- start ---------- */", PJS + "\n/* ---------- start ---------- */")
open(out, 'w', encoding='utf8').write(h)
print(len(h))
