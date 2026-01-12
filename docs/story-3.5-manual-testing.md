# Story 3.5 Manual Testing Guide

## Implementation Complete - Testing Required

**Story:** 3.5 Lead Inbox & Viewing  
**Status:** Implementation Complete - Awaiting Manual Testing  
**Developer:** James (GitHub Copilot CLI)  
**Date:** 2025-11-24

---

## What Was Implemented

### Files Created:
- `frontend/src/pages/admin/LeadInbox.jsx` - Complete Lead Inbox component

### Files Modified:
- `frontend/src/App.jsx` - Added /admin/leads route

### Features Implemented:
✅ Lead Inbox page at `/admin/leads`  
✅ Fetches leads from API with `dealershipId` parameter  
✅ Displays leads in table with columns: Name, Email, Phone, Vehicle, Message, Date, Actions  
✅ Sorts leads by newest first (created_at descending)  
✅ Vehicle title fetching and linking to public vehicle page (opens in new tab)  
✅ "General Enquiry" display for leads without vehicle_id  
✅ Message truncation (100 chars) with "View more" expansion  
✅ Contact action buttons (Call and Email with pre-filled subject)  
✅ Empty state message  
✅ Lead count display with filtering  
✅ Date range filter (Last 7 days, Last 30 days, All time)  
✅ Loading state during API fetch  
✅ Error handling with user-friendly messages  
✅ Mobile-responsive layout  
✅ Multi-tenancy security (dealershipId enforcement)  
✅ Full JSDoc documentation

---

## How to Run Manual Tests

### Prerequisites:
1. Ensure backend server is running (`npm run server` from root)
2. Ensure frontend dev server is running (`npm run client` from root)
3. Have at least one dealership with leads in the database

### Test Procedure:

#### 1. **Basic Page Load**
- [ ] Log in to admin panel
- [ ] Click "Lead Inbox" link in navigation menu
- [ ] Verify page loads at URL: `/admin/leads`
- [ ] Verify page title displays: "Lead Inbox"
- [ ] Verify lead count shows: "Showing X leads"

#### 2. **Data Display**
- [ ] Verify leads are displayed in table format
- [ ] Check all columns present: Name, Email, Phone, Vehicle, Message, Date Submitted, Actions
- [ ] Verify data populates correctly in each column

#### 3. **Sorting**
- [ ] Check that newest leads appear at the top (check Date Submitted column)
- [ ] Verify dates are in descending order

#### 4. **Vehicle Linking**
- [ ] For leads WITH vehicle_id:
  - [ ] Verify vehicle title displays as blue underlined link
  - [ ] Click vehicle link
  - [ ] Verify it opens public vehicle detail page in NEW TAB
  - [ ] Verify correct vehicle loads
- [ ] For leads WITHOUT vehicle_id:
  - [ ] Verify "General Enquiry" displays in gray text
  - [ ] Verify no clickable link present

#### 5. **Message Truncation & Expansion**
- [ ] Find a lead with message longer than 100 characters
- [ ] Verify message shows only first 100 chars + "..."
- [ ] Verify "View more" button appears
- [ ] Click "View more"
- [ ] Verify full message displays below
- [ ] Click "Show less"
- [ ] Verify message returns to truncated view
- [ ] For short messages (<100 chars):
  - [ ] Verify full message displays
  - [ ] Verify NO "View more" button

#### 6. **Contact Action Buttons**
- [ ] Click "Call" button
  - [ ] Verify phone dialer opens (or tel: link handler on desktop)
  - [ ] Verify phone number is correct
- [ ] Click "Email" button
  - [ ] Verify email client opens
  - [ ] Verify "To:" field has correct email address
  - [ ] Verify "Subject:" field contains: "Re: Your enquiry about [Vehicle Title or 'our dealership']"

#### 7. **Date Range Filter**
- [ ] Verify dropdown shows: "All time" (default), "Last 7 days", "Last 30 days"
- [ ] Select "Last 7 days"
  - [ ] Verify only leads from last 7 days display
  - [ ] Verify lead count updates
- [ ] Select "Last 30 days"
  - [ ] Verify only leads from last 30 days display
  - [ ] Verify lead count updates
- [ ] Select "All time"
  - [ ] Verify all leads display again
  - [ ] Verify lead count matches total

#### 8. **Empty State**
- [ ] Switch to a dealership with NO leads (or temporarily filter to show none)
- [ ] Verify empty state message displays: "No leads yet. Leads submitted through the website will appear here."
- [ ] Verify no table displays

#### 9. **Error Handling**
- [ ] Stop backend server
- [ ] Refresh Lead Inbox page
- [ ] Verify error message displays in red box
- [ ] Restart backend server

#### 10. **Multi-Tenancy**
- [ ] Switch to Dealership A in dealership selector
- [ ] Note which leads display for Dealership A
- [ ] Switch to Dealership B
- [ ] Verify ONLY Dealership B's leads display
- [ ] Verify Dealership A's leads DO NOT appear
- [ ] Check browser Network tab → verify API request includes correct `dealershipId` parameter

#### 11. **Loading State**
- [ ] Reload page or switch dealerships
- [ ] Watch for "Loading leads..." text during fetch
- [ ] Verify it disappears once leads load

#### 12. **Mobile Responsiveness**
- [ ] Resize browser window to tablet width (~768px)
  - [ ] Verify table remains readable
  - [ ] Verify horizontal scroll appears if needed
- [ ] Resize to mobile width (~375px)
  - [ ] Verify table adapts or allows horizontal scroll
  - [ ] Verify all content accessible

#### 13. **Session Authentication**
- [ ] Log out from admin panel
- [ ] Try to access `/admin/leads` directly in URL bar
- [ ] Verify redirect to `/admin/login`
- [ ] Log back in
- [ ] Verify redirect to dashboard or Lead Inbox loads correctly

---

## Expected API Behavior

### GET /api/leads?dealershipId=X
**Request:**
```
GET /api/leads?dealershipId=1
Credentials: include (session cookies)
```

**Response (Success 200):**
```json
[
  {
    "id": 1,
    "dealership_id": 1,
    "vehicle_id": 5,
    "name": "John Doe",
    "email": "john@example.com",
    "phone": "(555) 111-2222",
    "message": "I'm interested in the 2015 Toyota Camry. Is it still available?",
    "created_at": "2025-11-19T14:20:00.000Z"
  }
]
```

**Response (Error 400):**
```json
{ "error": "dealershipId query parameter is required" }
```

### GET /api/vehicles/:id?dealershipId=X
**Request:**
```
GET /api/vehicles/5?dealershipId=1
Credentials: include
```

**Response (Success 200):**
```json
{
  "id": 5,
  "title": "2015 Toyota Camry",
  ...
}
```

---

## Browser DevTools Checks

### Network Tab:
- [ ] Verify `GET /api/leads?dealershipId=X` request sent on page load
- [ ] Verify `GET /api/vehicles/:id?dealershipId=X` requests sent for each unique vehicle_id
- [ ] Verify all requests include `credentials: include`
- [ ] Verify Status 200 for successful requests
- [ ] Verify no CORS errors

### Console Tab:
- [ ] Verify no JavaScript errors
- [ ] Verify no React warnings
- [ ] Check for any console.error messages

---

## Pass/Fail Criteria

**PASS:** All 13 test sections complete with no issues  
**FAIL:** Any acceptance criteria not met, errors in console, or functionality not working as specified

---

## Issues Found During Testing

*(User should document any issues here)*

| Issue # | Description | Severity | Steps to Reproduce |
|---------|-------------|----------|-------------------|
| | | | |

---

## Sign-Off

- [ ] All manual tests completed
- [ ] All acceptance criteria verified
- [ ] No blocking issues found
- [ ] Story ready to mark as "Ready for Review"

**Tested By:** ___________________  
**Date:** ___________________  
**Signature:** ___________________
