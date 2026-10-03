#!/usr/bin/env python3
"""
Mesh Warp на OpenCV.

Идея: на изображение накладывается сетка контрольных точек (rows x cols).
Каждую точку можно сдвинуть, и изображение «тянется» вслед за ней.
Сетка разбивается на четырёхугольники, каждый — на два треугольника;
для каждого треугольника считается аффинное преобразование
(конечный треугольник -> исходный), из них собираются карты map_x/map_y,
и одним вызовом cv2.remap получается результат.

Использование:
    python mesh_warp.py image.jpg                 # интерактивный режим
    python mesh_warp.py image.jpg --rows 6 --cols 8
    python mesh_warp.py image.jpg --demo wave -o out.jpg   # без окна
    python mesh_warp.py --demo bulge -o out.jpg            # на тестовой картинке

Управление в интерактивном режиме:
    ЛКМ + перетаскивание — двигать узел сетки
    g — показать/скрыть сетку
    r — сбросить сетку
    s — сохранить результат (warped.png или путь из -o)
    q / Esc — выход
"""

import argparse
import sys

import cv2
import numpy as np


def make_grid(width, height, rows, cols):
    """Равномерная сетка узлов формы (rows+1, cols+1, 2), координаты (x, y)."""
    xs = np.linspace(0, width - 1, cols + 1, dtype=np.float32)
    ys = np.linspace(0, height - 1, rows + 1, dtype=np.float32)
    gx, gy = np.meshgrid(xs, ys)
    return np.dstack([gx, gy])


def build_maps(src_grid, dst_grid, width, height):
    """
    Строит карты для cv2.remap: для каждого пикселя результата —
    координата в исходном изображении. Пиксели вне сетки получают -1.
    """
    map_x = np.full((height, width), -1, dtype=np.float32)
    map_y = np.full((height, width), -1, dtype=np.float32)

    rows, cols = src_grid.shape[0] - 1, src_grid.shape[1] - 1
    for r in range(rows):
        for c in range(cols):
            # Углы ячейки: tl, tr, br, bl -> два треугольника
            idx = [(r, c), (r, c + 1), (r + 1, c + 1), (r + 1, c)]
            for tri in ((0, 1, 2), (0, 2, 3)):
                s = np.float32([src_grid[idx[i]] for i in tri])
                d = np.float32([dst_grid[idx[i]] for i in tri])
                _fill_triangle(map_x, map_y, s, d)
    return map_x, map_y


def _fill_triangle(map_x, map_y, src_tri, dst_tri):
    h, w = map_x.shape
    x0, y0, bw, bh = cv2.boundingRect(dst_tri)
    x1, y1 = min(x0 + bw, w), min(y0 + bh, h)
    x0, y0 = max(x0, 0), max(y0, 0)
    if x1 <= x0 or y1 <= y0:
        return

    # Обратное аффинное преобразование: точка результата -> точка источника
    m = cv2.getAffineTransform(dst_tri, src_tri)

    # Маска треугольника в пределах bounding box
    mask = np.zeros((y1 - y0, x1 - x0), dtype=np.uint8)
    local = np.round(dst_tri - [x0, y0]).astype(np.int32)
    cv2.fillConvexPoly(mask, local, 1)
    sel = mask.astype(bool)

    ys, xs = np.mgrid[y0:y1, x0:x1].astype(np.float32)
    sx = m[0, 0] * xs + m[0, 1] * ys + m[0, 2]
    sy = m[1, 0] * xs + m[1, 1] * ys + m[1, 2]
    map_x[y0:y1, x0:x1][sel] = sx[sel]
    map_y[y0:y1, x0:x1][sel] = sy[sel]


def mesh_warp(image, src_grid, dst_grid, border=cv2.BORDER_CONSTANT):
    """Деформирует image так, что узлы src_grid переезжают в dst_grid."""
    h, w = image.shape[:2]
    map_x, map_y = build_maps(src_grid, dst_grid, w, h)
    return cv2.remap(image, map_x, map_y, cv2.INTER_LINEAR, borderMode=border)


def draw_grid(image, grid, color=(0, 255, 0)):
    out = image.copy()
    pts = np.round(grid).astype(np.int32)
    rows, cols = grid.shape[:2]
    for r in range(rows):
        cv2.polylines(out, [pts[r]], False, color, 1, cv2.LINE_AA)
    for c in range(cols):
        cv2.polylines(out, [np.ascontiguousarray(pts[:, c])], False, color, 1, cv2.LINE_AA)
    for p in pts.reshape(-1, 2):
        cv2.circle(out, tuple(int(v) for v in p), 4, (0, 0, 255), -1, cv2.LINE_AA)
    return out


# ---------------------------------------------------------------- демо-эффекты

def demo_grid(grid, kind, width, height):
    """Готовые деформации сетки (внутренние узлы; край остаётся на месте)."""
    dst = grid.copy()
    inner = (slice(1, -1), slice(1, -1))
    x, y = grid[..., 0], grid[..., 1]
    if kind == "wave":
        dst[..., 1][inner] += (0.04 * height * np.sin(x / width * 2 * np.pi * 2))[inner]
        dst[..., 0][inner] += (0.04 * width * np.sin(y / height * 2 * np.pi * 2))[inner]
    elif kind == "bulge":
        cx, cy = width / 2, height / 2
        dx, dy = x - cx, y - cy
        r = np.sqrt(dx ** 2 + dy ** 2)
        rmax = np.sqrt(cx ** 2 + cy ** 2)
        k = 0.35 * (1 - r / rmax) ** 2  # чем ближе к центру, тем сильнее
        dst[..., 0][inner] += (dx * k)[inner]
        dst[..., 1][inner] += (dy * k)[inner]
    elif kind == "twist":
        cx, cy = width / 2, height / 2
        dx, dy = x - cx, y - cy
        r = np.sqrt(dx ** 2 + dy ** 2)
        rmax = min(cx, cy)
        a = 0.8 * np.clip(1 - r / rmax, 0, 1)
        dst[..., 0][inner] = (cx + dx * np.cos(a) - dy * np.sin(a))[inner]
        dst[..., 1][inner] = (cy + dx * np.sin(a) + dy * np.cos(a))[inner]
    else:
        raise ValueError(f"неизвестный эффект: {kind}")
    return dst


def test_image(width=800, height=600):
    img = np.full((height, width, 3), 255, np.uint8)
    for x in range(0, width, 40):
        cv2.line(img, (x, 0), (x, height), (200, 200, 200), 1)
    for y in range(0, height, 40):
        cv2.line(img, (0, y), (width, y), (200, 200, 200), 1)
    cv2.circle(img, (width // 2, height // 2), 150, (255, 120, 0), 6, cv2.LINE_AA)
    cv2.putText(img, "Mesh Warp", (width // 2 - 190, height // 2 + 25),
                cv2.FONT_HERSHEY_SIMPLEX, 2.2, (40, 40, 200), 5, cv2.LINE_AA)
    return img


# ------------------------------------------------------------ интерактивный UI

class Editor:
    WIN = "Mesh Warp"

    def __init__(self, image, rows, cols, out_path):
        self.image = image
        h, w = image.shape[:2]
        self.src = make_grid(w, h, rows, cols)
        self.dst = self.src.copy()
        self.drag = None
        self.show_grid = True
        self.out_path = out_path
        self.result = image.copy()

    def on_mouse(self, event, x, y, flags, _):
        if event == cv2.EVENT_LBUTTONDOWN:
            d = np.linalg.norm(self.dst - [x, y], axis=2)
            i = np.unravel_index(np.argmin(d), d.shape)
            if d[i] < 15:
                self.drag = i
        elif event == cv2.EVENT_MOUSEMOVE and self.drag is not None:
            self.dst[self.drag] = (x, y)
        elif event == cv2.EVENT_LBUTTONUP and self.drag is not None:
            self.drag = None
            self.result = mesh_warp(self.image, self.src, self.dst)

    def run(self):
        cv2.namedWindow(self.WIN)
        cv2.setMouseCallback(self.WIN, self.on_mouse)
        while True:
            # Во время перетаскивания показываем только сетку (быстро),
            # пересчёт изображения — при отпускании кнопки.
            frame = draw_grid(self.result, self.dst) if self.show_grid else self.result
            cv2.imshow(self.WIN, frame)
            key = cv2.waitKey(15) & 0xFF
            if key in (ord("q"), 27):
                break
            if key == ord("g"):
                self.show_grid = not self.show_grid
            elif key == ord("r"):
                self.dst = self.src.copy()
                self.result = self.image.copy()
            elif key == ord("s"):
                cv2.imwrite(self.out_path, self.result)
                print(f"Сохранено: {self.out_path}")
        cv2.destroyAllWindows()


def main():
    p = argparse.ArgumentParser(description="Mesh Warp на OpenCV")
    p.add_argument("image", nargs="?", help="входное изображение (по умолчанию — тестовое)")
    p.add_argument("--rows", type=int, default=4, help="число ячеек по вертикали")
    p.add_argument("--cols", type=int, default=4, help="число ячеек по горизонтали")
    p.add_argument("--demo", choices=["wave", "bulge", "twist"],
                   help="применить готовый эффект без окна")
    p.add_argument("--show-grid", action="store_true", help="нарисовать сетку на результате (demo)")
    p.add_argument("-o", "--output", default="warped.png", help="куда сохранить результат")
    args = p.parse_args()

    if args.image:
        image = cv2.imread(args.image)
        if image is None:
            sys.exit(f"Не удалось открыть {args.image}")
    else:
        image = test_image()

    if args.demo:
        h, w = image.shape[:2]
        src = make_grid(w, h, args.rows, args.cols)
        dst = demo_grid(src, args.demo, w, h)
        result = mesh_warp(image, src, dst)
        if args.show_grid:
            result = draw_grid(result, dst)
        cv2.imwrite(args.output, result)
        print(f"Сохранено: {args.output}")
    else:
        Editor(image, args.rows, args.cols, args.output).run()


if __name__ == "__main__":
    main()
