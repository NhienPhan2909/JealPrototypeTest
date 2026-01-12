# Agent Reference Guide: Sales Request Feature (Epic 6)

**Last Updated:** December 17, 2025  
**Status:** ✅ IMPLEMENTED AND DEPLOYED  
**For:** PM, Architect, SM, Dev, QA, and all AI development agents

---

## Quick Reference

### Feature Name
**Vehicle Sales Request Feature ("Sell Your Car")**

### Epic ID
**Epic 6**

### Implementation Date
**December 17, 2025**

### Current Status
**✅ COMPLETED - Production Ready**

---

## Purpose of This Document

This document serves as a comprehensive reference for all AI agents (PM, Architect, SM, Dev, QA, etc.) working on or maintaining the Sales Request feature. It provides context, technical details, and pointers to detailed documentation.

---

## What Is This Feature?

### Business Context
The Sales Request feature enables customers to submit vehicle sales inquiries through the dealership website. Instead of only selling vehicles (outbound), dealerships can now accept inbound requests from customers who want to sell their vehicles to the dealership.

### User Flow
1. **Customer visits dealership website** → Clicks "Sell Your Car" in header
2. **Customer fills out form** → Provides personal info and vehicle details
3. **Customer submits request** → Sees success message
4. **Dealership staff views request** → In admin panel "Sales Requests" section
5. **Staff manages request** → Updates status, contacts customer, or deletes

---

## Technical Architecture

### Database Layer
- **Table:** `sales_request`
- **Key Columns:** dealership_id (FK), name, email, phone, make, model, year, kilometers, additional_message, status, created_at
- **Indexes:** dealership_id, created_at DESC
- **Location:** Database file: `backend/db/salesRequests.js`

### Backend API
- **Routes Module:** `backend/routes/salesRequests.js`
- **Endpoints:**
  - `POST /api/sales-requests` (public)
  - `GET /api/sales-requests?dealershipId=:id` (admin)
  - `PATCH /api/sales-requests/:id/status?dealershipId=:id` (admin)
  - `DELETE /api/sales-requests/:id?dealershipId=:id` (admin)

### Frontend Components
- **Public Page:** `frontend/src/pages/public/SellYourCar.jsx`
- **Admin Page:** `frontend/src/pages/admin/SalesRequests.jsx`
- **Routes:** `/sell-your-car` (public), `/admin/sales-requests` (protected)

### Navigation Integration
- Public header: "Sell Your Car" link (order 7, FaDollarSign icon)
- Admin header: "Sales Requests" link (between Lead Inbox and View Website)

---

## Key Features

### Public-Facing
- ✅ Form with validation (name, email, phone, make, model, year, kilometers, additional_message)
- ✅ Email format validation
- ✅ Year range validation (1900 to current year + 1)
- ✅ Kilometers positive integer validation
- ✅ Success/error messaging
- ✅ Form reset after submission
- ✅ Theme color integration
- ✅ Responsive design

### Admin-Facing
- ✅ Table view of all sales requests
- ✅ Sorted by newest first
- ✅ Status management (received, in progress, done)
- ✅ Color-coded status badges
- ✅ Call button (tel: link)
- ✅ Email button (mailto: link with pre-filled subject)
- ✅ Delete with confirmation modal
- ✅ Date filtering (All time, Last 7 days, Last 30 days)
- ✅ Message expansion for long text
- ✅ Multi-tenant isolation

---

## Security Measures

### Input Sanitization (XSS Prevention)
All text inputs sanitized using HTML entity encoding:
```javascript
function sanitizeInput(input) {
  return input
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#x27;')
    .replace(/\//g, '&#x2F;');
}
```

### Multi-Tenant Isolation
- All queries filter by `dealershipId`
- API endpoints require `dealershipId` parameter (admin endpoints)
- Foreign key constraint enforces data integrity
- Ownership validation on update/delete operations

### SQL Injection Prevention
- All queries use parameterized statements via `pg` library
- No string concatenation in SQL queries

### Validation Rules
- **Name:** Max 255 characters
- **Email:** Max 255 characters, valid email format
- **Phone:** Max 20 characters
- **Make/Model:** Max 100 characters each
- **Year:** Integer, 1900 ≤ year ≤ (current_year + 1)
- **Kilometers:** Integer, ≥ 0
- **Additional Message:** Max 5000 characters

---

## Documentation Map

### For Product Managers
📄 **Primary Doc:** `docs/prd/epic-6-sales-request-feature.md`
- Business context and value proposition
- User stories with acceptance criteria
- Functional and non-functional requirements
- Success metrics
- Future enhancements

### For Architects
📐 **Primary Doc:** `docs/architecture/sales-request-architecture.md`
- System architecture diagrams
- Data model and relationships
- API specifications
- Frontend architecture
- Security architecture
- Performance considerations
- Integration points
- Architecture decision records (ADRs)

### For Scrum Masters
📋 **Primary Doc:** `docs/stories/sprint-sales-request-feature.md`
- Sprint overview and goals
- User stories with tasks
- Sprint metrics and velocity
- Sprint review and retrospective
- Test scenarios and results
- Deployment checklist

### For Developers
💻 **Primary Docs:**
- `SELL_YOUR_CAR_FEATURE.md` (root) - Implementation summary
- `SELL_YOUR_CAR_QUICK_START.md` (root) - Quick start guide
- API endpoints in `backend/routes/salesRequests.js`
- Database schema in `backend/db/schema.sql`
- Components in `frontend/src/pages/`

### For QA Engineers
🧪 **Primary Docs:**
- `docs/stories/sprint-sales-request-feature.md` (Acceptance Testing section)
- `SELL_YOUR_CAR_QUICK_START.md` (Testing Steps section)
- Manual test scenarios in sprint document

---

## Common Questions & Answers

### Q: Where is the sales request data stored?
**A:** In the `sales_request` table in PostgreSQL. Each request is linked to a dealership via `dealership_id` foreign key.

### Q: How do I test the public form?
**A:** 
1. Start backend and frontend servers
2. Navigate to `http://localhost:3000`
3. Click "Sell Your Car" in header
4. Fill out form and submit
5. Check admin panel at `/admin/sales-requests`

### Q: How is multi-tenant isolation enforced?
**A:** Three layers:
1. Database: Foreign key constraint on `dealership_id`
2. API: All admin endpoints require `dealershipId` query parameter
3. UI: AdminContext provides `selectedDealership`, all API calls include it

### Q: What happens when a dealership is deleted?
**A:** All associated sales requests are automatically deleted due to `ON DELETE CASCADE` constraint.

### Q: Can customers upload photos with their sales request?
**A:** Not in current implementation (v1.0). This is a planned future enhancement.

### Q: How are sales requests different from leads?
**A:**
- **Leads:** Customer interested in *buying* a vehicle from dealership
- **Sales Requests:** Customer wants to *sell* their vehicle to dealership
- Both are stored in separate tables with similar structure

### Q: What status values are available?
**A:** Three statuses with CHECK constraint:
- `received` (default, blue badge)
- `in progress` (yellow badge)
- `done` (green badge)

### Q: Is email notification sent when request is submitted?
**A:** Yes (as of December 31, 2025). Dealerships receive an email notification immediately when a customer submits a sales request. The email includes:
- Customer information (name, email, phone)
- Vehicle details (make, model, year, kilometers)
- Additional message from customer
- Professional HTML formatting with plain text fallback
- Subject line: "New Sales Request: [Year] [Make] [Model]"

Email configuration is required in `.env` file. See `SALES_REQUEST_EMAIL_NOTIFICATION.md` for setup details.

---

## Code Locations

### Backend Files
```
backend/
├── db/
│   ├── salesRequests.js           # Database query functions
│   ├── schema.sql                 # Table definition (lines 88-113)
│   └── migrations/
│       └── 006_add_sales_request_table.sql  # Migration script
├── routes/
│   └── salesRequests.js           # API route handlers + email notification
├── services/
│   └── emailService.js            # Email service with sales request notification
├── config/
│   └── defaultNavigation.js       # Navigation config (lines 66-75)
└── server.js                      # Route registration (line 47)
```

### Frontend Files
```
frontend/src/
├── pages/
│   ├── public/
│   │   └── SellYourCar.jsx        # Public form page
│   └── admin/
│       └── SalesRequests.jsx      # Admin management page
├── components/
│   └── AdminHeader.jsx            # Admin nav link (lines 195-201)
├── utils/
│   ├── defaultNavigation.js       # Public nav config (lines 71-80)
│   └── iconMapper.js              # Icon imports (line 34, line 50)
└── App.jsx                        # Route definitions (lines 18-19, 57, 70)
```

### Documentation Files
```
docs/
├── prd/
│   ├── epic-6-sales-request-feature.md       # PRD document
│   ├── epic-list.md                          # Epic list (updated)
│   └── requirements.md                       # Requirements (updated FR22-FR36)
├── architecture/
│   └── sales-request-architecture.md         # Architecture document
├── stories/
│   └── sprint-sales-request-feature.md       # Sprint document
└── README-FOR-AGENTS-SALES-REQUEST.md        # This file
```

### Root Documentation Files
```
/
├── SELL_YOUR_CAR_FEATURE.md                         # Implementation summary
├── SELL_YOUR_CAR_QUICK_START.md                     # Quick start guide
├── SALES_REQUEST_EMAIL_NOTIFICATION.md              # Email notification feature (Added 2025-12-31)
├── SALES_REQUEST_EMAIL_IMPLEMENTATION_SUMMARY.md    # Email feature summary (Added 2025-12-31)
└── test_sales_request_email.js                      # Email notification test script (Added 2025-12-31)
```

---

## Integration Points

### With Navigation System
- Public navigation: "Sell Your Car" appears in header menu (order 7)
- Admin navigation: "Sales Requests" appears in admin header
- Icon system: Uses FaDollarSign from react-icons/fa

### With Dealership Context
- Public form uses `currentDealershipId` from DealershipContext
- Admin page uses `selectedDealership` from AdminContext
- All operations respect multi-tenant boundaries

### With Database
- Foreign key to `dealership` table
- Cascade delete when dealership removed
- Indexed queries for performance

---

## Performance Characteristics

### Database Performance
- **Query Time:** < 300ms for filtered list (indexed)
- **Insert Time:** < 200ms for new request
- **Update Time:** < 150ms for status change
- **Delete Time:** < 150ms for deletion

### API Performance
- All endpoints respond in < 300ms under normal load
- Indexed queries prevent full table scans
- Parameterized queries cached by PostgreSQL

### Frontend Performance
- Form submission < 3 seconds (including network)
- Table rendering handles 100+ requests without lag
- Lazy loading for long additional messages

---

## Testing Reference

### Manual Test Scenarios
See: `SELL_YOUR_CAR_QUICK_START.md` (Testing Steps section)

### Automated Tests (Planned)
- [ ] Unit tests for database functions
- [ ] Unit tests for API validation
- [ ] Integration tests for endpoints
- [ ] E2E tests for user flows

### Security Tests
✅ XSS injection prevention verified
✅ SQL injection prevention verified
✅ Multi-tenant isolation verified
✅ Input validation verified

---

## Deployment Information

### Database Migration
**File:** `backend/db/migrations/006_add_sales_request_table.sql`

**Run Migration:**
```bash
Get-Content backend\db\migrations\006_add_sales_request_table.sql | docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype
```

**Verify Migration:**
```bash
docker exec -i jeal-prototype-db psql -U postgres -d jeal_prototype -c "\d sales_request"
```

### Environment Variables
**None required** - Uses existing database connection configuration

### Dependencies
**None added** - Uses existing npm packages

---

## Troubleshooting

### Issue: "Table doesn't exist" error
**Solution:** Run database migration (see Deployment Information above)

### Issue: Navigation link not appearing
**Solution:** 
1. Clear browser cache
2. Restart frontend dev server
3. Verify `defaultNavigation.js` updated

### Issue: Form submission fails with 400 error
**Solution:** Check validation:
- Email format correct?
- Year in valid range (1900 to current+1)?
- Kilometers positive number?
- All required fields filled?

### Issue: Admin page shows no data
**Solution:** 
1. Verify dealership selected in admin header
2. Check browser console for API errors
3. Verify database has data for that dealership

### Issue: Status update doesn't persist
**Solution:** 
1. Check browser console for API errors
2. Verify dealershipId parameter included in request
3. Check backend logs for database errors

---

## Future Enhancements (Planned)

### Phase 2 Features
1. ✅ **Email Notifications:** Auto-email dealership on new submission (COMPLETED 2025-12-31)
2. **Photo Upload:** Allow customers to upload vehicle photos
3. **Valuation API:** Integrate with KBB/Edmunds for instant estimates
4. **SMS Notifications:** Text alerts for new requests
5. **Export to CSV:** Export data for external CRM systems
6. **Advanced Filtering:** Filter by make, model, year range, price range
7. **Notes/Comments:** Add internal notes to requests
8. **Response Templates:** Pre-written responses for common scenarios

### Technical Debt
- Add automated unit tests
- Add integration tests
- Add E2E tests
- Implement pagination (> 100 records)
- Refactor shared validation/sanitization into utility module

---

## Related Epics & Features

### Similar Features
- **Lead Inbox (Epic 2):** Customer inquiries about *buying* vehicles
- **General Enquiry Form (Epic 2):** Contact form for general questions

### Dependencies
- **Multi-Tenancy (Epic 1):** Database architecture foundation
- **Navigation System (Epic 5):** Header navigation infrastructure
- **Admin CMS (Epic 3):** Admin panel framework

---

## Contact & Support

### For Questions About This Feature
1. **Check Documentation:** Start with this file, then detailed docs
2. **Review Code:** Check code comments and structure
3. **Check Git History:** See commit messages for implementation details
4. **Ask Team:** Contact PM, Architect, or Dev team

### Key Stakeholders
- **Product Manager:** Feature requirements and business context
- **Tech Lead/Architect:** Technical decisions and architecture
- **Scrum Master:** Sprint planning and execution
- **QA Lead:** Testing and quality assurance

---

## Version History

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0 | 2025-12-17 | Initial implementation | Development Team |
| 1.1 | 2025-12-31 | Added email notifications | Development Team |

---

## Appendix: API Examples

### Example 1: Submit Sales Request (Public)
```bash
POST /api/sales-requests
Content-Type: application/json

{
  "dealership_id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "(555) 123-4567",
  "make": "Toyota",
  "model": "Camry",
  "year": 2018,
  "kilometers": 75000,
  "additional_message": "Well maintained, one owner"
}
```

### Example 2: List Sales Requests (Admin)
```bash
GET /api/sales-requests?dealershipId=1
Authorization: Session Cookie
```

### Example 3: Update Status (Admin)
```bash
PATCH /api/sales-requests/42/status?dealershipId=1
Content-Type: application/json
Authorization: Session Cookie

{
  "status": "in progress"
}
```

### Example 4: Delete Sales Request (Admin)
```bash
DELETE /api/sales-requests/42?dealershipId=1
Authorization: Session Cookie
```

---

## Quick Links

- 📄 [Epic 6 PRD](prd/epic-6-sales-request-feature.md)
- 📐 [Architecture Doc](architecture/sales-request-architecture.md)
- 📋 [Sprint Doc](stories/sprint-sales-request-feature.md)
- 💻 [Implementation Summary](../SELL_YOUR_CAR_FEATURE.md)
- 🚀 [Quick Start Guide](../SELL_YOUR_CAR_QUICK_START.md)
- 📋 [Epic List](prd/epic-list.md)
- 📝 [Requirements](prd/requirements.md)

---

**Document Purpose:** Agent reference and onboarding  
**Intended Audience:** All AI development agents (PM, Architect, SM, Dev, QA, etc.)  
**Maintenance:** Update when feature is modified or extended  
**Last Review:** December 17, 2025
