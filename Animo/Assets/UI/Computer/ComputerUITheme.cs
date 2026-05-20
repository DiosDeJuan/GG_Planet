// ComputerUITheme — ShopMaster
// Centralizes all visual constants used across computer app UIs:
// UpgradesUIController, EmployeeAppUIController, ExpansionAppUIController,
// and any future computer app.
//
// Usage:
//   Image bg = go.AddComponent<Image>();
//   bg.color = ComputerUITheme.PanelDarkBg;
//
//   TMP_Text label = go.GetComponent<TMP_Text>();
//   label.fontSize = ComputerUITheme.FontBody;
//   label.color    = ComputerUITheme.TextPrimary;

using UnityEngine;

namespace FLOBUK.StoreSimulator
{
    /// <summary>
    /// Static design-token class shared by all computer app UI controllers.
    /// All values are readonly so they act as compile-time constants from the
    /// compiler's perspective (no allocation overhead).
    /// </summary>
    public static class ComputerUITheme
    {
        // ── Background colours ────────────────────────────────────────────────

        /// <summary>Root background — very dark blue/grey used as the outermost panel.</summary>
        public static readonly Color RootBg         = new Color(0.04f, 0.05f, 0.08f, 0.95f);

        /// <summary>Header strip background across all apps.</summary>
        public static readonly Color HeaderBg        = new Color(0.09f, 0.11f, 0.15f, 0.98f);

        /// <summary>Standard dark panel background (list panels, detail panels).</summary>
        public static readonly Color PanelDarkBg     = new Color(0.07f, 0.09f, 0.12f, 0.97f);

        /// <summary>Slightly lighter card background for individual list items.</summary>
        public static readonly Color CardBg          = new Color(0.11f, 0.13f, 0.17f, 0.96f);

        /// <summary>Selected card tint — applied on top of CardBg.</summary>
        public static readonly Color CardSelectedBg  = new Color(0.18f, 0.21f, 0.28f, 1.00f);

        // ── Node / status colours ─────────────────────────────────────────────

        /// <summary>Locked node — dark grey (state [BLOQ]).</summary>
        public static readonly Color NodeLockedBg    = new Color(0.20f, 0.20f, 0.22f, 1.00f);

        /// <summary>Ready-to-unlock node (all requirements met, have points).</summary>
        public static readonly Color NodeReadyBg     = new Color(0.15f, 0.40f, 0.18f, 1.00f);

        /// <summary>Unlocked node — green (state [OK]).</summary>
        public static readonly Color NodeUnlockedBg  = new Color(0.10f, 0.50f, 0.22f, 1.00f);

        // ── Node type accent colours ──────────────────────────────────────────

        /// <summary>Product-type node accent (slightly warm).</summary>
        public static readonly Color NodeAccentProduct  = new Color(0.55f, 0.38f, 0.10f, 1.00f);

        /// <summary>Employee-type node accent — blue-purple so it stands out.</summary>
        public static readonly Color NodeAccentEmployee = new Color(0.22f, 0.28f, 0.60f, 1.00f);

        /// <summary>Security-type node accent — red-orange.</summary>
        public static readonly Color NodeAccentSecurity = new Color(0.55f, 0.15f, 0.15f, 1.00f);

        /// <summary>Improvement-type node accent — teal.</summary>
        public static readonly Color NodeAccentImprovement = new Color(0.10f, 0.38f, 0.45f, 1.00f);

        // ── Button colours ────────────────────────────────────────────────────

        /// <summary>Primary action button — asset-brand fucsia/pink.</summary>
        public static readonly Color ButtonPrimary   = new Color(0.87f, 0.26f, 0.56f, 1.00f);

        /// <summary>Secondary / navigation button — dark blue.</summary>
        public static readonly Color ButtonSecondary = new Color(0.18f, 0.28f, 0.42f, 1.00f);

        /// <summary>Positive / unlock / hire button — green.</summary>
        public static readonly Color ButtonPositive  = new Color(0.14f, 0.44f, 0.20f, 1.00f);

        /// <summary>Danger / irreversible button — red.</summary>
        public static readonly Color ButtonDanger    = new Color(0.60f, 0.08f, 0.08f, 1.00f);

        /// <summary>Disabled button background.</summary>
        public static readonly Color ButtonDisabled  = new Color(0.22f, 0.22f, 0.24f, 1.00f);

        // ── Text colours ──────────────────────────────────────────────────────

        /// <summary>Primary text — near white.</summary>
        public static readonly Color TextPrimary     = new Color(0.92f, 0.92f, 0.94f, 1.00f);

        /// <summary>Secondary / subtitle text — light grey.</summary>
        public static readonly Color TextSecondary   = new Color(0.68f, 0.68f, 0.72f, 1.00f);

        /// <summary>Muted text — dimmer for hints / descriptions.</summary>
        public static readonly Color TextMuted       = new Color(0.50f, 0.50f, 0.54f, 1.00f);

        /// <summary>Warning / message text — yellow-amber.</summary>
        public static readonly Color TextWarning     = new Color(1.00f, 0.85f, 0.25f, 1.00f);

        /// <summary>Success / positive text — light green.</summary>
        public static readonly Color TextSuccess     = new Color(0.30f, 0.82f, 0.45f, 1.00f);

        /// <summary>Error / danger text — salmon red.</summary>
        public static readonly Color TextDanger      = new Color(1.00f, 0.38f, 0.38f, 1.00f);

        // ── Status-state colours (used in employee / node status labels) ──────

        /// <summary>[BLOQ] — locked state: dark grey.</summary>
        public static readonly Color StatusBlocked   = new Color(0.40f, 0.40f, 0.42f, 1.00f);

        /// <summary>[DISP] — available state: warm yellow-orange.</summary>
        public static readonly Color StatusAvailable = new Color(0.80f, 0.65f, 0.10f, 1.00f);

        /// <summary>[OK] / hired — green.</summary>
        public static readonly Color StatusOk        = new Color(0.17f, 0.50f, 0.28f, 1.00f);

        /// <summary>[SIN PUESTO] — no workstation: orange.</summary>
        public static readonly Color StatusNoStation = new Color(0.75f, 0.40f, 0.10f, 1.00f);

        /// <summary>[TRABAJANDO] — actively working: blue.</summary>
        public static readonly Color StatusWorking   = new Color(0.12f, 0.42f, 0.72f, 1.00f);

        // ── Font sizes ────────────────────────────────────────────────────────

        /// <summary>Large header / app title.</summary>
        public const float FontTitle    = 28f;

        /// <summary>Section header.</summary>
        public const float FontHeader   = 22f;

        /// <summary>Primary body text.</summary>
        public const float FontBody     = 18f;

        /// <summary>Secondary / detail text.</summary>
        public const float FontSmall    = 15f;

        /// <summary>Tiny caption / legend text.</summary>
        public const float FontCaption  = 12f;

        // ── Metrics ───────────────────────────────────────────────────────────

        /// <summary>Standard button height in pixels.</summary>
        public const float ButtonHeight  = 52f;

        /// <summary>Standard card/row height in pixels.</summary>
        public const float CardHeight    = 48f;

        /// <summary>Standard header strip height in pixels.</summary>
        public const float HeaderHeight  = 68f;

        /// <summary>Standard inner padding for panels (left/right).</summary>
        public const float PanelPadding  = 16f;

        // ── Status label ASCII text ───────────────────────────────────────────

        /// <summary>ASCII label for locked node / employee.</summary>
        public const string LabelBlocked   = "[BLOQ]";

        /// <summary>ASCII label for ready / available node.</summary>
        public const string LabelReady     = "[LISTO]";

        /// <summary>ASCII label for unlocked / hired node.</summary>
        public const string LabelOk        = "[OK]";

        /// <summary>ASCII label for employee with no workstation assigned.</summary>
        public const string LabelNoStation = "[SIN PUESTO]";

        /// <summary>ASCII label for employee actively working.</summary>
        public const string LabelWorking   = "[TRABAJANDO]";

        /// <summary>ASCII label for purchased expansion zone.</summary>
        public const string LabelPurchased = "[COMPRADO]";

        /// <summary>ASCII label for insufficient funds.</summary>
        public const string LabelNoFunds   = "[SIN FONDOS]";

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the accent color for a tree node based on its type.
        /// Used to tint the left/top border stripe on node cards.
        /// </summary>
        public static Color GetNodeAccent(TreeNodeType type)
        {
            switch (type)
            {
                case TreeNodeType.Employee:    return NodeAccentEmployee;
                case TreeNodeType.Security:    return NodeAccentSecurity;
                case TreeNodeType.Improvement: return NodeAccentImprovement;
                default:                       return NodeAccentProduct;
            }
        }

        /// <summary>
        /// Returns the background tint for a node card given its unlock state.
        /// </summary>
        public static Color GetNodeBg(bool isUnlocked, bool canUnlock)
        {
            if (isUnlocked)  return NodeUnlockedBg;
            if (canUnlock)   return NodeReadyBg;
            return NodeLockedBg;
        }

        /// <summary>
        /// Returns a short ASCII status string for a node.
        /// </summary>
        public static string GetNodeStatusLabel(bool isUnlocked, bool canUnlock)
        {
            if (isUnlocked) return LabelOk;
            if (canUnlock)  return LabelReady;
            return LabelBlocked;
        }
    }
}
