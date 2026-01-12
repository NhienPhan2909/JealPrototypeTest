# Design Templates - Multi-Tenant Behavior Explained

## Template Visibility Rules

### Pre-set Templates (8 total)
- ✅ **Global** - Visible to ALL users (admin and dealership users)
- ✅ **Read-only** - Cannot be modified or deleted
- ✅ **Always visible** - Show up for everyone

### Custom Templates
- ✅ **Dealership-specific** - Each dealership has their own private custom templates
- ✅ **Not shared** - Dealership A cannot see Dealership B's custom templates
- ✅ **Manageable** - Can be created and deleted by users with `settings` permission

## User Types and What They See

### Dealership Owner / Staff (e.g., "Acme Owner")
**What you see:**
- 8 Pre-set Templates (in "Pre-set Templates" section)
- Your dealership's Custom Templates (in "Your Custom Templates" section)

**Example:** Acme Owner sees:
- 8 pre-set templates
- 1 custom template ("Test Template Save") created for Acme

### Admin User
**What you see:**
- Depends on selected dealership in admin dropdown

**When dealership is selected (e.g., Acme):**
- 8 Pre-set Templates
- That dealership's Custom Templates

**When no dealership selected:**
- 8 Pre-set Templates only

## How It Works Technically

### Frontend
- Passes `dealership_id` as query parameter: `/api/design-templates?dealership_id=1`
- Uses `selectedDealership.id` from AdminContext

### Backend
```javascript
// Get dealership_id from:
// 1. User session (for dealership users)
// 2. Query parameter (for admin users)
const dealershipId = user.dealership_id || parseInt(req.query.dealership_id);

// If no dealership_id, return only presets
if (!dealershipId) {
  return res.json(presetTemplates);
}

// Otherwise, return presets + custom templates for that dealership
return res.json(getAllTemplatesForDealership(dealershipId));
```

### Database
```sql
-- Preset templates: dealership_id = NULL, is_preset = true
-- Custom templates: dealership_id = 1, is_preset = false

SELECT * FROM design_templates 
WHERE is_preset = true 
   OR dealership_id = $1
ORDER BY is_preset DESC, name ASC;
```

## Example Scenarios

### Scenario 1: Acme Owner Creates Template
1. **Login:** Acme Owner logs in
2. **Create:** Saves "Summer Sale 2026" template
3. **Database:** Template saved with `dealership_id = 1` (Acme)
4. **Visibility:** Only Acme users can see it

### Scenario 2: Admin Views Acme Templates
1. **Login:** Admin logs in
2. **Select:** Chooses "Acme Auto Sales" from dealership dropdown
3. **Navigate:** Goes to Settings page
4. **Sees:** 8 presets + Acme's custom templates

### Scenario 3: Different Dealerships
1. **Acme:** Creates "Acme Special" template
2. **Beta Motors:** Creates "Beta Promo" template
3. **Result:** Each sees only their own custom template

## Testing Verification

You can verify this is working by:

1. **Log in as Acme Owner**
   - Create a custom template
   - Should see it in "Your Custom Templates"

2. **Log out and log in as Admin**
   - Select Acme from dealership dropdown
   - Go to Settings
   - Should see 8 presets + the Acme custom template

3. **Check database:**
```bash
docker exec -it jeal-prototype-db psql -U postgres -d jeal_prototype -c "SELECT id, name, dealership_id, is_preset FROM design_templates;"
```

## Summary

✅ **This is correct behavior!**

- Pre-set templates are global
- Custom templates are dealership-specific
- Admin can view any dealership's templates by selecting that dealership
- Each dealership's custom templates are private and isolated

This ensures proper **multi-tenant data isolation** - a key security feature of the system.

---

**Date:** 2026-01-09  
**Feature:** Design Templates Multi-Tenancy  
**Status:** Working as designed
