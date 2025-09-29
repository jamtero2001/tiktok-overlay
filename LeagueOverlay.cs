using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace LeagueTikTokOverlay
{
    public partial class LeagueOverlay : Form
    {
        private void PositionWebView()
        {
            if (webView == null) return;
            // Reserve space for the top bar so it never gets covered
            int topOffset = (topBar?.Height ?? 28);
            // Leave small margins for resize handles
            int margin = RESIZE_HANDLE;
            webView.Location = new Point(margin, topOffset);
            webView.Size = new Size(this.ClientSize.Width - (margin * 2), this.ClientSize.Height - topOffset - margin);
            webView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            try { topBar?.BringToFront(); exitButton?.BringToFront(); } catch { }
        }
        private WebView2 webView;
        private Button exitButton;
        private Panel topBar;
        private Label titleLabel;
        private NotifyIcon trayIcon;
        private ContextMenuStrip contextMenu;
        private const int WS_EX_TOPMOST = 0x8;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = -20;
        private const int WM_NCHITTEST = 0x84;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int HTCAPTION = 2;
        private const int RESIZE_HANDLE = 10;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCLIENT = 1;
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID_EXIT = 0xBEEF;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public LeagueOverlay()
        {
            InitializeComponent();
            SetupOverlay();
            SetupWebView();
            StartLeagueDetection();
            try { RegisterHotKey(this.Handle, HOTKEY_ID_EXIT, MOD_CONTROL | MOD_SHIFT, (uint)Keys.Q); } catch { }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Draw a subtle resize border to show where users can drag
            using (Pen borderPen = new Pen(Color.FromArgb(80, 255, 255, 255), 2))
            {
                e.Graphics.DrawRectangle(borderPen, 0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            this.Name = "LeagueOverlay";
            this.Text = "League TikTok Overlay";
            this.Size = new Size(400, 600);
            this.MinimumSize = new Size(250, 350);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Black;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Padding = Padding.Empty;
            this.DoubleBuffered = true;

            // Top bar with Exit button
            topBar = new Panel { Height = 36, BackColor = Color.FromArgb(192, 15, 15, 15) };
            topBar.Location = new Point(RESIZE_HANDLE, 0);
            topBar.Width = this.ClientSize.Width - (RESIZE_HANDLE * 2);
            topBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            topBar.MouseDown += (s, e) => StartWindowDrag();
            titleLabel = new Label();
            titleLabel.Text = "League TikTok Overlay";
            titleLabel.ForeColor = Color.White;
            titleLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(12, 9);
            titleLabel.MouseDown += (s, e) => StartWindowDrag();
            exitButton = new Button();
            exitButton.Text = "X";
            exitButton.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            exitButton.ForeColor = Color.White;
            exitButton.BackColor = Color.FromArgb(220, 220, 20, 60);
            exitButton.FlatStyle = FlatStyle.Flat;
            exitButton.FlatAppearance.BorderSize = 0;
            exitButton.Size = new Size(32, 24);
            exitButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            exitButton.Location = new Point(topBar.Width - exitButton.Width - 8, 6);
            exitButton.Click += (s, e) => this.Close();
            exitButton.TabStop = false;
            topBar.Controls.Add(titleLabel);
            topBar.Controls.Add(exitButton);
            topBar.Resize += (s, e) => exitButton.Location = new Point(topBar.Width - exitButton.Width - 8, 6);
            this.Controls.Add(topBar);
            
            // Context menu (right-click) and tray icon
            contextMenu = new ContextMenuStrip();
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => this.Close();
            contextMenu.Items.Add(exitItem);
            this.ContextMenuStrip = contextMenu;

            trayIcon = new NotifyIcon();
            trayIcon.Text = "League TikTok Overlay";
            trayIcon.Icon = SystemIcons.Application;
            trayIcon.Visible = true;
            trayIcon.ContextMenuStrip = contextMenu;
            trayIcon.DoubleClick += (s, e) => this.Activate();

            this.ResumeLayout(false);
        }

        private void SetupOverlay()
        {
            int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle | WS_EX_LAYERED | WS_EX_TOPMOST);
            
            this.Location = new Point(50, 50);
        }

        private async void SetupWebView()
        {
            webView = new WebView2();
            this.Controls.Add(webView);
            PositionWebView();
            this.Resize += (s, e) => { PositionWebView(); this.Invalidate(); };

            try
            {
                var options = new CoreWebView2EnvironmentOptions("--disable-features=SameSiteByDefaultCookies,CookiesWithoutSameSiteMustBeSecure");
                string userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LeagueTikTokOverlay", "WebView2");
                Directory.CreateDirectory(userDataFolder);
                var environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, options);
                await webView.EnsureCoreWebView2Async(environment);
                
                webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                webView.CoreWebView2.Settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36 Edg/126.0.0.0";
                webView.CoreWebView2.PermissionRequested += (s, e) =>
                {
                    e.State = CoreWebView2PermissionState.Allow;
                };
                webView.CoreWebView2.NewWindowRequested += (s, e) =>
                {
                    try
                    {
                        string uri = e.Uri;
                        if (!string.IsNullOrWhiteSpace(uri))
                        {
                            e.Handled = true;
                            webView.CoreWebView2.Navigate(uri);
                        }
                    }
                    catch { }
                };
                webView.CoreWebView2.AddWebResourceRequestedFilter("https://www.tiktok.com/*", CoreWebView2WebResourceContext.All);
                webView.CoreWebView2.AddWebResourceRequestedFilter("https://m.tiktok.com/*", CoreWebView2WebResourceContext.All);
                webView.CoreWebView2.AddWebResourceRequestedFilter("https://*.tiktok.com/*", CoreWebView2WebResourceContext.All);
                webView.CoreWebView2.AddWebResourceRequestedFilter("https://*.tiktokcdn.com/*", CoreWebView2WebResourceContext.All);
                webView.CoreWebView2.WebResourceRequested += (s, e) =>
                {
                    try
                    {
                        if (e.Request.Uri.Contains("tiktok.com") || e.Request.Uri.Contains("tiktokcdn.com"))
                        {
                            e.Request.Headers.SetHeader("Referer", "https://www.tiktok.com/");
                            e.Request.Headers.SetHeader("Origin", "https://www.tiktok.com");
                        }
                    }
                    catch { }
                };
                webView.CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    // try to center content and reduce chrome on desktop pages
                    var js = @"(function(){
                        try {
                          var style = document.createElement('style');
                          style.textContent = `
                            header, nav, [data-e2e='left-nav'], [data-e2e='layout-leftbar'] { display: none !important; }
                            body { margin: 0 !important; background: #000 !important; }
                            main { margin: 0 auto !important; }
                            #overlay-close-btn {
                              position: fixed;
                              top: 12px;
                              right: 12px;
                              z-index: 999999;
                              width: 36px;
                              height: 36px;
                              border-radius: 18px;
                              border: none;
                              background: rgba(220, 20, 60, 0.85);
                              color: #fff;
                              font-size: 20px;
                              font-weight: bold;
                              cursor: pointer;
                              box-shadow: 0 0 8px rgba(0,0,0,0.4);
                            }
                            #overlay-close-btn:hover {
                              background: rgba(255, 0, 80, 0.95);
                            }
                          `;
                          document.head.appendChild(style);

                          if (!document.getElementById('overlay-close-btn')) {
                            var btn = document.createElement('button');
                            btn.id = 'overlay-close-btn';
                            btn.textContent = 'X';
                            btn.addEventListener('click', function(ev){
                              try {
                                ev.preventDefault();
                                ev.stopPropagation();
                                if (window.chrome && window.chrome.webview) {
                                  window.chrome.webview.postMessage(JSON.stringify({ action: 'close' }));
                                }
                              } catch (err) {}
                            });
                            document.body.appendChild(btn);
                          }
                        } catch (e) {}
                        try {
                          if (!window.__overlayDragInit) {
                            window.__overlayDragInit = true;
                            window.addEventListener('mousedown', function(ev){
                              try {
                                if (ev.button === 0) {
                                  var edgeMargin = 14;
                                  var nearHorizontalEdge = ev.clientX < edgeMargin || ev.clientX > window.innerWidth - edgeMargin;
                                  var nearVerticalEdge = ev.clientY < edgeMargin || ev.clientY > window.innerHeight - edgeMargin;
                                  if (nearHorizontalEdge || nearVerticalEdge) {
                                    if (window.chrome && window.chrome.webview) {
                                      window.chrome.webview.postMessage(JSON.stringify({ action: 'startDrag' }));
                                      ev.preventDefault();
                                      ev.stopPropagation();
                                    }
                                  }
                                }
                              } catch (err) {}
                            }, true);
                          }
                        } catch (err) {}
                      })();";
                    try { webView.CoreWebView2.ExecuteScriptAsync(js); } catch { }
                    try { topBar?.BringToFront(); exitButton?.BringToFront(); } catch { }
                };
                
                string tiktokHtml = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>TikTok Overlay</title>
    <style>
        body {
            margin: 0;
            padding: 8px;
            background: rgba(0, 0, 0, 0.8);
            color: white;
            font-family: Arial, sans-serif;
            overflow: hidden;
        }
        .container { height: 100vh; display: flex; flex-direction: column; }
        .url-input {
            width: 100%;
            padding: 8px;
            border: 1px solid #333;
            background: rgba(0, 0, 0, 0.8);
            color: white;
            border-radius: 3px;
            margin-bottom: 8px;
        }
        .feed { flex: 1; overflow-y: auto; overscroll-behavior-y: contain; scroll-snap-type: y mandatory; }
        .card { scroll-snap-align: start; display: flex; align-items: center; justify-content: center; height: calc(100vh - 60px); }
        .frame-wrap { width: min(90vw, 56.25vh); height: min(177.78vw, 90vh); max-height: calc(100vh - 60px); background: #000; border-radius: 10px; overflow: hidden; }
        iframe { width: 100%; height: 100%; border: none; pointer-events: auto; }
        .scroll-mode iframe { pointer-events: none; }
    </style>
</head>
<body>
    <div class='container'>
        <input type='text' class='url-input' id='tiktokUrl' placeholder='Paste TikTok URL and press Enter...'>
        <div id='feed' class='feed'></div>
    </div>

    <script>
        const input = document.getElementById('tiktokUrl');
        const feed = document.getElementById('feed');
        const ids = [];

        function addCard(id){
            const card = document.createElement('div');
            card.className = 'card';
            const wrap = document.createElement('div');
            wrap.className = 'frame-wrap';
            const iframe = document.createElement('iframe');
            iframe.allowFullscreen = true;
            iframe.src = 'https://www.tiktok.com/embed/v2/' + id;
            wrap.appendChild(iframe);
            card.appendChild(wrap);
            feed.appendChild(card);
            // Click to toggle between scroll navigation and interaction mode
            wrap.addEventListener('click', ()=>{
                document.body.classList.toggle('scroll-mode');
            });
        }

        function scrollToLast(){
            const last = feed.lastElementChild; if (last) last.scrollIntoView({behavior:'smooth'});
        }

        function extractId(url){
            const m = url.match(/\/video\/(\d+)/); return m ? m[1] : null;
        }

        async function addFromUrl(url){
            let id = extractId(url);
            if(!id){
                try{
                    const resp = await fetch('https://www.tiktok.com/oembed?url=' + encodeURIComponent(url));
                    if(resp.ok){ const data = await resp.json(); const h = data.html||''; const m = h.match(/data-video-id=['""](\d+)['""]/); if(m) id = m[1]; }
                }catch(e){}
            }
            if(id){ ids.push(id); addCard(id); scrollToLast(); }
        }

        input.addEventListener('keydown', (e)=>{ if(e.key==='Enter'){ const v=input.value.trim(); if(v) addFromUrl(v); }});
        // Default to scroll-mode (wheel navigates). Press 'S' to toggle.
        document.body.classList.add('scroll-mode');
        document.addEventListener('keydown', (e)=>{ if(e.key==='s' || e.key==='S'){ document.body.classList.toggle('scroll-mode'); }});

        // Mouse wheel navigation to snap between cards (only when scroll-mode class is enabled)
        let scrolling = false;
        feed.addEventListener('wheel', (e)=>{
            if (!document.body.classList.contains('scroll-mode')) return;
            if (scrolling) return; // debounce
            const dir = e.deltaY > 0 ? 1 : -1;
            const cards = Array.from(feed.children);
            const current = cards.findIndex(c => Math.abs(c.getBoundingClientRect().top - 0) < 2 || c.getBoundingClientRect().top >= 0);
            let next = current + dir;
            if (next < 0) next = 0;
            if (next >= cards.length) next = cards.length - 1;
            if (cards[next]) {
                scrolling = true;
                cards[next].scrollIntoView({behavior:'smooth'});
                setTimeout(()=> scrolling = false, 400);
            }
            e.preventDefault();
        }, { passive: false });
        
        // Expose toggler to host (optional)
        window.toggleScrollMode = function(){
            document.body.classList.toggle('scroll-mode');
        };
    </script>
</body>
</html>";

                // Serve HTML from a secure virtual host to provide HTTPS origin/referrer
                string assetsDir = Path.Combine(Path.GetTempPath(), "overlay_tiktok_assets");
                Directory.CreateDirectory(assetsDir);
                string indexPath = Path.Combine(assetsDir, "index.html");
                File.WriteAllText(indexPath, tiktokHtml, Encoding.UTF8);
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(assetsHost, assetsDir, CoreWebView2HostResourceAccessKind.Allow);
                // Load our UI and then open TikTok login in a new window/tab inside WebView
                webView.CoreWebView2.Navigate("https://www.tiktok.com/foryou");
                
                webView.CoreWebView2.WebMessageReceived += (sender, args) =>
                {
                    try
                    {
                        var json = args.TryGetWebMessageAsString();
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        var root = doc.RootElement;
                        var action = root.GetProperty("action").GetString();
                        if (action == "close")
                        {
                            this.Invoke(new Action(() => this.Close()));
                        }
                        else if (action == "navigate")
                        {
                            var url = root.TryGetProperty("url", out var u) ? u.GetString() : null;
                            if (!string.IsNullOrWhiteSpace(url))
                            {
                                webView.CoreWebView2.Navigate(url);
                            }
                        }
                        else if (action == "startDrag")
                        {
                            this.Invoke(new Action(() => StartWindowDrag()));
                        }
                    }
                    catch { }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 initialization failed: {ex.Message}", 
                              "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private System.Windows.Forms.Timer leagueDetectionTimer;
        private bool isAttachedToLeague = false;
        private bool isClickThrough = false;
        private bool anchorCentered = true;
        private string assetsHost = "appassets.local";

        private void StartLeagueDetection()
        {
            leagueDetectionTimer = new System.Windows.Forms.Timer();
            leagueDetectionTimer.Interval = 2000; // Check every 2 seconds
            leagueDetectionTimer.Tick += DetectLeagueOfLegends;
            leagueDetectionTimer.Start();
        }

        private void DetectLeagueOfLegends(object sender, EventArgs e)
        {
            IntPtr leagueWindow = FindLeagueWindowRobust();

            if (leagueWindow != IntPtr.Zero)
            {
                AttachToLeague(leagueWindow);
                if (!isAttachedToLeague)
                {
                    isAttachedToLeague = true;
                    this.Show();
                }
            }
            else
            {
                if (isAttachedToLeague)
                {
                    isAttachedToLeague = false;
                    this.Hide();
                }
            }
        }

        private void AttachToLeague(IntPtr leagueWindow)
        {
            RECT leagueRect;
            if (GetWindowRect(leagueWindow, out leagueRect))
            {
                int leagueWidth = leagueRect.Right - leagueRect.Left;
                int leagueHeight = leagueRect.Bottom - leagueRect.Top;

                // Size overlay to 70% of League height, keep 9:16 aspect (phone-like)
                int desiredHeight = Math.Max(400, (int)(leagueHeight * 0.70));
                int desiredWidth = (int)(desiredHeight * 9.0 / 16.0);
                // Clamp if wider than 45% of league width
                int maxWidth = (int)(leagueWidth * 0.45);
                if (desiredWidth > maxWidth)
                {
                    desiredWidth = maxWidth;
                    desiredHeight = (int)(desiredWidth * 16.0 / 9.0);
                }
                this.Size = new Size(desiredWidth, desiredHeight);

                int overlayX;
                int overlayY;
                if (anchorCentered)
                {
                    overlayX = leagueRect.Left + (leagueWidth - desiredWidth) / 2;
                    overlayY = leagueRect.Top + (leagueHeight - desiredHeight) / 2;
                }
                else
                {
                    overlayX = leagueRect.Right - desiredWidth - 20; // 20px margin from right edge
                    overlayY = leagueRect.Top + 20; // 20px margin from top
                }

                this.Location = new Point(overlayX, overlayY);
                this.BringToFront();
            }
        }

        private IntPtr FindLeagueWindowRobust()
        {
            // Try known window title
            IntPtr hWnd = FindWindow(null, "League of Legends (TM) Client");
            if (hWnd != IntPtr.Zero) return hWnd;

            // Try known window class
            hWnd = FindWindow("RiotWindowClass", null);
            if (hWnd != IntPtr.Zero) return hWnd;

            // Fallback: search visible top-level windows with matching title
            IntPtr found = IntPtr.Zero;
            EnumWindows((wnd, lParam) =>
            {
                if (!IsWindowVisible(wnd)) return true;
                int len = GetWindowTextLength(wnd);
                if (len <= 0) return true;
                var sb = new StringBuilder(len + 1);
                GetWindowText(wnd, sb, sb.Capacity);
                var title = sb.ToString();
                if (!string.IsNullOrWhiteSpace(title) &&
                    title.IndexOf("League of Legends", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found = wnd;
                    return false; // stop enumeration
                }
                return true;
            }, IntPtr.Zero);
            return found;
        }

        private bool isDragging = false;
        private Point lastCursor;
        private Point lastForm;

        private void EnableDragging()
        {
            isDragging = true;
        }

        private void StartWindowDrag()
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (isDragging)
            {
                lastCursor = Cursor.Position;
                lastForm = this.Location;
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging && e.Button == MouseButtons.Left)
            {
                int deltaX = Cursor.Position.X - lastCursor.X;
                int deltaY = Cursor.Position.Y - lastCursor.Y;
                this.Location = new Point(lastForm.X + deltaX, lastForm.Y + deltaY);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isDragging = false;
            base.OnMouseUp(e);
        }

        private bool isLargeSize = false;

        private void ToggleSize()
        {
            if (isLargeSize)
            {
                this.Size = new Size(400, 600);
                isLargeSize = false;
            }
            else
            {
                this.Size = new Size(600, 800);
                isLargeSize = true;
            }
        }

        private void SetClickThrough(bool enable)
        {
            int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            if (enable)
            {
                exStyle |= WS_EX_TRANSPARENT;
                if (topBar != null) topBar.Visible = false; // hide UI when click-through
            }
            else
            {
                exStyle &= ~WS_EX_TRANSPARENT;
                if (topBar != null) { topBar.Visible = true; topBar.BringToFront(); exitButton?.BringToFront(); }
            }
            SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle);
            isClickThrough = enable;
        }

        private void ToggleClickThrough()
        {
            SetClickThrough(!isClickThrough);
        }

        private static string ToMobileTikTokUrl(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return url;
                var uri = new Uri(url);
                var host = uri.Host.ToLowerInvariant();
                if (host == "www.tiktok.com")
                {
                    // Force mobile site which centers the video column and hides left nav
                    var builder = new UriBuilder(uri) { Host = "m.tiktok.com" };
                    return builder.Uri.ToString();
                }
                return url;
            }
            catch
            {
                return url;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            leagueDetectionTimer?.Stop();
            leagueDetectionTimer?.Dispose();
            webView?.Dispose();
            try { trayIcon.Visible = false; trayIcon.Dispose(); } catch { }
            try { contextMenu.Dispose(); } catch { }
            try { UnregisterHotKey(this.Handle, HOTKEY_ID_EXIT); } catch { }
            base.OnFormClosed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID_EXIT)
            {
                this.Close();
                return;
            }
            if (m.Msg == WM_NCHITTEST)
            {
                base.WndProc(ref m);
                if (m.Result.ToInt32() == HTCLIENT)
                {
                    var cursor = this.PointToClient(new Point((int)m.LParam & 0xFFFF, (int)m.LParam >> 16));
                    bool top = cursor.Y < RESIZE_HANDLE;
                    bool left = cursor.X < RESIZE_HANDLE;
                    bool right = cursor.X > this.ClientSize.Width - RESIZE_HANDLE;
                    bool bottom = cursor.Y > this.ClientSize.Height - RESIZE_HANDLE;

                    // Prioritize corner resizing
                    if (top && left) { m.Result = (IntPtr)HTTOPLEFT; return; }
                    if (top && right) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                    if (bottom && left) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                    if (bottom && right) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                    
                    // Edge resizing
                    if (left) { m.Result = (IntPtr)HTLEFT; return; }
                    if (right) { m.Result = (IntPtr)HTRIGHT; return; }
                    if (top) { m.Result = (IntPtr)HTTOP; return; }
                    if (bottom) { m.Result = (IntPtr)HTBOTTOM; return; }

                    // Make the topBar area act as caption for moving (but not the very top edge which is for resizing)
                    if (cursor.Y > RESIZE_HANDLE && cursor.Y <= (topBar?.Height ?? 36) + RESIZE_HANDLE)
                    {
                        m.Result = (IntPtr)HTCAPTION;
                        return;
                    }
                }
                return;
            }
            base.WndProc(ref m);
        }

        // Handle hotkeys for quick control
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.H)) // Ctrl+H to hide/show
            {
                this.Visible = !this.Visible;
                return true;
            }
            else if (keyData == Keys.Escape) // Esc to exit
            {
                this.Close();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.Q)) // Ctrl+Q force quit
            {
                this.Close();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.R)) // Ctrl+R to resize
            {
                ToggleSize();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.M)) // Ctrl+M to enable move
            {
                EnableDragging();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.T)) // Ctrl+T to toggle click-through
            {
                ToggleClickThrough();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}