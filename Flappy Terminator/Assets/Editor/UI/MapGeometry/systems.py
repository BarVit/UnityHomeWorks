import json, math, random
from render_lib import render

W, H = 1920, 1080
random.seed(23)


def g(s):
    return random.gauss(0, s)


DEFS = '''
<filter id="b2" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="2"/></filter>
<filter id="b6" x="-100%" y="-100%" width="300%" height="300%"><feGaussianBlur stdDeviation="6"/></filter>
<filter id="b16" filterUnits="userSpaceOnUse" x="-200" y="-200" width="2320" height="1480"><feGaussianBlur stdDeviation="16"/></filter>
<filter id="b40" filterUnits="userSpaceOnUse" x="-200" y="-200" width="2320" height="1480"><feGaussianBlur stdDeviation="40"/></filter>
<filter id="b80" filterUnits="userSpaceOnUse" x="-200" y="-200" width="2320" height="1480"><feGaussianBlur stdDeviation="80"/></filter>
'''


def starfield(n, opacity=(0.2, 0.8)):
    out = []
    for _ in range(n):
        x, y = random.uniform(0, W), random.uniform(0, H)
        r = random.choice([0.5, 0.6, 0.7, 0.9, 1.1]) if random.random() < 0.985 else 1.7
        c = random.choice(["#ffffff", "#dfe8ff", "#c6d6ff", "#fff0d8"])
        out.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{r}" fill="{c}" opacity="{random.uniform(*opacity):.2f}"/>')
    return ''.join(out)


# ---------------- Orion arm view ----------------
SYSTEMS = [
    ("sys_01", (330, 780), "#ffd66b"),
    ("sys_02", (700, 600), "#ffa04a"),
    ("sys_03", (1030, 700), "#cfe4ff"),
    ("sys_04", (1350, 480), "#ff6a4a"),
    ("sys_05", (1640, 300), "#7fb0ff"),
]


def arm_axis(t):
    # the band runs from lower-left to upper-right with a gentle bend
    x = -150 + t * (W + 300)
    y = 900 - t * 680 + 70 * math.sin(t * math.pi)
    return x, y


parts = [f'<rect width="{W}" height="{H}" fill="#03050b"/>', starfield(1600, (0.15, 0.6))]
glow, nebula, stars, dust = [], [], [], []
for i in range(320):
    t = i / 320
    x, y = arm_axis(t)
    glow.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{random.uniform(150,230):.0f}" fill="#5f7fd8" opacity="0.045"/>')
    for _ in range(70):
        dx, dy = g(140), g(140)
        c = random.choice(["#ffffff", "#e6eeff", "#c4d6ff", "#a9c2ff", "#fff1d6", "#ffe0b0"])
        r = random.choice([0.5, 0.6, 0.8, 1.0, 1.2]) if random.random() < 0.985 else random.choice([1.8, 2.4])
        stars.append(f'<circle cx="{x+dx:.1f}" cy="{y+dy:.1f}" r="{r}" fill="{c}" opacity="{random.uniform(0.35,1):.2f}"/>')
    if random.random() < 0.06:
        cx, cy = x + g(110), y + g(110)
        for _ in range(random.randint(40, 90)):
            stars.append(f'<circle cx="{cx+g(14):.1f}" cy="{cy+g(14):.1f}" r="{random.choice([0.7,1.0,1.3])}" fill="#f4f8ff" opacity="0.95"/>')
        glow.append(f'<circle cx="{cx:.1f}" cy="{cy:.1f}" r="40" fill="#d8e6ff" opacity="0.25"/>')
    if random.random() < 0.05:
        nebula.append(f'<ellipse cx="{x+g(120):.1f}" cy="{y+g(120):.1f}" rx="{random.uniform(60,150):.0f}" ry="{random.uniform(30,80):.0f}" fill="{random.choice(["#ff4f9a","#e0458f","#ff7ab0"])}" opacity="{random.uniform(0.18,0.35):.2f}" transform="rotate({random.uniform(0,180):.0f} {x:.0f} {y:.0f})"/>')
    if random.random() < 0.04:
        nebula.append(f'<ellipse cx="{x+g(140):.1f}" cy="{y+g(140):.1f}" rx="{random.uniform(80,180):.0f}" ry="{random.uniform(40,90):.0f}" fill="#5aa8ff" opacity="{random.uniform(0.12,0.22):.2f}"/>')
for off, width, op in ((-70, 60, 0.5), (95, 26, 0.4), (10, 14, 0.3)):
    pts = ' '.join('%.0f,%.0f' % (arm_axis(i / 24)[0] + off * 0.4, arm_axis(i / 24)[1] + off) for i in range(25))
    dust.append(f'<polyline points="{pts}" fill="none" stroke="#150b10" stroke-width="{width}" stroke-linejoin="round" opacity="{op}" filter="url(#b16)"/>')
parts.append('<g filter="url(#b80)">' + ''.join(glow) + '</g>')
parts.append('<g filter="url(#b40)">' + ''.join(nebula) + '</g>')
parts += stars
parts += dust
render('Orion Arm Map', W, H, ''.join(parts), DEFS)

geometry = {"size": [W, H], "systems": [{"id": sid, "pos": [x / W, y / H], "color": col} for sid, (x, y), col in SYSTEMS]}

# ---------------- star systems ----------------
COUNTS = [5, 7, 9, 6, 10]
PLANETS = ["#c98b5a", "#7fa7d6", "#d9c08a", "#8fc59a", "#b67ad1", "#e07a5f", "#6fb7c9", "#cfcfcf"]
SCX, SCY = 960, 530
geometry["star_systems"] = []
for idx, (sid, _, star_col) in enumerate(SYSTEMS):
    n = COUNTS[idx]
    parts = [f'<rect width="{W}" height="{H}" fill="#03050b"/>', starfield(1300, (0.15, 0.65))]
    neb = []
    for _ in range(5):
        neb.append(f'<ellipse cx="{random.uniform(0,W):.0f}" cy="{random.uniform(0,H):.0f}" rx="{random.uniform(200,420):.0f}" ry="{random.uniform(120,260):.0f}" fill="{random.choice(["#2b3f8a","#5a2a7a","#1f5a7a",star_col])}" opacity="{random.uniform(0.10,0.22):.2f}"/>')
    parts.append('<g filter="url(#b80)">' + ''.join(neb) + '</g>')
    # star
    parts.append(f'<circle cx="{SCX}" cy="{SCY}" r="190" fill="{star_col}" opacity="0.18" filter="url(#b80)"/>')
    parts.append(f'<circle cx="{SCX}" cy="{SCY}" r="80" fill="{star_col}" opacity="0.45" filter="url(#b40)"/>')
    parts.append(f'<circle cx="{SCX}" cy="{SCY}" r="44" fill="{star_col}" filter="url(#b6)"/>')
    parts.append(f'<circle cx="{SCX}" cy="{SCY}" r="30" fill="#fffbe8" opacity="0.9" filter="url(#b2)"/>')
    for k in range(4):
        a = k * 45 + 22
        parts.append(f'<line x1="{SCX-140*math.cos(math.radians(a)):.0f}" y1="{SCY-140*math.sin(math.radians(a)):.0f}" x2="{SCX+140*math.cos(math.radians(a)):.0f}" y2="{SCY+140*math.sin(math.radians(a)):.0f}" stroke="#fff" stroke-width="2" opacity="0.18" filter="url(#b2)"/>')
    locs = []
    theta = random.uniform(0, 2 * math.pi)
    for i in range(n):
        rx = 150 + (780 - 150) * i / max(1, n - 1)
        ry = rx * 0.42
        theta += math.radians(random.uniform(55, 80))
        x, y = SCX + rx * math.cos(theta), SCY + ry * math.sin(theta)
        is_belt = i % 3 == 2
        if is_belt:
            belt = []
            for _ in range(int(rx * 1.6)):
                a = random.uniform(0, 2 * math.pi)
                d = g(9)
                belt.append(f'<circle cx="{SCX+(rx+d)*math.cos(a):.1f}" cy="{SCY+(ry+d*0.42)*math.sin(a):.1f}" r="{random.choice([0.7,0.9,1.2,1.6])}" fill="{random.choice(["#9a8f84","#b8aa98","#7d7468"])}" opacity="{random.uniform(0.4,0.9):.2f}"/>')
            parts.append(''.join(belt))
            for _ in range(14):
                parts.append(f'<circle cx="{x+g(16):.1f}" cy="{y+g(8):.1f}" r="{random.uniform(1.5,3.5):.1f}" fill="#a89a88"/>')
        else:
            parts.append(f'<ellipse cx="{SCX}" cy="{SCY}" rx="{rx:.0f}" ry="{ry:.0f}" fill="none" stroke="#9fc6ff" stroke-width="1.2" opacity="0.22"/>')
            pr = random.uniform(11, 24)
            col = random.choice(PLANETS)
            lx = (SCX - x) / math.hypot(SCX - x, SCY - y)
            ly = (SCY - y) / math.hypot(SCX - x, SCY - y)
            gid = f'p{idx}_{i}'
            parts.append(f'<defs><radialGradient id="{gid}" cx="{50+lx*30:.0f}%" cy="{50+ly*30:.0f}%" r="70%"><stop offset="0" stop-color="#ffffff" stop-opacity="0.9"/><stop offset="0.25" stop-color="{col}"/><stop offset="1" stop-color="#05070c"/></radialGradient></defs>')
            parts.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{pr*1.8:.1f}" fill="{col}" opacity="0.18" filter="url(#b6)"/>')
            parts.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{pr:.1f}" fill="url(#{gid})"/>')
            if random.random() < 0.3:
                parts.append(f'<ellipse cx="{x:.1f}" cy="{y:.1f}" rx="{pr*1.9:.1f}" ry="{pr*0.5:.1f}" fill="none" stroke="#e8d9b5" stroke-width="2.5" opacity="0.7" transform="rotate(-18 {x:.1f} {y:.1f})"/>')
        locs.append([x / W, y / H])
    render(f'System {idx + 1} Map', W, H, ''.join(parts), DEFS)
    geometry["star_systems"].append({"id": sid, "color": star_col, "locations": locs})

json.dump(geometry, open('systems_geometry.json', 'w'), indent=1)
print('systems', [len(s["locations"]) for s in geometry["star_systems"]])
