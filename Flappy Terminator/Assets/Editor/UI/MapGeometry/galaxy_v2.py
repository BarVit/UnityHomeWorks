# Galaxy map, version 2: four major arms, the Orion spur as its own arm, and the Empire around the core.
# Writes "Galaxy Map v2.png" and "Galaxy Map v2.json" (zones + borders, normalised 0..1, y down).
import json, math, random, sys
from render_lib import render

W, H = 1920, 1080
CX, CY = 960, 540
R0 = 105                     # where the arms leave the core
RING = 494                   # outer border of the galaxy: encloses every arm
PITCH = math.radians(22)
K = 1 / math.tan(PITCH)
random.seed(7)


def spiral(offset, r):
    return offset + K * math.log(r / R0)


def pt(r, th):
    return (CX + r * math.cos(th), CY + r * math.sin(th))


def norm(p):
    return {"x": round(p[0] / W, 5), "y": round(p[1] / H, 5)}


def g(s):
    return random.gauss(0, s)


# --- layout -------------------------------------------------------------------------------------------
BAR = math.radians(-28)
ARMS = [  # id, title, centre offset of the arm (radians, before ROT)
    ("scutum", "SCUTUM-CENTAURUS ARM", BAR),
    ("sagittarius", "SAGITTARIUS ARM", BAR + math.pi / 2),
    ("perseus", "PERSEUS ARM", BAR + math.pi),
    ("norma", "NORMA ARM", BAR + 3 * math.pi / 2),
]
ORION_OFFSET = BAR + 3 * math.pi / 4          # in the gap between Sagittarius and Perseus

# turn the whole picture so that the Orion arm sits on the right, where the old map had it
ROT = -spiral(ORION_OFFSET, 330)
ARMS = [(i, t, o + ROT) for i, t, o in ARMS]
BAR += ROT
ORION_OFFSET += ROT


def empire_radius(th):
    # lopsided core: one bulge to a side and a softer second harmonic, so the Empire takes uneven bites
    return 186 + 30 * math.sin(th - ROT - 0.9) + 13 * math.sin(2 * (th - ROT) + 1.7)


def exit_radius(offset):
    # where a spiral leaves the Empire, going outward
    r = R0
    while r < RING and r < empire_radius(spiral(offset, r)):
        r += 0.5
    return r


def spiral_line(offset, r_from, r_to, step=46.0, drift=None):
    pts = []
    r = r_from
    while True:
        o = offset + (drift(r) if drift else 0.0)
        pts.append(pt(r, spiral(o, r)))
        if r >= r_to:
            break
        r = min(r_to, r + step)
    return pts


# borders between neighbouring zones (offsets halfway between arm centres)
B = [ARMS[i][2] + math.pi / 4 for i in range(4)]     # B[i]: between arm i and arm i+1
FORK_WIDTH = math.radians(17)


def fork_drift(sign, r_fork):
    def f(r):
        t = min(1.0, max(0.0, (r - r_fork) / 70.0))
        return sign * FORK_WIDTH * (t * t * (3 - 2 * t))
    return f


starts = [exit_radius(o) for o in B]
R_FORK = max(starts[1] + 45, 255)

border_lines = {}
for i, o in enumerate(B):
    if i == 1:   # Sagittarius | Perseus forks around the Orion arm
        trunk = spiral_line(o, starts[i], R_FORK)
        border_lines["sag_orion"] = trunk[:-1] + spiral_line(o, R_FORK, RING, drift=fork_drift(-1, R_FORK))
        border_lines["orion_per"] = trunk[:-1] + spiral_line(o, R_FORK, RING, drift=fork_drift(+1, R_FORK))
        border_lines["orion_left"] = spiral_line(o, R_FORK, RING, drift=fork_drift(-1, R_FORK))
        border_lines["orion_right"] = spiral_line(o, R_FORK, RING, drift=fork_drift(+1, R_FORK))
        border_lines["trunk"] = trunk
    else:
        border_lines[i] = spiral_line(o, starts[i], RING)


def angle_of(p):
    return math.atan2(p[1] - CY, p[0] - CX)


# Empire polygon: radius function sampled every 12 degrees plus the exact border exits
empire_pts = []
for k in range(15):
    th = k * 2 * math.pi / 15
    empire_pts.append((th % (2 * math.pi), pt(empire_radius(th), th)))
border_starts = {0: border_lines[0][0], 1: border_lines["trunk"][0], 2: border_lines[2][0], 3: border_lines[3][0]}
for p in border_starts.values():
    empire_pts.append((angle_of(p) % (2 * math.pi), p))
empire_pts.sort()
# drop sampled vertices that sit within 3 degrees of an exact border start
exact = [angle_of(p) % (2 * math.pi) for p in border_starts.values()]
clean = []
for a, p in empire_pts:
    if any(abs(a - e) < math.radians(7) and p not in border_starts.values() for e in exact):
        continue
    clean.append((a, p))
empire = [p for _, p in clean]


def empire_chain(p_from, p_to):
    a0 = angle_of(p_from) % (2 * math.pi)
    a1 = angle_of(p_to) % (2 * math.pi)
    span = (a1 - a0) % (2 * math.pi)
    chain = [p_from]
    for a, p in clean:
        d = (a - a0) % (2 * math.pi)
        if 1e-6 < d < span - 1e-6:
            chain.append(p)
    chain.append(p_to)
    return chain


def ring_arc(p_from, p_to):
    # from p_from backwards (decreasing angle) to p_to, every 3 degrees
    a0 = angle_of(p_from)
    span = (a0 - angle_of(p_to)) % (2 * math.pi)
    steps = max(1, int(math.degrees(span) / 5))
    return [pt(RING, a0 - span * s / steps) for s in range(1, steps)]


def zone(left, right, inner):
    # left/right: border polylines from the inside out; zone lies between them going counter-clockwise
    poly = list(inner)
    poly += right[1:]
    poly += ring_arc(right[-1], left[-1])
    poly += list(reversed(left))[:-1]
    if inner and poly[0] == left[0]:
        pass
    return poly


lines = {
    "scutum": (border_lines[3], border_lines[0]),
    "sagittarius": (border_lines[0], border_lines["sag_orion"]),
    "perseus": (border_lines["orion_per"], border_lines[2]),
    "norma": (border_lines[2], border_lines[3]),
}

zones = []
for aid, title, offset in ARMS:
    left, right = lines[aid]
    inner = empire_chain(left[0], right[0])
    poly = zone(left, right, inner)
    zones.append((aid, title, False, poly, offset))

orion_poly = [border_lines["orion_left"][0]] + border_lines["orion_right"][1:] \
    + ring_arc(border_lines["orion_right"][-1], border_lines["orion_left"][-1]) \
    + list(reversed(border_lines["orion_left"]))[:-1]
zones.insert(0, ("orion", "ORION ARM", True, orion_poly, ORION_OFFSET))
zones.append(("empire", "EMPIRE", False, empire, None))


def label_for(aid, offset):
    if aid == "empire":
        best = max(range(36), key=lambda k: empire_radius(k * math.pi / 18))
        th = best * math.pi / 18
        return pt(empire_radius(th) * 0.62, th)
    if aid == "orion":
        r = (R_FORK + RING) / 2 + 20
        return pt(r, spiral(offset, r))
    r = RING * 0.76
    return pt(r, spiral(offset, r))


# --- art ----------------------------------------------------------------------------------------------
defs = '''
<radialGradient id="disk" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#3a3350" stop-opacity="0.9"/><stop offset="0.35" stop-color="#1d2240" stop-opacity="0.75"/><stop offset="0.75" stop-color="#0d1328" stop-opacity="0.35"/><stop offset="1" stop-color="#05070f" stop-opacity="0"/></radialGradient>
<radialGradient id="bulge" cx="50%" cy="50%" r="50%"><stop offset="0" stop-color="#fff4d6"/><stop offset="0.25" stop-color="#ffd38a" stop-opacity="0.95"/><stop offset="0.6" stop-color="#d98a4a" stop-opacity="0.45"/><stop offset="1" stop-color="#7a3d2a" stop-opacity="0"/></radialGradient>
<radialGradient id="hole"><stop offset="0" stop-color="#000"/><stop offset="0.55" stop-color="#000"/><stop offset="0.7" stop-color="#ffcf80"/><stop offset="0.82" stop-color="#ff7a2a" stop-opacity="0.8"/><stop offset="1" stop-color="#ff7a2a" stop-opacity="0"/></radialGradient>
<filter id="blur2"><feGaussianBlur stdDeviation="2"/></filter>
<filter id="blur6"><feGaussianBlur stdDeviation="6"/></filter>
<filter id="blur14"><feGaussianBlur stdDeviation="14"/></filter>
'''
parts = [f'<rect width="{W}" height="{H}" fill="#03050b"/>']
for i in range(1400):
    parts.append(f'<circle cx="{random.uniform(0, W):.1f}" cy="{random.uniform(0, H):.1f}" r="{random.choice([0.5,0.6,0.8,1.0])}" fill="#cfd8ff" opacity="{random.uniform(0.15,0.6):.2f}"/>')
parts.append(f'<circle cx="{CX}" cy="{CY}" r="{RING + 60}" fill="url(#disk)"/>')
for _ in range(6500):
    r = min(RING - 8, random.expovariate(1 / 165) + 40)
    x, y = pt(r, random.uniform(0, 2 * math.pi))
    parts.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{random.choice([0.4,0.5,0.6,0.8])}" fill="{random.choice(["#e8ecff","#fff2dc","#d6e0ff"])}" opacity="{random.uniform(0.25,0.75):.2f}"/>')

glow, stars, nebulae, dust = [], [], [], []


def draw_arm(offset, r_from, r_to, weight, steps, width_from, width_to):
    for i in range(steps):
        t = i / steps
        r = r_from + (r_to - r_from) * t
        x, y = pt(r, spiral(offset, r))
        width = (width_from + (width_to - width_from) * t)
        fade = min(1.0, (r_to - r) / 70.0 + 0.15)
        if i % 3 == 0:
            glow.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{width * 1.5:.1f}" fill="#8aa6ff" opacity="{0.11 * weight * fade:.3f}"/>')
        for _ in range(int(24 * weight)):
            c = random.choice(["#ffffff", "#dfe8ff", "#bcd0ff", "#a8c4ff", "#fff1d8"])
            rad = random.choice([0.5, 0.6, 0.7, 0.8, 1.0, 1.2]) if random.random() < 0.98 else 1.8
            sx, sy = x + g(width), y + g(width)
            if math.hypot(sx - CX, sy - CY) > RING - 6:
                continue
            stars.append(f'<circle cx="{sx:.1f}" cy="{sy:.1f}" r="{rad}" fill="{c}" opacity="{random.uniform(0.45,1) * fade:.2f}"/>')
        if random.random() < 0.10 * weight:
            cx_, cy_ = x + g(width * 0.6), y + g(width * 0.6)
            for _ in range(random.randint(14, 30)):
                stars.append(f'<circle cx="{cx_ + g(5):.1f}" cy="{cy_ + g(5):.1f}" r="{random.choice([0.8,1.0,1.3])}" fill="#eef4ff" opacity="0.95"/>')
            glow.append(f'<circle cx="{cx_:.1f}" cy="{cy_:.1f}" r="12" fill="#cfe0ff" opacity="0.18" filter="url(#blur6)"/>')
        if random.random() < 0.07 * weight:
            nebulae.append(f'<ellipse cx="{x + g(width*0.5):.1f}" cy="{y + g(width*0.5):.1f}" rx="{random.uniform(6,14):.1f}" ry="{random.uniform(4,9):.1f}" fill="{random.choice(["#ff5fa8","#ff7ab8","#e04f9a"])}" opacity="{random.uniform(0.35,0.7):.2f}" filter="url(#blur2)"/>')
    pts = []
    for i in range(60):
        r = r_from + (r_to - r_from) * i / 59
        pts.append('%.1f,%.1f' % pt(r, spiral(offset, r) - 0.09))
    dust.append(f'<polyline points="{" ".join(pts)}" fill="none" stroke="#1a0e10" stroke-width="{6 * weight:.1f}" opacity="{0.6 * weight:.2f}" filter="url(#blur2)"/>')


for _, _, offset in ARMS:
    draw_arm(offset, R0 * 0.9, RING - 34, 1.0, 300, 12, 30)
# minor arms inside the major zones: fainter and shorter
draw_arm(ARMS[0][2] + math.radians(28), 150, 400, 0.45, 150, 8, 18)
draw_arm(ARMS[1][2] - math.radians(26), 170, 420, 0.45, 150, 8, 18)
draw_arm(ARMS[2][2] + math.radians(27), 160, 430, 0.45, 150, 8, 18)
draw_arm(ARMS[3][2] + math.radians(30), 150, 410, 0.45, 150, 8, 18)
# the Orion arm: a bright spur of its own between Sagittarius and Perseus
draw_arm(ORION_OFFSET, R_FORK - 25, RING - 40, 0.8, 200, 10, 22)

parts.append('<g filter="url(#blur14)">' + ''.join(glow) + '</g>')
parts.append(''.join(nebulae))
parts += stars
parts += dust

parts.append(f'<ellipse cx="{CX}" cy="{CY}" rx="120" ry="95" fill="url(#bulge)" opacity="0.45" filter="url(#blur14)"/>')
parts.append(f'<ellipse cx="{CX}" cy="{CY}" rx="125" ry="34" fill="url(#bulge)" transform="rotate({math.degrees(BAR):.0f} {CX} {CY})" filter="url(#blur6)"/>')
parts.append(f'<ellipse cx="{CX}" cy="{CY}" rx="95" ry="18" fill="#fff0c8" opacity="0.55" transform="rotate({math.degrees(BAR):.0f} {CX} {CY})" filter="url(#blur6)"/>')
for _ in range(1800):
    u, v = g(55), g(13)
    x = CX + u * math.cos(BAR) - v * math.sin(BAR)
    y = CY + u * math.sin(BAR) + v * math.cos(BAR)
    parts.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{random.choice([0.6,0.8,1.0])}" fill="{random.choice(["#fff3d0","#ffe0a0","#ffd090"])}" opacity="{random.uniform(0.4,1):.2f}"/>')
parts.append(f'<circle cx="{CX}" cy="{CY}" r="40" fill="#ffe7b5" opacity="0.45" filter="url(#blur14)"/>')
parts.append(f'<ellipse cx="{CX}" cy="{CY}" rx="26" ry="7" fill="none" stroke="#ffb15a" stroke-width="3" opacity="0.9" transform="rotate(-12 {CX} {CY})" filter="url(#blur2)"/>')
parts.append(f'<circle cx="{CX}" cy="{CY}" r="15" fill="url(#hole)"/>')
parts.append(f'<circle cx="{CX}" cy="{CY}" r="7" fill="#000"/>')

art = ''.join(parts)
render('Galaxy Map v2', W, H, art, defs)

# --- geometry -----------------------------------------------------------------------------------------
ring = [pt(RING, a * 2 * math.pi / 72) for a in range(73)]
borders = [ring, empire + [empire[0]], border_lines[0], border_lines[2], border_lines[3],
           border_lines["trunk"], border_lines["orion_left"], border_lines["orion_right"]]
geometry = {
    "arms": [{"id": aid, "title": title, "available": available, "polygon": [norm(p) for p in poly],
              "border": [], "label": norm(label_for(aid, offset))}
             for aid, title, available, poly, offset in zones],
    "borders": [{"points": [norm(p) for p in line]} for line in borders],
}
json.dump(geometry, open('Galaxy Map v2.json', 'w'), indent=1)
print('zones', [(z[0], len(z[3])) for z in zones], 'fork', round(R_FORK), 'starts', [round(s) for s in starts])

# preview with borders and labels, to check by eye
if '--preview' in sys.argv:
    over = []
    for line in borders:
        over.append('<polyline points="%s" fill="none" stroke="#36d9ff" stroke-opacity="0.6" stroke-width="2.5"/>' % ' '.join('%.1f,%.1f' % p for p in line))
    for aid, title, available, poly, offset in zones:
        x, y = label_for(aid, offset)
        over.append(f'<text x="{x:.0f}" y="{y:.0f}" fill="{"#36d9ff" if available else "#c9d2dc"}" font-family="Arial" font-weight="bold" font-size="{26 if available else 22}" text-anchor="middle" dominant-baseline="middle" stroke="#000" stroke-width="3" paint-order="stroke">{title}</text>')
    fill_zone = sys.argv[sys.argv.index('--preview') + 1] if len(sys.argv) > sys.argv.index('--preview') + 1 else 'orion'
    for aid, title, available, poly, offset in zones:
        if aid == fill_zone:
            over.insert(0, '<polygon points="%s" fill="#66d9ff" fill-opacity="0.18"/>' % ' '.join('%.1f,%.1f' % p for p in poly))
    render('preview_' + fill_zone, W, H, art + ''.join(over), defs)
