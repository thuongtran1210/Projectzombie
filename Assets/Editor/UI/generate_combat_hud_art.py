"""Deterministic vector-drawn HUD plates and glyphs. No source images are modified."""
from PIL import Image, ImageDraw
from pathlib import Path
import math

OUT = Path(__file__).resolve().parents[2] / 'Art/UI/CombatHUD'
OUT.mkdir(parents=True, exist_ok=True)
S = 768
BRONZE = '#C9A86A'
IVORY = '#EFE5C8'

def canvas():
    im=Image.new('RGBA',(S,S)); return im,ImageDraw.Draw(im)

def save(im,name):
    im.resize((384,384),Image.Resampling.LANCZOS).save(OUT/(name+'.png'))

for name,accent in [('Attack','#D9AB63'),('Signature','#80C9DC'),('Relic','#7FB9A4'),('Dash','#9CADBB'),('Command','#A69875')]:
    im,d=canvas()
    d.ellipse((27,37,741,751),fill=(8,12,15,130))
    d.ellipse((27,23,741,737),fill='#332B25',outline='#746047',width=8)
    d.arc((32,28,736,732),190,350,fill='#E0C48C',width=6)
    d.ellipse((43,39,725,721),fill='#151C23',outline=BRONZE,width=8 if name=='Attack' else 5)
    d.ellipse((62,58,706,702),outline='#4B493E',width=3)
    d.arc((68,64,700,696),205,335,fill=accent,width=5)
    d.arc((68,64,700,696),25,155,fill=accent,width=3)
    for angle in (0,90,180,270):
        a=math.radians(angle);x=384+math.cos(a)*344;y=380+math.sin(a)*344
        d.polygon([(x,y-9),(x+9,y),(x,y+9),(x-9,y)],fill=BRONZE)
    if name=='Attack':
        for side in (-1,1):
            for k in range(3):
                a=math.radians(90+side*(22+k*9));r=325
                x=384+math.cos(a)*r;y=380+math.sin(a)*r
                d.line((x-5,y-5,x+5,y+5),fill=BRONZE,width=4)
    save(im,'Plate_'+name)

im,d=canvas();d.ellipse((38,38,730,730),outline='white',width=10);save(im,'Ring')
im,d=canvas();d.ellipse((0,0,768,768),fill='white');save(im,'Disc')

# A brush-like pair of attack strokes remains legible at small sizes.
im,d=canvas()
d.polygon([(129,573),(220,401),(456,192),(644,114),(506,242),(302,467),(267,575),(218,533)],fill='#D5AB69')
d.polygon([(158,535),(257,379),(476,194),(592,151),(431,305),(264,479)],fill=IVORY)
d.polygon([(330,628),(433,427),(609,289),(529,429),(437,588)],fill='#C99D5F')
d.polygon([(359,586),(465,420),(568,337),(481,475),(421,577)],fill=IVORY)
save(im,'Glyph_Attack')

im,d=canvas()
d.polygon([(439,100),(215,409),(356,409),(302,667),(564,316),(411,316),(500,100)],fill='#6CAEBF')
d.polygon([(414,108),(237,389),(381,389),(333,592),(523,338),(385,338),(474,108)],fill='#DCF2E9')
save(im,'Glyph_Thunder')

im,d=canvas()
d.polygon([(184,590),(263,435),(453,194),(510,177),(553,210),(538,261),(344,484)],fill='#C7AD7E')
d.polygon([(184,590),(263,435),(316,469),(247,559)],fill='#E8E0C3')
d.polygon([(276,431),(460,207),(501,218),(314,461)],fill='#87BEC6')
d.arc((150,284,598,640),15,158,fill='#E8E0C3',width=30)
d.arc((187,321,559,604),18,153,fill='#87BEC6',width=10)
save(im,'Glyph_Scholar')

im,d=canvas()
for box,width,end in [((143,209,622,539),29,333),((116,280,547,621),19,322),((184,148,654,476),12,308)]:
    d.arc(box,155,end,fill=IVORY if width==29 else '#93AFBC',width=width)
d.polygon([(610,272),(629,405),(503,374)],fill=IVORY)
save(im,'Glyph_Dash')

im,d=canvas()
d.rounded_rectangle((244,125,501,643),radius=122,fill='#8DBEAF',outline='#E4E8CE',width=16)
d.rounded_rectangle((266,147,479,617),radius=105,fill='#273D3A')
d.line((282,312,371,386,459,312),fill=IVORY,width=30)
d.line((371,386,371,232),fill=IVORY,width=26)
for y in range(444,577,40):d.line((298,y,447,y),fill='#719C8E',width=9)
save(im,'Glyph_Slipper')

im,d=canvas()
d.line((160,572,444,341),fill='#94BAAC',width=49)
d.line((157,559,434,334),fill=IVORY,width=20)
d.polygon([(421,267),(566,267),(551,427),(452,427)],fill='#9BBAA2',outline=IVORY,width=12)
d.ellipse((417,244,569,294),fill='#253A37',outline=IVORY,width=14)
d.arc((431,114,575,253),60,270,fill='#B8D2C3',width=13)
d.arc((477,65,620,209),90,280,fill='#789F99',width=10)
save(im,'Glyph_Pipe')

im=Image.new('RGBA',(768,256));d=ImageDraw.Draw(im)
d.rounded_rectangle((4,4,764,252),radius=40,fill=(17,22,26,155),outline=(201,168,106,95),width=3)
im.resize((384,128),Image.Resampling.LANCZOS).save(OUT/'Dock.png')

im,d=canvas();d.rounded_rectangle((216,340,552,614),radius=34,fill=IVORY)
d.arc((263,154,505,461),180,360,fill=IVORY,width=42);save(im,'Glyph_Lock')
print('Generated',len(list(OUT.glob('*.png'))),'HUD sprites in',OUT)
