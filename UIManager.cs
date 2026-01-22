using System;
using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

namespace GooseGameAP
{
    public class UIManager
    {
        private static ManualLogSource Log => Plugin.Log;
        
        private List<string> receivedItemNames = new List<string>();
        
        public string ServerAddress = "archipelago.gg";
        public string ServerPort = "38281";
        public string SlotName = "Goose";
        public string Password = "";
        public string Status = "Not connected";
        
        private string currentNotification = "";
        private float notificationTimer = 0f;
        
        // Connection window
        private Rect connectionWindowRect;
        private bool isDraggingConnection = false;
        private Vector2 connectionDragOffset = Vector2.zero;
        private bool connectionPositionInitialized = false;
        private float connectionScale = 1.0f;
        private const string CONNECTION_SCALE_KEY = "AP_ConnectionScale";
        private const string CONNECTION_X_KEY = "AP_ConnectionX";
        private const string CONNECTION_Y_KEY = "AP_ConnectionY";
        
        // Server log window (notebook style)
        private bool showServerLog = true;
        private Rect serverLogWindowRect;
        private bool isDraggingServerLog = false;
        private Vector2 serverLogDragOffset = Vector2.zero;
        private bool serverLogPositionInitialized = false;
        private float serverLogScale = 1.0f;
        private Vector2 serverLogScrollPosition = Vector2.zero;
        private const string SERVERLOG_SCALE_KEY = "AP_ServerLogScale";
        private const string SERVERLOG_X_KEY = "AP_ServerLogX";
        private const string SERVERLOG_Y_KEY = "AP_ServerLogY";
        
        // Soul tracker window
        private bool showSoulTracker = false;
        private Rect soulTrackerWindowRect;
        private bool isDraggingSoulTracker = false;
        private Vector2 soulDragOffset = Vector2.zero;
        private bool soulTrackerPositionInitialized = false;
        private float soulTrackerScale = 1.0f;
        private Vector2 soulScrollPosition = Vector2.zero;
        private const string SOUL_SCALE_KEY = "AP_SoulTrackerScale";
        private const string SOUL_X_KEY = "AP_SoulTrackerX";
        private const string SOUL_Y_KEY = "AP_SoulTrackerY";
        
        private const float MIN_SCALE = 0.5f;
        private const float MAX_SCALE = 2.0f;
        
        // Textures for notebook style
        private Texture2D paperTexture;
        private Texture2D lineTexture;
        private bool texturesInitialized = false;
        
        // Custom styles
        private GUIStyle customVerticalScrollbar;
        private GUIStyle customVerticalScrollbarThumb;
        private GUIStyle customHorizontalSlider;
        private GUIStyle customHorizontalSliderThumb;
        private bool scrollbarStylesInitialized = false;
        
        public UIManager()
        {
            connectionScale = PlayerPrefs.GetFloat(CONNECTION_SCALE_KEY, 1.0f);
            connectionScale = Mathf.Clamp(connectionScale, MIN_SCALE, MAX_SCALE);
            
            serverLogScale = PlayerPrefs.GetFloat(SERVERLOG_SCALE_KEY, 1.0f);
            serverLogScale = Mathf.Clamp(serverLogScale, MIN_SCALE, MAX_SCALE);
            
            soulTrackerScale = PlayerPrefs.GetFloat(SOUL_SCALE_KEY, 1.0f);
            soulTrackerScale = Mathf.Clamp(soulTrackerScale, MIN_SCALE, MAX_SCALE);
        }
        
        public void ShowNotification(string message)
        {
            currentNotification = message;
            notificationTimer = 4f;
        }
        
        public void AddReceivedItem(string itemName) { receivedItemNames.Add(itemName); }
        public void AddChatMessage(string message) { }
        public void UpdateNotification(float deltaTime) { if (notificationTimer > 0) notificationTimer -= deltaTime; }
        public void ClearReceivedItems() { receivedItemNames.Clear(); }
        public int ReceivedItemCount => receivedItemNames.Count;
        public void ToggleSoulTracker() { showSoulTracker = !showSoulTracker; }
        public void ToggleServerLog() { showServerLog = !showServerLog; }
        public bool IsSoulTrackerVisible => showSoulTracker;
        public bool IsServerLogVisible => showServerLog;
        
        private void InitializeTextures()
        {
            if (texturesInitialized) return;
            
            // Create paper texture (cream/beige colored)
            paperTexture = new Texture2D(1, 1);
            paperTexture.SetPixel(0, 0, new Color(0.96f, 0.94f, 0.88f, 0.98f));
            paperTexture.Apply();
            
            // Create line texture for ruled paper effect
            lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0, 0, new Color(0.7f, 0.85f, 0.95f, 0.4f));
            lineTexture.Apply();
            
            texturesInitialized = true;
        }
        
        private void InitializeScrollbarStyles()
        {
            if (scrollbarStylesInitialized) return;
            
            Texture2D scrollbarBg = new Texture2D(1, 1);
            scrollbarBg.SetPixel(0, 0, new Color(0.8f, 0.78f, 0.72f, 0.8f));
            scrollbarBg.Apply();
            
            Texture2D thumbNormal = new Texture2D(1, 1);
            thumbNormal.SetPixel(0, 0, new Color(0.6f, 0.55f, 0.45f, 1f));
            thumbNormal.Apply();
            
            Texture2D thumbHover = new Texture2D(1, 1);
            thumbHover.SetPixel(0, 0, new Color(0.5f, 0.45f, 0.35f, 1f));
            thumbHover.Apply();
            
            Texture2D thumbActive = new Texture2D(1, 1);
            thumbActive.SetPixel(0, 0, new Color(0.4f, 0.35f, 0.25f, 1f));
            thumbActive.Apply();
            
            customVerticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
            customVerticalScrollbar.normal.background = scrollbarBg;
            customVerticalScrollbar.fixedWidth = 0;
            
            customVerticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
            customVerticalScrollbarThumb.normal.background = thumbNormal;
            customVerticalScrollbarThumb.hover.background = thumbHover;
            customVerticalScrollbarThumb.active.background = thumbActive;
            customVerticalScrollbarThumb.fixedWidth = 0;
            
            Texture2D sliderBg = new Texture2D(1, 1);
            sliderBg.SetPixel(0, 0, new Color(0.2f, 0.2f, 0.25f, 1f));
            sliderBg.Apply();
            
            Texture2D sliderThumbNormal = new Texture2D(1, 1);
            sliderThumbNormal.SetPixel(0, 0, new Color(0.6f, 0.5f, 0.8f, 1f));
            sliderThumbNormal.Apply();
            
            Texture2D sliderThumbHover = new Texture2D(1, 1);
            sliderThumbHover.SetPixel(0, 0, new Color(0.7f, 0.6f, 0.9f, 1f));
            sliderThumbHover.Apply();
            
            Texture2D sliderThumbActive = new Texture2D(1, 1);
            sliderThumbActive.SetPixel(0, 0, new Color(0.8f, 0.7f, 1f, 1f));
            sliderThumbActive.Apply();
            
            customHorizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
            customHorizontalSlider.normal.background = sliderBg;
            customHorizontalSlider.fixedHeight = 0;
            
            customHorizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
            customHorizontalSliderThumb.normal.background = sliderThumbNormal;
            customHorizontalSliderThumb.hover.background = sliderThumbHover;
            customHorizontalSliderThumb.active.background = sliderThumbActive;
            customHorizontalSliderThumb.fixedHeight = 0;
            customHorizontalSliderThumb.fixedWidth = 0;
            
            scrollbarStylesInitialized = true;
        }
        
        // ============== CONNECTION WINDOW (Notebook Style) ==============
        
        public void DrawUI(Plugin plugin)
        {
            InitializeScrollbarStyles();
            
            float baseW = 400, baseH = 524;
            float winW = baseW * connectionScale;
            float winH = baseH * connectionScale;
            
            if (!connectionPositionInitialized)
            {
                float savedX = PlayerPrefs.GetFloat(CONNECTION_X_KEY, 15);
                float savedY = PlayerPrefs.GetFloat(CONNECTION_Y_KEY, 15);
                connectionWindowRect = new Rect(savedX, savedY, winW, winH);
                connectionPositionInitialized = true;
            }
            
            connectionWindowRect.width = winW;
            connectionWindowRect.height = winH;
            connectionWindowRect.x = Mathf.Clamp(connectionWindowRect.x, 0, Screen.width - winW);
            connectionWindowRect.y = Mathf.Clamp(connectionWindowRect.y, 0, Screen.height - winH);
            
            float s = connectionScale;
            int titleSize = Mathf.RoundToInt(16 * s);
            int headerSize = Mathf.RoundToInt(13 * s);
            int textSize = Mathf.RoundToInt(12 * s);
            int fieldTextSize = Mathf.RoundToInt(14 * s);
            float lineH = 28 * s;
            float inputH = 26 * s;
            float buttonH = 28 * s;
            
            float winX = connectionWindowRect.x;
            float winY = connectionWindowRect.y;
            
            // Paper background (white/very light)
            GUI.color = new Color(0.99f, 0.99f, 0.97f, 1f);
            GUI.DrawTexture(new Rect(winX, winY, winW, winH), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // Red margin line on the left
            float marginX = winX + 30 * s;
            GUI.color = new Color(0.9f, 0.6f, 0.6f, 1f);
            GUI.DrawTexture(new Rect(marginX, winY, 2 * s, winH), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // Blue horizontal ruled lines
            float ruleSpacing = 24 * s;
            GUI.color = new Color(0.6f, 0.75f, 0.9f, 0.8f);
            for (float ly = winY + ruleSpacing; ly < winY + winH - 5 * s; ly += ruleSpacing)
            {
                GUI.DrawTexture(new Rect(winX + 5 * s, ly, winW - 10 * s, 1), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
            
            // Content area
            float x = marginX + 10 * s;
            float y = winY + 8 * s;
            float w = winW - (marginX - winX) - 20 * s;
            
            // Title
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            GUI.Label(new Rect(x, y, w * 0.7f, 24 * s), $"<size={titleSize}><i>Archipelago</i></size>");
            GUI.color = Color.white;
            
            y += 28 * s;
            
            // Scale slider
            GUI.color = new Color(0.3f, 0.3f, 0.35f);
            GUI.Label(new Rect(x, y + 3 * s, 36 * s, 20 * s), $"<size={Mathf.RoundToInt(10 * s)}>Size:</size>");
            GUI.color = Color.white;
            
            float sliderX = x + 38 * s;
            float sliderW = w - 95 * s;
            
            GUI.color = new Color(0.8f, 0.8f, 0.82f, 1f);
            GUI.DrawTexture(new Rect(sliderX, y + 8 * s, sliderW, 8 * s), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            float newScale = GUI.HorizontalSlider(new Rect(sliderX, y, sliderW, 22 * s), connectionScale, MIN_SCALE, MAX_SCALE);
            
            GUI.color = new Color(0.3f, 0.3f, 0.35f);
            GUI.Label(new Rect(sliderX + sliderW + 6 * s, y + 3 * s, 45 * s, 20 * s), $"<size={Mathf.RoundToInt(10 * s)}>{Mathf.RoundToInt(connectionScale * 100)}%</size>");
            GUI.color = Color.white;
            
            if (Mathf.Abs(newScale - connectionScale) > 0.01f)
            {
                connectionScale = newScale;
                PlayerPrefs.SetFloat(CONNECTION_SCALE_KEY, connectionScale);
                PlayerPrefs.Save();
            }
            
            y += 26 * s;
            
            // Create style for text fields
            GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField);
            fieldStyle.fontSize = fieldTextSize;
            
            // Connection fields
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            GUI.Label(new Rect(x, y + 3 * s, 55 * s, inputH), $"<size={textSize}>Server:</size>");
            GUI.color = Color.white;
            ServerAddress = GUI.TextField(new Rect(x + 55 * s, y, w - 120 * s, inputH), ServerAddress, fieldStyle);
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            GUI.Label(new Rect(x + w - 60 * s, y + 3 * s, 15 * s, inputH), $"<size={textSize}>:</size>");
            GUI.color = Color.white;
            ServerPort = GUI.TextField(new Rect(x + w - 45 * s, y, 45 * s, inputH), ServerPort, fieldStyle);
            y += inputH + 6 * s;
            
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            GUI.Label(new Rect(x, y + 3 * s, 55 * s, inputH), $"<size={textSize}>Slot:</size>");
            GUI.color = Color.white;
            SlotName = GUI.TextField(new Rect(x + 55 * s, y, w - 55 * s, inputH), SlotName, fieldStyle);
            y += inputH + 6 * s;
            
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            GUI.Label(new Rect(x, y + 3 * s, 68 * s, inputH), $"<size={textSize}>Password:</size>");
            GUI.color = Color.white;
            Password = GUI.PasswordField(new Rect(x + 70 * s, y, w - 70 * s, inputH), Password, '*', fieldStyle);
            y += inputH + 12 * s;
            
            // Buttons
            float btnW = (w - 12 * s) / 4;
            if (GUI.Button(new Rect(x, y, btnW, buttonH), plugin.IsConnected ? "Connected!" : "Connect"))
                if (!plugin.IsConnected) plugin.Connect();
            if (GUI.Button(new Rect(x + btnW + 4 * s, y, btnW, buttonH), "Disconnect"))
                plugin.Disconnect();
            if (GUI.Button(new Rect(x + (btnW + 4 * s) * 2, y, btnW, buttonH), "Sync"))
            {
                plugin.GateManager?.SyncGatesFromAccessFlags();
                ShowNotification("Gates synced!");
            }
            if (GUI.Button(new Rect(x + (btnW + 4 * s) * 3, y, btnW, buttonH), "Reset"))
            {
                plugin.ResetAllAccess();
                plugin.NPCManager?.RefreshNPCStates();
                ShowNotification("Progress reset!");
            }
            y += buttonH + 10 * s;
            
            // Status
            GUI.color = plugin.IsConnected ? new Color(0.2f, 0.55f, 0.2f) : new Color(0.7f, 0.3f, 0.2f);
            GUI.Label(new Rect(x, y, w, lineH), $"<size={headerSize}><b>{Status}</b></size>");
            GUI.color = Color.white;
            y += lineH;
            
            // Notification
            if (notificationTimer > 0 && !string.IsNullOrEmpty(currentNotification))
            {
                GUI.color = new Color(0.55f, 0.45f, 0.1f);
                GUI.Label(new Rect(x, y, w, lineH), $"<size={textSize}><b>â–¶ {currentNotification}</b></size>");
                GUI.color = Color.white;
                y += lineH;
            }
            
            y += 6 * s;
            
            // Area Access Header
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            GUI.Label(new Rect(x, y, w, lineH), $"<size={headerSize}><b>Area Access</b></size>");
            GUI.color = Color.white;
            y += lineH;
            
            // Area Access Grid
            float accessColW = w / 3f;
            float accessRowH = 26 * s;
            int accessTextSize = Mathf.RoundToInt(11 * s);
            
            DrawAccessCompact(x, y, accessColW, "Garden", plugin.HasGardenAccess, accessTextSize);
            DrawAccessCompact(x + accessColW, y, accessColW, "High St", plugin.HasHighStreetAccess, accessTextSize);
            DrawAccessCompact(x + accessColW * 2, y, accessColW, "Back Gdn", plugin.HasBackGardensAccess, accessTextSize);
            y += accessRowH;
            
            DrawAccessCompact(x, y, accessColW, "Pub", plugin.HasPubAccess, accessTextSize);
            DrawAccessCompact(x + accessColW, y, accessColW, "Model V", plugin.HasModelVillageAccess, accessTextSize);
            DrawAccessCompact(x + accessColW * 2, y, accessColW, "Bell", plugin.HasGoldenBell, accessTextSize);
            y += accessRowH + 8 * s;
            
            // Stats
            int speed = (int)(plugin.TrapManager.GetEffectiveSpeedMultiplier() * 100);
            string trapText = plugin.TrapManager.GetActiveTrapText();
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            if (!string.IsNullOrEmpty(trapText))
            {
                GUI.color = new Color(0.7f, 0.25f, 0.2f);
                GUI.Label(new Rect(x, y, w, lineH), $"<size={textSize}>{trapText}</size>");
            }
            else
            {
                GUI.Label(new Rect(x, y, w, lineH), $"<size={textSize}>Spd:{speed}% | Silent:{(plugin.IsSilent ? "ON" : "OFF")} | Honk:Lv{plugin.MegaHonkCount} | Locs:{plugin.CheckedLocationCount}</size>");
            }
            GUI.color = Color.white;
            
            // Footers
            GUI.color = new Color(0.4f, 0.4f, 0.45f);
            GUI.Label(new Rect(x, winY + winH - 66 * s, w, 20 * s), 
                $"<size={Mathf.RoundToInt(9 * s)}>F1=Toggle | F2=Souls | F3=Log</size>");
            GUI.color = Color.white;
            
            // Footer 2
            GUI.color = new Color(0.4f, 0.4f, 0.45f);
            GUI.Label(new Rect(x, winY + winH - 44 * s, w, 20 * s), 
                $"<size={Mathf.RoundToInt(9 * s)}>G=GooseDay | C=Colour | 0-5=Area Warps | H=Hub Warp</size>");
            GUI.color = Color.white;
            
            // Footer 3
            GUI.color = new Color(0.4f, 0.4f, 0.45f);
            GUI.Label(new Rect(x, winY + winH - 22 * s, w, 20 * s), 
                $"<size={Mathf.RoundToInt(9 * s)}>0=Start | 1=Garden | 2=High Street | 3=Back Gardens | 4=Pub | 5=Model Village</size>");
            GUI.color = Color.white;
            
            // Handle dragging from title area (top 26px) - AFTER all controls
            float dragAreaH = 26 * s;
            Event e = Event.current;
            Rect titleBar = new Rect(winX, winY, winW, dragAreaH);
            
            if (e.type == EventType.MouseDown && e.button == 0 && titleBar.Contains(e.mousePosition))
            {
                isDraggingConnection = true;
                connectionDragOffset = e.mousePosition - new Vector2(winX, winY);
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && isDraggingConnection)
            {
                isDraggingConnection = false;
                PlayerPrefs.SetFloat(CONNECTION_X_KEY, connectionWindowRect.x);
                PlayerPrefs.SetFloat(CONNECTION_Y_KEY, connectionWindowRect.y);
                PlayerPrefs.Save();
            }
            else if (e.type == EventType.MouseDrag && isDraggingConnection)
            {
                connectionWindowRect.x = e.mousePosition.x - connectionDragOffset.x;
                connectionWindowRect.y = e.mousePosition.y - connectionDragOffset.y;
                e.Use();
            }
        }
        
        // ============== SERVER LOG WINDOW (Notebook Style) ==============
        
        public void DrawServerLog(Plugin plugin)
        {
            if (!showServerLog) return;
            
            InitializeScrollbarStyles();
            
            float baseW = 450, baseH = 400;
            float winW = baseW * serverLogScale;
            float winH = baseH * serverLogScale;
            float s = serverLogScale;
            
            if (!serverLogPositionInitialized)
            {
                float savedX = PlayerPrefs.GetFloat(SERVERLOG_X_KEY, 15);
                float savedY = PlayerPrefs.GetFloat(SERVERLOG_Y_KEY, 520);
                serverLogWindowRect = new Rect(savedX, savedY, winW, winH);
                serverLogPositionInitialized = true;
            }
            
            serverLogWindowRect.width = winW;
            serverLogWindowRect.height = winH;
            serverLogWindowRect.x = Mathf.Clamp(serverLogWindowRect.x, 0, Screen.width - winW);
            serverLogWindowRect.y = Mathf.Clamp(serverLogWindowRect.y, 0, Screen.height - winH);
            
            float winX = serverLogWindowRect.x;
            float winY = serverLogWindowRect.y;
            
            // Paper background (white/very light)
            GUI.color = new Color(0.99f, 0.99f, 0.97f, 1f);
            GUI.DrawTexture(new Rect(winX, winY, winW, winH), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // Red margin line on the left
            float marginX = winX + 35 * s;
            GUI.color = new Color(0.9f, 0.6f, 0.6f, 1f);
            GUI.DrawTexture(new Rect(marginX, winY, 2 * s, winH), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // Blue horizontal ruled lines
            float lineSpacing = 24 * s;
            float firstLineY = winY + 60 * s;
            GUI.color = new Color(0.6f, 0.75f, 0.9f, 0.8f);
            for (float ly = firstLineY; ly < winY + winH - 5 * s; ly += lineSpacing)
            {
                GUI.DrawTexture(new Rect(winX + 5 * s, ly, winW - 10 * s, 1), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
            
            // Content area (right of margin)
            float textX = marginX + 10 * s;
            float textW = winW - (marginX - winX) - 20 * s;
            
            // Title
            int titleSize = Mathf.RoundToInt(14 * s);
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            GUI.Label(new Rect(textX, winY + 5 * s, textW - 50 * s, 22 * s), 
                $"<size={titleSize}><i>Server Log</i></size>");
            GUI.color = Color.white;
            
            // Clear button
            if (GUI.Button(new Rect(winX + winW - 50 * s, winY + 4 * s, 42 * s, 20 * s), "Clear"))
            {
                plugin.Client?.ClearServerLog();
            }
            
            // Scale slider
            float sliderY = winY + 30 * s;
            float sliderH = 24 * s;
            int sliderTextSize = Mathf.RoundToInt(11 * s);
            
            GUI.color = new Color(0.3f, 0.3f, 0.35f);
            GUI.Label(new Rect(textX, sliderY + 3 * s, 36 * s, sliderH), $"<size={sliderTextSize}>Size:</size>");
            GUI.color = Color.white;
            
            float sliderX = textX + 38 * s;
            float sliderW = textW - 100 * s;
            
            // Draw slider track background
            GUI.color = new Color(0.8f, 0.8f, 0.82f, 1f);
            GUI.DrawTexture(new Rect(sliderX, sliderY + 8 * s, sliderW, 8 * s), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // The actual slider
            float newScale = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderW, sliderH), 
                serverLogScale, MIN_SCALE, MAX_SCALE);
            
            GUI.color = new Color(0.3f, 0.3f, 0.35f);
            GUI.Label(new Rect(sliderX + sliderW + 8 * s, sliderY + 3 * s, 50 * s, sliderH), $"<size={sliderTextSize}>{Mathf.RoundToInt(serverLogScale * 100)}%</size>");
            GUI.color = Color.white;
            
            if (Mathf.Abs(newScale - serverLogScale) > 0.01f)
            {
                serverLogScale = newScale;
                PlayerPrefs.SetFloat(SERVERLOG_SCALE_KEY, serverLogScale);
                PlayerPrefs.Save();
            }
            
            // Log content
            float contentY = firstLineY;
            float contentH = winH - (contentY - winY) - 10 * s;
            DrawServerLogContent(plugin, textX, contentY, textW, contentH, s);
            
            // Handle dragging AFTER all other controls - only from title area (top 28px)
            float dragAreaH = 28 * s;
            Event e = Event.current;
            Rect titleBar = new Rect(winX, winY, winW, dragAreaH);
            
            if (e.type == EventType.MouseDown && e.button == 0 && titleBar.Contains(e.mousePosition))
            {
                isDraggingServerLog = true;
                serverLogDragOffset = e.mousePosition - new Vector2(winX, winY);
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && isDraggingServerLog)
            {
                isDraggingServerLog = false;
                PlayerPrefs.SetFloat(SERVERLOG_X_KEY, serverLogWindowRect.x);
                PlayerPrefs.SetFloat(SERVERLOG_Y_KEY, serverLogWindowRect.y);
                PlayerPrefs.Save();
            }
            else if (e.type == EventType.MouseDrag && isDraggingServerLog)
            {
                serverLogWindowRect.x = e.mousePosition.x - serverLogDragOffset.x;
                serverLogWindowRect.y = e.mousePosition.y - serverLogDragOffset.y;
                e.Use();
            }
        }
        
        private void DrawServerLogContent(Plugin plugin, float x, float y, float w, float h, float s)
        {
            var serverLog = plugin.Client?.ServerLog;
            if (serverLog == null || serverLog.Count == 0)
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                GUI.Label(new Rect(x + 5, y + 10, w - 10, 20), $"<size={Mathf.RoundToInt(11 * s)}><i>Waiting for messages...</i></size>");
                GUI.color = Color.white;
                return;
            }
            
            float lineH = 15 * s;  // Tighter line height
            int textSize = Mathf.RoundToInt(11 * s);
            float maxTextWidth = w - 16 * s;
            float entryGap = 3 * s;  // Small gap between entries
            
            // Calculate content height
            float contentH = 8;
            foreach (var entry in serverLog)
            {
                string fullText = $"{entry.Timestamp:HH:mm} {entry.Message}";
                int lineCount = EstimateLineCount(fullText, maxTextWidth, textSize);
                contentH += lineH * lineCount + entryGap;
            }
            
            customVerticalScrollbar.fixedWidth = Mathf.Max(10, 10 * s);
            customVerticalScrollbarThumb.fixedWidth = Mathf.Max(8, 8 * s);
            
            GUIStyle origScrollbar = GUI.skin.verticalScrollbar;
            GUIStyle origThumb = GUI.skin.verticalScrollbarThumb;
            GUI.skin.verticalScrollbar = customVerticalScrollbar;
            GUI.skin.verticalScrollbarThumb = customVerticalScrollbarThumb;
            
            serverLogScrollPosition = GUI.BeginScrollView(
                new Rect(x, y, w, h), serverLogScrollPosition,
                new Rect(0, 0, w - 14 * s, contentH), false, true);
            
            GUI.skin.verticalScrollbar = origScrollbar;
            GUI.skin.verticalScrollbarThumb = origThumb;
            
            GUIStyle wrapStyle = new GUIStyle(GUI.skin.label);
            wrapStyle.wordWrap = true;
            wrapStyle.richText = true;
            wrapStyle.fontSize = textSize;
            wrapStyle.padding = new RectOffset(0, 0, 0, 0);
            wrapStyle.margin = new RectOffset(0, 0, 0, 0);
            
            float logY = 2;
            foreach (var entry in serverLog)
            {
                GUI.color = GetNotebookLogColor(entry.Type);
                
                string fullText = $"{entry.Timestamp:HH:mm} {entry.Message}";
                int lineCount = EstimateLineCount(fullText, maxTextWidth, textSize);
                float entryH = lineH * lineCount;
                
                GUI.Label(new Rect(2, logY, maxTextWidth, entryH + 2), $"<size={textSize}>{fullText}</size>", wrapStyle);
                logY += entryH + entryGap;
            }
            GUI.color = Color.white;
            
            GUI.EndScrollView();
            
            // Auto-scroll
            if (contentH > h)
                serverLogScrollPosition.y = contentH - h;
        }
        
        private Color GetNotebookLogColor(ServerLogType type)
        {
            // Notebook-style ink colors
            switch (type)
            {
                case ServerLogType.System: return new Color(0.3f, 0.3f, 0.35f);      // Grey pencil
                case ServerLogType.Chat: return new Color(0.15f, 0.15f, 0.2f);        // Dark ink
                case ServerLogType.ItemReceived: return new Color(0.1f, 0.5f, 0.2f);  // Green ink
                case ServerLogType.LocationSent: return new Color(0.1f, 0.3f, 0.6f);  // Blue ink
                case ServerLogType.Hint: return new Color(0.6f, 0.4f, 0.1f);          // Brown/orange
                case ServerLogType.PlayerJoin: return new Color(0.2f, 0.5f, 0.3f);    // Green
                case ServerLogType.PlayerLeave: return new Color(0.5f, 0.3f, 0.2f);   // Brown
                case ServerLogType.Goal: return new Color(0.2f, 0.6f, 0.2f);          // Bright green
                case ServerLogType.DeathLink: return new Color(0.7f, 0.15f, 0.15f);   // Red ink
                case ServerLogType.Error: return new Color(0.7f, 0.2f, 0.2f);         // Red
                default: return new Color(0.2f, 0.2f, 0.25f);
            }
        }
        
        private int EstimateLineCount(string text, float maxWidth, int fontSize)
        {
            float charWidth = fontSize * 0.5f;
            float textWidth = text.Length * charWidth;
            int lines = Mathf.CeilToInt(textWidth / maxWidth);
            return Mathf.Max(1, lines);
        }
        
        private void DrawScaleSlider(ref float scale, float x, ref float y, float w, float s, string prefKey)
        {
            float sliderH = Mathf.Max(18, 18 * s);
            float thumbSize = Mathf.Max(22, 22 * s);
            int textSize = Mathf.RoundToInt(10 * s);
            
            GUI.color = new Color(0.5f, 0.5f, 0.5f);
            GUI.Label(new Rect(x, y, 38 * s, sliderH), $"<size={textSize}>Scale:</size>");
            GUI.color = Color.white;
            
            float sliderX = x + 40 * s;
            float sliderW = w - 95 * s;
            
            customHorizontalSlider.fixedHeight = sliderH * 0.4f;
            customHorizontalSliderThumb.fixedHeight = sliderH;
            customHorizontalSliderThumb.fixedWidth = thumbSize;
            
            float newScale = GUI.HorizontalSlider(new Rect(sliderX, y, sliderW, sliderH), scale, MIN_SCALE, MAX_SCALE,
                customHorizontalSlider, customHorizontalSliderThumb);
            
            GUI.color = new Color(0.6f, 0.6f, 0.6f);
            GUI.Label(new Rect(sliderX + sliderW + 4 * s, y, 48 * s, sliderH), $"<size={textSize}>{Mathf.RoundToInt(scale * 100)}%</size>");
            GUI.color = Color.white;
            
            if (Mathf.Abs(newScale - scale) > 0.01f)
            {
                scale = newScale;
                PlayerPrefs.SetFloat(prefKey, scale);
                PlayerPrefs.Save();
            }
            
            y += sliderH;
        }
        
        private void HandleWindowDrag(ref Rect windowRect, ref bool isDragging, ref Vector2 dragOffset,
            float titleBarH, string xKey, string yKey)
        {
            Rect dragArea = new Rect(windowRect.x, windowRect.y, windowRect.width, titleBarH);
            HandleWindowDragFromRect(ref windowRect, ref isDragging, ref dragOffset, dragArea, xKey, yKey);
        }
        
        private void HandleWindowDragFromRect(ref Rect windowRect, ref bool isDragging, ref Vector2 dragOffset,
            Rect dragArea, string xKey, string yKey)
        {
            Event e = Event.current;
            
            if (e.type == EventType.MouseDown && e.button == 0 && dragArea.Contains(e.mousePosition))
            {
                isDragging = true;
                dragOffset = e.mousePosition - new Vector2(windowRect.x, windowRect.y);
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && isDragging)
            {
                isDragging = false;
                PlayerPrefs.SetFloat(xKey, windowRect.x);
                PlayerPrefs.SetFloat(yKey, windowRect.y);
                PlayerPrefs.Save();
            }
            else if (e.type == EventType.MouseDrag && isDragging)
            {
                windowRect.x = e.mousePosition.x - dragOffset.x;
                windowRect.y = e.mousePosition.y - dragOffset.y;
                e.Use();
            }
        }
        
        private void DrawAccessCompact(float x, float y, float w, string label, bool hasAccess, int textSize)
        {
            int checkSize = textSize + 4;
            GUI.color = hasAccess ? new Color(0.2f, 0.5f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
            GUI.Label(new Rect(x, y, w, 26), $"<size={checkSize}>{(hasAccess ? "âœ“" : "âœ—")}</size><size={textSize}> {label}</size>");
            GUI.color = Color.white;
        }
        
        // ============== SOUL TRACKER WINDOW (Notebook Style) ==============
        
        public void DrawSoulTracker(Plugin plugin)
        {
            if (!showSoulTracker) return;
            
            InitializeScrollbarStyles();
            
            float baseW = 320, baseH = 450;
            float winW = baseW * soulTrackerScale;
            float winH = baseH * soulTrackerScale;
            float s = soulTrackerScale;
            
            if (!soulTrackerPositionInitialized)
            {
                float savedX = PlayerPrefs.GetFloat(SOUL_X_KEY, Screen.width - winW - 15);
                float savedY = PlayerPrefs.GetFloat(SOUL_Y_KEY, 15);
                soulTrackerWindowRect = new Rect(savedX, savedY, winW, winH);
                soulTrackerPositionInitialized = true;
            }
            
            soulTrackerWindowRect.width = winW;
            soulTrackerWindowRect.height = winH;
            soulTrackerWindowRect.x = Mathf.Clamp(soulTrackerWindowRect.x, 0, Screen.width - winW);
            soulTrackerWindowRect.y = Mathf.Clamp(soulTrackerWindowRect.y, 0, Screen.height - winH);
            
            int titleSize = Mathf.RoundToInt(15 * s);
            int headerSize = Mathf.RoundToInt(12 * s);
            int itemSize = Mathf.RoundToInt(11 * s);
            float lineH = 18 * s;
            float headerH = 22 * s;
            
            float winX = soulTrackerWindowRect.x;
            float winY = soulTrackerWindowRect.y;
            
            // Paper background (white/very light)
            GUI.color = new Color(0.99f, 0.99f, 0.97f, 1f);
            GUI.DrawTexture(new Rect(winX, winY, winW, winH), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // Red margin line on the left
            float marginX = winX + 28 * s;
            GUI.color = new Color(0.9f, 0.6f, 0.6f, 1f);
            GUI.DrawTexture(new Rect(marginX, winY, 2 * s, winH), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            // Blue horizontal ruled lines
            float ruleSpacing = 20 * s;
            GUI.color = new Color(0.6f, 0.75f, 0.9f, 0.8f);
            for (float ly = winY + ruleSpacing; ly < winY + winH - 5 * s; ly += ruleSpacing)
            {
                GUI.DrawTexture(new Rect(winX + 5 * s, ly, winW - 10 * s, 1), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
            
            // Content area
            float x = marginX + 8 * s;
            float y = winY + 8 * s;
            float w = winW - (marginX - winX) - 16 * s;
            
            // Title
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            GUI.Label(new Rect(x, y, w * 0.6f, 24 * s), $"<size={titleSize}><i>Souls</i></size>");
            GUI.color = Color.white;
            
            y += 26 * s;
            
            // Scale slider
            GUI.color = new Color(0.3f, 0.3f, 0.35f);
            GUI.Label(new Rect(x, y + 3 * s, 32 * s, 18 * s), $"<size={Mathf.RoundToInt(10 * s)}>Size:</size>");
            GUI.color = Color.white;
            
            float sliderX = x + 34 * s;
            float sliderW = w - 85 * s;
            
            GUI.color = new Color(0.8f, 0.8f, 0.82f, 1f);
            GUI.DrawTexture(new Rect(sliderX, y + 8 * s, sliderW, 8 * s), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            float newScale = GUI.HorizontalSlider(new Rect(sliderX, y, sliderW, 22 * s), soulTrackerScale, MIN_SCALE, MAX_SCALE);
            
            GUI.color = new Color(0.3f, 0.3f, 0.35f);
            GUI.Label(new Rect(sliderX + sliderW + 6 * s, y + 3 * s, 42 * s, 18 * s), $"<size={Mathf.RoundToInt(10 * s)}>{Mathf.RoundToInt(soulTrackerScale * 100)}%</size>");
            GUI.color = Color.white;
            
            if (Mathf.Abs(newScale - soulTrackerScale) > 0.01f)
            {
                soulTrackerScale = newScale;
                PlayerPrefs.SetFloat(SOUL_SCALE_KEY, soulTrackerScale);
                PlayerPrefs.Save();
            }
            
            y += 26 * s;
            
            // Scroll area
            float scrollTop = y;
            float scrollH = winH - (scrollTop - winY) - 8 * s;
            float contentH = CalculateSoulContentHeight(plugin, lineH, headerH, s);
            
            customVerticalScrollbar.fixedWidth = Mathf.Max(12, 12 * s);
            customVerticalScrollbarThumb.fixedWidth = Mathf.Max(10, 10 * s);
            
            GUIStyle origScrollbar = GUI.skin.verticalScrollbar;
            GUIStyle origThumb = GUI.skin.verticalScrollbarThumb;
            GUI.skin.verticalScrollbar = customVerticalScrollbar;
            GUI.skin.verticalScrollbarThumb = customVerticalScrollbarThumb;
            
            soulScrollPosition = GUI.BeginScrollView(
                new Rect(x - 4 * s, scrollTop, w + 8 * s, scrollH),
                soulScrollPosition,
                new Rect(0, 0, w - 10 * s, contentH));
            
            GUI.skin.verticalScrollbar = origScrollbar;
            GUI.skin.verticalScrollbarThumb = origThumb;
            
            float scrollY = 0;
            float sectionW = w - 6 * s;
            
            scrollY = DrawSoulSection(scrollY, sectionW, "NPC Souls", GetNPCSouls(plugin), headerSize, itemSize, lineH, headerH, s);
            
            if (plugin.PropSoulsEnabled)
            {
                scrollY = DrawSoulSection(scrollY, sectionW, "Starting Area", GetStartingAreaPropSouls(plugin), headerSize, itemSize, lineH, headerH, s);
                scrollY = DrawSoulSection(scrollY, sectionW, "Hub", GetHubPropSouls(plugin), headerSize, itemSize, lineH, headerH, s);
                scrollY = DrawSoulSection(scrollY, sectionW, "Garden", GetGardenPropSouls(plugin), headerSize, itemSize, lineH, headerH, s);
                scrollY = DrawSoulSection(scrollY, sectionW, "High Street", GetHighStreetPropSouls(plugin), headerSize, itemSize, lineH, headerH, s);
                scrollY = DrawSoulSection(scrollY, sectionW, "Back Gardens", GetBackGardensPropSouls(plugin), headerSize, itemSize, lineH, headerH, s);
                scrollY = DrawSoulSection(scrollY, sectionW, "Pub", GetPubPropSouls(plugin), headerSize, itemSize, lineH, headerH, s);
                scrollY = DrawSoulSection(scrollY, sectionW, "Model Village", GetModelVillagePropSouls(plugin), headerSize, itemSize, lineH, headerH, s);
                //scrollY = DrawSoulSection(scrollY, sectionW, "Shared", GetSharedPropSouls(plugin), headerSize, itemSize, lineH, headerH, s);
            }
            else
            {
                GUI.color = new Color(0.4f, 0.5f, 0.4f);
                GUI.Label(new Rect(4 * s, scrollY, sectionW, headerH), $"<size={headerSize}>Prop Souls: DISABLED</size>");
                GUI.color = Color.white;
            }
            
            GUI.EndScrollView();
            
            // Handle dragging from title area (top 24px) - AFTER all controls
            float dragAreaH = 24 * s;
            Event e = Event.current;
            Rect titleBar = new Rect(winX, winY, winW, dragAreaH);
            
            if (e.type == EventType.MouseDown && e.button == 0 && titleBar.Contains(e.mousePosition))
            {
                isDraggingSoulTracker = true;
                soulDragOffset = e.mousePosition - new Vector2(winX, winY);
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0 && isDraggingSoulTracker)
            {
                isDraggingSoulTracker = false;
                PlayerPrefs.SetFloat(SOUL_X_KEY, soulTrackerWindowRect.x);
                PlayerPrefs.SetFloat(SOUL_Y_KEY, soulTrackerWindowRect.y);
                PlayerPrefs.Save();
            }
            else if (e.type == EventType.MouseDrag && isDraggingSoulTracker)
            {
                soulTrackerWindowRect.x = e.mousePosition.x - soulDragOffset.x;
                soulTrackerWindowRect.y = e.mousePosition.y - soulDragOffset.y;
                e.Use();
            }
        }
        
        private float CalculateSoulContentHeight(Plugin plugin, float lineH, float headerH, float s)
        {
            float h = headerH + 11 * lineH + 6 * s;
            if (plugin.PropSoulsEnabled)
            {
                h += headerH + 20 * lineH + 6 * s;
                h += headerH + 24 * lineH + 6 * s;
                h += headerH + 25 * lineH + 6 * s;
                h += headerH + 22 * lineH + 6 * s;
                h += headerH + 12 * lineH + 6 * s;
                h += headerH + 26 * lineH + 6 * s;
            }
            return h + 20 * s;
        }
        
        private float DrawSoulSection(float startY, float width, string title, List<SoulInfo> souls,
            int headerSize, int itemSize, float lineH, float headerH, float s)
        {
            float y = startY;
            int collected = 0;
            foreach (var soul in souls) if (soul.HasSoul) collected++;
            
            // Header in dark ink
            GUI.color = new Color(0.2f, 0.2f, 0.25f);
            GUI.Label(new Rect(4 * s, y, width, headerH), $"<size={headerSize}><b>{title}</b> ({collected}/{souls.Count})</size>");
            GUI.color = Color.white;
            y += headerH;
            
            int checkSize = itemSize + 4;
            foreach (var soul in souls)
            {
                // Green for collected, gray for not
                GUI.color = soul.HasSoul ? new Color(0.2f, 0.5f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
                GUI.Label(new Rect(8 * s, y, width - 8 * s, lineH), $"<size={checkSize}>{(soul.HasSoul ? "•" : "○")}</size><size={itemSize}> {soul.Name}</size>");
                GUI.color = Color.white;
                y += lineH;
            }
            return y + 4 * s;
        }
        
        private struct SoulInfo { public string Name; public bool HasSoul; public SoulInfo(string n, bool h) { Name = n; HasSoul = h; } }
        
        private List<SoulInfo> GetNPCSouls(Plugin p) => new List<SoulInfo> {
            new SoulInfo("Groundskeeper", p.HasGroundskeeperSoul),
            new SoulInfo("Boy", p.HasBoySoul),
            new SoulInfo("TV Shop Owner", p.HasTVShopOwnerSoul),
            new SoulInfo("Market Lady", p.HasMarketLadySoul),
            new SoulInfo("Tidy Neighbour", p.HasTidyNeighbourSoul),
            new SoulInfo("Messy Neighbour", p.HasMessyNeighbourSoul),
            new SoulInfo("Burly Man", p.HasBurlyManSoul),
            new SoulInfo("Old Man", p.HasOldManSoul),
            new SoulInfo("Pub Lady", p.HasPubLadySoul),
            new SoulInfo("Fancy Ladies", p.HasFancyLadiesSoul),
            new SoulInfo("Cook", p.HasCookSoul),
        };
        
        private List<SoulInfo> GetStartingAreaPropSouls(Plugin p) { var m = p.PropManager; return new List<SoulInfo> {
            new SoulInfo("Boots", m?.HasReceivedSoul("Boots") ?? false),
            new SoulInfo("Drink Can", m?.HasReceivedSoul("Drink Can") ?? false),
            new SoulInfo("Tennis Ball", m?.HasReceivedSoul("Tennis Ball") ?? false),
        }; }
        
        private List<SoulInfo> GetHubPropSouls(Plugin p) { var m = p.PropManager; return new List<SoulInfo> {
            new SoulInfo("Boots", m?.HasReceivedSoul("Boots") ?? false),
            new SoulInfo("Dummy", m?.HasReceivedSoul("Dummy") ?? false),
            new SoulInfo("Fishing Bobber", m?.HasReceivedSoul("Fishing Bobber") ?? false),
            new SoulInfo("Pint Bottles", m?.HasReceivedSoul("Pint Bottles") ?? false),
            new SoulInfo("Ribbons", m?.HasReceivedSoul("Ribbons") ?? false),
            new SoulInfo("Tackle Box", m?.HasReceivedSoul("Tackle Box") ?? false),
        }; }
        
        private List<SoulInfo> GetGardenPropSouls(Plugin p) { var m = p.PropManager; return new List<SoulInfo> {
            new SoulInfo("Apples", m?.HasReceivedSoul("Apples") ?? false),
            new SoulInfo("Carrots", m?.HasReceivedSoul("Carrots") ?? false),
            new SoulInfo("Esky", m?.HasReceivedSoul("Esky") ?? false),
            new SoulInfo("Gumboots", m?.HasReceivedSoul("Gumboots") ?? false),
            new SoulInfo("Jam", m?.HasReceivedSoul("Jam") ?? false),
            new SoulInfo("Mallet", m?.HasReceivedSoul("Mallet") ?? false),
            new SoulInfo("Picnic Basket", m?.HasReceivedSoul("Picnic Basket") ?? false),
            new SoulInfo("Picnic Mug", m?.HasReceivedSoul("Picnic Mug") ?? false),
            new SoulInfo("Pumpkins", m?.HasReceivedSoul("Pumpkins") ?? false),
            new SoulInfo("Radio", m?.HasReceivedSoul("Radio") ?? false),
            new SoulInfo("Rake", m?.HasReceivedSoul("Rake") ?? false),
            new SoulInfo("Sandwich", m?.HasReceivedSoul("Sandwich") ?? false),
            new SoulInfo("Shovel", m?.HasReceivedSoul("Shovel") ?? false),
            new SoulInfo("Sun Hat", m?.HasReceivedSoul("Sun Hat") ?? false),
            new SoulInfo("Thermos", m?.HasReceivedSoul("Thermos") ?? false),
            new SoulInfo("Topsoil Bag", m?.HasReceivedSoul("Topsoil Bags") ?? false),
            new SoulInfo("Trowel", m?.HasReceivedSoul("Trowel") ?? false),
            new SoulInfo("Tulip", m?.HasReceivedSoul("Tulip") ?? false),
            new SoulInfo("Watering Can", m?.HasReceivedSoul("Watering Can") ?? false),
            new SoulInfo("Wooden Crate", m?.HasReceivedSoul("Wooden Crate") ?? false),
        }; }
        
        private List<SoulInfo> GetHighStreetPropSouls(Plugin p) { var m = p.PropManager; return new List<SoulInfo> {
            new SoulInfo("Adding Machine", m?.HasReceivedSoul("Adding Machine") ?? false),
            new SoulInfo("Apple Cores", m?.HasReceivedSoul("Apple Cores") ?? false),
            new SoulInfo("Baby Doll", m?.HasReceivedSoul("Baby Doll") ?? false),
            new SoulInfo("Carrots", m?.HasReceivedSoul("Carrots") ?? false),
            new SoulInfo("Cucumbers", m?.HasReceivedSoul("Cucumbers") ?? false),
            new SoulInfo("Dish Soap Bottle", m?.HasReceivedSoul("Dish Soap Bottle") ?? false),
            new SoulInfo("Dustbin", m?.HasReceivedSoul("Dustbin") ?? false),
            new SoulInfo("Dustbin Lid", m?.HasReceivedSoul("Dustbin Lid") ?? false),
            new SoulInfo("Chalk", m?.HasReceivedSoul("Chalk") ?? false),
            new SoulInfo("Hairbrush", m?.HasReceivedSoul("Hairbrush") ?? false),
            new SoulInfo("Horn-Rimmed Glasses", m?.HasReceivedSoul("Horn-Rimmed Glasses") ?? false),
            new SoulInfo("Leeks", m?.HasReceivedSoul("Leeks") ?? false),
            new SoulInfo("Lily Flower", m?.HasReceivedSoul("Lily Flower") ?? false),
            new SoulInfo("Loo Paper", m?.HasReceivedSoul("Loo Paper") ?? false),
            new SoulInfo("Oranges", m?.HasReceivedSoul("Oranges") ?? false),
            new SoulInfo("Pint Bottles", m?.HasReceivedSoul("Pint Bottles") ?? false),
            new SoulInfo("Pricing Gun", m?.HasReceivedSoul("Pricing Gun") ?? false),
            new SoulInfo("Push Broom", m?.HasReceivedSoul("Push Broom") ?? false),
            new SoulInfo("Red Glasses", m?.HasReceivedSoul("Red Glasses") ?? false),
            new SoulInfo("Shopping Basket", m?.HasReceivedSoul("Shopping Basket") ?? false),
            new SoulInfo("Spray Bottle", m?.HasReceivedSoul("Spray Bottle") ?? false),
            new SoulInfo("Stereoscope", m?.HasReceivedSoul("Stereoscope") ?? false),
            new SoulInfo("Sunglasses", m?.HasReceivedSoul("Sunglasses") ?? false),
            new SoulInfo("Tinned Food", m?.HasReceivedSoul("Tinned Food") ?? false),
            new SoulInfo("Tomatoes", m?.HasReceivedSoul("Tomatoes") ?? false),
            new SoulInfo("Toothbrush", m?.HasReceivedSoul("Toothbrush") ?? false),
            new SoulInfo("Toy Car", m?.HasReceivedSoul("Toy Car") ?? false),
            new SoulInfo("Toy Plane", m?.HasReceivedSoul("Toy Plane") ?? false),
            new SoulInfo("Umbrellas", m?.HasReceivedSoul("Umbrellas") ?? false),
            new SoulInfo("Walkie Talkies", m?.HasReceivedSoul("Walkie Talkies") ?? false),
            new SoulInfo("Weed Tools", m?.HasReceivedSoul("Weed Tools") ?? false),
        }; }
        
        private List<SoulInfo> GetBackGardensPropSouls(Plugin p) { var m = p.PropManager; return new List<SoulInfo> {
            new SoulInfo("Badminton Racket", m?.HasReceivedSoul("Badminton Racket") ?? false),
            new SoulInfo("Bra", m?.HasReceivedSoul("Bra") ?? false),
            new SoulInfo("Bust Pipe", m?.HasReceivedSoul("Bust Pipe") ?? false),
            new SoulInfo("Bust Glasses", m?.HasReceivedSoul("Bust Glasses") ?? false),
            new SoulInfo("Bust Hat", m?.HasReceivedSoul("Bust Hat") ?? false),
            new SoulInfo("Clippers", m?.HasReceivedSoul("Clippers") ?? false),
            new SoulInfo("Cricket Ball", m?.HasReceivedSoul("Cricket Ball") ?? false),
            new SoulInfo("Cricket Bat", m?.HasReceivedSoul("Cricket Bat") ?? false),
            new SoulInfo("Drawer", m?.HasReceivedSoul("Drawer") ?? false),
            new SoulInfo("Duck Statue", m?.HasReceivedSoul("Duck Statue") ?? false),
            new SoulInfo("Enamel Jug", m?.HasReceivedSoul("Enamel Jug") ?? false),
            new SoulInfo("Frog Statue", m?.HasReceivedSoul("Frog Statue") ?? false),
            new SoulInfo("Jeremy Fish", m?.HasReceivedSoul("Jeremy Fish") ?? false),
            new SoulInfo("Newspaper", m?.HasReceivedSoul("Newspaper") ?? false),
            new SoulInfo("No Goose Sign (Clean)", m?.HasReceivedSoul("No Goose Sign (Clean)") ?? false),
            new SoulInfo("No Goose Sign (Messy)", m?.HasReceivedSoul("No Goose Sign (Messy)") ?? false),
            new SoulInfo("Paintbrush", m?.HasReceivedSoul("Paintbrush") ?? false),
            new SoulInfo("Pot Stack", m?.HasReceivedSoul("Pot Stack") ?? false),
            new SoulInfo("Ribbons", m?.HasReceivedSoul("Ribbons") ?? false),
            new SoulInfo("Rose", m?.HasReceivedSoul("Rose") ?? false),
            // new SoulInfo("Rose Box", m?.HasReceivedSoul("Rose Box") ?? false),
            new SoulInfo("Soap", m?.HasReceivedSoul("Soap") ?? false),
            new SoulInfo("Socks", m?.HasReceivedSoul("Socks") ?? false),
            new SoulInfo("Tea Cup", m?.HasReceivedSoul("Tea Cup") ?? false),
            new SoulInfo("Tea Pot", m?.HasReceivedSoul("Tea Pot") ?? false),
            new SoulInfo("Vase", m?.HasReceivedSoul("Vase") ?? false),
        }; }
        
        private List<SoulInfo> GetPubPropSouls(Plugin p) { var m = p.PropManager; return new List<SoulInfo> {
            new SoulInfo("Bucket", m?.HasReceivedSoul("Bucket") ?? false),
            new SoulInfo("Dartboard", m?.HasReceivedSoul("Dartboard") ?? false),
            new SoulInfo("Candlestick", m?.HasReceivedSoul("Candlestick") ?? false),
            new SoulInfo("Cork", m?.HasReceivedSoul("Cork") ?? false),
            new SoulInfo("Flower for Vase", m?.HasReceivedSoul("Flower for Vase") ?? false),
            new SoulInfo("Forks", m?.HasReceivedSoul("Forks") ?? false),
            new SoulInfo("Harmonica", m?.HasReceivedSoul("Harmonica") ?? false),
            new SoulInfo("Knives", m?.HasReceivedSoul("Knives") ?? false),
            new SoulInfo("Letter", m?.HasReceivedSoul("Letter") ?? false),
            new SoulInfo("Mop", m?.HasReceivedSoul("Mop") ?? false),
            new SoulInfo("Mop Bucket", m?.HasReceivedSoul("Mop Bucket") ?? false),
            new SoulInfo("No Goose Sign (Pub)", m?.HasReceivedSoul("No Goose Sign (Pub)") ?? false),
            new SoulInfo("Parcel", m?.HasReceivedSoul("Parcel") ?? false),
            new SoulInfo("Pepper Grinder", m?.HasReceivedSoul("Pepper Grinder") ?? false),
            new SoulInfo("Plates", m?.HasReceivedSoul("Plates") ?? false),
            new SoulInfo("Pint Glasses", m?.HasReceivedSoul("Pint Glasses") ?? false),
            new SoulInfo("Portable Stool", m?.HasReceivedSoul("Portable Stool") ?? false),
            new SoulInfo("Quoits", m?.HasReceivedSoul("Quoits") ?? false),
            new SoulInfo("Stealth Box", m?.HasReceivedSoul("Stealth Box") ?? false),
            new SoulInfo("Tomatoes", m?.HasReceivedSoul("Tomatoes") ?? false),
            new SoulInfo("Toy Boat", m?.HasReceivedSoul("Toy Boat") ?? false),
            new SoulInfo("Traffic Cone", m?.HasReceivedSoul("Traffic Cone") ?? false),
        }; }
        
        private List<SoulInfo> GetModelVillagePropSouls(Plugin p) { var m = p.PropManager; return new List<SoulInfo> {
            new SoulInfo("Golden Bell", m?.HasReceivedSoul("Golden Bell") ?? false),
            new SoulInfo("Mini Bench", m?.HasReceivedSoul("Miniature Benches") ?? false),
            new SoulInfo("Mini Birdbath", m?.HasReceivedSoul("Miniature Birdbath") ?? false),
            new SoulInfo("Mini Easel", m?.HasReceivedSoul("Miniature Easel") ?? false),
            new SoulInfo("Mini Mail Pillar", m?.HasReceivedSoul("Miniature Mail Pillar") ?? false),
            new SoulInfo("Mini People", m?.HasReceivedSoul("Miniature People") ?? false),
            new SoulInfo("Mini Phone Door", m?.HasReceivedSoul("Miniature Phone Door") ?? false),
            new SoulInfo("Mini Pump", m?.HasReceivedSoul("Miniature Pump") ?? false),
            new SoulInfo("Mini Shovel", m?.HasReceivedSoul("Miniature Shovel") ?? false),
            new SoulInfo("Mini Sun Lounge", m?.HasReceivedSoul("Miniature Sun Lounge") ?? false),
            new SoulInfo("Poppy Flower", m?.HasReceivedSoul("Poppy Flower") ?? false),
            new SoulInfo("Timber Handle", m?.HasReceivedSoul("Timber Handle") ?? false),
        }; }
        
        private List<SoulInfo> GetSharedPropSouls(Plugin p) { var m = p.PropManager; return new List<SoulInfo> {
            // new SoulInfo("Carrot", m?.HasReceivedSoul("Carrot Soul") ?? false),
            // new SoulInfo("Tomato", m?.HasReceivedSoul("Tomato Soul") ?? false),
            // new SoulInfo("Pumpkin", m?.HasReceivedSoul("Pumpkin Soul") ?? false),
            // new SoulInfo("Topsoil Bag", m?.HasReceivedSoul("Topsoil Bag Soul") ?? false),
            // new SoulInfo("Quoit", m?.HasReceivedSoul("Quoit Soul") ?? false),
            // new SoulInfo("Plate", m?.HasReceivedSoul("Plate Soul") ?? false),
            // new SoulInfo("Orange", m?.HasReceivedSoul("Orange Soul") ?? false),
            // new SoulInfo("Leek", m?.HasReceivedSoul("Leek Soul") ?? false),
            // new SoulInfo("Cucumber", m?.HasReceivedSoul("Cucumber Soul") ?? false),
            // new SoulInfo("Umbrella", m?.HasReceivedSoul("Umbrella Soul") ?? false),
            // new SoulInfo("Tinned Food", m?.HasReceivedSoul("Tinned Food Soul") ?? false),
            // new SoulInfo("Sock", m?.HasReceivedSoul("Sock Soul") ?? false),
            // new SoulInfo("Pint Bottle", m?.HasReceivedSoul("Pint Bottle Soul") ?? false),
            // new SoulInfo("Knife", m?.HasReceivedSoul("Knife Soul") ?? false),
            // new SoulInfo("Gumboot", m?.HasReceivedSoul("Gumboot Soul") ?? false),
            // new SoulInfo("Fork", m?.HasReceivedSoul("Fork Soul") ?? false),
            // new SoulInfo("Apple Core", m?.HasReceivedSoul("Apple Core Soul") ?? false),
            // new SoulInfo("Apple", m?.HasReceivedSoul("Apple Soul") ?? false),
            // new SoulInfo("Sandwich", m?.HasReceivedSoul("Sandwich Soul") ?? false),
            // new SoulInfo("Bow", m?.HasReceivedSoul("Bow Soul") ?? false),
            // new SoulInfo("Walkie Talkie", m?.HasReceivedSoul("Walkie Talkie Soul") ?? false),
            // new SoulInfo("Boot", m?.HasReceivedSoul("Boot Soul") ?? false),
            // new SoulInfo("Mini Person", m?.HasReceivedSoul("Mini Person Soul") ?? false)
        }; }
    }
}