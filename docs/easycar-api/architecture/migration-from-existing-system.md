# Migration from Existing System

Since this is a **greenfield integration** (no existing EasyCars integration), the migration focuses on onboarding dealerships to the new feature.

### Onboarding Process

1. **Dealership Notification:**
   - Email announcement of new EasyCars integration feature
   - Link to documentation and setup guide
   - Offer training session or walkthrough

2. **Credential Setup:**
   - Dealership admin logs into CMS
   - Navigates to EasyCars Settings
   - Enters credentials provided by EasyCars
   - Tests connection to verify
   - Saves credentials

3. **Initial Sync:**
   - Admin triggers manual stock sync
   - System imports all current inventory from EasyCars
   - Admin reviews imported vehicles for accuracy
   - Adjusts any necessary mappings or settings

4. **Ongoing Monitoring:**
   - Admin monitors sync dashboard for status
   - Reviews sync logs periodically
   - Reports any issues to support team

### Data Reconciliation

**Initial Import:**
- First stock sync imports all vehicles from EasyCars
- Existing manually-entered vehicles NOT automatically matched
- Admin manually reviews and merges duplicates if needed

**Duplicate Handling:**
- System detects duplicates by VIN or StockNumber
- Presents list of potential duplicates to admin
- Admin chooses to merge or keep separate

**Conflict Resolution:**
- If local vehicle modified after EasyCars sync, admin chooses:
  - Keep local changes (skip sync update)
  - Accept EasyCars data (overwrite local)
  - Merge (future enhancement)

---
