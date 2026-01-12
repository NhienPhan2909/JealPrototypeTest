# Google Reviews Carousel - Visual Guide

## Feature Overview
A visually appealing carousel that displays customer reviews from Google Maps directly on your dealership homepage.

## Location on Page

```
┌─────────────────────────────────────────────────────────────┐
│                     DEALERSHIP HOMEPAGE                      │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│                        HERO SECTION                          │
│                  (Dealership Name & CTA)                     │
│                                                              │
├──────────────────────────┬──────────────────────────────────┤
│                          │                                   │
│  FIND YOUR PERFECT       │     GENERAL ENQUIRY               │
│  VEHICLE                 │     FORM                          │
│  (Search Widget)         │                                   │
│                          │                                   │
├──────────────────────────┴──────────────────────────────────┤
│                                                              │
│              ⭐ CUSTOMER REVIEWS CAROUSEL ⭐                  │
│              (NEW - GOOGLE REVIEWS)                          │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Carousel Layout

### Desktop View (3 reviews side-by-side)

```
┌────────────────────────────────────────────────────────────────────┐
│  Customer Reviews                                      [Google Logo] │
├────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ◀  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ▶       │
│     │ 🙂 John Doe  │  │ 🙂 Jane Smith │  │ 🙂 Bob Wilson │         │
│     │ 2 weeks ago  │  │ 1 month ago   │  │ 3 weeks ago  │         │
│     │              │  │               │  │              │         │
│     │ ★★★★★        │  │ ★★★★★         │  │ ★★★★☆        │         │
│     │              │  │               │  │              │         │
│     │ "Great       │  │ "Excellent    │  │ "Very good   │         │
│     │ service and  │  │ experience!   │  │ dealership,  │         │
│     │ excellent    │  │ The staff     │  │ would        │         │
│     │ vehicles!"   │  │ was amazing"  │  │ recommend"   │         │
│     └──────────────┘  └──────────────┘  └──────────────┘         │
│                                                                     │
│                          ● ○ ○                                      │
│                                                                     │
│                    [ Read More Reviews ]                            │
│                                                                     │
└────────────────────────────────────────────────────────────────────┘
```

### Mobile View (Stacked)

```
┌─────────────────────────┐
│ Customer Reviews  [G]   │
├─────────────────────────┤
│                         │
│ ◀  ┌──────────────┐  ▶  │
│    │ 🙂 John Doe  │     │
│    │ 2 weeks ago  │     │
│    │              │     │
│    │ ★★★★★        │     │
│    │              │     │
│    │ "Great       │     │
│    │ service and  │     │
│    │ excellent    │     │
│    │ vehicles!"   │     │
│    └──────────────┘     │
│                         │
│         ● ○ ○           │
│                         │
│  [ Read More Reviews ]  │
│                         │
└─────────────────────────┘
```

## Component Breakdown

### 1. Header Section
```
┌────────────────────────────────────────┐
│ Customer Reviews    [Google Logo]      │
└────────────────────────────────────────┘
```
- **Left**: Bold "Customer Reviews" heading
- **Right**: Official Google logo (branding)

### 2. Individual Review Card
```
┌──────────────────────┐
│ 🙂 John Doe          │  ← Profile photo + name
│ 2 weeks ago          │  ← Time posted
│                      │
│ ★★★★★                │  ← Star rating
│                      │
│ "Great service and   │  ← Review text
│  excellent vehicles! │     (truncated to 4 lines)
│  Highly recommend    │
│  this dealership."   │
└──────────────────────┘
```

### 3. Navigation Elements

**Arrow Buttons:**
```
◀  [Reviews]  ▶
```
- Circular buttons with shadow
- Gray background, hover effect
- Only visible when more than 3 reviews

**Pagination Dots:**
```
● ○ ○
```
- Filled dot = current page
- Empty dots = other pages
- Click to jump to page

### 4. Call-to-Action Button
```
┌───────────────────────┐
│  Read More Reviews    │  ← Links to Google Maps
└───────────────────────┘
```
- Uses dealership theme color
- Opens Google Reviews in new tab
- Full review page on Google Maps

## Visual Design Elements

### Colors
- **Background**: White (#FFFFFF)
- **Border**: Light gray (#E5E7EB)
- **Text**: Dark gray (#1F2937)
- **Stars (filled)**: Yellow (#FBBF24)
- **Stars (empty)**: Light gray (#D1D5DB)
- **Button**: Dealership theme color
- **Button Text**: Secondary theme color

### Typography
- **Heading**: 2xl, Bold (Customer Reviews)
- **Reviewer Name**: Semibold
- **Time**: Extra small, Gray
- **Review Text**: Small, Regular
- **Star Icons**: XL (★)

### Spacing
- **Card Padding**: 1rem (16px)
- **Gap Between Cards**: 1rem (16px)
- **Section Padding**: 1.5rem (24px)
- **Margins**: Responsive (varies by screen)

### Shadows & Borders
- **Card**: Light border + subtle shadow
- **Arrow Buttons**: Medium shadow
- **Overall Container**: Medium shadow

## Star Rating Display

### Visual Representation
```
5 stars: ★★★★★
4.5 stars: ★★★★⯨
4 stars: ★★★★☆
3 stars: ★★★☆☆
```

### Color Coding
- **Filled stars**: Bright yellow (#FBBF24)
- **Empty stars**: Light gray (#D1D5DB)
- **Half star**: Yellow (when applicable)

## Interactive Elements

### 1. Arrow Navigation
```
[◀]  ←  Click to go to previous page
[▶]  →  Click to go to next page
```
- Smooth transition between pages
- Circular navigation (last → first)
- Hover effect: Light gray background

### 2. Pagination Dots
```
● ○ ○  ←  Click any dot to jump to that page
```
- Smooth animation on transition
- Active page shown with filled dot
- Clickable for direct navigation

### 3. Read More Button
```
[ Read More Reviews ]  ←  Click to open Google Maps
```
- Opens in new tab (target="_blank")
- Full Google Business profile
- All reviews visible

## Responsive Breakpoints

### Mobile (< 768px)
- **1 review per view**
- Stacked layout
- Full-width cards
- Arrows closer to cards

### Tablet (768px - 1024px)
- **2-3 reviews per view**
- Grid layout adjusts
- Comfortable spacing

### Desktop (> 1024px)
- **3 reviews per view**
- Side-by-side layout
- Optimal spacing
- Arrows positioned outside

## Example Review Card Details

### Review Card Structure
```
┌─────────────────────────────────────┐
│  ┌──┐                               │
│  │ 🙂│  John Doe                    │  ← Profile section
│  └──┘  2 weeks ago                  │
│                                     │
│  ⭐⭐⭐⭐⭐                              │  ← Rating
│                                     │
│  "I had an excellent experience at  │
│  this dealership. The staff was     │  ← Review text
│  professional, friendly, and        │     (max 4 lines)
│  helped me find the perfect car!"   │
│                                     │
└─────────────────────────────────────┘
```

### Content Truncation
- **Text overflow**: Truncated with ellipsis (...)
- **Max lines**: 4 lines of text
- **Full text**: Available on Google Maps (Read More)

## Loading & Error States

### Loading State
```
┌────────────────────────────┐
│  Customer Reviews          │
├────────────────────────────┤
│                            │
│    Loading reviews...      │  ← Centered message
│                            │
└────────────────────────────┘
```

### No Reviews / Error State
- **Behavior**: Component doesn't render
- **Result**: Clean homepage without broken section
- **User Experience**: Seamless (no errors shown)

## Integration with Theme

### Theme Color Variables
```css
--theme-color: #3B82F6          /* Primary color */
--secondary-theme-color: #FFFFFF /* Text on primary */
```

### Applied To:
- ✅ "Read More Reviews" button background
- ✅ Button text color
- ✅ Maintains brand consistency

### Other Elements Use:
- Standard grays and yellows
- Google brand colors (logo)
- Neutral design adapts to any theme

## Accessibility Features

- ✅ **ARIA labels** on navigation buttons
- ✅ **Semantic HTML** structure
- ✅ **Keyboard navigation** support
- ✅ **Screen reader friendly** content
- ✅ **Color contrast** meets WCAG standards
- ✅ **Focus indicators** on interactive elements

## Animation & Transitions

### Page Transitions
- Smooth carousel slide
- 300ms transition duration
- Easing: ease-in-out

### Button Hovers
- Background color change
- Opacity adjustment
- 150ms transition

### Dot Indicators
- Width expansion on active
- Smooth color transition
- 200ms duration

## Google Branding Compliance

### Required Elements
- ✅ Google logo displayed
- ✅ Attribution to Google
- ✅ Link to Google Maps
- ✅ No modification of review content

### Logo Usage
```
┌────────────────────┐
│  [Google Logo]     │  ← Official PNG from Google CDN
└────────────────────┘
```
- Source: Google's official CDN
- Size: 30px height
- Position: Top right of carousel

## Summary

The Google Reviews carousel is a **clean**, **professional**, and **user-friendly** addition to the homepage that:

- ✅ Builds customer trust with real reviews
- ✅ Integrates seamlessly with existing design
- ✅ Maintains theme consistency
- ✅ Works perfectly on all devices
- ✅ Provides easy navigation
- ✅ Links directly to Google for full reviews
- ✅ Fails gracefully if reviews unavailable

**Result**: A polished, conversion-optimizing feature that enhances the dealership website experience.
