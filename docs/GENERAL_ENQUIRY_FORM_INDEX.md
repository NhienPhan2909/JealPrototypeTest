# General Enquiry Form - Documentation Index

**Feature:** General Enquiry Form on Homepage  
**Implementation Date:** 2025-12-09  
**Status:** ✅ Completed  
**Version:** 1.0

## Quick Links

### Primary Documentation
- 📖 **[Feature Documentation](GENERAL_ENQUIRY_FORM.md)** - Complete feature guide with usage and testing
- 📋 **[User Story 6.1](stories/6.1.general-enquiry-form.md)** - Detailed story with acceptance criteria and tasks
- 📝 **[Changelog](CHANGELOG-GENERAL-ENQUIRY-FORM-2025-12-09.md)** - Implementation summary and impact analysis

### Project Documents (Updated)
- 📄 **[PRD v1.3](prd.md)** - Product Requirements Document (added FR24)
- 🏗️ **[Architecture v1.1](architecture.md)** - Architecture Document (added component specs)

### Code Files
- ⚛️ **[GeneralEnquiryForm.jsx](../frontend/src/components/GeneralEnquiryForm.jsx)** - Main form component
- 🏠 **[Home.jsx](../frontend/src/pages/public/Home.jsx)** - Updated homepage with grid layout
- 🔍 **[SearchWidget.jsx](../frontend/src/components/SearchWidget.jsx)** - Updated for grid layout
- 🧪 **[test_general_enquiry.js](../test_general_enquiry.js)** - Automated API test

## What This Feature Does

The General Enquiry Form allows website visitors to contact the dealership without selecting a specific vehicle. It's positioned on the homepage next to the vehicle search widget.

### Key Features
- ✅ Four required fields: Name, Email, Phone, Message
- ✅ Real-time validation with inline errors
- ✅ Character counter (5000 max)
- ✅ Success/error message handling
- ✅ Responsive grid layout (side-by-side on desktop, stacked on mobile)
- ✅ Integrates with existing Lead Inbox in admin panel
- ✅ No backend or database changes required

## For Development Teams

### Quick Start
```bash
# View the feature on homepage
http://localhost:3000

# View general enquiries in admin
http://localhost:3000/admin/login
→ Navigate to Lead Inbox
→ Look for "General Enquiry" label

# Run automated test
node test_general_enquiry.js
```

### Component Usage
```jsx
import GeneralEnquiryForm from '../components/GeneralEnquiryForm';

// In your page component
<GeneralEnquiryForm />
// That's it! Component handles everything internally
```

### API Endpoint
```javascript
POST /api/leads
{
  "dealership_id": 1,
  "name": "John Doe",
  "email": "john@example.com",
  "phone": "(555) 123-4567",
  "message": "Customer message here"
  // Note: vehicle_id is omitted for general enquiries
}
```

## For Product Managers

### Business Value
- **Lead Capture:** Captures enquiries from customers in early research phase
- **User Experience:** Easy contact method without browsing inventory first
- **Efficiency:** Auto-appears in existing Lead Inbox workflow
- **Mobile-Friendly:** Fully responsive design

### Success Metrics (To Track)
- General enquiry submission rate
- Conversion rate (enquiries → sales)
- Time to first response
- Customer satisfaction scores

### PRD Reference
- **Requirement:** FR24 (see prd.md v1.3)
- **Story:** 6.1 (see stories/6.1.general-enquiry-form.md)

## For QA Teams

### Testing Resources
- **Manual Test Checklist:** See [Story 6.1](stories/6.1.general-enquiry-form.md) → Testing section
- **Automated Test:** Run `node test_general_enquiry.js`
- **Test Data:** Use test script or manual submission

### Test Coverage
- ✅ Form validation (all fields)
- ✅ API submission
- ✅ Success/error messages
- ✅ Form reset
- ✅ Admin Lead Inbox display
- ✅ Responsive layout
- ⚠️ Browser testing (Chrome only, need Firefox/Safari/Edge)
- 🚧 Screen reader testing (not performed)

## For Architects

### Technical Overview
- **Pattern:** Reusable React component with hooks
- **Context:** Uses existing DealershipContext
- **Styling:** Tailwind CSS (follows project standards)
- **Validation:** Client-side (email regex, phone digits, character limits)
- **Security:** Backend sanitization (existing XSS prevention)
- **Layout:** CSS Grid (responsive breakpoint at 1024px)

### Architecture Changes
- ✅ New component: GeneralEnquiryForm.jsx
- ✅ Modified: Home.jsx (grid layout)
- ✅ Modified: SearchWidget.jsx (styling)
- ✅ No backend changes
- ✅ No database changes
- ✅ No API changes

### Integration Points
- `DealershipContext` → Provides current dealership ID
- `/api/leads` → Existing POST endpoint
- `LeadInbox.jsx` → Existing admin interface (no changes needed)

## For Scrum Masters

### Sprint Summary
- **Story Points:** 3 (estimated)
- **Actual Time:** ~2-3 hours implementation + documentation
- **Dependencies:** None (used existing infrastructure)
- **Blockers:** None encountered

### Deliverables
1. ✅ Working feature deployed
2. ✅ Automated test passing
3. ✅ User story completed (15/15 criteria)
4. ✅ Documentation updated (4 docs)
5. ✅ Code reviewed and merged

### Sprint Artifacts
- **Story:** [6.1.general-enquiry-form.md](stories/6.1.general-enquiry-form.md)
- **Demo:** Homepage at http://localhost:3000
- **Acceptance:** All criteria met ✅

## Documentation Structure

```
docs/
├── GENERAL_ENQUIRY_FORM.md                    # Feature guide (you are here)
├── CHANGELOG-GENERAL-ENQUIRY-FORM-2025-12-09.md  # Implementation summary
├── GENERAL_ENQUIRY_FORM_INDEX.md              # This index file
├── prd.md (v1.3)                              # Updated with FR24
├── architecture.md (v1.1)                     # Updated with component specs
└── stories/
    └── 6.1.general-enquiry-form.md            # User story with tasks
```

## Related Features

### Similar Components
- **EnquiryForm.jsx** - Vehicle-specific enquiry form on detail pages
- **SearchWidget.jsx** - Vehicle search form on homepage

### Related Admin Features
- **LeadInbox.jsx** - Where general enquiries appear
- **DealerSettings.jsx** - Configure dealership contact info

### Related Stories
- **Story 2.1** - Lead Inbox implementation (dependency)
- **Story 1.2** - Public home page (dependency)
- **Story 1.5** - Vehicle enquiry form (similar feature)

## Future Enhancements

See detailed list in:
- [Story 6.1](stories/6.1.general-enquiry-form.md) → Future Enhancements section
- [Changelog](CHANGELOG-GENERAL-ENQUIRY-FORM-2025-12-09.md) → Future Enhancements section

### Top 3 Priorities
1. Email notifications (customer + dealership)
2. reCAPTCHA spam prevention
3. Enhanced form fields (preferred contact method, subject)

## Support & Troubleshooting

### Common Issues

**Issue:** Form submission fails
- **Check:** Backend server running on port 5000
- **Check:** Database connection established
- **Check:** DealershipContext has valid dealership ID

**Issue:** Validation not working
- **Check:** Client-side JavaScript enabled
- **Check:** React dev tools for error messages
- **Check:** Browser console for errors

**Issue:** Enquiries not in Lead Inbox
- **Check:** Database leads table for vehicle_id = NULL entries
- **Check:** Correct dealership selected in admin
- **Check:** API response returns 201 status

### Debug Commands
```bash
# Test API directly
node test_general_enquiry.js

# Check database
psql $DATABASE_URL
SELECT * FROM leads WHERE vehicle_id IS NULL;

# Check backend logs
npm start  # in backend folder, watch console output
```

## Version History

| Date | Version | Changes | Author |
|------|---------|---------|--------|
| 2025-12-09 | 1.0 | Initial feature implementation and documentation | Dev Agent |

## Contact

For questions about this feature:
- **Code Questions:** See GeneralEnquiryForm.jsx JSDoc comments
- **Business Questions:** See prd.md FR24
- **Technical Questions:** See architecture.md Components section
- **Testing Questions:** See Story 6.1 Testing section

---

**Last Updated:** 2025-12-09  
**Maintained By:** Development Team  
**Status:** ✅ Production Ready
