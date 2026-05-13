Admin Dashboard CSS - Style Guide & Examples

HEADER STYLING
==============

.dashboard-header
- Background: Linear gradient from #1a237e to #3949ab
- Padding: 24px 28px
- Border radius: 12px
- Box shadow: 0 4px 14px rgba(26, 35, 126, 0.15)
- Display: Flex with space-between alignment
- Margin bottom: 28px

Example HTML:
<header class="dashboard-header">
  <div>
    <h1>Admin Dashboard</h1>
    <div class="admin-welcome">Welcome, {{ adminName || 'Admin' }}</div>
  </div>
  <button class="btn btn-secondary" (click)="logout()">Logout</button>
</header>

Result: Professional header with gradient background, title, welcome message, and logout button

NAVIGATION TABS
===============

.dashboard-tabs
- Display: Flex with gap 8px
- Border bottom: 2px solid #e0e0e0
- Margin bottom: 24px

.dashboard-tabs button
- Padding: 12px 20px
- Color: #757575 (inactive), #1a237e (active)
- Border bottom: 3px solid transparent (inactive), #3949ab (active)
- Cursor: pointer
- Transition: all 0.2s ease

Example HTML:
<nav class="dashboard-tabs">
  <button [class.active]="activeTab === 'teachers'">Teachers</button>
  <button [class.active]="activeTab === 'students'">Students</button>
  <button [class.active]="activeTab === 'audit'">Audit Logs</button>
</nav>

Result: Three tabs with underline indicator for active tab

ALERT MESSAGES
==============

.alert
- Padding: 14px 18px
- Border radius: 8px
- Margin bottom: 20px
- Font size: 14px
- Border left: 4px solid

.alert-error
- Background: #fff5f5 (light red)
- Border color: #d32f2f (red)
- Color: #b71c1c (dark red)

.alert-success
- Background: #f1f8e9 (light green)
- Border color: #388e3c (green)
- Color: #1b5e20 (dark green)

Example HTML:
<div *ngIf="error" class="alert alert-error">{{ error }}</div>
<div *ngIf="successMsg" class="alert alert-success">{{ successMsg }}</div>

Result: Color-coded alerts with proper contrast and visibility

FORM STYLING
=============

.form-card
- Background: #fff (white)
- Border radius: 10px
- Padding: 24px
- Box shadow: 0 2px 10px rgba(0, 0, 0, 0.08)
- Margin bottom: 20px

.field-row
- Display: Flex flex-direction column
- Gap: 6px

.field-row label
- Font size: 13px
- Font weight: 600
- Color: #424242
- Text transform: uppercase
- Letter spacing: 0.5px

.field-row input, select, textarea
- Padding: 10px 12px
- Border: 1px solid #ddd
- Border radius: 6px
- Font size: 14px
- Transition: border-color 0.2s, box-shadow 0.2s

.field-row input:focus, select:focus, textarea:focus
- Outline: none
- Border color: #3949ab
- Box shadow: 0 0 0 3px rgba(57, 73, 171, 0.1)

Example HTML:
<div class="form-card">
  <h2>Create Teacher</h2>
  <form (ngSubmit)="createTeacher()">
    <div class="field-row">
      <label for="teacher-id">ID/Passport No.</label>
      <input id="teacher-id" [(ngModel)]="newTeacher.idPassportNo" />
    </div>
    <div class="form-actions">
      <button class="btn btn-primary">Save Teacher</button>
      <button class="btn btn-secondary">Cancel</button>
    </div>
  </form>
</div>

Result: Clean form with labeled inputs, clear focus states, and action buttons

TABLE STYLING
=============

.table-card
- Background: #fff
- Border radius: 10px
- Padding: 24px
- Box shadow: 0 2px 10px rgba(0, 0, 0, 0.08)

.table-card table
- Width: 100%
- Border collapse: collapse
- Font size: 14px

.table-card th
- Padding: 12px 14px
- Background: #f5f5f5
- Font weight: 700
- Color: #424242
- Border bottom: 2px solid #e0e0e0
- Text transform: uppercase
- Font size: 12px
- Letter spacing: 0.5px

.table-card td
- Padding: 14px
- Color: #424242
- Border bottom: 1px solid #f0f0f0

.table-card tbody tr:hover
- Background: #fafafa
- Transition: background-color 0.15s

.actions-cell
- Display: Flex
- Gap: 8px
- Flex wrap: wrap

Example HTML:
<div class="table-card">
  <h2>Teachers</h2>
  <table>
    <thead>
      <tr>
        <th>ID</th>
        <th>Name</th>
        <th>Email</th>
        <th>Actions</th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let teacher of teachers">
        <td>{{ teacher.idPassportNo }}</td>
        <td>{{ teacher.firstName }} {{ teacher.lastName }}</td>
        <td>{{ teacher.email }}</td>
        <td class="actions-cell">
          <button class="btn btn-secondary">Edit</button>
          <button class="btn btn-danger">Delete</button>
        </td>
      </tr>
    </tbody>
  </table>
</div>

Result: Professional data table with hover effects and action buttons

BUTTON STYLING
==============

.btn (Base)
- Display: Inline flex
- Padding: 10px 18px
- Border radius: 6px
- Border: none
- Cursor: pointer
- Font size: 13px
- Font weight: 600
- Transition: all 0.2s ease
- White space: nowrap

.btn-primary
- Background: #3949ab
- Color: #fff
- Box shadow: 0 2px 6px rgba(57, 73, 171, 0.3)

.btn-primary:hover:not(:disabled)
- Background: #1a237e (darker)
- Box shadow: 0 4px 12px rgba(57, 73, 171, 0.4)
- Transform: translateY(-1px) (lifts button)

.btn-secondary
- Background: #757575
- Color: #fff
- Box shadow: 0 2px 6px rgba(117, 117, 117, 0.3)

.btn-danger
- Background: #d32f2f
- Color: #fff
- Box shadow: 0 2px 6px rgba(211, 47, 47, 0.3)

.btn:disabled
- Opacity: 0.6
- Cursor: not-allowed

Example HTML:
<button class="btn btn-primary">Create</button>
<button class="btn btn-secondary">Cancel</button>
<button class="btn btn-danger">Delete</button>

Result: Three button types with hover effects and disabled states

RESPONSIVE DESIGN
=================

Desktop (1200px+)
- Max width: 1200px
- Full spacing and padding
- Multi-column layouts
- All elements visible

Tablet (768px - 1199px)
- Adjusted padding: 16px 12px
- Stacked button layouts (.panel-actions flex-direction: column)
- Full-width buttons
- Smaller font sizes
- Grid to single column (.filter-row)

Mobile (480px - 767px)
- Minimal padding: 12px 8px
- All buttons full-width with centering
- Reduced font sizes (11px-12px)
- Table font size: 11px
- Smaller table cell padding

Small Mobile (<480px)
- Extra minimal padding
- Maximum readability focus
- Simplified layouts

Example:
@media (max-width: 768px) {
  .dashboard-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 16px;
    padding: 16px 20px;
  }

  .panel-actions {
    flex-direction: column;
  }

  .panel-actions .btn {
    width: 100%;
    justify-content: center;
  }
}

ANIMATIONS
==========

.fadeIn - Panel Appearance
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

Applied to: .panel
Duration: 0.3s ease

.slideUp - Modal Entrance
@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

Applied to: .modal-box
Duration: 0.3s ease

.dots - Loading State
@keyframes dots {
  0%, 20% { content: ''; }
  40% { content: '.'; }
  60% { content: '..'; }
  80%, 100% { content: '...'; }
}

Applied to: .loading::after
Duration: 1.5s steps(4, end) infinite

SPECIAL COMPONENTS
==================

Teacher Badge
.teacher-badge
- Display: Inline flex
- Background: #e3f2fd (light blue)
- Border: 1px solid #90caf9 (blue)
- Color: #1565c0 (blue)
- Padding: 4px 10px
- Border radius: 12px
- Font size: 13px
- Gap: 6px between items

Example:
<div class="teacher-badge">
  Teacher Name
  <button class="link-button" (click)="unassignTeacher(...)">Unassign</button>
</div>

Preview Card
.preview-card
- Background: #f8f9fa (light gray)
- Border left: 4px solid #ff9800 (orange)
- Same padding as other cards

Search Row
.search-row
- Display: Flex
- Align items: flex-end
- Gap: 16px
- Background: #fff
- Box shadow: 0 2px 10px rgba(0, 0, 0, 0.08)

Filter Row
.filter-row
- Display: Grid
- Grid template columns: repeat(auto-fit, minmax(250px, 1fr))
- Gap: 16px
- Background: #fff
- Box shadow: 0 2px 10px rgba(0, 0, 0, 0.08)

COLOR REFERENCE GUIDE
====================

Primary Brand Colors
#1a237e - Dark Indigo (primary, headers, active states)
#3949ab - Medium Indigo (secondary buttons, accents)

Semantic Colors
#388e3c - Green (success)
#1b5e20 - Dark Green (success text)
#d32f2f - Red (danger, errors)
#b71c1c - Dark Red (danger text)
#1565c0 - Blue (info, links)
#e65100 - Orange (warning)

Neutral Colors
#fff / #ffffff - White (backgrounds)
#f5f5f5 - Light Gray (table headers)
#f0f0f0 - Very Light Gray (borders)
#ddd / #dddddd - Medium Gray (input borders)
#9e9e9e - Medium Gray (secondary text)
#757575 - Dark Gray (buttons, secondary text)
#424242 - Very Dark Gray (labels, main text)
#1a1a2e - Almost Black (primary text)

Background Colors
#fff5f5 - Error background (light red)
#f1f8e9 - Success background (light green)
#e3f2fd - Info background (light blue)
#f8f9fa - Preview/secondary background (light gray)

Box Shadow Reference
Light: 0 2px 10px rgba(0, 0, 0, 0.08)
Medium: 0 2px 6px rgba(57, 73, 171, 0.3)
Larger: 0 4px 12px rgba(57, 73, 171, 0.4)
Large Modal: 0 8px 32px rgba(0, 0, 0, 0.2)

SPACING SCALE
=============
4px
6px
8px
10px
12px
14px
16px (common gap)
18px
20px
24px (common padding)
28px

FONT SIZES
==========
11px - Small text (table footer, hints)
12px - Extra small UI text (labels, badges)
13px - Small UI text (labels, buttons, inputs)
14px - Body text, table cells
18px - Section titles
22px - Dashboard h1
28px - Main heading

FONT WEIGHTS
============
500 - Welcome messages, secondary text
600 - Labels, button text, section titles
700 - Headings, important text

This style guide provides a comprehensive overview of all CSS styling in the admin dashboard,
with examples for implementing common patterns and maintaining consistency across the interface.
