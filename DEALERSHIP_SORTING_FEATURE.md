# Dealership Sorting Feature

## Overview
The Dealership Management page now supports **sorting by ID, Name, or Date Created** with ascending/descending order toggle.

## Features Added

### 1. Sortable Columns
Users can click on column headers to sort:
- **ID** - Numeric sorting
- **Name** - Alphabetical sorting (case-insensitive)
- **Created** - Date/time sorting

### 2. Visual Indicators
- Clickable column headers have hover effect (gray background)
- Active sort column shows arrow indicator:
  - `↑` for ascending order
  - `↓` for descending order

### 3. Sort Behavior
- **First click**: Sort by that column in ascending order
- **Second click**: Reverse to descending order
- **Click different column**: Sort by new column in ascending order

## Implementation Details

### Frontend Changes (`frontend/src/pages/admin/DealershipManagement.jsx`)

#### New State Variables
```javascript
const [sortBy, setSortBy] = useState('id');     // Current sort field
const [sortOrder, setSortOrder] = useState('asc'); // 'asc' or 'desc'
```

#### Sort Handler Function
```javascript
const handleSort = (field) => {
  if (sortBy === field) {
    // Toggle order if clicking same column
    setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
  } else {
    // New column - default to ascending
    setSortBy(field);
    setSortOrder('asc');
  }
};
```

#### Sorting Logic
```javascript
const sortedDealerships = [...dealerships].sort((a, b) => {
  let aValue = a[sortBy];
  let bValue = b[sortBy];

  // Convert values based on type
  if (sortBy === 'created_at') {
    aValue = new Date(aValue).getTime();
    bValue = new Date(bValue).getTime();
  } else if (sortBy === 'id') {
    aValue = Number(aValue);
    bValue = Number(bValue);
  } else {
    aValue = String(aValue || '').toLowerCase();
    bValue = String(bValue || '').toLowerCase();
  }

  // Apply sort order
  if (sortOrder === 'asc') {
    return aValue > bValue ? 1 : aValue < bValue ? -1 : 0;
  } else {
    return aValue < bValue ? 1 : aValue > bValue ? -1 : 0;
  }
});
```

#### Updated Table Headers
```jsx
<th 
  className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider cursor-pointer hover:bg-gray-100"
  onClick={() => handleSort('id')}
>
  <div className="flex items-center">
    ID
    {sortBy === 'id' && (
      <span className="ml-1">{sortOrder === 'asc' ? '↑' : '↓'}</span>
    )}
  </div>
</th>
```

## Usage

### How to Sort Dealerships

1. Navigate to **Dealership Management** page (Admin account required)
2. Click any sortable column header:
   - **ID** - Sort by dealership ID number
   - **Name** - Sort alphabetically by dealership name
   - **Created** - Sort by creation date
3. Click again to reverse the sort order
4. Look for the arrow (↑/↓) to see current sort direction

### Default Behavior
- By default, dealerships are sorted by **ID in ascending order** (1, 2, 3, 4...)
- This matches the original database query order

## Examples

### Sort by Name (Alphabetical)
1. Click "Name" column header
2. Dealerships appear A-Z
3. Click again for Z-A

### Sort by Date Created (Newest First)
1. Click "Created" column header twice
2. Most recent dealerships appear at top

### Sort by ID (Find Gaps)
1. Click "ID" column header
2. See ID sequence: 1, 2, 4 (missing 3)
3. Helps identify deleted dealerships

## Technical Notes

### Client-Side Sorting
- Sorting happens in the browser (no API changes needed)
- Works with existing `GET /api/dealers` endpoint
- Good performance for reasonable number of dealerships

### Data Type Handling
- **Numeric** (ID): Converts to Number before comparing
- **String** (Name): Case-insensitive comparison
- **Date** (Created): Converts to timestamp for comparison

### Edge Cases Handled
- `null` or `undefined` values treated as empty strings
- Maintains stable sort (equal values keep original order)

## Testing

### Manual Test Steps
1. Log in as Admin account
2. Go to Dealership Management
3. Verify default sort (ID ascending)
4. Click "Name" - verify alphabetical order
5. Click "Name" again - verify reversed
6. Click "Created" - verify date order
7. Click "ID" - verify numeric order

### Expected Results
- ✅ Clicking headers changes sort
- ✅ Arrow indicators show correctly
- ✅ Hover effect on clickable headers
- ✅ All dealerships remain visible
- ✅ No errors in browser console

## Browser Compatibility
- Works in all modern browsers (Chrome, Firefox, Safari, Edge)
- Uses standard JavaScript Array.sort()
- No external dependencies required

## Future Enhancements (Optional)
- Add sort to other admin tables (Vehicles, Leads, etc.)
- Save sort preference in localStorage
- Add multi-column sorting
- Add filter/search functionality
