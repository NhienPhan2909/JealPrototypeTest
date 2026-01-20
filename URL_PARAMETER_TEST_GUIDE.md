# URL Parameter Routing - Quick Test Guide

**Testing dealership selection via URL parameters**

---

## ✅ What Was Implemented

Updated `DealershipContext.jsx` to support URL parameters for dealership selection.

**Selection Priority:**
1. **URL parameter** (`?dealership=X`) - When explicitly specified in URL
2. **Default to ID 1** (Acme Auto Sales) - Clean start, always

**Important:** The system now **always defaults to Acme Auto Sales (ID 1)** when visiting `localhost:3000` without a parameter. LocalStorage is updated when URL parameters are used but does NOT override the default behavior.

---

## 🧪 How to Test

### Test URL Parameters

Visit these URLs and verify which dealership loads:

**Test 1: Default (No Parameter)**
```
http://localhost:3000
```
Expected: **Acme Auto Sales** (ID 1) - Always!

**Test 2: Acme Auto Sales (Explicit)**
```
http://localhost:3000?dealership=1
```
Expected: **Acme Auto Sales** (ID 1)

**Test 3: Premier Motors**
```
http://localhost:3000?dealership=2
```
Expected: **Premier Motors** (ID 2)

### Check Debug Info

Look at the bottom-right corner of the page. You should see a debug panel showing:
- Current Dealership ID
- Dealership Name
- URL Parameter value
- LocalStorage value

---

## 🔍 Verification Checklist

For each URL, verify:

**Visual Elements:**
- [ ] Correct dealership name in header
- [ ] Correct logo (if set)
- [ ] Correct theme color
- [ ] Correct about text on About page
- [ ] Correct contact info in footer

**Debug Panel (bottom-right):**
- [ ] Shows correct dealership ID
- [ ] Shows correct dealership name
- [ ] URL Param matches the `?dealership=X` value (or "None")
- [ ] Storage value updates after visiting URL with parameter

---

## 🐛 Troubleshooting

### Issue: localhost:3000 shows Premier Motors instead of Acme

**Cause:** Old browser cache or need to refresh  
**Solution:** Hard refresh (Ctrl+Shift+R or Cmd+Shift+R)

### Issue: URL parameter ignored

**Cause:** React not re-rendering on URL change  
**Solution:** Hard refresh (Ctrl+Shift+R) or restart dev server

### Issue: Debug panel not showing

**Cause:** Component not loaded or in production mode  
**Solution:** Check browser console for errors, ensure dev server running

### Issue: "Dealership not found" error

**Cause:** Invalid dealership ID in URL  
**Solution:** Use valid IDs: 1 (Acme) or 2 (Premier)

---

## 📊 Expected Results

### Test Matrix

| URL | Expected Dealership | ID | Name | Notes |
|-----|--------------------|----|------|-------|
| `localhost:3000` | Acme Auto Sales | 1 | Acme Auto Sales | Default - always |
| `?dealership=1` | Acme Auto Sales | 1 | Acme Auto Sales | Explicit |
| `?dealership=2` | Premier Motors | 2 | Premier Motors | Explicit |
| `?dealership=999` | Acme Auto Sales | 1 | Acme Auto Sales | Invalid ID, falls back to default |

---

## 🔧 Technical Details

### How It Works

1. **URL Parameter Detection:**
   - Reads `?dealership=X` from URL on page load
   - Validates it's a positive integer
   - Sets as current dealership if valid

2. **Default Behavior:**
   - **No URL parameter** = Always defaults to dealership ID 1 (Acme Auto Sales)
   - This ensures a clean, predictable starting point
   - LocalStorage is NOT used for default selection

3. **LocalStorage Sync:**
   - URL parameter selection is saved to localStorage
   - LocalStorage does NOT override the default behavior
   - Used for persistence when switching between dealerships with parameters

4. **React Router Integration:**
   - Listens for `popstate` events (browser back/forward)
   - Updates dealership when URL changes
   - Triggers re-render of all components

### Code Changes

**File:** `frontend/src/context/DealershipContext.jsx`

**Selection Logic:**
```javascript
// Priority 1: Check URL parameter
const urlParams = new URLSearchParams(window.location.search);
const urlDealershipId = urlParams.get('dealership');
if (urlDealershipId) {
  const parsed = parseInt(urlDealershipId, 10);
  if (!isNaN(parsed) && parsed > 0) {
    return parsed;  // Use URL parameter
  }
}

// Priority 2: Default to ID 1 (Acme Auto Sales)
return 1;  // Always default to first dealership
```

**What Changed:**
- Removed localStorage check from default selection
- Base URL (`localhost:3000`) always shows Acme Auto Sales (ID 1)
- URL parameters still work for explicit dealership selection

---

## 💡 Advanced Testing

### Test Default Behavior

1. Visit `http://localhost:3000?dealership=2` (Premier Motors)
2. Click browser address bar
3. Remove `?dealership=2` leaving just `http://localhost:3000`
4. Press Enter
5. **Expected:** Switches back to Acme Auto Sales (ID 1)

### Test URL Parameter Switching

1. Visit `http://localhost:3000?dealership=1` (Acme)
2. Change URL to `?dealership=2` (Premier)
3. Press Enter
4. **Expected:** Switches to Premier Motors
5. Change URL back to `?dealership=1`
6. Press Enter
7. **Expected:** Switches back to Acme Auto Sales

### Test Browser Navigation

1. Visit `http://localhost:3000` (Acme - default)
2. Navigate to `?dealership=2` (Premier)
3. Click browser back button
4. **Expected:** Returns to Acme Auto Sales

### Test Refresh Behavior

1. Visit `http://localhost:3000?dealership=2` (Premier)
2. Press F5 to refresh
3. **Expected:** Still shows Premier Motors (URL param persists)
4. Visit `http://localhost:3000` (no param)
5. Press F5 to refresh
6. **Expected:** Still shows Acme Auto Sales (default)

---

## 📝 Manual Test Script

Copy this into browser console to test:

```javascript
// Test script
console.log('🧪 Testing URL Parameter Routing\n');

// Test 1: Check URL parameter
const params = new URLSearchParams(window.location.search);
const dealershipParam = params.get('dealership');
console.log('1. URL Parameter:', dealershipParam || 'None (should default to ID 1)');

// Test 2: Check localStorage
const stored = localStorage.getItem('selectedDealershipId');
console.log('2. LocalStorage:', stored || 'None');
console.log('   Note: LocalStorage does NOT affect default behavior');

// Test 3: Check expected dealership
if (!dealershipParam) {
  console.log('3. Expected: Acme Auto Sales (ID 1) - Default behavior');
} else if (dealershipParam === '1') {
  console.log('3. Expected: Acme Auto Sales (ID 1) - Explicit URL param');
} else if (dealershipParam === '2') {
  console.log('3. Expected: Premier Motors (ID 2) - Explicit URL param');
}

// Test 4: Check page content
setTimeout(() => {
  const text = document.body.innerText;
  if (text.includes('Acme Auto Sales')) {
    console.log('4. Current Page: ✅ Acme Auto Sales');
  } else if (text.includes('Premier Motors')) {
    console.log('4. Current Page: ✅ Premier Motors');
  } else {
    console.log('4. Current Page: ⚠️ Unknown');
  }
}, 1000);

console.log('\n📋 Quick Tests:');
console.log('Visit: http://localhost:3000 (Should show Acme - DEFAULT)');
console.log('Visit: http://localhost:3000?dealership=1 (Should show Acme)');
console.log('Visit: http://localhost:3000?dealership=2 (Should show Premier)');
```

---

## ✅ Success Criteria

The feature works correctly if:

1. `localhost:3000` (no param) **always** loads Acme Auto Sales (ID 1)
2. `?dealership=1` loads Acme Auto Sales
3. `?dealership=2` loads Premier Motors
4. Debug panel shows correct dealership info
5. LocalStorage syncs when URL parameters are used
6. Browser navigation (back/forward) works correctly
7. Invalid IDs fall back to default (ID 1)
8. Refreshing without URL param returns to Acme Auto Sales

---

## 🚀 Next Steps

After confirming URL parameters work:

1. **Remove debug panel** when ready for production
   - Edit `frontend/src/components/Layout.jsx`
   - Remove `<DealershipDebugInfo />` line

2. **Consider domain-based routing** for production
   - See `DOMAIN_ROUTING_GUIDE.md` for implementation steps
   - Allows `acme-auto.com` to automatically load dealership 1
   - Allows `premier-motors.com` to automatically load dealership 2

3. **Update navigation links** if needed
   - Add `?dealership=X` to internal links for explicit routing
   - Or rely on default behavior for base URL

---

## 🎯 Design Decision: Why Default to ID 1?

**Rationale:**
- **Predictable behavior** - Base URL always shows the same dealership
- **Clean starting point** - New visitors always see Acme Auto Sales first
- **Foundation for multi-domain** - When you implement domain routing, each domain will have its own URL (no parameters needed)
- **Explicit control** - Want a specific dealership? Use the URL parameter

**Alternative approaches considered:**
- ❌ Use localStorage: Unpredictable - depends on browser history
- ❌ Random selection: Confusing for users
- ✅ **Default to ID 1**: Clean, predictable, professional

---

**Last Updated:** 2026-01-14  
**Version:** 2.0 (Updated for default behavior change)

---

**End of Test Guide**
