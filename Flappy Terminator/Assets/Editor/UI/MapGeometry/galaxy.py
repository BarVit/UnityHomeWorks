import json, math, random
from render_lib import render

W, H = 1920, 1080
CX, CY = 960, 540
R_IN, R_OUT = 105, 440          # arm territories span this radius band
PITCH = math.radians(24)        # winding of arms and borders
K = 1 / math.tan(PITCH)
TILT = 0.0
random.seed(11)

ARMS = [  # id, title, available ; listed counter-clockwise from the Orion slot
    ("orion", "ORION ARM", True),
    ("perseus", "PERSEUS ARM", False),
    ("outer", "OUTER ARM", False),
    ("norma", "NORMA ARM", False),
    ("scutum", "SCUTUM-CENTAURUS ARM", False),
    ("far3kpc", "FAR 3 KPC ARM", False),
    ("carina", "CARINA ARM", False),
    ("sagittarius", "SAGITTARIUS ARM", False),
]
N = len(ARMS)
THETA0 = math.radians(150)      # Orion territory starts lower-left


def spiral(theta0, r):
    return theta0 + K * math.log(r / R_IN)


def pt(r, th):
    return (CX + r * math.cos(th), CY + r * math.sin(th))


def border(k, radii):
    th0 = THETA0 + k * 2 * math.pi / N
    return [pt(r, spiral(th0, r)) for r in radii]


BORDER_RADII = [R_IN, 150, 200, 260, 320, 385, R_OUT]


def region(k):
    a = border(k, BORDER_RADII)
    b = border(k + 1, BORDER_RADII)
    th_a = spiral(THETA0 + k * 2 * math.pi / N, R_OUT)
    th_b = spiral(THETA0 + (k + 1) * 2 * math.pi / N, R_OUT)
    outer = [pt(R_OUT, th_a + (th_b - th_a) * t / 4) for t in range(1, 4)]
    ti_a = THETA0 + k * 2 * math.pi / N
    ti_b = THETA0 + (k + 1) * 2 * math.pi / N
    inner = [pt(R_IN, ti_b - (ti_b - ti_a) * t / 3) for t in range(1, 3)]
    return a + outer + list(reversed(b)) + inner


def centre_of(k, frac):
    r = R_IN + (R_OUT - R_IN) * frac
    th = spiral(THETA0 + (k + 0.5) * 2 * math.pi / N, r)
    return pt(r, th)


def gauss(s):
    return random.gauss(0, s)


parts = []
defs = '''
<radialGradient id="disk" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#3a3350" stop-opacity="0.9"/><stop offset="0.35" stop-color="#1d2240" stop-opacity="0.75"/><stop offset="0.75" stop-color="#0d1328" stop-opacity="0.35"/><stop offset="1" stop-color="#05070f" stop-opacity="0"/></radialGradient>
<radialGradient id="bulge" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#fff4d6"/><stop offset="0.25" stop-color="#ffd38a" stop-opacity="0.95"/><stop offset="0.6" stop-color="#d98a4a" stop-opacity="0.45"/><stop offset="1" stop-color="#7a3d2a" stop-opacity="0"/></radialGradient>
<radialGradient id="soft"><stop offset="0" stop-color="#fff" stop-opacity="1"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></radialGradient>
<radialGradient id="hole"><stop offset="0" stop-color="#000"/><stop offset="0.55" stop-color="#000"/><stop offset="0.7" stop-color="#ffcf80"/><stop offset="0.82" stop-color="#ff7a2a" stop-opacity="0.8"/><stop offset="1" stop-color="#ff7a2a" stop-opacity="0"/></radialGradient>
<filter id="blur2"><feGaussianBlur stdDeviation="2"/></filter>
<filter id="blur6"><feGaussianBlur stdDeviation="6"/></filter>
<filter id="blur14"><feGaussianBlur stdDeviation="14"/></filter>
<filter id="blur30"><feGaussianBlur stdDeviation="30"/></filter>
'''
parts.append(f'<rect width="{W}" height="{H}" fill="#03050b"/>')
for i in range(1400):
    x, y = random.uniform(0, W), random.uniform(0, H)
    parts.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{random.choice([0.5,0.6,0.8,1.0])}" fill="#cfd8ff" opacity="{random.uniform(0.15,0.6):.2f}"/>')

parts.append(f'<ellipse cx="{CX}" cy="{CY}" rx="{R_OUT + 70}" ry="{R_OUT + 70}" fill="url(#disk)"/>')

for _ in range(6000):
    r = min(R_OUT + 60, random.expovariate(1 / 170) + 40)
    th = random.uniform(0, 2 * math.pi)
    x, y = pt(r, th)
    parts.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{random.choice([0.4,0.5,0.6,0.8])}" fill="{random.choice(["#e8ecff","#fff2dc","#d6e0ff"])}" opacity="{random.uniform(0.25,0.75):.2f}"/>')

ARM_SPIRAL_OFFSETS = [(k + 0.5) * 2 * math.pi / N for k in range(N)]
glow = []
stars = []
nebulae = []
dust = []
for k, off in enumerate(ARM_SPIRAL_OFFSETS):
    major = k % 2 == 0
    th0 = THETA0 + off
    steps = 260 if major else 160
    for i in range(steps):
        t = i / steps
        r = R_IN * 0.9 + (R_OUT + 40 - R_IN * 0.9) * t
        th = spiral(th0, r)
        x, y = pt(r, th)
        width = (12 + 22 * t) * (1.0 if major else 0.65)
        if i % 3 == 0:
            glow.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{width * 1.5:.1f}" fill="#8aa6ff" opacity="{0.10 if major else 0.06}"/>')
        n = 22 if major else 11
        for _ in range(n):
            dx, dy = gauss(width), gauss(width)
            c = random.choice(["#ffffff", "#dfe8ff", "#bcd0ff", "#a8c4ff", "#fff1d8"])
            rad = random.choice([0.5, 0.6, 0.7, 0.8, 1.0, 1.2]) if random.random() < 0.98 else 1.8
            stars.append(f'<circle cx="{x+dx:.1f}" cy="{y+dy:.1f}" r="{rad}" fill="{c}" opacity="{random.uniform(0.45,1):.2f}"/>')
        if random.random() < (0.10 if major else 0.06):
            cx_, cy_ = x + gauss(width * 0.6), y + gauss(width * 0.6)
            for _ in range(random.randint(14, 30)):
                stars.append(f'<circle cx="{cx_+gauss(5):.1f}" cy="{cy_+gauss(5):.1f}" r="{random.choice([0.8,1.0,1.3])}" fill="#eef4ff" opacity="0.95"/>')
            glow.append(f'<circle cx="{cx_:.1f}" cy="{cy_:.1f}" r="12" fill="#cfe0ff" opacity="0.18" filter="url(#blur6)"/>')
        if random.random() < (0.07 if major else 0.04):
            nebulae.append(f'<ellipse cx="{x+gauss(width*0.5):.1f}" cy="{y+gauss(width*0.5):.1f}" rx="{random.uniform(6,14):.1f}" ry="{random.uniform(4,9):.1f}" fill="{random.choice(["#ff5fa8","#ff7ab8","#e04f9a"])}" opacity="{random.uniform(0.35,0.7):.2f}" filter="url(#blur2)"/>')
        if random.random() < 0.05:
            nebulae.append(f'<ellipse cx="{x:.1f}" cy="{y:.1f}" rx="{width*1.4:.1f}" ry="{width*0.8:.1f}" fill="#9db6ff" opacity="0.10" filter="url(#blur14)" transform="rotate({math.degrees(th)+90:.0f} {x:.1f} {y:.1f})"/>')
    # dust lane along the inner edge of each major arm
    if True:
        pts = []
        for i in range(60):
            r = R_IN + (R_OUT - R_IN) * i / 59
            th = spiral(th0, r) - 0.10
            pts.append('%.1f,%.1f' % pt(r, th))
        dust.append(f'<polyline points="{" ".join(pts)}" fill="none" stroke="#1a0e10" stroke-width="{6 if major else 3}" opacity="{0.6 if major else 0.4}" filter="url(#blur2)"/>')

parts.append('<g filter="url(#blur14)">' + ''.join(glow) + '</g>')
parts.append(''.join(nebulae))
parts += stars
parts += dust

# bulge, bar and black hole
BAR = math.radians(-28)
parts.append(f'<ellipse cx="{CX}" cy="{CY}" rx="120" ry="95" fill="url(#bulge)" opacity="0.45" filter="url(#blur14)"/>')
parts.append(f'<ellipse cx="{CX}" cy="{CY}" rx="125" ry="34" fill="url(#bulge)" transform="rotate({math.degrees(BAR):.0f} {CX} {CY})" filter="url(#blur6)"/>')
parts.append(f'<ellipse cx="{CX}" cy="{CY}" rx="95" ry="18" fill="#fff0c8" opacity="0.55" transform="rotate({math.degrees(BAR):.0f} {CX} {CY})" filter="url(#blur6)"/>')
for _ in range(1800):
    u, v = gauss(55), gauss(13)
    x = CX + u * math.cos(BAR) - v * math.sin(BAR)
    y = CY + u * math.sin(BAR) + v * math.cos(BAR)
    parts.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{random.choice([0.6,0.8,1.0])}" fill="{random.choice(["#fff3d0","#ffe0a0","#ffd090"])}" opacity="{random.uniform(0.4,1):.2f}"/>')
parts.append(f'<circle cx="{CX}" cy="{CY}" r="40" fill="#ffe7b5" opacity="0.45" filter="url(#blur14)"/>')
parts.append(f'<ellipse cx="{CX}" cy="{CY}" rx="26" ry="7" fill="none" stroke="#ffb15a" stroke-width="3" opacity="0.9" transform="rotate(-12 {CX} {CY})" filter="url(#blur2)"/>')
parts.append(f'<circle cx="{CX}" cy="{CY}" r="15" fill="url(#hole)"/>')
parts.append(f'<circle cx="{CX}" cy="{CY}" r="7" fill="#000"/>')

render('Galaxy Map', W, H, ''.join(parts), defs)

geom = {"size": [W, H], "center": [CX / W, CY / H], "arms": []}
for k, (aid, title, available) in enumerate(ARMS):
    poly = region(k)
    geom["arms"].append({
        "id": aid, "title": title, "available": available,
        "polygon": [[x / W, y / H] for x, y in poly],
        "border": [[x / W, y / H] for x, y in border(k, BORDER_RADII)],
        "label": [c / s for c, s in zip(centre_of(k, 0.78), (W, H))],
    })
sun = centre_of(0, 0.55)
geom["sun"] = [sun[0] / W, sun[1] / H]
json.dump(geom, open('galaxy_geometry.json', 'w'), indent=1)
print('arms', len(geom["arms"]), 'sun', geom["sun"])
