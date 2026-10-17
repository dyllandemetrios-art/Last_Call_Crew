from pathlib import Path
import math, json, html
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[1]
W,H=1800,1120
im=Image.new('RGB',(W,H),'#101c2b'); d=ImageDraw.Draw(im)
svg=[f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}">',f'<rect width="{W}" height="{H}" fill="#101c2b"/>']
def rect(b,fill,outline=None,width=1):
    d.rectangle(b,fill,outline,width)
    x,y,x2,y2=b; svg.append(f'<rect x="{x}" y="{y}" width="{x2-x}" height="{y2-y}" fill="{fill or "none"}" stroke="{outline or "none"}" stroke-width="{width}"/>')
def line(ps,c,w=2,dashed=False):
    if dashed:
        for a,b in zip(ps,ps[1:]):
            dist=math.dist(a,b)
            for t in range(0,int(dist),14):
                p=(a[0]+(b[0]-a[0])*t/dist,a[1]+(b[1]-a[1])*t/dist)
                q=(a[0]+(b[0]-a[0])*min(t+8,dist)/dist,a[1]+(b[1]-a[1])*min(t+8,dist)/dist)
                d.line([p,q],fill=c,width=w)
    else:d.line(ps,fill=c,width=w,joint='curve')
    pts=' '.join(f'{x},{y}' for x,y in ps)
    dash='stroke-dasharray="8 6"' if dashed else ''
    svg.append(f'<polyline points="{pts}" fill="none" stroke="{c}" stroke-width="{w}" stroke-linejoin="round" {dash}/>')
def text(x,y,s,size=20,c='#edf3fa',bold=False,anchor='la'):
    f=ImageFont.truetype('C:/Windows/Fonts/arialbd.ttf' if bold else 'C:/Windows/Fonts/arial.ttf',size)
    d.text((x,y),s,font=f,fill=c,anchor=anchor)
    align='middle' if anchor.startswith('m') else 'start'
    svg.append(f'<text x="{x}" y="{y+size*.82}" fill="{c}" font-size="{size}" font-family="Arial, sans-serif" font-weight="{700 if bold else 400}" text-anchor="{align}">{html.escape(s)}</text>')
def xy(x,z):return (50+(x+170)*6,180+(105-z)*6)
def wr(x,z,w,h,c,o=None):
    a=xy(x,z+h);b=xy(x+w,z);rect((*a,*b),c,o,2)
def wl(ps,c,w=2,dash=False):line([xy(*p) for p in ps],c,w,dash)
def wt(x,z,s,size=18,c='#edf3fa',bold=False,center=False):
    a,b=xy(x,z);text(a,b,s,size,c,bold,'ma' if center else 'la')
def point(x,z,label,color):
    a,b=xy(x,z);d.ellipse((a-14,b-14,a+14,b+14),fill=color,outline='#101c2b',width=2)
    svg.append(f'<circle cx="{a}" cy="{b}" r="14" fill="{color}" stroke="#101c2b" stroke-width="2"/>')
    text(a,b-10,label,17,'#101c2b',True,'ma')
def reserve(x,z,w,h,title,sub,title_size=17,sub_size=14):
    wr(x,z,w,h,'#433d52','#a991c9')
    for off in range(0,int(w*6+h*6),14):
        # clipped diagonal hatch, fully within the rectangular lot
        x0,y0=xy(x,z+h);x1,y1=xy(x+w,z)
        ax=x0+max(0,off-h*6); ay=y1-min(off,h*6)
        bx=x0+min(off,w*6); by=y1-max(0,off-w*6)
        line([(ax,ay),(bx,by)],'#61516e',1)
    a,b=xy(x+w/2,z+h/2+3);text(a,b,title,title_size,'#f0dfff',True,'ma');text(a,b+20,sub,sub_size,'#eee1fa',False,'ma')

text(50,28,'RENTRER VIVANT',20,'#75ddc6',True)
text(50,60,'Les Deux-Lanternes',42,bold=True)
text(50,116,'Révision 4 · petits bars thématiques · proposition de tracé, pas un relevé de la scène',22,'#afc2d6')
text(1430,42,'ÉTUDE ENVIRONNEMENT',17,'#afc2d6',True)
text(1430,70,'17 octobre 2026',20)
text(1430,103,'P1 visé : 30 novembre',18,'#afc2d6')
rect((50,180,1370,960),'#233342','#53697b',2)
# District backgrounds, 10 m construction grid.
wr(-170,5,105,91,'#302e40');wr(-65,20,55,65,'#263d3c');wr(-10,5,60,90,'#2b3d43')
for x in range(-170,51,10):wl([(x,-25),(x,105)],'#344652',1)
for z in range(-25,106,10):wl([(-170,z),(50,z)],'#344652',1)
wr(-170,97,220,8,'#264e69');wl([(-170,96),(50,96)],'#84a8b8',3)
wt(-59,103,'CANAL DÉCORATIF · rive fermée par garde-corps',16,'#bcdcea',center=True)
wt(-147,95,'QUARTIER NOCTURNE',17,'#d7c4ec',True)
wt(-58,92,'LIAISON PIÉTONNE',17,'#a0d7c3',True)
wt(1,92,'RÉSIDENTIEL',17,'#bbd6d9',True)
# Road with footways: 6 m carriageway + two 3 m footways.
taxi=[(0,16),(0,-4),(-130,-4),(-130,16)]
wl(taxi,'#899398',72);wl(taxi,'#495660',36);wl(taxi,'#f0bc58',4,True)
wr(4,8,6,8,'#6b6147','#caa45c');wr(-142,8,6,8,'#6b6147','#caa45c')
# Main 6 m path, then 8 m nightlife streets, 4 m shortcut / alleys.
main=[(15,16),(0,16),(0,40),(-70,40),(-70,60),(-110,60),(-110,30),(-125.75,30),(-125.75,36)]
short=[(-70,40),(-110,40),(-110,30)]
alley=[(-110,60),(-110,86),(-80,86),(-80,60),(-70,60)]
wl(main[:-1],'#92a9a7',36);wl(main[4:-1],'#92a9a7',48);wl(main[-2:],'#92a9a7',18)
wl(short,'#88909e',24);wl(alley,'#88909e',24)
soif=[(-110,56),(-146,56),(-146,86)]
champignons_meme=[(-146,30),(-146,56)]
bonbons_calins=[(-146,86),(-110,86)]
fumee_dragon=[(-146,66),(-162,66)]
tisane_morphee=[(-80,68),(-64,68),(-64,80)]
wl(soif,'#a8a19b',48)
for ps in [champignons_meme,bonbons_calins,fumee_dragon,tisane_morphee]:wl(ps,'#8995a2',24)
for x,z in [(-162,66),(-64,80)]:
    wr(x-3,z-3,6,6,'#8995a2')
    # Three-sided end court; the approach remains open.
    if x==-162:wl([(x+3,z+3),(x-3,z+3),(x-3,z-3),(x+3,z-3)],'#d3a3a1',4)
    else:wl([(x-3,z-3),(x-3,z+3),(x+3,z+3),(x+3,z-3)],'#d3a3a1',4)
wl([(-150,68),(-159,68),(-159,69)],'#d3a3a1',3)
wl([(-150,64),(-159,64),(-159,63)],'#d3a3a1',3)
wl([(-78,70),(-66,70),(-66,77),(-67,77)],'#d3a3a1',3)
wl([(-78,66),(-62,66),(-62,77),(-61,77)],'#d3a3a1',3)
wr(-146,16,32,20,'#a4a5a1')
wl([(-130,16),(-130,36)],'#ced3c9',24)
# Lot masses guide movement, all nonplayable except the two rooms.
for x,z,w,h in [(9,31,20,14),(10,56,20,16),(33,30,12,20),(-56,57,20,18),(-32,57,20,18),(-62,16,40,20),(-103,50,26,5),(-165,40,12,20)]:
    wr(x,z,w,h,'#40515c','#667984')
reserve(-164,72,12,20,'X1','Boîte')
reserve(-135,64,10,12,'B3','Cocktails')
reserve(-104,74,16,8,'X3','Vendeurs')
reserve(-165,20,14,14,'X4','Stands')
# The former 17 × 16 m shell becomes two narrow 8.5 × 16 m venues.
reserve(-138.5,36,8.5,16,'B2','Shooters',14,10)
wr(-130,36,8.5,16,'#b2764b','#ffd49e')
wr(-129.5,48,5.5,2,'#5d4238');wr(-129.5,39,2.5,3,'#795344');wr(-124,43,2,3,'#795344')
wl([(-127.25,36),(-124.25,36)],'#f8e5b4',8)
wt(-125.75,49,'B1',16,'#fff3db',True,True);wt(-125.75,45,'Bières · P1',11,'#fff3db',center=True)
wl([(-121.5,44),(-110,44)],'#92a9a7',18)
wl([(-121.5,42.5),(-121.5,45.5)],'#b1e7ee',7)
wt(-119,49,'Sortie',13,'#b1e7ee')
# Keep the existing home anchor and west entrance.
wr(15,11,10,10,'#418a73','#a8f0c1');wl([(15,14.5),(15,17.5)],'#d9ffb2',8)
point(20,16,'1','#c2f2c5')
wt(15,7,'COLOC',19,'#bdedc9',True);wt(15,3,'10 × 10 m',16,'#c7ded5')
wl(main,'#5ce1c0',5,True);wl(short,'#d4b2fc',5,True)
point(0,16,'A','#f0bc58');point(-130,16,'B','#f0bc58')
point(-110,60,'2','#99d9ec');point(-125.75,36,'3','#ffd49e')
wt(-143,29,'PLACE',18,'#253740',True);wt(-143,25,'32 × 20 m',15,'#253740')
wt(-62,35,'Promenade des Ateliers',18,'#d3eee3',True)
wt(-65,29,'70 m · poches futures hors passage',16,'#b8d1cc')
wt(-105,29,'P · Ruelle de la Poudre de Perlimpinpin',12,'#dec6fd')
wt(-159,51,'Façades',13,'#c2cbd4',center=True)
wt(-25,72,'Ateliers fermés',16,'#b8c8d1',center=True)
wt(5,79,'Maisons non visitables',16,'#c5d6db')
wt(-93,-8,'ROUTE DU TAXI · 170 m sur l’axe · cible 12 s à tester',18,'#ffe0a4',True)
wt(-91,-15,'Trottoirs continus : retour à pied toujours possible',16,'#c5d0dc')
# Future encounters: two empty lay-bys off the clear path.
wr(-55,42,8,6,'#487265','#9cbfaf');wr(-24,42,8,6,'#487265','#9cbfaf')
wt(-54,47,'E1',17,'#d1f0de',True);wt(-23,47,'E2',17,'#d1f0de',True)
for x,z,label in [(-104,32,'R1'),(-94,42,'R2'),(-82,32,'R3')]:
    wr(x,z,6,6,'#705c80','#dfb6f5')
    wt(x+3,z+5,label,15,'#fff0ff',True,True)
wt(-142,58,'S · RUE DE LA SOIF',16,'#172b36',True)
for x,z,label,c in [(-146,77,'S','#f8ce9b'),(-146,42,'M','#b7d4f0'),(-162,66,'D','#e6b7bb'),(-64,80,'T','#e6b7bb'),(-120,86,'B','#b7d4f0')]:
    point(x,z,label,c)
# Closed future venue thresholds face the Rue de la Soif.
wl([(-152,78.5),(-152,81.5)],'#e9bce7',6)
wl([(-131.5,64),(-128.5,64)],'#e9bce7',6)
wl([(-135.75,36),(-132.75,36)],'#e9bce7',6)
# Frontage breaks and limits never depend on a colliderless ground edge.
wl([(-170,96),(-170,-25),(50,-25),(50,96)],'#9fadb5',4)
wt(35,-15,'N ↑',23,'#d6e3ed',True)
# Sidebar.
text(1420,186,'RUES & IMPASSES',20,'#75ddc6',True)
for i,(h,s) in enumerate([
    ('S  Rue de la Soif','Entre B1/B2, B3 et boîte X1.'),
    ('P  Ruelle de la Poudre de Perlimpinpin','Raccourci ; trois poches R1–R3.'),
    ('M  Passage des Champignons de Mémé','Nouvelle liaison place ↔ rue S.'),
    ('B  Ruelle des Bonbons Câlins','Boucle nord et liaison vers X1.'),
    ('D  Impasse de la Fumée de Dragon','16 m ; cour de retournement.'),
    ('T  Impasse de la Tisane de Morphée','28 m ; fond visible après le coude.')]):
    y=225+i*72;text(1420,y,h,16,bold=True);text(1420,y+26,s,16,'#b6c9d9')
text(1420,672,'Noms tirés du GDD.',17,'#dfb6f5')
text(1420,698,'Affectation aux voies modifiable.',17,'#b6c9d9')
text(1420,744,'REPÈRES & RENCONTRES',19,'#75ddc6',True)
text(1420,776,'1 Coloc · 2 Carrefour · 3 Bières P1',17,'#b6c9d9')
text(1420,805,'B2 Shooters · B3 Cocktails : fermés.',16,'#b6c9d9')
text(1420,832,'X1 boîte · X3 vendeurs · X4 stands.',16,'#b6c9d9')
text(1420,874,'Ville ouverte hors obstacles visibles.',17,'#b6c9d9')
text(1420,904,'Tracés mesurés : 221 m / 181 m.',17,'#b6c9d9')
text(1420,931,'Diagonales libres : temps à rejouer.',17,'#b6c9d9')
# Footer and actual scale.
line([(50,997),(170,997)],'#e7edf4',4);line([(50,989),(50,1005)],'#e7edf4',2);line([(170,989),(170,1005)],'#e7edf4',2)
text(50,1011,'0',16);text(170,1011,'20 m',16,anchor='ma')
text(225,985,'Emprise : 220 × 130 m · grille : 10 m · sol de jeu plat',20,bold=True)
text(225,1017,'Trajets par l’entrée principale. Retour par la sortie latérale : 164,5 m / 33 s seul.',18,'#b6c9d9')
text(50,1070,'AJOUTS DE DESIGN · Trois bars compacts et thématiques. Impasses signalées, greybox prioritaire.',19,'#b6c9d9')
svg.append('</svg>')
(ROOT/'Carte_P1.svg').write_text('\n'.join(svg),encoding='utf-8');im.save(ROOT/'Carte_P1.png')
def length(ps):return sum(math.dist(a,b) for a,b in zip(ps,ps[1:]))
routes={'promenade':main,'ruelle':main[:4]+[(-110,40),(-110,30),(-125.75,30),(-125.75,36)],'sortie_laterale':[(-121.5,44),(-110,44),(-110,40),(-70,40),(0,40),(0,16),(15,16)],'taxi_axe':taxi,'trottoir_interieur':[(15,16),(-4.5,16),(-4.5,0.5),(-125.5,0.5),(-125.5,16),(-125.75,16),(-125.75,36)],'rue_de_la_soif':soif,'passage_champignons_meme':champignons_meme,'ruelle_bonbons_calins':bonbons_calins,'impasse_fumee_dragon':fumee_dragon,'impasse_tisane_morphee':tisane_morphee}
data={n:{'points_xz':ps,'metres':length(ps),'secondes_a_5m_s':length(ps)/5,'secondes_a_3m_s':length(ps)/3} for n,ps in routes.items()}
(ROOT/'preuves'/'trajets.json').write_text(json.dumps(data,indent=2),encoding='utf-8')
(ROOT/'preuves'/'calculs_trajets.txt').write_text(json.dumps(data,indent=2,ensure_ascii=False)+'\n',encoding='utf-8')
print(json.dumps(data,indent=2))
