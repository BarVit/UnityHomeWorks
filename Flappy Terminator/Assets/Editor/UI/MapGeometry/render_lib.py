import subprocess, os, sys, random
CHROME = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome"
HERE = os.path.dirname(os.path.abspath(__file__))
def render(name, w, h, body, defs=''):
    svg = f'<svg width="{w}" height="{h}" viewBox="0 0 {w} {h}" xmlns="http://www.w3.org/2000/svg"><defs>{defs}</defs>{body}</svg>'
    html = os.path.join(HERE, name + '.html')
    open(html, 'w').write(f'<html><body style="margin:0;background:transparent;overflow:hidden">{svg}</body></html>')
    out = os.path.join(HERE, name + '.png')
    subprocess.run([CHROME, '--headless=new', '--disable-gpu', '--hide-scrollbars', '--default-background-color=00000000',
                    f'--window-size={w},{h}', f'--screenshot={out}', 'file://' + html], capture_output=True)
    print(name, os.path.getsize(out))

