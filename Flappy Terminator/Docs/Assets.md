# Происхождение графики и звука

[← План развития](Roadmap.md)

Установлено 18.09.2026 по точному совпадению имён файлов и размеров с пакетами, скачанными
в `~/Library/Unity/Asset Store-5.x/`. Нигде в проекте это раньше не было записано.

| В проекте | Набор | Издатель |
|---|---|---|
| `Sprites/Ships/Player.png` | `tinyShip5.png` | **Tiny Space Ships**, Disruptor Art |
| `Sprites/Ships/Enemy 1…5.png` | `tinyShip12, 2, 9, 16, 18` | там же |
| `Sprites/Ships/Boss 1…5.png` | `tinyShip13, 7, 11, 14, 20` | там же |
| `2D Pixel Spaceships…/Sprites/ship1,2.png` | те же файлы | **2D Pixel Spaceships (2 Small Ships + Explosions)**, EVPO Games |
| `Sprites/Ammo/{Spark,Pulse,Waveform,Hits}` | `Pixel Art/*` | **Warped Shooting Fx**, Ansimuz |
| `Sounds/warped-shooting-fx.wav` | `Music/warped-shooting-fx.wav` | там же |
| `Sprites/Ammo/Torpedo/2DShipsMissilesTorpedoesAtlas.png` | тот же файл | **2D Spaceships Free Trial**, MSGDI |
| `Sprites/Asteroids.png` | `asteroids.png` 498×403 | **2D Pixel Asteroids**, EVPO Games |
| `GabrielAguiarProductions/` | по имени папки | **Free Quick Effects Vol 1**, Gabriel Aguiar Prod |
| `Sci-Fi UI/` | весь интерфейс | **Sci-fi GUI skin**, 3drina |
| `Unity UI Samples/` | | **UI Samples**, Unity Technologies |

Корабли — не одиночные спрайты, а листы кадров: `Player.png` содержит десять, остальные от восьми
до пятнадцати. Имена файлов означают роль, а не порядок в наборе: одиннадцать кораблей выбраны
из двадцати, что есть в наборе, и пронумерованы по месту в игре — обычные враги 1–5 в порядке
появления, боссы 1–5 по системам. Шестой босс придёт из другого набора.

`Boss 1` и `Boss 2` — не те файлы, что лежали здесь раньше. Прежний `Boss 1` (`tinyShip4`)
удалён: роли ему не нашлось. Прежний `Enemy 2` (`tinyShip13`) стал `Boss 1` — переименован
вместе с `.meta`, поэтому guid сохранился.

**`Sprites/Skybox.png` не опознан.** В наборе **Starfield Skybox** (PULSAR BYTES) шесть граней
куба по 4096×4096, а здесь одна картинка 2048×2048. Либо другой источник, либо сведено вручную.

**`Sprites/UI/progress_bar_white.png` сделан на месте** — это `progress_bar` из набора 3drina
с обнулённым цветом при сохранённой альфе. Понадобился для промежуточного урона: тонировкой
бирюзовый спрайт белым не сделать.

## Что удалено из купленного

Из `Sci-Fi UI` убраны `atlas.png` (4096×4096, **64 МБ из 66**), `Demo.unity` и `Demo_Assets` —
на них ссылалась только демо-сцена самого набора. Проверено grep по guid перед удалением.

Из набора EVPO оставлены только взрывы: два листа `ship1.png` и `ship2.png` и клипы
`Ship1Expl.anim`, `Ship2Expl.anim`. Убраны префабы, демо-сцена, четыре скрипта, контроллеры,
огонёк двигателя и `frame.png` — 472 КБ стали 192. Ссылок извне набора не было ни одной.
Первый кадр каждого листа — целый корабль, дальше одиннадцать кадров взрыва; корабли в игре
не используются, но вырезать их из листа нельзя, не сломав клипы.

## Локальный кэш

В `~/Library/Unity/Asset Store-5.x/` лежит **26 скачанных пакетов**, включая все перечисленные.
Импортировать их можно, не обращаясь к стору, — но каждый приносит с собой демо-сцены и
атласы, которые стоит вычищать сразу, как это было со `Sci-Fi UI`.
