# Builds the galaxy zones from hand-drawn borders.
# Input: "Galaxy Borders.json" (lines in 1920x1080 pixels, from the border editor).
# Output: "Galaxy Map v2.json" for MapScreenBuilder: zones (polygon, label point, caption, kind) and the border lines,
# normalised 0..1. Run: python zones_from_borders.py "Galaxy Borders.json" "Galaxy Map v2.json"
# Needs shapely (pip install shapely).
import json, sys
from shapely.geometry import LineString, Point
from shapely.ops import unary_union, polygonize_full, nearest_points, polylabel

W, H = 1920, 1080
SNAP = 4.0

# zones are recognised by a point inside them; voids are side areas for special missions
ZONES = [
    ("orion", "ORION ARM", "arm", True, (1316, 640)),
    ("scutum", "SCUTUM-CENTAURUS ARM", "arm", False, (710, 300)),
    ("sagittarius", "SAGITTARIUS ARM", "arm", False, (1150, 260)),
    ("perseus", "PERSEUS ARM", "arm", False, (1190, 700)),
    ("norma", "NORMA ARM", "arm", False, (600, 640)),
    ("empire", "EMPIRE", "core", False, (960, 470)),
    ("silent_void", "SILENT VOID", "void", False, (600, 260)),
    ("echo_void", "ECHO VOID", "void", False, (1330, 230)),
    ("null_void", "NULL VOID", "void", False, (640, 840)),
    ("ashen_void", "ASHEN VOID", "void", False, (1395, 680)),
]
LABELS = {"orion": (1305, 645), "empire": (960, 470), "scutum": (850, 170), "echo_void": (1180, 115),
          "null_void": (630, 850), "ashen_void": (1405, 735)}
CAPTIONS = {"ashen_void": "ASHEN\nVOID", "orion": "ORION\nARM"}


def norm(c):
    return {"x": round(c[0] / W, 5), "y": round(c[1] / H, 5)}


def main(source, target):
    data = json.load(open(source))
    lines = []
    for line in data["lines"]:
        points = [tuple(p) for p in line["points"]]
        if line.get("closed"):
            points.append(points[0])
        if len(set(points)) >= 2:
            lines.append(LineString(points))

    fixed = []
    for i, geom in enumerate(lines):
        others = unary_union([g for j, g in enumerate(lines) if j != i])
        coords = list(geom.coords)
        for k in (0, -1):
            p = Point(coords[k])
            if p.distance(others) <= SNAP:
                q = nearest_points(p, others)[1]
                coords[k] = (q.x, q.y)
        fixed.append(LineString(coords))

    faces, cuts, dangles, invalid = polygonize_full(unary_union(fixed))
    faces = list(faces.geoms)
    if len(faces) != len(ZONES):
        sys.exit("expected %d closed areas, got %d" % (len(ZONES), len(faces)))

    zones = []
    used = set()
    for zone_id, title, kind, available, probe in ZONES:
        hits = [i for i, f in enumerate(faces) if f.contains(Point(probe))]
        if len(hits) != 1 or hits[0] in used:
            sys.exit("zone %s: probe point %s is not inside exactly one free area" % (zone_id, probe))
        used.add(hits[0])
        face = faces[hits[0]].simplify(0.05)
        label = LABELS.get(zone_id)
        if label is None or not face.contains(Point(label)):
            pole = polylabel(face, tolerance=1.0)
            label = (pole.x, pole.y)
        ring = list(face.exterior.coords)[:-1]
        if label != LABELS.get(zone_id) and zone_id in LABELS:
            print("label of %s fell outside, moved to %s" % (zone_id, [round(v) for v in label]))
        zones.append({"id": zone_id, "title": title, "caption": CAPTIONS.get(zone_id, title), "kind": kind,
                      "available": available,
                      "polygon": [norm(c) for c in ring], "border": [], "label": norm(label)})

    geometry = {"arms": zones, "borders": [{"points": [norm(c) for c in g.coords]} for g in fixed]}
    json.dump(geometry, open(target, "w"), indent=1)
    print("zones", [(z["id"], len(z["polygon"])) for z in zones], "borders", len(fixed))


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
