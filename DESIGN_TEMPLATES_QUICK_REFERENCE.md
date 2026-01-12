# Design Templates - Quick Reference Card

## 🎯 What Is This?
A feature that lets dealership admins apply pre-set design themes or create custom templates with one click.

## 📍 Where Is It?
**Admin Panel → Settings → Design Templates** (at the top of the page)

## 🎨 What's Included in a Template?
- Primary Theme Color (header, main elements)
- Secondary Theme Color (buttons, accents)
- Body Background Color
- Website Font

## 🚀 Quick Actions

### Apply a Pre-set Template
```
Dealership Users:
1. Go to Settings
2. Find "Pre-set Templates" section
3. Click "Apply Template" on any card
4. Scroll down
5. Click "Save Changes"

Admin Users:
1. Select dealership from dropdown
2. Go to Settings
3. Find "Pre-set Templates" section
4. Click "Apply Template" on any card
5. Scroll down
6. Click "Save Changes"
```

### Create Custom Template
```
1. Go to Settings
2. Adjust colors and font in the form
3. Click "Save Current as Template"
4. Enter name and description
5. Click "Save Template"
```

### Delete Custom Template
```
1. Go to Settings
2. Find template in "Your Custom Templates"
3. Click red "Delete" button
4. Confirm deletion
```

## 🎨 Available Pre-set Templates

| Template | Primary Color | Font | Best For |
|----------|--------------|------|----------|
| Modern Blue | #3B82F6 | Inter | Professional |
| Classic Black | #000000 | Playfair | Luxury |
| Luxury Gold | #D4AF37 | Montserrat | Premium |
| Sporty Red | #DC2626 | Poppins | Performance |
| Elegant Silver | #71717A | Roboto | Modern |
| Eco Green | #10B981 | Lato | Eco-friendly |
| Premium Navy | #1E3A8A | Open Sans | Corporate |
| Sunset Orange | #F97316 | Nunito | Welcoming |

## 🔐 Permissions Required
- **Permission:** `settings`
- **Default:** Dealership owners have this permission

## 📝 Validation Rules

### Template Name
- ✅ Required
- ✅ Max 100 characters
- ✅ Must be unique (per dealership)

### Colors
- ✅ Must be valid hex (#RGB or #RRGGBB)
- ✅ Examples: #3B82F6, #000, #FFFFFF

### Description
- ✅ Optional
- ✅ Max 500 characters

## 💡 Pro Tips

✨ **Try Before You Buy** - Apply templates without saving to preview  
✨ **Name Clearly** - Use descriptive names like "Summer Sale 2026"  
✨ **Save Variations** - Create multiple templates for different seasons  
✨ **Preset as Base** - Apply a preset, modify it, then save as custom  

## 🔧 API Reference (Developers)

```
GET    /api/design-templates       → List all templates
POST   /api/design-templates       → Create custom template
DELETE /api/design-templates/:id   → Delete custom template
```

## 📖 Full Documentation

- **Quick Start:** `DESIGN_TEMPLATES_QUICK_START.md`
- **Visual Guide:** `DESIGN_TEMPLATES_VISUAL_GUIDE.md`
- **Full Docs:** `DESIGN_TEMPLATES_FEATURE.md`
- **Tech Docs:** `DESIGN_TEMPLATES_IMPLEMENTATION_SUMMARY.md`
- **Index:** `DESIGN_TEMPLATES_DOCS_INDEX.md`

## 🐛 Troubleshooting

**Templates not loading?**
→ Refresh page, check console for errors

**Admin only sees 8 preset templates?**
→ Select a dealership from the dropdown first

**Can't save template?**
→ Check name is unique, colors are valid hex

**Don't see template selector?**
→ You need `settings` permission

**Deleted template still showing?**
→ Refresh the page

**Can't delete template as admin?**
→ Make sure you've selected a dealership from dropdown

**Getting NaN errors?**
→ Update to version 1.1.1 or later, refresh browser

## ❓ FAQ

**Q: Can I edit a template?**  
A: No, create a new one and delete the old one.

**Q: Are templates shared?**  
A: Pre-sets are global. Custom templates are private to your dealership.

**Q: Limit on templates?**  
A: No limit on custom templates!

**Q: Why can't I delete templates as admin?**  
A: Make sure you've selected a dealership from the dropdown. The dealership_id is required for all create/delete operations.

**Q: I'm getting "NaN" errors, what do I do?**  
A: This was fixed in version 1.1.1. Refresh your browser (Ctrl+F5) and ensure you have the latest version.

## 📊 Feature Stats

- ✅ 8 Pre-set Templates
- ✅ Unlimited Custom Templates
- ✅ 14 Font Options
- ✅ 100% Secure & Isolated
- ✅ Mobile Responsive
- ✅ One-Click Application

## 🎉 That's It!

You're now ready to use Design Templates. Start by applying a pre-set template and see the magic happen!

---

**Need more help?** Read the full docs: `DESIGN_TEMPLATES_DOCS_INDEX.md`
