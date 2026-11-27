# Home layout spec (from ApprovedHome.png)

TEMPORARY. Delete this file together with `ApprovedHome.png` before submission.

Source of truth for the Home visual stages. Measured from `SuperMaze_Home_Kit/Reference/ApprovedHome.png` (941x1672), scaled uniformly by 1080/941 = 1.1477 to the 1080x1920 canvas. This is exactly what `ReferenceOverlay` shows at 1080x1920. The scaled image is 1919 units tall, so it is 0.5 unit off vertically; ignore that.

## Conventions

- Units are canvas units at the 1080x1920 reference resolution. At a 1080x1920 screen there is no safe-area inset, so canvas = SafeArea.
- `cx`, `cy` = centre of the element's bounding box. **x is measured from the left edge, y from the BOTTOM edge** (Unity, y up).
- `w`, `h` = bounding box of the visible art, including outlines and baked-in shadows, but not the soft drop shadow on the floor.
- To get anchoredPosition: anchors at the bottom-left → (cx, cy). Anchors at the centre → (cx − 540, cy − 960). Anchors at the top-centre → (cx − 540, cy − 1920).
- Accuracy: about ±2 units for flat UI shapes (bars, pills, plates, icons, text), measured by pixel scans and flood fills. About ±5 units for illustrated pieces (stands, food, shelves, checkout pieces), read from 2x crops with a 10-unit grid. Edges hidden behind other art are marked "est.".

## Top bar

| Element | cx | cy | w | h | Bounds (x / y) | Notes |
| --- | ---: | ---: | ---: | ---: | --- | --- |
| TopBar (cream bar) | 539 | 1813 | 972 | 138 | 53–1025 / 1744–1882 | |
| Logo | 180 | 1829 | 156 | 160 | 102–258 / 1749–1909 | sticks up 27 above the bar |
| Separator 1 | 298 | 1821 | 4 | 64 | 296–300 / 1789–1853 | navy, rounded ends |
| Coin icon | 368 | 1818 | 78 | 78 | 329–407 / 1779–1857 | |
| Coins text ("530") | 485 | 1819 | 91 | 39 | 439–530 / 1799–1838 | glyph box, left edge 439; cap height 39 → font size ≈ 55 (est.) |
| Separator 2 | 583 | 1818 | 4 | 56 | 581–585 / 1790–1846 | |
| Heart icon | 664 | 1818 | 82 | 73 | 623–705 / 1781–1854 | |
| Lives text ("3/3") | 768 | 1818 | 81 | 45 | 727–808 / 1795–1840 | glyph box, left edge 727 |
| Separator 3 | 852 | 1818 | 4 | 56 | 850–854 / 1790–1846 | |
| Gear (Settings button) | 935 | 1818 | 98 | 95 | 886–984 / 1770–1865 | blue ring outer edge |

## Leaves (decoration, behind the top bar)

| Element | cx | cy | w | h | Bounds (x / y) | Notes |
| --- | ---: | ---: | ---: | ---: | --- | --- |
| Leaves top-left, corner cluster | 131 | 1795 | 261 | 250 | 0–261 / 1670–1920 | runs off the top and left edges |
| Leaves top-left, hanging vine | 334 | 1654 | 158 | 182 | 255–413 / 1563–1745 | hangs below the bar |
| Leaves top-right, corner cluster | 980 | 1805 | 200 | 230 | 880–1080 / 1690–1920 | runs off the top and right edges |
| Leaves top-right, hanging vine | 818 | 1653 | 155 | 183 | 740–895 / 1561–1744 | hangs below the bar |

## Shelves and plants

| Element | cx | cy | w | h | Bounds (x / y) | Notes |
| --- | ---: | ---: | ---: | ---: | --- | --- |
| Produce shelf (incl. awning) | 134 | 1258 | 268 | 955 | 0–268 / 780–1735 | cut by the left edge; bottom hidden by the flower pot (est.) |
| PRODUCE sign | 133 | 1513 | 215 | 165 | 25–240 / 1430–1595 | tilted |
| Bakery shelf (incl. awning) | 944 | 1219 | 272 | 1027 | 808–1080 / 705–1732 | cut by the right edge; bottom-right hidden by the plant (est.) |
| BAKERY sign | 944 | 1509 | 217 | 173 | 835–1052 / 1422–1595 | tilted |
| Flower pot (left, white flowers) | 69 | 666 | 138 | 254 | 0–138 / 539–793 | cut by the left edge; pot 0–110 / 539–641 |
| Potted plant (right, red pot) | 1016 | 806 | 128 | 281 | 952–1080 / 665–946 | in front of the bakery shelf, cut by the right edge |

## Level nodes

Each node has 4 pieces. The pill overlaps the stand, and the plate overlaps the pill by about 3 units. Pieces get smaller toward the top because of perspective.

| Level | Piece | cx | cy | w | h | Bounds (x / y) |
| --- | --- | ---: | ---: | ---: | ---: | --- |
| 1 | Stand (wood table, blue legs) | 345 | 693 | 261 | 155 | 214–475 / 615–770 |
| 1 | Food (apple) | 342 | 785 | 114 | 119 | 285–399 / 725–844 |
| 1 | Number pill | 369 | 688 | 101 | 61 | 318–419 / 657–718 |
| 1 | Star plate | 370 | 630 | 182 | 61 | 279–461 / 599–660 |
| 2 | Stand (round yellow table, red legs) | 702 | 875 | 246 | 156 | 579–825 / 797–953 |
| 2 | Food (cheese) | 702 | 957 | 147 | 113 | 628–775 / 900–1013 |
| 2 | Number pill | 699 | 868 | 104 | 64 | 647–751 / 836–900 |
| 2 | Star plate | 700 | 809 | 178 | 59 | 611–789 / 779–838 |
| 3 | Stand (wood bench) | 338 | 973 | 253 | 115 | 211–464 / 915–1030 |
| 3 | Food (basket of baguettes) | 342 | 1094 | 173 | 178 | 255–428 / 1005–1183 |
| 3 | Number pill | 373 | 1002 | 101 | 63 | 322–423 / 970–1033 |
| 3 | Star plate | 373 | 946 | 165 | 55 | 290–455 / 918–973 |
| 4 | Stand (blue milk display, with jars) | 712 | 1164 | 212 | 228 | 606–818 / 1050–1278 |
| 4 | Food (milk carton) | 714 | 1295 | 103 | 115 | 662–765 / 1237–1352 |
| 4 | Number pill | 688 | 1130 | 103 | 61 | 636–739 / 1099–1160 |
| 4 | Star plate | 685 | 1076 | 167 | 54 | 601–768 / 1049–1103 |
| 5 | Stand (crate on coral legs) | 382 | 1338 | 262 | 155 | 251–513 / 1260–1415 |
| 5 | Food (strawberries) | 374 | 1428 | 208 | 106 | 270–478 / 1375–1481 |
| 5 | Number pill | 432 | 1343 | 99 | 61 | 382–481 / 1312–1373 |
| 5 | Star plate | 433 | 1288 | 166 | 55 | 350–516 / 1260–1315 |
| 6 | Stand (wood table, blue legs) | 560 | 1548 | 257 | 105 | 431–688 / 1495–1600 |
| 6 | Food (honey jar + dipper) | 557 | 1626 | 146 | 117 | 484–630 / 1567–1684 |
| 6 | Number pill | 560 | 1548 | 97 | 55 | 511–608 / 1520–1575 |
| 6 | Star plate | 560 | 1495 | 157 | 54 | 481–638 / 1468–1522 |
| 7 | Number pill only | 684 | 1715 | 112 | 56 | 628–740 / 1687–1743 (top hidden by TopBar) |

Star plates hold three stars about 44 units wide each (est.).

## Path

- Cream band with white dots and a soft golden glow. Its perpendicular width narrows with height (perspective): about **78** near Levels 1–2 (y 700–850), about **62–68** near Levels 5–6 (y 1400–1450), and about **55** just below Level 7 (y 1650). These widths include the soft edge.
- Route: it starts above the checkout (about x 560, y 600), loops right of Level 1 (outer edge about x 724 at y 750), and then zig-zags through the plates of Levels 2 → 3 → 4 → 5 → 6. Above Level 6 it continues to Level 7 (x 684, y 1715).

## Checkout

| Element | cx | cy | w | h | Bounds (x / y) | Notes |
| --- | ---: | ---: | ---: | ---: | --- | --- |
| Checkout group (all pieces below) | 587 | 376 | 936 | 451 | 119–1055 / 150–601 | |
| Counter (yellow top, blue body, yellow base) | 497 | 292 | 752 | 256 | 121–873 / 164–420 | |
| Receipt screen | 499 | 475 | 324 | 253 | 337–661 / 348–601 | |
| CHECKOUT label | 510 | 247 | 336 | 77 | 342–678 / 208–285 | |
| Card terminal | 679 | 404 | 58 | 103 | 650–708 / 352–455 | |
| Shopping bag | 765 | 458 | 150 | 186 | 690–840 / 365–551 | |
| Bread basket | 957 | 285 | 193 | 266 | 860–1053 / 152–418 | overlaps the counter's right end |
| Plant on counter | 222 | 460 | 206 | 183 | 119–325 / 368–551 | |

## Bottom bar

| Element | cx | cy | w | h | Bounds (x / y) | Notes |
| --- | ---: | ---: | ---: | ---: | --- | --- |
| BottomBar (cream bar) | 540 | 72 | 1063 | 139 | 8–1071 / 2–141 | |
| Home button (yellow capsule) | 274 | 72 | 506 | 104 | 21–527 / 20–124 | the current tab, shown pressed |
| Home icon | 187 | 75 | 69 | 68 | 152–221 / 41–109 | |
| HOME text | 346 | 75 | 152 | 41 | 270–422 / 54–95 | cap height 41 |
| Bottom divider | 590 | 73 | 4 | 64 | 588–592 / 41–105 | not centred on the bar (bar centre 540) |
| Shop icon | 728 | 77 | 85 | 72 | 685–770 / 41–113 | |
| SHOP text | 887 | 74 | 139 | 40 | 817–956 / 54–94 | |
| Shop button hit area | 805 | 72 | 506 | 104 | 552–1058 / 20–124 | not drawn in the reference; mirror of the Home capsule (est.) |

## Not in the reference (decisions needed)

- **Daily gift and +10 buttons** exist in the scene and work, but the reference has no place for them.
- **Level 7**: the reference shows a 7th pill at the top. The scene has 6 levels, and adding a level is more than a visual change.
- **Checkout.png** packs the pieces closer together than the reference (sprite aspect 1.74 against 2.08 for the reference group). Its receipt also reads "Miik", where the reference reads "Milk".
