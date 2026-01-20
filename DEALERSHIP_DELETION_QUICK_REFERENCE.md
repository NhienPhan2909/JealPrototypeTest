# Dealership Deletion - Quick Reference

## ⚠️ CRITICAL: Read Before Deleting

**DELETION IS PERMANENT AND IRREVERSIBLE**

Deleting a dealership will delete:
- ❌ The dealership record
- ❌ ALL vehicles (inventory)
- ❌ ALL customer leads
- ❌ ALL sales requests
- ❌ ALL blog posts
- ❌ ALL user accounts (owners and staff)

**This data CANNOT be recovered.**

---

## How to Delete a Dealership

### Step 1: Navigate to Dealership Management
1. Log in as admin (`admin`/`admin123`)
2. Click **"Dealership Management"** in navigation

### Step 2: Locate the Dealership
- Find the dealership in the table
- Look in the **Actions** column

### Step 3: Click Delete
- Click the red **"Delete"** button
- A confirmation dialog appears

### Step 4: Confirm Deletion
The dialog shows:
```
⚠️ WARNING: This action is IRREVERSIBLE!

Deleting "[Dealership Name]" will permanently delete:
• The dealership record
• ALL vehicles
• ALL leads and sales requests
• ALL blog posts
• ALL user accounts (owners and staff)

This data CANNOT be recovered.

Type the dealership name to confirm deletion:
```

**You must type the EXACT dealership name** (case-sensitive)

### Step 5: Verify
- If name matches → Dealership deleted ✅
- If name wrong → Deletion cancelled ❌
- Success message appears
- Dealership removed from list

---

## Safety Checklist

Before deleting, ask yourself:

- [ ] Do I have the right dealership?
- [ ] Have I backed up any important data?
- [ ] Have I notified the dealership owner?
- [ ] Have I considered alternatives (deactivation)?
- [ ] Am I certain I want to proceed?

**If you answered NO to any question, DO NOT DELETE.**

---

## Cancelling Deletion

You can cancel at two points:

1. **Before typing the name**: Click "Cancel" button
2. **After typing wrong name**: Type incorrect name and click OK

Both will cancel the deletion with this message:
```
❌ Deletion cancelled: Dealership name did not match
```

---

## What Gets Deleted

### Dealership Data
- Basic info (name, address, phone, email)
- Logo and branding
- Theme colors and customization
- Hero media (images/videos)
- Policies (finance, warranty)
- Social media links
- Navigation configuration

### Inventory
- All vehicle listings
- Vehicle images (Cloudinary URLs remain, but references deleted)
- Vehicle descriptions and details

### Customer Data
- All customer leads
- All "Sell Your Car" requests
- Contact information

### Content
- All blog posts
- Blog images (Cloudinary URLs remain, but references deleted)

### Users
- Dealership owner account(s)
- Staff accounts
- Login credentials

---

## API Usage

If you need to delete programmatically:

```bash
# Login first
curl -X POST http://localhost:3001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  -c cookies.txt

# Delete dealership (IRREVERSIBLE!)
curl -X DELETE http://localhost:3001/api/dealers/[ID] \
  -b cookies.txt
```

**Response (Success):**
```json
{
  "message": "Dealership deleted successfully",
  "dealership": { "id": 3, "name": "...", ... }
}
```

---

## Testing

Run automated test:
```bash
node test_dealership_deletion.js
```

This creates, then deletes a test dealership to verify functionality.

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Delete button not visible | Ensure you're logged in as admin |
| Confirmation not appearing | Check browser popup blockers |
| Name doesn't match error | Type exact name (case-sensitive) |
| 403 Forbidden error | Only admins can delete dealerships |
| 404 Not Found error | Dealership may already be deleted |

---

## Common Mistakes

### ❌ Wrong Name Typed
```
Dealership name: "Acme Auto Sales"
User types: "acme auto sales"  ← Wrong! (lowercase)
Result: Deletion cancelled
```

### ✅ Correct Name Typed
```
Dealership name: "Acme Auto Sales"
User types: "Acme Auto Sales"  ← Correct! (exact match)
Result: Dealership deleted
```

---

## Alternatives to Deletion

Consider these alternatives if you want to preserve data:

1. **Remove Access**: Delete user accounts, but keep dealership
2. **Hide Content**: Set all vehicles to "draft" status
3. **Archive**: Export data before deleting
4. **Soft Delete**: Add a feature flag (requires development)

---

## Emergency Recovery

**Q: I deleted by accident. Can I recover?**

**A: NO.** Deletion is permanent. There is no recovery mechanism.

### Prevention Tips:
1. Always read the warning carefully
2. Type the name slowly and verify
3. Consider the checklist above
4. When in doubt, don't delete

---

## For Developers

### Database Cascade
The `dealership` table uses `ON DELETE CASCADE`:

```sql
-- All related tables will auto-delete
vehicle → dealership_id
lead → dealership_id
sales_request → dealership_id
blog → dealership_id
app_user → dealership_id
```

### Backend Endpoint
```javascript
DELETE /api/dealers/:id
// Requires: requireAuth, requireAdmin
// Returns: { message, dealership }
```

### Frontend Handler
```javascript
handleDeleteDealership(id, name)
// 1. Shows confirmation prompt
// 2. Validates user input
// 3. Calls API
// 4. Updates UI
```

---

## Support

### Need Help?
1. Check `DEALERSHIP_DELETION_FEATURE.md` for full documentation
2. Review error messages in UI
3. Check browser console for details
4. Verify admin permissions

### Still Having Issues?
- Ensure backend is running
- Check database connection
- Verify admin session is active
- Try logging out and back in

---

## Summary

**Deletion Process:**
1. Click "Delete" button
2. Read warning
3. Type exact dealership name
4. Confirm

**Remember:**
- ⚠️ Deletion is PERMANENT
- ⚠️ ALL related data is deleted
- ⚠️ NO recovery possible
- ⚠️ Requires exact name match

**Use with extreme caution.**
