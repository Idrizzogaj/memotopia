# Builds the art for the Memotopia ad studio: 9 phone panels (1080x1920), app icon, hero logo -> base64 JSON
import base64, io, json, sys
from PIL import Image, ImageDraw, ImageFilter
R = r'C:\Users\IdrZog_MEM\GitProjects\memotopia-sprint\docs'
raw = sorted(__import__('glob').glob(R + r'\store-screenshots\raw\*.png'))
def grad(w, h, c0, c1):
    im = Image.new('RGB', (w, h)); px = im.load()
    for y in range(h):
        t = y / (h - 1); c = tuple(int(c0[i] + (c1[i] - c0[i]) * t) for i in range(3))
        for x in range(w): px[x, y] = c
    return im
def panel(path):
    bg = grad(1080, 1920, (124, 34, 255), (62, 16, 168))
    s = Image.open(path).convert('RGB'); H = 1640; W = int(s.width * H / s.height)
    s = s.resize((W, H), Image.LANCZOS)
    m = Image.new('L', (W, H), 0); ImageDraw.Draw(m).rounded_rectangle([0, 0, W - 1, H - 1], 70, fill=255)
    x, y = (1080 - W) // 2, 200
    sh = Image.new('RGBA', (1080, 1920), (0, 0, 0, 0)); ImageDraw.Draw(sh).rounded_rectangle([x, y + 24, x + W, y + H + 24], 70, fill=(0, 0, 0, 120))
    bg = Image.alpha_composite(bg.convert('RGBA'), sh.filter(ImageFilter.GaussianBlur(28))).convert('RGB')
    d = ImageDraw.Draw(bg); d.rounded_rectangle([x - 14, y - 14, x + W + 14, y + H + 14], 82, fill=(255, 255, 255))
    bg.paste(s, (x, y), m)
    return bg
def b64(im, fmt, **kw):
    b = io.BytesIO(); im.save(b, fmt, **kw); return 'data:image/%s;base64,' % ('png' if fmt == 'PNG' else 'jpeg') + base64.b64encode(b.getvalue()).decode()
panels = [b64(panel(p), 'JPEG', quality=80) for p in raw]
logo = Image.open(R + r'\web\site\screens\logo.png').convert('RGBA')
bb = logo.getbbox(); logo = logo.crop(bb)
hero = logo.resize((760, int(760 * logo.height / logo.width)), Image.LANCZOS)
ic = grad(512, 512, (124, 34, 255), (62, 16, 168)).convert('RGBA')
lg = logo.resize((340, int(340 * logo.height / logo.width)), Image.LANCZOS)
ic.paste(lg, ((512 - lg.width) // 2, (512 - lg.height) // 2), lg)
mk = Image.new('L', (512, 512), 0); ImageDraw.Draw(mk).rounded_rectangle([0, 0, 511, 511], 110, fill=255); ic.putalpha(mk)
json.dump({'panels': panels, 'hero': b64(hero, 'PNG'), 'icon': b64(ic, 'PNG')}, open(sys.argv[1], 'w'))
print(len(raw), 'panels ok')
