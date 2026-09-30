// MiniShot - 极小截图工具
// PrtScn: 冻结全屏 -> 拖框选区 -> 复制到剪贴板并存 PNG
// 按键被低级钩子吞掉，前台程序收不到，下拉菜单不会收起
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

static class Program
{
    [DllImport("user32.dll")] static extern bool SetProcessDpiAwarenessContext(IntPtr v);
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();

    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--paste") { PasteTo(args[1]); return; }
        bool created;
        using (var mtx = new Mutex(true, "MiniShot_Single", out created))
        {
            if (!created) return;
            try { SetProcessDpiAwarenessContext(new IntPtr(-4)); } catch { try { SetProcessDPIAware(); } catch { } }
            Application.EnableVisualStyles();
            Application.Run(new TrayApp());
        }
    }

    // 资源管理器右键「粘贴截图」：把剪贴板里的图片存到该文件夹
    static void PasteTo(string dir)
    {
        if (!Clipboard.ContainsImage())
        {
            MessageBox.Show("剪贴板里没有图片", "MiniShot", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using (var img = Clipboard.GetImage())
            img.Save(TrayApp.NewFileName(dir), ImageFormat.Png);
    }
}

class TrayApp : ApplicationContext
{
    delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll")] static extern IntPtr SetWindowsHookEx(int id, HookProc fn, IntPtr hMod, uint tid);
    [DllImport("user32.dll")] static extern bool UnhookWindowsHookEx(IntPtr h);
    [DllImport("user32.dll")] static extern IntPtr CallNextHookEx(IntPtr h, int n, IntPtr w, IntPtr l);
    [DllImport("kernel32.dll")] static extern IntPtr GetModuleHandle(string name);
    [DllImport("kernel32.dll")] static extern bool SetProcessWorkingSetSize(IntPtr p, IntPtr min, IntPtr max);

    const int VK_SNAPSHOT = 0x2C;
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string CfgKey = @"Software\MiniShot";

    readonly NotifyIcon tray;
    readonly HookProc proc;
    readonly Control ui = new Control();
    readonly ToolStripMenuItem keyItem;
    IntPtr hook;
    bool busy, recording;
    int hotKey; // 自定义截图键，PrtScn 始终可用

    const string MenuKey = @"Software\Classes\Directory\Background\shell\MiniShotPaste";
    public static string SaveDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    public static string NewFileName(string dir)
    {
        Directory.CreateDirectory(dir);
        string name = "截图_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string path = Path.Combine(dir, name + ".png");
        for (int i = 2; File.Exists(path); i++) path = Path.Combine(dir, name + "_" + i + ".png");
        return path;
    }

    public TrayApp()
    {
        ui.CreateControl();
        using (var k = Registry.CurrentUser.OpenSubKey(CfgKey))
        {
            hotKey = k == null ? 0 : (int)k.GetValue("HotKey", 0);
            if (k != null) SaveDir = (string)k.GetValue("SaveDir", SaveDir);
        }
        proc = HookCallback;
        hook = SetWindowsHookEx(13, proc, GetModuleHandle(null), 0);

        var menu = new ContextMenuStrip();
        menu.Items.Add("立即截图 (PrtScn)", null, (s, e) => Capture());
        menu.Items.Add("3 秒后截图", null, (s, e) => Delayed(3));
        menu.Items.Add("5 秒后截图", null, (s, e) => Delayed(5));
        keyItem = new ToolStripMenuItem();
        keyItem.Click += (s, e) =>
        {
            recording = true;
            tray.ShowBalloonTip(5000, "设置截图键", "现在按一下你想用的键（如 Fn+F1）", ToolTipIcon.Info);
        };
        menu.Items.Add(keyItem);
        var dirItem = new ToolStripMenuItem("保存位置: " + SaveDir);
        dirItem.Click += (s, e) =>
        {
            using (var dlg = new FolderBrowserDialog { SelectedPath = SaveDir, Description = "截图保存到哪个文件夹" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                SaveDir = dlg.SelectedPath;
                using (var k = Registry.CurrentUser.CreateSubKey(CfgKey)) k.SetValue("SaveDir", SaveDir);
                dirItem.Text = "保存位置: " + SaveDir;
            }
        };
        menu.Items.Add(dirItem);
        menu.Items.Add("打开截图文件夹", null, (s, e) => { Directory.CreateDirectory(SaveDir); Process.Start(SaveDir); });
        var ctx = new ToolStripMenuItem("右键菜单「粘贴截图」") { Checked = HasPasteMenu() };
        ctx.Click += (s, e) => { SetPasteMenu(!ctx.Checked); ctx.Checked = HasPasteMenu(); };
        menu.Items.Add(ctx);
        var auto = new ToolStripMenuItem("开机启动") { Checked = IsAutoStart() };
        auto.Click += (s, e) => { SetAutoStart(!auto.Checked); auto.Checked = IsAutoStart(); };
        menu.Items.Add(auto);
        menu.Items.Add("退出", null, (s, e) => ExitThread());

        tray = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };
        UpdateKeyText();
        tray.DoubleClick += (s, e) => Capture();
        try { tray.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
        SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (IntPtr)(-1), (IntPtr)(-1));
    }

    IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int vk = Marshal.ReadInt32(lParam);
            int msg = wParam.ToInt32();
            bool down = msg == 0x100 || msg == 0x104;
            if (recording && down)
            {
                recording = false;
                ui.BeginInvoke((Action)(() => SetHotKey(vk)));
                return (IntPtr)1;
            }
            if (vk == VK_SNAPSHOT || (hotKey != 0 && vk == hotKey))
            {
                if (down) ui.BeginInvoke((Action)Capture);
                return (IntPtr)1; // 吞掉按下和松开，前台程序完全收不到
            }
        }
        return CallNextHookEx(hook, nCode, wParam, lParam);
    }

    void SetHotKey(int vk)
    {
        hotKey = vk;
        using (var k = Registry.CurrentUser.CreateSubKey(CfgKey)) k.SetValue("HotKey", vk);
        UpdateKeyText();
        tray.ShowBalloonTip(3000, "截图键已设置", "以后按 " + KeyName(vk) + " 或 PrtScn 截图", ToolTipIcon.Info);
    }

    void UpdateKeyText()
    {
        keyItem.Text = "设置截图键（当前: " + KeyName(hotKey) + "）";
        tray.Text = hotKey == 0 ? "MiniShot - PrtScn 截图" : "MiniShot - " + KeyName(hotKey) + " / PrtScn 截图";
    }

    static string KeyName(int vk) { return vk == 0 ? "未设置" : ((Keys)vk).ToString(); }

    void Delayed(int sec)
    {
        var t = new System.Windows.Forms.Timer { Interval = sec * 1000 };
        t.Tick += (s, e) => { t.Stop(); t.Dispose(); Capture(); };
        t.Start();
    }

    void Capture()
    {
        if (busy) return;
        busy = true;
        try
        {
            Rectangle vs = SystemInformation.VirtualScreen;
            var bmp = new Bitmap(vs.Width, vs.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
                g.CopyFromScreen(vs.Location, Point.Empty, vs.Size, CopyPixelOperation.SourceCopy);
            using (var f = new SelectForm(bmp, vs)) f.ShowDialog();
            bmp.Dispose();
        }
        finally { busy = false; }
        GC.Collect();
        SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, (IntPtr)(-1), (IntPtr)(-1));
    }

    static bool HasPasteMenu()
    {
        using (var k = Registry.CurrentUser.OpenSubKey(MenuKey)) return k != null;
    }

    static void SetPasteMenu(bool on)
    {
        if (!on) { Registry.CurrentUser.DeleteSubKeyTree(MenuKey, false); return; }
        using (var k = Registry.CurrentUser.CreateSubKey(MenuKey))
        {
            k.SetValue("", "粘贴截图");
            k.SetValue("Icon", Application.ExecutablePath);
        }
        using (var k = Registry.CurrentUser.CreateSubKey(MenuKey + @"\command"))
            k.SetValue("", "\"" + Application.ExecutablePath + "\" --paste \"%V\"");
    }

    static bool IsAutoStart()
    {
        using (var k = Registry.CurrentUser.OpenSubKey(RunKey))
            return k != null && k.GetValue("MiniShot") != null;
    }

    static void SetAutoStart(bool on)
    {
        using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
        {
            if (on) k.SetValue("MiniShot", "\"" + Application.ExecutablePath + "\"");
            else k.DeleteValue("MiniShot", false);
        }
    }

    protected override void ExitThreadCore()
    {
        if (hook != IntPtr.Zero) UnhookWindowsHookEx(hook);
        tray.Visible = false;
        tray.Dispose();
        base.ExitThreadCore();
    }
}

class SelectForm : Form
{
    readonly Bitmap shot;
    readonly Brush dim = new SolidBrush(Color.FromArgb(110, 0, 0, 0));
    readonly Brush barBg = new SolidBrush(Color.FromArgb(235, 32, 32, 32));
    readonly Brush hover = new SolidBrush(Color.FromArgb(0, 120, 215));
    readonly Pen border = new Pen(Color.FromArgb(0, 174, 255), 2);
    readonly Pen red;
    readonly Font font = new Font("Microsoft YaHei UI", 10);
    readonly System.Collections.Generic.List<Rectangle> boxes = new System.Collections.Generic.List<Rectangle>();
    readonly string[] btnText = { "撤销", "完成 ✓", "取消 ✕" };
    Rectangle[] btns = new Rectangle[0];
    Point start;
    Rectangle sel, cur;
    bool dragging, drawing, annotating;
    int hot = -1;

    public SelectForm(Bitmap bmp, Rectangle vs)
    {
        shot = bmp;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = vs;
        TopMost = true;
        ShowInTaskbar = false;
        DoubleBuffered = true;
        Cursor = Cursors.Cross;
        KeyPreview = true;
        float scale;
        using (var g = CreateGraphics()) scale = g.DpiX / 96f;
        red = new Pen(Color.Red, Math.Max(2f, (float)Math.Round(3 * scale)));
    }

    protected override void OnShown(EventArgs e) { base.OnShown(e); Activate(); }

    static Rectangle Norm(Point a, Point b)
    {
        return Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
    }

    // 工具栏放在选区下方靠右，放不下就放上方，再不行放选区里
    void LayoutBar(Graphics g)
    {
        int pad = (int)(g.DpiX / 96f * 10), h = (int)(g.DpiX / 96f * 30), gap = (int)(g.DpiX / 96f * 6);
        var w = new int[btnText.Length];
        int total = 0;
        for (int i = 0; i < w.Length; i++) { w[i] = (int)g.MeasureString(btnText[i], font).Width + pad * 2; total += w[i]; }
        int x = Math.Max(0, sel.Right - total), y = sel.Bottom + gap;
        if (y + h > ClientSize.Height) y = sel.Top - gap - h;
        if (y < 0) y = sel.Bottom - gap - h;
        btns = new Rectangle[w.Length];
        for (int i = 0; i < w.Length; i++) { btns[i] = new Rectangle(x, y, w[i], h); x += w[i]; }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.DrawImageUnscaled(shot, 0, 0);
        g.FillRectangle(dim, ClientRectangle);
        if (sel.Width > 0 && sel.Height > 0)
        {
            g.DrawImage(shot, sel, sel, GraphicsUnit.Pixel);
            g.SetClip(sel);
            foreach (var b in boxes) g.DrawRectangle(red, b);
            if (drawing && cur.Width > 0) g.DrawRectangle(red, cur);
            g.ResetClip();
            g.DrawRectangle(border, sel);
            string s = sel.Width + " × " + sel.Height;
            var sz = g.MeasureString(s, font);
            float y = sel.Y - sz.Height - 4 < 0 ? sel.Y + 4 : sel.Y - sz.Height - 4;
            g.FillRectangle(Brushes.Black, sel.X, y, sz.Width, sz.Height);
            g.DrawString(s, font, Brushes.White, sel.X, y);
            if (annotating)
            {
                LayoutBar(g);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                for (int i = 0; i < btns.Length; i++)
                {
                    g.FillRectangle(i == hot ? hover : barBg, btns[i]);
                    g.DrawString(btnText[i], font, Brushes.White, btns[i], sf);
                }
                const string tip = "拖动画红框 · Enter/双击 完成 · Ctrl+Z 撤销";
                var tz = g.MeasureString(tip, font);
                float tx = btns[0].Left - tz.Width - 8, ty = btns[0].Top + (btns[0].Height - tz.Height) / 2;
                if (tx >= 0)
                {
                    g.FillRectangle(barBg, tx, ty, tz.Width, tz.Height);
                    g.DrawString(tip, font, Brushes.White, tx, ty);
                }
            }
        }
        else
        {
            const string tip = "拖动框选 · Enter 全屏 · Esc/右键 取消";
            var sz = g.MeasureString(tip, font);
            var p = PointToClient(Cursor.Position);
            g.FillRectangle(Brushes.Black, p.X + 16, p.Y + 16, sz.Width, sz.Height);
            g.DrawString(tip, font, Brushes.White, p.X + 16, p.Y + 16);
        }
    }

    int HitButton(Point p)
    {
        if (!annotating) return -1;
        for (int i = 0; i < btns.Length; i++) if (btns[i].Contains(p)) return i;
        return -1;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right) { Close(); return; }
        if (e.Button != MouseButtons.Left) return;
        int b = HitButton(e.Location);
        if (b == 0) { Undo(); return; }
        if (b == 1) { Finish(); return; }
        if (b == 2) { Close(); return; }
        start = e.Location;
        if (annotating && sel.Contains(e.Location)) { drawing = true; cur = Rectangle.Empty; return; }
        // 选区外按下：重新框选
        annotating = false;
        boxes.Clear();
        dragging = true;
        sel = Rectangle.Empty;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (dragging) sel = Norm(start, e.Location);
        else if (drawing) cur = Rectangle.Intersect(Norm(start, e.Location), sel);
        else
        {
            int h = HitButton(e.Location);
            Cursor = h >= 0 ? Cursors.Hand : Cursors.Cross;
            if (h == hot && annotating) return;
            hot = h;
        }
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (dragging)
        {
            dragging = false;
            annotating = sel.Width > 3 && sel.Height > 3;
            if (!annotating) sel = Rectangle.Empty;
        }
        else if (drawing)
        {
            drawing = false;
            if (cur.Width > 3 && cur.Height > 3) boxes.Add(cur);
        }
        Invalidate();
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && annotating && sel.Contains(e.Location)) Finish();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape) Close();
        else if (e.KeyCode == Keys.Enter)
        {
            if (!annotating) sel = ClientRectangle;
            Finish();
        }
        else if (e.KeyCode == Keys.Z && e.Control) Undo();
    }

    void Undo()
    {
        if (boxes.Count > 0) { boxes.RemoveAt(boxes.Count - 1); Invalidate(); }
    }

    void Finish()
    {
        using (var crop = shot.Clone(sel, shot.PixelFormat))
        {
            using (var g = Graphics.FromImage(crop))
                foreach (var b in boxes)
                    g.DrawRectangle(red, b.X - sel.X, b.Y - sel.Y, b.Width, b.Height);
            Clipboard.SetImage(crop);
            crop.Save(TrayApp.NewFileName(TrayApp.SaveDir), ImageFormat.Png);
        }
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { dim.Dispose(); barBg.Dispose(); hover.Dispose(); border.Dispose(); red.Dispose(); font.Dispose(); }
        base.Dispose(disposing);
    }
}
