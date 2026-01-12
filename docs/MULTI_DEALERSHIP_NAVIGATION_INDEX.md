# Multi-Dealership Navigation Features - Documentation Index

**Last Updated:** 2025-11-24  
**Features:** Public Dealership Selector (3.6.1) + Admin View Website (3.2.1)

---

## Quick Navigation

### For Understanding the Features

| What You Need | Document |
|---------------|----------|
| **User Story: Public Selector** | [`docs/stories/3.6.1.story.md`](./stories/3.6.1.story.md) |
| **User Story: Admin Button** | [`docs/stories/3.2.1.story.md`](./stories/3.2.1.story.md) |
| **Quick Summary** | [`docs/CHANGELOG-2025-11-24.md`](./CHANGELOG-2025-11-24.md) |
| **Recent Changes** | [`docs/README-FOR-AGENTS.md`](./README-FOR-AGENTS.md) (see top section) |

### For Implementation Details

| What You Need | Document |
|---------------|----------|
| **Technical Architecture** | [`docs/DEALERSHIP_SELECTOR_IMPLEMENTATION.md`](./DEALERSHIP_SELECTOR_IMPLEMENTATION.md) |
| **Admin Button Details** | [`docs/ADMIN_VIEW_WEBSITE_FEATURE.md`](./ADMIN_VIEW_WEBSITE_FEATURE.md) |
| **Epic Context** | [`docs/prd/epic-3-admin-cms-dealership-management-production-deployment.md`](./prd/epic-3-admin-cms-dealership-management-production-deployment.md) |

### For Architecture Reference

| What You Need | Document |
|---------------|----------|
| **Component Documentation** | [`docs/architecture/components.md`](./architecture/components.md) (see DealershipContext and AdminHeader sections) |
| **File Structure** | [`docs/architecture/source-tree.md`](./architecture/source-tree.md) |

---

## Feature Overview

### Story 3.6.1: Public Website Dealership Selector

**What It Does:**
- Adds dropdown in public site header to switch dealerships
- Selection persists across browser sessions
- All pages update without reload

**Key Files:**
- `frontend/src/context/DealershipContext.jsx` - Global state management
- `frontend/src/components/DealershipSelector.jsx` - Dropdown component
- 6 public page components modified to use context

**Usage Pattern:**
```javascript
import { useDealershipContext } from '../../context/DealershipContext';

function MyComponent() {
  const { currentDealershipId } = useDealershipContext();
  // Use currentDealershipId for API calls
}
```

### Story 3.2.1: Admin Panel "View Website" Button

**What It Does:**
- Adds "View Website" button in admin header
- One-click navigation from admin to public site
- Automatically syncs dealership selection between contexts

**Key File:**
- `frontend/src/components/AdminHeader.jsx` - Modified with view website button

**Usage Pattern:**
```javascript
// Syncing contexts for navigation
const handleViewWebsite = () => {
  if (selectedDealership) {
    setCurrentDealershipId(selectedDealership.id); // Sync public context
    navigate('/'); // Navigate to public home
  }
};
```

---

## How They Work Together

```
Admin Panel (AdminContext)
  ├── selectedDealership.id = 2
  └── User clicks "View Website"
       ↓
       ↓ handleViewWebsite() syncs contexts
       ↓
Public Site (DealershipContext)
  ├── currentDealershipId = 2
  └── All pages display dealership 2 data
```

**Context Hierarchy:**
```jsx
<AdminProvider>                     // Admin authentication + selection
  <DealershipProvider>              // Public site selection
    <BrowserRouter>
      <Routes>
        {/* Public and admin routes */}
      </Routes>
    </BrowserRouter>
  </DealershipProvider>
</AdminProvider>
```

---

## Documentation Map

### Story Documents (Complete Implementation Details)
```
docs/stories/
  ├── 3.6.1.story.md    # Public dealership selector
  └── 3.2.1.story.md    # Admin view website button
```

### Implementation Guides (Technical Details)
```
docs/
  ├── DEALERSHIP_SELECTOR_IMPLEMENTATION.md  # Full technical overview
  └── ADMIN_VIEW_WEBSITE_FEATURE.md          # Admin button details
```

### Quick References
```
docs/
  ├── CHANGELOG-2025-11-24.md    # Change summary for both features
  └── README-FOR-AGENTS.md       # Recent changes (top section)
```

### Architecture Documentation
```
docs/architecture/
  ├── components.md       # Component patterns (see DealershipContext, AdminHeader)
  └── source-tree.md      # File locations
```

### Epic Documentation
```
docs/prd/
  └── epic-3-admin-cms-dealership-management-production-deployment.md
      ├── Story 3.2 - Admin Dashboard
      ├── Story 3.2.1 - View Website Button  ← NEW
      ├── Story 3.6 - Dealership Settings
      └── Story 3.6.1 - Public Selector      ← NEW
```

---

## For Future Development

### When Adding New Public Pages

Always use `useDealershipContext()`:

```javascript
import { useDealershipContext } from '../../context/DealershipContext';

function NewPublicPage() {
  const { currentDealershipId } = useDealershipContext();
  
  useEffect(() => {
    // Fetch data using currentDealershipId
    fetchData(currentDealershipId);
  }, [currentDealershipId]); // Re-fetch when dealership changes
  
  // Render content
}
```

### When Adding Admin to Public Navigation

Follow the pattern from AdminHeader:

```javascript
const { setCurrentDealershipId } = useDealershipContext();

const handleNavigateToPublic = (path = '/') => {
  if (selectedDealership) {
    setCurrentDealershipId(selectedDealership.id); // Sync contexts
    navigate(path); // Navigate to public site
  }
};
```

### Context Dependencies

**Both features depend on:**
- `AdminContext` - Admin panel state (Story 3.2)
- `DealershipContext` - Public site state (Story 3.6.1)

**Provider hierarchy must maintain:**
```
AdminProvider > DealershipProvider > BrowserRouter
```

---

## Testing Reference

### What to Test When Modifying

**Public Dealership Selector (3.6.1):**
- Dropdown appears when 2+ dealerships
- Dropdown hidden when 1 dealership
- Selection persists after page refresh
- All pages update when dealership changes
- Mobile responsive layout

**Admin View Website (3.2.1):**
- Button navigates to `/`
- Public site shows correct dealership
- Button disabled when no dealership
- Tooltip displays on hover
- Mobile responsive layout

### Testing Checklist

- [ ] Public selector shows/hides correctly
- [ ] Selection persists in localStorage
- [ ] All public pages use context (not hardcoded IDs)
- [ ] Admin button syncs contexts
- [ ] Navigation works from admin to public
- [ ] Mobile layouts work for both features

---

## Key Patterns to Remember

1. **Public Pages:** Always use `useDealershipContext()`, never hardcode dealership IDs
2. **Context Sync:** Update public context before navigating from admin
3. **localStorage:** Used for persistence (key: `selectedDealershipId`)
4. **Conditional Rendering:** Selector only shows when needed (2+ dealerships)
5. **Provider Order:** AdminProvider → DealershipProvider → BrowserRouter

---

## Related Documentation

- **Story 3.2:** Admin Dashboard & Dealership Selector
- **Story 3.6:** Dealership Settings Management
- **Architecture:** React Context API patterns
- **Tech Stack:** React Router v6 navigation

---

**Questions?** Refer to the detailed story documents or implementation guides linked above.
