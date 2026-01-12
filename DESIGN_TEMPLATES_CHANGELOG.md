# Design Templates - Changelog

## Version 1.1.1 (2026-01-09)

### Bug Fixes
- 🐛 **CRITICAL FIX:** Fixed template deletion failing with NaN error for admin users
- 🐛 **CRITICAL FIX:** Fixed template creation not including dealership_id for admin users
- 🐛 Added validation to prevent NaN being passed to database

### Changes
**Frontend (`frontend/src/components/admin/TemplateSelector.jsx`):**
- Added `dealership_id` to DELETE request URL as query parameter
- Added `dealership_id` to POST request body
- Ensures admin users can properly create and delete templates

**Backend (`backend/routes/designTemplates.js`):**
- Added `dealership_id` validation in POST route (prevents NaN)
- Added `dealership_id` validation in DELETE route (prevents NaN)
- Better error messages when dealership_id is missing

### Impact
- **Before:** Admin users could not delete or create templates (NaN database error)
- **After:** Admin users can successfully create and delete templates for selected dealership

### Files Modified
- `frontend/src/components/admin/TemplateSelector.jsx`
- `backend/routes/designTemplates.js`

---

## Version 1.1.0 (2026-01-09)

### Features Added
- ✅ Multi-tenant support for admin users
- ✅ Query parameter support for dealership_id
- ✅ Admin users can now view/manage templates for any dealership

### Bug Fixes
- 🐛 Fixed authentication to use `req.session.user` instead of `req.user`
- 🐛 Fixed admin users only seeing preset templates
- 🐛 Fixed dealership_id not being passed from frontend

### Changes
**Backend (`backend/routes/designTemplates.js`):**
- Updated GET route to accept `dealership_id` as query parameter
- Updated POST route to accept `dealership_id` from query/body
- Updated DELETE route to accept `dealership_id` as query parameter
- Removed `enforceDealershipScope` middleware (handled manually now)
- Added fallback to show only presets when no dealership selected

**Frontend (`frontend/src/components/admin/TemplateSelector.jsx`):**
- Added `dealership_id` query parameter to API calls
- Uses `selectedDealership.id` from AdminContext

### Behavior
**Before:**
- Admin users: Only saw 8 preset templates
- Dealership users: Saw presets + their custom templates
- No way for admin to view dealership-specific templates

**After:**
- Admin users (with dealership selected): See presets + that dealership's custom templates
- Admin users (no dealership): See only presets
- Dealership users: Unchanged - see presets + their custom templates

### Documentation Updates
Updated all documentation files:
- ✅ DESIGN_TEMPLATES_FEATURE.md - Added query param docs, admin behavior
- ✅ DESIGN_TEMPLATES_QUICK_START.md - Added admin user instructions
- ✅ DESIGN_TEMPLATES_IMPLEMENTATION_SUMMARY.md - Updated security, testing sections
- ✅ DESIGN_TEMPLATES_BUGFIX_AUTH.md - Added multi-tenant fix details
- ✅ DESIGN_TEMPLATES_README.md - Added admin user workflow
- ✅ DESIGN_TEMPLATES_QUICK_REFERENCE.md - Added admin troubleshooting
- ✅ DESIGN_TEMPLATES_DOCS_INDEX.md - Updated file counts
- ✅ Created DESIGN_TEMPLATES_MULTI_TENANT_BEHAVIOR.md - Explains visibility rules
- ✅ Created DESIGN_TEMPLATES_CHANGELOG.md - This file

### Migration Required
None - Database schema unchanged

### Breaking Changes
None - Backward compatible

---

## Version 1.0.0 (2026-01-09)

### Initial Release
- ✅ 8 professional pre-set templates
- ✅ Unlimited custom templates per dealership
- ✅ CRUD operations via API
- ✅ Multi-tenant data isolation
- ✅ Permission-based access control
- ✅ Comprehensive documentation
- ✅ Automated test suite
- ✅ Responsive UI component

### Components
- Database migration: `011_add_design_templates.sql`
- Database queries: `designTemplates.js`
- API routes: `routes/designTemplates.js`
- Frontend component: `TemplateSelector.jsx`
- Test suite: `test_design_templates.js`

### Pre-set Templates
1. Modern Blue (#3B82F6 / Inter)
2. Classic Black (#000000 / Playfair)
3. Luxury Gold (#D4AF37 / Montserrat)
4. Sporty Red (#DC2626 / Poppins)
5. Elegant Silver (#71717A / Roboto)
6. Eco Green (#10B981 / Lato)
7. Premium Navy (#1E3A8A / Open Sans)
8. Sunset Orange (#F97316 / Nunito)

---

**Last Updated:** 2026-01-09 01:30 UTC  
**Current Version:** 1.1.0  
**Status:** Production Ready
