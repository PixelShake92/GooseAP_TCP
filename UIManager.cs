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
        
        public void ShowNotification(string message)
        {
            currentNotification = message;
            notificationTimer = 4f;
        }
        
        public void AddReceivedItem(string itemName)
        {
            receivedItemNames.Add(itemName);
        }
        
        public void AddChatMessage(string message)
        {
            // Disabled - log removed
        }
        
        public void UpdateNotification(float deltaTime)
        {
            if (notificationTimer > 0)
                notificationTimer -= deltaTime;
        }
        
        public void ClearReceivedItems()
        {
            receivedItemNames.Clear();
        }
        
        public int ReceivedItemCount => receivedItemNames.Count;
        
        public void DrawUI(Plugin plugin)
        {
            // Window dimensions
            float winX = 15;
            float winY = 15;
            float winW = 750;
            float winH = 540;
            
            // Dark background
            GUI.color = new Color(0.12f, 0.12f, 0.12f, 0.97f);
            GUI.DrawTexture(new Rect(winX, winY, winW, winH), Texture2D.whiteTexture);
            
            // Gold border
            GUI.color = new Color(0.7f, 0.55f, 0.2f, 1f);
            GUI.DrawTexture(new Rect(winX, winY, winW, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(winX, winY, 3, winH), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(winX, winY + winH - 3, winW, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(winX + winW - 3, winY, 3, winH), Texture2D.whiteTexture);
            GUI.color = Color.white;
            
            float x = winX + 15;
            float y = winY + 12;
            float w = winW - 30;
            
            // === TITLE ===
            GUI.color = new Color(1f, 0.85f, 0.35f);
            GUI.Label(new Rect(x, y, w, 28), "<size=18><b>ARCHIPELAGO - UNTITLED GOOSE GAME</b></size>");
            GUI.color = Color.white;
            y += 32;
            
            // === CONNECTION ===
            GUI.Label(new Rect(x, y, 55, 24), "Server:");
            ServerAddress = GUI.TextField(new Rect(x + 60, y, 480, 24), ServerAddress);
            GUI.Label(new Rect(x + 545, y, 15, 24), ":");
            ServerPort = GUI.TextField(new Rect(x + 560, y, 100, 24), ServerPort);
            y += 28;
            
            GUI.Label(new Rect(x, y, 55, 24), "Slot:");
            SlotName = GUI.TextField(new Rect(x + 60, y, 600, 24), SlotName);
            y += 28;
            
            GUI.Label(new Rect(x, y, 70, 24), "Password:");
            Password = GUI.PasswordField(new Rect(x + 75, y, 585, 24), Password, '*');
            y += 32;
            
            // Buttons
            if (GUI.Button(new Rect(x, y, 150, 30), plugin.IsConnected ? "Connected!" : "Connect"))
            {
                if (!plugin.IsConnected) plugin.Connect();
            }
            if (GUI.Button(new Rect(x + 160, y, 140, 30), "Disconnect"))
            {
                plugin.Disconnect();
            }
            if (GUI.Button(new Rect(x + 310, y, 140, 30), "Sync Gates"))
            {
                plugin.GateManager?.SyncGatesFromAccessFlags();
                ShowNotification("Gates synced!");
            }
            if (GUI.Button(new Rect(x + 460, y, 100, 30), "Reset"))
            {
                plugin.ResetAllAccess();
                plugin.NPCManager?.RefreshNPCStates(); // Re-apply (all disabled now)
                ShowNotification("Progress reset!");
            }
            y += 36;
            
            // Status line
            GUI.color = plugin.IsConnected ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.5f, 0.3f);
            GUI.Label(new Rect(x, y, w, 22), "<size=13><b>" + Status + "</b></size>");
            GUI.color = Color.white;
            y += 22;
            
            // Notification
            if (notificationTimer > 0 && !string.IsNullOrEmpty(currentNotification))
            {
                GUI.color = new Color(1f, 0.9f, 0.2f);
                GUI.Label(new Rect(x, y, w, 20), "<size=12><b>▶ " + currentNotification + "</b></size>");
                GUI.color = Color.white;
            }
            y += 26;
            
            // === AREA ACCESS ===
            GUI.color = new Color(0.85f, 0.85f, 0.85f);
            GUI.Label(new Rect(x, y, w, 20), "<size=12><b>AREA ACCESS</b></size>");
            GUI.color = Color.white;
            y += 22;
            
            float col1 = x;
            float col2 = x + 240;
            float col3 = x + 480;
            
            DrawAccess(col1, y, "Garden", plugin.HasGardenAccess);
            DrawAccess(col2, y, "High Street", plugin.HasHighStreetAccess);
            DrawAccess(col3, y, "Back Gardens", plugin.HasBackGardensAccess);
            y += 20;
            DrawAccess(col1, y, "Pub", plugin.HasPubAccess);
            DrawAccess(col2, y, "Model Village", plugin.HasModelVillageAccess);
            DrawAccess(col3, y, "Golden Bell", plugin.HasGoldenBell);
            y += 26;
            
            // === BUFFS ===
            GUI.color = new Color(0.85f, 0.85f, 0.85f);
            GUI.Label(new Rect(x, y, w, 20), "<size=12><b>BUFFS & STATUS</b></size>");
            GUI.color = Color.white;
            y += 22;
            
            int speed = (int)(plugin.TrapManager.GetEffectiveSpeedMultiplier() * 100);
            string buffs = $"Speed: {speed}%  |  Silent: {(plugin.IsSilent ? "ON" : "OFF")}  |  Mega Honk: Lv{plugin.MegaHonkCount}";
            GUI.Label(new Rect(x, y, w, 18), buffs);
            y += 20;
            
            string trapText = plugin.TrapManager.GetActiveTrapText();
            if (!string.IsNullOrEmpty(trapText))
            {
                GUI.color = new Color(1f, 0.5f, 0.5f);
                GUI.Label(new Rect(x, y, w, 18), trapText);
                GUI.color = Color.white;
                y += 20;
            }
            
            GUI.Label(new Rect(x, y, w, 18), $"Locations: {plugin.CheckedLocationCount}  |  Items: {receivedItemNames.Count}");
            y += 26;
            
            // === RECENT ITEMS ===
            GUI.color = new Color(0.85f, 0.85f, 0.85f);
            GUI.Label(new Rect(x, y, w, 20), "<size=12><b>RECENT ITEMS</b></size>");
            GUI.color = Color.white;
            y += 22;
            
            int itemsToShow = Math.Min(4, receivedItemNames.Count);
            if (itemsToShow == 0)
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                GUI.Label(new Rect(x + 8, y, w, 18), "No items yet");
                GUI.color = Color.white;
                y += 18;
            }
            else
            {
                for (int i = receivedItemNames.Count - itemsToShow; i < receivedItemNames.Count; i++)
                {
                    GUI.Label(new Rect(x + 8, y, w - 16, 18), "• " + receivedItemNames[i]);
                    y += 18;
                }
            }
            y += 10;
            
            // Controls
            GUI.color = new Color(0.55f, 0.55f, 0.55f);
            GUI.Label(new Rect(x, y, w, 18), "<size=11>G = Goose Day  |  C = Colour  |  Ctrl+C = Reset  |  F9 = Resync Gates</size>");
            GUI.color = Color.white;
        }
        
        private void DrawAccess(float x, float y, string label, bool hasAccess)
        {
            if (hasAccess)
            {
                GUI.color = new Color(0.3f, 1f, 0.3f);
                GUI.Label(new Rect(x, y, 230, 18), "✓ " + label);
            }
            else
            {
                GUI.color = new Color(0.5f, 0.5f, 0.5f);
                GUI.Label(new Rect(x, y, 230, 18), "✗ " + label);
            }
            GUI.color = Color.white;
        }
    }
}