# Код-ревью: Flappy Terminator

Unity 6000.3.7f1, URP, 2D Renderer. 42 скрипта, ~2341 строка в `Assets/Scripts`. Ветка `flappy-terminator/cleanup`, коммит `93ec4f0e`. Дата: 2026-09-13.

## Как это получено

17 независимых ревьюеров по осям → дедупликация → состязательная проверка каждой находки тремя скептиками с разными линзами (репродукция / семантика Unity / правило и область), задача каждого — **опровергнуть**; при сомнении находка считается опровергнутой → критик полноты и добор по названным им пробелам → синтез.

Всего 191 агент, 0 падений. Каждая находка ниже подтверждена чтением конкретных строк.

| | |
|---|---|
| Сырых находок | 159 |
| После дедупликации | 100 |
| **Подтверждено** | **27** |
| из них схлопнуто дублей после добора | 3 |
| Опровергнуто скептиками | 21 |
| Осталось непроверенными | 0 |
| Правил стиля с нарушениями | 19 |
| Рекомендаций (без доказанного дефекта) | 15 |

Распределение: **1 blocker**, **15 major**, **11 minor**. 10 из 27 требуют работы в редакторе Unity, а не только правки кода.

Находки из раунда добора по пробелам не проходили дедупликацию повторно, поэтому 3 пары описывали один и тот же дефект дважды — они схлопнуты, ссылки в плане правок пересчитаны.

---

## 1. Сводная таблица

| # | Серьёзность | Ось | Файл:строка | Суть |
|---:|---|---|---|---|
| 2 | blocker | Pause | `Assets/Scripts/Game.cs:99` | Escape is not gated on the settings screen: it resumes the game behind the settings overlay and Back then shows the p… |
| 26 | major | Добор по пробелу | `Assets/Scenes/SampleScene.unity:9359` ⚙️ | 'Game Over' is the second Canvas child, so the defeat screen renders beneath the HUD and beneath the Settings overlay… |
| 16 | major | Physics | `Assets/Scenes/SampleScene.unity:9605` ⚙️ | Top/bottom kill triggers sit ~4.5 units outside the camera, so the ship keeps playing off-screen and dies invisibly |
| 11 | major | SOLID: SRP and OCP | `Assets/Scripts/Game.cs:5` ⚙️ | SRP: Game is four classes in one (screen navigation, pause, level progression, outcome arbitration) held together by… |
| 15 | major | Initialization order | `Assets/Scripts/Game.cs:53` | Game.Start() disables PlayerMover before PlayerMover.Start() is guaranteed to have run, so the ship's start position… |
| 1 | major | Cutscene races | `Assets/Scripts/Game.cs:166` | Game.Resume() unconditionally re-enables player control, handing the ship back mid-warp-cutscene and after death - ro… |
| 7 | major | SOLID: SRP and OCP | `Assets/Scripts/Interfaces/IDamageable.cs:1` | IDamageable has zero consumers and bundles Die() with TakeDamage, forcing a public kill-switch on both ships that byp… |
| 10 | major | OOP and encapsulation | `Assets/Scripts/Level/WarpJump.cs:9` ⚙️ | WarpJump owns player state it should not - a second reference to the player's SpriteRenderer plus direct writes to th… |
| 4 | major | Pool lifecycle | `Assets/Scripts/Ships/Enemy/EnemyMover.cs:15` | Enemy flight speed is randomised once in Awake instead of OnSpawn, so pooled enemies keep the same speed for the whol… |
| 21 | major | SOLID: SRP and OCP | `Assets/Scripts/Ships/Enemy/EnemySpawner.cs:6` ⚙️ | SRP: EnemySpawner is a spawner, an object-pool warehouse for four things it does not spawn, and the game's victory co… |
| 27 | major | Добор по пробелу | `Assets/Scripts/Ships/Enemy/EnemySpawner.cs:10` ⚙️ | Enemy ships spawn inside the visible frame at the project's own default 16:9 - a third of each ship pops into existen… |
| 8 | major | OOP and encapsulation | `Assets/Scripts/Ships/Enemy/EnemySpawner.cs:29` | EnemySpawner builds its Awake-time pools by reaching several levels into the enemy prefab, forcing EnemyShip/EnemySho… |
| 17 | major | Physics | `Assets/Scripts/Ships/Enemy/EnemySpawner.cs:85` ⚙️ | A wave only clears once the slowest survivor crosses x=-14.1, five units past the left edge of the screen - up to ~10… |
| 20 | major | SOLID: SRP and OCP | `Assets/Scripts/Ships/Player/PlayerCollisionHandler.cs:32` ⚙️ | OCP: the player's collision reaction dispatches on concrete type, so one new hazard or pickup costs seven new members… |
| 22 | major | SOLID: SRP and OCP | `Assets/Scripts/Ships/Player/PlayerShip.cs:6` ⚙️ | SRP: PlayerShip mixes input routing, control-lock policy, damage relay, death presentation, collision reaction and re… |
| 3 | major | Unity serialization | `Assets/VFX/Sparks/Player ship explosion.prefab:59` ⚙️ | Particle Start Delays of 4-6 s in the player explosion prefab make PlayerShip.DeathDuration ~16 s, so the defeat scre… |
| 19 | minor | Audio | `Assets/Scripts/Ammo/ExplosionAnimation.cs:79` | An explosion's pooled lifetime is computed from particles and animation only, never from its own AudioSource clip, so… |
| 14 | minor | Restart and level transition | `Assets/Scripts/Game.cs:181` | LoseLevel stops the spawners but not the enemies already spawned, so ships keep flying and firing (with sound) across… |
| 12 | minor | Coroutines | `Assets/Scripts/Game.cs:202` | _isRunning is cleared inside the ShowGameOver coroutine, so the pause menu stays reachable for the whole death animat… |
| 24 | minor | OOP and encapsulation | `Assets/Scripts/Health/Health.cs:6` | Health.Value is serialized into the inspector but is always overwritten by MaxValue before anything reads it |
| 6 | minor | OOP and encapsulation | `Assets/Scripts/Level/WarpJump.cs:24` | WarpJump.Duration is public API with zero callers |
| 5 | minor | Coroutines | `Assets/Scripts/Level/WarpJump.cs:51` | WarpJump.Stop() does not stop the two ExpandingFade effects it started, and ExpandingFade has no stop path at all, so… |
| 25 | minor | Allocations and GC | `Assets/Scripts/Pools/SpawnPool.cs:73` | SpawnPool allocates two Action<IPoolable> delegates for every single spawn/release cycle, on every pooled object in t… |
| 18 | minor | Physics | `Assets/Scripts/Ships/Player/PlayerMover.cs:88` | _pushSpeed is overwritten by the Update clamp before physics ever integrates it, so values above _maxVerticalSpeed do… |
| 23 | minor | OOP and encapsulation | `Assets/Scripts/Ships/Player/PlayerShip.cs:87` | PlayerShip.EnableControl and PlayerShip.ReturnToStart are public with no caller outside the class |
| 13 | minor | Restart and level transition | `Assets/Scripts/Ships/Player/PlayerShip.cs:140` | Die() never zeroes Health, so an out-of-bounds death shows the defeat screen with a full health bar |
| 9 | minor | Coroutines | `Assets/Scripts/Ships/Player/PlayerShooter.cs:14` | PlayerShooter._shootDelayer is written twice and never read - dead coroutine handle, and off the project's coroutine-… |

⚙️ — правка затрагивает сцену или префаб, не только код.

---

## 2. Находки

### 2. Escape is not gated on the settings screen: it resumes the game behind the settings overlay and Back then shows the pause menu over a live game; StartLevel never hides the settings screen either

**blocker** · ось: Pause · `Assets/Scripts/Game.cs:99` · единогласно

**Сценарий воспроизведения.** 1. Start a level (Game.cs:143 sets _isRunning = true).
2. Press Escape. Pause() runs: timeScale 0, AudioListener.pause true, pause menu up.
3. Click Settings. OpenSettingsFromPause (Game.cs:82) hides the pause panel and shows the settings panel. Still paused, _isPaused == true.
4. Press Escape (the universal "go back" key, and the same key the player just used to open this menu). TogglePause sees _isRunning == true and _isPaused == true, so it calls Resume(): Time.timeScale = 1, AudioListener.pause = false, _playerShip.UnlockInput(), _pauseScreen.Hide() (a no-op, that panel is already hidden). The settings menu is still on screen and is the only UI the player is looking at, but the world is now simulating at full speed behind it: asteroids and enemy ships move, enemy bullets fly, the player's collider is live, and the player silently loses health and can die while dragging a volume slider. W and Space now fly and fire the ship the player cannot see.
5. Click "Back". CloseSettings (Game.cs:89) sees _isSettingsFromPause == true and calls _pauseScreen.Show() without going through Pause(). The pause menu is now displayed with Time.timeScale == 1, AudioListener.pause == false, _isPaused == false and player input unlocked: the game keeps playing behind the pause menu.
Variant at step 4: press Escape twice instead of once. The second Escape calls Pause(), whose _pauseScreen.Show() (Game.cs:158) never hides the settings panel, so the pause menu is drawn stacked on top of the still-open settings menu.
Compounded variant: die, then during the death countdown (Game.cs:197-204, _isRunning is still true) press Escape, click Settings, press Escape. The countdown unfreezes, ShowGameOver finishes and the defeat screen appears underneath the settings menu; clicking Back then puts the pause menu on top of the defeat screen with _isRunning == false, so Escape no longer does anything.

**Почему это дефект.** Not taste: the pause state (_isPaused / Time.timeScale / AudioListener.pause) and the pause UI are two independent pieces of state and this code lets them disagree. A menu that means "the game is stopped" is on screen while the world simulates and the run can be lost, with no input from the player that says "resume". Step 5 leaves a reachable, stable state where the pause menu is visible at timeScale 1. The single-responsibility problem is concrete: Game::CloseSettings sets UI visibility without going through the one method that owns the pause state.

**Код:**

```csharp
Assets/Scripts/Input/InputReader.cs:14-24
    14  private void Update()
    15  {
    ...
    22      if (Input.GetKeyDown(_pauseKey))
    23          PauseToggled?.Invoke();
    24  }

Assets/Scripts/Game.cs:99-108
    99  private void TogglePause()
   100  {
   101      if (_isRunning == false)
   102          return;
   103
   104      if (_isPaused)
   105          Resume();
   106      else
   107          Pause();
   108  }

Assets/Scripts/Game.cs:82-97
    82  private void OpenSettingsFromPause()
    83  {
    84      _isSettingsFromPause = true;
    85      _pauseScreen.Hide();
    86      _settingsScreen.Show();
    87  }
    88
    89  private void CloseSettings()
    90  {
    91      _settingsScreen.Hide();
    92
    93      if (_isSettingsFromPause)
    94          _pauseScreen.Show();
    95      else
    96          _startScreen.Show();
    97  }

Assets/Scripts/Game.cs:152-168
   152  private void Pause()
   153  {
   154      _isPaused = true;
   155      Time.timeScale = PausedTimeScale;
   156      AudioListener.pause = true;
   157      _playerShip.LockInput();
   158      _pauseScreen.Show();
   159  }
   160
   161  private void Resume()
   162  {
   163      _isPaused = false;
   164      Time.timeScale = NormalTimeScale;
   165      AudioListener.pause = false;
   166      _playerShip.UnlockInput();
   167      _pauseScreen.Hide();
   168  }

The two panels are distinct objects on the same Canvas, so Resume()'s _pauseScreen.Hide() cannot close the settings panel:
Assets/Scenes/SampleScene.unity:6818-6832 (PauseScreen &554024878)  _panel: {fileID: 818673099}
Assets/Scenes/SampleScene.unity:12850-12864 (SettingsScreen &279842321)  _panel: {fileID: 296470199}
Assets/Scenes/SampleScene.unity:8224-8238  GameObject &818673099  m_Name: Pause
Assets/Scenes/SampleScene.unity:12287-12301 GameObject &296470199  m_Name: Settings

Unity semantics verified in the docs (not assumed):
- Time.timeScale docs: "FixedUpdate functions and suspended Coroutines with WaitForSeconds are not called when timeScale is set to zero." Update is NOT in that list; Unity's own guidance is that Update keeps running and input keeps being polled at timeScale 0, which is why InputReader.Update:22 still fires PauseToggled while paused.
- ProjectSettings/ProjectSettings.asset:667  activeInputHandler: 0 (legacy Input Manager), so Input.GetKeyDown is live.
```

**Как чинить.** Make the pause state the single source of truth and gate the key. (a) Add a private bool _isSettingsOpen, set true in OpenSettingsFromStart and OpenSettingsFromPause and false in CloseSettings, and return early from TogglePause while it is set - or, better for the player, have TogglePause call CloseSettings() when the settings screen is open so Escape acts as "Back". (b) Change CloseSettings so the from-pause branch calls Pause() instead of _pauseScreen.Show(); Pause() already shows the pause panel, and routing through it makes it impossible for the pause panel to be visible while Time.timeScale == 1. (c) Have Pause() hide the settings screen (or rely on (a) so it can never be open when Pause runs), so the two panels can never stack.

**Риск правки.** Low. Code-only, confined to Game.cs, no serialized field added or removed, so no scene rewiring. The only behaviour change outside the bug is that Escape in the settings screen now either does nothing or acts as Back - both are what a player expects.

**Уточнения проверяющих.**

- Two details in the prose overstate the symptom without changing the defect. (1) "the player cannot see" the world: the Settings panel's first child is a full-screen Backdrop (&872292694, RectTransform 857861553 anchored 0,0-1,1, sizeDelta 0,0) whose Image is `m_Color: {r: 0, g: 0, b: 0, a: 0.75}` (SampleScene.unity:10153), so the running game is 75% dimmed behind the menu, not hidden — the player still takes damage and can die under a menu that means "paused", which is the actual defect. (2) There is no softlock in any traced variant: from the pause-menu-over-live-game state, Escape re-enters Pause() and the pause menu's own Resume/Restart buttons (Game.cs:39-40, 124-128) restore a consistent state; in the compounded death variant, PauseScreen's Restart -> RestartFromPause -> Resume() + RestartGame() clears both stacked panels. Also worth narrowing: the start-menu settings path is already safe — Game.cs:101's `_isRunning == false` early return makes Escape inert there, so the bug is specific to settings opened from pause. The "StartLevel never hides the settings screen" sub-claim holds as written (Game.cs:137-150) and is reachable only through the Escape bug: resume behind the settings overlay, clear the wave, and WarpJump.Finished -> StartNextLevel -> StartLevel begins the next level with the settings panel still on screen.
- Two details in the write-up are wrong and should be fixed, neither touching the core claim. (1) The "press Escape twice" variant says the pause menu is "drawn stacked on top of the still-open settings menu" — it is the other way round: the Canvas child list (SampleScene.unity:9356-9363) orders Pause (886339174) before Settings (898607871), so the settings panel renders on top of the re-shown pause panel. Both panels are simultaneously active either way. (2) "silently loses health" overstates the concealment: the settings panel's Backdrop (GameObject 872292694, Image &294827197) is a full-screen black image at alpha 0.75, so the world is heavily dimmed but still faintly visible behind the menu.
- Core claim stands; two details need fixing. (1) Z-order is inverted in the "press Escape twice" variant: the Canvas children list (SampleScene.unity:9357-9365) is ... Pause (886339174) then Settings (898607871) last, so the settings panel renders ON TOP of the pause panel - the pause menu appears underneath the still-open settings panel, not stacked on top of it. The substance (two menus open at once) is unaffected. (2) Severity is major, not blocker: the state is recoverable and nothing is corrupted - a further Escape calls Pause() and re-freezes correctly, the pause panel's Resume/Restart buttons still work, and Time.timeScale is never left stuck. The worst outcome is a run silently lost while the player sits in the settings menu, which is wrong behaviour rather than broken or corrupted play. The "StartLevel never hides the settings screen" sub-claim is literally true (Game.cs:137-150 hides only _gameOverScreen) but is only reachable as a downstream consequence of the same Escape desync, so it belongs inside this finding rather than standing as evidence of a second defect.

<sub>Слито из 4 формулировок других ревьюеров.</sub>

---

### 26. 'Game Over' is the second Canvas child, so the defeat screen renders beneath the HUD and beneath the Settings overlay that can still be open when the player dies

**major** · ось: Добор по пробелу · `Assets/Scenes/SampleScene.unity:9359` · требует редактора Unity · единогласно

**Сценарий воспроизведения.** (a) The always-reproducible half. Press Start, fly, let the ship die. Game.LoseLevel -> ShowGameOver (Game.cs:175-204) calls _gameOverScreen.Show(), which activates 'Game Over' and its 72%-black full-screen Backdrop. Because 'Game Over' is child index 1 and 'Health Bar', 'Level' and 'Shoot Cooldown' are indices 2, 3 and 4, those three HUD widgets draw ON TOP of the dimming backdrop at full brightness, while the score at index 0 is correctly dimmed underneath it. The result is a half-dimmed HUD: the score darkens, the health bar and LEVEL label sitting directly beside it do not.

(b) The open half of the confirmed Escape-on-settings defect. Press Escape to pause (Pause panel shows, Time.timeScale = 0). Click Settings: Game.OpenSettingsFromPause (Game.cs:82-87) hides Pause and shows Settings. Press Escape again: Game.TogglePause (Game.cs:99-108) sees _isRunning true and _isPaused true and calls Resume (Game.cs:161-168), which restores timeScale, unlocks the ship and hides the Pause panel but never hides Settings — so the game is now running live behind the settings overlay. Fly blind until the ship dies. Game.ShowGameOver calls _gameOverScreen.Show(_killCounter.Kills) with 'Settings' still active. 'Game Over' is child index 1 and 'Settings' is child index 7, so the settings overlay draws completely over the defeat screen: the player sees the MASTER/MUSIC/EFFECTS sliders, not the defeat screen, and the settings Backdrop's m_RaycastTarget: 1 (line 10154) also swallows every click aimed at the defeat screen's 'Restart Button' underneath it. The only reachable control is the settings Back button, which fires CloseSettings (Game.cs:89-97); since _isSettingsFromPause is still true it then shows the Pause panel — child index 6, still above 'Game Over' — so the defeat screen stays buried a second time and the player has to click Resume on a pause menu for a game that is already over before the defeat screen finally becomes visible. So the answer to which panel draws over which: Settings > Pause > Start Menu > Shoot Cooldown > Level > Health Bar > Game Over > score. The defeat screen is beneath every other panel in the Canvas.

**Почему это дефект.** The Unity documentation makes the draw order deterministic and non-negotiable: first child drawn first, later siblings on top. A full-screen 72%-black backdrop whose stated job is to dim the play field is therefore doing that job for exactly one of the four HUD widgets and being painted over by the other three, which is visibly inconsistent rather than a style choice. And placing the defeat screen below the settings overlay is what turns the already-confirmed Escape-while-settings-open state bug from 'the game resumes invisibly' into 'the player cannot see or click the defeat screen at all' — the Restart button, the one control on that screen, is covered by a raycast-blocking backdrop. This is the scene-side half of that blocker and it is fixed in a different place from the code-side half.

**Код:**

```csharp
Canvas RectTransform &1229452746 (the Canvas GameObject &1229452742), m_Children in hierarchy order — index 0 is drawn first, index 7 last and therefore on top:
  9357	  m_Children:
  9358	  - {fileID: 656648042}   -> GameObject &656648041 'Text (TMP)'      (score, m_Name at 7269, m_IsActive: 1 at 7274)
  9359	  - {fileID: 607739895}   -> GameObject &260790321 'Game Over'       (m_Name at 749, m_IsActive: 0 at 754)
  9360	  - {fileID: 414378552}   -> GameObject &653257116 'Health Bar'      (m_Name at 7251, m_IsActive: 1 at 7256)
  9361	  - {fileID: 817881944}   -> GameObject &965806047 'Level'           (m_Name at 8972)
  9362	  - {fileID: 220985581}   -> GameObject &434668534 'Shoot Cooldown'  (m_Name at 6284, m_IsActive: 1 at 6289)
  9363	  - {fileID: 345113249}   -> GameObject &452419521 'Start Menu'      (m_Name at 6319)
  9364	  - {fileID: 886339174}   -> GameObject &818673099 'Pause'           (m_Name at 8234)
  9365	  - {fileID: 898607871}   -> GameObject &296470199 'Settings'        (m_Name at 12297)
These are exactly the objects the four screen scripts toggle (each _panel is the top-level Canvas child itself):
  6938	  _panel: {fileID: 260790321}   (GameOverScreen, m_Script guid 49b40b29... at 6935)
  8610	  _panel: {fileID: 452419521}   (StartScreen, guid 0e911e13... at 8607)
  6829	  _panel: {fileID: 818673099}   (PauseScreen, guid 921e9c56... at 6826)
  12861	  _panel: {fileID: 296470199}   (SettingsScreen, guid 6893c1ec... at 12858)
'Game Over' is a full-screen stretch (7049-7052: m_AnchorMin {0,0}, m_AnchorMax {1,1}, m_SizeDelta {0,0}) whose first child &681352173 is 'Backdrop' (GameObject &427282941, m_Name at 6268), also full-screen stretch, carrying Image &195992348:
  360	  m_Color: {r: 0, g: 0, b: 0, a: 0.72}
  361	  m_RaycastTarget: 1
'Settings' is the same shape (12328-12331 full-screen stretch) with its own full-screen 'Backdrop' &857861553 (GameObject &872292694, m_Name at 10108) carrying Image:
  10153	  m_Color: {r: 0, g: 0, b: 0, a: 0.75}
  10154	  m_RaycastTarget: 1
Nothing ever hides the HUD: grep 'SetActive' over Assets/Scripts returns only GameOverScreen.cs:27/32, PauseScreen.cs:32/37, SettingsScreen.cs:37/42, StartScreen.cs:28/33 and SpawnPool.cs:55/65/79 — no call touches 'Text (TMP)', 'Health Bar', 'Level' or 'Shoot Cooldown', and Game.cs never references them.
There is exactly one Canvas in the scene (grep '--- !u!223 &' -> 9323 only), no child canvases and no sorting override (grep 'm_OverrideSorting' -> 9337 only, value 0), so hierarchy order is the only thing deciding what draws over what.
Unity docs, Canvas (com.unity.ugui@2.0 manual/UICanvas.html): "UI elements in the Canvas are drawn in the same order they appear in the Hierarchy. The first child is drawn first, the second child next, and so on." Screen Space - Overlay "places UI elements on the screen rendered on top of the scene."
```

**Как чинить.** Reorder the Canvas children in the Hierarchy so the four full-screen overlay panels all sit after the four HUD widgets, and so 'Game Over' is last: Text (TMP), Health Bar, Level, Shoot Cooldown, Start Menu, Pause, Settings, Game Over. Drag 'Game Over' to the bottom of the Canvas in the Hierarchy window (or right-click it and use Transform > Set As Last Sibling). That single move makes the defeat backdrop dim the whole HUD uniformly and puts the defeat screen above the settings and pause overlays instead of beneath them. The state bug that lets Settings stay open while the game runs is a separate defect in Game.TogglePause/Resume and must be fixed there as well — reordering only stops the defeat screen from being invisible when it happens.

**Риск правки.** Low, and editor-only: reordering children of a Screen Space - Overlay canvas changes nothing but render and raycast order. Putting 'Game Over' last also puts it above 'Pause' and 'Start Menu', which is correct for every legal flow — Game.StartLevel hides the defeat panel (Game.cs:144) before any level begins, and once it is shown _isRunning is false (Game.cs:202) so Escape can no longer open the pause menu over it. Verify afterwards that the defeat screen still disappears on Restart, since it is now the topmost panel and any failure to hide it would be immediately obvious rather than hidden behind another overlay.

**Уточнения проверяющих.**

- One overstatement worth trimming: at alpha 0.75 the defeat screen is heavily dimmed behind the Settings overlay, not literally invisible - the player can make out Game Over through the backdrop. The load-bearing half is unchanged and correct: the Settings backdrop's m_RaycastTarget: 1 (SampleScene.unity:10154) covers the full screen and swallows clicks aimed at the Restart Button underneath, so the one control on the defeat screen is unusable until the player backs out through Settings and then Resume on the pause menu.
- Finding stands as written; two scope tightenings for accuracy. (1) The always-reproducible half (a) is cosmetic only - the health bar, level label and cooldown bar render at full brightness over the 72% dim while the score is dimmed. I checked whether the undimmed HUD also blocks the defeat screen's Restart Button and it does not: the Restart Button is centre-anchored at (0,-150) size 320x96 (SampleScene.unity:6812-6815) while Shoot Cooldown is bottom-anchored at (0,118) size 220x14 (585-588), so at normal window heights they never overlap. (2) The severe half (b) - defeat screen invisible and its only button unclickable - is gated on the separate Game.Resume/TogglePause state bug (Game.cs:99-108, 161-168); fixing that code defect removes half (b) entirely and leaves only the cosmetic ordering issue, so this should be reported as the scene-side aggravator of that bug rather than as an independent second blocker. Also minor imprecision in the write-up: while Settings is buried over the defeat screen the sliders are reachable too, not just the Back button.
- The finding is correct as stated; one detail is understated rather than wrong. Claim (a) describes the HUD as merely failing to be dimmed by the backdrop, but the overlap reaches the defeat panel's own body: the 'Window' RT 722619767 is centred (0,-27) at 780x524 (SampleScene.unity:7714-7717) with a near-opaque Image 855511673, m_Color {r:0.1,g:0.16,b:0.26,a:0.97} (SampleScene.unity:8350), so it spans x[-390,390], y[-289,235]. The 'Level' label at anchoredPosition (-440,210), sizeDelta 320x40 (SampleScene.unity:8221-8222) spans x[-600,-280], y[190,230] and therefore prints on top of that opaque defeat panel in x[-390,-280], not just on top of the dimmer. 'Health Bar' (-440,252, 320x34; SampleScene.unity:6169-6170) grazes the Window's top edge at y=235. Also note 'Shoot Cooldown' is bottom-anchored (SampleScene.unity:585-588) and the CanvasScaler is Constant Pixel Size (m_UiScaleMode: 0, SampleScene.unity:9312), so whether it too lands inside the Window varies with screen height — that resolution dependence belongs to the separate CanvasScaler question, not to this finding.

---

### 16. Top/bottom kill triggers sit ~4.5 units outside the camera, so the ship keeps playing off-screen and dies invisibly

**major** · ось: Physics · `Assets/Scenes/SampleScene.unity:9605` · требует редактора Unity · единогласно

**Сценарий воспроизведения.** Start a level and stop tapping W. The ship sinks; its sprite is fully off the bottom of the screen once its centre passes y = -5.68, but the kill trigger only reaches it at centre y = -8.82. At the clamped fall speed of 3 u/s that is 1.05 s of an empty-looking screen with the ship still alive and controllable. The explosion then spawns at y ~ -8.8, off camera, and ExplosionAnimation.cs:64 also suppresses its sound because ScreenArea.Contains is false, so the death has no visual and no audio at all - the player only learns they died when the defeat screen appears DeathDuration seconds later. The mirror case is worse: tapping W lifts the ship to centre y ~ 8.8 before it dies, so there is a band roughly y in 7.2 to 8.5 that is 2+ units above the top of the screen, invisible, where the ship is alive and unreachable - enemies spawn at y in [-3,3] (SampleScene.unity:9242-9243) and the largest asteroid spawned at y=5.3 reaches only y~6.2 (Asteroid 2.prefab polygon half-height 0.61, half-width 0.71). Park there and the wave cannot touch you.

**Почему это дефект.** The kill volume is a gameplay boundary, not a garbage-collection boundary, and it is 4.5 units outside the visible area on both axes. Two concrete consequences follow, neither of them taste: the player's own death is invisible and silent for over a second, and there is an off-screen safe zone in which the hazard set cannot reach the ship. The one-point ScreenArea test the project already uses for audio shows the author knew objects live off-camera; the bounds were simply never brought in to match.

**Код:**

```csharp
Camera (SampleScene.unity:6683 `--- !u!20 &519420031`, 6718 `orthographic size: 5`, transform at (0,0,-10)) => visible y in [-5, +5].
ObjectRemover bottom (SampleScene.unity:9596 `--- !u!4 &1654201612`, 9604 `m_LocalRotation: {x: -0, y: -0, z: 0.7071068, w: 0.7071068}`, 9605 `m_LocalPosition: {x: 0, y: -10, z: 0}`, 9606 `m_LocalScale: {x: 1, y: 31, z: 1}`, BoxCollider2D `m_Size: {x: 1, y: 1}`, `m_IsTrigger: 1`) => world box x in [-15.5,15.5], y in [-10.5,-9.5].
ObjectRemover top (SampleScene.unity:8056/8065 `m_LocalPosition: {x: 0, y: 10, z: 0}`, 8066 scale {1,31,1}) => y in [9.5,10.5].
ObjectRemover.cs:5-10 `private void OnTriggerEnter2D(Collider2D other) { IRemoveable removeable = other.GetComponentInParent<IRemoveable>(); removeable?.Remove(); }` and PlayerShip.cs:170-173 `public void Remove() { Die(); }`.
Player Ship.prefab:90 `m_GravityScale: 0.2`, :213 `_maxVerticalSpeed: 3`, :211 `_tapForce: 2`; body collider on child "Body sprite" (localScale 4,4,1, rotated -90deg) has vertical half-extent ~0.68.
ExplosionAnimation.cs:64 `if (ScreenArea.Contains(_camera, transform.position))` gates the explosion sound.
```

**Как чинить.** Move ObjectRemover top/bottom in so their inner faces sit just outside the ship's silhouette - about y = +6 and y = -6 with the same 1-unit thickness, i.e. localPosition y = +/-6.5. Keep left/right where they are for bullets/asteroids, or split the player's boundary from the cleanup boundary: give the player its own thin pair of triggers at +/-5.7 and leave the wide ones for pooled debris. Either way the ship should die (and explode, audibly) at the moment it leaves the screen.

**Риск правки.** Low. Moving the top/bottom removers in does not affect bullets or enemies, which travel horizontally and never approach y=+/-6. The only care needed is that the new boundary must clear the asteroid lanes (asteroid centres go to y=+/-5.3, reach +/-6.24 when rotated), so +/-6.5 is the tightest safe value; anything tighter would start deleting asteroids mid-flight.

**Уточнения проверяющих.**

- One numeric refinement, not a refutation. The cited thresholds (-5.68 invisible, -8.82 death, 1.05 s) assume an unrotated ship. PlayerMover.cs:36 lerps the ship toward `_minRotation` = -45deg (Player Ship.prefab:215 `_minRotationZ: -45`) while it falls, and at ~-45deg the sprite/collider AABB half-height grows from ~0.68 to ~0.9-1.0. The real numbers are therefore: sprite fully off the bottom at centre y ~ -6.0, kill trigger at centre y ~ -8.6, i.e. ~2.6 units / ~0.87 s of alive, invisible, controllable flight rather than 3.14 units / 1.05 s. The "~4.5 units outside the camera" headline figure is exact. Also soften "no visual at all": the ship sprite is genuinely gone (PlayerShip.cs:152 `_shipSprite.enabled = false`) and the explosion spawns ~3.6 units below the visible edge, but Player ship explosion.prefab declares startSpeed up to 5 and startLifetime 5, so stray fast particles may spray back into frame; the death sound is unambiguously suppressed (ExplosionAnimation.cs:64).
- The defect is real but the thresholds are computed from an untilted hull and should be restated. PlayerMover.cs:36 continuously lerps the ship toward -45 deg and :44 snaps it to +45 deg on each tap, so the tilted sprite's bounding half-height is up to ~1.02 (not 0.68) while the narrow polygon collider's vertical half-extent varies only between ~0.57 and ~0.70. Corrected bottom case: the sprite is fully below the view once the centre passes y ~= -6.02, and the collider only reaches the bottom remover's top face at centre y ~= -8.90, so the invisible-but-alive fall is ~2.9 units, about 0.96 s at the clamped 3 u/s (not 3.14 units / 1.05 s). Corrected top case: fully hidden above centre y ~= +6.02, killed at centre y ~= +8.8, and unreachable by hazards above centre y ~= +6.94 (highest asteroid top edge ~6.24 including spin, plus the ship's ~0.70 lower extent) - an off-screen safe band roughly y in 6.9 to 8.8, about 1.9 units wide rather than 7.2 to 8.5. The headline claim - kill triggers 4.5 units outside the camera, producing a ~1 s invisible and silent death and an off-screen parking spot that clears a wave untouched - is unaffected.
- Narrow the finding to the bottom boundary and to death feedback; drop the "off-screen safe zone" argument. Corrected claim: the bottom kill trigger's inner face (world y = -9.5, SampleScene.unity:9596-9612) sits 3.82 units below the point at which the ship's sprite has fully left the screen (centre y = -5.68, from visible bottom -5 plus the 0.68 half-extent computed from Player Ship.prefab:249 and :250). Reproduction: start a level and stop tapping; at the clamped 3 u/s fall speed (PlayerMover.cs:37-38, Player Ship.prefab:213) the ship remains alive and controllable for ~1.05 s after it is no longer drawn, and dies at centre y = -8.82, where the explosion's particles are off-camera and ExplosionAnimation.cs:64 additionally mutes its AudioSource because ScreenArea.Contains is false. Game.cs:199 then waits DeathDuration before the defeat screen. The consequence is that the explosion cutscene the project implements is neither seen nor heard on the most common death in a Flappy-style game, and the player makes recovery decisions about a ship they cannot see. Drop the top-boundary claims: "outliving a wave" is an explicitly sanctioned win condition per the brief, leaving the top of the screen is genre-standard for this game, and y~8 is an unstable band under the 8.818 death line, not a safe park. The proportionate fix is the first one offered - bring the bottom remover in so its inner face sits just outside the ship's silhouette (localPosition y ~ -6.5 with the same 1-unit thickness), leaving the left/right removers as the wide cleanup frame for pooled bullets and asteroids. The second proposal (a separate pair of player-only boundary triggers) is more machinery than a project this size needs.

---

### 11. SRP: Game is four classes in one (screen navigation, pause, level progression, outcome arbitration) held together by four overlapping boolean state fields

**major** · ось: SOLID: SRP and OCP · `Assets/Scripts/Game.cs:5` · требует редактора Unity · с особым мнением

**Нарушенное правило.** Violated rule: Single Responsibility Principle — a class has exactly one reason to change. Game has four: change the difficulty curve and you edit Game; change what pause freezes and you edit Game; add or reorder a menu screen and you edit Game; change the win/lose rules and you edit Game. Four unrelated teams of state (_isPaused, _isLevelOver/_isRunning, _level, _isSettingsFromPause) live in one 214-line file, and they interact by accident — finding #1 in this report is exactly that accident (the pause concern writing into the outcome concern's control state).
What it should look like: Game keeps only the match: _playerShip, _enemySpawner, _asteroidSpawner, _killCounter, _warpJump, StartLevel, WinLevel, LoseLevel, ShowGameOver — roughly 80 lines.

**Почему это дефект.** This is not a preference about file size. Each of the four groups has a separate, nameable trigger for change, and each owns private state the other three must not touch. The evidence that they already interfere is concrete and in this report: Resume() (pause concern) overwrites _isControlEnabled after Die()/WarpJump (outcome concern) cleared it, and TogglePause consults _isRunning (outcome concern) to decide whether pause is legal.

**Код:**

```csharp
Reason to change #1, screen navigation: fields `_startScreen/_pauseScreen/_settingsScreen/_gameOverScreen` (Game.cs:17-20), the `_isSettingsFromPause` flag (29), the initial screen state in Start (54-57), and OpenSettingsFromStart / OpenSettingsFromPause / CloseSettings (75-97).
Reason #2, pause: `NormalTimeScale`/`PausedTimeScale` (8-9), `_input` (11), `_isPaused` (28), TogglePause (99-108), Pause (152-159), Resume (161-168), and the Time.timeScale / AudioListener.pause writes at 50-51, 155-156, 164-165.
Reason #3, level and difficulty progression: `FirstLevel` (7), `_firstWaveSize`/`_waveSizeGrowth` (21-22), `_level` (25), StartNextLevel (130-135), GetWaveSize (170-173), `LevelChanged` (31, 149).
Reason #4, match-outcome arbitration: `_gameOverCountdown` (24), `_isLevelOver` (26), `_isRunning` (27), LoseLevel (175-184), WinLevel (186-195), ShowGameOver (197-204), StopGameOverCountdown (206-213).
The OnEnable/OnDisable pair (33-46, 60-73) subscribes eleven events spanning all four concerns.
```

**Как чинить.** Split into three new MonoBehaviours plus a slimmed Game, with Game remaining the composition root that subscribes to them in OnEnable/OnDisable exactly as it does now.
(a) ScreenNavigator: takes over Game.cs:17-20, 29, 54-57 and 75-97. It holds the four screens, keeps `Screen _current` and `Screen _previous` (a real reference, not the _isSettingsFromPause bool), and exposes ShowStart(), ShowSettings(), ShowGameOver(int), Back(), HideAll(), re-raising StartRequested / RestartRequested / ResumeRequested for Game.
(b) PauseController: takes over Game.cs:8-9, 11, 28, 99-108 and 152-168. It owns _input, _isPaused, Time.timeScale and AudioListener.pause, raises Paused/Resumed, and (per finding #1) gates gameplay input in InputReader rather than in PlayerShip. It asks Game (or a shared `IsMatchRunning` property) whether pausing is currently allowed.
(c) LevelProgression: takes over Game.cs:7, 21-22, 25, 132 and 170-173. Plain serialized MonoBehaviour with `int Current`, `int WaveSize`, `Restart()`, `Advance()` and an `event Action<int> Changed`. LevelViewer.cs:6/11/15 then subscribes to LevelProgression.Changed instead of Game.LevelChanged, and Game.LevelChanged (Game.cs:31) disappears.
Wiring afterwards: Game keeps serialized references to the three new components and to the gameplay objects; StartLevel (137-150) becomes `_screens.HideAll(); _warpJump.Stop(); _playerShip.Restart(); _enemySpawner.StartWave(_levels.WaveSize); _asteroidSpawner.StartSpawn();`. All three new components sit on the same GameObject as Game, so the inspector wiring is a drag-and-drop of the existing references.

**Риск правки.** Medium. The split itself is mechanical, but the eleven event subscriptions in OnEnable/OnDisable (33-46, 60-73) must be redistributed without dropping one, and the serialized references on the Game GameObject have to be re-assigned in the scene. Do it in three separate steps (screens, then pause, then levels), checking start → play → pause → settings → win → warp → next level → die → restart after each.

**Особое мнение скептика.** Read Game.cs:1-214 in full, plus StartScreen.cs:1-45, PauseScreen.cs:1-54, SettingsScreen.cs:1-49, GameOverScreen.cs:1-39, LevelViewer.cs:1-23, InputReader.cs:1-24, KillCounter.cs:1-39, PlayerShip.cs:1-179, WarpJump.cs:1-143, EnemySpawner.cs:1-105, and both exemplar files with `diff` of before/after. The line citations are all accurate, but the SRP conclusion does not follow.

(1) Game implements none of the four "responsibilities" - it delegates all of them, and the delegates already exist as separate classes. Screen navigation: each screen owns its panel, its buttons and its own Show/Hide (StartScreen.cs:26-34, PauseScreen.cs:30-38, SettingsScreen.cs:32-43, GameOverScreen.cs:24-33); Game only picks which one (three methods, 3-5 lines each, Game.cs:75-97). Pause: the actual work is four lines (Game.cs:155-156, 164-165) plus a delegated LockInput/UnlockInput. Level progression: literally `_level++` (Game.cs:132) and one arithmetic expression (Game.cs:172). Outcome: stop two spawners and hand off to WarpJump.Play() or a countdown (Game.cs:175-195), where the cutscene itself is all in WarpJump.cs:62-91 and wave tracking all in EnemySpawner.cs:78-87. Of the 214 lines, 41 are the OnEnable/OnDisable subscribe/unsubscribe pair (Game.cs:33-73); no method body exceeds 9 statements (StartLevel, Game.cs:137-150). That is a composition root wiring eleven events, which is what a composition root is for - the finding itself concedes Game must stay "the composition root that subscribes to them in OnEnable/OnDisable exactly as it does now".

(2) Under Martin's actual formulation of SRP ("a module should be responsible to one, and only one, actor"), there is one actor here - the person tuning game flow - and the four named triggers are that same person doing the same job. "Four nameable triggers" is not a violation test; by it, every orchestrator in every game is a violation.

(3) The "evidence that they already interfere" does not support this finding. Resume() -> _playerShip.UnlockInput() (Game.cs:166) collides with Die() -> DisableControl() (PlayerShip.cs:151) because PlayerShip exposes two overlapping public pairs over the SAME field: `_isControlEnabled` (PlayerShip.cs:23) is written by LockInput/UnlockInput (77-85) and again by EnableControl/DisableControl (87-101), where only the latter also touches `_playerMover.enabled` and `_bodyCollider.enabled`. That defect lives in PlayerShip's API and survives the proposed split unchanged - a PauseController would call UnlockInput() on resume exactly as Game does.

(4) The proposed fix does not remove the coupling, it publishes it, and as written it cannot compile. The proposal concedes PauseController "asks Game (or a shared `IsMatchRunning` property)" - so `_isRunning` (Game.cs:27), today a private field read on one line (101), becomes public API and Game<->PauseController becomes bidirectional. ScreenNavigator's `Screen _current`/`Screen _previous` requires a base type that does not exist: the four screens are unrelated MonoBehaviours with incompatible signatures (GameOverScreen.Show(int score) at GameOverScreen.cs:24 vs StartScreen.Show() at StartScreen.cs:26), so the fix silently demands a fifth refactor of all four screen classes - and `grep -rn "^namespace" Assets/Scripts` returns nothing, so a global-namespace type named `Screen` would shadow UnityEngine.Screen in every file.

(5) Disproportionate for the size. LevelProgression would be a whole MonoBehaviour for two serialized ints, one counter and the expression at Game.cs:172, in a 2341-line student project. The graded exemplar sets no decomposition rule at all: BulletShooter.cs and PlacesMover.cs are ~30 lines each and the only graded change between before and after is the rename GetNextPlace -> GetNextPosition (verified by diff of ../Code style/Assets vs ../Code style genius). Nothing in the school material fails a 214-line orchestrator, so "major - a rule the graders will fail" is unsupported.

**Уточнения проверяющих.**

- Drop the OCP half of the axis; the axis should read "SOLID: SRP" only. Nothing in the evidence, the stated rule, or the proposed fix argues an open/closed violation — there is no abstraction in Game.cs that a new screen, level rule, or outcome would have to modify rather than extend, and the whole case rests on the four reasons-to-change. Also note the fix sketch's LevelProgression list is slightly incomplete: _level is additionally written at Game.cs:118 (RestartGame) and read at Game.cs:149, so those two lines move with it as well.
- Four corrections to the finding's wording and its proposed fix; none touch the substance.

1. File it as SRP only. The axis says "SOLID: SRP and OCP" but the finding presents no OCP evidence anywhere — no closed abstraction, no extension point, no switch over types. The OCP half is asserted and never argued; drop it.

2. "four overlapping boolean state fields" is imprecise. _isLevelOver and _isRunning (Game.cs:26-27) belong to the SAME concern (outcome) and are the only pair that genuinely overlap; _isPaused (28) and _isSettingsFromPause (29) each stand alone, and the fourth concern, level progression, is carried by an int (_level, Game.cs:25), not a bool. Accurate phrasing: "four disjoint groups of state — two redundant outcome booleans, a pause boolean, a navigation boolean and a level int."

3. Proposed-fix hazard: do not name the base type `Screen`. A global-namespace `class Screen` shadows `UnityEngine.Screen` in every file carrying `using UnityEngine;`, because C# resolves enclosing-namespace types before using-directive imports. It is currently harmless — grep over Assets/Scripts finds no `Screen.` static usage — but it is a live trap the moment anyone writes Screen.width. Call it `GameScreen` or `UIScreen`.

4. Two small errors in the fix sketch: the LevelViewer citation "15" should be 16 (LevelViewer.cs:16 is `_game.LevelChanged -= Show;`; line 15 is a brace), and "(b) PauseController ... asks Game ... whether pausing is allowed" reintroduces a Game <-> PauseController cycle. Better: Game sets a `pauseController.IsEnabled` flag when a match starts and ends, so the dependency stays one-way.
- Nothing about Game's SRP survives. The only real defect anywhere near this evidence is not in Game.cs at all: PlayerShip exposes LockInput/UnlockInput (PlayerShip.cs:77-85) and EnableControl/DisableControl (87-101) writing one shared `_isControlEnabled` field (23), so an outside caller can re-enable input while `_playerMover.enabled` and `_bodyCollider.enabled` stay false - which is finding #1's territory, one file and one field, not a four-way split of Game.

<sub>Слито из 1 формулировок других ревьюеров.</sub>

---

### 15. Game.Start() disables PlayerMover before PlayerMover.Start() is guaranteed to have run, so the ship's start position and its rotation limits are left to undefined ordering

**major** · ось: Initialization order · `Assets/Scripts/Game.cs:53` · единогласно

**Сценарий воспроизведения.** Press Play and look at the ship through the 72%-opaque start-menu backdrop before clicking anything.
- If Unity happens to call PlayerMover.Start() first, it runs ReturnToStart() and the ship sits at _startPosition (-6, 0); Game.Start() then freezes it there (bodyType Kinematic).
- If Unity happens to call Game.Start() first, line 53 sets _playerMover.enabled = false while PlayerMover.Start() is still pending. Unity then skips it (Start only fires "on the frame when a script is enabled"), so ReturnToStart() never runs and the ship sits at its authored scene position (-2.88, -2.7) - roughly 3 units right and 2.7 units down from where it belongs - until the player clicks START.
Which of the two you get is not fixed by anything in the project, and can flip when the scene is re-saved or objects are re-ordered.
Second consequence of the same ordering, same run: in that second case _minRotation and _maxRotation stay default(Quaternion) = (0,0,0,0) until the frame AFTER the player clicks START (Start for a newly enabled component runs in the next frame's initialization phase, before FixedUpdate/Update). If the player presses W in the same frame they click START, PlayerShip.OnJumped -> PlayerMover.Jump() executes `transform.rotation = _maxRotation` with the zero quaternion and Unity logs "transform.rotation assign attempt for 'Player Ship' is not valid."

**Почему это дефект.** This is not taste: two components on different GameObjects have an ordering dependency (PlayerMover.Start must run before Game.Start disables it), the Unity manual explicitly states that order is not guaranteed, and the project pins no script execution order. The two orders produce two different on-screen states, so the code's behaviour is unspecified by construction. The axis rule the graders teach - never let one object's Start read or mutate another object's not-yet-initialized state - is violated directly.

**Код:**

```csharp
Assets/Scripts/Game.cs:48-58
    48  private void Start()
    49  {
    50      Time.timeScale = NormalTimeScale;
    51      AudioListener.pause = false;
    52
    53      _playerShip.DisableControl();

Assets/Scripts/Ships/Player/PlayerShip.cs:95-101
    95  public void DisableControl()
    96  {
    97      LockInput();
    98      _playerMover.Stop();
    99      _playerMover.enabled = false;
   100      _bodyCollider.enabled = false;
   101  }

Assets/Scripts/Ships/Player/PlayerMover.cs:26-32
    26  private void Start()
    27  {
    28      _maxRotation = Quaternion.Euler(0, 0, _maxRotationZ);
    29      _minRotation = Quaternion.Euler(0, 0, _minRotationZ);
    30
    31      ReturnToStart();
    32  }

`Game` is on scene GameObject 'Game' (&165193575); `PlayerMover` is on the 'Player Ship' prefab instance (&2935624913016190949) - two different GameObjects.
No execution order is pinned anywhere: `grep -rn executionOrder Assets/Scripts | grep -v 'executionOrder: 0'` returns nothing, and there is no [DefaultExecutionOrder] in the project.
Assets/Prefabs/Player Ship.prefab:217  _startPosition: {x: -6, y: 0}
Assets/Prefabs/Player Ship.prefab (PlayerMover &3651529538033820116) m_Enabled: 1  -> enabled at load, so its Start is pending.
SampleScene.unity PrefabInstance &2935624913016190949 modification: m_LocalPosition.x = -2.88, m_LocalPosition.y = -2.7 -> the authored scene position.
SampleScene.unity 'Start Menu' > 'Backdrop' Image m_Color: {r: 0, g: 0, b: 0, a: 0.72} -> the menu is only 72% opaque, the ship is visible behind it.

Unity docs (verified, not intuition):
- MonoBehaviour.Start: "Start is called on the frame when a script is enabled just before any of the Update methods are called the first time" and "Start might not be called on the same frame as Awake if the script is not enabled at initialization time."
- Manual/execution-order: "In general, you can't rely on the order in which the same event function is invoked for different GameObjects, except when the order is explicitly documented or settable."
- MonoBehaviour.Awake: "Awake is always called before any Start functions."
```

**Как чинить.** Two one-line changes, both code-side, remove the dependency entirely. (1) Move the two Quaternion.Euler assignments from PlayerMover.Start into PlayerMover.Awake - Awake is documented to run before any Start, so _minRotation/_maxRotation are always valid by the time anything can call Jump/Push/Update. (2) Delete ReturnToStart() from PlayerMover.Start and instead call _playerShip.ReturnToStart() from Game.Start right after _playerShip.DisableControl() - PlayerShip already exposes ReturnToStart() publicly (PlayerShip.cs:103-106), so Game owns the placement explicitly instead of hoping PlayerMover's Start wins the race. After that PlayerMover has no Start at all and nothing depends on inter-object Start order.

**Риск правки.** Low. Awake is guaranteed before every Start, so moving the rotation setup earlier cannot regress. The only behavioural change is that the ship is now deterministically placed at _startPosition on the title screen, which is what the code already intended.

**Уточнения проверяющих.**

- Keep the core claim (Game.Start disables PlayerMover before PlayerMover.Start has run, so the ship's placement is decided by unspecified inter-GameObject Start order), but drop two parts. (1) Wrong evidence: the 0.72 backdrop is the GAME OVER screen's - Image &195992348 on Backdrop &427282941, parent RectTransform &607739895 = GameObject &260790321 named "Game Over". The Start Menu (&452419521, transform &345113249) uses Backdrop &900425869 / Image &889926514 with a: 0.75, full-screen (anchors 0-1, sizeDelta 0). Still translucent, so the symptom survives, but cite 0.75. (2) The second consequence is unsupported and should be removed: PlayerMover.Update (line 36) cannot run before Start, so _minRotation is never read while invalid, and the Jump() path is blocked - InputReader is component &661464565 on the Game object (scene line 289) while the EventSystem is at scene line 9385, so InputReader.Update (InputReader.cs:14-24) is dispatched before the START button's onClick that frame; Input.GetKeyDown(W) is consumed while _isControlEnabled is still false and PlayerShip.cs:63-64 returns early. No "transform.rotation assign attempt ... is not valid" log is demonstrable. The only proven consequence is cosmetic and pre-game: until START is clicked the ship is drawn at the authored (-2.88,-2.7) instead of (-6,0) behind the 75%-black menu scrim, and PlayerShip.Restart() (PlayerShip.cs:157-168, EnableControl + ReturnToStart) corrects it the moment the player clicks START.
- Keep the ordering claim, drop the error-log claim, and downgrade to minor.

1) WRONG UNITY SEMANTICS - strike the console error. The finding asserts that Jump() executing `transform.rotation = _maxRotation` with default(Quaternion) = (0,0,0,0) makes Unity log "transform.rotation assign attempt for 'Player Ship' is not valid." That error is emitted for NON-FINITE input (NaN/Inf) - every reported instance is "Input rotation is { NaN, NaN, NaN, NaN }". A zero quaternion is finite and is normalize-safed to identity on assignment; the Unity Discussions thread "Why are quaternions initialized to an invalid rotation by default?" records exactly this: `transform.rotation = new Quaternion(); // This works just fine`, "it magically gets treated like being set to the identity quaternion". So the real second consequence is silent, not an error: PlayerMover.cs:44 would snap the ship to 0 degrees instead of the intended +45. PlayerMover.cs:36 is likewise safe - Quaternion.Lerp(current, (0,0,0,0), t) normalizes back to `current`, no error and no visible drift.

2) FACTUAL SLIP - the Start Menu backdrop alpha is 0.75, not 0.72. The 0.72 backdrop (Image &195992348) belongs to the Game Over screen; Start Menu > Backdrop is Image &889926514, m_Color a: 0.75. Does not change the conclusion (ship still ~25% visible), but the cited evidence line is misattributed.

3) SEVERITY - major -> minor. The divergence never reaches gameplay. The instant START is clicked, Game.StartGame -> RestartGame -> StartLevel -> PlayerShip.Restart() (PlayerShip.cs:157-168) calls EnableControl() and then ReturnToStart() (PlayerShip.cs:166-167), which snaps the ship to (-6,0) and zeroes velocity in BOTH orderings. So the only observable effect is the ship's idle position on the start menu, seen at 25% opacity behind the backdrop, plus - in the one-frame race where W goes down in the same frame the START button's onClick fires - one frame of 0-degree instead of 45-degree rotation, silently. The deferred PlayerMover.Start then runs at the next frame's EarlyUpdate.ScriptRunDelayedStartupFrame and calls ReturnToStart() a second time, which would also wipe that frame's jump velocity - again one frame, no error.

Corrected claim: "Game.Start() (Game.cs:53) disables PlayerMover on a different GameObject before PlayerMover.Start() (PlayerMover.cs:26-32) is guaranteed to have run, and nothing in the project pins the order, so PlayerMover's own initialization - its start position and its two rotation limits - either runs or is deferred to the frame after the player clicks START, depending on an ordering Unity documents as not guaranteed. Observable effect: the ship idles on the start menu at either (-6,0) or the authored (-2.88,-2.7)."

The proposed fix is sound as written and worth keeping: move the two Quaternion.Euler assignments into PlayerMover.Awake (Awake is documented to always run before any Start), and drop ReturnToStart() from PlayerMover.Start, calling the already-public PlayerShip.ReturnToStart() (PlayerShip.cs:103-106) from Game.Start right after DisableControl(). PlayerMover then has no Start and no inter-object ordering dependency remains.
- Game.cs:53 disables PlayerMover (via PlayerShip.cs:99) before PlayerMover.Start() (PlayerMover.cs:26-32) is guaranteed to have run, and nothing pins script execution order, so PlayerMover.Start is skipped in one of the two possible orderings. The only observable consequence is cosmetic and confined to the start-menu screen: the ship is drawn either at _startPosition (-6, 0) or at its authored scene position (-2.88, -2.7), as a 25%-visible silhouette behind the full-screen 75%-opaque Start Menu backdrop (SampleScene.unity:8532, RectTransform anchors 0,0-1,1 at SampleScene.unity:6366-6367). Gameplay is NOT affected: PlayerShip.Restart() calls ReturnToStart() at PlayerShip.cs:167 on every level start, so the ship is always at (-6, 0) once play begins. The claimed second consequence - a zero-quaternion "transform.rotation assign attempt is not valid" error from Jump() - has no player-reachable repro; it needs a W keypress inside the same frame as the START click plus a favourable intra-frame script order. Drop the 0.72 backdrop citation entirely: that alpha belongs to the Game Over backdrop (SampleScene.unity:347), not the Start Menu. Correct framing: a latent initialization-order dependency between two GameObjects, minor, with the proposed fix (move the two Quaternion.Euler assignments into PlayerMover.Awake and let Game own the initial placement by calling _playerShip.ReturnToStart() after DisableControl()) still being the right and proportionate change.

---

### 1. Game.Resume() unconditionally re-enables player control, handing the ship back mid-warp-cutscene and after death - root cause: _isControlEnabled is one un-owned flag with four public mutators (LockInput/UnlockInput/EnableControl/DisableControl) written by both Game and the cutscenes

**major** · ось: Cutscene races · `Assets/Scripts/Game.cs:166` · единогласно

**Сценарий воспроизведения.** A) WARP CUTSCENE. Start the game and clear level 1's 3 enemies. WinLevel -> WarpJump.Play() -> Jump() calls DisableControl() (WarpJump.cs:66) so the ship is supposed to be uncontrollable for the whole 9.6 s cutscene. Press Escape (TogglePause passes because _isRunning is still true), then click Resume on the pause panel. Resume() calls _playerShip.UnlockInput() (Game.cs:166). Now press Space: PlayerShip.OnShot passes the _isControlEnabled check and the ship fires bullets right through the cutscene. Press W: PlayerMover.Jump() writes linearVelocity=(0,2) on a Rigidbody2D that DisableControl made Kinematic, while PlayerMover itself is disabled so the per-frame clamp at PlayerMover.cs:34-39 never runs. Unity's RigidbodyType2D.Kinematic doc: "This type of Rigidbody2D can be moved by setting its Rigidbody2D.linearVelocity". The ship therefore drifts upward at 2 units/s with nothing stopping it, and nothing writes its position during Align (0.6 s), the 3.75 s warp wait or Stretch (0.4 s), so it flies clean off the top of the ~10-unit-tall screen mid-cutscene. It cannot even die out there, because DisableControl left _bodyCollider off so it never reaches "ObjectRemover top". Dash (WarpJump.cs:123-134) later snaps it back by writing absolute positions.

B) DEATH CUTSCENE. Die. PlayerShip.Die() calls DisableControl() (PlayerShip.cs:151); Game.LoseLevel starts ShowGameOver(), which waits _playerShip.DeathDuration (16 s -- see the separate finding). Anywhere inside that window press Escape and click Resume. ShowGameOver then falls through to `_isRunning = false` (Game.cs:202) and shows the defeat screen (203); nothing ever re-locks input. On the defeat screen, pressing Space still reaches PlayerShooter.Shoot() (PlayerShooter.cs:31-44): the dead, invisible ship keeps firing bullets and playing the shot sound, and the reload bar keeps animating. Escape is now inert (_isRunning == false, Game.cs:101), so the state persists until the player clicks Restart.

**Почему это дефект.** Both cutscenes explicitly take control away from the player by calling DisableControl(). Resume() is the only handback path besides PlayerShip.Restart(), and it restores a fixed state (`unlocked`) instead of the state that was in force before the pause. Pause must be state-preserving: resuming from a pause has to return the game to exactly the state it was paused in, not to "playable". In case B the unlock also survives past the end of the cutscene, so the player is left firing from behind the defeat screen -- a state nothing in the code can clear.

**Код:**

```csharp
Game.cs:99-108  private void TogglePause() { if (_isRunning == false) return; if (_isPaused) Resume(); else Pause(); }
Game.cs:161-168  private void Resume() { _isPaused = false; Time.timeScale = NormalTimeScale; AudioListener.pause = false; _playerShip.UnlockInput(); _pauseScreen.Hide(); }
Game.cs:143 `_isRunning = true;` (set in StartLevel) and Game.cs:202 `_isRunning = false;` (set only AFTER the game-over wait) -- so `_isRunning` is true for the whole of both cutscenes.
WarpJump.cs:66  `_playerShip.DisableControl();` (first statement of Jump(), runs synchronously inside Play()).
PlayerShip.cs:95-101  DisableControl() { LockInput(); _playerMover.Stop(); _playerMover.enabled = false; _bodyCollider.enabled = false; }
PlayerShip.cs:77-80  public void UnlockInput() { _isControlEnabled = true; }
PlayerShip.cs:61-75  OnJumped/OnShot are gated ONLY on `_isControlEnabled`.
PlayerShip.cs:151  DisableControl() inside Die().
PlayerMover.cs:41-45  Jump() { _rigidbody2D.linearVelocity = new Vector2(0, _tapForce); transform.rotation = _maxRotation; }
PlayerMover.cs:50  `_rigidbody2D.bodyType = RigidbodyType2D.Kinematic;` (inside Stop()).
PlayerMover.cs:34-39  the clamp lives in Update(), which never runs while the component is disabled.
Scene: Game._firstWaveSize = 3 (SampleScene.unity, Game component).
```

**Как чинить.** Make the pause round-trip state-preserving instead of state-setting. Expose PlayerShip's current input state as a read-only property (`public bool IsControlEnabled => _isControlEnabled;`), have Game.Pause() capture it into a field before calling LockInput(), and have Resume() reapply the captured value instead of calling UnlockInput() unconditionally. A second, cheaper guard that also fixes it: keep a `_isCutscenePlaying` flag in Game, set it in WinLevel and LoseLevel, clear it in StartLevel and at the end of ShowGameOver, and check it in TogglePause next to `_isRunning` so the player cannot pause during a cutscene at all.

**Риск правки.** Low. Confined to Game.Pause/Resume plus one read-only property on PlayerShip; the normal pause-during-play path behaves identically because there _isControlEnabled is already true.

**Уточнения проверяющих.**

- The defect and both reproductions hold as stated; two supporting details need fixing. (1) "Dash (WarpJump.cs:123-134) later snaps it back by writing absolute positions" is wrong - Dash captures startPosition = ship.position at its own start (WarpJump.cs:125) and lerps forward from there, so a ship that drifted off-screen is not brought back by Dash; it only returns at the next StartLevel via Restart() -> ReturnToStart() (PlayerShip.cs:167, PlayerMover.cs:59-65). (2) The repro does not need the Resume button at all: pressing Escape a second time reaches Resume() directly through TogglePause (Game.cs:104-105), since InputReader.Update keeps polling at timeScale 0.
- Two cosmetic inaccuracies, neither load-bearing. (1) "Dash ... later snaps it back" cites _dashDistance 16 — that is the script default at WarpJump.cs:20; the scene value is `_dashDistance: 4` (SampleScene.unity:8777), so the dash is 4 units, not 16. (2) The headline "Resume() unconditionally re-enables player control" is loose: Resume() calls UnlockInput() only, which re-enables input handling (shoot + the Jump() call), not EnableControl() — the PlayerMover component stays disabled and _bodyCollider stays off. The body of the finding already reasons correctly from that exact distinction, which is what produces the kinematic-drift and no-collider consequences.
- Two incidental details in the write-up are wrong and should be dropped or reworded; the claim itself stands. (1) Dash does not "snap the ship back": WarpJump.cs:125 captures startPosition = ship.position, i.e. the drifted position, and lerps 4 units right from there (_dashDistance is 4 in the scene, SampleScene.unity:8776, not the 16 default) - the ship stays off-screen until the next StartLevel calls PlayerShip.Restart() -> ReturnToStart() (PlayerShip.cs:157-168, PlayerMover.cs:59-65). (2) DeathDuration is computed at runtime from the explosion prefab (ExplosionAnimation.cs:70-89), and the prefab data supports at least ~11 s rather than a verified 16 s; state it as "many seconds" unless the separate finding pins it down.

<sub>Слито из 10 формулировок других ревьюеров.</sub>

---

### 7. IDamageable has zero consumers and bundles Die() with TakeDamage, forcing a public kill-switch on both ships that bypasses Health

**major** · ось: SOLID: SRP and OCP · `Assets/Scripts/Interfaces/IDamageable.cs:1` · с особым мнением

**Нарушенное правило.** Violated rule: Open/Closed, and the supporting rule that an abstraction exists so that callers can depend on it. Adding a third damageable (a destructible asteroid, a boss, a shield generator) today requires writing the same three pieces again: a serialized AmmoHitHandler field, a subscription in OnEnable, an unsubscription in OnDisable, and a `TakeDamage(ammo.Damage)` forwarder. The interface that would have made that free is sitting unused in Assets/Scripts/Interfaces.
What it should look like: AmmoHitHandler resolves the damageable itself — `if (GetComponentInParent<IDamageable>() is IDamageable damageable) damageable.TakeDamage(ammo.Damage);` — and PlayerShip/EnemyShip lose the field, both subscriptions and the forwarder entirely. A new damageable is then a component that implements IDamageable plus an AmmoHitHandler on its body collider, with zero edits to existing classes.

**Почему это дефект.** An interface no caller depends on provides no polymorphism; it is decoration that still has to be maintained. Worse, its second member forces API that should not exist: because IDamageable declares Die(), both ships must expose `public void Die()` (PlayerShip.cs:140, EnemyShip.cs:87), and that public door is then used to kill an enemy outside its Health component (EnemyCollision.cs:11 `_enemyShip.Die();`). 'Can take damage' and 'can be destroyed on command' are two different capabilities, and only the first is ever consumed.

**Код:**

```csharp
Interfaces/IDamageable.cs:1-5  `public interface IDamageable { public void TakeDamage(int damage); public void Die(); }`
Implemented at PlayerShip.cs:6 and EnemyShip.cs:5. A grep for `IDamageable` across all 42 scripts returns only those two class declarations and the interface file itself — no variable, parameter, field, GetComponent or cast ever uses the type.
Instead the damage path is hand-wired twice, identically:
  PlayerShip.cs:12 `[SerializeField] private AmmoHitHandler _ammoHitHandler;`, :37 `_ammoHitHandler.HitDetected += OnAmmoHit;`, :113-116 `private void OnAmmoHit(Ammo ammo) { TakeDamage(ammo.Damage); }`
  EnemyShip.cs:7 `[SerializeField] private AmmoHitHandler _ammoHitHandler;`, :36 `_ammoHitHandler.HitDetected += OnAmmoHit;`, :82-85 `private void OnAmmoHit(Ammo ammo) { TakeDamage(ammo.Damage); }`
AmmoHitHandler.cs:8,18 `public event Action<Ammo> HitDetected; ... HitDetected?.Invoke(ammo);` — the handler already knows the damage and already resolves the ammo; it just refuses to deliver it.
Prefab layout verified in YAML: AmmoHitHandler sits on the child 'Body sprite' of both Player Ship.prefab and Enemy Ship.prefab, with PlayerShip / EnemyShip on the root — so GetComponentInParent reaches the damageable from where the handler already lives.
The project already uses that exact pattern for the sibling interface: ObjectRemover.cs:7 `IRemoveable removeable = other.GetComponentInParent<IRemoveable>();`
```

**Как чинить.** Make the interface load-bearing and shrink it. (1) Move delivery into AmmoHitHandler.OnTriggerEnter2D (AmmoHitHandler.cs:10-21): after the layer check, resolve `IDamageable` with GetComponentInParent and call TakeDamage(ammo.Damage) before ammo.Hit/ammo.Remove. Keep the HitDetected event only if something else needs it — nothing currently does. (2) Delete `_ammoHitHandler` plus the OnEnable/OnDisable lines and OnAmmoHit from PlayerShip (12, 37, 47, 113-116) and EnemyShip (7, 36, 42, 82-85). (3) Drop `void Die()` from IDamageable, leaving `void TakeDamage(int damage)`; Die() can then become private on PlayerShip (it is only reached via Health.Died at PlayerShip.cs:40 and Remove() at 172) and stay public on EnemyShip only for EnemyCollision's ram kill.

**Риск правки.** Low-medium. Code-only; the removed serialized fields simply become stale YAML that Unity discards. One ordering detail to preserve: AmmoHitHandler currently raises HitDetected before calling ammo.Hit() and ammo.Remove() (lines 18-20), so call TakeDamage in that same position or a lethal hit will spawn its impact effect after the victim has already been returned to the pool.

**Особое мнение скептика.** The finding's mechanism is contradicted by the prefabs and its named rule is not violated.

(1) The premise "the handler already knows the damage ... it just refuses to deliver it" is false. AmmoHitHandler (guid dfa614011e7f740be990b8fb2c10dfc5) is attached to five more prefabs the reviewer never opened: Assets/Prefabs/Asteroids/Asteroid 1-5.prefab. In "Assets/Prefabs/Asteroids/Asteroid 1.prefab":191-200 the MonoBehaviour sits on the prefab's single GameObject (fileID 6747026704579016348) — the same object that carries Asteroid with <Damage>k__BackingField: 30 at :190. Assets/Scripts/Environment/Asteroid.cs:6 declares `Asteroid : MonoBehaviour, IPoolable, IRemoveable` — NOT IDamageable — and grep for HitDetected across all 42 scripts shows only PlayerShip.cs:37,47 and EnemyShip.cs:36,42 subscribe. So on 5 of the 7 objects carrying it, AmmoHitHandler exists purely to absorb ammo and spawn the hit effect (AmmoHitHandler.cs:19-20) on a thing that must NOT take damage. The event is exactly what lets one component serve both damageable ships and non-damageable asteroids; the proposed "resolve IDamageable and call TakeDamage inside OnTriggerEnter2D" would push damage semantics into a component whose job on most of its instances is to have no damage semantics. That is a taste trade, and arguably the worse SRP of the two.

(2) OCP is not violated. Adding a destructible asteroid or boss today requires zero modification of AmmoHitHandler.cs, PlayerShip.cs or EnemyShip.cs — you write a new class with its own serialized handler and subscription. Only the new class is written. The reviewer's real complaint is ~4 duplicated lines across two classes (DRY), which is not the rule they named, and is not a rule stated in the school exemplar.

(3) "Forces a public kill-switch on both ships" is factually wrong for EnemyShip: EnemyCollision.cs:11 `_enemyShip.Die();` requires EnemyShip.cs:87 to be public independently of the interface — the finding's own proposed fix concedes this ("stay public on EnemyShip only for EnemyCollision's ram kill"), contradicting its headline. For PlayerShip (Die at :140, reached only from `_health.Died += Die` at :40 and Remove() at :172) the entire cost is one method being public instead of private.

(4) "Bypasses Health" is not a defect: EnemyShip.OnSpawn() calls `_health.Init()` at EnemyShip.cs:60 on every spawn, so the un-drained Health value never leaks into the next life, and the ram kill is the described game design.

What actually survives is only the small, true observation that grep for IDamageable across Assets/Scripts returns nothing but IDamageable.cs:1,3,4, PlayerShip.cs:6 and EnemyShip.cs:5 — an interface never used as a type. That is minor hygiene with no rule against it in the exemplar (which contains no interfaces at all), not the major SRP/OCP violation filed, and the prescribed rewiring of the damage pipeline is disproportionate to it.

**Уточнения проверяющих.**

- Drop the SRP/OCP framing and the "both ships" causation. The defensible claim is narrower: IDamageable (Interfaces/IDamageable.cs:1-5) is an unused abstraction — implemented at PlayerShip.cs:6 and EnemyShip.cs:5 but never used as a type by any caller — so it provides no polymorphism, and its second member is the only reason PlayerShip.Die() (PlayerShip.cs:140) is public, since nothing outside the class calls it. Alongside it, the damage delivery is hand-duplicated in both ships (PlayerShip.cs:12/37/47/113-116, EnemyShip.cs:7/36/42/82-85) although AmmoHitHandler.cs:18 already resolves the ammo and its damage, and the project already uses interface resolution for the sibling case at ObjectRemover.cs:7. That is a dead-code plus duplication cleanup — a minor "recommendation"-grade item, not a proven SOLID violation: OCP is satisfied (no existing class changes to add a damageable) and EnemyShip.Die() must stay public for EnemyCollision.cs:11 regardless. If the fix is proposed, note that moving TakeDamage into AmmoHitHandler.OnTriggerEnter2D also changes behaviour: today a ship whose component is disabled has unsubscribed in OnDisable and takes no damage, whereas the handler-side call would apply damage regardless of the ship component's enabled state.
- Two sub-claims are wrong and must be dropped before this is reported.

(1) "forcing a public kill-switch on both ships" is half false. EnemyShip.Die() must be public regardless of IDamageable, because EnemyCollision.cs:11 calls `_enemyShip.Die()` on the ram path. Only PlayerShip.Die() (PlayerShip.cs:140) is widened by the interface.

(2) The named rule, Open/Closed, is misapplied. Adding a destructible asteroid or boss today requires modifying no existing class — it requires retyping the same four lines in the new one. That is duplication, not OCP. The rule actually violated is that an abstraction nothing depends on is dead code, plus Interface Segregation: IDamageable bundles "can be damaged" and "can be destroyed on command", and no consumer uses either.

(3) The premise behind the proposed fix — "the handler just refuses to deliver [damage]", "nothing currently needs HitDetected" — misses that AmmoHitHandler is also on all five asteroid prefabs (e.g. Assets/Prefabs/Asteroids/Asteroid 1.prefab:189-201, same GameObject as `Asteroid`, `_ammoLayers` bits 640), where nothing subscribes at all. Asteroid.cs:6 is `IPoolable, IRemoveable` only — deliberately not damageable. So AmmoHitHandler is by design a hit *announcer* that consumes the bullet, used by three prefab families of which only two take damage. The suggested `GetComponentInParent<IDamageable>()` would still behave correctly there (null result, no damage), but the fix should be pitched as "let the handler resolve an optional IDamageable" rather than "the handler already owed the ships this call".

Restated claim that survives: IDamageable is a dead abstraction — no consumer anywhere in Assets — and its second member `Die()` exists only to force `public void Die()` onto PlayerShip, which would otherwise be private.
- One count correction: IDamageable forces FOUR members public (TakeDamage and Die on each of the two classes), not three. Three of those four — PlayerShip.TakeDamage (PlayerShip.cs:108), PlayerShip.Die (PlayerShip.cs:140) and EnemyShip.TakeDamage (EnemyShip.cs:77) — have no caller outside their own class and would become private if the interface were dropped; EnemyShip.Die (EnemyShip.cs:87) stays public regardless, for EnemyCollision.cs:11. Accurate phrasing: "IDamageable has no consumer anywhere; it is the sole reason three of the four members it mandates are public." The secondary point about the redundant `public` modifiers on IDamageable.cs:3-4 is also correct and the project is internally inconsistent about it — IRemoveable.cs:3 writes `public void Remove();` while IPoolable.cs:5-9 correctly omits the modifier.
- Precision tightening only, the substance is correct: IDamageable declares two members, which force four public method implementations across the two classes (not "four members"); three of those four — PlayerShip.TakeDamage (PlayerShip.cs:108), PlayerShip.Die (PlayerShip.cs:140), EnemyShip.TakeDamage (EnemyShip.cs:77) — have no caller outside their own class, while EnemyShip.Die (EnemyShip.cs:87) must remain public for EnemyCollision.cs:11. Worth adding to the write-up as corroboration: the sibling interface IRemoveable is consumed polymorphically at Assets/Scripts/Environment/ObjectRemover.cs:7 via GetComponentInParent<IRemoveable>(), which shows the project does use interfaces as abstractions where they earn their place and makes IDamageable's zero consumers an in-project inconsistency, not an external stylistic preference.
- Keep the claim, tighten the justification. Drop the appeal to the school exemplar: I read ../Code style genius/BulletShooter.cs and PlacesMover.cs and they do declare no public member, but they are single-collaborator scripts with nobody to call them, so they establish nothing about interfaces or public surface. The defensible basis is internal: IRemoveable is consumed as a type at ObjectRemover.cs:7 and IPoolable at SpawnPool.cs:6,82, while IDamageable is consumed nowhere — it is the one dead abstraction in the project, and it is the sole reason PlayerShip.TakeDamage (:108), PlayerShip.Die (:140) and EnemyShip.TakeDamage (:77) must be public. Also, the redundant `public` on interface members is not IDamageable-specific — IRemoveable.cs:3 does the same — so that sub-point must either cover both files or be dropped rather than pinned on IDamageable.

<sub>Слито из 4 формулировок других ревьюеров.</sub>

---

### 10. WarpJump owns player state it should not - a second reference to the player's SpriteRenderer plus direct writes to the player's transform - and its Stop() cannot undo any of it; only Game's later PlayerShip.Restart() repairs it

**major** · ось: OOP and encapsulation · `Assets/Scripts/Level/WarpJump.cs:9` · требует редактора Unity · единогласно

**Нарушенное правило.** Violated rule: single ownership of state / encapsulation. PlayerShip already declares the ship's visibility as its own concern (it owns _shipSprite and flips it in Die and Restart). WarpJump is a second, unrelated class writing the same `enabled` flag on the same renderer. It should look like WarpJump.cs:66 already does for control - ask the object: `_playerShip.Hide()` (a new public method on PlayerShip alongside DisableControl), with PlayerShip.Restart already restoring it at line 160. WarpJump then keeps only its _playerShip reference and drops _shipSprite entirely.

**Почему это дефект.** Two classes writing one flag with no coordination is a proven encapsulation break, not a preference. It currently survives only because of an accident of call ordering in Game.StartLevel - line 140 _warpJump.Stop() then line 145 _playerShip.Restart() - so the ship reappears. Change that ordering, or call WarpJump.Stop() from anywhere else, and the player ship stays invisible with no class responsible for turning it back on. The same file also drives ship.rotation (104), ship.localScale (117) and ship.position (130) directly through _playerShip.transform, so the leak is systematic.

**Код:**

```csharp
WarpJump.cs:8    [SerializeField] private PlayerShip _playerShip;
WarpJump.cs:9    [SerializeField] private SpriteRenderer _shipSprite;
WarpJump.cs:82       _shipSprite.enabled = false;

PlayerShip.cs:15  [SerializeField] private SpriteRenderer _shipSprite;
PlayerShip.cs:152     _shipSprite.enabled = false;   // in Die()
PlayerShip.cs:160     _shipSprite.enabled = true;    // in Restart()

Both serialized references resolve to the same SpriteRenderer, verified in the asset files:
Prefabs/Player Ship.prefab:250   _shipSprite: {fileID: 796812979710454581}
Scenes/SampleScene.unity:8766    _shipSprite: {fileID: 366751404}
Scenes/SampleScene.unity:5906    --- !u!212 &366751404 stripped
Scenes/SampleScene.unity:5908      m_CorrespondingSourceObject: {fileID: 796812979710454581, guid: 09ba11221984c417dbda5f0b88456507, type: 3}
Prefabs/Player Ship.prefab.meta   guid: 09ba11221984c417dbda5f0b88456507
```

**Как чинить.** Add a Hide()/Show() pair (or a single SetVisible-style command) to PlayerShip that wraps _shipSprite.enabled, have WarpJump.cs:82 call _playerShip.Hide(), and delete WarpJump's _shipSprite field. Then clear the now-unused _shipSprite reference on the WarpJump component in SampleScene.unity. If you also want the transform writes contained, expose the align/stretch/dash as methods on PlayerShip (or on PlayerMover) that WarpJump drives, instead of WarpJump mutating _playerShip.transform.

**Риск правки.** Low for the sprite part - one method added, one call site changed, one inspector field cleared. The optional transform part is a larger refactor of a working cutscene and is not required to close the encapsulation break.

**Уточнения проверяющих.**

- The finding stands, but one clause of its rationale is false and must be removed: "Change that ordering ... and the player ship stays invisible with no class responsible for turning it back on." Swapping Game.cs:140 `_warpJump.Stop()` and Game.cs:145 `_playerShip.Restart()` would NOT leave the ship invisible - Stop() (WarpJump.cs:51-60) never disables anything, and StopCoroutine is synchronous, so Restart() first then Stop() still ends with `_shipSprite.enabled = true`. Restate the coupling as what the code actually shows: WarpJump.cs:82/104/117/130 leave the player with its sprite off, x-scale doubled and position dashed _dashDistance units right (4 in SampleScene.unity:8780), WarpJump has no code path that can undo any of it, and PlayerShip.Restart() carries repair lines that exist only for that reason - PlayerShip.cs:160 re-enables the renderer and PlayerShip.cs:161 restores the scale, where a repo-wide grep shows WarpJump.cs:117 is the only other writer of the player's localScale in the project. Also narrow the emphasis: the `_shipSprite` duplication is the proven core (two classes, one flag, no coordination, with `_playerShip.DisableControl()` at WarpJump.cs:66 showing the available alternative in the same file); the transform writes are corroborating rather than independently damning, their evidence being that PlayerShip.cs:161 exists solely to undo them. The proposed fix (a Hide()/Show() pair on PlayerShip, WarpJump.cs:82 calling _playerShip.Hide(), dropping WarpJump's _shipSprite field) is correct as written.
- Drop the call-ordering rationale - it is wrong. WarpJump.Stop() (WarpJump.cs:51-60) never disables the sprite, and a coroutine cannot resume between two statements of Game.StartLevel, so reordering Game.cs:140 and Game.cs:145 would not leave the ship invisible. State the finding as it actually is: one boolean, `_shipSprite.enabled`, has two writers with no owner - PlayerShip.cs:152/160 (which declares the renderer as its private state at PlayerShip.cs:15) and WarpJump.cs:82 through a duplicate serialized reference at WarpJump.cs:9 - and WarpJump's own Stop() cannot restore what its coroutine changed, so the invariant is maintained only by Game calling PlayerShip.Restart(). No current playthrough leaves the ship invisible; this is a structural defect, not a live bug. The fix stands: add a Hide()/Show() pair to PlayerShip wrapping `_shipSprite.enabled`, call `_playerShip.Hide()` at WarpJump.cs:82, delete WarpJump's `_shipSprite` field and clear the reference at SampleScene.unity:8766. The transform writes (WarpJump.cs:104/117/130 via `_playerShip.transform`) are ordinary Unity cutscene practice and should stay a recommendation at most, not part of the claim.
- Narrow the claim to the SpriteRenderer, and drop the transform half.

Surviving claim: WarpJump holds a second serialized reference to the player's SpriteRenderer (WarpJump.cs:9) and disables it at WarpJump.cs:82, while PlayerShip declares that same renderer (PlayerShip.cs:15) and owns its enabled flag (PlayerShip.cs:152, :160). Two classes write one flag; WarpJump.Stop() (WarpJump.cs:51-60) restores nothing, so the only thing that turns the ship visible again is PlayerShip.Restart() called five lines later at Game.cs:145. Fix as proposed: a Hide() command on PlayerShip wrapping _shipSprite.enabled, called from WarpJump.cs:82 exactly as WarpJump.cs:66 already calls DisableControl(), and delete WarpJump's _shipSprite field plus its scene reference.

Cut from the finding: the "ship.rotation (104), ship.localScale (117), ship.position (130)" half and its proposed fix. `_playerShip.transform` is public Unity API, and a cutscene class animating a foreign Transform is a mainstream Unity idiom, not a proven violation — the project does the same elsewhere (ExpandingFade.cs:56 drives itself from a foreign _target). The suggested remedy — moving align/stretch/dash onto PlayerShip or PlayerMover — would push cutscene choreography into the gameplay class and is disproportionate for a project this size. The reviewer also hedged it themselves ("If you also want the transform writes contained"), which is exactly the softness rule 6 bans. It is legitimate only as supporting evidence that PlayerShip.cs:161 is cleanup for WarpJump.cs:117, not as its own ask.

Severity major -> minor: there is no current misbehaviour. Stop() has exactly one caller (Game.cs:140), immediately before the Restart that repairs everything, so the finding's own scenario is hypothetical ("change that ordering"), not reproducible today. And "a rule the graders will fail" is not provable from the material supplied: the exemplar (../Code style genius/BulletShooter.cs, PlacesMover.cs) is style-only — explicit types, _camelCase serialized fields, cached WaitForSeconds, brace-less single statements, member order — and states nothing about ownership of state; PlacesMover.cs:14-17 even reaches into a foreign Transform hierarchy. The defect is real and the fix is one field and one line, which is "real but small".

<sub>Слито из 3 формулировок других ревьюеров.</sub>

---

### 4. Enemy flight speed is randomised once in Awake instead of OnSpawn, so pooled enemies keep the same speed for the whole session

**major** · ось: Pool lifecycle · `Assets/Scripts/Ships/Enemy/EnemyMover.cs:15` · единогласно

**Сценарий воспроизведения.** Start a new game. Level 1 spawns 6 enemies (EnemySpawner._firstWaveSize) and SpawnPool.Create instantiates 6 EnemyShips, each drawing one speed in [0.5, 3]. Clear the level; StartNextLevel -> StartWave(9) reuses those same 6 pooled instances in stack order and creates only 3 new ones. From level 2 on, the first six enemies of every wave fly at the exact same six speeds they had on level 1, in the same order, and once the pool has grown to the largest wave size reached (level N has 6+(N-1)*3 enemies) no enemy in the rest of the session ever gets a new speed. Watch two consecutive waves side by side and the pacing repeats identically.

**Почему это дефект.** Not taste: the class serialises a _minSpeed/_maxSpeed range specifically so each enemy flies at its own speed, and pooling silently converts that from per-spawn randomisation to per-instance randomisation. The project proves it knows the correct place for this - Asteroid.cs:32-36 re-rolls both of its randomised values in OnSpawn, which is the hook SpawnPool calls on every reuse (SpawnPool.cs:52-59). EnemyShip.OnSpawn resets _isDead, health, movement and shooting but not this, so the reset is incomplete.

**Код:**

```csharp
Assets/Scripts/Ships/Enemy/EnemyMover.cs
   6    [SerializeField] private float _minSpeed = 1f;
   7    [SerializeField] private float _maxSpeed = 3f;
  12    private void Awake()
  13    {
  14        _rigidbody2D = GetComponent<Rigidbody2D>();
  15        _speed = Random.Range(_minSpeed, _maxSpeed);
  16    }
  18    public void Fly()
  20        _rigidbody2D.linearVelocity = transform.right * -_speed;

Assets/Scripts/Ships/Enemy/EnemyShip.cs (the pool's reset hook - it never re-randomises)
  57    public void OnSpawn()
  58    {
  59        _isDead = false;
  60        _health.Init();
  61        _mover.Fly();
  62        _shooter.StartShoot();
  63    }

Contrast, the project's own correct version of exactly this pattern:
Assets/Scripts/Environment/Asteroid.cs
  32    public void OnSpawn()
  33    {
  34        _speed = Random.Range(_minSpeed, _maxSpeed);
  35        _rotationSpeed = Random.Range(_minRotationSpeed, _maxRotationSpeed);
  36    }

Awake runs exactly once per instance, at Instantiate time:
Assets/Scripts/Pools/SpawnPool.cs
  61    private T Create()
  63        T item = UnityEngine.Object.Instantiate(_prefab);
  64        item.gameObject.SetActive(false);

Assets/Prefabs/Enemy Ship.prefab:305-306   _minSpeed: 0.5   _maxSpeed: 3
```

**Как чинить.** Move the draw out of Awake into a public reset on EnemyMover - e.g. have Fly() itself do _speed = Random.Range(_minSpeed, _maxSpeed) before setting the velocity, since Fly() is already called from EnemyShip.OnSpawn:61 and from nowhere else. Awake then keeps only the GetComponent<Rigidbody2D>() line. No call-site changes are needed.

**Риск правки.** Very low. Fly() has a single caller (EnemyShip.OnSpawn) and _speed has a single reader (Fly itself). The only observable change is that enemy speeds vary again across waves, which is the serialised intent.

**Уточнения проверяющих.**

- The mechanism is right, but three details in the reproduction are wrong and should be trimmed before publishing. (a) Wave sizes: `_firstWaveSize` lives on Game (Game.cs:21), not EnemySpawner, and the scene overrides it to 3 (Assets/Scenes/SampleScene.unity:8280-8281, on the Game component whose `_enemySpawner` reference is at :8272). So level 1 spawns 3 enemies, level 2 six, level N = 3 + (N-1)*3 — not 6/9. (b) The pool grows to the peak number of *simultaneously active* enemies, not to the wave size: enemies are released on death or retire (SpawnPool.cs:82-87) and spawn one every 3 s (EnemySpawner.cs:9, SampleScene.unity:9240), so a 9-enemy wave may never need 9 instances. The correct statement is "once an instance exists, it never draws a new speed again". (c) "The same six speeds in the same order" and "the pacing repeats identically" are overstated and not supported: the ObjectPool hands instances back in release order, which depends on which enemy dies or leaves first, and the spawn Y is freshly randomised every spawn (EnemySpawner.cs:95-98). What is proven is only that the set of speeds in play stops refreshing — each enemy keeps its instantiation-time speed for the whole session. The proposed fix is sound as written: `Fly()` is the only consumer and the only call site is EnemyShip.cs:61, so moving the draw into Fly() (or a new reset method called from OnSpawn) needs no call-site changes.
- Drop the "in the same order" part of the reproduction: enemies are released individually as they die (EnemyShip.cs:102) or retire (EnemyShip.cs:69), so the LIFO pool ends up ordered by death order rather than spawn order, and each larger wave appends brand-new instances. The accurate claim is that the same SET of speed values recurs - once the pool has grown to the largest wave size reached, no enemy in the rest of the session ever draws a new speed - not that the speeds recur in the same sequence. Severity lowered to minor: the reset is genuinely incomplete and Asteroid.cs:32-36 proves the project knows the right hook, but every wave still contains a spread of speeds and, with spawn Y randomised per spawn (EnemySpawner.cs:95-98) and reuse order shuffled by death order, the repetition is barely perceptible in play. Proposed fix stands: move the draw into Fly() (or a public reset called from OnSpawn) and leave only GetComponent in Awake; no call sites change.
- Keep the claim but fix the attribution and drop the invented arithmetic. _firstWaveSize is a field of Game (Game.cs:21), not EnemySpawner, and wave size is computed in Game.GetWaveSize (Game.cs:170-173). Level 1 does NOT instantiate six ships: Spawn() staggers spawns by _spawnDelay (EnemySpawner.cs:91-101) and enemies return to the pool as soon as they die or cross the off-screen trigger (ObjectRemover.cs:5-10 -> EnemyShip.Remove -> Released -> SpawnPool.Release, SpawnPool.cs:82-87), so the pool grows only to the peak number of simultaneously active enemies, normally well below the wave size. "The same six speeds in the same order" is also unsupported — reuse is LIFO over ObjectPool's internal list, so order follows release order, which varies with how the player plays. Accurate statement: once the pool has grown to K instances (K = the highest number of enemies ever alive at once), exactly K flight speeds exist, each drawn once at instantiation, and every enemy for the rest of the session — across all later levels and across restarts, since RestartGame reuses the same pools — flies at one of those K speeds and never at a new one. The serialized 0.5-3 range is therefore sampled K times per session instead of once per spawn. Proposed fix is unchanged and correct: move the draw into Fly(), leaving only GetComponent<Rigidbody2D>() in Awake.

<sub>Слито из 2 формулировок других ревьюеров.</sub>

---

### 21. SRP: EnemySpawner is a spawner, an object-pool warehouse for four things it does not spawn, and the game's victory condition

**major** · ось: SOLID: SRP and OCP · `Assets/Scripts/Ships/Enemy/EnemySpawner.cs:6` · требует редактора Unity · с особым мнением

**Нарушенное правило.** Violated rule: Single Responsibility Principle. Three separate triggers for change land on this one file: change the spawn pattern (edit EnemySpawner), change how anything an enemy emits is pooled (edit EnemySpawner), change what counts as clearing a wave — for example 'survivors that fly off-screen should not count' (edit EnemySpawner). The third one is also a layering inversion: Game.cs:35 subscribes `_enemySpawner.WaveCleared += WinLevel`, so the rule that ends the level is owned by the object that produces enemies rather than by the object that runs the match.
What it should look like: EnemySpawner should only place enemies at intervals and announce what happened to them.

**Почему это дефект.** Each responsibility has its own private state (_spawnDelay/_spawnPosition*, the five pools, _waveSize/_retiredCount/_isRestarting) and its own reason to change, and they are entangled: _isRestarting (22, 42, 50, 80) exists purely because emptying the pools (a pool-ownership concern) fires Retired events that the wave counter (a rules concern) would otherwise miscount. That flag is the SRP violation made visible — it is a guard one responsibility needs against another responsibility living in the same class.

**Код:**

```csharp
Reason to change #1, spawning: `_spawnDelay`, `_spawnPositionX`, `_minSpawnPositionY`, `_maxSpawnPositionY` (EnemySpawner.cs:9-12) and the Spawn coroutine (89-104).
Reason #2, pool ownership: five pool fields (14-18), all five constructed in Awake (27-36), all five emptied in StartWave (44-48), and OnEnemyCreated injecting four of them into each ship (66-71). Four of those pools hold objects the spawner never spawns — explosions, dead bodies, enemy bullets and bullet hit effects.
Reason #3, the victory condition: `_waveSize`, `_retiredCount` (20-21), `WaveCleared` (25), the reset at 51-52, and OnEnemyRetired (78-87) — `_retiredCount++; if (_retiredCount >= _waveSize) WaveCleared?.Invoke();`. That `>=` is the rule that decides the player wins the level, and it lives in a spawner.
Reason #4, kill relaying: `EnemyDied` (24) and OnEnemyDied (73-76).
The pool ownership also forces three public prefab getters that exist only for this class: EnemyShip.cs:20-21 `ExplosionPrefab` / `DeadBodyPrefab`, EnemyShooter.cs:16 `AmmoPrefab`, consumed by the reach-through chain at EnemySpawner.cs:29-34 (`_prefab.GetComponent<EnemyShooter>().AmmoPrefab`, then `ammoPrefab.HitEffectPrefab`).
Contrast, same project: PlayerShooter.cs:19-23 builds its own ammo and hit-effect pools from its own serialized prefab, with no outside class reaching into it.
```

**Как чинить.** Split three ways.
(a) `EnemyFactory` (MonoBehaviour on the same GameObject): takes over lines 14-18, 27-36, 44-48 and 66-71. It serializes the enemy ship, explosion, dead body, ammo and hit-effect prefabs directly, owns the five SpawnPools, exposes `EnemyShip Get(Vector3 position)`, `void ReleaseAll()` and the `Created` hook that calls ship.Init(...). This deletes the reach-through at EnemySpawner.cs:29-34 and lets you delete the three public prefab getters at EnemyShip.cs:20-21 and EnemyShooter.cs:16.
(b) `Wave` (plain C# class, no MonoBehaviour): takes over 20-21, 51-52 and 78-87. `Wave(int size)`, `void Register()`, `bool IsCleared`, `event Action Cleared`. The _isRestarting guard disappears because the Wave object is simply recreated at the start of each level instead of being reset while pool events are still firing.
(c) `EnemySpawner`: keeps 9-12 and the Spawn coroutine, holds a `Wave` field, and re-raises `WaveCleared` so Game.cs:35 needs no change at all. StartWave becomes `_factory.ReleaseAll(); _wave = new Wave(waveSize); _wave.Cleared += ...; _spawning = StartCoroutine(Spawn());`.
Wiring afterwards: Game keeps its single `_enemySpawner` reference; EnemySpawner holds a serialized reference to EnemyFactory; KillCounter (which already requires EnemySpawner on the same GameObject, KillCounter.cs:4) is unaffected.

**Риск правки.** Medium. The five prefab references currently resolve through the enemy prefab at runtime and would become explicit inspector fields on EnemyFactory — assign all five or pools construct on null. Recheck the restart path specifically: StartWave currently empties all five pools before spawning (44-48), and dropping that step leaks last level's bullets and explosions into the new one.

**Особое мнение скептика.** Every line number cited is accurate, but the SRP indictment built on them does not hold as stated. (1) The headline leg — "the game's victory condition lives in a spawner" — is misattributed. Game.cs:186-195 WinLevel IS the victory condition: it guards on _isLevelOver, stops both spawners and starts the warp jump. EnemySpawner.cs:85-86 only publishes a neutral fact named WaveCleared, not LevelWon, and it is the only object that can: it receives waveSize at line 38 and is the sole subscriber to each ship's Retired (line 70). "Announce what happened to them" is the reviewer's own prescription, and line 86 is that announcement. (2) The fix disproves the layering claim: (c) states EnemySpawner "re-raises WaveCleared so Game.cs:35 needs no change at all" — every inter-class boundary is byte-identical afterwards, so no inversion is being corrected, only code moved. (3) Leg 4 is padding: EnemySpawner.cs:24 + 73-76 is a four-line relay, and it is mandatory — ships are instantiated lazily inside SpawnPool.Create (SpawnPool.cs:61-69), so KillCounter can never hold them; aggregating a per-instance event into a class-level one is the same mechanism fix (c) itself relies on. (4) Leg 2's structure is forced, not chosen: `created:` occurs exactly once in the project (EnemySpawner.cs:35) and is the only per-instance init hook for a lazily created pooled ship (SpawnPool.cs:13, 66), so whoever owns the enemy pool must own the sub-pools it injects at line 68. The PlayerShooter.cs:21-22 contrast is not comparable — one never-pooled player instance can self-own its pools; EnemyShooter sits on a prefab instantiated N times, where per-ship pools would fragment. (5) _isRestarting is a pool-contract artifact, not an SRP scar: I traced line 44 -> SpawnPool.Release (82) -> PrepareForStorage (76) -> OnDespawn -> EnemyShip.cs:69 Retired?.Invoke. OnDespawn firing on release is SpawnPool's contract (SpawnPool.cs:78); a separate Wave subscribed to that same event would replay identically. (6) Scope: a 105-line class that is correct, with one clear verb, would gain two new classes plus re-raise plumbing for an unchanged public API — disproportionate for a 42-script student project, and the brief forbids proposing structure beyond what fits this size.

**Уточнения проверяющих.**

- The finding stands as written; only one sentence of the proposed remedy is over-optimistic and should be softened. It claims "the _isRestarting guard disappears because the Wave object is simply recreated at the start of each level". Recreating it is not sufficient on its own: Retired fires from EnemyShip.OnDespawn (EnemyShip.cs:69) *during* the ReleaseAll sweep, i.e. while the previous Wave is still the subscribed one, and Game.WinLevel's own `_isLevelOver` guard has already been cleared at Game.cs:142 before StartWave is called at Game.cs:146 — so a stale Wave reaching its threshold mid-sweep could still re-raise WaveCleared into a fresh level. The remedy must detach/replace the Wave (or null the spawner's _wave field) *before* calling _factory.ReleaseAll(), not after. With that ordering the guard genuinely disappears, which is still the point being made: the flag is only needed because the two responsibilities share a class.
- Sharpen reason #3: EnemySpawner does not own "the victory condition" outright — Game decides the win (Game.cs:186-195 WinLevel plays the warp jump) and Game owns the wave size (Game.cs:170-173 GetWaveSize, passed in at :146). What lives in the spawner is the wave-depletion bookkeeping (_waveSize/_retiredCount/_isRestarting and the >= threshold at :85), which is still a third block of private state with its own reason to change. The finding's strongest, unarguable legs are the other two: a pool warehouse for four object types the class never spawns (fields 14-18, Awake 27-36, ReleaseAll 44-48, Init 68), built by reaching through into three foreign prefab getters (EnemySpawner.cs:29/31/32/34 consuming EnemyShip.cs:20-21 and EnemyShooter.cs:16, each of which has no other consumer in the project), against the in-project counter-example PlayerShooter.cs:21-22. Also note the proposed fix's ordering: in 'StartWave becomes _factory.ReleaseAll(); _wave = new Wave(waveSize); ...' the ReleaseAll still fires Retired while the OLD Wave is the one subscribed, so the new Wave must be created (or the old one detached) before ReleaseAll for the _isRestarting guard to genuinely disappear.
- The three-way SRP split does not survive. What does survive is a much narrower encapsulation point at minor severity: EnemySpawner.cs:29-34 reaches through the enemy prefab to harvest three prefabs it needs only to build shared pools (`_prefab.GetComponent<EnemyShooter>().AmmoPrefab`, then `ammoPrefab.HitEffectPrefab`), and the only reason EnemyShip.cs:20-21, EnemyShooter.cs:16 and Ammo.cs:17 expose public prefab getters at all is to feed that chain. The proportionate fix is serialization, not new classes: give EnemySpawner four more [SerializeField] prefab fields (explosion, dead body, enemy ammo, hit effect) assigned in the Inspector, which deletes lines 29-34's traversal and lets all four public prefab getters go away. No EnemyFactory, no Wave class, no change to Game.cs or KillCounter.cs.

---

### 27. Enemy ships spawn inside the visible frame at the project's own default 16:9 - a third of each ship pops into existence at the right edge instead of flying in

**major** · ось: Добор по пробелу · `Assets/Scripts/Ships/Enemy/EnemySpawner.cs:10` · требует редактора Unity · единогласно

**Сценарий воспроизведения.** Launch at the project's declared default 1920x1080 (16:9) and press Start. Unity's docs confirm orthographicSize is the half-height of the viewing volume and the width is derived from Camera.aspect, which defaults to the screen aspect (docs.unity3d.com/ScriptReference/Camera-orthographicSize.html, Camera-aspect.html), so with size 5 at x=0 the frame spans x = -8.889 .. +8.889. The enemy body quad is 0.27 units tall in the child's local space, the child is rotated 90 deg about Z (so that 0.27 maps to world X) and scaled 3x, giving a world half-extent of 0.27/2 * 3 = 0.405. A ship spawned at x=9 therefore spans x = 8.595 .. 9.405, and 8.889 - 8.595 = 0.294 units of it - about 32 px at 1920 wide, roughly 36% of its 0.81-unit (87 px) width - is already on screen on the very first frame it is rendered: SpawnPool.Get sets the position and calls SetActive(true) in the same frame, and nothing in EnemyShip.OnSpawn (EnemyShip.cs:57-63) hides the renderer. Game.StartLevel calls StartWave (Game.cs:146) and the Spawn coroutine's first iteration runs before its first yield, so enemy #1 is there the instant the start screen hides; enemies #2..#6 then pop in one every 3 s (_spawnDelay: 3). Watch the right edge for ~15 s and count six pops; it repeats for every wave (6 enemies on level 1, 9 on level 2, ...) and after every warp jump. The same frame also spawns that enemy's first bullet at the shoot point x = 9 - 0.6 = 8.4, which is 0.49 units inside the frame, and ScreenArea.Contains returns true there (viewport x = 0.973), so the shot sound plays as well - a bullet and its sound appear next to a half-drawn ship. The ship then crawls the rest of the way in at 0.5..3 units/s (Enemy Ship.prefab:306-307, EnemyMover), so at minimum speed the half-visible pop is on screen for ~0.6 s before the ship is whole.

**Почему это дефект.** It is not taste: the spawn X is 0.111 units outside a frame edge that the ship overhangs by 0.405 units, so the arithmetic is simply wrong by ~0.3 units at the resolution the project itself declares. The author's intent to spawn off-screen is provable from the code: the project already carries ScreenArea.Contains (Assets/Scripts/ScreenArea.cs:5-15) and EnemyShooter.cs:54 uses it specifically to suppress the shot sound for off-screen enemies - a guard that can never fire on an enemy's first shot because the shoot point at x=8.4 is inside the frame. The exit side of the same boundary was already flagged (kill triggers at y=+/-10, wave-clear at x=-14.1); this is the entry side of the identical mistake. The ship becomes hidden at spawn only when 5 * aspect <= 8.595, i.e. aspect <= 1.719 (narrower than 16:9), so the bug is present at 16:9 and at everything wider.

**Код:**

```csharp
Assets/Scripts/Ships/Enemy/EnemySpawner.cs:10
    [SerializeField] private float _spawnPositionX = 9f;
Assets/Scripts/Ships/Enemy/EnemySpawner.cs:93-98
    for (int i = 0; i < _waveSize; i++)
    {
        _pool.Get(new Vector3(
            _spawnPositionX,
            Random.Range(_minSpawnPositionY, _maxSpawnPositionY),
            0));
Assets/Scenes/SampleScene.unity:9238-9243 (EnemySpawner instance, m_Script guid bfdef16c9e9ff4ae5bce13b403dec0cf = EnemySpawner.cs)
  _spawnDelay: 3
  _spawnPositionX: 9
  _minSpawnPositionY: -3
  _maxSpawnPositionY: 3
Camera (Assets/Scenes/SampleScene.unity:6717-6718, transform at :6741)
  orthographic: 1
  orthographic size: 5
  m_LocalPosition: {x: 0, y: 0, z: -10}
ProjectSettings/ProjectSettings.asset:45-46
  defaultScreenWidth: 1920
  defaultScreenHeight: 1080
Assets/Prefabs/Enemy Ship.prefab:63-65 (child "Body sprite", the only renderer under the ship)
  m_LocalRotation: {x: 0, y: 0, z: 0.7071068, w: 0.7071068}   # 90 deg about Z
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 3, y: 3, z: 1}
Assets/Prefabs/Enemy Ship.prefab:111,115-116
  m_Sprite: {fileID: 2145440094, guid: 8c1f41ea49e262e4785cd51c44db339f, type: 3}
  m_DrawMode: 0
  m_Size: {x: 0.26, y: 0.27}
Sprite asset confirmation - Assets/Sprites/Ships/Enemy 1.png.meta:126-132,141 and :51
      name: tinyShip12_0
        width: 26
        height: 27
      internalID: 2145440094
  spritePixelsToUnits: 100
so the sprite's true bounds are 26/100 x 27/100 = 0.26 x 0.27 units; m_Size is the real bounds, not a draw-mode override (m_DrawMode 0 = Simple).
Assets/Scripts/SpawnPool.cs:28-35,52-58
    public T Get(Vector3 position)
    {
        T item = _pool.Get();
        item.transform.position = position;
        return Activate(item);
    }
    private T Activate(T item)
    {
        _active.Add(item);
        item.gameObject.SetActive(true);
        item.OnSpawn();
Assets/Prefabs/Enemy Ship.prefab:28 (ShootPoint child)
  m_LocalPosition: {x: -0.6, y: 0, z: 0}
Assets/Scripts/Ships/Enemy/EnemyShooter.cs:51-55
            Ammo ammo = _ammoPool.Get(_shootPoint.position, _shootPoint.rotation);
            ammo.Init(_hitEffectPool);
            if (ScreenArea.Contains(_camera, _shootPoint.position))
                _shotAudioSource.PlayOneShot(ammo.ShotSound.Clip, ammo.ShotSound.Volume);
```

**Как чинить.** Stop hard-coding the world X and derive it from the camera, in the idiom the project already uses. Add a companion to the existing static helper, e.g. ScreenArea.GetRightEdge(Camera camera) returning camera.transform.position.x + camera.orthographicSize * camera.aspect (the name says what it returns, satisfying the school's GetNextPlace -> GetNextPosition rule), cache Camera.main in EnemySpawner.Awake the way EnemyShooter.cs:20 and ExplosionAnimation.cs:32 already do, replace _spawnPositionX with a serialized _spawnMargin, and compute the X inside the Spawn loop as ScreenArea.GetRightEdge(_camera) + _spawnMargin, with _spawnMargin at least the ship's 0.405-unit half-extent plus slack (1 is a clean value). Computing it per spawn rather than caching it in Awake also keeps it correct if the window ever changes size. If a minimal edit is preferred instead, the serialized scene value must go from 9 to at least 9.3 (0.405 half-extent + slack), but that only fixes 16:9 and leaves the ultrawide case below.

**Риск правки.** Low. The only behavioural side effect is that each enemy takes marginally longer to reach the player - pushing the spawn from x=9 to x=9.9 adds 0.3 s at the 3 u/s maximum speed and 1.8 s at the 0.5 u/s minimum - which lengthens a wave slightly but changes no game rule. Nothing keys off the literal 9: the right ObjectRemover trigger sits at x=15 (SampleScene.unity:9727), so a larger spawn X is still well clear of it, and the wave-clearing count is driven by EnemyShip.OnDespawn, not by position.

**Уточнения проверяющих.**

- The geometry and the reproduction are correct, but two supporting details are wrong and the severity is overstated. (1) Aspect: ProjectSettings.asset:79 sets defaultIsNativeResolution: 1, so a desktop build launches at the display's native resolution rather than the declared 1920x1080 (ProjectSettings.asset:45-46). The pop-in exists only above aspect 8.595/5 = 1.719 - present at 16:9 and wider, absent at 16:10 (half-width 8.0) and 4:3, where the ship is fully off-screen at spawn. So the claim should read "at 16:9 and wider, which the project declares as its default screen size", not "at the project's own default" as if it were unconditional. (2) Exposure time: with _minSpeed: 0.5 (Enemy Ship.prefab:306) the tail must travel 9.405 - 8.889 = 0.516 units to clear the right edge, which takes about 1.0 s, not the "~0.6 s" stated. (3) Visible fraction: the sprite mesh is Tight (Enemy 1.png.meta:48 spriteMeshType: 1) and the auto-generated outline at Enemy Ship.prefab:166-177 spans local y in [-0.1066, +0.1336], so the drawn ship occupies world x in [8.599, 9.320] rather than [8.595, 9.405]; the visible slice is ~0.29 units (~32 px at 1920) which is ~40% of the drawn ship - the headline number survives. Severity should be minor, not major: this is a sub-second cosmetic pop at the frame edge that is aspect-conditional, breaks no gameplay (the ship's collider and its first bullet both travel leftward with ample warning, so nothing unfair reaches the player) and violates no named school rule. The proposed fix (derive the spawn X from camera.orthographicSize * camera.aspect plus a serialized margin, in the ScreenArea helper the project already owns) is sound as written.
- Two adjustments. (1) The resolution anchor is wrong: ProjectSettings.asset:79 sets defaultIsNativeResolution: 1 with fullscreenMode: 1 (:105), so defaultScreenWidth/Height 1920x1080 (:45-46) is never used by a standalone build - the finding must not rest on "the project's declared default". It survives on stronger evidence instead: the committed editor Game view renders 1280x720, exactly 16:9 (UserSettings/Layouts/default-6000.dwlt:646; same in CurrentMaximizeLayout.dwlt:73), so it reproduces in the author's own editor, and in a build on any display of aspect >= 8.5992/5 = 1.72 (16:9 and wider; a 16:10 or 3:2 display hides it). (2) The magnitude should be stated as opaque pixels, not quad extent: the tight sprite mesh's leading edge is at 8.5992, so 0.290 units show, which is ~21 px at the 1280x720 Game view and ~31 px at 1080p - about 40% of the ship's 0.72-unit opaque width, and it is the narrow nose tip rather than the ship's body. The more conspicuous half of the same defect is the bullet: its full ~0.69-unit flame sprite (Enemy Bullet.prefab:4940) materialises entirely in view at x=8.4, ~0.49 units (35-53 px) inside the right edge, with its shot sound, on the very frame the ship appears.
- Two corrections. (1) The "project's declared default 1920x1080" premise is wrong: ProjectSettings.asset:79 `defaultIsNativeResolution: 1` with :105 `fullscreenMode: 1` means a standalone build opens fullscreen at the display's native resolution, so defaultScreenWidth/Height (:45-46) never applies. The finding is therefore conditional on the display, not on a project setting — its own threshold is the accurate part: visible iff 5*aspect > 8.595, i.e. aspect > 1.719. That is every 16:9 and wider monitor (the common case) but NOT the 16:10 of a Mac laptop (half-width 8.0), which is presumably why the author never saw it. State it as "on 16:9 and wider displays", not "at the project's own default". (2) The extent arithmetic is off in detail though not in direction: the sprite's bottom 3 pixel rows are empty, so the ship is not symmetric about its pivot — it spans -0.405 to +0.315 in world X, total width 0.72 units (78 px at 1920), and the slice left on screen at 16:9 is 0.294 units ~ 32 px, which is 41% of the ship, not "36% of its 0.81-unit width". The headline "a third of each ship" is if anything understated.

---

### 8. EnemySpawner builds its Awake-time pools by reaching several levels into the enemy prefab, forcing EnemyShip/EnemyShooter/Ammo to publish prefab getters that only the spawner reads

**major** · ось: OOP and encapsulation · `Assets/Scripts/Ships/Enemy/EnemySpawner.cs:29` · с особым мнением

**Нарушенное правило.** Violated rule: encapsulation / Law of Demeter - a class must not navigate through another object's components and serialized fields to assemble that object's collaborators, and a type must not add public members that exist only to let one external class do so. EnemySpawner.cs:29 is a four-hop chain (_prefab -> GetComponent<EnemyShooter>() -> AmmoPrefab -> HitEffectPrefab) across three types. It should look like the player side: each object owns and builds the pools for the things it spawns, so EnemyShooter creates its own SpawnPool<Ammo>/SpawnPool<ExplosionAnimation> from its own _prefab and EnemyShip creates its own explosion and dead-body pools, and none of ExplosionPrefab, DeadBodyPrefab or AmmoPrefab needs to be public at all.

**Почему это дефект.** Three public properties whose only caller is a single foreign class are, by definition, leaked internals - grep confirms zero other readers. They also force EnemyShip.Init into a four-parameter list of pools it did not ask for and mostly just forwards (line 54 passes two of them straight to _shooter). Adding a second projectile or a second death effect to the enemy means editing EnemySpawner, EnemyShip, EnemyShooter and Ammo together; that is exactly the coupling the school's encapsulation rule exists to prevent.

**Код:**

```csharp
EnemySpawner.cs:27   private void Awake()
EnemySpawner.cs:29       Ammo ammoPrefab = _prefab.GetComponent<EnemyShooter>().AmmoPrefab;
EnemySpawner.cs:31       _explosionPool = new SpawnPool<ExplosionAnimation>(_prefab.ExplosionPrefab);
EnemySpawner.cs:32       _deadBodyPool = new SpawnPool<EnemyDeadBody>(_prefab.DeadBodyPrefab);
EnemySpawner.cs:33       _ammoPool = new SpawnPool<Ammo>(ammoPrefab);
EnemySpawner.cs:34       _hitEffectPool = new SpawnPool<ExplosionAnimation>(ammoPrefab.HitEffectPrefab);
EnemySpawner.cs:68       ship.Init(_explosionPool, _deadBodyPool, _ammoPool, _hitEffectPool);

EnemyShip.cs:20      public ExplosionAnimation ExplosionPrefab => _explosion;
EnemyShip.cs:21      public EnemyDeadBody DeadBodyPrefab => _deadBody;
EnemyShip.cs:46      public void Init(
EnemyShip.cs:47          SpawnPool<ExplosionAnimation> explosionPool,
EnemyShip.cs:48          SpawnPool<EnemyDeadBody> deadBodyPool,
EnemyShip.cs:49          SpawnPool<Ammo> ammoPool,
EnemyShip.cs:50          SpawnPool<ExplosionAnimation> hitEffectPool)
EnemyShooter.cs:16   public Ammo AmmoPrefab => _prefab;
Ammo.cs:17           public ExplosionAnimation HitEffectPrefab => _hitEffectPrefab;

Grep results - every one of these getters has exactly one caller, EnemySpawner:
  ExplosionPrefab  -> EnemySpawner.cs:31 only
  DeadBodyPrefab   -> EnemySpawner.cs:32 only
  AmmoPrefab       -> EnemySpawner.cs:29 only

The same project already shows the correct shape on the player side:
PlayerShooter.cs:21      _ammoPool = new SpawnPool<Ammo>(_prefab);
PlayerShooter.cs:22      _hitEffectPool = new SpawnPool<ExplosionAnimation>(_prefab.HitEffectPrefab);
```

**Как чинить.** Give each type ownership of the pools for what it spawns. EnemyShooter builds its own SpawnPool<Ammo> and SpawnPool<ExplosionAnimation> in Awake from its own _prefab, exactly as PlayerShooter.cs:21-22 does, so Ammo.HitEffectPrefab and EnemyShooter.AmmoPrefab become private. If the pools genuinely need to be shared across all enemy instances (they should be - one pool per wave, not one per ship), keep them in EnemySpawner but pass the prefabs in as EnemySpawner's own [SerializeField] fields instead of mining them out of _prefab; EnemyShip.ExplosionPrefab and DeadBodyPrefab then become private too and EnemyShip.Init shrinks to the two pools it actually uses.

**Риск правки.** Medium if the pools are moved into each ship, because you would then get one pool per enemy instead of one shared pool - that changes allocation behaviour across a wave. The low-risk variant is the second one: add [SerializeField] prefab fields on EnemySpawner and wire them in the inspector, which needs an editor touch on the spawner object but leaves all runtime behaviour identical.

**Особое мнение скептика.** Every quoted line is accurate (EnemySpawner.cs:29-35, EnemyShip.cs:20-21/46-55, EnemyShooter.cs:16, Ammo.cs:17) and the grep is accurate, but the named rule and the contrast both collapse under reading.

1. The "correct shape" the finding holds up refutes it. PlayerShooter.cs:22 is `new SpawnPool<ExplosionAnimation>(_prefab.HitEffectPrefab)` — PlayerShooter reaches into another type's serialized prefab field to build a pool for effects that type spawns, exactly the pattern the finding calls a leak. The player side is shorter, not different in kind. The finding's own proposed fix trips on this: it says Ammo.HitEffectPrefab "become[s] private", but PlayerShooter.cs:22 reads it (grep: Ammo.cs:17 has two callers, EnemySpawner.cs:34 and PlayerShooter.cs:22), so that half of the fix does not compile. Reach-through to a prefab's effect prefab is this project's consistent idiom, applied on both sides.

2. The named rule is not the school's. The exemplar is two files with no public members and no statement about encapsulation; and the graded "genius" version itself chains through other objects: PlacesMover.cs:36 `_places[_index].transform.position` (three hops into a foreign Transform), PlacesMover.cs:17 `_placesArea.GetChild(i)`, BulletShooter.cs:22 `_target.position`. A Law-of-Demeter rule that the exemplar violates in the file the graders rewrote is not a rule the graders will fail this on. Rule 4's "name the violated rule and show what it should look like" is not met.

3. "A public property with one caller is by definition a leaked internal" is asserted, not proven. One caller is a fact about today's call graph, not a rule violation, and the getters return the enemy's configured effects — defensible read-only API.

4. The primary fix is a regression. EnemySpawner.cs:44-48 releases all four pools on StartWave to clear the field between waves. Move SpawnPool<Ammo>/SpawnPool<ExplosionAnimation> into each EnemyShooter's Awake "exactly as PlayerShooter.cs:21-22 does" and the pools become per-ship, so enemy bullets already in flight when a wave restarts are in pools the spawner can no longer ReleaseAll. The finding concedes this mid-sentence and falls back to a second, different fix.

5. No defect exists. GetComponent on a prefab asset is valid, and EnemyShip.cs:4 `[RequireComponent(..., typeof(EnemyShooter))]` guarantees the component, so EnemySpawner.cs:29 cannot null. EnemyShip.Init uses two of its four pools directly (Die(), EnemyShip.cs:94/98) and forwards two to _shooter (line 54), a component it is required to own — standard composition, not "pools it did not ask for". No reproduction scenario, no wrong behaviour, and "major" is unsupportable.

**Уточнения проверяющих.**

- The defect stands as stated, but the proposed fix needs two corrections. (1) Its primary suggestion - "EnemyShooter builds its own SpawnPool<Ammo> and SpawnPool<ExplosionAnimation> in Awake from its own _prefab, exactly as PlayerShooter.cs:21-22 does" - would break wave reset and must not be recommended. EnemyShip instances are created lazily inside the pool (SpawnPool.cs:61-69), so a per-shooter Awake pool means one ammo pool per pooled ship, and EnemySpawner.StartWave clears all five pools centrally (EnemySpawner.cs:44-48, including _ammoPool.ReleaseAll() and _hitEffectPool.ReleaseAll()) on every level start from Game.cs:146. With per-shooter pools the spawner would hold no handle to recall enemy bullets already in flight, so they would survive into the next level. Only the finding's second alternative is correct: keep the five pools in EnemySpawner and give it its own [SerializeField] ExplosionAnimation / EnemyDeadBody / Ammo prefab fields instead of mining them out of _prefab; EnemyShip.ExplosionPrefab, EnemyShip.DeadBodyPrefab and EnemyShooter.AmmoPrefab plus their three backing fields then delete outright, and EnemyShip.Init shrinks to the two pools it actually stores. (2) "Ammo.HitEffectPrefab becomes private" is wrong - PlayerShooter.cs:22 is a second, legitimate reader (its own serialized _prefab), so that one getter must stay public.
- Two parts of the finding are wrong and must be dropped. (1) Ammo is not in the leak set: Ammo.cs:17 HitEffectPrefab is also read by PlayerShooter.cs:22, so it is not "read only by the spawner" and the proposed "Ammo.HitEffectPrefab ... become private" would fail compilation. (2) The primary proposed fix - "EnemyShooter builds its own SpawnPool<Ammo>/SpawnPool<ExplosionAnimation> in Awake, exactly as PlayerShooter.cs:21-22 does" - is invalid under Unity semantics and would be a regression. PlayerShooter sits on a single scene object (Game.cs:12 holds the PlayerShip reference; PlayerShip.cs:31 builds a capacity-1 pool), while EnemyShooter rides a prefab that SpawnPool.cs:63 Instantiates N times, and Unity calls Awake once per clone ("For the clone and each object in its hierarchy, Unity calls the Awake and OnEnable methods on a MonoBehaviour or Component only if it's active in the hierarchy when you call this method", Object.Instantiate docs). Per-instance pools would give one ammo pool per enemy ship and would leave EnemySpawner.cs:45-46 `_ammoPool.ReleaseAll()` / `_hitEffectPool.ReleaseAll()` with nothing to clear, so enemy bullets in flight would survive a wave restart. Also, line 29 is a three-hop chain (_prefab -> GetComponent<EnemyShooter>() -> AmmoPrefab), not four; HitEffectPrefab is read on line 34 from a local. Corrected claim: EnemyShip.cs:8-9,20-21 and EnemyShooter.cs:7,16 carry serialized prefab references and public getters that the owning components never use themselves and that exist solely so EnemySpawner.Awake can mine them; the only sound fix is the reviewer's fallback - move the ExplosionAnimation, EnemyDeadBody and Ammo prefab references onto EnemySpawner as its own [SerializeField] fields (the pools must stay shared on the spawner), make the three getters private or delete the now-unused fields, and shrink EnemyShip.Init to the two pools it actually stores.
- Only a narrow, minor residue survives, and I do not think it is worth reporting on its own: EnemyShooter.cs:7 `_prefab` and EnemyShip.cs:8-9 `_explosion`/`_deadBody` are serialized fields their own class never uses for anything except the getter (grep confirms no other reference in either file) — they exist purely as data carriers for EnemySpawner's Awake. That is a mild cohesion smell at most, it is the same pattern the player side uses, and it is not tied to any rule in the exemplar.

<sub>Слито из 2 формулировок других ревьюеров.</sub>

---

### 17. A wave only clears once the slowest survivor crosses x=-14.1, five units past the left edge of the screen - up to ~10 s of waiting on an invisible enemy

**major** · ось: Physics · `Assets/Scripts/Ships/Enemy/EnemySpawner.cs:85` · требует редактора Unity · с особым мнением

**Сценарий воспроизведения.** Start the game and never press Space. Wave 1 is three enemies (SampleScene.unity:8280 `_firstWaveSize: 3`), spawned 3 s apart (`_spawnDelay: 3`). An enemy drawn at the minimum speed of 0.5 u/s leaves the visible area when its centre reaches x = -9.28, but is only counted as retired when its collider touches x = -14.5, i.e. centre x = -14.11 - another 4.83 units, or 9.7 seconds. During those ~10 seconds there is not a single enemy on screen, the player has no way to act on the one that is holding the level open (bullets fly right, away from it), and nothing indicates whether more enemies are coming. Total pacifist wave time is ~46 s for one slow ship travelling 23.1 units at 0.5 u/s.

**Почему это дефект.** WaveCleared is the level's win condition, so the position of an off-screen cleanup trigger is silently acting as a gameplay timer. The delay is unbounded from the player's point of view (they cannot see or influence the enemy) and scales with how slow the RNG made the last survivor. This is not a tuning preference: the win moment is decided by a collider the designer placed for pooling hygiene, five units outside anything the player can see.

**Код:**

```csharp
EnemySpawner.cs:78-87 `private void OnEnemyRetired(EnemyShip ship) { if (_isRestarting) return; _retiredCount++; if (_retiredCount >= _waveSize) WaveCleared?.Invoke(); }`
EnemyShip.cs:65-70 `public void OnDespawn() { _shooter.StopShoot(); _mover.Stop(); Retired?.Invoke(this); }` - so retirement happens only in the pool's release path, which for a survivor is ObjectRemover.cs:5-10.
ObjectRemover left: SampleScene.unity:9505 `--- !u!4 &1635448958`, 9514 `m_LocalPosition: {x: -15, y: 0, z: 0}`, 9515 `m_LocalScale: {x: 1, y: 19, z: 1}` => x in [-15.5,-14.5].
Enemy Ship.prefab:306-307 `_minSpeed: 0.5` / `_maxSpeed: 3`; EnemyMover.cs:20 `_rigidbody2D.linearVelocity = transform.right * -_speed;`
Spawn at SampleScene.unity:9241 `_spawnPositionX: 9`; enemy collider half-width ~0.39 (Body sprite localScale 3, sprite m_Size 0.26x0.27).
Camera half-width = 5 * 1920/1080 = 8.889 (ProjectSettings.asset:45-46 `defaultScreenWidth: 1920` / `defaultScreenHeight: 1080`).
```

**Как чинить.** Decouple retirement from cleanup. EnemySpawner already has everything it needs: have EnemyShip report itself retired when it leaves the camera (ScreenArea.Contains(Camera.main, transform.position) == false after having been inside), and leave ObjectRemover purely as the pool-return boundary. A cheaper editor-only fix is to move ObjectRemover left to x ~ -10.5 so the despawn line is just outside the 16:9 view; that also shortens the pointless off-screen tail for asteroids and bullets.

**Риск правки.** Moving the remover is low risk but aspect-ratio dependent - at a wider aspect (21:9, half-width 11.7) x=-10.5 would delete ships while still visible, so the code-side ScreenArea fix is the safer of the two. The code fix needs care that an enemy spawned at x=9 (already outside the 8.889 half-width) is not retired the instant it spawns: gate on "has been visible at least once".

**Особое мнение скептика.** Geometry checks out but the framing does not — this is tuning, not a defect. (1) The mechanic is exactly the design the brief states ("a level is won by destroying or outliving a wave of N enemies; an enemy that flies off-screen counts toward the wave"). EnemySpawner.cs:78-87 counts `Retired`; EnemyShip.cs:65-70 raises it from `OnDespawn`, and EnemyShip.cs:87-104 -> SpawnPool.cs:82-87 -> SpawnPool.cs:76-80 routes a kill through the same path, so `Retired` means exactly "this enemy left play, by death or by exit". That is one coherent rule, faithfully implemented; nothing hangs, nothing is corrupted, the wave always clears. (2) The "collider the designer placed for pooling hygiene" premise is contradicted by the scene: the removers are a deliberate symmetric world-bounds box — left SampleScene.unity:9505/9514-9515 (x=-15, scale y 19), right :9718 (x=+15, same scale), top :8056 and bottom :9596 (y=+-10, m_LocalEulerAnglesHint z=90, scale 31 -> horizontal walls spanning x in [-15.5,15.5]). Spawners deliberately live outside the camera too (EnemySpawner _spawnPositionX 9 at :9241, asteroids 10 at :9781), so a margin past the 8.889-unit camera half-width is the intended play area, not an accident. Moving that wall to -10.5 is a scene tuning number, i.e. taste. (3) The magnitude is bounded and dominated by an intended knob, not by the wall: with Enemy Ship.prefab:306-307 `_minSpeed: 0.5` and m_GravityScale 0 (:282) the slowest enemy is *visible* for (9+9.28)/0.5 = 36.6 s before the 4.83-unit invisible tail (9.7 s worst case; expected min of three Uniform(0.5,3) draws is 1.125 u/s -> ~4.3 s). The player therefore had ~36 s of shooting opportunity on the very ship the scenario says they "cannot act on", and the long wave is caused by `_minSpeed: 0.5`, a designer value, far more than by the wall position. (4) The claim of dead air is wrong: the asteroid spawner is not stopped until the level ends (Game.cs:186-195) and spawns a pair every 0.4 s (SampleScene.unity:9780, AsteroidSpawner.cs:45-57), so the player is dodging continuously during the tail. (5) The reproduction requires deliberately never firing, and the proposed fix (per-enemy ScreenArea polling plus a "was inside" latch) adds machinery disproportionate to this project for a bounded few-second pacing preference.

**Уточнения проверяющих.**

- The mechanism is right but the headline number is the extreme tail, and the "dead screen" framing is overstated. (1) The ~10 s figure needs the RNG to draw near the 0.5 minimum: the gap is 4.81 units (invisible at centre -9.29 on a 16:9 display, retired at -14.10), so the delay is 4.81/v with v uniform on [0.5,3) — expected value ~3.4 s, and only ~4% of draws give v <= 0.6 (a >=8 s tail). Honest statement: "typically ~2-4 s, up to ~10 s when the last survivor drew a near-minimum speed". (2) The delay is display-dependent, not fixed: half-width = 5*aspect, so at 16:10 (a MacBook panel) the gap grows to ~6.1 units and at 21:9 shrinks to ~2.4 units. (3) "Not a single enemy on screen... nothing indicates whether more enemies are coming" overstates it — AsteroidSpawner keeps spawning a top and a bottom asteroid every 0.4 s (AsteroidSpawner.cs:45-57, SampleScene.unity:9780) right up until WinLevel calls StopSpawn (Game.cs:193), so the player is still actively dodging, not staring at an empty field. (4) The boundary is a uniform global cleanup margin, not something placed for enemies: all four removers sit 5+ units outside the view (left x=-15, right x=+15 at :9760-9762, top/bottom y=+/-10), so the value is a shared tuning constant rather than a mistake specific to the win condition.
- The mechanism and the repro are correct but the framing needs three fixes. (a) The dead window is longer than claimed, not shorter: PlayerMover.cs:38 zeroes horizontal velocity every frame and the ship sits at _startPosition x=-6 (Player Ship.prefab:217) while bullets fly +x (BulletMover.cs:17), so a survivor is already unkillable from x=-6, i.e. 8.1 units before retirement (~16 s at 0.5 u/s), of which ~9.6 s is fully off-screen (centre -9.28 to -14.11). (b) "Not a single enemy on screen" is literally true but "nothing on screen" is not - AsteroidSpawner keeps spawning two asteroids every 0.4 s throughout (SampleScene.unity:9780), since Game.cs:182/192 only stops it at Lose/WinLevel. (c) The pacifist wave-1 worst case is ~52 s, not ~46 s: the third enemy spawns at t=6 s (EnemySpawner.cs:100, _spawnDelay 3) and then travels 23.1 units at 0.5 u/s. Also worth noting the worst case is a tail event, since EnemyMover.cs:15 rolls Random.Range(0.5f, 3f) once per pooled instance in Awake; typical last-survivor delay is ~4-5 s of off-screen wait, not 10.

---

### 20. OCP: the player's collision reaction dispatches on concrete type, so one new hazard or pickup costs seven new members across three classes

**major** · ось: SOLID: SRP and OCP · `Assets/Scripts/Ships/Player/PlayerCollisionHandler.cs:32` · требует редактора Unity · с особым мнением

**Нарушенное правило.** Violated rule: Open/Closed — open for extension, closed for modification. Adding one new collidable (a mine, a health pickup, a second hazard) requires editing three existing files and adding seven members: a gravity constant, an event and a branch in PlayerCollisionHandler; an event and a near-duplicate handler in PlayerShip; a Sound field and a Play method in PlayerShipAudio. Nothing about the new hazard is new behaviour — it damages, it pushes, it makes a noise, exactly like the two that exist.
What it should look like: one interface and one branch. `public interface IBumpable { int Damage { get; } float BumpGravity { get; } Sound BumpSound { get; } }` implemented by Asteroid (which already has Damage) and by the enemy body component. OnTriggerEnter2D becomes a single `if (other.TryGetComponent(out IBumpable bumpable))` that raises one `event Action<IBumpable> Bumped`; PlayerShip collapses 118-138 into one handler and 58-59 into one event; PlayerShipAudio plays `bumpable.BumpSound`. A new hazard is then a prefab with values in the inspector and zero C# edits — the same shape ObjectRemover already uses.

**Почему это дефект.** The two branches are not two behaviours, they are one behaviour parameterised by two numbers and a clip. The duplication is literal (PlayerShip.cs:118-127 and 129-138 are the same five statements) and the extension cost is measurable at seven members in three files. The project itself demonstrates the correct pattern in ObjectRemover, so this is a rule the codebase already accepts and then breaks here.

**Код:**

```csharp
PlayerCollisionHandler.cs:6-8  three per-type constants: `AsteroidBumpGravityFromAbove`, `AsteroidBumpGravityFromBelow`, `EnemyBumpGravity`.
PlayerCollisionHandler.cs:14-15  `public event Action<Asteroid> AsteroidBumped; public event Action<EnemyShip> EnemyBumped;`
PlayerCollisionHandler.cs:32-49  the if-chain: `if (other.TryGetComponent(out Asteroid asteroid)) { AsteroidBumped?.Invoke(asteroid); PlayBump(...); }` then `if (other.TryGetComponent(out EnemyCollision enemyCollision)) { EnemyBumped?.Invoke(enemyCollision.GetShip()); PlayBump(other, EnemyBumpGravity); enemyCollision.Bump(); }`
PlayerShip.cs:58-59  the mirror events `AsteroidBumped` / `EnemyBumped`.
PlayerShip.cs:118-138  two handlers that differ only in the damage source and the event fired:
  `OnAsteroidBumped(Asteroid asteroid){ TakeDamage(asteroid.Damage); if (_isDead) return; PushOff(); AsteroidBumped?.Invoke(); }`
  `OnEnemyBumped(EnemyShip enemyShip){ TakeDamage(enemyShip.BodyDamage); if (_isDead) return; PushOff(); EnemyBumped?.Invoke(); }`
PlayerShipAudio.cs:7-8 two Sound fields, 12-13 two subscriptions, 22-30 two one-line methods that both call Play(sound).
The damage properties are already the same type: Asteroid.cs:17 `[field: SerializeField] public int Damage` and EnemyShip.cs:18 `[field: SerializeField] public int BodyDamage`.
Contrast, same project: ObjectRemover.cs:5-10 does the polymorphic thing — `IRemoveable removeable = other.GetComponentInParent<IRemoveable>(); removeable?.Remove();` — with no type branch at all.
```

**Как чинить.** Add `IBumpable` next to the existing interfaces in Assets/Scripts/Interfaces. Implement it on Asteroid (Damage already exists at Asteroid.cs:17; add serialized BumpGravity and BumpSound) and on EnemyCollision, which already sits on the enemy's Body sprite child and already owns the `Bump()` side effect (EnemyCollision.cs:9-12) — it returns EnemyShip.BodyDamage for Damage and kills the ship in Bump(). Reduce PlayerCollisionHandler.OnTriggerEnter2D to one TryGetComponent, one `Bumped?.Invoke(bumpable)` and one `PlayBump(other, bumpable.BumpGravity)`. Keep the above/below sign flip local to PlayerCollisionHandler if you want to preserve the asteroid's asymmetric -2 / +4 values: read the magnitude from the interface and choose the sign from `transform.position.y > 0`. Then delete PlayerShip.cs:58-59 and 118-138 in favour of one `OnBumped(IBumpable bumpable)`, and delete the two Sound fields and two methods in PlayerShipAudio in favour of `Play(bumpable.BumpSound)`.

**Риск правки.** Medium. The two bump sounds currently live on the PlayerShipAudio component in the scene and would move onto the Asteroid 1-5 prefabs and the Enemy Ship prefab, so the clips and volumes must be re-assigned by hand on six prefabs or they go silent. The asymmetric asteroid gravity (-2 from above, +4 from below) is easy to flatten by accident — preserve both values explicitly.

**Особое мнение скептика.** The quoted lines are all accurate (PlayerCollisionHandler.cs:6-8,14-15,32-49; PlayerShip.cs:58-59,118-138; PlayerShipAudio.cs:7-8,12-13,22-30; Asteroid.cs:17; EnemyShip.cs:18; ObjectRemover.cs:5-10), but the premise "one behaviour parameterised by two numbers and a clip" is false on the code itself.

(1) The enemy branch has a side effect the asteroid branch does not: PlayerCollisionHandler.cs:47 calls enemyCollision.Bump(), and EnemyCollision.cs:9-12 is `_enemyShip.Die()`. Ramming an enemy destroys it (a wave kill, per the brief); ramming an asteroid does not destroy the asteroid — Asteroid.cs has no death path at all, only Remove() via IRemoveable at 43-46. Two different interactions, not one.

(2) The bump "gravity" is not a property of the hazard. PlayerCollisionHandler.cs:38-40 branches on `transform.position.y > 0` — the PLAYER's screen half, not the asteroid's — and PlayerMover.cs:73-89 makes the same player-position branch for the push (with an extra rotation reset only in the upward case). A `float BumpGravity { get; }` on the collidable cannot express state that belongs to the player.

(3) The proposed remedy provably does not preserve behaviour. "Read the magnitude from the interface and choose the sign from transform.position.y > 0" cannot produce -2f and +4f (PlayerCollisionHandler.cs:6-7) — those differ in magnitude, 2 vs 4, not only sign. And the stated interface `{ Damage; BumpGravity; BumpSound }` carries no member for the enemy's death-on-ram, so as written it deletes behaviour; adding Bump() to it forces Asteroid to implement a no-op.

(4) `Sound BumpSound` on the interface would move an audio asset reference onto Asteroid and EnemyCollision. The project deliberately isolates audio in dedicated components (PlayerShipAudio.cs, Assets/Scripts/Audio/*); the fix trades a speculative OCP gain for a real SRP regression.

(5) The ObjectRemover contrast proves nothing. IRemoveable is used there because removal is literally identical for all four implementors — verified by grep: EnemyShip.cs:5, PlayerShip.cs:6, Ammo.cs:6, Asteroid.cs:6 — one parameterless call, no per-type data. Bumping is not identical, per (1)-(2). Interface where behaviour is shared, concrete types where it diverges, is coherent.

(6) The seven-member cost is hypothetical: there is no third collidable anywhere (15 prefabs, all enumerated in the brief), and the finding's own example, a health pickup, neither damages nor pushes off, so it would not travel the IBumpable path regardless. Two cases with genuinely divergent behaviour is the point at which an abstraction is speculative generality.

(7) No such rule exists in the graded material. I read both exemplar files (../Code style genius/BulletShooter.cs, PlacesMover.cs, ~39 and ~40 lines, no interfaces present); the graded conventions are explicit types, _camelCase serialized fields, cached WaitForSeconds, braceless single statements, no comments, member order, and the GetNextPlace -> GetNextPosition rename. Nothing about OCP or polymorphic dispatch. The project already uses interfaces where they fit (IDamageable, IRemoveable, IPoolable), so this is a deliberate choice, not ignorance of the pattern.

No misbehaviour is claimed or present, so this is not a behaviour defect; and the "violated rule" is not one the graders state, applied to a two-case split whose cases really differ, with a remedy that cannot reproduce the current tuned values.

**Уточнения проверяющих.**

- The rule violation stands, but the rationale and the fix are wrong in three checkable ways and the extension cost is overstated.

(1) "The two branches are not two behaviours, they are one behaviour parameterised by two numbers and a clip" is false. The enemy branch has a third effect the asteroid branch lacks: PlayerCollisionHandler.cs:47 calls `enemyCollision.Bump()`, which is EnemyCollision.cs:9-12 → `_enemyShip.Die()`. Asteroid has no counterpart. The proposed `IBumpable { int Damage; float BumpGravity; Sound BumpSound; }` has no member for it, so applying the fix exactly as written silently deletes "ramming an enemy kills it". The interface needs a `void Bump()` (no-op on Asteroid).

(2) "preserve the asteroid's asymmetric -2 / +4 values ... read the magnitude from the interface and choose the sign" cannot work: PlayerCollisionHandler.cs:6-7 are -2f and +4f, different magnitudes, not one magnitude with two signs. A single `float BumpGravity` yields ±2 or ±4 and changes the effect; the asteroid needs two values or its own positional rule.

(3) "one new hazard or pickup" overreaches on the pickup half: a healing pickup neither damages nor pushes off, so IBumpable as specified would not absorb it either.

Cost, corrected: six new members (gravity constant, event, event, handler, Sound field, Play method) plus edits to five existing methods (OnTriggerEnter2D, and the OnEnable/OnDisable pairs in PlayerShip.cs:35-53 and PlayerShipAudio.cs:10-20) — an if-branch is not a member. And the cheapest new hazard already costs zero C#: Asteroid.Damage is `[field: SerializeField]` (Asteroid.cs:17) and Asteroid 1-5.prefab are five inspector-only variants of one class.

Corrected claim: the player's bump reaction type-switches on Asteroid/EnemyCollision where the project elsewhere dispatches through an interface (ObjectRemover.cs:5-10), producing two near-identical handlers (PlayerShip.cs:118-138) and two near-identical audio methods (PlayerShipAudio.cs:22-30). Real DRY/OCP smell over ~10 duplicated lines with two cases and no behavioural defect — minor, not major.
- The OCP claim and the seven-members-in-three-files count hold, but two parts of the write-up overstate and the proposed fix is under-specified.

1. "The two branches are not two behaviours, they are one behaviour parameterised by two numbers and a clip" is not exact. The enemy branch carries a side effect the asteroid branch has no analogue for: `enemyCollision.Bump()` at PlayerCollisionHandler.cs:47, which calls `_enemyShip.Die()` (EnemyCollision.cs:9-12) — the enemy dies from the collision, the asteroid survives. So the unifying interface needs a `Bump()` member (a no-op on Asteroid) as well as Damage/BumpGravity/BumpSound: four members, not the three listed.

2. The suggested "read the magnitude from the interface and choose the sign from `transform.position.y > 0`" does not preserve current behaviour. The asteroid values are asymmetric in magnitude as well as sign — `-2f` from above vs `+4f` from below (PlayerCollisionHandler.cs:6-7, applied at :38-40) — so one magnitude cannot reproduce both, and applying a position-derived sign would also flip the enemy's fixed `+1f` (:8, :46) that currently never flips. The interface must expose the two gravity values (or the resolved value), not a single magnitude.

3. "one new hazard or pickup" — a pickup does not fit IBumpable, whose contract is damage + push-off + bump sound. The extension-cost claim is sound for a new hazard only.
- The only true residue is literal duplication, not an OCP violation: PlayerShip.cs:118-127 and 129-138 are five statements each differing in the damage source and the event fired, and PlayerShipAudio.cs:22-30 is two three-line forwarders. That is the ordinary shape for two distinct events with different payload types, it costs ~14 lines, and it is at most kind:"recommendation" — it must not be reported as a major SOLID failure, and the IBumpable remedy as written would lose the enemy's death-on-ram (EnemyCollision.cs:9-12) and the -2f/+4f asymmetry (PlayerCollisionHandler.cs:6-7).

---

### 22. SRP: PlayerShip mixes input routing, control-lock policy, damage relay, death presentation, collision reaction and restart orchestration

**major** · ось: SOLID: SRP and OCP · `Assets/Scripts/Ships/Player/PlayerShip.cs:6` · требует редактора Unity · с особым мнением

**Нарушенное правило.** Violated rule: Single Responsibility Principle. Five separate triggers hit this file: remap or re-gate input; change the death effect; change what a bump does; change how damage is routed; change what a restart resets. What makes it a defect rather than a facade is that the responsibilities share mutable state and contradict each other — `_isControlEnabled` (input concern) and `_playerMover.enabled` / `_bodyCollider.enabled` (life-state concern) both mean 'can the player act', and Game drives them through different doors (Game.cs:157/166 use Lock/UnlockInput, Game.cs:53 and WarpJump.cs:66 use Disable/EnableControl), which is how the ship ends up controllable while dead.
What it should look like: PlayerShip stays the ship's facade — TakeDamage, Die, Restart, Died — and delegates the rest.

**Почему это дефект.** Not a size complaint: the classes are already separated in the project (PlayerMover, PlayerShooter, Health, PlayerCollisionHandler, PlayerShipAudio all exist), and PlayerShip has accumulated the leftovers plus a second control-state concept that already produces a reproducible player-facing bug. Two public method pairs that mean the same thing is the definition of an ambiguous responsibility.

**Код:**

```csharp
Reason to change #1, input routing and the lock policy: `_input` (PlayerShip.cs:11), `_isControlEnabled` (23), the subscriptions at 41-42 and 51-52, OnJumped/OnShot (61-75), and the four public mutators UnlockInput/LockInput/EnableControl/DisableControl (77-101).
Reason #2, death presentation: `PoolCapacity`/`PoolMaxSize` (8-9), `_explosionAnimation` (16), `_explosionPool` (18, 31), `DeathDuration` (55), and the explosion block in Die (147-152).
Reason #3, collision reaction: `_playerCollisionHandler` (13), events (58-59), OnAsteroidBumped/OnEnemyBumped (118-138), PushOff (175-178).
Reason #4, damage relay: `_health` (20, 29), TakeDamage (108-111), OnAmmoHit (113-116).
Reason #5, restart orchestration: `_startScale` (22, 32), Restart (157-168), which reaches into four collaborators — `_playerShooter.ReleaseAll(); _explosionPool.ReleaseAll(); _playerCollisionHandler.ReleaseAll(); _health.Init();` — plus the sprite and scale repair.
The overlap is visible in the public surface: thirteen public members, including two pairs of control methods for one concept (77-85 and 87-101), which is the root cause of the behavioural defect reported as finding #1.
```

**Как чинить.** Three moves, each small.
(a) Input routing out: create `PlayerInputRouter` holding `_input` and a reference to PlayerShip; it owns the subscriptions at 41-42/51-52 and OnJumped/OnShot (61-75). Delete UnlockInput/LockInput (77-85) entirely — per finding #1 the pause gate belongs in InputReader — leaving EnableControl/DisableControl as the one control switch, which the router consults.
(b) Death presentation out: move `_explosionAnimation`, `_explosionPool`, the two pool constants and `DeathDuration` into a `ShipExplosion` component (PlayerShip.cs:8-9, 16, 18, 31, 55, 147-149 and the ReleaseAll at 163). Die() then reads `_explosion.Play(); _explosion.Duration` and Game.cs:199 asks the same component. Note this component is reusable by EnemyShip, which does the identical thing at EnemyShip.cs:98-100.
(c) Collision reaction out, or collapsed: per finding #3, the two handlers at 118-138 and the two events at 58-59 become one of each behind IBumpable.
Wiring afterwards: PlayerInputRouter and ShipExplosion sit on the Player Ship prefab root next to PlayerShip and take serialized references to it; Game keeps its single `_playerShip` reference and its Died subscription unchanged. PlayerShip drops to roughly 90 lines with one control concept and one public control pair.

**Риск правки.** Medium. New components on the Player Ship prefab need their references assigned, and Restart() (157-168) is the fragile part — it currently resets six things in one place, and splitting it risks leaving one out (most likely _explosionPool.ReleaseAll, which is what stops a stale explosion from the previous run being visible). After the split, test: play, take an asteroid hit, die, restart, and confirm health, sprite, scale, position, cooldown bar and the absence of leftover bullets.

**Особое мнение скептика.** Refuted: the finding's load-bearing argument is factually wrong about the code, and the rest describes an entity facade doing facade work. (1) "Two public method pairs that mean the same thing" is false. PlayerShip.cs:77-85 flips `_isControlEnabled` only, gating OnJumped/OnShot (63, 71) while leaving the rigidbody Dynamic and `_bodyCollider` enabled; PlayerShip.cs:87-101 is a strict superset that also does `_playerMover.Stop()` -> `bodyType = Kinematic` (PlayerMover.cs:47-51), `_playerMover.enabled = false` and `_bodyCollider.enabled = false`. The two are required to differ: pause (Game.cs:152-168) must not freeze the body because `Time.timeScale = 0` already does and Resume has no state to restore, while death (PlayerShip.cs:151) and the warp cutscene (WarpJump.cs:66) must freeze it, since the cutscene hand-drives `ship.position`/`ship.localScale` (WarpJump.cs:110-134) against a PlayerMover.Update that rewrites `linearVelocity` every frame (PlayerMover.cs:34-39). Layering, not an ambiguous responsibility. (2) Four of the five claimed "reasons to change" live in the collaborators, not here: input remap -> InputReader.cs:14-24; bump impulse -> PlayerMover.cs:73-89 and PlayerCollisionHandler.cs:51-54; damage arithmetic and the Died event -> Health.cs:23-33. PlayerShip's matching members (66, 74, 110, 120, 131, 177) are one-line forwards that would not change in any of those edits; the only genuinely owned one is Restart (157-168), which the proposal keeps. Every method body in the file is 1-7 lines of delegation with no domain logic beyond the `_isDead` re-entrancy guard. (3) The bug it cites as proof is a different finding. `Die()` calls `DisableControl()` (PlayerShip.cs:151), so death does lock control; the only path back to input while dead is `Game.Resume` calling `UnlockInput` unconditionally (Game.cs:166) during the countdown before `_isRunning` clears (Game.cs:202) - fixed by a guard in Game or in OnJumped/OnShot, with zero members moved. Using it to justify a second finding double-counts one defect. (4) The proposed fix is disproportionate for a 179-line student-project coordinator: `PlayerInputRouter` would be two 4-line handlers plus six lines of subscribe/unsubscribe plus two serialized references, and would still change whenever input changes - the reason to change relocates rather than disappears. The only residue with merit is three duplicated lines (PlayerShip.cs:147-149 vs EnemyShip.cs:98-100), and even there the pools differ in origin (EnemyShip's injected at EnemyShip.cs:46-55, PlayerShip's built at 31) - a DRY nit at recommendation strength. Nothing in the exemplar establishes a rule against a facade owning its own wiring.

**Уточнения проверяющих.**

- Narrow the enumeration from five reasons-to-change to three. The reviewer's own target state keeps TakeDamage (108-116) and Restart (157-168) in PlayerShip ("PlayerShip stays the ship's facade — TakeDamage, Die, Restart, Died"), so counting "damage relay" and "restart orchestration" as misplaced responsibilities is padding: a facade must implement IDamageable and must reset its own collaborators. The defensible charge is: (1) input adaptation plus a lock policy (_input 11, subs 41-42/51-52, OnJumped/OnShot 61-75, the Lock/Unlock pair 77-85); (2) ownership of a VFX object pool with its own two constants (8-9, 18, 31, 147-149, 163), duplicated verbatim in EnemyShip.cs:94-100; (3) a pass-through collision relay that re-raises PlayerCollisionHandler's own events (58-59, 118-138). The load-bearing proof is not the count but the ambiguous control API: two public pairs meaning "can the player act", which I verified produces the reproducible dead-ship-still-shoots state above. Note also that the incremental value of this finding over the separately-reported behavioural finding #1 is the structural fix, not the bug — the two should be presented as root cause and symptom, not as independent hits.
- Trim the claim to: "PlayerShip carries two concerns beyond the ship facade — input routing/gating (PlayerShip.cs:11, 41-42, 51-52, 61-75, 77-85) and death presentation (8-9, 16, 18, 31, 55, 147-149, 163, duplicated at EnemyShip.cs:98-100) — and exposes two overlapping public control switches (77-85 vs 87-101) that Game.cs:53/157/166 and WarpJump.cs:66 use inconsistently." Three of the five cited "reasons to change" must be dropped: damage relay (108-116), collision reaction (118-138, 175-178) and restart orchestration (157-168) are the facade delegating to its own sibling components, and the finding itself says TakeDamage/Die/Restart/Died should stay in PlayerShip — it cannot count them as proof of a violation and as the surviving core at once. The root-cause sentence is also wrong: collapsing the two pairs into one does not fix finding #1, it worsens it. If Lock/UnlockInput were deleted and Game.Resume called EnableControl (87-93), resuming from a pause taken while dead would set _playerMover.enabled = true, call _playerMover.Resume() (bodyType back to Dynamic) and re-enable _bodyCollider on a ship whose _isDead is true and whose sprite is off — a fully physical invisible ship, strictly worse than today's gate-only flip. Finding #1 is fixed by a dead-state guard or by moving the pause gate out of PlayerShip, which is an independent change, not a consequence of the SRP split. Likewise "drops to roughly 90 lines" is wrong: removing both concerns deletes about 40 of 179 lines, leaving ~140.

---

### 3. Particle Start Delays of 4-6 s in the player explosion prefab make PlayerShip.DeathDuration ~16 s, so the defeat screen is withheld ~16 s after death and the pooled explosion is held that long

**major** · ось: Unity serialization · `Assets/VFX/Sparks/Player ship explosion.prefab:59` · требует редактора Unity · единогласно

**Сценарий воспроизведения.** Start a level and fly the player into an asteroid until health reaches 0. PlayerShip.Die() (PlayerShip.cs:140-155) plays the explosion, hides the ship sprite and raises Died; Game.LoseLevel() (Game.cs:175-184) stops both spawners and starts ShowGameOver(). Emission in this prefab is burst-only (every rateOverTime is Constant 0, e.g. L≈ EmissionModule of 'Sparks ellipse'), so the bursts fire at t=0 ('Inner fire'), t=4 ('Explosion'), t=4.5 ('Sparks', 'Big flash') and t=6 ('Sparks ellipse'). The screen is visually finished by about t=11, but ShowGameOver waits the full 16.0 s. For sixteen seconds the player sits on an empty, frozen-looking screen with no ship, no enemies, no asteroids and no restart button before the defeat panel appears. Restart is impossible during that window: Game._isRunning is still true, so Esc only opens the pause menu.

**Почему это дефект.** The numbers are read straight out of the serialized prefab and straight out of the formula in ExplosionAnimation.CalculateDuration(); no judgement is involved. A death-to-defeat-screen gap of 16 s (11 s of which is after the last particle dies) is a hard-stop in the game loop, not a matter of taste. The same code path is fine for every other explosion in the project - only this prefab's data breaks it.

**Код:**

```csharp
Assets/Scripts/Game.cs:
  197	    private IEnumerator ShowGameOver()
  199	        yield return new WaitForSeconds(_playerShip.DeathDuration);
  203	        _gameOverScreen.Show(_killCounter.Kills);

Assets/Scripts/Ships/Player/PlayerShip.cs:
   55	    public float DeathDuration => _explosionAnimation.Duration;

Assets/Scripts/Ammo/ExplosionAnimation.cs:
   70	    private float CalculateDuration()
   74	        foreach (ParticleSystem particleSystem in GetComponentsInChildren<ParticleSystem>(true))
   78	            longest = Mathf.Max(longest,
   79	                main.startDelay.constantMax + main.duration + main.startLifetime.constantMax);

PlayerShip._explosionAnimation points at this prefab (Assets/Prefabs/Player Ship.prefab:251 guid 27bd167c... = VFX/Sparks/Player ship explosion.prefab).

Assets/VFX/Sparks/Player ship explosion.prefab, the five child ParticleSystems (startDelay is a MinMaxCurve with minMaxState 0 = Constant, where 'scalar' is m_ConstantMax, i.e. what main.startDelay.constantMax returns):
   44	  lengthInSec: 5            ('Sparks ellipse')
   56	  startDelay:
   58	    minMaxState: 0
   59	    scalar: 6
  116	    startLifetime:
  118	      minMaxState: 0
  119	      scalar: 5
  -> 6 + 5 + 5 = 16.0 s
 5079	  lengthInSec: 3            ('Explosion')   5094 scalar: 4    -> 10.0 s
 9967	  lengthInSec: 4            ('Sparks')      9982 scalar: 4.5  ->  9.1 s
14913	  lengthInSec: 3            ('Inner fire') 14928 scalar: 0    ->  4.5 s
19743	  lengthInSec: 2            ('Big flash')  19758 scalar: 4.5  ->  7.5 s
The prefab has no Animator (components are Transform, AudioSource, ExplosionAnimation plus five ParticleSystem children), so CalculateDuration() returns exactly 16.0.

That the 'scalar' key is constantMax is confirmed inside this same file, where a TwoConstants curve writes the larger number to scalar: L10905 countCurve minMaxState: 3, scalar: 150, minScalar: 100; L10041 startLifetime minMaxState: 3, scalar: 0.6, minScalar: 0.2.

By contrast every other VFX prefab in the project has startDelay scalar 0 (vfx_EnemyExposion.prefab, vfx_RocketExplosion.prefab, vfx_Sparks_01.prefab, Enemy Bullet.prefab), so these four non-zero delays are unique to this one prefab.
```

**Как чинить.** In the editor, open Assets/VFX/Sparks/Player ship explosion.prefab and set Start Delay to 0 on 'Sparks ellipse', 'Explosion', 'Sparks' and 'Big flash' (they were almost certainly meant to be 0.4-0.6, not 4-6). Note that this alone still leaves DeathDuration at 10 s, because 'Sparks ellipse' has Duration 5 and Start Lifetime 5 while its only emission is a single burst at t=0; bring its Duration down to roughly the burst time and its Start Lifetime down to about 1-1.5 s so the computed span matches what is actually visible. If you would rather not retune the VFX, stop deriving the cutscene length from the VFX span: give PlayerShip its own serialized death-cutscene duration and have Game.ShowGameOver wait on that instead of on _explosionAnimation.Duration.

**Риск правки.** Editing Start Delay and Duration changes how the death explosion looks, so it needs a visual check in Play mode. The value is also consumed by ExplosionAnimation.CountLifetime, which returns the effect to the pool - shortening it shortens how long the explosion stays spawned, which is the intent. Nothing else reads DeathDuration (grep: only Game.cs:199).

**Уточнения проверяющих.**

- The defect holds, but three supporting statements are wrong and must be fixed before this ships. (1) The duration is exactly 16.0 s, not "~16 s". (2) "Restart is impossible during that window" is false. During the wait _isRunning is still true, so Esc reaches Game.TogglePause (Game.cs:99-108) and, since _isPaused is false, calls Pause() — which shows PauseScreen. PauseScreen has a restart button (PauseScreen.cs:9, 45-48) wired to Game.RestartFromPause (Game.cs:40, 124-128), which calls RestartGame -> StartLevel -> StopGameOverCountdown (Game.cs:139, 206-213). The player therefore does have an escape hatch after dying; it is merely undiscoverable, since nothing on screen suggests pressing Esc on a corpse. Drop the "impossible" claim and say the game offers no visible way out for 16 s. (3) "11 s of which is after the last particle dies" is bad arithmetic. Emission is burst-only in four of the five systems, so the last particles are the 'Sparks ellipse' burst (m_Bursts at L977, time 0, cycleCount 1) emitted at absolute t=6 with a 5 s lifetime, dying at t=11; the blank tail is therefore about 5 s, not 11. (4) "every rateOverTime is Constant 0" is false for 'Inner fire': its EmissionModule (L15736) has rateOverTime minMaxState 1 (Curve) with scalar 4 and m_Bursts: [] (L15846), i.e. continuous emission — for that one system the startDelay+duration+startLifetime formula is exact. This does not change the 16.0 maximum, which comes from 'Sparks ellipse'. Restate the finding as: PlayerShip.DeathDuration evaluates to exactly 16.0 s, so the defeat screen is withheld for 16 s after death (about 5 s of which is a completely blank screen) and the single-instance explosion pool is held the whole time, versus 1.3-1.6 s for every other explosion in the project.
- Keep the finding, but fix three factual errors in its narrative:

(a) "Emission in this prefab is burst-only (every rateOverTime is Constant 0)" is false. 'Inner fire' emits continuously off a curve: Assets/VFX/Sparks/Player ship explosion.prefab:15739-15743 — rateOverTime minMaxState: 1 (Curve), scalar: 4, and m_Bursts: [] at :15846. The other four are indeed burst-only with rateOverTime Constant 0 (:870-873, :5905-5908, :10793-10796, :20569-20572).

(b) "16 s, 11 s of which is after the last particle dies" is arithmetically inconsistent with the reviewer's own timeline and overstates the dead air. The last emission is 'Sparks ellipse's 100-particle burst at relative t=0 (:977-983), which fires at absolute t=6 because of the 6 s start delay, and those particles live 5 s — so visuals end at about t=11 and the empty-screen gap is roughly 5 seconds, not 11. The total wait is still 16.0 s.

(c) "Restart is impossible during that window: Game._isRunning is still true, so Esc only opens the pause menu" is false, and backwards. _isRunning is still true precisely so TogglePause works (Game.cs:96-105), and the pause menu carries a Restart button (PauseScreen.cs:10 and :47-50) wired to Game.cs:40 `_pauseScreen.RestartRequested += RestartFromPause`, which resumes and restarts. A player who presses Esc can escape the 16 s wait; one who just waits cannot.

Corrected claim: the player-explosion prefab's four non-zero particle Start Delays (up to 6 s) make ExplosionAnimation.CalculateDuration return exactly 16.0 s, so Game.ShowGameOver withholds the defeat screen for 16 s after the player dies — about 5 s of which is a completely empty, motionless screen after the last particle has expired — and the single pooled explosion instance is held for that whole span. The only way out during the wait is Esc -> Restart from the pause menu.
- Keep the finding, but fix the reproduction, which is wrong in three places and mislabelled in a fourth.

1. "Restart is impossible during that window" is FALSE. `Game.cs:40` wires `_pauseScreen.RestartRequested += RestartFromPause`, and `PauseScreen.cs:9,45-48` has a `_restartButton` raising it; `RestartFromPause` (Game.cs:124-128) → `RestartGame` → `StartLevel` → `StopGameOverCountdown` (Game.cs:137-139, 206-213). So Esc → Restart does work during the wait. The defect is the withheld defeat screen, not a lockout.

2. "11 s of which is after the last particle dies" is arithmetic error. The last particle dies at t≈11, so the dead tail is ≈5 s, not 11.

3. "an empty screen with no enemies and no asteroids" is FALSE. `EnemySpawner.StopSpawn` (Assets/Scripts/Level/EnemySpawner.cs:57-64) and `AsteroidSpawner.StopSpawn` (Assets/Scripts/Level/AsteroidSpawner.cs:36-43) only `StopCoroutine` the spawn loops; already-spawned enemies and asteroids keep flying, and enemies keep shooting, at an invisible ship for the full 16 s.

4. The axis label "Unity serialization" is a misnomer — nothing is failing to serialize. This is VFX authoring data feeding a game-loop timing formula.

Corrected claim: the 'Sparks ellipse' child of Assets/VFX/Sparks/Player ship explosion.prefab has Start Delay 6, Duration 5 and Start Lifetime 5, so `ExplosionAnimation.CalculateDuration()` returns 16.0 s; `Game.ShowGameOver` (Game.cs:197-204) waits that full span, leaving ~16 s between the player's death and the defeat screen, the last ~5 s of which has no particle on screen at all. The secondary half of the claim — that the pooled explosion is held 16 s by `CountLifetime` (ExplosionAnimation.cs:65, 94-100) — is true but inconsequential: `PlayerShip.PoolCapacity`/`PoolMaxSize` are 1 (PlayerShip.cs:8-9) and the player dies once per level, and `PlayerShip.Restart` calls `_explosionPool.ReleaseAll()` (PlayerShip.cs:163). Drop that half; the 16 s gap is the finding.

<sub>Слито из 3 формулировок других ревьюеров.</sub>

---

### 19. An explosion's pooled lifetime is computed from particles and animation only, never from its own AudioSource clip, so the sound is cut off when the object returns to the pool

**minor** · ось: Audio · `Assets/Scripts/Ammo/ExplosionAnimation.cs:79` · единогласно

**Сценарий воспроизведения.** Shoot an enemy ship dead while it is on screen. The enemy explosion sound starts, and exactly 1.30 s later the pooled vfx_EnemyExposion object is released and SetActive(false) - the remaining 0.55 s of "Enemy explosion.wav" is discarded. Same on every enemy rocket that hits you: the last 0.25 s of "Rocket hit.wav" never plays.

**Почему это дефект.** The class computes a lifetime that is supposed to cover the whole effect, and it demonstrably does not cover one of the three things the effect plays. Honest caveat measured from the wav data: at the cut point both clips are already 33-37 dB below their onset (reverb tail), so today you will barely hear it - this is a latent defect, not an obvious one. It becomes audible the moment anyone swaps in a longer clip, which is exactly the scenario the Duration property exists to protect against.

**Код:**

```csharp
Ammo/ExplosionAnimation.cs:64-67
   64        if (ScreenArea.Contains(_camera, transform.position))
   65            _audioSource.Play();
   66
   67        _lifetimeCountdown = StartCoroutine(CountLifetime(Duration));

Ammo/ExplosionAnimation.cs:70-88 - CalculateDuration walks GetComponentsInChildren<ParticleSystem> (line 74-80) and the Animator's clips (line 82-86). _audioSource.clip.length never appears anywhere in the method.

Ammo/ExplosionAnimation.cs:97-103
   99        yield return new WaitForSeconds(lifetime);
  102        Released?.Invoke(this);

Pools/SpawnPool.cs:76-80
   76    private void PrepareForStorage(T item)
   77    {
   78        item.OnDespawn();
   79        item.gameObject.SetActive(false);
   80    }
(OnDespawn, ExplosionAnimation.cs:42-51, stops the coroutine and the particles but never touches _audioSource.)

Unity 6000.3 docs, AudioSource.playOnAwake: "If you deactivate Enable audio source, any audio that is currently playing will stop." So line 79 of SpawnPool kills playback.

Measured mismatches:
- Enemy Ship.prefab:433 _explosion -> vfx_EnemyExposion.prefab: one ParticleSystem, lengthInSec 1, startDelay scalar 0, startLifetime scalar 0.3, no Animator => Duration = 1.3 s. Its AudioSource (line 4868) plays "Enemy explosion.wav" = 1.85 s. 0.55 s (30%) never plays.
- Enemy Bullet.prefab:4967 _prefab (hit effect) -> vfx_RocketExplosion.prefab: four systems, longest 0+1+0.6 = 1.6 s. AudioSource (line 9817) plays "Rocket hit.wav" = 1.85 s. 0.25 s never plays.
```

**Как чинить.** In CalculateDuration, fold in the audio the same way the animation clips are folded in: after the Animator loop, if _audioSource != null and _audioSource.clip != null, longest = Mathf.Max(longest, _audioSource.clip.length). No prefab changes needed; Duration then covers every channel the effect owns.

**Риск правки.** Low, but it lengthens the pooled lifetime of each effect, so more instances may be live at once (SpawnPool default maxSize is 100, far above what this game produces). Note it would also push the player death explosion's Duration out - do this together with the 16 s finding, not instead of it.

**Уточнения проверяющих.**

- The mechanism is real but the stated symptom overstates what a player can perceive, and the write-up should be narrowed to match. Measured from Enemy explosion.wav directly: the clip peaks at -1.0 dBFS, but the 10 ms window at the 1.3 s cut peaks at -43.0 dBFS (-42 dB relative to the clip peak), and the entire discarded 0.549 s tail is RMS -53.5 dBFS / peak -41.9 dBFS. The rocket case is the same file cut at 1.6 s: local peak at the cut -47.9 dBFS, discarded tail RMS -55.9 dBFS. On top of that the AudioSource is authored at `m_Volume: 0.5` (vfx_EnemyExposion.prefab:4880), a further -6 dB, and it plays under Music.wav and other SFX. The instantaneous sample at the cut is -90 dBFS, so there is not even a click. Nothing is audible today.

So this should be filed as kind:"recommendation" (latent robustness gap), not kind:"behavior": per the review's own rule 3 and rule 6, a behavioural finding needs a scenario where something observably goes wrong, and here the only thing that goes wrong is a number. The accurate claim is: "Duration is documented by its own name and use as the effect's full lifetime, yet it covers only two of the three channels the effect owns; today the third channel's overrun falls entirely in a -42 dB reverb tail, so it is inaudible, but any longer or less decayed clip dropped onto these prefabs would be cut mid-sound with no code change." The proposed fix (fold `_audioSource.clip.length` into CalculateDuration after the Animator loop, guarded for null source and null clip) is correct and is the right shape for this codebase.
- The finding stands as written; two factual refinements worth carrying into the report. (1) The third explosion prefab is not affected and the finding was right not to claim it: Assets/VFX/Sparks/Player ship explosion.prefab computes Duration = 16.0 s (its "Sparks ellipse" system has startDelay 6 + lengthInSec 5 + startLifetime 5, line 56/116 region) against a 10.0 s Player explosion.wav, so it over-covers rather than truncates - the defect is confined to the enemy explosion and the rocket hit. (2) Assets/Sounds/Enemy explosion.wav and Assets/Sounds/Rocket hit.wav are byte-identical (md5 ea61760c6f10d4c2546e7c8514fa965c, 88755 frames at 48 kHz); it is literally one 1.849 s clip being cut at two different points. On the proposed fix: folding Mathf.Max(longest, _audioSource.clip.length) into CalculateDuration needs the null guard the finding already mentions, and it is safe for the one other consumer of Duration - PlayerShip.cs:55 exposes it as DeathDuration and Game.cs:199 waits on it, but since the player prefab's 16 s already exceeds its 10 s clip, the death cutscene timing would not change.
- Claim stands as written; two measured refinements. (1) Scope is exactly the two effects named and no others - I checked the other two ExplosionAnimation prefabs and they are safe: Assets/VFX/Sparks/Player ship explosion.prefab's longest system is startDelay 6 + lengthInSec 5 + startLifetime 5 = 16 s against a 10.0 s Player explosion.wav, and Assets/Prefabs/PlayerBulletHit.prefab has no ParticleSystem but an Animator whose clip "Hit-5 Animation" is 0.583 s against a 0.238 s Plasma hit 2.wav. So the player's death explosion and every player-bullet hit are unaffected. (2) The severity ceiling is minor and must not be raised: I measured Enemy explosion.wav at the 1.30 s cut point as -49.9 dBFS RMS / -35.4 dBFS peak in a 50 ms window versus -10.1 dBFS RMS at the onset, and the AudioSource plays at m_Volume 0.5, so the discarded tail is inaudible in play today. The defect is the arithmetic mismatch in a method whose stated job is the effect's lifetime, not a sound the player can currently hear.

---

### 14. LoseLevel stops the spawners but not the enemies already spawned, so ships keep flying and firing (with sound) across the defeat screen

**minor** · ось: Restart and level transition · `Assets/Scripts/Game.cs:181` · с особым мнением

**Сценарий воспроизведения.** Play a level, let an enemy ship reach mid-screen, then let it kill you (or ram an asteroid). The explosion plays, the defeat screen appears - and behind/through it the surviving enemy keeps flying left and firing its weapon, shot sound included, until it drifts out through ObjectRemover left. Its bullets fly across the GAME OVER text. Only the eventual restart (EnemySpawner.StartWave -> the ReleaseAll block at EnemySpawner.cs:44-48) clears them.

**Почему это дефект.** LoseLevel's own code shows the intent is to stop the level - it calls StopSpawn on both spawners - but it only stops production, not the objects already produced, so the stop is half-done and the result is a 'live' defeat screen the code did not ask for. The win path already has an equivalent teardown available (EnemySpawner.StartWave releases the pools); nothing equivalent is reachable from LoseLevel.

**Код:**

```csharp
Game.cs:175-184
  175  private void LoseLevel()
  176  {
  177      if (_isLevelOver)
  178          return;
  180      _isLevelOver = true;
  181      _enemySpawner.StopSpawn();
  182      _asteroidSpawner.StopSpawn();
  183      _gameOverCountdown = StartCoroutine(ShowGameOver());
  184  }

EnemySpawner.cs:57-64 - StopSpawn touches only the spawn coroutine:
   57  public void StopSpawn()
   59      if (_spawning == null)
   60          return;
   62      StopCoroutine(_spawning);
   63      _spawning = null;

EnemyShooter.cs:45-60 - a live enemy's fire loop runs `while (enabled)` and is only stopped from EnemyShip.OnDespawn (EnemyShip.cs:65-70), which only runs when the ship is released back to the pool.
EnemyMover.cs:18-21 - Fly() sets a constant velocity that nothing clears until OnDespawn.
Game.cs:197-204 ShowGameOver leaves Time.timeScale at 1.
Scene values: EnemySpawner._spawnDelay 3, Game._firstWaveSize 3 / _waveSizeGrowth 3, so at the moment of death there is normally at least one live enemy.
```

**Как чинить.** Give EnemySpawner and AsteroidSpawner a StopLevel()/Clear() that both stops the coroutine and releases the live pools (the same ReleaseAll block StartWave already runs, with the _isRestarting guard so it does not count retirements), and call it from LoseLevel. Clearing the enemy ammo pool at the same time also removes bullets in flight. Note that the same leftover applies to enemy bullets during the ~9.6 s warp cutscene on the win path.

**Риск правки.** Low-medium. The release must run with EnemySpawner._isRestarting set, exactly as StartWave does at lines 42-50, otherwise each released enemy's OnDespawn fires Retired and pushes _retiredCount to _waveSize, invoking WaveCleared. That would be swallowed by Game._isLevelOver today, but it is a trap worth being deliberate about.

**Особое мнение скептика.** The mechanism is accurately described but it is a design choice, not a defect, and the stated repro is factually wrong on its key visual claim.

1) Code checks out as quoted: Game.cs:175-184 LoseLevel calls only StopSpawn on both spawners; EnemySpawner.cs:57-64 stops only the coroutine; EnemyShooter.cs:45-59 loops `while (enabled)`; EnemyMover.cs:18-21 sets a constant linearVelocity; Game.cs:197-204 leaves timeScale at 1. So yes, live enemies keep flying and firing after death.

2) No rule and no stated intent is violated. The brief says only "On death an explosion cutscene plays, then a defeat screen with restart" - nothing requires the arena to freeze, and the author freezes deliberately elsewhere when he wants to (Game.cs:152-159 Pause sets Time.timeScale = 0 and AudioListener.pause = true; Game.cs:197-204 pointedly does not). Keeping the arena live behind a defeat overlay is a normal arcade choice. The reviewer's only evidence of contrary intent is an inference from StopSpawn, but StopSpawn has its own sufficient purpose: without it the Spawn coroutine (EnemySpawner.cs:89-104) would keep injecting new enemies through the whole defeat screen.

3) The concrete repro is wrong where it matters. The Game Over panel (SampleScene.unity:739 "Game Over", RectTransform 7029 anchored 0,0-1,1) contains a full-screen Backdrop Image at black alpha 0.72 (SampleScene.unity:347,360) and a centred 780x524 Window at alpha 0.97 (SampleScene.unity:8337,8350) carrying an opaque Title Plate, title and score. Nothing "flies across the GAME OVER text" - the text sits on opaque plates and the world behind is dimmed to ~28%.

4) Nothing breaks or leaks. The leftovers drain themselves: ObjectRemover left (SampleScene.unity:9453) is a trigger wall at x=-15 scaled y=19 while the ortho-5 camera sees to about x=-8.9, so an enemy at 1-3 u/s leaves the visible area in seconds and is then silent anyway (EnemyShooter.cs:54 gates the shot sound on ScreenArea.Contains). A retirement during the defeat screen that pushes _retiredCount to _waveSize fires WaveCleared, and WinLevel swallows it at Game.cs:188-189 via _isLevelOver. Restart clears everything: EnemySpawner.cs:38-55 StartWave releases the ship, ammo, hit-effect, explosion and dead-body pools before the wave, and PlayerShip.cs:157-168 Restart releases its own pools - and StartLevel runs Restart (Game.cs:145) and StartWave (Game.cs:146) in the same frame, so no physics step can carry a stale bullet into the new life. Health.TakeDamage returns early at Health.cs:25 when Value == 0, so a stray bullet cannot re-fire Died on the corpse.

What remains is a few seconds of enemy motion and shot sound behind a 72%-dim overlay, with no wrong state - polish taste, not a proven misbehaviour. The proposed fix (new StopLevel/Clear API on both spawners plus ammo-pool clearing, extended to the warp path) is a design change proposal, disproportionate for this project and unsupported by any school rule.

**Уточнения проверяющих.**

- The defect holds, but two details in the write-up are wrong and should be dropped. (1) "Only the eventual restart clears them" is false: an enemy that reaches the left ObjectRemover (scene object 'ObjectRemover left' at x = -15) is removed by ObjectRemover.cs:5-10 -> EnemyShip.Remove (EnemyShip.cs:72-75) -> pool Release -> OnDespawn, which does StopShoot/Stop. With Enemy Ship.prefab:306-307 speeds 0.5-3 u/s, an enemy at mid-screen self-clears in roughly 5-29 s, so the leftover is bounded, not permanent. (2) "Its bullets fly across the GAME OVER text" overstates the visual: the centred Window image is alpha 0.97 and the rest of the screen is dimmed by a full-screen alpha-0.72 black Backdrop, so leftover ships and bullets read as dimmed motion outside the window, never over the text. The accurate claim: after LoseLevel the already-spawned enemies keep flying and firing (shot sound included, since timeScale stays 1 and AudioListener.pause stays false) through the death explosion and into the defeat screen, until each one drifts out through the left remover.
- Substance holds; one detail in the repro wording is wrong. The defeat screen's full-screen Backdrop is black at alpha 0.72 (SampleScene.unity:347-352) and the central Window panel is alpha 0.97 over 780x524 (SampleScene.unity:8337-8342), so the surviving enemy and its bullets show as dimmed silhouettes around the window rather than "flying across the GAME OVER text" — anything passing behind the title plate is hidden. The audio half is exact: the enemy keeps firing every 2 s and the shot sound plays at full volume while the enemy is on-screen, since AudioListener.pause stays false on the death path.

---

### 12. _isRunning is cleared inside the ShowGameOver coroutine, so the pause menu stays reachable for the whole death animation

**minor** · ось: Coroutines · `Assets/Scripts/Game.cs:202` · с особым мнением

**Сценарий воспроизведения.** 1. Play until the player ship is destroyed. The explosion plays and Game.ShowGameOver() begins its WaitForSeconds(DeathDuration).
2. Press Escape before the defeat screen appears. Because _isRunning is still true, TogglePause() pauses: Time.timeScale goes to 0 and the pause menu (Resume / Restart / Settings) opens on top of a frozen death explosion of a player who is already dead.
3. The WaitForSeconds is frozen with it, so the defeat screen never arrives while paused; the run only ends when the player resumes. (Combined with the Game.Resume() finding, resuming also hands control back to the corpse.)

**Почему это дефект.** Not taste: _isRunning is the game's "a run is in progress, pausing is allowed" flag, and the run demonstrably ends at LoseLevel() - that is where _isLevelOver is set and both spawners are stopped. Parking the flag's clear inside a coroutine that then sleeps for the whole death animation makes the flag disagree with the state machine for the entire window, and reachable pause/settings UI over a dead player is the observable result. The other end-of-run state (_isLevelOver) is set synchronously in LoseLevel; _isRunning is the inconsistent one.

**Код:**

```csharp
Game.cs:175-184 - death only starts the countdown; _isRunning is left true:
    private void LoseLevel()
    {
        if (_isLevelOver)
            return;

        _isLevelOver = true;
        _enemySpawner.StopSpawn();
        _asteroidSpawner.StopSpawn();
        _gameOverCountdown = StartCoroutine(ShowGameOver());
    }

Game.cs:197-204 - the flag is only cleared after the scaled wait completes:
    private IEnumerator ShowGameOver()
    {
        yield return new WaitForSeconds(_playerShip.DeathDuration);

        _gameOverCountdown = null;
        _isRunning = false;
        _gameOverScreen.Show(_killCounter.Kills);
    }

Game.cs:99-102 - and that flag is the only gate on pausing:
    private void TogglePause()
    {
        if (_isRunning == false)
            return;

PlayerShip.cs:55 - the wait length is the explosion prefab's full computed duration:
    public float DeathDuration => _explosionAnimation.Duration;

Unity docs, WaitForSeconds: "The real time suspended is equal to the given time divided by Time.timeScale" - so pausing (Game.cs:155 sets Time.timeScale to 0) freezes this wait indefinitely, keeping _isRunning true for as long as the player stays paused.
```

**Как чинить.** Move `_isRunning = false;` out of ShowGameOver() (Game.cs:202) and into LoseLevel() (Game.cs:175-184), next to `_isLevelOver = true;`, leaving the coroutine responsible only for the delay and for showing the screen. _isRunning is read in exactly one place (TogglePause, Game.cs:101) and written in exactly two (StartLevel Game.cs:143, here), so the move is fully local.

**Риск правки.** Low. Code-only, one statement moved. StartLevel() sets _isRunning = true again on every restart (Game.cs:143), including the Restart-from-pause path, so no state gets stuck. Check that Escape is ignored from the moment of death until the next level starts.

**Особое мнение скептика.** The mechanics are quoted correctly (Game.cs:202 clears _isRunning after the wait; Game.cs:101 is the only pause gate; InputReader.cs:22-23 keeps firing PauseToggled), so pause IS reachable during the death cutscene — but that is the design, not a defect, and no rule is violated.

1) _isRunning and _isLevelOver are two different concepts, so they are not "in disagreement". _isLevelOver = "this level's outcome is decided" (re-entry guard + stop spawners, Game.cs:177-182 and 188-193). _isRunning = "no blocking menu screen owns the UI, so the pause key is live". The proof that _isRunning is not meant to mirror _isLevelOver is WinLevel (Game.cs:186-195): it sets _isLevelOver = true and deliberately leaves _isRunning true for the whole warp-jump cutscene, so the player can pause during the win cutscene too. Clearing _isRunning at Game.cs:202, one line before _gameOverScreen.Show() at Game.cs:203, is exactly the moment pause must stop working — when the defeat screen takes over the UI. The placement matches the flag's meaning; the proposed move to LoseLevel would change behaviour (kill the pause key during the death cutscene only, inconsistent with the win cutscene), which is a design change, not a fix.

2) Nothing goes wrong in the reproduction. Escape during the countdown: Pause() (Game.cs:152-159) freezes time; the death explosion's own CountLifetime (ExplosionAnimation.cs:97-103) is an equally scaled WaitForSeconds, so cutscene and countdown freeze together — "the defeat screen never arrives while paused" is simply what pause means, not a fault. Restart from the pause menu is explicitly handled: RestartFromPause (Game.cs:124-128) -> RestartGame -> StartLevel, whose first statement is StopGameOverCountdown() (Game.cs:139, 206-213), cancelling the pending coroutine, with PlayerShip.Restart() (PlayerShip.cs:157-168) resetting _isDead, the sprite and control. The author wrote StopGameOverCountdown for precisely this interruption, which is evidence the state is anticipated and handled, not corrupted. Settings from pause closes back to pause (Game.cs:82-97). No state corruption in any of the three pause options.

3) No rule is named or violated. The exemplar conventions (explicit types, _camelCase [SerializeField] fields, WaitForSeconds cached before a while loop, brace-less single statements, no comments, member order, method named for what it does/returns) say nothing about where a bool is assigned, and no standard C#/Unity rule requires a state flag to be set synchronously rather than at the end of a timed sequence. The brief's design statement ("On death an explosion cutscene plays, then a defeat screen with restart") does not forbid pausing during the cutscene.

4) The only genuinely bad outcome gestured at — Resume() calling _playerShip.UnlockInput() (Game.cs:166, PlayerShip.cs:77-80) on a ship whose _playerMover was disabled at PlayerShip.cs:99 while PlayerMover.Jump (PlayerMover.cs:41-45) still executes — is explicitly scoped out by the reviewer as a separate Game.Resume() finding. Standing alone, this finding proves no misbehaviour of its own; keeping it would double-count that harm at a different file:line.

**Уточнения проверяющих.**

- The defect is real as stated, but the proposed fix covers only half of it: WinLevel (Game.cs:186-195) leaves _isRunning true in exactly the same way for the whole warp-jump cutscene (WarpJump.cs:66 disables player control there too, and _isRunning is not restored until StartLevel at Game.cs:143), so Escape opens the pause menu over the warp jump as well. The flag should be cleared in both end-of-level paths (or TogglePause should also check _isLevelOver), not only in LoseLevel.

---

### 24. Health.Value is serialized into the inspector but is always overwritten by MaxValue before anything reads it

**minor** · ось: OOP and encapsulation · `Assets/Scripts/Health/Health.cs:6` · с особым мнением

**Нарушенное правило.** Violated rule: do not serialize state that the object computes for itself. The serialized Value knob is visible and editable on the Health component of the Player Ship and Enemy Ship prefabs, yet whatever a designer types there is discarded by Awake -> Init -> `Value = MaxValue` (Health.cs:19) before the first reader in Start. It should be a plain auto-property: `public int Value { get; private set; }` with no [field: SerializeField], leaving MaxValue as the single authored number.

**Почему это дефект.** This is a concrete inspector lie, not a style opinion: the field advertises authored starting health that provably cannot take effect, because Unity calls Awake on every component before any Start (the Init path at line 14 runs unconditionally). A designer setting Value to 1 on a prefab to test a low-health case will see full health and have no way to tell why.

**Код:**

```csharp
Health.cs:6   [field: SerializeField] public int Value { get; private set; }
Health.cs:7   [field: SerializeField] public int MaxValue { get; private set; }
Health.cs:12  private void Awake()
Health.cs:14      Init();
Health.cs:17  public void Init()
Health.cs:19      Value = MaxValue;
Health.cs:20      Changed?.Invoke(Value);

Every reader of Value runs in Start or later, i.e. after all Awakes:
  SmoothHealthBar.cs:21  private void Start()
  SmoothHealthBar.cs:23      SetInstantly(GetPart(_health.Value));
  HealthLabel.cs:14      private void Start()
  HealthLabel.cs:16          Show(_health.Value);

The other Init callers re-run the same overwrite on every respawn:
  PlayerShip.cs:165      _health.Init();     // inside Restart()
  EnemyShip.cs:60        _health.Init();     // inside OnSpawn()
```

**Как чинить.** Remove `[field: SerializeField]` from Value on Health.cs:6, leaving `public int Value { get; private set; }`. MaxValue keeps its attribute. No call site changes are required - every read is through the property already.

**Риск правки.** Low. Removing the attribute drops the stored value from the Health components in Player Ship.prefab and Enemy Ship.prefab on the next save, which is harmless because it is overwritten anyway; MaxValue is a separate serialized entry and is unaffected.

**Особое мнение скептика.** The mechanical facts check out, but the finding is taste, not a defect, and it fails both of the review's own gates.

What I read and confirmed: Health.cs:6 `[field: SerializeField] public int Value { get; private set; }`, Health.cs:12-15 `Awake() { Init(); }`, Health.cs:19 `Value = MaxValue`. The only external readers are HealthLabel.cs:16 (in Start) and SmoothHealthBar.cs:23 (in Start); Health.cs:25/28/31 are internal. The Player Ship prefab instance in Assets/Scenes/SampleScene.unity:10018-10077 has no `m_IsActive` override and Assets/Prefabs/Player Ship.prefab:18 is `m_IsActive: 1`, so Health.Awake does run before those Starts. So yes, the authored number cannot influence behaviour.

Why that is not a finding:

1. No defect exists in this project, and no repro satisfies Rule 3 ("what the player does, in what order, and what goes wrong"). Both authored values already agree with MaxValue: Assets/Prefabs/Player Ship.prefab:264-265 `<Value>k__BackingField: 200` / `<MaxValue>k__BackingField: 200`, Assets/Prefabs/Enemy Ship.prefab:448-449 `20` / `20`. Nothing diverges, nothing misbehaves, and no player action produces a wrong result. The cited scenario is a hypothetical future designer typing a value nobody has typed — that is a speculative wart, not an observed defect.

2. The named rule does not exist. The exemplar (../Code style genius/BulletShooter.cs, PlacesMover.cs) contains no properties at all — only `[SerializeField] private` fields — so it says nothing about `[field: SerializeField]` on runtime-mutated state, and "do not serialize state that the object computes for itself" is not a standard C#/Unity convention either. Rule 4 requires a violated rule, not a preference.

3. The attribute is not dead weight: because the backing field is serialized, the Health component shows the ship's live current HP in the inspector during play, which is the ordinary reason Unity developers write it this way. Calling that an "inspector lie" presumes edit-time authoring is the field's only purpose; runtime inspection is the other, and it works.

4. Value is runtime state mutated by TakeDamage (Health.cs:28), not a value the object "computes for itself" — so even the rule's own premise is misapplied here.

At most this is a recommendation-level nicety (drop a redundant knob), not a minor finding, and the brief says to use "recommendation" sparingly.

**Уточнения проверяющих.**

- The claim is accurate as written; one tightening for honesty about impact. Both prefabs currently author Value == MaxValue (200/200 in Player Ship.prefab:264-265, 20/20 in Enemy Ship.prefab:448-449), so nothing is visibly wrong in the game today - the defect is dead, inert serialized configuration (an editable inspector knob that provably cannot influence behaviour), not a live misbehaviour. That is exactly why minor is the right severity. Also note the field does have one incidental use: because it is serialized, the inspector shows live health during Play mode; the fix should acknowledge that removing `[field: SerializeField]` gives up that debug readout (the property itself would no longer be drawn). The proposed fix is otherwise sound and needs no call-site changes - both readers (HealthLabel.cs:16, SmoothHealthBar.cs:23) go through the property.

---

### 6. WarpJump.Duration is public API with zero callers

**minor** · ось: OOP and encapsulation · `Assets/Scripts/Level/WarpJump.cs:24` · единогласно

**Нарушенное правило.** Violated rule: same as the previous finding - no external caller means it is not public API. Here there is no caller at all, so `public float Duration` at line 24 should simply be deleted. Game already learns when the cutscene ends the right way, by subscribing to Finished (Game.cs:37), which is why nothing needs the precomputed length.

**Почему это дефект.** It is dead public surface that a reader has to evaluate: Duration reads _warpEffect.main at line 30 and 37, so someone maintaining the particle settings has to work out whether changing them breaks a timing contract with Game. There is no such contract. Unlike PlayerShip.DeathDuration, which Game.cs:199 genuinely consumes, this one has never been wired up.

**Код:**

```csharp
WarpJump.cs:24  public float Duration => _alignDuration + Mathf.Max(EffectDuration, ActionsDuration);

grep -rn "Duration" over Assets/Scripts - every consumer of a duration goes elsewhere:
  Game.cs:199   yield return new WaitForSeconds(_playerShip.DeathDuration);
  PlayerShip.cs:55  public float DeathDuration => _explosionAnimation.Duration;
  WarpJump.cs:85    yield return new WaitForSeconds(TailDuration);   // private, line 39
No line anywhere reads a WarpJump's Duration. Game subscribes to WarpJump.Finished instead:
  Game.cs:37    _warpJump.Finished += StartNextLevel;

The two private helpers it composes are not dead - TailDuration (line 39) uses both:
  WarpJump.cs:26  private float EffectDuration
  WarpJump.cs:36  private float ActionsDuration =>
```

**Как чинить.** Delete the Duration property. Keep EffectDuration, ActionsDuration and TailDuration - they are private and TailDuration is used at line 85.

**Риск правки.** Very low - it is unreferenced, so removal is compiler-verified.

**Уточнения проверяющих.**

- Keep the finding, but trim the rationale. The claim "someone maintaining the particle settings has to work out whether changing them breaks a timing contract with Game" overstates the harm: EffectDuration (WarpJump.cs:26-34) and ActionsDuration (WarpJump.cs:36-37) both read _warpEffect.main and are read anyway by the live TailDuration (WarpJump.cs:39, consumed at WarpJump.cs:85), so the particle-settings coupling exists whether or not Duration is present. The accurate statement is narrower: WarpJump.cs:24 `public float Duration` is a dead public member with no caller anywhere in the project, and deleting it removes public surface without changing behaviour, since Game learns the cutscene ended through WarpJump.Finished (Game.cs:37, WarpJump.cs:90). EffectDuration, ActionsDuration and TailDuration stay.
- Keep the claim, but file it as kind "design" (dead public surface), never "behavior": there is no player-visible symptom and therefore no reproduction scenario — the proof is static reachability, which I reproduced. State the violated rule as "no dead code / do not expose a public member with no consumer" rather than "encapsulation"; the exemplar (BulletShooter.cs) exposes nothing public at all, and WarpJump's own remaining public surface (Play 43, Stop 51, Finished 41) all has callers in Game.cs. Minor is the right severity.
- Two precision fixes, neither changing the verdict. (1) The original reviewer's project-wide grep list is incomplete: bare-ish `Duration` also appears at Game.cs:199 (`_playerShip.DeathDuration`), ExplosionAnimation.cs:24 and :70 (`CalculateDuration`). Those are distinct identifiers, so WarpJump.Duration still has zero callers. (2) Reframe the violated rule: the school exemplar states no "a public member must have a caller" rule, so this should be reported as dead public surface — an unused member kept alive only by its own declaration — which is why minor is the correct severity rather than major. The proposed fix is sound as written: deleting line 24 alone compiles, since EffectDuration (26-34) and ActionsDuration (36-37) remain reachable through TailDuration (39) and the coroutine at lines 74 and 85.
- The defect is real; only the rule label is wrong. "Encapsulation: a public member must have a caller" is a misnomer — encapsulation concerns hiding state, not reachability. State it as the no-dead-code / minimal-public-surface rule: WarpJump.cs:24 exposes a computed public property that nothing in the single-assembly game ever reads, so it is unreachable code rather than API surface. Fix as proposed — delete line 24, leaving EffectDuration (26-34), ActionsDuration (36-37) and TailDuration (39), which are still used at :39, :74 and :85.

<sub>Слито из 4 формулировок других ревьюеров.</sub>

---

### 5. WarpJump.Stop() does not stop the two ExpandingFade effects it started, and ExpandingFade has no stop path at all, so they play on into the next level with the flash still attached to the player transform

**minor** · ось: Coroutines · `Assets/Scripts/Level/WarpJump.cs:51` · с особым мнением

**Сценарий воспроизведения.** 1. Clear a level so the warp cutscene plays.
2. Watch for the moment the ship vanishes and the shockwave rings out (WarpJump.cs:82-83). From the instant the flash starts (WarpJump.cs:78) you have about 0.65 s of wall time in which at least one ExpandingFade is mid-animation; pressing Escape freezes that window open, so the reaction window is comfortable.
3. Press Escape, then click Restart on the pause menu. RestartFromPause -> Resume -> RestartGame -> StartLevel -> _warpJump.Stop() kills Jump(), but nothing stops ExpandingFade.Expand() on Shockwave/Warp flash.
4. Level 1 restarts, and the shockwave quad keeps expanding out of nowhere over the fresh level for the remainder of its 0.5 s, growing to scale 10. If the flash was the one interrupted, it is worse: it was started with PlayAttached, so MoveToTarget() (ExpandingFade.cs:56, called every frame at :67) keeps snapping it onto the respawned player ship at its start position, so a bright quad rides the player for up to 0.3 s of the new level.

**Почему это дефект.** Not taste: Stop() exists specifically to tear down everything the cutscene put on screen - it already stops the coroutine and the particle system (StopWarpEffect, WarpJump.cs:59, 93-96). The two ExpandingFade effects are part of the same cutscene and were started by the same coroutine, but they are simply missed, so the teardown is incomplete and leaks visuals into the next level. This is the coroutine-outlives-its-owner case: Expand() has no stop path at all, so even a caller that wants to cancel it cannot.

**Код:**

```csharp
WarpJump.cs:51-60 - Stop() stops only its own coroutine and the particle system:
    public void Stop()
    {
        if (_jumping != null)
        {
            StopCoroutine(_jumping);
            _jumping = null;
        }

        StopWarpEffect();
    }

WarpJump.cs:78 and :83 - but Jump() starts two independent coroutines on two other GameObjects:
        _flash.PlayAttached(ship, _engineOffset);
        ...
        _shockWave.Play(ship.position);

ExpandingFade.cs:29-52 - the only entry points are Play/PlayAttached; the coroutine starter is private and there is no public stop/cancel:
    public void Play(Vector3 position) { _target = null; _offset = position; Restart(); }
    public void PlayAttached(Transform target, Vector3 offset) { _target = target; _offset = offset; Restart(); }

    private void Restart()
    {
        if (_playing != null)
            StopCoroutine(_playing);

        MoveToTarget();
        _playing = StartCoroutine(Expand());
    }

ExpandingFade.cs:59-76 - Expand() only hides the renderer after the full _duration has elapsed; there is no early-out:
    private IEnumerator Expand()
    {
        _renderer.enabled = true;

        for (float elapsed = 0f; elapsed < _duration; elapsed += Time.deltaTime)
        {
            float progress = elapsed / _duration;

            MoveToTarget();
            SetScale(Mathf.Lerp(_startScale, _endScale, _scaleCurve.Evaluate(progress)));
            SetStrength(Mathf.Lerp(_startStrength, 0f, progress));

            yield return null;
        }

        Hide();
        _playing = null;
    }

Game.cs:137-140 - Stop() is called on every level (re)start:
    private void StartLevel()
    {
        StopGameOverCountdown();
        _warpJump.Stop();

Scene values: "Warp flash" ExpandingFade _duration 0.3, _endScale 3; "Shockwave" ExpandingFade _duration 0.5, _endScale 10. Both are root GameObjects that are active and are never SetActive(false) (only their Renderer is toggled), so - per the Unity docs, deactivating a GameObject is what stops its coroutines - nothing else kills Expand() either.
```

**Как чинить.** Add a public Stop() to ExpandingFade that stops _playing, nulls it and calls the existing private Hide() (which already clears the strength and disables the renderer), then call _flash.Stop() and _shockWave.Stop() from WarpJump.Stop() alongside StopWarpEffect(). The private Restart() can then reuse the same stop path instead of its inline `if (_playing != null) StopCoroutine(_playing);`.

**Риск правки.** Low. Code-only, two small additions, no serialized data touched. Hide() is already the normal terminal state of Expand() and is already what Awake() calls, so calling it early cannot leave the effect in a state the object does not already reach. Sanity-check that the normal (uninterrupted) warp still looks right: with the scene values the tail wait (>= 0.7 s) already outlasts both fades, so Stop() is not reached while they are running in the normal flow.

**Особое мнение скептика.** The finding's headline claim — the two ExpandingFade effects "play on into the next level" — is disproved by the scene data. WarpJump.Jump() deliberately waits them out via TailDuration (WarpJump.cs:39 = EffectDuration - ActionsDuration; EffectDuration at :26-34 = main.duration + startLifetime.constantMax; ActionsDuration at :36-37). Scene values: SampleScene.unity:861 lengthInSec 5; :933-936 startLifetime minMaxState 0 / scalar 4 (constant 4); :8773-8776 _stretchAtWarpProgress 0.75, _stretchDuration 0.4, _dashDuration 0.15. So EffectDuration = 9, ActionsDuration = 4.3, TailDuration = 4.7 s. WarpJump.cs:85 therefore idles 4.7 s after the shockwave starts (:83) and 4.85 s after the flash starts (:78) before Finished fires (:90) and Game.StartLevel (Game.cs:137-140) calls Stop(). The flash runs 0.3 s (SampleScene.unity:5815) and the shockwave 0.5 s (:6584) — both have completed and already executed Hide() (ExpandingFade.cs:74) with ~10x margin. No fade ever leaks into the next level on any ordinary win, and the reviewer never computed TailDuration. The only surviving path is a hand-crafted one: pressing Escape inside a ~0.65 s slice of a ~9.6 s cutscene and then choosing Restart (Game.cs:124-128), producing at most 0.5 s of a decorative quad that then terminates and hides itself — no state corruption (PlayerShip.Restart(), PlayerShip.cs:157-168, restores sprite, scale and start position). No rule is violated either: the school exemplar defines no teardown/Stop convention (its only coroutine-lifetime idiom is `while (enabled)`), and ExpandingFade.Expand() is a bounded for-loop that always terminates and always calls Hide(), so it is not the coroutine-outlives-its-owner case the finding invokes. Under the brief's rule 6 this is at most cosmetic polish, not a proven defect or a violated rule. Two supporting details in the evidence are also wrong: _dashDistance is 4 in the scene (SampleScene.unity:8777), not the 16 code default, so the shockwave is left near the screen edge rather than 16 units away.

**Уточнения проверяющих.**

- Two refinements that make the finding tighter and honest about its scope, neither of which contradicts it:

1. The leak happens ONLY on the interrupted-cutscene path, never on the normal win -> next-level transition. With the scene's particle `lengthInSec: 5` (SampleScene.unity:861) and WarpJump's serialized values (`_stretchAtWarpProgress` 0.75, `_stretchDuration` 0.4, `_dashDuration` 0.15 at SampleScene.unity:8773-8775): ActionsDuration = 3.75 + 0.4 + 0.15 = 4.3, so TailDuration (WarpJump.cs:39) = max(0, 5 + startLifetime - 4.3) = 0.7 + startLifetime >= 0.7s. That is longer than both fades (0.3 and 0.5), so both have already reached Hide() before Finished fires at WarpJump.cs:90. The finding should not be read as "the cutscene normally bleeds into the next level" — it does not.

2. The reaction window is narrower than the write-up implies in context. The flash starts at t = 0.6 (align) + 3.75 + 0.4 = 4.75s into a cutscene that runs ~6s; the shockwave ends at 5.4s. So the Escape press must land in a 0.65s window near the end of a ~6s cutscene. Once it lands, the freeze is indefinite, so the rest of the repro is untimed.

Consequence is purely cosmetic and self-healing: the orphaned Expand() finishes its remaining <=0.3s / <=0.5s over the new level and then calls Hide() (ExpandingFade.cs:74), and the next Play/PlayAttached resets scale and strength from the start. No corrupted state — hence minor is the right severity, and the proposed fix (public Stop() on ExpandingFade reusing Hide(), called from WarpJump.Stop(), with Restart() reusing the same stop path) is correct.

<sub>Слито из 2 формулировок других ревьюеров.</sub>

---

### 25. SpawnPool allocates two Action<IPoolable> delegates for every single spawn/release cycle, on every pooled object in the game

**minor** · ось: Allocations and GC · `Assets/Scripts/Pools/SpawnPool.cs:73` · с особым мнением

**Нарушенное правило.** Violated rule: a delegate created from a method group is a fresh heap object at each evaluation under C# 9 (confirmed above by the absence of the `<>O` cache class in the compiled assembly). A pool exists to stop per-spawn heap traffic; this one reintroduces two heap objects per spawn/release pair, in the one class every spawned object in the game passes through.

How often: every object that leaves and re-enters a pool. Asteroids alone are 5 Get() + 5 Release() per second, continuously, for the entire level (2 per 0.4 s spawn tick, each removed when it crosses the ObjectRemover trigger via Asteroid.Remove -> Released -> SpawnPool.Release). Add player bullets (up to 4 Get + 4 Release/s), enemy bullets (~3/s with a 6-enemy wave), the hit effect spawned by each of those impacts, plus enemy explosions, dead bodies and bump effects. A level-1 wave in steady state is roughly 15 Get() + 15 Release() per second across the 14 SpawnPool instances the scene constructs.

How much garbage: exactly two Action<IPoolable> objects per object lifecycle — one at SpawnPool.cs:73, one at SpawnPool.cs:84. At ~15 cycles/s that is ~30 delegate objects per second. A Mono/IL2CPP delegate instance is on the order of 64-100 bytes, so roughly 2-3 KB/s, ~150 KB/min of pure churn. The object count is exact (IL-verified); the byte figure is an estimate of delegate instance size.

Also note ReleaseAll (lines 46-50) runs Release for every active item on each level restart, so a level transition with ~50 live objects allocates ~50 more delegates in one frame.

**Почему это дефект.** The receiver (`this`, the SpawnPool) and the target method (Release) are both fixed for the lifetime of the pool — the delegate is a constant, yet it is reconstructed on every Get and every Release. This is a proven allocation (two `newobj System.Action`1` instructions confirmed in the shipped IL), not a style preference, and it sits in the one piece of infrastructure whose entire purpose is to avoid per-spawn allocation. It is also a latent correctness smell: `-=` on line 84 relies on delegate value-equality to find the right subscriber, which works but is easy to break.

**Код:**

```csharp
SpawnPool.cs
 71	    private void PrepareForUse(T item)
 72	    {
 73	        item.Released += Release;
 74	    }
 82	    private void Release(IPoolable item)
 83	    {
 84	        item.Released -= Release;
 85	        _active.Remove((T)item);
 86	        _pool.Release((T)item);
 87	    }

IL from ikdasm of the dotnet build output (p.il, SpawnPool`1):
  PrepareForUse:
    IL_0008:  ldftn      instance void class SpawnPool`1<!T>::Release(class IPoolable)
    IL_000e:  newobj     instance void System.Action`1<class IPoolable>::.ctor(object, native int)
    IL_0013:  callvirt   instance void IPoolable::add_Released(...)
  Release:
    IL_0003:  ldftn      instance void class SpawnPool`1<!T>::Release(class IPoolable)
    IL_0009:  newobj     instance void System.Action`1<class IPoolable>::.ctor(object, native int)
    IL_000e:  callvirt   instance void IPoolable::remove_Released(...)

No Roslyn method-group cache exists to elide these: `strings -a p.dll | grep '<>O'` returns nothing, and /tmp/claude-501/compile/p.csproj pins `<LangVersion>9.0</LangVersion>` (Unity 6000.3's C# level), which predates C# 11 method-group conversion caching. Both newobj instructions execute every time.

Rate inputs actually configured in the project:
  Assets/Scenes/SampleScene.unity:9780  AsteroidSpawner _spawnDelay: 0.4
  AsteroidSpawner.cs:53-54  two Spawn() calls per tick -> 5 asteroid Get()/s
  Assets/Prefabs/Player Ship.prefab:232  _shootRate: 0.25 -> up to 4 bullet Get()/s
  Assets/Prefabs/Enemy Ship.prefab:322   _shootDelay: 2 (x N live enemies)
  Assets/Scenes/SampleScene.unity:9240   EnemySpawner _spawnDelay: 3
```

**Как чинить.** Add `private readonly Action<IPoolable> _release;` to SpawnPool and assign `_release = Release;` once in the constructor (alongside the four delegates the constructor already builds for ObjectPool at lines 18-25). Then use `item.Released += _release;` at line 73 and `item.Released -= _release;` at line 84. Both allocations disappear; the `-=` becomes a reference-equal removal of the exact instance that was added.

**Риск правки.** Very low, and it strictly improves the unsubscribe. `Action<IPoolable>` equality already made the current `-=` work, so caching one instance changes nothing semantically — it removes the identical delegate it added. No call sites outside SpawnPool touch these lines.

**Особое мнение скептика.** The allocation is factually real (I confirmed SpawnPool.cs:73 `item.Released += Release;` and SpawnPool.cs:84 `item.Released -= Release;`, with `event Action<IPoolable> Released` at IPoolable.cs:5, and C# 9 does allocate per method-group conversion). But "an allocation exists" is not "a defect" under this brief, and the named rule does not exist.

1) NO SUCH RULE IN THE EXEMPLAR. I read both exemplar files in full: /Users/baronkinvitaliy/Unity/UnityHomeWorks/Code style genius/BulletShooter.cs and PlacesMover.cs. Neither contains a single event subscription, let alone a delegate-caching convention. The one allocation-caching convention the exemplar demonstrates is `WaitForSeconds waitForSeconds = new WaitForSeconds(...)` hoisted above the `while` loop — and this project already complies (Assets/Scripts/Environment/AsteroidSpawner.cs:47). "A method group conversion allocates under C# 9" is a language fact, not a coding rule; the brief requires a VIOLATED RULE, and this one was invented for the occasion.

2) THE RULE IS APPLIED TO 2 OF 58 IDENTICAL SITES. `grep -rn -E '(\+=|-=) *[A-Z][A-Za-z0-9_]*;' Assets/Scripts --include='*.cs' | wc -l` returns 58. Assets/Scripts/Game.cs:35-45 and 62-72 alone are 22; Assets/Scripts/Ships/Player/PlayerShip.cs:36-52 are 12. Worse for the finding: Assets/Scripts/Ships/Enemy/EnemyShip.cs:34-44 subscribes and unsubscribes two events in OnEnable/OnDisable, and EnemyShip IS pooled (EnemySpawner.cs:35 `new SpawnPool<EnemyShip>(_prefab, created: OnEnemyCreated)`) with SpawnPool.cs:55 `SetActive(true)` / SpawnPool.cs:79 `SetActive(false)` firing those callbacks on every Get/Release. So the exact same spawn/release cycle the finding measures allocates FOUR delegates inside EnemyShip — double the two it blames on SpawnPool — and that goes unreported. Applied consistently, the rule demands a cached `readonly Action` field behind all 58 subscriptions in a 2341-line student project. That is exactly the disproportionate proposal the brief forbids, and it would make the standard OnEnable/OnDisable idiom less readable.

3) NO PROVEN SYMPTOM. Brief rule 3 requires "what goes wrong". Nothing does. The finding's own magnitude is self-labelled "an estimate" (~2-3 KB/s), and ProjectSettings/ProjectSettings.asset:589 sets `gcIncremental: 1`. Unity's manual (docs.unity3d.com/6000.3 .../performance-incremental-garbage-collection.html) states incremental GC "spreads out the process of garbage collection over multiple frames" so that "performance spikes related to garbage collection are reduced" — the documented mitigation for precisely this steady low-rate churn. No frame drop, hitch, or profiler evidence is offered; there is no reproduction in which a player observes anything.

4) THE "LATENT CORRECTNESS SMELL" IS FALSE. `-=` removing a method-group delegate by target+method equality is the guaranteed, documented C# event-unsubscription mechanism — it is how all 58 sites in this project work and how every Unity OnEnable/OnDisable pair works. Labelling the canonical pattern "easy to break" is not a defect.

5) THE FRAMING MISSTATES WHAT A POOL IS FOR. "A pool exists to stop per-spawn heap traffic" is wrong: SpawnPool exists to avoid `Object.Instantiate` (SpawnPool.cs:63) and `Object.Destroy` (SpawnPool.cs:91) — native object construction, component wiring, scene-graph churn, deferred destruction — which cost orders of magnitude more than an ~80-byte managed delegate. Two delegates per cycle do not "reintroduce" the cost the pool removes; the pool still delivers essentially all of its benefit. (For scale, SpawnPool.cs:85 `_active.Remove((T)item)` is an O(n) linear scan on every release, and ObjectRemover.cs:7 runs `GetComponentInParent<IRemoveable>()` on every trigger crossing in this same path — both larger per-cycle costs than the delegate, and neither is flagged.)

This is a micro-optimisation preference, not a proven defect or a violated rule. At absolute best it is kind:"recommendation"; as a filed finding it is a false positive.

**Уточнения проверяющих.**

- The claim survives, but one supporting argument is wrong and should be replaced (it makes the finding stronger, not weaker): the delegates are not uncached *because* p.csproj pins LangVersion 9.0. C# 11's method-group conversion caching applies only to STATIC method groups (dotnet/roslyn#58288 — the cache is a static field, so it cannot hold a delegate bound to a receiver). `SpawnPool.Release` is an instance method closed over `this`, so no C# version — 9, 11, 13 — would elide these two `newobj`s. Drop the LangVersion/`<>O` reasoning and state instead: an instance method group conversion allocates a fresh delegate at every evaluation under every current C# version, confirmed by the newobj at PrepareForUse IL_000e and Release IL_0009. Also soften the aside about `-=` being a "latent correctness smell": delegate value-equality (same target + same method) makes the current removal correct, so that sentence is not part of the defect.
- Two of the author's supporting arguments should be tightened. (1) The stated reason for no cache — "LangVersion 9.0 predates C# 11 method-group conversion caching" — is true for Unity 6.3 but is not the real reason: Roslyn caches ONLY static method-group conversions (dotnet/roslyn PR #58288), so a conversion of the instance method Release with receiver `this` allocates under C# 11, 12 and 13 as well. The defect is therefore version-independent, which strengthens the finding rather than weakening it. (2) The trailing "latent correctness smell" about `-=` at SpawnPool.cs:84 relying on delegate value-equality should be dropped: Delegate equality by target+method is spec-guaranteed, and the unsubscribe works correctly as written. The finding is purely an allocation finding. The byte/second figure remains an estimate (the object count of exactly 2 per lifecycle is IL-exact); at roughly 2-3 KB/s this is churn, not a behavioural defect, so minor is the correct severity.

---

### 18. _pushSpeed is overwritten by the Update clamp before physics ever integrates it, so values above _maxVerticalSpeed do nothing

**minor** · ось: Physics · `Assets/Scripts/Ships/Player/PlayerMover.cs:88` · с особым мнением

**Сценарий воспроизведения.** Fly into an asteroid. Push() runs inside OnTriggerEnter2D, i.e. during the physics step, and sets linearVelocity.y to +/-4. Unity's frame order runs the physics step before Update, so PlayerMover.Update executes next, reads 4, clamps it to 3 and writes it back - all before the next FixedUpdate integrates anything. The knockback the player feels is always exactly _maxVerticalSpeed. Raise _pushSpeed from 4 to 12 in the inspector to make the recoil punchier and nothing at all changes on screen.

**Почему это дефект.** A serialized tuning field whose configured value can never reach the simulation is a defect, not a preference: the inspector lies about what the game does, and the next person to tune the knockback will chase a value that has no effect. The same clamp is also what makes the bump feel identical whether you clip an asteroid or ram an enemy ship.

**Код:**

```csharp
PlayerMover.cs:73-89
    public void Push()
    {
        Vector2 direction;
        ...
        _rigidbody2D.linearVelocity = direction * _pushSpeed;
    }
PlayerMover.cs:37-38
        _verticalSpeed = Mathf.Clamp(_rigidbody2D.linearVelocity.y, -_maxVerticalSpeed, _maxVerticalSpeed);
        _rigidbody2D.linearVelocity = new Vector2(0, _verticalSpeed);
Player Ship.prefab:212 `_pushForce: 4` (mapped onto _pushSpeed by PlayerMover.cs:8 `[FormerlySerializedAs("_pushForce")]`), :213 `_maxVerticalSpeed: 3`.
Call path: PlayerCollisionHandler.cs:32-48 OnTriggerEnter2D -> PlayerShip.cs:118-127 OnAsteroidBumped -> PlayerShip.cs:175-178 PushOff -> PlayerMover.Push.
```

**Как чинить.** Either exempt the knockback from the per-frame clamp (track a short 'pushing' window during which Update leaves linearVelocity alone, clearing it once |velocity.y| falls back under _maxVerticalSpeed), or delete _pushSpeed and push at _maxVerticalSpeed explicitly so the code says what actually happens. The first is the behaviour the field's existence implies.

**Риск правки.** Low if you take the second option (pure simplification). The first option needs the exemption to expire, or a push that happens while the ship is already falling would leave the clamp permanently bypassed.

**Особое мнение скептика.** Mechanism checks out arithmetically, but it is a deliberate clamp saturating, not a defect, and two of the finding's supporting claims are false.

1) The numbers and the ordering are real. PlayerMover.cs:88 `_rigidbody2D.linearVelocity = direction * _pushSpeed;`, PlayerMover.cs:37-38 clamps `linearVelocity.y` to ±`_maxVerticalSpeed` every Update, and Assets/Prefabs/Player Ship.prefab:212 `_pushForce: 4` / :213 `_maxVerticalSpeed: 3` (no scene override — `grep -c _pushForce Assets/Scenes/SampleScene.unity` = 0). Call path confirmed: PlayerCollisionHandler.cs:32-48 -> PlayerShip.cs:118-138 -> PlayerShip.cs:175-178. Unity's execution-order doc confirms FixedUpdate -> physics simulation -> trigger callbacks -> Update in one frame. So yes, the extra 1 unit is clipped before integration.

2) But that is a clamp doing exactly what a clamp is written to do. PlayerMover.cs:37-38 is an explicit, unconditional, serialized speed cap the author wrote on purpose; `_maxVerticalSpeed` is the game's vertical speed limit. `_pushSpeed` is honoured across its entire meaningful range (0..3) and only saturates above the cap. The finding's own premise — "a serialized tuning field whose configured value can never reach the simulation" — is therefore false: the field reaches the simulation for every value the speed limit permits. "Configured 4, capped at 3" is the design speaking, not the inspector lying. Nothing in play goes wrong: bump an asteroid and the ship is shoved away at the game's top vertical speed, which is what the brief describes.

3) The load-bearing supporting claim is factually wrong. "The same clamp is also what makes the bump feel identical whether you clip an asteroid or ram an enemy ship" — no. PlayerShip.cs:125 and PlayerShip.cs:136 both call the same `PushOff()` (PlayerShip.cs:175-178) with the same `_pushSpeed`; asteroid and enemy bumps would be byte-for-byte identical with the clamp removed entirely. The clamp has nothing to do with it.

4) "Always exactly _maxVerticalSpeed" is also conditional, not absolute: the Unity doc confirms the physics loop can run more than once per frame, so at sub-50fps a second fixed step integrates the un-clamped 4 before Update ever sees it.

5) The proposed fix — a tracked "pushing" window that suspends the per-frame clamp until |velocity.y| falls back under the cap — adds a state flag and an exemption path to an 89-line mover, to buy a 4-vs-3 velocity difference over one 0.02s step (~0.02 world units) that no player can perceive. Disproportionate for a project this size, and it changes behaviour nobody has shown is wanted. Also note _tapForce is 2 (prefab:211), below the cap, so the clamp is not silently eating the player's ordinary input either.

No school-exemplar rule is named or violated (naming, explicit types, cached WaitForSeconds, member order are all untouched by this), and no player-observable misbehaviour exists. This does not survive as a minor behavioural defect.

**Уточнения проверяющих.**

- Claim stands; two edges of the rationale need tightening. (1) "Nothing at all changes on screen" is exact only at or above the 50 Hz fixed-step rate; below it a frame can run two fixed steps, so one 0.02 s step would integrate the un-clamped value (4 vs 3 = about 0.02 world units of extra travel, imperceptible, and framerate-dependent rather than deterministic). (2) "The same clamp is what makes the bump feel identical whether you clip an asteroid or ram an enemy ship" is wrong: PlayerShip.cs:125 and :136 both call the same PushOff -> Push with the same _pushSpeed, so the two bumps would be identical with or without the clamp. The defensible finding is narrower and fully verified: with the shipped values (_pushForce 4, _maxVerticalSpeed 3) the knockback is always delivered at exactly _maxVerticalSpeed, and any _pushSpeed above _maxVerticalSpeed is indistinguishable from _maxVerticalSpeed.
- Two precisions to the original wording, neither of which unseats the finding:

(a) The rationale sentence "The same clamp is also what makes the bump feel identical whether you clip an asteroid or ram an enemy ship" is wrong and should be dropped. Both paths call the identical `PlayerShip.PushOff()` (PlayerShip.cs:125 and :136) with the same `_pushSpeed`, so the two knockbacks would be identical with or without the clamp. What actually differs between the two collisions is only the particle gravityModifier (PlayerCollisionHandler.cs:6-8, :38-46).

(b) "Nothing at all changes on screen" is true at any normal frame rate but not literally unconditional: if a frame is long enough that two fixed steps run back to back, the un-clamped velocity is integrated for one 0.02s step before Update intervenes (≈0.18 extra units at _pushSpeed 12, ≈0.02 at 4). The accurate claim is: the push is capped at _maxVerticalSpeed every frame in which Update runs between physics steps, which is every frame at playable frame rates, so `_pushSpeed` has no tunable range above 3 and its shipped value of 4 is unreachable.
- The only honest residue is kind:"recommendation", not a defect: `_pushSpeed` is set to 4 (Player Ship.prefab:212) while the per-frame cap `_maxVerticalSpeed` is 3 (:213), so the last unit saturates against PlayerMover.cs:37-38 and the field's effective range is 0..3. If anything, the one-line note is "set _pushSpeed to 3 so the prefab value matches what the ship actually does" — a data tidy-up, not a code change, and not worth a finding slot.

---

### 23. PlayerShip.EnableControl and PlayerShip.ReturnToStart are public with no caller outside the class

**minor** · ось: OOP and encapsulation · `Assets/Scripts/Ships/Player/PlayerShip.cs:87` · единогласно

**Нарушенное правило.** Violated rule: a member's access level must match its widest actual caller - a method whose only caller is the declaring class is private. Both are reached only from PlayerShip.Restart (lines 166-167), so both should read `private void EnableControl()` and `private void ReturnToStart()`. ReturnToStart is additionally a pure pass-through: it adds nothing over `_playerMover.ReturnToStart()` and Restart could call the mover directly, exactly as PushOff (line 175) adds nothing over `_playerMover.Push()`.

**Почему это дефект.** Every public member is a promise about what outside code may do. EnableControl being public is the same hazard as finding 1 - it is a second public door onto _isControlEnabled - and ReturnToStart/PushOff advertise forwarding methods that only rename an existing call. Grep is the proof, not judgement: neither name appears outside PlayerShip.cs.

**Код:**

```csharp
PlayerShip.cs:87   public void EnableControl()
PlayerShip.cs:103  public void ReturnToStart()
PlayerShip.cs:105      _playerMover.ReturnToStart();
PlayerShip.cs:157  public void Restart()
PlayerShip.cs:166      EnableControl();
PlayerShip.cs:167      ReturnToStart();

grep -rn "EnableControl" -> PlayerShip.cs:87 (decl) and PlayerShip.cs:166 (self) only.
  (DisableControl by contrast has two real external callers: Game.cs:53 and WarpJump.cs:66.)
grep -rn "ReturnToStart" -> PlayerShip.cs:103 (decl), PlayerShip.cs:105, PlayerShip.cs:167 (self);
  the other hits are PlayerMover's own ReturnToStart at PlayerMover.cs:31 and :59.

Same shape one method lower:
PlayerShip.cs:175  private void PushOff()
PlayerShip.cs:177      _playerMover.Push();
```

**Как чинить.** Change both to private. Then inline ReturnToStart and PushOff into their call sites (`_playerMover.ReturnToStart()` at line 167, `_playerMover.Push()` at lines 125 and 136) and delete them, since a one-line forwarder that only renames the call adds no meaning.

**Риск правки.** Very low. Access-level narrowing is compiler-checked and the methods are not referenced from any UnityEvent in the scene (they take no arguments but nothing in SampleScene.unity wires them - all screen buttons go through the event fields in StartScreen/PauseScreen/GameOverScreen).

**Уточнения проверяющих.**

- The defect is exactly this and no more: PlayerShip.cs:87 and PlayerShip.cs:103 are declared `public` but their only call sites in the entire project are PlayerShip.cs:166 and :167, both inside PlayerShip.Restart. Both should read `private void EnableControl()` and `private void ReturnToStart()`. Drop the rest of the proposed fix. PushOff (PlayerShip.cs:175) is already `private`, so it is not an access-level violation and does not belong in this finding; and the claim that ReturnToStart/PushOff should be deleted as "forwarders that only rename the call" is a preference, not a named rule - under the school's own headline rule (a method is named for what it does) `PushOff()` names the domain action in PlayerShip's vocabulary while `_playerMover.Push()` is the mover's, so deleting it is not a proven defect. Also note the path in the evidence: WarpJump.cs is Assets/Scripts/Level/WarpJump.cs.
- The access-modifier half survives exactly as written: PlayerShip.cs:87 and :103 should be `private`, since `Restart()` (public, called from Game.cs:145) is the real external entry point. Trim the rest of the proposed fix. Deleting `PushOff` (PlayerShip.cs:175, called twice at :125 and :136) is not a proven violation of anything — it is already private, and the exemplar itself keeps a private single-purpose helper (PlacesMover.cs:30) rather than inlining it, so "a one-line forwarder adds no meaning" is preference, not the school's rule. Inlining `ReturnToStart` into line 167 is likewise optional; the demonstrable defect is the access level only. Also drop the "second public door onto _isControlEnabled" framing as the justification — EnableControl does four things (UnlockInput, `_playerMover.enabled = true`, `Resume()`, collider on), so it is not a duplicate of the public UnlockInput (:77, genuinely called from Game.cs:166); it is simply an internal step exposed for no caller.
- Narrow the finding to the access-level claim only: PlayerShip.cs:87 EnableControl and PlayerShip.cs:103 ReturnToStart are public though their only call sites are PlayerShip.cs:166-167 inside Restart(); both should read `private void`. Inlining ReturnToStart into line 167 as `_playerMover.ReturnToStart();` is a fair secondary note since the method only renames one call. Drop two parts of the original write-up. (1) Drop the proposal to delete PushOff (PlayerShip.cs:175): it is already private, so it breaks no access rule, and it has two call sites (lines 125 and 136) where it names the domain action rather than the mover's mechanism — removing it is taste, not a defect. (2) Drop the "second public door onto _isControlEnabled" rationale: UnlockInput (PlayerShip.cs:77) is already a public door onto that same field and is legitimately called from Game.cs:166, so EnableControl exposes nothing new. The finding rests on the least-permissive-access rule alone.
- Two path/scope corrections to the cited evidence, neither affecting the claim: the external DisableControl caller is at Assets/Scripts/Level/WarpJump.cs:66 (not Assets/Scripts/WarpJump.cs), and the "no external caller" conclusion should be stated as resting on both the *.cs grep and the absence of any UnityEvent persistent-call wiring in the project's own scene and prefabs (verified: zero m_MethodName entries there, with ForceText serialization). Fix stands as proposed: `private void EnableControl()` at PlayerShip.cs:87 and `private void ReturnToStart()` at PlayerShip.cs:103, both still invoked from Restart at lines 166-167; nothing else changes and it cannot break compilation, since no other translation unit references either member.

<sub>Слито из 1 формулировок других ревьюеров.</sub>

---

### 13. Die() never zeroes Health, so an out-of-bounds death shows the defeat screen with a full health bar

**minor** · ось: Restart and level transition · `Assets/Scripts/Ships/Player/PlayerShip.cs:140` · с особым мнением

**Сценарий воспроизведения.** Start a level and hold W until the ship flies off the top of the screen. ObjectRemover top fires -> PlayerShip.Remove() -> Die(): the ship explodes, control is dropped and the defeat screen appears - but the health bar and the "200 / 200" label are still full for the whole death animation and on the defeat screen, because Health.Value was never reduced. The same happens flying off the bottom.

**Почему это дефект.** The HUD contradicts the game state: the player is dead while the health readout says untouched. Health is the single source of truth for the health HUD (HealthLabel.cs:24-27, SmoothHealthBar.cs:31-48 both subscribe to Health.Changed), and Die() bypasses it, so the two can disagree. It is not taste - one of the two displayed facts is wrong.

**Код:**

```csharp
PlayerShip.cs:140-155
  140  public void Die()
  141  {
  142      if (_isDead)
  143          return;
  145      _isDead = true;
  147      ExplosionAnimation explosion = _explosionPool.Get(transform.position);
  149      explosion.Play();
  151      DisableControl();
  152      _shipSprite.enabled = false;
  154      Died?.Invoke();
  155  }            <- _health is never touched

PlayerShip.cs:170-173
  170  public void Remove()
  171  {
  172      Die();
  173  }

ObjectRemover.cs:5-10 - anything with IRemoveable that enters the trigger is killed:
    5  private void OnTriggerEnter2D(Collider2D other)
    7      IRemoveable removeable = other.GetComponentInParent<IRemoveable>();
    9      removeable?.Remove();

Scene: ObjectRemover top/bottom are rotated 90 deg (m_LocalRotation z: 0.7071068, m_LocalEulerAnglesHint z: 90) at y = +10 / -10 with localScale (1, 31, 1) on a 1x1 BoxCollider2D, i.e. horizontal walls 31 units wide; ProjectSettings/Physics2DSettings.asset m_LayerCollisionMatrix is all 'ff', so the Player layer (6) does interact with them. PlayerMover.Update (line 37) clamps vertical SPEED but nothing clamps vertical POSITION.

Health.cs:23-33 TakeDamage is the only thing that lowers Value, and Health.cs:17-21 Init() is what the HUD listens to.
SampleScene.unity Canvas child order puts "Health Bar" (index 2) after "Game Over" (index 1), so the HUD renders on top of the defeat screen.
```

**Как чинить.** Have Die() drain the health first, e.g. `_health.TakeDamage(_health.Value);` before the explosion. That is re-entrancy safe: Health.TakeDamage fires Died -> PlayerShip.Die(), which returns immediately on the `_isDead` guard, provided `_isDead = true` is set before the call. Alternatively give Health an explicit Kill()/Empty() method and call it from Die().

**Риск правки.** Low, but the ordering matters: set `_isDead = true` before touching Health or the Died event fires twice into Game.LoseLevel (which is itself guarded by _isLevelOver, so worst case is harmless, but the intent should be explicit).

**Особое мнение скептика.** Mechanics verified and correct, but the alleged rule does not exist. PlayerShip.cs:140-155 indeed never touches _health, and PlayerShip.cs:170-173 routes Remove() to Die(); ObjectRemover.cs:5-10 is a trigger (SampleScene.unity, "ObjectRemover top" m_IsTrigger: 1, m_LocalPosition y: 10, layer 0) and the player is layer 6 with Physics2DSettings.asset:56 m_LayerCollisionMatrix all 'ff', so the boundary death is reachable; both HUD widgets bind the same Health (two "_health: {fileID: 92249142}" refs) and "Health Bar" is Canvas sibling index 2 vs "Game Over" index 1 over an a:0.72 backdrop, so the bar does show on the defeat screen. What the finding gets wrong is the verdict. (1) The invariant "dead implies Health == 0" is the reviewer's, not the project's: IDamageable.cs:1-5 and IRemoveable.cs:1-4 are deliberately separate concepts, and EnemyShip.cs:72-75 proves the intent - Remove() releases to the pool with no explosion, no Died, no score, i.e. leaving the playfield is explicitly NOT damage (matching the brief's "an enemy that flies off-screen counts toward the wave but not toward the score"). The player cannot be despawned quietly, so Remove() reuses the death presentation, but no damage was dealt and the readout of 200/200 (Player Ship.prefab:264-265) is literally accurate. (2) No school-exemplar or standard C#/Unity rule requires the HUD to be emptied on a non-damage death; this is a presentation preference, excluded by rule 6. (3) Zero consequence: PlayerShip.cs:165 Restart() calls _health.Init(), so nothing persists. (4) The repro as written fails - InputReader.cs:16 uses Input.GetKeyDown, so holding W fires one jump; with _tapForce 2, _maxVerticalSpeed 3, m_GravityScale 0.2 the remover at y=+10 sits five units above the visible top (Main Camera at y=0, orthographic size 5), so it takes a deliberate off-screen tapping climb, and the explosion then plays at y=10 off-camera, killing the "full bar during the death animation" half of the symptom. (5) The proposed fix _health.TakeDamage(_health.Value) fabricates a 200-point damage event nobody dealt and would make SmoothHealthBar.cs:31-48 play a drain plus a 1s damage-hold flash over the defeat screen.

**Уточнения проверяющих.**

- The defect stands; three details of the repro are wrong and should be restated. (a) "Hold W" does not climb: InputReader.cs:16 uses Input.GetKeyDown, so holding W fires one jump of _tapForce 2 (Player Ship.prefab:211). The easiest repro is the opposite - start a level and press nothing: gravityScale 0.2 pulls the ship down, PlayerMover.cs:37 caps the fall at 3 u/s, and ~4-5 s later it crosses "ObjectRemover bottom" and Die() runs. Climbing off the top needs W tapped repeatedly. (b) Death does not happen at the screen edge: the Main Camera is orthographic size 5 at (0,0,-10) (SampleScene.unity:6717-6718), so the visible band is y -5..+5 while the kill walls sit at y +/-10 - the ship flies ~5 units off-screen first, and the explosion it spawns is off-camera too. (c) "Full health bar" is only the undamaged case; if the player was hit during the level the bar/label freeze at whatever value remained. The invariant that is actually broken is "dead => Health.Value == 0", which holds on the damage path (PlayerShip.cs:40 subscribes Die to Health.Died, Health.cs:31-32) but not on the IRemoveable path. The proposed fix checks out: putting _health.TakeDamage(_health.Value) after _isDead = true (PlayerShip.cs:145) is re-entrancy safe - the nested Died fires Die() which returns at PlayerShip.cs:142-143, and on the normal path Health.cs:25 already returns early once Value == 0.
- The defect stands; only the reproduction is wrong. InputReader.cs:16 uses Input.GetKeyDown(_jumpKey), so "hold W" produces exactly one jump, not a climb — reaching the top remover needs repeated taps of W. The reliable repro is the opposite direction and requires no input at all: press START, then press nothing. PlayerShip.Restart() (PlayerShip.cs:157-168) makes the body Dynamic again and puts it at _startPosition (-6, 0) (Player Ship.prefab:217); with m_GravityScale 0.2 and _maxVerticalSpeed 3 the ship falls past the camera's bottom edge (Main Camera at y=0, orthographic size 5) after about 2.4 s and reaches "ObjectRemover bottom" at y=-10 after about 4 s. ObjectRemover fires -> PlayerShip.Remove() -> Die(): the ship explodes (off-screen, 5 units below the view) and roughly a second later the defeat screen appears with the health bar still completely full and the label reading "200 / 200", because Health.Value was never reduced. The proposed fix is also sound as written: with _isDead = true already set at PlayerShip.cs:145, a _health.TakeDamage(_health.Value) inserted after it re-enters Die() through Health.cs:32 and returns immediately at the guard on line 142, and in the normal death path it is a no-op because Health.cs:25 rejects damage <= 0.

---

### 9. PlayerShooter._shootDelayer is written twice and never read - dead coroutine handle, and off the project's coroutine-handle convention

**minor** · ось: Coroutines · `Assets/Scripts/Ships/Player/PlayerShooter.cs:14` · единогласно

**Нарушенное правило.** Violated rule: a private field that is written and never read is dead state, and a stored Coroutine handle whose only purpose is to allow cancellation but which is never passed to StopCoroutine is misleading - it advertises a cancellation path that does not exist. The project's own established convention (10 other handles, all with a stop method - listed in the evidence) is what this one breaks.

What it should look like: either the field is deleted, so Reload() just clears _isOnCooldown (and the writes at :43 and :56 become a bare `StartCoroutine(Reload());` and nothing), or - better, because it also fixes a loose end - it gains the stop method the convention calls for and PlayerShip.Restart() calls it, so the reload state is reset with the rest of the ship.

The loose end: because nothing stops or resets Reload(), the HUD reload bar keeps animating after the player is dead (ReadinessChanged fires every frame at PlayerShooter.cs:50 for the rest of the 0.25 s _shootRate while the death explosion plays) and _isOnCooldown is carried across a restart rather than cleared by PlayerShip.Restart().

**Почему это дефект.** Not taste: the field is provably unread (grep over all of Assets/Scripts shows exactly three occurrences, two writes and the declaration), which is dead code by any standard, and the C# compiler agrees - CS0414 is only silent here because the review csproj explicitly suppresses it. It is also a concrete inconsistency with a convention the author follows in all ten other cases in this codebase.

**Код:**

```csharp
PlayerShooter.cs - the field's three and only occurrences (verified by grep over Assets/Scripts):
    14:    private Coroutine _shootDelayer;
    43:        _shootDelayer = StartCoroutine(Reload());
    56:        _shootDelayer = null;

PlayerShooter.cs:31-58 in full context:
    public void Shoot()
    {
        if (_isOnCooldown)
            return;
        ...
        _isOnCooldown = true;
        _shootDelayer = StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        for (float elapsed = 0f; elapsed < _shootRate; elapsed += Time.deltaTime)
        {
            ReadinessChanged?.Invoke(elapsed / _shootRate);

            yield return null;
        }

        _isOnCooldown = false;
        _shootDelayer = null;
        ReadinessChanged?.Invoke(1f);
    }

There is no StopCoroutine anywhere in PlayerShooter.cs. Every other Coroutine handle in the project has a matching stop: Game._gameOverCountdown -> StopGameOverCountdown (Game.cs:206-213); WarpJump._jumping -> Stop (WarpJump.cs:51-60); EnemySpawner._spawning -> StopSpawn (EnemySpawner.cs:57-64); AsteroidSpawner._spawning -> StopSpawn (AsteroidSpawner.cs:36-43); EnemyShooter._shooting -> StopShoot (EnemyShooter.cs:36-43); ExplosionAnimation._lifetimeCountdown -> OnDespawn (ExplosionAnimation.cs:42-51); EnemyDeadBody._lifetimeCountdown -> StopCountdown (EnemyDeadBody.cs:39-46); BumpEffect._lifetimeCountdown -> StopCountdown (BumpEffect.cs:46-53); SmoothHealthBar._healthFalling/_damageFalling -> StopFalling (SmoothHealthBar.cs:77-84); ExpandingFade._playing -> inline stop in Restart (ExpandingFade.cs:45-52).

PlayerShip.cs:157-168 - Restart() resets everything else about the ship but never touches the shooter's cooldown:
    public void Restart()
    {
        _isDead = false;
        _shipSprite.enabled = true;
        transform.localScale = _startScale;
        _playerShooter.ReleaseAll();
        _explosionPool.ReleaseAll();
        _playerCollisionHandler.ReleaseAll();
        _health.Init();
        EnableControl();
        ReturnToStart();
    }

Note: `dotnet build ... 0 Warning(s)` is NOT evidence that the field is used - the review csproj at /tmp/claude-501/compile/p.csproj carries `<NoWarn>CS0649;CS0414;CS0169</NoWarn>`, which masks exactly the CS0414 "assigned but its value is never used" diagnostic this field would raise.
```

**Как чинить.** Give PlayerShooter the stop method the rest of the project uses, e.g. a public ResetCooldown() that does `if (_shootDelayer == null) return; StopCoroutine(_shootDelayer); _shootDelayer = null;` then `_isOnCooldown = false; ReadinessChanged?.Invoke(1f);`, and call it from PlayerShip.Restart() (Assets/Scripts/Ships/Player/PlayerShip.cs:157-168) next to the existing _playerShooter.ReleaseAll(). That makes the field load-bearing, resets the HUD reload bar on every restart, and clears a cooldown carried over from the previous life. If you would rather keep the class minimal, delete _shootDelayer entirely (lines 14, 43's assignment, 56) and leave _isOnCooldown as the only state.

**Риск правки.** Low. Code-only. The added call sits in a method that already re-initialises health, pools and position, and ReadinessChanged has a single subscriber (ShootCooldownBar, Assets/Scripts/UI/ShootCooldownBar.cs:10) that only writes an anchor, so an extra invoke is harmless. If you take the delete option instead, confirm nothing external ever intended to cancel the reload - nothing does today.

**Уточнения проверяющих.**

- The behavioural "loose end" is overstated and should be narrowed or dropped; it is not what makes this a finding. (a) "_isOnCooldown is carried across a restart" does NOT happen on the ordinary death path: nothing stops Reload(), but nothing needs to — it keeps running in real time and self-clears at PlayerShooter.cs:55-57 after _shootRate, which the prefab sets to 0.25 (Assets/Prefabs/Player Ship.prefab:232), whereas Game.cs:197-204 waits _playerShip.DeathDuration before the defeat screen even appears, and that duration is the player explosion's full particle length (Assets/VFX/Sparks/Player ship explosion.prefab:44 lengthInSec 5, :119 startLifetime 5, summed by ExplosionAnimation.CalculateDuration at ExplosionAnimation.cs:70-89) — seconds, not 0.25 s. The only window where cooldown actually survives into a new life is restart-from-pause (Game.cs:124-128) pressed within 0.25 s of a shot: Pause sets Time.timeScale = 0 (Game.cs:155), Unity's docs state deltaTime "also becomes 0" at timeScale 0 and only WaitForSeconds coroutines are suspended, so the `yield return null` loop at PlayerShooter.cs:48-53 spins without advancing `elapsed` and resumes its leftover fraction of a second after Resume(). (b) "the HUD reload bar keeps animating after the player is dead" is literally true (ShootCooldownBar.cs:10 stays subscribed, PlayerShooter is never disabled) but is cosmetically harmless — the bar just fills and ends at ReadinessChanged(1f). Keep the finding as what it is: dead, unread state that advertises a cancellation path the class does not have, against a convention the author follows in all 11 other cases. kind should be "style" (or "design"), not "behavior".
- Keep the finding, but strike its two supporting arguments - both are wrong, and one is a checkable factual error that would damage the report.

1. The CS0414 argument is false. I rebuilt with the suppression removed: `dotnet build /tmp/claude-501/compile/p.csproj -t:Rebuild -v m --nologo -p:NoWarn=""` surfaces all 166 previously-masked warnings, every one of them CS0649 - and zero CS0414 anywhere, including for _shootDelayer. A minimal scratch repro confirms why: Roslyn emits CS0414 only for constant assignments (`private int _v; _v = 5;` and `private string _s; _s = "x";` both warn), while a reference-type field assigned from a method call or from `null` (`_handleField = Make(); _handleField = null;`) produces no warning at all. So the compiler does not "agree" here and NoWarn is not what silences it; CS0414 could never fire for a Coroutine handle assigned from StartCoroutine. The evidence for this finding is the grep and the read, nothing more - say only that.

2. The "loose end" behavioural claims should go. "_isOnCooldown is carried across a restart" is unreachable on both normal restart paths, with _shootRate = 0.25 (Assets/Prefabs/Player Ship.prefab:232): on death, Game.LoseLevel -> ShowGameOver waits WaitForSeconds(_playerShip.DeathDuration) and only then shows the screen, whose button the player must click (Game.cs:175-204, 41), all at timeScale 1 - Time.timeScale is touched only at Game.cs:50/155/164, never on death; on a win, StartNextLevel runs after WarpJump, whose Duration is _alignDuration (0.6s serialized default) plus more (WarpJump.cs:15, 24). Both are far longer than 0.25s, so Reload has always completed and cleared _isOnCooldown before Restart. Only pause -> restart (Game.cs:124-128) can leave a residue, and Resume() restores timeScale first, so it self-clears within a quarter second. And the reload bar animating during the death explosion is real in mechanism (PlayerShip.Die at :140-155 disables control and the sprite but neither disables PlayerShooter nor deactivates the ship - SetActive appears only on UI panels and SpawnPool items) yet is not a defect: the loop ends by invoking ReadinessChanged(1f) at PlayerShooter.cs:57, leaving the bar full, which is exactly what a fresh ship should show.

What remains, and what should be reported: a private field written at :43 and :56 and read nowhere - dead state, and the only one of eleven coroutine handles in the project that advertises a cancellation path it does not have. The honest fix is the minimal one: delete line 14 and the assignments at :43 and :56, leaving `StartCoroutine(Reload());` and _isOnCooldown as the class's only state. The ResetCooldown()/PlayerShip.Restart() variant should not be presented as fixing a bug, because there is no bug to fix.
- Keep it as a minor style finding, stripped to the verified fact and the exemplar-shaped fix:

"PlayerShooter._shootDelayer (Assets/Scripts/Ships/Player/PlayerShooter.cs:14) is written at :43 and :56 and never read — the only three occurrences in Assets/Scripts. The stored handle is never passed to StopCoroutine, so it is unused state that advertises a cancellation path the class does not have. Delete line 14, make :43 a bare `StartCoroutine(Reload());`, and drop :56; _isOnCooldown stays as the class's only state. That matches the exemplar, which starts its coroutine without storing a handle (../Code style genius/BulletShooter.cs: `StartCoroutine(Shoot());`)."

Drop from the write-up: the CS0414 / NoWarn argument (Roslyn does not warn on this field at all), the claim that the cooldown or reload bar survives a restart, and the ResetCooldown()+PlayerShip.Restart() proposal. There is no behavioural consequence — this is cleanliness only.

<sub>Слито из 1 формулировок других ревьюеров.</sub>

---

## 3. SOLID

### SRP — ❌ нарушен

**Где.** Assets/Scripts/Game.cs:5-214 (four state groups: _level 25 written 118/132 read 149/172; _isPaused 28; _isSettingsFromPause 29; _isLevelOver 26 + _isRunning 27); Assets/Scripts/Ships/Player/PlayerShip.cs:6-178 (input routing 11, 41-42, 51-52, 61-75, 77-85 + VFX pool ownership 8-9, 16, 18, 31, 55, 147-149, 163); Assets/Scripts/Ships/Enemy/EnemySpawner.cs:6-105 (placement 9-12, 89-104 + five pools 14-18, 27-36, 44-48, 66-71 + wave counter 20-22, 51-52, 78-87)

**Чем грозит.** Not a size complaint - in all three files the co-resident responsibilities provably write each other's state. (1) Game: the pause concern reaches into the outcome concern. Resume() (161-168) calls _playerShip.UnlockInput() unconditionally, re-arming input that Die() (PlayerShip.cs:151) and the warp cutscene (WarpJump.cs:66) had deliberately taken away; and TogglePause (99-108) has to consult the outcome concern's _isRunning to decide whether pausing is legal, while that flag is cleared inside a coroutine (202) so the two disagree for the whole death animation. Those are report findings 1, 2 and 12 - the same class's four jobs colliding. (2) EnemySpawner: the _isRestarting flag (22, set 42, cleared 50, read 80) exists for no reason other than to stop one responsibility (emptying the five pools at 44-48, which fires Retired through EnemyShip.OnDespawn at EnemyShip.cs:69) from corrupting another responsibility in the same file (the wave counter at 83-86). A guard one responsibility needs against another inside one class is the violation made visible. (3) PlayerShip: two public control pairs for one concept - LockInput/UnlockInput (77-85) and EnableControl/DisableControl (87-101) over the single field _isControlEnabled (23) - and callers pick different doors (Game.cs:53 DisableControl, 157 LockInput, 166 UnlockInput; WarpJump.cs:66 DisableControl), which is the structural root of the dead-ship-still-shoots state.

**Как разносится.** Split each per the breakdowns below: Game -> ScreenNavigator + PauseController + LevelProgression + a ~85-line Game that still acts as the composition root and keeps all eleven event subscriptions in one OnEnable/OnDisable pair; PlayerShip -> PlayerInputRouter + ShipExplosion + a ~130-line ship facade; EnemySpawner -> EnemyFactory + a plain Wave class + a ~45-line spawner. Two rules to apply while splitting: the pause gate belongs where input is read, not on the ship, and the pause round-trip must restore the control state it captured rather than setting a fixed 'unlocked'.

### OCP — ❌ нарушен

**Где.** Assets/Scripts/Ships/Player/PlayerCollisionHandler.cs:32-49 (type branches at 34 and 43, per-type constants 6-8, per-type events 14-15); propagating to Assets/Scripts/Ships/Player/PlayerShip.cs:58-59 and 118-138; Assets/Scripts/Ships/Player/PlayerShipAudio.cs:7-8, 12-13, 18-19, 22-30

**Чем грозит.** The player's collision reaction dispatches on concrete type, so it must be modified to be extended. Adding one new hazard (a mine, a second enemy body, a destructible crate) costs six new members plus five edited methods across three existing files: a gravity constant, an event and a new if-branch in PlayerCollisionHandler.OnTriggerEnter2D; an event and a near-duplicate handler in PlayerShip (118-127 and 129-138 are literally the same five statements with a different damage source) plus both OnEnable/OnDisable; a Sound field and a Play method in PlayerShipAudio plus both OnEnable/OnDisable. This is the only concrete-type collision dispatch in the project - I grepped every TryGetComponent/GetComponent/GetComponentInParent site in Assets/Scripts - and the project's own counter-example sits five lines long in ObjectRemover.cs:5-10, where GetComponentInParent<IRemoveable>() makes a new removable type cost zero edits to existing code.

**Как разносится.** One interface, one branch. Add IBumpable next to the existing interfaces: int Damage { get; }, Sound BumpSound { get; }, float GetBumpGravity(Vector3 bumperPosition), void Bump(). Implement it on Asteroid (Damage already exists at Asteroid.cs:17; GetBumpGravity returns -2 or +4 from bumperPosition.y exactly as PlayerCollisionHandler.cs:38-40 decides today; Bump() is empty because an asteroid survives the ram) and on EnemyCollision (Damage forwards EnemyShip.BodyDamage, gravity +1, Bump() keeps its existing _enemyShip.Die() at EnemyCollision.cs:11). OnTriggerEnter2D collapses to one TryGetComponent, one Bumped?.Invoke(bumpable), one PlayBump(other, bumpable.GetBumpGravity(transform.position)) and one bumpable.Bump(); PlayerShip.cs:58-59 collapses to one event and 118-138 to one handler; PlayerShipAudio plays bumpable.BumpSound. Two behaviour-preservation constraints the obvious version of this refactor gets wrong: the asteroid gravities are two different magnitudes (-2 above, +4 below), not one magnitude with a sign, so the choice must stay a per-implementer computation; and the enemy's death-on-ram must survive as Bump() or ramming stops killing enemies. Editor cost: the two bump clips move off PlayerShipAudio onto the five Asteroid prefabs and Enemy Ship.prefab, so re-assign them or they go silent. Do not stretch IBumpable to cover pickups - healing is a different capability.

### LSP — ❌ нарушен

**Где.** Assets/Scripts/Interfaces/IRemoveable.cs:1-3, consumed at Assets/Scripts/Environment/ObjectRemover.cs:5-10; implementations Assets/Scripts/Environment/Asteroid.cs:43-46, Assets/Scripts/Ammo/Ammo.cs:41-44, Assets/Scripts/Ships/Enemy/EnemyShip.cs:72-75 (all three raise Released -> SpawnPool.Release, SpawnPool.cs:82-87) versus Assets/Scripts/Ships/Player/PlayerShip.cs:170-173 -> Die() (PlayerShip.cs:140-155). Second instance: Assets/Scripts/Pools/IPoolable.cs:7 OnSpawn, honoured at Asteroid.cs:32-36 but not at EnemyShip.cs:57-63 (EnemyMover.cs:12-16)

**Чем грозит.** Three of the four IRemoveable implementations recycle an object; the fourth ends the match. The single consumer cannot tell them apart, so the player's death line is wherever the pooling boundary happens to sit - and it was sited for pool hygiene: four 1x31 trigger walls at y = +10 (SampleScene.unity:8065), y = -10 (:9605), x = -15 (:9514) and x = +15 (:9727), against a camera of orthographic size 5 at the origin (:6718, :6741), i.e. 5+ units outside the frame on every side. The measurable cost is report finding 16: the player stays alive and controllable for about a second after the ship has left the screen, then explodes ~3.8 units below the visible edge where the particles are off-camera and ExplosionAnimation.cs:64 additionally mutes the death sound because ScreenArea.Contains is false. The boundary cannot simply be brought in either, because asteroids are spawned with centres out to y = +/-5.3 (AsteroidSpawner.cs:10-13, SampleScene.unity:9782-9785) and their silhouettes reach past +/-6 - the two meanings packed into one interface are in direct tuning conflict. The same call also skips the health drain (PlayerShip.Die at 140-155 never touches _health), so the defeat screen can show a non-empty health bar - a state the damage path, which reaches Die through Health.Died at PlayerShip.cs:40, can never produce. Second instance: SpawnPool.Get promises an as-new item (SpawnPool.cs:52-58 activates then calls OnSpawn), and Asteroid delivers that by re-rolling both randomised values, while EnemyShip does not - EnemyMover draws its speed in Awake (EnemyMover.cs:15), which runs once per instance at SpawnPool.cs:63, so the serialized 0.5-3 range is sampled once per pooled instance rather than once per spawn (report finding 4).

**Как разносится.** Stop overloading the cleanup interface with a game rule. Drop IRemoveable from PlayerShip.cs:6 and delete PlayerShip.Remove() (170-173); give the ship its own boundary - a small PlayerBounds trigger pair placed just outside the visible edge whose OnTriggerEnter2D calls _playerShip.Die(). The four wide removers then mean exactly one thing, stay where they are for pooled debris, and the player's death line becomes independently tunable, which is precisely what finding 16's fix needs and could not have without this separation. For the second instance: move the Random.Range out of EnemyMover.Awake into Fly() (EnemyMover.cs:18-21), whose only caller is EnemyShip.OnSpawn (EnemyShip.cs:61), leaving only GetComponent in Awake - no call site changes, and it matches Asteroid.OnSpawn.

### ISP — ❌ нарушен

**Где.** Assets/Scripts/Interfaces/IDamageable.cs:1-5, implemented at Assets/Scripts/Ships/Player/PlayerShip.cs:6 and Assets/Scripts/Ships/Enemy/EnemyShip.cs:5; the four members it mandates are PlayerShip.cs:108, PlayerShip.cs:140, EnemyShip.cs:77, EnemyShip.cs:87

**Чем грозит.** The interface has zero clients anywhere in the project - a grep for IDamageable across all 42 scripts returns only the declaration and the two implements clauses; no field, parameter, local, cast or TryGetComponent is ever typed as it. So the whole cost of the abstraction is paid by the implementers, and it is a fat interface: it bundles two unrelated capabilities, 'can be damaged' (TakeDamage) and 'can be destroyed on command' (Die). It forces four public methods of which three have no caller outside their own class - PlayerShip.TakeDamage:108 (callers 115, 120, 131, all internal), PlayerShip.Die:140 (reached only through the Health.Died subscription at :40 and Remove at :172), EnemyShip.TakeDamage:77 (caller 84, internal). Only EnemyShip.Die:87 has a genuine external caller, EnemyCollision.cs:11, and would stay public regardless. The comparison inside the same project is what makes this a rule rather than a preference: the other two interfaces earn their keep - IRemoveable is consumed as a type at ObjectRemover.cs:7 and IPoolable at SpawnPool.cs:6, 73, 78, 84.

**Как разносится.** Either delete Assets/Scripts/Interfaces/IDamageable.cs (and its .meta) with the two implements clauses and narrow PlayerShip.TakeDamage, PlayerShip.Die and EnemyShip.TakeDamage to private; or keep it, shrink it to a single member - void TakeDamage(int damage) - and give it the client it was written for, per the DIP remedy. Do not do both halves at once and do not split IPoolable while you are in there: Ammo.cs:32-34 and EnemyDeadBody.cs:19-21 do implement OnSpawn as empty bodies, but two empty methods across six implementers is a proportionate price for a two-member pool contract and splitting it would buy nothing. Also drop the redundant public modifiers on IDamageable.cs:3-4 and IRemoveable.cs:3 - interface members are public by definition, and IPoolable.cs:5-9 already omits them, so the project currently contradicts itself.

### DIP — ❌ нарушен

**Где.** Assets/Scripts/Ships/Enemy/EnemySpawner.cs:27-36 (especially 29, 31, 32, 34), served by Assets/Scripts/Ships/Enemy/EnemyShip.cs:8-9 and 20-21 and Assets/Scripts/Ships/Enemy/EnemyShooter.cs:7 and 16; second address Assets/Scripts/Ships/Player/PlayerShip.cs:12, 37, 47, 113-116 and Assets/Scripts/Ships/Enemy/EnemyShip.cs:7, 36, 42, 82-85 against the unused abstraction at Assets/Scripts/Interfaces/IDamageable.cs:1-5

**Чем грозит.** EnemySpawner's Awake depends on the internal component layout and private wiring of objects it does not own, three hops deep: _prefab.GetComponent<EnemyShooter>().AmmoPrefab (29) and then ammoPrefab.HitEffectPrefab (34), plus _prefab.ExplosionPrefab (31) and _prefab.DeadBodyPrefab (32). Two serialized fields on EnemyShip (8-9) and one on EnemyShooter (7) are published as getters for that single caller and nothing else: grep confirms EnemyShip.ExplosionPrefab is read only at EnemySpawner.cs:31, EnemyShip.DeadBodyPrefab only at :32 and EnemyShooter.AmmoPrefab only at :29 - Ammo.HitEffectPrefab is the exception and must stay public, because PlayerShooter.cs:22 is a second, legitimate reader of its own serialized prefab. The practical cost: moving the enemy's gun onto a different component, or giving the enemy a second projectile, breaks the spawner rather than the ship; and EnemyShip.Init (46-55) is a four-parameter pool list of which two are forwarded straight through at line 54. The project already shows the right shape on the player side at PlayerShooter.cs:21-22, where the shooter builds its pools from its own prefab and nothing reaches into it. Second address: the damage intake is wired to the concrete AmmoHitHandler four times per damageable (serialized field, subscribe, unsubscribe, forwarder) although AmmoHitHandler.cs:18 already holds the ammo and its damage, and the sibling case already inverts correctly at ObjectRemover.cs:7.

**Как разносится.** Give EnemySpawner its own [SerializeField] fields for the four prefabs it pools (ExplosionAnimation, EnemyDeadBody, Ammo, hit-effect ExplosionAnimation) and delete the traversal at 29-34; then delete EnemyShip._explosion/_deadBody and both getters (8-9, 20-21) and EnemyShooter.AmmoPrefab (16), and shrink EnemyShip.Init to the two pools it actually stores. The pools must stay on the spawner - they are shared across the wave and StartWave's central reset (44-48) depends on holding every handle, so per-shooter Awake pools (one per pooled ship, since SpawnPool.cs:63 instantiates N clones and Unity runs Awake per clone) would leave enemy bullets in flight with nothing able to recall them at the next level. Second address: have AmmoHitHandler.OnTriggerEnter2D resolve an OPTIONAL IDamageable with GetComponentInParent and call TakeDamage(ammo.Damage) in the exact position HitDetected fires today (AmmoHitHandler.cs:18, before ammo.Hit at 19 and ammo.Remove at 20 - otherwise a lethal hit spawns its impact effect after the victim has been pooled); then PlayerShip.cs:12/37/47/113-116 and EnemyShip.cs:7/36/42/82-85 all delete. Optional resolution is mandatory, not defensive: the same AmmoHitHandler component also sits on all five Asteroid prefabs (Asteroid 1.prefab, same GameObject as Asteroid, which deliberately is not IDamageable) where it must remain a pure bullet-consumer. One behaviour delta to accept knowingly: damage would no longer depend on the ship component being enabled - inert here, since neither PlayerShip nor EnemyShip is ever disabled (the only .enabled writes in the project are PlayerShip.cs:90/99 on PlayerMover and the two colliders/renderers).

**Циклы в графе зависимостей.** не найдено

### Раскладка «толстых» классов

#### Game (Assets/Scripts/Game.cs) — 214 строк

**Причины для изменения, которые в нём сейчас сосуществуют:**

1. Screen navigation: which panel is on screen and what Back means (fields 17-20, _isSettingsFromPause 29, the panel setup in Start 54-57, OpenSettingsFromStart/OpenSettingsFromPause/CloseSettings 75-97)
2. Pause: what freezing the game means (NormalTimeScale/PausedTimeScale 8-9, _input 11, _isPaused 28, TogglePause 99-108, Pause 152-159, Resume 161-168, the Time.timeScale and AudioListener.pause writes at 50-51, 155-156, 164-165)
3. Level progression and the difficulty curve (FirstLevel 7, _firstWaveSize/_waveSizeGrowth 21-22, _level 25 written at 118 and 132, GetWaveSize 170-173, LevelChanged 31 raised at 149)
4. Match-outcome arbitration: what wins, what loses, and when the defeat screen appears (_gameOverCountdown 24, _isLevelOver 26, _isRunning 27, LoseLevel 175-184, WinLevel 186-195, ShowGameOver 197-204, StopGameOverCountdown 206-213)

**Во что распадается:**

| Класс | Ответственность | Что переезжает |
|---|---|---|
| `ScreenNavigator` | Owns which panel is visible and what Back returns to. Nothing else may call Show/Hide on a screen. | Fields _startScreen/_pauseScreen/_settingsScreen/_gameOverScreen (17-20); _isSettingsFromPause (29) replaced by a real GameScreen _previous reference; the four panel calls in Start (54-57); OpenSettingsFromStart/OpenSettingsFromPause/CloseSettings (75-97); the ten screen subscriptions in OnEnable/O… |
| `PauseController` | Owns the single answer to 'is the game frozen', including Time.timeScale and AudioListener.pause. | NormalTimeScale/PausedTimeScale (8-9), _input (11) and its PauseToggled subscription (45, 72), _isPaused (28), TogglePause (99-108), Pause (152-159), Resume (161-168), and the two resets in Start (50-51). Raises Paused/Resumed. Two fixes land here while the code is moving: capture PlayerShip's curr… |
| `LevelProgression` | Which level it is and how big this level's wave is. | FirstLevel (7), _firstWaveSize/_waveSizeGrowth (21-22), _level (25, written at 118 in RestartGame and 132 in StartNextLevel, read at 149 and 172), GetWaveSize (170-173), LevelChanged (31, raised at 149). API: int Current, int WaveSize, Restart(), Advance(), event Action<int> Changed. LevelViewer.cs… |
| `Game (remaining, ~85 lines)` | Runs one match: start a level, decide win or lose, hand off to the cutscenes. | Keeps _playerShip, _enemySpawner, _asteroidSpawner, _killCounter, _warpJump (12-16), _gameOverCountdown (24), _isLevelOver (26), _isRunning (27), StartGame/RestartGame/RestartFromPause/StartNextLevel/StartLevel (110-150) and LoseLevel/WinLevel/ShowGameOver/StopGameOverCountdown (175-213). |

**Связывание после разбиения.** All four components sit on the existing Game GameObject, so the inspector work is dragging the references that are already there onto the new components. Game holds three serialized references (_screens, _pause, _levels) plus its five gameplay references; it stays the only class that subscribes across concerns. StartLevel becomes: StopGameOverCountdown(); _warpJump.Stop(); _isLevelOver = false; _isRunning = true; _screens.HideAll(); _playerShip.Restart(); _enemySpawner.StartWave(_levels.WaveSize); _asteroidSpawner.StartSpawn(); _levels.Announce();. PauseController must never hold a reference back to Game - Game pushes an IsPausable flag down when a match starts and ends. That matters because the 42-script dependency graph is currently acyclic (Tarjan SCC pass over every declared type name in every file, 82 over-approximated edges: no component larger than one, no mutual pair), and a PauseController -> Game back-reference would create the project's first cycle. Do the split in three passes - screens, then pause, then levels - re-checking the eleven subscriptions in OnEnable/OnDisable (33-46, 60-73) after each, because that is where this refactor breaks. While moving the outcome code, also clear _isRunning inside both LoseLevel (175-184) and WinLevel (186-195) instead of inside ShowGameOver (202): today it stays true for the whole of both cutscenes. Regression path after each pass: start -> play -> pause -> settings -> back -> resume -> clear a wave -> warp -> next level -> die -> restart.

#### PlayerShip (Assets/Scripts/Ships/Player/PlayerShip.cs) — 178 строк

**Причины для изменения, которые в нём сейчас сосуществуют:**

1. Input adaptation and the control-lock policy (_input 11, _isControlEnabled 23, subscriptions 41-42 and 51-52, OnJumped/OnShot 61-75, and the two overlapping public control pairs 77-85 and 87-101)
2. Death presentation: owning an object pool for one VFX instance and publishing its length as the game's cutscene timer (PoolCapacity/PoolMaxSize 8-9, _explosionAnimation 16, _explosionPool 18 built at 31, DeathDuration 55, the explosion block in Die 147-149, the ReleaseAll at 163) - duplicated verbatim in EnemyShip.cs:94-100
3. The ship facade itself: damage in, death, restart, collision relay, visibility (TakeDamage 108-116, Die 140-155, Restart 157-168, Remove 170-173, the bump handlers 118-138 and PushOff 175-178) - this part stays

**Во что распадается:**

| Класс | Ответственность | Что переезжает |
|---|---|---|
| `PlayerInputRouter` | Turns InputReader events into ship commands, and is the only thing that knows about keys. | _input (11), the Jumped/Shot subscriptions (41-42, 51-52) and OnJumped/OnShot (61-75). It asks PlayerShip whether the ship is controllable rather than keeping a second copy of that state; the pause gate moves out to PauseController/InputReader entirely, which is what lets LockInput/UnlockInput (77-… |
| `ShipExplosion` | Owns the single-instance explosion pool and reports how long the explosion plays. | PoolCapacity/PoolMaxSize (8-9), _explosionAnimation (16), _explosionPool (18) and its construction (31), DeathDuration (55), the three explosion lines in Die (147-149) and the ReleaseAll at 163. Die() then reads _explosion.Play() and Game.cs:199 waits on _explosion.Duration. The same component drop… |
| `PlayerShip (remaining, ~130 lines)` | The ship facade: takes damage, dies, restarts, relays bumps, and owns its own visibility. | Keeps TakeDamage/OnAmmoHit (108-116), Die (140-155), Restart (157-168), Remove (170-173), Died (57), the collision relay (58-59 and 118-138, which the OCP fix collapses to one event and one handler) and PushOff (175-178). Gains a Hide()/Show() pair wrapping _shipSprite.enabled so WarpJump stops wri… |

**Связывание после разбиения.** Both new components sit on the Player Ship prefab root beside PlayerShip and take a serialized PlayerShip reference; Game keeps its single _playerShip reference and its Died subscription unchanged, and Game.cs:199 either reads the new component directly or keeps a one-line DeathDuration forwarder on PlayerShip. One correction to the obvious version of this split: do NOT simply delete Lock/UnlockInput and point Game.Resume at EnableControl. EnableControl (87-93) also sets _playerMover.enabled = true, calls _playerMover.Resume() (bodyType back to Dynamic) and re-enables _bodyCollider, so resuming from a pause taken while dead would produce a fully physical invisible ship - strictly worse than today's gate-only flip. The end state must be one control gate that the router reads, the pause gate outside the ship, and Restart() as the only path that revives a dead ship. Restart (157-168) is the fragile part of this refactor: it resets six things and the split must not drop _explosionPool.ReleaseAll (163), which is what stops the previous run's explosion from reappearing. After the split, test: play -> asteroid hit -> die -> restart, and confirm health, sprite, scale, start position, the reload bar and the absence of leftover bullets.

#### WarpJump (Assets/Scripts/Level/WarpJump.cs) — 143 строк

**Причины для изменения, которые в нём сейчас сосуществуют:**

1. The cutscene choreography and its tuning (the eight serialized fields 13-20, Play/Stop 43-60, Jump 62-91, Align/Stretch/Dash/Progress 98-142) - one timeline, one reason, and it stays
2. Presentation state that belongs to the player ship: a second serialized reference to the ship's SpriteRenderer (9) written at 82, while PlayerShip declares the same renderer at PlayerShip.cs:15 and owns its flag at 152 and 160
3. A dead timing API: public Duration (24) has no reader anywhere in the 42 scripts - Game learns the cutscene ended through Finished (Game.cs:37, WarpJump.cs:90) - and it is the only public member of the four-member duration algebra (24, 26-34, 36-37, 39), of which only the private TailDuration is actually used (85)
4. An incomplete teardown: Stop() (51-60) stops its own coroutine and the particle system but not the two ExpandingFade effects Jump() started at 78 and 83, and ExpandingFade has no stop path at all

**Во что распадается:**

| Класс | Ответственность | Что переезжает |
|---|---|---|
| `WarpJump (remaining, ~125 lines)` | Directs the warp cutscene and announces when it has finished. No new class is warranted here - this is one timeline with one reason to change; what it needs is to give back what it took. | Keeps _playerShip (8), the three effects (10-12), the eight tuning fields (13-20), Play/Stop/Finished, Jump, Align, Stretch, Dash, Progress, and TailDuration (39) with its two private helpers (26-34, 36-37). Delete public Duration (24) - unreferenced, so removal is compiler-verified. Line 82 become… |
| `PlayerShip (existing, receives)` | Continues to own the ship's visibility, now as a command instead of a leaked field. | A Hide()/Show() pair wrapping _shipSprite.enabled, next to the existing DisableControl/EnableControl. PlayerShip.cs:160 already restores the renderer on Restart, so Show() has a caller from day one and nothing else changes. |
| `ExpandingFade (existing, receives)` | Gains the cancellation path it never had, so a caller that starts it can stop it. | A public Stop() that stops _playing, nulls it and calls the existing private Hide() (78-82); the private Restart() (45-52) then reuses it instead of its inline if (_playing != null) StopCoroutine(_playing). Hide() is already the normal terminal state of Expand() (74) and already what Awake() calls… |

**Связывание после разбиения.** No new class and no new scene object: three code edits plus clearing one inspector field on the WarpJump component. Game keeps its single _warpJump reference and its Finished subscription (Game.cs:37, 64) untouched. Deliberately NOT moved: the direct writes to ship.rotation (104), ship.localScale (117) and ship.position (130). A cutscene animating a foreign Transform through the public transform API is ordinary Unity practice - ExpandingFade.cs:56 does the same with its _target - and pushing align/stretch/dash onto PlayerShip or PlayerMover would put cutscene choreography inside the gameplay class, which is a worse trade for a project this size. The one thing worth knowing about them is that PlayerShip.cs:161 (transform.localScale = _startScale) exists solely to undo WarpJump.cs:117, so if you ever do move them, move that repair line with them.

#### EnemySpawner (Assets/Scripts/Ships/Enemy/EnemySpawner.cs) — 105 строк

**Причины для изменения, которые в нём сейчас сосуществуют:**

1. Spawn placement and cadence (_spawnDelay, _spawnPositionX, _minSpawnPositionY, _maxSpawnPositionY at 9-12; the Spawn coroutine 89-104)
2. Pool ownership for four kinds of object it never spawns - explosions, dead bodies, enemy bullets and bullet hit effects (five pool fields 14-18, all five built in Awake 27-36 by reaching through the enemy prefab, all five emptied in StartWave 44-48, four injected per ship at 66-71)
3. Wave-depletion bookkeeping and the threshold Game turns into a win (_waveSize/_retiredCount 20-21, the reset at 51-52, OnEnemyRetired 78-87, and the >= test at 85 that raises WaveCleared into Game.cs:35)

**Во что распадается:**

| Класс | Ответственность | Что переезжает |
|---|---|---|
| `EnemyFactory` | Builds enemies and everything an enemy emits, and hands them out. | The five pool fields (14-18) and all of Awake (27-36), but with its own four [SerializeField] prefab fields (explosion, dead body, ammo, hit effect) instead of the reach-through at 29-34; the five ReleaseAll calls (44-48); OnEnemyCreated (66-71). Exposes EnemyShip Get(Vector3 position), ReleaseAll(… |
| `Wave` | Counts one wave down and says when it is cleared. | _waveSize and _retiredCount (20-21), the reset at 51-52, and the body of OnEnemyRetired (83-86). Plain C# class, no MonoBehaviour: Wave(int size), void Register(), event Action Cleared. _isRestarting (22, set 42, cleared 50, read 80) disappears with it - but only if the spawner installs the new Wav… |
| `EnemySpawner (remaining, ~45 lines)` | Places enemies at intervals and reports what happened to them. | Keeps the placement fields (9-12), StartWave/StopSpawn (38-64) and the Spawn coroutine (89-104); holds an EnemyFactory and a Wave; re-raises WaveCleared and EnemyDied so Game.cs:35, Game.cs:62 and KillCounter.cs need no change at all. _spawnPositionX (10) belongs to this class and is also the entry… |

**Связывание после разбиения.** EnemyFactory goes on the same GameObject as EnemySpawner and KillCounter, so KillCounter.cs:4's RequireComponent(typeof(EnemySpawner)) and its GetComponent at :15 are unaffected, and Game keeps its single _enemySpawner reference. Assign all four prefabs on EnemyFactory in the inspector or the pools construct on null prefabs. Three constraints: the pools must stay on this one shared component (per-shooter pools would leave StartWave with no handle on bullets already in flight, so they would survive into the next level); StartWave must keep emptying all five pools before spawning or the previous level's bullets and explosions leak forward; and the Wave/ReleaseAll ordering above. If only one change is affordable, make it the four serialized prefab fields - that alone deletes the three-hop traversal at 29-34 and lets EnemyShip.cs:8-9/20-21 and EnemyShooter.cs:7/16 be deleted outright, with EnemyShip.Init shrinking from four pools to the two it actually stores.

---

## 4. Стиль и нейминг

Эталон — `../Code style genius/`. Одна строка на правило, а не на нарушение.

| Правило | Нарушений | Файлы | Примеры |
|---|---:|---|---|
| A field or a type is named for what it actually holds or does, not for a neighbouring concept or for the value it displays. | 10 | Ships/Player/PlayerMover.cs, Ships/Player/PlayerShooter.cs, ScoreViewer.cs, UI/GameOverScreen.cs, UI/SettingsScreen.cs, Level/ExpandingFade.cs, Pools/SpawnPool… | Assets/Scripts/Ships/Player/PlayerMover.cs:7 - [SerializeField] private float _tapForce; (never passed to AddForce; assigned straight into linearVelocity at :43, so it is a speed in units/second) -> [SerializeField] private float _jumpSpeed; matching _pushSpeed at :9, which the author already renamed from _pushForce (… |
| A method is named for what it does or returns, not for something adjacent to it (the school's headline rule: the graders' one edit in the exemplar was GetNextPlace -> GetNextPosition). | 6 | Ships/Enemy/EnemyDeadBody.cs, Ships/Player/PlayerShip.cs, UI/SmoothHealthBar.cs, Ammo/AmmoHitHandler.cs | Assets/Scripts/Ships/Enemy/EnemyDeadBody.cs:28 - public void End() (it rebinds the animator at :32-33 and starts the lifetime coroutine at :36, i.e. it BEGINS playback) -> public void Play(), so the death sequence at EnemyShip.cs:96/100 reads deadBody.Play(); ... explosion.Play(); instead of one End and one Play for t… |
| Declaration groups appear in one order and are never split: serialized fields -> private fields -> properties/events -> Unity methods -> public methods -> private methods (exemplar BulletShooter.cs a… | 4 | Ships/Player/PlayerShip.cs, Ships/Player/PlayerCollisionHandler.cs, Level/WarpJump.cs, Ammo/ExplosionAnimation.cs | Assets/Scripts/Ships/Player/PlayerShip.cs:55-59 - public float DeathDuration and three public event Action declarations are wedged below OnDisable, so the class's whole public surface is invisible from the top of the file -> move them above Awake (:26), directly after the private-field block that ends at :24 Assets/Sc… |
| Public methods precede private helpers; the two groups are never interleaved (exemplar BulletShooter.cs and PlacesMover.cs both end with the private method; 17 of this project's 20 classes that have… | 4 | Ships/Player/PlayerShip.cs, Ships/Enemy/EnemyShip.cs, Ships/Player/PlayerMover.cs | Assets/Scripts/Ships/Player/PlayerMover.cs:73 - public void Push() is declared after private void StopMotion() (:67), so a public entry point called from PlayerShip.cs:177 reads as though it were tacked on after the class was finished -> Push() between ReturnToStart() (:59-65) and StopMotion() Assets/Scripts/Ships/Ene… |
| No abbreviations in identifiers. Init is a truncation of Initialize, in the same family as pos/dmg/mgr. | 4 | Health/Health.cs, Ammo/Ammo.cs, Ships/Enemy/EnemyShip.cs, Ships/Enemy/EnemyShooter.cs (4 declarations; 7 call sites in Health.cs:14, EnemyShip.cs:54/60, Player… | Assets/Scripts/Health/Health.cs:17 - public void Init() -> public void Initialize() (call sites Health.cs:14, EnemyShip.cs:60, PlayerShip.cs:165) Assets/Scripts/Ships/Enemy/EnemyShip.cs:46 - public void Init(SpawnPool<ExplosionAnimation> explosionPool, ...) -> public void Initialize(...) (call site EnemySpawner.cs:68)… |
| Identifiers use correct English. Drop-e verbs form -able: remove -> removable (like move -> movable). | 8 | Interfaces/IRemoveable.cs (and its filename), Environment/ObjectRemover.cs, Ammo/Ammo.cs, Environment/Asteroid.cs, Ships/Enemy/EnemyShip.cs, Ships/Player/Playe… | Assets/Scripts/Interfaces/IRemoveable.cs:1 - public interface IRemoveable -> public interface IRemovable (file renamed to IRemovable.cs) Assets/Scripts/Environment/ObjectRemover.cs:7 - IRemoveable removeable = other.GetComponentInParent<IRemoveable>(); -> IRemovable removable = other.GetComponentInParent<IRemovable>()… |
| The type is written out in a `new` expression - no target-typed `new()` (exemplar BulletShooter.cs:18 `new WaitForSeconds(_timeWaitShooting)`; this project writes the explicit form 26 times and the s… | 7 | Environment/AsteroidSpawner.cs, Ships/Enemy/EnemySpawner.cs, Ships/Enemy/EnemyShooter.cs, Pools/SpawnPool.cs, Level/WarpJump.cs | Assets/Scripts/Environment/AsteroidSpawner.cs:47 - WaitForSeconds spawnDelay = new(_spawnDelay); -> WaitForSeconds spawnDelay = new WaitForSeconds(_spawnDelay); (the exact line the exemplar writes at BulletShooter.cs:18; EnemySpawner.cs:91 and EnemyShooter.cs:47 are the same line) Assets/Scripts/Level/WarpJump.cs:13 -… |
| Every file uses one line ending throughout and ends with a final newline. | 7 | Health/Health.cs, Environment/ObjectRemover.cs, Interfaces/IRemoveable.cs, Ships/Enemy/EnemyCollision.cs, Ships/Player/PlayerMover.cs, Ships/Player/PlayerShip.… | Assets/Scripts/Health/Health.cs:1 - `using System;` ends CRLF while the other 32 lines end LF -> one ending for the whole file; as it stands every future diff of Health.cs is noisy for anyone who opens it on another OS Assets/Scripts/Ships/Player/PlayerShip.cs:178 - file ends with `}` and no newline -> add the final n… |
| One concept keeps one name across the whole codebase. | 6 | Ships/Player/PlayerShooter.cs, Ships/Enemy/EnemyShooter.cs, KillCounter.cs, ScoreViewer.cs, UI/GameOverScreen.cs, UI/ShootCooldownBar.cs, Ammo/Ammo.cs, Ammo/Bu… | Assets/Scripts/Ships/Player/PlayerShooter.cs:9 vs Assets/Scripts/Ships/Enemy/EnemyShooter.cs:8 - _shootRate vs _shootDelay for the identical serialized pause between shots -> both _shootDelay Assets/Scripts/KillCounter.cs:9 -> Assets/Scripts/ScoreViewer.cs:7 -> Assets/Scripts/UI/GameOverScreen.cs:24/26 - one number is… |
| A method name is a verb phrase; Start/Stop take a gerund, and a bare noun is never a method name. | 6 | Ships/Enemy/EnemyShooter.cs, Environment/AsteroidSpawner.cs, Ships/Enemy/EnemySpawner.cs, Level/WarpJump.cs | Assets/Scripts/Ships/Enemy/EnemyShooter.cs:29 - public void StartShoot() -> public void StartShooting() (and :36 StopShoot() -> StopShooting()) Assets/Scripts/Environment/AsteroidSpawner.cs:26 - public void StartSpawn() -> public void StartSpawning() (and :36 StopSpawn() -> StopSpawning(); same for EnemySpawner.cs:57)… |
| A WaitForSeconds with a fixed duration is built once and reused, not allocated per call (exemplar BulletShooter.cs:18 hoists it above the loop and yields it at :28; Unity 6000.3 Manual, Coroutines: "… | 5 | Ammo/ExplosionAnimation.cs, Ships/Enemy/EnemyDeadBody.cs, Ships/Player/BumpEffect.cs, UI/SmoothHealthBar.cs, Game.cs | Assets/Scripts/Ammo/ExplosionAnimation.cs:99 - yield return new WaitForSeconds(lifetime); one allocation per pooled activation, and this is the project's most replayed effect (every bullet impact, every enemy death, the player's death); the caller at :67 always passes Duration, which is memoised into _duration at :23-… |
| Assets are named for what they are, in one convention across a family. | 4 | Assets/Scenes/SampleScene.unity, Assets/Prefabs/PlayerBullet.prefab, Assets/Prefabs/PlayerBulletHit.prefab, Assets/Prefabs/Enemy Ship dead body.prefab | Assets/Scenes/SampleScene.unity - the game's only scene still carries the name Unity generated, which says only that Unity created it -> name it for the game, e.g. Game.unity Assets/Prefabs/PlayerBullet.prefab vs Assets/Prefabs/Enemy Bullet.prefab - one family of assets spelled two ways, concatenated PascalCase and sp… |
| Statement whitespace follows the exemplar: a space between a control keyword and its parenthesis, no trailing whitespace on any line, and a blank line separating a block statement from the statements… | 3 | Ships/Player/PlayerMover.cs, Ships/Enemy/EnemyShooter.cs | Assets/Scripts/Ships/Player/PlayerMover.cs:77 - if(transform.position.y > 0) -> if (transform.position.y > 0) - the only `if(` without a space in all 42 scripts (exemplar PlacesMover.cs:26 `if (transform.position == _placePosition)`, BulletShooter.cs:20 `while (enabled)`) Assets/Scripts/Ships/Player/PlayerMover.cs:80… |
| Interface members carry no `public` modifier - they are public by definition, so the keyword carries no information (the project's own IPoolable.cs:5-9 omits it). | 3 | Interfaces/IDamageable.cs, Interfaces/IRemoveable.cs | Assets/Scripts/Interfaces/IDamageable.cs:3 - public void TakeDamage(int damage); -> void TakeDamage(int damage); Assets/Scripts/Interfaces/IDamageable.cs:4 - public void Die(); -> void Die(); Assets/Scripts/Interfaces/IRemoveable.cs:3 - public void Remove(); -> void Remove(); (three files totalling 18 lines carry two… |
| No dead code: no unused usings, no field that is written and never read. | 3 | Ships/Enemy/EnemyCollision.cs, Ships/Player/PlayerShooter.cs | Assets/Scripts/Ships/Enemy/EnemyCollision.cs:1-2 - using System.Collections; using System.Collections.Generic; neither namespace is used anywhere in the 17-line file -> keep only `using UnityEngine;`; these are the Unity script-template defaults and the only unused usings in the project Assets/Scripts/Ships/Player/Pla… |
| A value used inside a single method is a local with an explicit type, not a field; a field is state that must survive between calls (exemplar: PlacesMover.cs:8-10 keeps _places/_placePosition/_index… | 2 | Ships/Player/PlayerMover.cs, Audio/MusicPlayer.cs | Assets/Scripts/Ships/Player/PlayerMover.cs:19 - private float _verticalSpeed; written at :37 and read at :38, both inside Update, and touched nowhere else -> delete line 19 and write float verticalSpeed = Mathf.Clamp(_rigidbody2D.linearVelocity.y, -_maxVerticalSpeed, _maxVerticalSpeed); inside Update Assets/Scripts/Au… |
| Configuration numbers are named constants, not bare literals (the project already does this in six classes). | 2 | Pools/SpawnPool.cs | Assets/Scripts/Pools/SpawnPool.cs:13 - public SpawnPool(T prefab, int capacity = 10, int maxSize = 100, Action<T> created = null) -> private const int DefaultCapacity = 10; private const int DefaultMaxSize = 100; used as the defaults Every pool in the game silently inherits those two unexplained numbers (EnemySpawner.… |
| Files of the same kind live in the same folder. | 2 | ScoreViewer.cs, Pools/IPoolable.cs | Assets/Scripts/ScoreViewer.cs:1 - a HUD viewer sitting at the Scripts root while its twin, Assets/Scripts/UI/LevelViewer.cs, sits in UI/ (the two classes are 22 and 23 lines of the same shape) -> move ScoreViewer.cs into Scripts/UI/ Assets/Scripts/Pools/IPoolable.cs:1 - one of the project's three interfaces lives in P… |
| A member that only hands back a stored reference, with no work and no parameters, is a property - not a Get-method (the project's own convention: 17 properties do exactly this). | 1 | Ships/Enemy/EnemyCollision.cs (call site Ships/Player/PlayerCollisionHandler.cs:45) | Assets/Scripts/Ships/Enemy/EnemyCollision.cs:14 - public EnemyShip GetShip() { return _enemyShip; } -> public EnemyShip Ship => _enemyShip; placed after the serialized field at :7, matching Ammo.cs:17-18, EnemyShip.cs:20-21, EnemyShooter.cs:16, Sound.cs:10-11, VolumeSettings.cs:20-22 Assets/Scripts/Ships/Player/Player… |

**Правила, которые проект уже соблюдает** (проверены, нарушений нет):

- Explicit types everywhere - `var` appears 0 times in all 2341 lines (exemplar BulletShooter.cs:22 `Vector3 direction = ...`, PlacesMover.cs:32 `Vector3 nextPosition;`).
- Zero comments - no `//`, `/* */` or XML docs in any of the 42 files, exactly as the exemplar.
- No public fields anywhere - every field is private, and state is exposed through 17 properties (Ammo.cs:15-18, Health.cs:6-7, VolumeSettings.cs:20-22, EnemyShip.cs:18-21, Sound.cs:10-11, KillCounter.cs:9).
- [SerializeField] private fields in _camelCase - every serialized field in all 42 files, with no exceptions.
- Constants are PascalCase and grouped at the very top of the class: Game.cs:7-9, VolumeSettings.cs:6-13, PlayerCollisionHandler.cs:6-8, PlayerShip.cs:8-9, ExplosionAnimation.cs:7, ExpandingFade.cs:7.
- Single-statement if/for/foreach/while bodies are written without braces - 42 sites across the project, matching PlacesMover.cs:16-17 and :26-27. Every braced block in the project has two or more statements (the one arguable case, PlayerMover.cs:78, is the if-arm of an if/else whose else-arm has two statements).
- WaitForSeconds is cached above the loop in all three coroutine loops: EnemySpawner.cs:91, AsteroidSpawner.cs:47, EnemyShooter.cs:47 - the exact shape of BulletShooter.cs:18/28.
- Every OnEnable subscription has its matching OnDisable unsubscription, in the same order: Game.cs:33-73 (11 pairs), PlayerShip.cs:35-53 (6), EnemyShip.cs:34-44 (2), KillCounter.cs:18-26, plus all five HUD viewers and all four screens.
- Boolean fields read is/has - all nine: _isLevelOver, _isRunning, _isPaused, _isSettingsFromPause (Game.cs:26-29), _isRestarting (EnemySpawner.cs:22), _isOnCooldown (PlayerShooter.cs:15), _isControlEnabled, _isDead (PlayerShip.cs:23-24), _isDead (EnemyShip.cs:16).
- Events are participle or past-tense names raised through `?.Invoke`, never On-prefixed: Died, Released, Changed, Retired, HitDetected, WaveCleared, LevelChanged, RestartRequested, ResumeRequested, SettingsRequested, StartRequested, CloseRequested, ReadinessChanged, PauseToggled, Finished, AsteroidBumped, EnemyBumped, Jumped, Shot.
- One type per file and the file name always matches the type name - 42 of 42.
- Four-space indentation, Allman braces, no tabs and no off-grid indentation anywhere in the project.
- Interfaces are I-prefixed and single-purpose: IPoolable (IPoolable.cs), IDamageable (IDamageable.cs), IRemoveable (IRemoveable.cs) - only the spelling of the third is wrong, not its shape.
- Tuning values live in named [SerializeField] fields rather than as literals inside methods: Asteroid.cs:8-11, EnemyMover.cs:6-7, AsteroidSpawner.cs:9-13, WarpJump.cs:13-20, ExpandingFade.cs:9-13, SmoothHealthBar.cs:9-11.
- Compiles clean: `dotnet build` against Assets/Scripts reports 0 warnings and 0 errors.

---

## 5. Рекомендации

> **Это не находки.** Доказанного дефекта здесь нет — ни сценария воспроизведения, ни нарушенного правила. Раздел отделён намеренно: пункты ниже можно игнорировать без последствий для корректности.

**Р1. Ammo.OnSpawn is empty while the Enemy Bullet prefab carries three playOnAwake particle systems, two of them looping**

`Assets/Scripts/Ammo/Ammo.cs:32`

Почему всплыло: This is the recommendation section, so: no proven symptom. What is proven is that the codebase established a reset convention for pooled particle systems and applied it to two of the three poolables that have them, and Ammo is the one that skipped it. The bullet is also the case where the omission is least obviously safe, because two of its three systems loop and are therefore never self-terminating.

Что предлагается: Cache the child particle systems in Ammo.Awake alongside _bulletMover, and have OnSpawn clear them the same way ExplosionAnimation does - Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear) over the cached array - so a reused bullet starts its trail from empty. Leave OnDespawn as it is; SetActive(false) follows it immediately.

**Р2. CalculateDuration treats a looping particle system as if it ended, so the enemy-bullet hit effect's release time is arbitrary**

`Assets/Scripts/Ammo/ExplosionAnimation.cs:74`

Почему всплыло: Recommendation, not a defect: I verified there is no pool leak here (the object is always released after a finite wait and always cleaned up in OnDespawn), so the only cost is that the release time is meaningless for looping content and would silently change if the VFX were re-authored. The same structural weakness is what produces the 16 s stall reported separately, so the two share a fix.

Что предлагается: Give ExplosionAnimation a [SerializeField] private float _lifetime and use it for CountLifetime, keeping the computed duration only as an editor-time default or dropping it entirely. That makes every pooled effect's hold time an explicit, inspectable number per prefab instead of an inference from particle authoring data that is wrong for looping systems and misleading for delayed ones.

**Р3. Die releases the ship before announcing the kill, so the whole win-level chain runs before KillCounter records it**

`Assets/Scripts/Ships/Enemy/EnemyShip.cs:102`

Почему всплыло: Recommendation only. The order is latent coupling rather than a bug: any future reader of Kills on the WaveCleared path (a level-complete summary, a per-level bonus, persisting the score before the warp) would read a value that is one short, and the cause would be three call frames away in a pool callback.

Что предлагается: Swap lines 102 and 103 so Died?.Invoke(this) fires before Released?.Invoke(this). The kill is then counted while the ship is still the live, active object it describes, and the release - which is what triggers wave accounting and the level transition - happens last, as the final act of Die().

**Р4. RECOMMENDATION: Restart() resets the shooter's pools but not its reload state, so the reset is incomplete by construction**

`Assets/Scripts/Ships/Player/PlayerShip.cs:162`

Почему всплыло: Not proven as a defect - flagged as a design smell only: `ReleaseAll()` is a pool-maintenance name being used as the restart hook, and it does not in fact restart the shooter. Anyone who later raises _shootRate turns this into a visible bug for free.

Что предлагается: Add a `PlayerShooter.Restart()` that stops `_shootDelayer` if non-null, sets `_isOnCooldown = false`, calls `ReleaseAll()` and pushes `ReadinessChanged?.Invoke(1f)`, and call that from PlayerShip.Restart instead of ReleaseAll.

**Р5. The warp cutscene spends its last 4.7 seconds on a screen with no ship in it**

`Assets/Scripts/Level/WarpJump.cs:85`

Почему всплыло: Tuning, not a defect. Flagged only because it compounds the 16 s defeat-screen wait: a player who loses and wins alternately spends a large fraction of the session watching particles.

Что предлагается: Either shorten the warp ParticleSystem's Start Lifetime so EffectDuration drops near ActionsDuration, or cap the tail (e.g. `Mathf.Min(TailDuration, someSerializedMaxTail)`) and let the particles finish over the first second of the next level rather than blocking the handover.

**Р6. The chosen volume is never saved - there is no PlayerPrefs (or any other persistence) anywhere in the project**

`Assets/Scripts/Audio/VolumeSettings.cs:24`

Почему всплыло: Marked recommendation, not defect, deliberately: I found no task text or spec in the repo stating that volume must survive a restart, so I cannot prove a requirement was violated - only that no persistence exists. Everything else on this axis checks out, which is why it stands out. If the assignment does ask for saved settings, this becomes a blocker and should be re-rated.

Что предлагается: In each setter, after Apply(...), write PlayerPrefs.SetFloat with a const key per channel, and in Awake (before Start applies them) read them back with PlayerPrefs.GetFloat(key, defaultPercent). Keys belong next to the existing MasterParameter/MusicParameter/EffectsParameter consts so the string literals stay in one place. Note SettingsScreen.Show (SettingsScreen.cs:34-36) already re-syncs the sliders from _volume.Master/Music/Effects, so the loaded values would show up correctly with no UI changes.

**Р7. Recommendation: the 'spawn a bullet and launch it' body is duplicated between PlayerShooter and EnemyShooter, and neither is reusable for a second weapon**

`Assets/Scripts/Ships/Player/PlayerShooter.cs:31`

Почему всплыло: Recommendation, not a finding: the duplication is four statements and the extension need is hypothetical for this homework. Flagged only because the OCP question was asked explicitly — the honest answer is 'adding a weapon requires editing PlayerShooter'.

Что предлагается: If a second weapon is ever wanted, extract the four shared statements into a small `AmmoLauncher` component (`Launch(Transform shootPoint)`, owning the ammo and hit-effect pools and the audio source) that both PlayerShooter and EnemyShooter delegate to; the enemy's on-screen audio guard becomes a serialized bool on that component. PlayerShooter then keeps only the cooldown and its ReadinessChanged event, and a second weapon is a second AmmoLauncher with a different prefab. Do not do this while there is one weapon — it would add a class for no present gain.

**Р8. Recommendation: four classes hand-roll the same elapsed-time loop, and WarpJump already contains the extracted version**

`Assets/Scripts/Level/WarpJump.cs:136`

Почему всплыло: Recommendation only. The duplication is a three-line idiom, and the four users live in unrelated folders, so extracting it buys consistency rather than correctness. Noted because WarpJump.Progress shows the author already found the abstraction and then did not reuse it.

Что предлагается: If consolidated, move Progress into a small static helper (for example alongside ScreenArea.cs, which is already a static utility in this project) as `public static IEnumerable<float> Over(float duration)`, and have PlayerShooter.Reload, SmoothHealthBar.Fall and ExpandingFade.Expand iterate it. Each call site keeps its own body; only the loop header changes. Low value — treat as optional tidying, not a grading concern.

**Р9. _rigidbody2D repeats the type in the name**

`Assets/Scripts/Ammo/BulletMover.cs:8`

Почему всплыло: Taste, flagged as a recommendation rather than a finding.

Что предлагается: If the author wants it uniform with _animator/_renderer, rename to _rigidbody in all four files.

**Р10. HUD class suffixes mix Label, Viewer and Bar**

`Assets/Scripts/UI/HealthLabel.cs:4`

Почему всплыло: Taste, flagged as a recommendation.

Что предлагается: Use one suffix for text readouts: HealthViewer/LevelViewer/ScoreViewer, or HealthLabel/LevelLabel/ScoreLabel.

**Р11. Five asteroid prefabs each carry their own copy of the same tuning numbers**

`Assets/Prefabs/Asteroids/Asteroid 1.prefab:190`

Почему всплыло: Not presented as a rule violation - hence kind "recommendation". The duplication is a fact of the asset files; whether it is worth factoring out at this size is a judgement call, and the smallest fix below is an editor operation rather than new architecture.

Что предлагается: Cheapest and most idiomatic: make Asteroid 2-5 Prefab Variants of Asteroid 1, overriding only the SpriteRenderer sprite and the PolygonCollider2D points, so the tuning lives once in the base prefab. If the numbers should be authorable outside the prefabs, a small [CreateAssetMenu] AsteroidSettings ScriptableObject holding speed range, rotation range and damage, referenced by one field on Asteroid, fits a project this size. A third option with no new asset types at all: move the four range fields and Damage onto AsteroidSpawner and pass them into Asteroid on spawn, changing OnSpawn() into an initialiser the spawner calls.

**Р12. SmoothHealthBar builds a WaitForSeconds from the fixed _damageHoldTime on every damage event**

`Assets/Scripts/UI/SmoothHealthBar.cs:89`

Почему всплыло: No proven defect — the value is fixed and the allocation is avoidable, but the frequency is a few objects per life, and the parameterised signature means the fix costs more than it saves. Recorded so the axis sweep is complete rather than as something that must change.

Что предлагается: If you want zero avoidable WaitForSeconds in the project: add `private WaitForSeconds _damageHoldWait;`, assign it in Start alongside the existing SetInstantly call at line 23, and change Fall's fourth parameter from `float delay` to `WaitForSeconds delay`, yielding it when non-null and passing null from line 36. Otherwise leave it.

**Р13. Player Ship carries two AudioSources with identical settings, one holding a clip that is never played through it**

`Assets/Prefabs/Player Ship.prefab:49`

Почему всплыло: Not a defect. Two sources are a legitimate choice if you later want independent Stop or routing. The one concrete oddity is the orphan clip at L113: the AudioSource's Audio Resource field points at a bump sound that is never played through .Play(), which reads as leftover state and will mislead the next person who opens the Inspector.

Что предлагается: Either clear the Audio Resource field on AudioSource #1 (Player Ship.prefab:113 -> {fileID: 0}) so both sources look like the PlayOneShot-only sources they are, or collapse to a single AudioSource and point both PlayerShooter._shotAudioSource and PlayerShipAudio._audioSource at it.

**Р14. Scene root 'Sound player' is an empty GameObject nothing references**

`Assets/Scenes/SampleScene.unity:9623`

Почему всплыло: Not a defect, just leftover scene clutter - the audio work ended up on the 'Music' GameObject (scene L7787, AudioSource + MusicPlayer) and on per-prefab sources, leaving this placeholder behind.

Что предлагается: Delete the 'Sound player' GameObject from SampleScene.

**Р15. ExpandingFade on 'Warp flash' starts at _startStrength 5, outside the Range(0, 4) its shader declares**

`Assets/Scenes/SampleScene.unity:5818`

Почему всплыло: Not a defect - the shader math does not break at 5. It is worth flagging only because the Inspector clamps the material's own slider to 4, so what you see when you tweak WarpFlashMaterial by hand will never match what ExpandingFade pushes at runtime, which makes the effect hard to tune.

Что предлагается: Either lower the scene value at SampleScene.unity:5818 to 4 or below, or widen the shader declaration to Range(0, 5) (or a plain Float) so the authored maximum and the runtime value agree.

---

## 6. План исправлений

Партии упорядочены: те, на которые опираются последующие, идут первыми. Партии, трогающие один файл, нельзя делать параллельно.

### 1. Pause, settings and end-of-run state machine

**Находки:** #2, #1, #12

**Почему в этом месте плана.** Highest severity in the set and the foundation three later batches stand on. Do findings 2, 1 and 12 in one sitting because they are one state machine, and fixing any one alone leaves the others reachable: clearing _isRunning in LoseLevel AND WinLevel (finding 12, plus the verifier's correction that WinLevel has the identical hole for the warp cutscene) by itself closes both cutscene repros in finding 1; gating Escape while the settings panel is open and routing CloseSettings' from-pause branch back through Pause() closes finding 2; capturing PlayerShip's input state in Pause() and reapplying it in Resume() instead of calling UnlockInput() unconditionally closes the residue of finding 1. This must land before batches 11 and 13, which move exactly these methods into new classes - refactoring first would mean re-deriving these fixes three times.

**Трогает:** Assets/Scripts/Game.cs (TogglePause, Pause, Resume, OpenSettingsFromStart/FromPause, CloseSettings, LoseLevel, WinLevel, ShowGameOver), Assets/Scripts/Ships/Player/PlayerShip.cs (one read-only IsControlEnabled property)

### 2. Canvas sibling order for the defeat screen ⚙️ **требует редактора Unity**

**Находки:** #26

**Почему в этом месте плана.** The scene-side half of batch 1's symptom, and the only batch that is pure editor work with zero code. It touches no file any other batch touches, so it can run in parallel with batch 1, but it should land before anyone play-tests the death and defeat paths from batches 1, 3 and 4 - otherwise the defeat screen is still drawn under the HUD and under any panel left open, and testers will misread the result. Cheap, reversible, and it removes the click-blocking Settings backdrop over the Restart button.

**Трогает:** Assets/Scenes/SampleScene.unity (Canvas children only)

**Работа в редакторе:** In the Hierarchy, reorder the eight Canvas children to: Text (TMP), Health Bar, Level, Shoot Cooldown, Start Menu, Pause, Settings, Game Over. The single required move is dragging 'Game Over' to the bottom (right-click > Transform > Set As Last Sibling). No script edits, no serialized field changes. Verify afterwards that the defeat screen still disappears on Restart, since it is now the topmost panel.

### 3. Death cutscene length ⚙️ **требует редактора Unity**

**Находки:** #3, #19

**Почему в этом месте плана.** Findings 3 and 19 must be one sitting because they fight each other, and the order inside matters. I measured Assets/Sounds/Player explosion.wav at exactly 10.0 s: once finding 19 folds _audioSource.clip.length into CalculateDuration, 10.0 s becomes the new floor for that prefab's Duration, so applying 19 after 3 in separate sittings would leave the defeat screen withheld 10 s instead of 16 s and silently undo most of finding 3's gain. Sequence: (a) retune the particle prefab; (b) add the audio term to CalculateDuration, guarded for null source and null clip, which fixes the real defect there - the enemy explosion and rocket hit being cut at 1.3 s / 1.6 s against a 1.849 s clip; (c) decouple the cutscene from the effect by giving PlayerShip its own serialized death-cutscene duration that Game.ShowGameOver waits on, leaving ExplosionAnimation.Duration as the pooled-lifetime number. Comes after batch 1 because it edits Game.cs and PlayerShip.cs.

**Трогает:** Assets/VFX/Sparks/Player ship explosion.prefab, Assets/Scripts/Ammo/ExplosionAnimation.cs (CalculateDuration), Assets/Scripts/Ships/Player/PlayerShip.cs (DeathDuration), Assets/Scripts/Game.cs (ShowGameOver)

**Работа в редакторе:** Open Assets/VFX/Sparks/Player ship explosion.prefab and set Start Delay to 0 on the four children that carry one: 'Sparks ellipse' (6), 'Explosion' (4), 'Sparks' (4.5), 'Big flash' (4.5). Then bring 'Sparks ellipse' Duration (5) and Start Lifetime (5) down to roughly 1-1.5 s so the computed span matches what is actually visible - its only emission is a single burst at t=0. Re-check the explosion in Play mode afterwards: this changes how the death VFX looks, and it also shortens how long the pooled instance stays spawned, which is the intent.

### 4. Death is not a clean stop

**Находки:** #13, #14

**Почему в этом месте плана.** With the timing fixed, this closes the two remaining wrong-state symptoms of dying. Finding 13: drain Health inside Die() so the HUD does not read full health next to a defeat screen - set _isDead = true before the TakeDamage call so the nested Died re-entry returns at the guard. Finding 14: give both spawners a stop-and-release (the same block StartWave already runs at EnemySpawner.cs:42-50, inside the _isRestarting guard so released ships do not push _retiredCount to _waveSize and fire WaveCleared) and call it from LoseLevel; call it from WinLevel too, since the verifier confirmed the same leftovers fly through the warp cutscene. Strictly after batches 1 and 3 - all three edit Game.cs and PlayerShip.cs.

**Трогает:** Assets/Scripts/Ships/Player/PlayerShip.cs (Die), Assets/Scripts/Game.cs (LoseLevel, WinLevel), Assets/Scripts/Ships/Enemy/EnemySpawner.cs (new Clear/StopLevel), Assets/Scripts/Environment/AsteroidSpawner.cs (same)

### 5. World bounds versus the camera ⚙️ **требует редактора Unity**

**Находки:** #16, #27, #17

**Почему в этом месте плана.** One coherent pass over where the playfield ends, so the boundary gets one round of play-testing instead of three. Findings 16, 27 and 17 are the same class of arithmetic error on three different edges: the kill triggers 4.5 units below the view (invisible, silent death), the enemy spawn X 0.3 units inside the right edge on 16:9 and wider, and wave-clear gated on a cleanup trigger 5 units past the left edge. Order inside the batch: 16 first (pure scene, no code), then 27, then 17. Must precede batch 10, which restructures EnemySpawner's serialized fields - doing 10 first would mean re-assigning the spawner twice.

**Трогает:** Assets/Scenes/SampleScene.unity (ObjectRemover top/bottom transforms, EnemySpawner fields), Assets/Scripts/ScreenArea.cs (new edge helper), Assets/Scripts/Ships/Enemy/EnemySpawner.cs, Assets/Scripts/Ships/Enemy/EnemyShip.cs

**Работа в редакторе:** Move ObjectRemover top and ObjectRemover bottom to localPosition y = +6.5 / -6.5, keeping the 1-unit thickness and the 31-wide scale (6.5 is the tightest value that still clears the asteroid lanes, whose centres reach y = +/-5.3 and whose rotated extent reaches 6.24). Leave left/right where they are as the wide cleanup frame. After the 27 code change, the _spawnPositionX slot on the EnemySpawner component is replaced by a serialized margin - set it to about 1. Do not do the cheap alternative of just typing 9.3 into _spawnPositionX: that only fixes 16:9.

### 6. Pooled instance reset and pool allocation

**Находки:** #4, #25

**Почему в этом месте плана.** Two isolated files that no other batch in this plan touches, so this can be slotted in at any point, including in parallel with any other batch - it is the one batch with no ordering constraint at all. Finding 4: move the speed draw out of Awake into Fly(), whose only caller is EnemyShip.OnSpawn, so a reused enemy re-rolls per spawn the way Asteroid.OnSpawn already does; Awake keeps only GetComponent<Rigidbody2D>(). Finding 25: add a readonly Action<IPoolable> assigned once in the SpawnPool constructor, next to the four delegates it already builds, and use it for both the += at line 73 and the -= at line 84.

**Трогает:** Assets/Scripts/Ships/Enemy/EnemyMover.cs, Assets/Scripts/Pools/SpawnPool.cs

### 7. PlayerMover initialization and tuning

**Находки:** #15, #18

**Почему в этом месте плана.** Removes the inter-GameObject Start-order dependency (finding 15) while the file is open for the _pushSpeed tidy-up (finding 18). Move the two Quaternion.Euler assignments into PlayerMover.Awake, drop ReturnToStart() from PlayerMover.Start so PlayerMover has no Start at all, and have Game.Start own the initial placement by calling _playerShip.ReturnToStart() right after DisableControl(). For 18, take the simplification route - delete _pushSpeed and push at _maxVerticalSpeed - so the code says what the clamp at PlayerMover.cs:37-38 actually does; the stale _pushForce YAML in the prefab is then ignored and needs no editor visit. Comes after batches 1, 3 and 4 because it edits Game.Start in a file all three have already changed.

**Трогает:** Assets/Scripts/Ships/Player/PlayerMover.cs, Assets/Scripts/Game.cs (Start)

### 8. Cutscene teardown and ownership ⚙️ **требует редактора Unity**

**Находки:** #5, #10, #6

**Почему в этом месте плана.** Three findings that all live in WarpJump and are one tidy-up of the same class, so they share a single re-test of the warp cutscene. Finding 5: add a public Stop() to ExpandingFade that stops _playing, nulls it and calls the existing private Hide(), have Restart() reuse that same path, and call it for both _flash and _shockWave from WarpJump.Stop() next to StopWarpEffect(). Finding 10: replace WarpJump's duplicate SpriteRenderer reference with a Hide() command on PlayerShip, called exactly the way WarpJump.cs:66 already calls DisableControl(). Finding 6/30: delete the dead public Duration property, keeping EffectDuration, ActionsDuration and TailDuration. After batches 1, 3 and 4 because it adds a method to PlayerShip.

**Трогает:** Assets/Scripts/Level/WarpJump.cs, Assets/Scripts/Level/ExpandingFade.cs, Assets/Scripts/Ships/Player/PlayerShip.cs (new Hide), Assets/Scenes/SampleScene.unity (WarpJump component)

**Работа в редакторе:** After deleting WarpJump's _shipSprite field, open SampleScene.unity so the now-orphaned reference at the WarpJump component is dropped on the next scene save, and confirm the WarpJump inspector still shows _playerShip assigned. Save the scene in the same commit as the code change so nobody reopens Unity with dangling YAML.

### 9. Dead state and access levels

**Находки:** #9, #24, #7, #23

**Почему в этом месте плана.** One compiler-verified cleanliness pass, all of it deleting or narrowing, run after every behavioural batch has finished moving these files so the greps it rests on are still true. Finding 9: delete PlayerShooter._shootDelayer and its two writes, leaving a bare StartCoroutine(Reload()) - this matches the exemplar, which starts its coroutine without storing a handle; do not add the ResetCooldown variant, the verifiers showed there is no bug to fix. Finding 24: drop [field: SerializeField] from Health.Value. Finding 7/28: delete IDamageable.cs and its two implements clauses, then narrow PlayerShip.TakeDamage, PlayerShip.Die and EnemyShip.TakeDamage to private, leaving EnemyShip.Die public for EnemyCollision.cs:11. Finding 23/29: narrow PlayerShip.EnableControl to private - see the conflict note about ReturnToStart.

**Трогает:** Assets/Scripts/Ships/Player/PlayerShooter.cs, Assets/Scripts/Health/Health.cs, Assets/Scripts/Interfaces/IDamageable.cs (+ .meta, deleted), Assets/Scripts/Ships/Player/PlayerShip.cs, Assets/Scripts/Ships/Enemy/EnemyShip.cs

### 10. EnemySpawner prefab references ⚙️ **требует редактора Unity**

**Находки:** #8, #21

**Почему в этом месте плана.** Structural, no behaviour change, so it comes after every behavioural fix to the same files (batch 5 edits EnemySpawner.cs, batch 9 edits EnemyShip.cs). Take only the verifiers' surviving route: keep all five pools on the spawner - per-shooter pools would be one ammo pool per pooled ship and would leave StartWave's central ReleaseAll with nothing to clear, leaking enemy bullets into the next level - and replace the reach-through chain at EnemySpawner.cs:29-34 with the spawner's own [SerializeField] prefab fields. Then delete EnemyShip.ExplosionPrefab, EnemyShip.DeadBodyPrefab and EnemyShooter.AmmoPrefab with their backing fields, and shrink EnemyShip.Init to the two pools it actually stores. Ammo.HitEffectPrefab stays public - PlayerShooter.cs:22 is a second, legitimate reader.

**Трогает:** Assets/Scripts/Ships/Enemy/EnemySpawner.cs, Assets/Scripts/Ships/Enemy/EnemyShip.cs, Assets/Scripts/Ships/Enemy/EnemyShooter.cs, Assets/Scenes/SampleScene.unity (EnemySpawner component)

**Работа в редакторе:** Assign the four new prefab slots on the EnemySpawner component in SampleScene.unity: the enemy explosion (the same ExplosionAnimation the Enemy Ship prefab referenced), Enemy Ship dead body, Enemy Bullet, and the bullet's hit effect. All four must be assigned or the pools construct on null in Awake and the first enemy spawn throws. Verify by clearing one wave: explosion, dead body, enemy bullets and bullet hits must all still appear.

### 11. PlayerShip decomposition ⚙️ **требует редактора Unity**

**Находки:** #22

**Почему в этом месте плана.** The riskiest structural change and the one most dependent on everything before it, so it runs only after every behavioural fix to PlayerShip has landed (batches 1, 3, 4, 7, 8, 9) and the split becomes mechanical. Extract only the two things the verifiers upheld: input routing plus its gate (into a PlayerInputRouter), and the explosion pool with its two constants (into a ShipExplosion component that EnemyShip can reuse - it does the identical thing). Keep TakeDamage, Die, Restart and Died on PlayerShip; they are the facade. Critically, do NOT collapse LockInput/UnlockInput into EnableControl/DisableControl as the finding's fix sketch proposes: the verifier showed that makes Resume() re-enable the collider and the mover on a dead ship, which is strictly worse than the bug batch 1 fixed. The class drops from 179 to roughly 140 lines, not 90.

**Трогает:** Assets/Scripts/Ships/Player/PlayerShip.cs, two new scripts under Assets/Scripts/Ships/Player/, Assets/Prefabs/Player Ship.prefab, Assets/Scripts/Game.cs (DeathDuration consumer), Assets/Scripts/Level/WarpJump.cs (control calls)

**Работа в редакторе:** Add the two new components to the Player Ship prefab root next to PlayerShip and assign their serialized references (the input asset, the explosion prefab, pool sizes). Because the ship is a prefab instance in the scene, apply the changes to the prefab, not the instance. Re-test in this order: play, take an asteroid hit, take an enemy bullet, die, restart - confirming health, sprite, scale, position, the reload bar and the absence of leftover bullets.

### 12. Bump dispatch through one interface ⚙️ **требует редактора Unity**

**Находки:** #20

**Почему в этом месте плана.** Lowest value of the structural batches and the one the verifiers cut hardest - two of three called it recommendation-grade duplication over about fourteen lines rather than an OCP failure - so it is last among the code refactors and is genuinely optional. If it is done, the interface needs a Bump() member (a no-op on Asteroid) or ramming stops killing the enemy, since PlayerCollisionHandler.cs:47 calls EnemyCollision.Bump() which is the only thing that calls EnemyShip.Die() on the ram path; and it must expose both asteroid gravity values, because -2 from above and +4 from below are different magnitudes, not one magnitude with a sign. After batch 11 because both edit PlayerShip's collision handlers.

**Трогает:** Assets/Scripts/Ships/Player/PlayerCollisionHandler.cs, Assets/Scripts/Ships/Player/PlayerShip.cs, Assets/Scripts/Ships/Player/PlayerShipAudio.cs, Assets/Scripts/Environment/Asteroid.cs, Assets/Scripts/Ships/Enemy/EnemyCollision.cs, new Assets/Scripts/Interfaces/IBumpable.cs, Assets/Prefabs/Asteroids/Asteroid 1-5.prefab, Assets/Prefabs/Enemy Ship.prefab

**Работа в редакторе:** The two bump Sounds currently live on the PlayerShipAudio component in the scene. Moving them behind the interface means re-assigning clip and volume on six prefabs - Asteroid 1 through 5 and Enemy Ship - plus the per-type bump gravity on each. Miss one and that hazard's bump goes silent with no compile error, so tick them off one at a time and ram each asteroid variant once.

### 13. Game decomposition ⚙️ **требует редактора Unity**

**Находки:** #11

**Почему в этом месте плана.** Genuinely last. Every earlier Game.cs batch (1, 3, 4, 7) rewrites the exact methods this split moves, so splitting first would mean re-deriving those fixes inside three new classes and re-testing each. Do it as three separate commits - screens, then pause, then levels - with a full start > play > pause > settings > win > warp > next level > die > restart pass after each, because the eleven event subscriptions in OnEnable/OnDisable must be redistributed without dropping one. Two verifier notes to carry in: file it as SRP only, since nothing in the evidence argues an open/closed violation; and do not name the screen base type Screen - a global-namespace Screen shadows UnityEngine.Screen in every file with using UnityEngine, which is a live trap the moment anyone writes Screen.width. Also move _level's write at Game.cs:118 and read at :149 with the level component, not just GetWaveSize.

**Трогает:** Assets/Scripts/Game.cs, three new scripts under Assets/Scripts/, Assets/Scripts/UI/LevelViewer.cs, Assets/Scenes/SampleScene.unity (Game GameObject)

**Работа в редакторе:** Add the three new components to the Game GameObject and redistribute its serialized references by drag-and-drop (screens to the navigator, input to the pause controller, wave-size fields to the level component); Game keeps the gameplay references. Re-point LevelViewer's serialized target from Game to the new level component. Every screen button in the scene stays wired to its own Screen script's event field, so no UnityEvent rewiring is needed - verify each button once anyway.

### Конфликты между партиями

Project root: /Users/baronkinvitaliy/Unity/UnityHomeWorks/Flappy Terminator. Paths below are Assets-relative. Note the digest miscites several: EnemySpawner.cs is Assets/Scripts/Ships/Enemy/EnemySpawner.cs (not Level/), AsteroidSpawner.cs is Assets/Scripts/Environment/AsteroidSpawner.cs (not Level/), SpawnPool.cs is Assets/Scripts/Pools/SpawnPool.cs, ExplosionAnimation.cs is Assets/Scripts/Ammo/ExplosionAnimation.cs, WarpJump.cs and ExpandingFade.cs are Assets/Scripts/Level/.

SAME-FILE CHAINS (must be sequential, never parallel):
- Game.cs: 1 -> 3 -> 4 -> 7 -> 11 -> 13. Five of the thirteen batches edit it; batch 13 dissolves it, so it is last.
- PlayerShip.cs: 1 -> 3 -> 4 -> 8 -> 9 -> 11 -> 12. The most contended file in the plan.
- EnemySpawner.cs: 4 -> 5 -> 10.
- EnemyShip.cs: 5 -> 9 -> 10.
- PlayerMover.cs: only batch 7. ExplosionAnimation.cs: only batch 3. WarpJump.cs: 8 -> 11 (batch 11 touches only its control calls).
- SampleScene.unity: 2 -> 5 -> 8 -> 10 -> 13. Unity rewrites the whole scene file on save, so two people must never have it open for two different batches at once, and each scene-touching batch must be committed with its code change, not before or after it.
- Prefabs: batch 3 owns Player ship explosion.prefab; batch 11 owns Player Ship.prefab; batch 12 owns Asteroid 1-5 and Enemy Ship. No overlap between them.

FULLY PARALLEL-SAFE: batch 6 (EnemyMover.cs, SpawnPool.cs) shares no file with anything and can be done at any point by anyone. Batch 2 shares no file with batch 1 and can be done alongside it, though it should land before the death paths are play-tested.

CROSS-BATCH CONTRADICTIONS TO DECIDE ONCE:
1. Batch 7 vs batch 9. Batch 7's fix has Game.Start call _playerShip.ReturnToStart(), which gives that method a real external caller - directly contradicting finding 23/29's instruction to narrow it to private. Pick one: either keep ReturnToStart public (batch 7's route) and narrow only EnableControl in batch 9, or keep the initial placement inside PlayerShip behind a method Game already calls and narrow both. Do not apply both as written or batch 9 breaks the build batch 7 just fixed.
2. Batch 3 internal ordering. Finding 19 applied without finding 3's decoupling step leaves DeathDuration at 10.0 s (the measured length of Player explosion.wav), not the 1.5-5 s the retuned particles would give. The audio term and the serialized cutscene duration must ship together.
3. Batch 4 vs batch 10. Batch 4 adds a Clear/StopLevel to EnemySpawner that must run inside the existing _isRestarting guard; batch 10 then rewrites how that class gets its prefabs. Batch 4 first, so the guard's behaviour is settled before the fields move.
4. Batches 11, 12 and 13 are the contested structural trio and are all optional relative to the graded defects. If time is short, stop after batch 10: every proven misbehaviour in the digest is closed by then, and batches 11-13 change no observable behaviour.

SERIALIZED-DATA HAZARDS: batch 8 removes WarpJump._shipSprite, batch 9 removes Health.Value's serialization, batch 10 adds four fields on EnemySpawner and removes three prefab getters, batch 7 orphans _pushForce. Each of those makes the scene or a prefab disagree with the code until Unity re-saves it, so commit the code edit and the re-saved asset together, and never leave the repo in a state where a pulled script expects a field the committed scene does not carry.

---

## 7. Что осталось непокрытым

Раздел честности: здесь перечислено то, что ревью **не** установило.

**Файлы, которые никто не открыл:** таких нет — все 42 скрипта были прочитаны хотя бы одним ревьюером

### Files never opened — the list is empty, and that is not the same as "all 42 scrutinised"

Уверенность: **partial**. I cross-checked all 42 scripts under /Users/baronkinvitaliy/Unity/UnityHomeWorks/Flappy Terminator/Assets/Scripts against every confirmed finding and every reviewer note: each one is named at least once, so nothing was left wholly unmentioned. But naming is not scrutiny. Nine of the seventeen axes assert they read all 42 in full, and much of that coverage arrives as bulk scope claims ("all 8 UI scripts", "all 3 Audio scripts", "both interfaces"). Ten scripts get no individual verdict anywhere beyond a single line-cite: /Users/baronkinvitaliy/Unity/UnityHomeWorks/Flappy Terminator/Assets/Scripts/Ammo/BulletMover.cs, .../Ships/Player/PlayerShipAudio.cs, .../UI/StartScreen.cs, .../UI/PauseScreen.cs, .../UI/GameOverScreen.cs, .../UI/ShootCooldownBar.cs, .../UI/HealthLabel.cs, .../ScoreViewer.cs, .../UI/LevelViewer.cs, .../Interfaces/IRemoveable.cs. I read all ten end to end for this section, plus the other twenty scripts that carry no confirmed finding. I found nothing unreported in them — subscribe/unsubscribe pairs balance, no per-frame allocation, explicit types, no comments, no public fields — so they are clean as far as reading can establish. That verdict rests on my pass, not on any axis having adjudicated them. The tree I read is HEAD with a clean working tree (d2bbe70f); I modified nothing.

### Four reviewer notes point at findings that are absent from the confirmed set

Уверенность: **low**. The pool-lifecycle note says "see the EnemyShip.OnDespawn finding"; the Unity-runtime note references "the Asteroid finding" and "the collision-matrix finding"; the naming note lists EnemyDeadBody.End among three names that contradict their bodies and says the unused usings in EnemyCollision.cs were "reported". None of Assets/Scripts/Ships/Enemy/EnemyShip.cs, Environment/Asteroid.cs, Ships/Enemy/EnemyDeadBody.cs or Ships/Enemy/EnemyCollision.cs appears in the confirmed-finding file list. I verified the underlying code exists as described (EnemyCollision.cs:1-2 does carry two unused usings; EnemyShip.cs:65-70 does raise Retired from OnDespawn; EnemyDeadBody.End at :28 does start a lifetime countdown rather than end anything). I cannot tell whether those findings were dropped in consolidation or the notes overstate what was filed. A reader should not treat those four files as cleared.

### WaitForSeconds allocation sites — two axes returned opposite verdicts and nobody adjudicated

Уверенность: **low**. The coroutines axis judged the seven uncached `new WaitForSeconds(...)` sites to be one-shot delays outside any loop, where the exemplar's caching rule does not apply, and called them explicitly "not findings". The allocations axis called four of the same sites findings. I grepped all ten sites and both notes describe them accurately: cached before the loop at EnemySpawner.cs:91, AsteroidSpawner.cs:47, EnemyShooter.cs:47 (matching the exemplar's BulletShooter.cs:18); uncached at Game.cs:199, WarpJump.cs:74 and :85, ExplosionAnimation.cs:99, EnemyDeadBody.cs:50, BumpEffect.cs:57, SmoothHealthBar.cs:89. Whichever reading is right, EnemyDeadBody.cs, BumpEffect.cs and SmoothHealthBar.cs hold sites one axis wanted filed and no confirmed finding covers.

### PlayerMover.Push formatting — a hand-off three axes made that may have fallen through

Уверенность: **low**. The pool-lifecycle, cutscene-races and Unity-runtime axes each independently flagged the formatting of PlayerMover.Push and each deferred it to the style axis; the style axis's note does not list it. I verified every element of the claim: Assets/Scripts/Ships/Player/PlayerMover.cs:77 reads `if(transform.position.y > 0)` with no space, lines 77-86 wrap both single-statement branches in braces against the exemplar's brace-free rule, and line 80 is twelve spaces of trailing whitespace — the only trailing whitespace in the project. PlayerMover.cs does carry a confirmed finding, but the notes suggest that finding is the Start()/ReturnToStart ordering or the kinematic-Jump one, so this style violation is plausibly unreported.

### Scene YAML — genuinely read, not sampled

Уверенность: **partial**. This one holds up under testing. The serialization axis's structural figures are exact: Assets/Scenes/SampleScene.unity is 12886 lines and contains exactly 340 `--- !u!` documents, both as claimed. Every scene citation I spot-checked lands on the stated content — 8270-8279 (Game's ten serialized refs), 8765-8769 (WarpJump's five), 10070-10073 (the `_input` prefab-instance override to fileID 661464565), 739 / 6309 / 8224 / 12287 (the four inactive panel GameObjects), 6626 (`m_TagString: MainCamera`), 9780 (`_spawnDelay: 0.4`), 5906 (the stripped SpriteRenderer &366751404), 10018 (the single PrefabInstance). I also independently confirmed the scene holds exactly one Canvas component (u!223 at :9323, ScreenSpaceOverlay on GameObject &1229452742), which several findings depend on. Caveat: verifying that citations resolve proves systematic parsing, not that a human eye passed over all 12886 lines.

### Prefab YAML — the eleven gameplay prefabs were read; the three large VFX prefabs were only searched

Уверенность: **partial**. The gameplay prefabs are small and every citation into them resolves: Player Ship.prefab (591 lines), Enemy Ship.prefab (449), PlayerBulletHit (219), Asteroid 1-5 (204-209), PlayerBullet (204), Enemy Ship dead body (153). Those were genuinely read. The large ones were not: Assets/VFX/Sparks/Player ship explosion.prefab is 24589 lines, vfx_RocketExplosion.prefab 19700, Assets/Prefabs/Enemy Bullet.prefab 14727 — roughly 59,000 lines, almost all ParticleSystem module serialization. The only citations into them are targeted field lookups, and I confirmed each lands exactly on an `AudioSource:` header (Player ship explosion.prefab:4930, vfx_RocketExplosion.prefab:9817, vfx_EnemyExposion.prefab:4868) or on the stale key at Enemy Bullet.prefab:4971. The cutscene-races note is explicit that it read "all five ParticleSystem blocks" — a slice. So: searched for the fields the code touches, not reviewed.

### Prefab line-number drift in Player Ship.prefab

Уверенность: **partial**. Three citations are off by one or two lines, though their quoted values are correct: `_pushForce: 4` is at Player Ship.prefab:212 (cited as :211 by the Unity-runtime axis), `_rotationSpeed: 1` at :214 (cited as :213), and the Body-sprite `m_LocalPosition: {x: 0, y: 0, z: 0}` at :413 (cited as :411 by two axes). The data was read; only the anchors drift, so a reader jumping to a cited line lands on a neighbouring field. Every other prefab citation I checked is exact (Player Ship.prefab:246, :232, :490; PlayerBullet.prefab:187; Enemy Ship.prefab:191, :325; Enemy Ship dead body.prefab:100; PlayerBulletHit.prefab:112).

### Nothing was observed at runtime — no axis launched Unity or ran the Profiler

Уверенность: **partial**. All seventeen axes state this. Every behavioural reproduction in the review is derived from source plus serialized YAML, never from play. The figures most exposed: the ~16 s player-death explosion duration (computed from particle YAML and resting on an unverified reading of MinMaxCurve.constantMax in Constant mode, which the docs leave unspecified); the WarpJump/ExpandingFade timing window (derived from warp ParticleSystem lengthInSec 5, _stretchAtWarpProgress 0.75, _stretchDuration 0.4, _dashDuration 0.15, flash 0.3, shockwave 0.5); the aspect-ratio-dependent bounds arithmetic; and the audibility judgements, made from WAV RMS envelopes rather than from listening. No Profiler capture was taken, so every byte figure in the allocations axis is calculated from object counts and assumed Mono layout, not measured. The only build run anywhere was `dotnet build /tmp/claude-501/compile/p.csproj` (0 warnings, 0 errors), which compiles C# against editor DLLs and says nothing about a Unity player build, IL2CPP, or managed stripping.

### Six Unity semantics the official docs did not settle

Уверенность: **low**. Each was chased into the documentation and came back unanswered, so the findings resting on them are soft or were withheld entirely. (1) Whether a second OnTriggerEnter2D is still delivered within one physics step after a collider involved in that step was disabled in an earlier callback — leaves the simultaneous win/death race at "likely" rather than certain. (2) Whether a ParticleSystem's live particles survive GameObject deactivation and reappear on reuse — the Enemy Bullet particle item was downgraded to a recommendation for this reason. (3) Whether an AudioSource keeps playing after its GameObject is deactivated — affects restarting during the death explosion; no finding was filed, and the restart axis flags it as a 10-second play-mode check. (4) Awake/OnEnable/Start ordering across different GameObjects, since docs.unity3d.com/Manual/execution-order.html defers to an SVG — the initialization axis deliberately built no finding on it. (5) Whether `GetComponentInParent<TInterface>()` allocates, at ObjectRemover.cs:7, the hottest non-Update path in the game (~5 asteroids/s plus bullets) — the docs say nothing about allocation or interface arguments, so no finding. (6) Whether a re-shown UGUI panel's Button can still take a Submit from the Space key, which is also `_shootKey`.

### URP / 2D Renderer configuration — opened by nobody

Уверенность: **none**. For a project whose whole rendering stack is URP 2D, no axis opened /Users/baronkinvitaliy/Unity/UnityHomeWorks/Flappy Terminator/Assets/Settings/Renderer2D.asset or Assets/Settings/UniversalRP.asset, nor ProjectSettings/GraphicsSettings.asset or ProjectSettings/URPProjectSettings.asset. Nothing is established about renderer features, the 2D light setup, or how the two warp shaders' materials behave under the configured pipeline. ProjectSettings/TagManager.asset was read for its physics layers (Player 6, PlayerAmmo 7, Enemy 8, EnemyAmmo 9) but its m_SortingLayers section was never examined, so 2D draw ordering is unverified.

### UI scaling and resolution behaviour

Уверенность: **none**. No axis mentions the CanvasScaler. The scene's single Canvas carries one at SampleScene.unity:9312 with `m_UiScaleMode: 0` (Constant Pixel Size) and `m_ReferenceResolution: {x: 800, y: 600}`, a value that mode ignores. Meanwhile the physics axis's bounds arithmetic uses the 1920x1080 default from ProjectSettings.asset:45-46. Nobody checked how the HUD, the four panels or the three volume sliders lay out at any other resolution or aspect. Stated here as an uncovered area only — I am not asserting a defect.

### ProjectSettings files and package manifest not opened

Уверенность: **none**. Seven settings assets were read across the axes (TagManager, Physics2DSettings, TimeManager, EditorSettings, QualitySettings, ProjectSettings.asset, EditorBuildSettings). Not opened by anyone: InputManager.asset — pointed, since ProjectSettings.asset:667 confirms `activeInputHandler: 0` (legacy) and every key in the game is read through Input.GetKeyDown; AudioManager.asset — pointed, since the entire audio axis rests on mixer routing, voice limits and DSP behaviour; plus GraphicsSettings, URPProjectSettings, MemorySettings, VFXManager and the rest. Packages/manifest.json was never opened either (Unity 6000.3.7f1 with URP 17.3.0, com.unity.feature.2d 2.0.2, ugui 2.0.0), so no package-version compatibility question was considered.

### Art, animation and audio assets beyond the specific fields the code reads

Уверенность: **none**. Assets/Sprites (77 files) was explicitly excluded by the serialization axis and touched by no other. Of the eleven Animator controllers and eleven .anim clips in the project, only the four actually referenced by the game's prefabs and scene were examined — I verified by GUID which ones those are: Assets/Animations/Player animation controller.controller, Assets/Animations/Enemy 1 animation controller.controller, Assets/Sprites/Ammo/Hits/Hit-5/hits-5-1 (1).controller and Assets/Sprites/Ammo/Spark/spark-preview1 (1).controller. The other seven controllers and their clips under Assets/Sprites/Ammo/** are unreferenced imports nobody audited. The 16 shaders, 4 shadergraphs and 11 materials were covered only as far as "the two warp shaders and their materials". No .meta import settings were reviewed for any asset anywhere in this review — no pixels-per-unit, filter mode, compression, or audio load-type/decompression check.

### Interface-member and file-hygiene conventions — no axis took a position

Уверенность: **low**. The naming axis verified var, comments, Hungarian prefixes, public fields, tag strings, file-name-to-type match and lifecycle ordering. Two things I noticed while reading for this section were adjudicated by nobody, and I record them as unresolved rather than as findings: Assets/Scripts/Interfaces/IRemoveable.cs:3 declares `public void Remove();` with an explicit accessibility modifier while Assets/Scripts/Pools/IPoolable.cs:5-9 declares its members without one; and six of the 42 files end with no trailing newline (ObjectRemover.cs, Health.cs, IRemoveable.cs, EnemyCollision.cs, PlayerMover.cs, PlayerShip.cs) while the other 36 have one. The exemplar in ../Code style genius/ is a single 30-line file with no interfaces, so it settles neither question — which is presumably why no axis ruled.

### Public-versus-private member ordering

Уверенность: **low**. The naming axis states Unity method blocks are in lifecycle order wherever they appear together, and the cutscene-races axis noticed that WarpJump.cs and PlayerShip.cs interleave public properties and events among the Unity methods and handed it to the style axis, which did not take it up. One further case nobody named: Assets/Scripts/Ships/Enemy/EnemyShip.cs declares `private void OnAmmoHit` at :82 and then `public void Die()` at :87. The exemplar's stated order (serialized fields, private fields, Unity methods, private methods) has no public members in it at all, so where public methods belong is simply undefined by the rule the graders use. The question is unresolved, not cleared.

### HUD state between scene load and the Start button

Уверенность: **low**. The event-subscription and initialization axes established that HealthLabel (Start at :14-17), SmoothHealthBar (Start at :21-24) and ShootCooldownBar (Start at :13-16) each re-seed their visual from Start, so an early Changed event cannot leave them stale — HealthLabel.Start is what corrects the authored "10 / 10" against a MaxValue of 200. Assets/Scripts/ScoreViewer.cs has no Start: it seeds only from KillCounter.Changed, first raised by KillCounter.Restart (:28-32) via Game.RestartGame. No axis recorded what the score label reads between scene load and the player pressing Start. Flagged as unchecked; I am not claiming it is wrong.

### Findings marked needs_editor were never validated, and neither were the serialized renames they imply

Уверенность: **low**. Several findings are marked needs_editor by their own authors because confirming or fixing them means touching SampleScene.unity or a prefab, which the read-only rule forbade. In particular, the serialized-field renames the naming axis proposes would silently drop the authored inspector values unless each rename carries [FormerlySerializedAs]; nobody checked, per field, what would be lost. The project has exactly one such migration attribute today — PlayerMover.cs:8 `[FormerlySerializedAs("_pushForce")]` — and the old key is still stored at Player Ship.prefab:212, so the pattern exists but was applied once. The blast radius of the proposed renames is unquantified.

### Опровергнутые находки

21 находок не пережили состязательную проверку и в отчёт не попали. Они перечислены ниже, чтобы было видно, что именно проверялось и почему отброшено.

| Файл:строка | Заявлялось | Почему отброшено |
|---|---|---|
| `Assets/Scripts/Ships/Player/PlayerShooter.cs:38` | Ammo's pool dependency is re-injected on every shot instead of guaranteed once through SpawnPool's created hook, so Hit/OnSpawn c… | The claimed symptom (NullReferenceException at Ammo.cs:53 via AmmoHitHandler.cs:19) cannot occur in this codebase, and the finding itself only asserts it for a hypothetical "future caller". Control flow walked end to end: grep over all 42 scripts shows exactl… |
| `Assets/Scripts/Ships/Player/PlayerMover.cs:34` | PlayerMover does all Rigidbody2D work in Update (the project has no FixedUpdate) and rotates the dynamic body through the Transfo… | The claimed symptom chain breaks at five independent links, each contradicted by lines I read. (1) The "passing through / teleported collider" symptom is structurally impossible. The player's ONLY collider is `Assets/Prefabs/Player Ship.prefab:501` PolygonCol… |
| `Assets/Scripts/Environment/Asteroid.cs:26` | Asteroid.Update() re-writes a kinematic Rigidbody2D's constant velocity every rendered frame instead of setting it once in OnSpawn | The code is quoted accurately (Asteroid.cs:26-30) but no reproduction exists and the "violated rule" is misapplied. 1) No behavioural symptom, proven line by line. Grep over Assets/Scripts shows the asteroid's velocity has exactly two writers: Asteroid.cs:29… |
| `Assets/Scripts/Ships/Player/PlayerCollisionHandler.cs:22` | PlayerCollisionHandler.OnValidate() dereferences a Collider2D the class never requires and silently rewrites that component's isT… | Every link of the reproduction fails against the actual wiring. 1) The NRE precondition does not exist and cannot be reached by playing. PlayerCollisionHandler (script guid 329c8450bf36f4ccca3c1282c69e403a) appears exactly once in the whole project — Assets/P… |
| `Assets/Scripts/Ships/Player/PlayerShip.cs:41` | PlayerShip._input is null in the prefab asset ({fileID: 0}) and dereferenced unguarded in OnEnable; only a scene-instance overrid… | The prefab-asset fact is real but the reproduction chain is broken at every link; nothing in the shipped project can reach the claimed NullReferenceException. 1) The null is real but permanently patched. Assets/Prefabs/Player Ship.prefab:246 does read `_input… |
| `Assets/Prefabs/Enemy Bullet.prefab:4971` | Field renames were never completed: prefabs still store old serialized keys (one with no owner at all) and the scripts still carr… | The facts are real but there is no defect and no violated rule; the "reproduction" is a consequence of the proposed fix, not of the code. 1. NOTHING MISBEHAVES TODAY. Walking the flow: PlayerShooter.Awake() (Assets/Scripts/Ships/Player/PlayerShooter.cs:21-22)… |
| `Assets/Scripts/Ships/Enemy/EnemyShip.cs:69` | EnemyShip raises the wave-accounting event Retired from the pool's storage hook, forcing an _isRestarting suppression flag in Ene… | The code facts are quoted accurately (EnemyShip.cs:69 raises Retired from OnDespawn; SpawnPool.cs:76-80 calls OnDespawn as actionOnRelease; EnemySpawner.cs:22/42/50/80 is the suppression flag), but neither leg of the finding survives. 1) The claimed symptom c… |
| `Assets/Scripts/Game.cs:177` | A win landing in the same physics step as the player's death discards the death and advances the level with the player revived | The chain breaks at the link the reviewer skipped: WinLevel does not leave the player killable, it synchronously disables the player's only damage collider before returning. Game.cs:194 `_warpJump.Play()` -> WarpJump.cs:48 `StartCoroutine(Jump())` -> WarpJump… |
| `Assets/Scripts/Ships/Player/PlayerMover.cs:62` | ReturnToStart writes the same position and rotation twice, once through the Rigidbody2D and once through the Transform | The load-bearing claim — "lines 62-63 are dead", i.e. one of the two writes provably has no effect — does not hold, and the primary proposed fix would introduce a real regression. 1. Line 64 is NOT redundant; it is the only write that makes the Transform corr… |
| `Assets/Scripts/Ships/Enemy/EnemyShooter.cs:55` | The enemy shot sound plays on an AudioSource that lives on the pooled enemy ship, so killing the shooter cuts its 3.75 s rocket s… | Mechanism is real but both stated reproductions collapse. (1) The "every enemy at once on a level change" path is impossible: EnemySpawner.cs:85-86 fires WaveCleared only when _retiredCount >= _waveSize, and _retiredCount is incremented from EnemyShip.OnDespa… |
| `Assets/Scripts/Ships/Enemy/EnemySpawner.cs:8` | OCP: a second enemy type cannot be added without rewriting EnemySpawner, although the sibling AsteroidSpawner already solves exac… | I read EnemySpawner.cs in full, AsteroidSpawner.cs (actual path Assets/Scripts/Environment/AsteroidSpawner.cs), EnemyShip.cs, EnemyShooter.cs, SpawnPool.cs, PlayerShooter.cs, Game.cs, the scene block at SampleScene.unity:9773-9779, `ls -R Assets/Prefabs`, and… |
| `Assets/Scripts/UI/StartScreen.cs:26` | Four screen classes duplicate the same panel/Show/Hide member set, and Game names all four concrete types in four places | Cited lines are accurate, but the SOLID framing and the extension-cost claim both fail on reading. (1) The fix does not remove the cost it is sold on. The proposed base supplies only _panel/Show/Hide — no events. Each screen's event surface is distinct and un… |
| `Assets/Scripts/Ammo/Ammo.cs:32` | ISP: IPoolable forces OnSpawn() on implementers that have nothing to do at spawn — two of six bodies are empty | Read all six IPoolable implementers, the interface, and the pool. The interface's actual contract is reset-for-reuse, not "start behaving", and the two empty bodies are exactly the two objects with nothing to reset — so the ISP premise fails, and the finding'… |
| `Assets/Scripts/Ships/Player/PlayerShip.cs:170` | LSP/naming: PlayerShip.Remove() removes nothing — it is an alias for Die(), so IRemoveable's only client (a despawner) ends the l… | Code facts check out (PlayerShip.cs:170-173 does call Die(); Game.cs:36 routes Died to LoseLevel), but both invoked rules collapse on reading. (1) "Remove() removes nothing" is false. Die() (PlayerShip.cs:140-155) calls DisableControl() (PlayerShip.cs:95-101)… |
| `Assets/Scripts/KillCounter.cs:4` | DIP: KillCounter is welded to a concrete EnemySpawner on its own GameObject via RequireComponent + GetComponent, against the proj… | Both load-bearing claims fail on lines I read. (a) "The one class that breaks the project's observer convention" is factually wrong. PlayerShip.cs:4-5 declares [RequireComponent(typeof(PlayerShooter), typeof(Health))] + [RequireComponent(typeof(PlayerMover))]… |
| `Assets/Scripts/Ships/Enemy/EnemyMover.cs:18` | EnemyMover and BulletMover are the same class apart from a sign and a randomised speed | Read EnemyMover.cs:1-27, BulletMover.cs:1-24, Asteroid.cs:1-46, PlayerMover.cs:1-89, EnemyShooter.cs:45-60, PlayerShooter.cs:31-44, SpawnPool.cs:28-69, the Enemy Ship/PlayerBullet/Enemy Bullet prefab YAML, and the graded exemplar. The quoted lines are accurat… |
| `Assets/Scripts/Ships/Enemy/EnemyShip.cs:77` | PlayerShip and EnemyShip duplicate their damage intake and death plumbing verbatim | The quoted lines are accurate, but every reason given for why the overlap is a defect fails against the code. (1) The stated cost is factually wrong. "A change to how ammo damage is applied (armour, damage types, invulnerability frames) currently has to be ma… |
| `ProjectSettings/Physics2DSettings.asset:56` | The Physics 2D layer collision matrix is left fully default, so every layer pair is simulated and the filtering is redone per cal… | The raw facts are accurate (Physics2DSettings.asset:56 is 256 hex chars all 'f'; TagManager.asset:8-17 lists the five purpose layers; AmmoHitHandler.cs:10-26 and ObjectRemover.cs:5-10 match verbatim; masks 512/128/640 and Asteroid 1.prefab:130 m_IncludeLayers… |
| `Assets/Scripts/Ammo/ExplosionAnimation.cs:74` | CalculateDuration() re-runs GetComponentsInChildren/GetComponentInChildren for components already cached in Awake | Read Assets/Scripts/Ammo/ExplosionAnimation.cs in full, plus PlayerShip.cs, SpawnPool.cs, Game.cs and the prefab YAML. The quoted lines are accurate, but the finding fails on all three of its legs and its proposed fix is a crash. (1) THE PROPOSED FIX BREAKS T… |
| `Assets/Prefabs/Asteroids/Asteroid 1.prefab:99` | All 5 asteroid prefabs use a Kinematic Rigidbody2D, so the ObjectRemover static triggers never fire and asteroids are never retur… | The chain breaks at its first link: the Unity semantics claim is wrong, because the asteroid's own collider is a TRIGGER, and Unity's kinematic contact suppression explicitly exempts triggers. What I read: 1. /Users/baronkinvitaliy/Unity/UnityHomeWorks/Flappy… |
| `Assets/Prefabs/Enemy Ship dead body.prefab:98` | The enemy 'dead body' prefab is assigned the same looping idle Animator controller as the live enemy, so EnemyDeadBody.End()'s Re… | The reproduction's load-bearing symptom cannot occur, and the "wrong controller" premise is a false dichotomy that the prefab data contradicts. 1) "The player sees a SECOND, fully alive-looking enemy ship" — impossible. EnemyShip.Die() is one synchronous bloc… |

### Непроверенные находки

Нет ни одной: все 51 кандидат прошли через скептиков и получили вердикт.

### Оценка критика полноты

Coverage of *behaviour* is genuinely strong and the YAML work is real, not asserted. I spot-checked three confirmed findings against the raw assets and they hold: the 16 s DeathDuration re-derives exactly from "Assets/VFX/Sparks/Player ship explosion.prefab" (the five systems give startDelay+duration+startLifetime of 16, 10, 9.1, 4.5 and 7.5 s — max 16, matching ExplosionAnimation.cs:74-79); EnemyMover.cs:15's once-per-instance Random.Range checks out against Enemy Ship.prefab:306-307 (_minSpeed 0.5, _maxSpeed 3); PlayerMover.cs:88's _pushSpeed 4 really is clamped to _maxVerticalSpeed 3 by Update at :37. I also opened all 21 scripts that had only a passing mention in a pairing table (Sound, MusicPlayer, ScreenArea, ObjectRemover, BulletMover, IRemoveable, IPoolable, ScoreViewer, LevelViewer, HealthLabel, ShootCooldownBar, SmoothHealthBar, ExpandingFade, the four screens, VolumeSettings, InputReader, KillCounter, Health) and found no unreported behavioural defect in any of them — they are genuinely clean, not merely unread. The three cross-axis interactions named in my brief were all actually checked: pause-during-cutscene (Game.cs:166 and :202, both confirmed), a pooled coroutine outliving a restart (SpawnPool.cs:76-80 runs OnDespawn before SetActive(false) — I re-read it), and audio on an object released mid-sound (ExplosionAnimation.cs:79, confirmed).

The failure is at the triage stage, not the reading stage. Three axes reported findings that appear in NEITHER the confirmed nor the refuted list, so they were dropped silently rather than judged: the naming axis ("the three findings whose names provably contradict their bodies"), the style/formatting observations that four separate axes each noticed and each deferred to "whoever owns the style axis", the allocations axis's four WaitForSeconds findings, and the OOP axis's "findings 4, 5, 6 ... 3, and partly 11 and 14". The result is 25 confirmed findings of which ZERO are kind:"style" — in a project whose graders' entire published delta between the ungraded and graded exemplar is a single method rename. That is the largest gap by grading weight, and it is not a matter of unread files: I verified the specific violations myself. Second, two confirmed findings rest on reasoning that another axis reviewer explicitly declined to build a repro for. Third, two pieces of scene YAML that bear directly on the confirmed findings were never opened at all: the CanvasScaler, and the spawn-entry side of the camera bounds.
