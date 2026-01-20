# Website URL Management - Visual UI Guide

**Guide to the User Interface Changes**

---

## 📍 Location 1: Admin Settings Page

**Path:** Admin Panel → Settings → Website URL Field

### Field Location
The "Website URL" field appears between the "Dealership Name" and "Primary Theme Color" sections.

### Field Details
```
┌────────────────────────────────────────────────────────────┐
│ Dealership Settings                                         │
├────────────────────────────────────────────────────────────┤
│                                                             │
│ Dealership Name *                                           │
│ ┌─────────────────────────────────────────────────────┐    │
│ │ Acme Auto Sales                                     │    │
│ └─────────────────────────────────────────────────────┘    │
│                                                             │
│ Website URL                                                 │
│ Custom URL/domain for this dealership's website            │
│ (e.g., acme-auto.com). Leave empty if not applicable.      │
│ ┌─────────────────────────────────────────────────────┐    │
│ │ acme-auto.com                                       │    │
│ └─────────────────────────────────────────────────────┘    │
│ This URL will be used to identify your dealership's        │
│ website. It must be unique across all dealerships.         │
│                                                             │
│ Primary Theme Color                                         │
│ Choose a primary color for your dealership's branding...   │
│ ...                                                         │
└────────────────────────────────────────────────────────────┘
```

### Features
- ✅ Text input field with placeholder "e.g., acme-auto.com"
- ✅ Helper text explaining the field's purpose
- ✅ Character limit: 255 characters
- ✅ Optional field (can be left empty)
- ✅ Read-only for users without settings permission
- ✅ Saves when "Update Dealership Settings" button is clicked

---

## 📍 Location 2: Dealership Management Page - Create Form

**Path:** Admin Panel → Dealerships → Create New Dealership Button

### Create Form
```
┌────────────────────────────────────────────────────────────┐
│ Create New Dealership                                       │
├────────────────────────────────────────────────────────────┤
│                                                             │
│ Dealership Name *                                           │
│ ┌─────────────────────────────────────────────────────┐    │
│ │ e.g., Acme Auto Sales                               │    │
│ └─────────────────────────────────────────────────────┘    │
│                                                             │
│ Address *                                                   │
│ ┌─────────────────────────────────────────────────────┐    │
│ │ 123 Main Street, City, State ZIP                    │    │
│ └─────────────────────────────────────────────────────┘    │
│                                                             │
│ Phone *                                                     │
│ ┌─────────────────────────────────────────────────────┐    │
│ │ (555) 123-4567                                      │    │
│ └─────────────────────────────────────────────────────┘    │
│                                                             │
│ Email *                                                     │
│ ┌─────────────────────────────────────────────────────┐    │
│ │ contact@dealership.com                              │    │
│ └─────────────────────────────────────────────────────┘    │
│                                                             │
│ Website URL (Optional)                                      │
│ ┌─────────────────────────────────────────────────────┐    │
│ │ e.g., acme-auto.com                                 │    │
│ └─────────────────────────────────────────────────────┘    │
│ Custom URL/domain for this dealership. Must be unique.     │
│                                                             │
│ Logo URL (Optional)                                         │
│ ┌─────────────────────────────────────────────────────┐    │
│ │ https://example.com/logo.png                        │    │
│ └─────────────────────────────────────────────────────┘    │
│                                                             │
│ ...more fields...                                           │
│                                                             │
│           [ Cancel ]  [ Create Dealership ]                 │
└────────────────────────────────────────────────────────────┘
```

### Features
- ✅ Appears after Email field, before Logo URL
- ✅ Optional field (not required)
- ✅ Placeholder shows example URL
- ✅ Helper text explains uniqueness requirement
- ✅ Validates on submit (max 255 chars, uniqueness)

---

## 📍 Location 3: Dealership Management Page - List Table

**Path:** Admin Panel → Dealerships → Dealerships List

### Table View
```
┌────────────────────────────────────────────────────────────────────────────────────┐
│ All Dealerships (2)                                                                 │
├────┬──────────────────┬──────────────────┬─────────────────────┬──────────┬────────┤
│ ID │ Name             │ Website URL      │ Email               │ Phone    │ Actions│
├────┼──────────────────┼──────────────────┼─────────────────────┼──────────┼────────┤
│ 1  │ Acme Auto Sales  │ acme-auto.com    │ info@acme.com       │ 555-1234 │ Delete │
├────┼──────────────────┼──────────────────┼─────────────────────┼──────────┼────────┤
│ 2  │ Premier Motors   │ Not set          │ info@premier.com    │ 555-5678 │ Delete │
└────┴──────────────────┴──────────────────┴─────────────────────┴──────────┴────────┘
```

### Column Details
- **Column Name:** "Website URL"
- **Position:** Between "Name" and "Email" columns
- **Display Logic:**
  - If URL is set: Shows the actual URL (e.g., "acme-auto.com")
  - If URL is NULL: Shows "Not set" in italic, gray text

### Features
- ✅ New column added to table
- ✅ Clearly shows which dealerships have URLs
- ✅ "Not set" styling: italic, gray color
- ✅ URL styling: normal, black text

---

## 🎨 Styling Details

### Settings Page Field
```css
Input Field:
- Width: 100% (full width)
- Padding: Standard input padding
- Border: Standard input border
- Max Length: 255 characters
- Placeholder: "e.g., acme-auto.com"

Helper Text:
- Font Size: Small (xs)
- Color: Gray-500
- Margin Top: 1 (4px)
```

### Management Page Create Form
```css
Input Field:
- Border: 1px solid gray-300
- Border Radius: Standard rounded
- Padding: 12px (px-3 py-2)
- Width: 100%
- Max Length: 255 characters

Helper Text:
- Font Size: Extra small (xs)
- Color: Gray-500
- Margin Top: 4px
```

### Management Page Table
```css
Website URL Column:
- Text Alignment: Left
- Padding: 24px horizontal, 16px vertical
- Font Size: Small (sm)
- White Space: No wrap

"Not set" Text:
- Color: Gray-400
- Font Style: Italic
```

---

## 📱 Responsive Behavior

### Desktop (≥1024px)
- All fields display at full width
- Table shows all columns
- No horizontal scrolling needed

### Tablet (768px - 1023px)
- Fields remain full width
- Table becomes scrollable horizontally
- Website URL column visible in scroll

### Mobile (<768px)
- Fields stack vertically
- Table requires horizontal scroll
- Website URL column accessible via scroll

---

## 🔐 Permission-Based Display

### System Administrator
- ✅ Can view Website URL field
- ✅ Can edit Website URL field
- ✅ Can create dealerships with URLs
- ✅ Can see Website URL column in table

### Dealership Owner (with settings permission)
- ✅ Can view Website URL field
- ✅ Can edit Website URL field
- ❌ Cannot access Dealership Management page

### Staff (without settings permission)
- ✅ Can view Website URL field (read-only)
- ❌ Cannot edit Website URL field
- ❌ Cannot access Dealership Management page

---

## 💡 User Experience Highlights

### Discovery
- Field is clearly labeled "Website URL"
- Helper text explains what it's for
- Placeholder shows example format
- Optional nature is clear (not marked with *)

### Input
- Text field accepts any string up to 255 chars
- No client-side format validation
- Saves with other dealership settings
- No separate save button needed

### Feedback
- Success message on save: "Dealership settings updated successfully!"
- Error message if URL already exists: "Failed to update dealership"
- "Not set" display for empty values in table
- Clear visual distinction between set/unset values

### Accessibility
- Proper label association
- Helper text for screen readers
- Keyboard navigation support
- Focus states on inputs

---

## 📊 Visual Comparison

### Before Implementation
```
Settings Page:
- Dealership Name field
- [THEME COLOR PICKER] <-- Website URL goes here
- Secondary Theme Color
...

Management Table:
┌────┬──────────────────┬─────────────────────┬──────────┐
│ ID │ Name             │ Email               │ Actions  │
└────┴──────────────────┴─────────────────────┴──────────┘
```

### After Implementation
```
Settings Page:
- Dealership Name field
- [WEBSITE URL FIELD]  <-- NEW!
- Theme Color Picker
- Secondary Theme Color
...

Management Table:
┌────┬──────────────────┬──────────────────┬──────────────┬──────────┐
│ ID │ Name             │ Website URL      │ Email        │ Actions  │
│    │                  │ <-- NEW COLUMN!  │              │          │
└────┴──────────────────┴──────────────────┴──────────────┴──────────┘
```

---

## 🎯 Key Visual Elements

### Field Placement Logic
The Website URL field was placed:
- **After** Dealership Name (logical grouping - both identify the dealership)
- **Before** Theme Colors (visual settings come after basic info)
- **In the basic settings section** (not hidden in advanced settings)

### Table Column Order
The Website URL column was placed:
- **After** Name (shows what the dealership is called, then where it's located)
- **Before** Email (contact info comes after identification)
- **Between identity and contact fields** (logical progression)

---

## ✅ Visual Checklist

When viewing the UI, you should see:

**Settings Page:**
- [ ] Website URL field appears
- [ ] Field is between Name and Theme Color
- [ ] Placeholder text shows "e.g., acme-auto.com"
- [ ] Helper text explains uniqueness
- [ ] Field loads existing URL value
- [ ] Field saves with form submission

**Management Page - Create Form:**
- [ ] Website URL field in create form
- [ ] Field appears after Email, before Logo URL
- [ ] Field is optional (no asterisk)
- [ ] Helper text appears below field
- [ ] Placeholder shows example

**Management Page - Table:**
- [ ] Website URL column appears
- [ ] Column is between Name and Email
- [ ] Shows actual URLs when set
- [ ] Shows "Not set" (italic, gray) when NULL
- [ ] Column header says "Website URL"

---

**End of Visual Guide**
