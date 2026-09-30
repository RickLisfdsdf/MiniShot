// 生成 README 截图：画一个假软件界面当背景，用 MiniShot 自己的 SelectForm 绘制框选和红框
// 运行（新编译的 exe 容易被杀毒误删，所以在 PowerShell 里加载）：
//   cd tools; Add-Type -Path DocShots.cs -ReferencedAssemblies System.Windows.Forms, System.Drawing
//   [IO.Directory]::SetCurrentDirectory($PWD); [DocShots]::Main(@("..\docs"))
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class DocShots
{
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
    const BindingFlags F = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static void Main(string[] args)
    {
        SetProcessDPIAware();
        string outDir = args.Length > 0 ? args[0] : ".";
        int W = 1920, H = 1200;
        var bg = MockScreen(W, H);
        bg.Save(System.IO.Path.Combine(outDir, "mock.png"), ImageFormat.Png);

        var asm = Assembly.LoadFrom(System.IO.Path.GetFullPath(@"..\MiniShot.exe"));
        var type = asm.GetType("SelectForm");
        var form = (Form)Activator.CreateInstance(type, F, null, new object[] { bg, new Rectangle(0, 0, W, H) }, null);

        var sel = new Rectangle(440, 100, 700, 560);
        var boxes = new List<Rectangle> { new Rectangle(484, 124, 92, 46), new Rectangle(498, 350, 372, 54) };
        type.GetField("sel", F).SetValue(form, sel);
        type.GetField("annotating", F).SetValue(form, true);
        type.GetField("hot", F).SetValue(form, 1);
        var list = (List<Rectangle>)type.GetField("boxes", F).GetValue(form);
        list.AddRange(boxes);

        using (var shot = new Bitmap(W, H))
        {
            using (var g = Graphics.FromImage(shot))
                type.GetMethod("OnPaint", F).Invoke(form, new object[] { new PaintEventArgs(g, new Rectangle(0, 0, W, H)) });
            shot.Save(System.IO.Path.Combine(outDir, "overlay.png"), ImageFormat.Png);
        }

        // 最终保存出来的图：和 SelectForm.Finish 一样裁剪后画红框
        var red = (Pen)type.GetField("red", F).GetValue(form);
        using (var crop = bg.Clone(sel, bg.PixelFormat))
        {
            using (var g = Graphics.FromImage(crop))
                foreach (var b in boxes) g.DrawRectangle(red, b.X - sel.X, b.Y - sel.Y, b.Width, b.Height);
            crop.Save(System.IO.Path.Combine(outDir, "result.png"), ImageFormat.Png);
        }
    }

    // 一个带展开下拉菜单的假 CAD 软件窗口
    static Bitmap MockScreen(int W, int H)
    {
        var bmp = new Bitmap(W, H, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using (var lg = new LinearGradientBrush(new Rectangle(0, 0, W, H), Color.FromArgb(44, 62, 96), Color.FromArgb(24, 30, 46), 60f))
                g.FillRectangle(lg, 0, 0, W, H);

            var win = new Rectangle(140, 70, 1640, 1040);
            g.FillRectangle(new SolidBrush(Color.FromArgb(243, 244, 246)), win);
            g.FillRectangle(new SolidBrush(Color.FromArgb(32, 36, 44)), win.X, win.Y, win.Width, 54);
            var title = new Font("Segoe UI", 12f);
            g.DrawString("Demo CAD  -  bracket_tube_01", title, Brushes.White, win.X + 20, win.Y + 14);
            string[] caps = { "—", "☐", "✕" };
            for (int i = 0; i < 3; i++) g.DrawString(caps[i], title, Brushes.White, 1640 + i * 50, win.Y + 12);

            var menuFont = new Font("Microsoft YaHei UI", 11f);
            int my = win.Y + 54;
            g.FillRectangle(Brushes.White, win.X, my, win.Width, 46);
            string[] menus = { "文件", "编辑", "视图", "零件", "排样", "工具", "帮助" };
            int mx = win.X + 16;
            var menuPos = new Dictionary<string, int>();
            foreach (var m in menus)
            {
                if (m == "排样") g.FillRectangle(new SolidBrush(Color.FromArgb(221, 234, 250)), mx - 8, my + 4, 76, 38);
                g.DrawString(m, menuFont, Brushes.Black, mx, my + 9);
                menuPos[m] = mx;
                mx += 86;
            }

            // 画布里几根管子
            var canvas = new Rectangle(win.X + 20, my + 66, 1600, 900);
            g.FillRectangle(Brushes.White, canvas);
            using (var grid = new Pen(Color.FromArgb(236, 238, 242)))
                for (int x = canvas.X; x < canvas.Right; x += 40) g.DrawLine(grid, x, canvas.Y, x, canvas.Bottom);
            using (var grid = new Pen(Color.FromArgb(236, 238, 242)))
                for (int y = canvas.Y; y < canvas.Bottom; y += 40) g.DrawLine(grid, canvas.X, y, canvas.Right, y);
            var tubeColors = new[] { Color.FromArgb(96, 165, 250), Color.FromArgb(52, 211, 153), Color.FromArgb(251, 191, 36) };
            for (int i = 0; i < 7; i++)
            {
                var r = new Rectangle(canvas.X + 80, canvas.Y + 80 + i * 110, 1100 - i * 60, 56);
                using (var b = new SolidBrush(tubeColors[i % 3])) g.FillRectangle(b, r);
                for (int k = 1; k < 5; k++)
                    g.FillEllipse(Brushes.White, r.X + k * r.Width / 5 - 12, r.Y + 16, 24, 24);
            }

            // 展开的「排样」下拉菜单
            var dd = new Rectangle(menuPos["排样"] - 8, my + 44, 380, 426);
            using (var sh = new SolidBrush(Color.FromArgb(60, 0, 0, 0))) g.FillRectangle(sh, dd.X + 6, dd.Y + 6, dd.Width, dd.Height);
            g.FillRectangle(Brushes.White, dd);
            g.DrawRectangle(new Pen(Color.FromArgb(200, 204, 210)), dd);
            string[] items = { "自动排样", "手动排样", "共边设置", "—", "排样参数…", "余料管理", "—", "导出排样报告", "批量排样 ▸" };
            int iy = dd.Y + 10;
            foreach (var it in items)
            {
                if (it == "—") { g.DrawLine(new Pen(Color.FromArgb(225, 228, 232)), dd.X + 12, iy + 8, dd.Right - 12, iy + 8); iy += 18; continue; }
                if (it == "排样参数…") g.FillRectangle(new SolidBrush(Color.FromArgb(221, 234, 250)), dd.X + 4, iy, dd.Width - 8, 50);
                g.DrawString(it, menuFont, Brushes.Black, dd.X + 56, iy + 11);
                iy += 52;
            }
        }
        return bmp;
    }
}
