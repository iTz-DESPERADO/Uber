"""Render the README motion-graphics film. Requires Pillow, numpy, imageio-ffmpeg.

Run from the repository root: python tools/render_demo.py
"""
from pathlib import Path
import math
import os
import subprocess
import tempfile

import imageio_ffmpeg
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'assets' / 'demo'
OUT.mkdir(parents=True, exist_ok=True)
W, H, FPS, SEG = 1280, 720, 24, 8
DURATION = SEG * 7
FONT = Path(os.environ.get('WINDIR', 'C:/Windows')) / 'Fonts'
regular = FONT / 'segoeui.ttf'
bold = FONT / 'segoeuib.ttf'
if not regular.exists():
    regular = Path('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf')
    bold = regular.with_name('DejaVuSans-Bold.ttf')
fonts = {}
WHITE, MUTED, CYAN, BLUE = '#f1f7ff', '#9fb8d2', '#64dddb', '#5aabff'
yy, xx = np.mgrid[0:H, 0:W]
glow = np.exp(-(((xx-1050)/700)**2+((yy-120)/650)**2))
background = np.stack([9+10*glow, 19+30*glow, 35+48*glow], axis=-1).astype('uint8')
BASE = Image.fromarray(background)

def text(d, xy, value, size=24, color=WHITE, heavy=False, anchor=None):
    key = (size, heavy)
    if key not in fonts:
        fonts[key] = ImageFont.truetype(str(bold if heavy else regular), size)
    d.text(xy, value, font=fonts[key], fill=color, anchor=anchor)

def card(d, box, fill='#142b43', outline='#28465f', radius=22):
    d.rounded_rectangle(box, radius, fill=fill, outline=outline, width=2)

def ease(x):
    x = max(0, min(1, x))
    return x*x*(3-2*x)

def line_progress(d, points, progress, color=CYAN, width=6):
    lengths = [math.dist(a,b) for a,b in zip(points,points[1:])]
    left = sum(lengths)*max(0,min(1,progress))
    for a,b,length in zip(points, points[1:], lengths):
        f = min(1,left/length)
        if f <= 0: break
        end = (a[0]+(b[0]-a[0])*f,a[1]+(b[1]-a[1])*f)
        d.line([a,end],fill=color,width=width)
        left -= length
    return end if progress > 0 else points[0]

TITLES = ['Every journey, connected.', 'A nearby driver. A clear next step.',
          'One trip. Live at every step.', 'Complete. Settle. Rate.',
          'Built around the business.', 'Consistency at the core.', 'Built together. Refined for the future.']
SUBS = ['Real-time ride-hailing, rebuilt with Clean Architecture.',
        'Select a route, receive a quote, and notify an available driver.',
        'SignalR updates keep rider and driver in sync.',
        'Wallet settlement follows completion. Ratings stay in the database.',
        'CQRS use cases. Explicit contracts. Focused responsibilities.',
        'Domain rules and database constraints protect the ride workflow.',
        'Original development and a focused architectural refactor.']

def frame(t):
    scene = min(6,int(t/SEG)); local=t-scene*SEG; p=local/SEG
    im=BASE.copy(); d=ImageDraw.Draw(im)
    for j,col in enumerate(['#143450','#19445f','#20536b']):
        points=[(x,650+j*18+16*math.sin(x/170+t*.32+j)) for x in range(-10,W+11,8)]
        d.polygon(points+[(W,H),(0,H)],fill=col)
    text(d,(62,30),'UBER',23,heavy=True)
    text(d,(1218,36),'SYSTEM OVERVIEW  /  MOTION GRAPHICS',13,MUTED,anchor='ra')
    offset=int(18*(1-ease(local/0.8)))
    text(d,(62,98+offset),TITLES[scene],43,heavy=True)
    text(d,(64,164+offset),SUBS[scene],21,MUTED)
    if scene==0:
        card(d,(64,242,1216,573))
        text(d,(106,276),'UBER',92,heavy=True)
        text(d,(112,393),'REAL-TIME RIDE-HAILING PLATFORM',22,CYAN)
        for i,label in enumerate(['BOOK','CONNECT','RIDE']):
            x=112+i*160
            card(d,(x,469,x+140,518),fill='#1b3c55')
            text(d,(x+70,480),label,17,anchor='ma')
        route=[(790,485),(790,360),(940,360),(940,285),(1110,285)]
        d.line(route,fill='#284b65',width=10)
        pos=line_progress(d,route,ease((local-.6)/5))
        for x,y in (route[0],route[-1]): d.ellipse((x-10,y-10,x+10,y+10),fill=WHITE)
        d.ellipse((pos[0]-15,pos[1]-15,pos[0]+15,pos[1]+15),fill=CYAN)
    elif scene==1:
        card(d,(64,238,790,584))
        for x in range(105,790,76): d.line((x,252,x,570),fill='#203a50',width=2)
        for y in range(270,584,58): d.line((80,y,775,y),fill='#203a50',width=2)
        route=[(160,500),(310,500),(310,330),(620,330),(620,280)]
        d.line(route,fill='#365169',width=9)
        pos=line_progress(d,route,ease(local/5))
        d.ellipse((148,488,172,512),fill=WHITE)
        d.ellipse((608,268,632,292),fill=CYAN)
        card(d,(pos[0]-22,pos[1]-13,pos[0]+22,pos[1]+13),CYAN,CYAN,7)
        for i,(a,b) in enumerate([('01 / ROUTE','Pickup + destination'),('02 / QUOTE','Server-calculated price'),('03 / MATCH','Notify a nearby driver')]):
            y=240+i*114
            card(d,(820,y,1216,y+96))
            text(d,(842,y+15),a,16,CYAN if local>i*1.6 else MUTED,True)
            text(d,(842,y+47),b,23)
    elif scene==2:
        steps=['Pending','Accepted','Driver waiting','In progress','Completed']
        active=min(4,int(local/1.35))
        d.line((150,344,1130,344),fill='#35516b',width=5)
        d.line((150,344,150+245*min(4,local/1.35),344),fill=CYAN,width=5)
        for i,label in enumerate(steps):
            x=150+i*245
            d.ellipse((x-26,318,x+26,370),fill=CYAN if i<=active else '#263e55')
            text(d,(x,326),str(i+1),23,'#102c40' if i<=active else MUTED,True,'ma')
            text(d,(x,390),label,21,WHITE if i<=active else MUTED,anchor='ma')
        card(d,(260,478,1020,570))
        text(d,(640,495),'Navigate away. Reconnect. Resume your active ride.',22,anchor='ma')
        text(d,(640,533),'Persisted ride state supports recovery.',17,MUTED,anchor='ma')
    elif scene==3:
        for i,(title,small) in enumerate([('RIDER WALLET','Debit on completion'),('DRIVER WALLET','Credit on completion')]):
            x=64+i*604
            card(d,(x,243,x+548,421))
            text(d,(x+30,267),title,17,CYAN)
            text(d,(x+30,308),small,32,heavy=True)
            text(d,(x+30,367),'One Unit of Work commit',19,MUTED)
        card(d,(64,445,1216,587))
        text(d,(96,468),'A rating that lasts.',30,heavy=True)
        text(d,(96,519),'One rating per participant, per completed ride.',20,MUTED)
        for i in range(5):
            cx=865+i*62; cy=508
            pts=[(cx+ (23 if j%2==0 else 10)*math.cos(-math.pi/2+j*math.pi/5),cy+(23 if j%2==0 else 10)*math.sin(-math.pi/2+j*math.pi/5)) for j in range(10)]
            d.polygon(pts,fill='#f5cd78' if local>i*.5+.4 else '#34516a')
    elif scene==4:
        boxes=[(64,260,330,420),(493,260,787,420),(950,260,1216,420),(493,470,787,587)]
        for a,b in [((330,340),(493,340)),((787,340),(950,340)),((640,470),(640,420))]:
            d.line([a,b],fill=CYAN,width=3)
            if a[1]==b[1]: d.polygon([(b[0],b[1]),(b[0]-10,b[1]-6),(b[0]-10,b[1]+6)],fill=CYAN)
            else: d.polygon([(640,420),(634,430),(646,430)],fill=CYAN)
        for box,title,sub in zip(boxes,['Presentation','Application','Domain','Infrastructure'],['MVC + APIs + SignalR','CQRS + validation','Entities + business rules','Persistence + integrations']):
            card(d,box)
            x=(box[0]+box[2])/2
            text(d,(x,box[1]+30),title,28,WHITE,True,'ma')
            text(d,(x,box[1]+82),sub,17,MUTED,anchor='ma')
        text(d,(195,530),'Dependencies point inward.',18,CYAN,anchor='ma')
        text(d,(1083,530),'One shared SQL database.',18,CYAN,anchor='ma')
    elif scene==5:
        items=[('01','VALID TRANSITIONS','Business rules live in the Domain.'),('02','EXCLUSIVE RESERVATION','One accepted or ongoing ride per driver.'),('03','CONCURRENCY CONTROL','Row versions protect against lost updates.'),('04','PERSISTENT RATINGS','Unique constraints prevent duplicate ratings.')]
        for i,(n,title,sub) in enumerate(items):
            x=64+(i%2)*592; y=242+(i//2)*176
            card(d,(x,y,x+560,y+150))
            text(d,(x+24,y+23),n,25,CYAN if local>i*.65 else MUTED,True)
            text(d,(x+81,y+27),title,20,heavy=True)
            text(d,(x+24,y+88),sub,20,MUTED)
    else:
        card(d,(64,236,1216,592))
        text(d,(640,266),'ORIGINAL DEVELOPERS',15,MUTED,anchor='ma')
        text(d,(640,290),'Ibrahim  /  Amr  /  Esraa Hussien',48,heavy=True,anchor='ma')
        text(d,(640,390),'REFACTORED BY',15,MUTED,anchor='ma')
        text(d,(640,420),'DESPERADO',48,WHITE,True,'ma')
        text(d,(640,485),'github.com/iTz-DESPERADO',20,MUTED,anchor='ma')
        text(d,(640,545),'Like the project? Star the repository.',25,WHITE,True,'ma')
    for i in range(7):
        x=64+i*166
        d.rounded_rectangle((x,680,x+150,684),2,fill='#355168')
        if i<=scene:
            amount=1 if i<scene else p
            d.rounded_rectangle((x,680,x+max(2,150*amount),684),2,fill=CYAN)
    text(d,(64,625),f'{scene+1:02d} / 07',14,MUTED)
    text(d,(1216,625),'ASP.NET CORE  /  SQL SERVER  /  SIGNALR',13,MUTED,anchor='ra')
    fade=min(ease(local/.45),ease((SEG-local)/.45))
    return Image.blend(BASE,im,fade)

def render():
    encoder=imageio_ffmpeg.get_ffmpeg_exe()
    with tempfile.TemporaryDirectory(prefix='uber-film-') as tmp:
        tmp=Path(tmp)
        video=tmp/'silent.mp4'
        process=subprocess.Popen([encoder,'-y','-f','rawvideo','-vcodec','rawvideo','-pix_fmt','rgb24','-s',f'{W}x{H}','-r',str(FPS),'-i','-','-an','-c:v','libx264','-preset','fast','-crf','20','-pix_fmt','yuv420p',str(video)],stdin=subprocess.PIPE,stderr=(tmp/'encode.log').open('w'))
        for idx in range(DURATION*FPS):
            process.stdin.write(frame(idx/FPS).tobytes())
            if idx%(SEG*FPS)==0: print(f'Rendering chapter {idx//(SEG*FPS)+1}/7',flush=True)
        process.stdin.close()
        if process.wait()!=0: raise RuntimeError((tmp/'encode.log').read_text())
        subprocess.run([encoder,'-y','-i',str(video),'-c:v','copy','-an','-movflags','+faststart',str(OUT/'uber-system-overview.mp4')],check=True,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
    frame(3).save(OUT/'poster.jpg',quality=94)
    sheet=Image.new('RGB',(960,1080),'#091323')
    for i in range(7):
        thumb=frame(i*SEG+4).resize((480,270),Image.Resampling.LANCZOS)
        sheet.paste(thumb,((i%2)*480,(i//2)*270))
    review=ROOT/'artifacts'/'video-review'
    review.mkdir(parents=True,exist_ok=True)
    sheet.save(review/'contact-sheet.jpg',quality=90)
    print(f'Created {OUT / "uber-system-overview.mp4"}',flush=True)

if __name__=='__main__': render()
