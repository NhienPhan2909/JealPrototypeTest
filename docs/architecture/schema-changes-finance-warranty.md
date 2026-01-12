# Database Schema Changes: Finance & Warranty Policy Fields

**Date:** 2025-11-27
**Related Stories:** 2.8, 2.9, 3.8
**Epic:** Epic 2 & Epic 3 Enhancement

## Overview

Added two new TEXT fields to the `dealership` table to support Finance and Warranty policy pages on the public website, manageable through the admin CMS.

## Schema Changes

### Dealership Table - New Fields

```sql
ALTER TABLE dealership
  ADD COLUMN IF NOT EXISTS finance_policy TEXT,
  ADD COLUMN IF NOT EXISTS warranty_policy TEXT;
```

| Field | Type | Nullable | Purpose |
|-------|------|----------|---------|
| `finance_policy` | TEXT | Yes | Financing options and policy content displayed on `/finance` page |
| `warranty_policy` | TEXT | Yes | Warranty coverage and terms displayed on `/warranty` page |

**Rationale for TEXT type:**
- Supports multi-paragraph content (no length restrictions)
- Allows line breaks and formatting
- Nullable to support dealerships that haven't set policies yet

## API Impact

### Affected Endpoints

**GET /api/dealers/:id**
- **Change:** Response now includes `finance_policy` and `warranty_policy` fields
- **Backward Compatible:** Yes (new fields are nullable)

**PUT /api/dealers/:id**
- **Change:** Request body may now include `finance_policy` and `warranty_policy` fields
- **Backward Compatible:** Yes (fields are optional)

### Example Response

```json
{
  "id": 1,
  "name": "Acme Auto Sales",
  "logo_url": "https://...",
  "address": "123 Main St...",
  "phone": "(555) 123-4567",
  "email": "sales@acmeauto.com",
  "hours": "Mon-Fri: 9:00 AM - 6:00 PM...",
  "about": "Family-owned dealership...",
  "finance_policy": "We offer flexible financing options...",
  "warranty_policy": "All vehicles come with a 30-day limited warranty...",
  "created_at": "2025-01-15T10:30:00Z"
}
```

## Migration Steps

### For New Databases
Use updated `backend/db/schema.sql` which includes the new fields in CREATE TABLE statement.

### For Existing Databases

1. **Run migration script:**
   ```bash
   railway run psql $DATABASE_URL < backend/db/migrations/add_finance_warranty_fields.sql
   ```

2. **Verify migration:**
   ```sql
   SELECT id, name,
     CASE WHEN finance_policy IS NULL THEN 'Not Set' ELSE 'Set' END as finance_status,
     CASE WHEN warranty_policy IS NULL THEN 'Not Set' ELSE 'Set' END as warranty_status
   FROM dealership;
   ```

3. **Optional:** Populate existing dealerships with default content via admin CMS

### Rollback (if needed)
```sql
ALTER TABLE dealership
  DROP COLUMN IF EXISTS finance_policy,
  DROP COLUMN IF EXISTS warranty_policy;
```

## Data Model Updates

**TypeScript Interface (documentation):**

```typescript
interface Dealership {
  id: number;
  name: string;
  logo_url: string | null;
  address: string;
  phone: string;
  email: string;
  hours: string | null;
  about: string | null;
  finance_policy: string | null;  // NEW
  warranty_policy: string | null; // NEW
  created_at: Date;
}
```

## Frontend Impact

### New Public Routes
- `/finance` - Displays dealership's financing policy
- `/warranty` - Displays dealership's warranty policy

### Updated Components
- **Header/Navigation:** Add "Finance" and "Warranty" links
- **DealershipSettings Form (Admin):** Add textarea inputs for both policies
- **Finance Page:** New component to display finance_policy content
- **Warranty Page:** New component to display warranty_policy content

## Testing Checklist

- [ ] Migration runs successfully on development database
- [ ] GET /api/dealers/:id returns new fields (null for existing data)
- [ ] PUT /api/dealers/:id accepts and saves new fields
- [ ] Admin CMS form displays and saves finance_policy and warranty_policy
- [ ] Public Finance page displays content or default message
- [ ] Public Warranty page displays content or default message
- [ ] Header navigation includes Finance and Warranty links
- [ ] Line breaks preserved when displaying policy content
- [ ] Mobile-responsive layout for new pages

## Security Considerations

- **No new security concerns:** Fields follow same pattern as existing `about` field
- **Input sanitization:** Apply same XSS prevention as other TEXT fields (HTML escaping)
- **No sensitive data:** Policy content is public-facing information

## Performance Impact

**Minimal:**
- Two TEXT fields add negligible storage overhead
- No new indexes required (no query filtering on these fields)
- Fields loaded as part of existing dealership queries (no additional DB calls)

---

**Estimated Implementation Time:** 2-3 hours (migration + frontend changes)

**Documentation Updated:**
- ✅ `database-schema.md` - Schema and seed data
- ✅ `data-models.md` - Dealership model definition
- ✅ Migration script created
- ✅ This change summary document
