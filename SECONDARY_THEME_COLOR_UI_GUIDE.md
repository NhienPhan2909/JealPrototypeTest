# Secondary Theme Color - Admin UI Guide

## Location
**Admin Panel → Dealership Settings → Secondary Theme Color section**

## UI Components Added

The secondary theme color section appears immediately after the primary theme color section in the Dealership Settings page.

### Layout Structure

```
┌─────────────────────────────────────────────────────────────┐
│ Secondary Theme Color                                        │
│                                                              │
│ Choose a secondary color for your dealership's branding.    │
│ This will be used for buttons, accents, and complementary   │
│ elements.                                                    │
│                                                              │
│ ┌──────┐  ┌─────────────┐  ┌───────────────────┐          │
│ │ 🎨   │  │  #6B7280   │  │ Reset to Default  │          │
│ └──────┘  └─────────────┘  └───────────────────┘          │
│                                                              │
│ Hex color code (e.g., #6B7280)                             │
│                                                              │
│ Preview:                                                     │
│ ┌──────────────┐  ┌───────────────────────────────┐        │
│ │Button Preview│  │ Accent Text Preview           │        │
│ └──────────────┘  └───────────────────────────────┘        │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Component Details

### 1. Label & Description
- **Label**: "Secondary Theme Color"
- **Description**: Clear explanation of where the color is used
- **Font**: Medium weight, consistent with other form labels

### 2. Color Picker Controls
Three interactive elements in a horizontal layout:

#### a) Color Widget (Left)
- HTML5 color input
- Visual color picker interface
- Dimensions: 48px height × 80px width
- Rounded corners with border
- Click to open browser's native color picker

#### b) Hex Text Input (Center)
- Text input field for direct hex code entry
- Placeholder: "#6B7280"
- Width: 128px (w-32)
- Pattern validation: `^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$`
- Real-time sync with color widget
- Helper text below: "Hex color code (e.g., #6B7280)"

#### c) Reset Button (Right)
- Button text: "Reset to Default"
- Type: Secondary button style
- Action: Sets color back to #6B7280
- Small text size for less prominence

### 3. Live Preview Section
Shows how the color will look in actual use:

#### a) Button Preview (Left)
- Styled button with text "Button Preview"
- Background: Selected secondary color
- Text: White, bold
- Padding: px-4 py-2
- Rounded corners
- Non-functional (type="button")

#### b) Accent Preview (Right)
- Bordered container showing accent usage
- Border: 2px solid in secondary color
- Text color: Secondary color
- Content: "Accent Text Preview"
- Flex-1 width to fill remaining space

## User Interactions

### Changing Color
1. **Via Color Picker**: Click the color widget → Choose color → Color updates instantly
2. **Via Hex Input**: Type/paste hex code → Color updates on blur or Enter
3. **Via Reset**: Click "Reset to Default" → Color returns to #6B7280

### Saving Changes
1. Make color selection using any method above
2. Scroll to bottom of form
3. Click "Save Settings" button
4. Success message appears
5. Color persists across page reloads

## Visual Hierarchy

The secondary theme color section follows this pattern:

```
Dealership Name
  └─ Text input

Primary Theme Color ← Existing
  └─ Color picker + preview

Secondary Theme Color ← NEW
  └─ Color picker + preview

Website Font
  └─ Dropdown select

Logo Upload
  └─ Upload button/image

(etc...)
```

## Color Coordination Tip

The UI is designed to encourage good color coordination:
- Primary and secondary colors shown close together
- Both have similar preview formats
- Easy to compare how they work together
- Visual previews show realistic usage

## Responsive Design

The layout adapts to different screen sizes:
- **Desktop**: All controls in one row
- **Tablet**: Controls may stack vertically
- **Mobile**: Full vertical stack for better touch targets

## Accessibility Features

- Color picker is keyboard accessible
- Labels properly associated with inputs
- Helper text provides format guidance
- Preview shows visual representation
- Text input allows manual entry for precision

## Example Color Schemes

### Professional
- Primary: #1E40AF (deep blue)
- Secondary: #6B7280 (neutral gray)

### Vibrant
- Primary: #DC2626 (red)
- Secondary: #F59E0B (amber)

### Eco-Friendly
- Primary: #047857 (green)
- Secondary: #78716C (warm gray)

### Luxury
- Primary: #1F2937 (dark gray)
- Secondary: #D97706 (gold)

## Technical Notes

### State Management
```javascript
const [secondaryThemeColor, setSecondaryThemeColor] = useState('#6B7280');
```

### Form Submission
```javascript
const dealershipData = {
  // ... other fields
  secondary_theme_color: secondaryThemeColor,
};
```

### Validation
- Pattern: `^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$`
- Backend validates format and rejects invalid colors
- Frontend provides instant visual feedback

## Where Secondary Color is Used

Once saved, the secondary theme color is available throughout the website via CSS variables:

### CSS Variables Set
```css
--secondary-theme-color: #6B7280;
--secondary-theme-color-dark: #4B5563;  /* 15% darker */
--secondary-theme-color-light: #F3F4F6; /* 90% lighter */
```

### Usage in Components
Developers can use these variables for:
- Button backgrounds
- Border colors
- Text colors
- Icon colors
- Hover states
- Accent elements
